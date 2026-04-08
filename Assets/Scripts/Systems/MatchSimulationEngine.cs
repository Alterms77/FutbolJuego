using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FutbolJuego.Models;
using FutbolJuego.Utils;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Core match simulation engine.  Uses Poisson-distributed goal sampling
    /// (λ derived from xG) and generates a minute-by-minute event timeline.
    /// </summary>
    public class MatchSimulationEngine
    {
        private readonly System.Random rng = new System.Random();

        // Pre-computed e^(-λ) lookup for λ values 0.0–4.5 in steps of 0.05
        // to avoid repeated Math.Exp calls during Monte Carlo simulations.
        private static readonly Dictionary<int, double> PoissonLCache = BuildPoissonLCache();
        private static Dictionary<int, double> BuildPoissonLCache()
        {
            var cache = new Dictionary<int, double>();
            for (int i = 0; i <= 90; i++) // 0.05 * i → 0.0 to 4.5
                cache[i] = Math.Exp(-0.05 * i);
            return cache;
        }
        private static double GetPoissonL(float lambda)
        {
            int key = Mathf.RoundToInt(lambda / 0.05f);
            if (PoissonLCache.TryGetValue(key, out double val)) return val;
            return Math.Exp(-lambda); // fallback for out-of-range values
        }


        /// <summary>
        /// Fully simulates a match between <paramref name="home"/> and
        /// <paramref name="away"/> and returns the completed <see cref="MatchData"/>.
        /// </summary>
        public MatchData SimulateMatch(TeamData home, TeamData away, bool isNeutralVenue = false)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));
            if (away == null) throw new ArgumentNullException(nameof(away));

            float homeXG = CalculateExpectedGoals(home, away, isHome: !isNeutralVenue);
            float awayXG = CalculateExpectedGoals(away, home, isHome: false);

            // Clamp xG to sensible range (very weak teams still generate ≥ 0.15)
            homeXG = Mathf.Clamp(homeXG, 0.15f, 4.5f);
            awayXG = Mathf.Clamp(awayXG, 0.15f, 4.5f);

            int homeGoals = SampleFromPoisson(homeXG);
            int awayGoals = SampleFromPoisson(awayXG);

            List<MatchEvent> events = GenerateMatchEvents(home, away, homeGoals, awayGoals, homeXG, awayXG);
            MatchStatistics stats   = CalculateMatchStatistics(home, away, homeXG, awayXG, events);

            return new MatchData
            {
                id           = Guid.NewGuid().ToString(),
                homeTeamId   = home.id,
                awayTeamId   = away.id,
                homeScore    = homeGoals,
                awayScore    = awayGoals,
                matchDate    = DateTime.UtcNow,
                status       = MatchStatus.Completed,
                statistics   = stats,
                events       = events,
                currentMinute = Constants.MatchDurationMinutes
            };
        }

        // ── xG calculation ─────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the expected-goals (xG) lambda for one team in this match.
        ///
        /// Base formula:
        ///   λ = baseGoals × squadQuality × tacticalEff × moraleMod × fatigueMod × homeAdv
        ///
        /// Base goals ≈ league average (1.35 per game per team).
        /// </summary>
        private float CalculateExpectedGoals(TeamData team, TeamData opponent, bool isHome)
        {
            const float baseGoals = 1.35f;

            float squadQuality   = CalculateSquadQuality(team);
            float oppDefense     = CalculateDefensiveStrength(opponent);
            float tacticalEff    = CalculateTacticalEffectiveness(
                                        team.currentTactic, opponent.currentTactic);
            float moraleMod      = GetMoraleModifier(team.morale);
            float fatigueMod     = GetFatigueModifier(team);
            float homeAdv        = isHome ? Constants.HomeAdvantage : 1.0f;

            // Relative quality ratio — higher squad quality vs opponent defense → more xG
            float qualityRatio   = squadQuality / Mathf.Max(oppDefense, 0.5f);

            float xg = baseGoals * qualityRatio * tacticalEff * moraleMod * fatigueMod * homeAdv;
            return xg;
        }

        /// <summary>
        /// Produces a normalised squad-quality score (reference 1.0 = average
        /// top-flight team with overall ~72).
        /// </summary>
        private float CalculateSquadQuality(TeamData team)
        {
            var starters = team.GetStartingEleven(team.currentTactic);
            if (starters == null || starters.Count == 0) return 0.7f;

            float avg = (float)starters.Average(p => p.CalculateOverall());
            // Normalize so overall 72 → 1.0, range roughly 0.5 – 1.5
            return avg / 72f;
        }

        /// <summary>
        /// Measures the defensive strength of a team (used as denominator in
        /// the quality ratio).
        /// </summary>
        private float CalculateDefensiveStrength(TeamData team)
        {
            var starters = team.GetStartingEleven(team.currentTactic);
            if (starters == null || starters.Count == 0) return 0.7f;

            // Weighted: defenders & GK contribute 70%, midfield 20%, attack 10%
            float defScore = 0f; int defCount = 0;
            float midScore = 0f; int midCount = 0;
            float attScore = 0f; int attCount = 0;

            foreach (var p in starters)
            {
                int ov = p.CalculateOverall();
                if (p.position.IsDefender() || p.position == PlayerPosition.GK)
                { defScore += ov; defCount++; }
                else if (p.position.IsMidfielder())
                { midScore += ov; midCount++; }
                else
                { attScore += ov; attCount++; }
            }

            float defAvg = defCount > 0 ? defScore / defCount : 60f;
            float midAvg = midCount > 0 ? midScore / midCount : 60f;
            float attAvg = attCount > 0 ? attScore / attCount : 60f;

            float composite = defAvg * 0.7f + midAvg * 0.2f + attAvg * 0.1f;
            return composite / 72f;
        }

        /// <summary>
        /// Returns a tactical multiplier (0.85-1.15) based on how well the
        /// attacking team's style matches up against the opponent's.
        /// </summary>
        private float CalculateTacticalEffectiveness(TacticData tactic, TacticData opponentTactic)
        {
            if (tactic == null || opponentTactic == null) return 1.0f;

            float score = 1.0f;

            // High Press is effective against slow-tempo teams
            if (tactic.playStyle == PlayStyle.HighPress && opponentTactic.tempo < 40)
                score += 0.08f;

            // Counter-attack rewards low opponent pressing
            if (tactic.playStyle == PlayStyle.CounterAttack && opponentTactic.pressing > 70)
                score += 0.07f;

            // Possession punished by high pressing
            if (tactic.playStyle == PlayStyle.Possession && opponentTactic.pressing > 75)
                score -= 0.06f;

            // Park the Bus severely limits own attack
            if (tactic.playStyle == PlayStyle.ParkTheBus)
                score -= 0.12f;

            // Pressing differential
            float pressRatio = tactic.pressing / Mathf.Max(opponentTactic.tempo, 1f);
            score += Mathf.Clamp((pressRatio - 1.0f) * 0.05f, -0.05f, 0.05f);

            return Mathf.Clamp(score, 0.85f, 1.15f);
        }

        // ── Poisson sampling ───────────────────────────────────────────────────

        /// <summary>
        /// Draws a random variate from the Poisson distribution with parameter
        /// <paramref name="lambda"/> using Knuth's algorithm.
        ///
        /// P(k) = (λ^k × e^(−λ)) / k!
        /// </summary>
        public int SampleFromPoisson(float lambda)
        {
            if (lambda <= 0f) return 0;

            // Knuth algorithm: generate uniform samples until their product drops below e^(-λ)
            double L = Math.Exp(-lambda);
            double p = 1.0;
            int k   = 0;

            do
            {
                k++;
                p *= rng.NextDouble();
            }
            while (p > L);

            return k - 1;
        }

        // ── Event generation ───────────────────────────────────────────────────

        /// <summary>
        /// Builds a plausible timeline of match events consistent with the
        /// final score and xG values.
        /// </summary>
        private List<MatchEvent> GenerateMatchEvents(
            TeamData home, TeamData away,
            int homeGoals, int awayGoals,
            float homeXG, float awayXG)
        {
            var events = new List<MatchEvent>();
            var usedMinutes = new HashSet<int>();

            // ── Goal events ────────────────────────────────────────────────────

            void AddGoals(TeamData team, int goalCount, float xg)
            {
                for (int i = 0; i < goalCount; i++)
                {
                    int minute = PickUniqueMinute(usedMinutes, 1, 90);
                    var scorer = SelectGoalScorer(team);
                    var assist = SelectAssistProvider(team, scorer);

                    events.Add(new MatchEvent
                    {
                        minute          = minute,
                        type            = MatchEventType.Goal,
                        teamId          = team.id,
                        playerId        = scorer?.id,
                        assistPlayerId  = assist?.id,
                        description     = $"{scorer?.name ?? "Unknown"} scores for {team.shortName}!"
                    });
                }
            }

            AddGoals(home, homeGoals, homeXG);
            AddGoals(away, awayGoals, awayXG);

            // ── Yellow cards ──────────────────────────────────────────────────

            int homeYellows = SampleFromPoisson(1.8f);
            int awayYellows = SampleFromPoisson(1.8f);
            AddCardEvents(events, home, homeYellows, MatchEventType.YellowCard, usedMinutes);
            AddCardEvents(events, away, awayYellows, MatchEventType.YellowCard, usedMinutes);

            // ── Red cards (rare) ──────────────────────────────────────────────

            if (rng.NextDouble() < 0.08)
                AddCardEvents(events, rng.NextDouble() < 0.5 ? home : away,
                              1, MatchEventType.RedCard, usedMinutes);

            // ── Substitutions ─────────────────────────────────────────────────

            AddSubstitutions(events, home, usedMinutes);
            AddSubstitutions(events, away, usedMinutes);

            // Sort by minute
            events.Sort((a, b) => a.minute.CompareTo(b.minute));
            return events;
        }

        private void AddCardEvents(List<MatchEvent> events, TeamData team,
                                   int count, MatchEventType type, HashSet<int> used)
        {
            var squad = team.GetAvailablePlayers();
            if (squad.Count == 0) return;

            for (int i = 0; i < count; i++)
            {
                int minute = PickUniqueMinute(used, 5, 90);
                var player = squad[rng.Next(squad.Count)];
                events.Add(new MatchEvent
                {
                    minute      = minute,
                    type        = type,
                    teamId      = team.id,
                    playerId    = player.id,
                    description = $"{player.name} receives a {type}."
                });
            }
        }

        private void AddSubstitutions(List<MatchEvent> events, TeamData team, HashSet<int> used)
        {
            int subCount = rng.Next(1, Constants.MaxSubstitutions + 1);
            var squad    = team.GetAvailablePlayers();
            if (squad.Count < 2) return;

            for (int i = 0; i < subCount; i++)
            {
                int minute    = PickUniqueMinute(used, 46, 85);
                var playerOut = squad[rng.Next(squad.Count)];
                var playerIn  = squad[rng.Next(squad.Count)];
                if (playerOut.id == playerIn.id) continue;

                events.Add(new MatchEvent
                {
                    minute      = minute,
                    type        = MatchEventType.Substitution,
                    teamId      = team.id,
                    playerId    = playerOut.id,
                    assistPlayerId = playerIn.id,
                    description = $"{playerOut.name} OFF — {playerIn.name} ON."
                });
            }
        }

        private int PickUniqueMinute(HashSet<int> used, int min, int max)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int m = rng.Next(min, max + 1);
                if (used.Add(m)) return m;
            }
            // Fallback: allow duplicate minute
            return rng.Next(min, max + 1);
        }

        // ── Statistics ─────────────────────────────────────────────────────────

        /// <summary>
        /// Derives aggregate match statistics from xG, squad quality, and the
        /// generated event list.
        /// </summary>
        private MatchStatistics CalculateMatchStatistics(
            TeamData home, TeamData away,
            float homeXG, float awayXG,
            List<MatchEvent> events)
        {
            float homeQuality = CalculateSquadQuality(home);
            float awayQuality = CalculateSquadQuality(away);
            float totalQuality = homeQuality + awayQuality;

            float homePoss = homeQuality / totalQuality * 100f;

            // Shots: roughly xG / average shot conversion rate (≈11%)
            int homeShots = Mathf.Max(1, Mathf.RoundToInt(homeXG / 0.11f));
            int awayShots = Mathf.Max(1, Mathf.RoundToInt(awayXG / 0.11f));
            int homeSoT   = Mathf.Max(1, Mathf.RoundToInt(homeShots * 0.40f));
            int awaySoT   = Mathf.Max(1, Mathf.RoundToInt(awayShots * 0.40f));

            // Passes proportional to possession
            int homePasses = Mathf.RoundToInt(450f * (homePoss / 100f));
            int awayPasses = Mathf.RoundToInt(450f * (1f - homePoss / 100f));

            int homeYellows = events.Count(e => e.teamId == home.id && e.type == MatchEventType.YellowCard);
            int awayYellows = events.Count(e => e.teamId == away.id && e.type == MatchEventType.YellowCard);
            int homeReds    = events.Count(e => e.teamId == home.id && e.type == MatchEventType.RedCard);
            int awayReds    = events.Count(e => e.teamId == away.id && e.type == MatchEventType.RedCard);

            return new MatchStatistics
            {
                homeXG             = homeXG,
                awayXG             = awayXG,
                homeShots          = homeShots,
                awayShots          = awayShots,
                homeShotsOnTarget  = homeSoT,
                awayShotsOnTarget  = awaySoT,
                homePossession     = homePoss,
                awayPossession     = 100f - homePoss,
                homePasses         = homePasses,
                awayPasses         = awayPasses,
                homeFouls          = Mathf.RoundToInt(SampleFromPoisson(10f)),
                awayFouls          = Mathf.RoundToInt(SampleFromPoisson(10f)),
                homeYellowCards    = homeYellows,
                awayYellowCards    = awayYellows,
                homeRedCards       = homeReds,
                awayRedCards       = awayReds,
                homeCorners        = rng.Next(3, 12),
                awayCorners        = rng.Next(3, 12)
            };
        }

        // ── Player selection helpers ───────────────────────────────────────────

        /// <summary>
        /// Randomly selects the goal scorer from attackers and midfielders,
        /// weighted by shooting attribute.
        /// </summary>
        private PlayerData SelectGoalScorer(TeamData team)
        {
            var candidates = team.GetAvailablePlayers()
                .Where(p => !p.position.IsGoalkeeper())
                .ToList();

            if (candidates.Count == 0) return null;

            // Weight by shooting (minimum 1 to avoid zero-weight entries)
            float[] weights = candidates.Select(p =>
            {
                float baseWeight = p.position.IsForward()  ? 3.0f :
                                   p.position.IsMidfielder() ? 1.5f : 0.5f;
                return baseWeight * Mathf.Max(1f, p.attributes.shooting);
            }).ToArray();

            return WeightedRandom(candidates, weights);
        }

        /// <summary>
        /// Randomly selects an assist provider (must differ from the scorer),
        /// weighted by passing attribute.
        /// </summary>
        private PlayerData SelectAssistProvider(TeamData team, PlayerData scorer)
        {
            // 30% chance of no recorded assist
            if (rng.NextDouble() < 0.30) return null;

            var candidates = team.GetAvailablePlayers()
                .Where(p => !p.position.IsGoalkeeper() && p.id != scorer?.id)
                .ToList();

            if (candidates.Count == 0) return null;

            float[] weights = candidates.Select(p =>
                Mathf.Max(1f, p.attributes.passing)).ToArray();

            return WeightedRandom(candidates, weights);
        }

        private PlayerData WeightedRandom(List<PlayerData> pool, float[] weights)
        {
            float total = weights.Sum();
            float roll  = (float)(rng.NextDouble() * total);
            float cumulative = 0f;

            for (int i = 0; i < pool.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative) return pool[i];
            }
            return pool[pool.Count - 1];
        }

        // ── Morale / fatigue modifiers ─────────────────────────────────────────

        private static float GetMoraleModifier(int morale)
        {
            // Morale 0 → 0.85, 50 → 1.0, 100 → 1.15
            return 0.85f + (morale / 100f) * 0.30f;
        }

        private float GetFatigueModifier(TeamData team)
        {
            var starters = team.GetStartingEleven(team.currentTactic);
            if (starters == null || starters.Count == 0) return 1.0f;

            float avgFatigue = (float)starters.Average(p => p.fatigue);
            // Full rest (0) → 1.0, exhausted (100) → 0.88
            return 1.0f - (avgFatigue / 100f) * 0.12f;
        }
    }
}
