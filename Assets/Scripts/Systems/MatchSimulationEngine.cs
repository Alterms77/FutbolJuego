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
        ///
        /// Red cards are pre-rolled so their timing can reduce the penalised
        /// team's xG before goal sampling (a minute-5 red is far more costly
        /// than one in minute 75).  Fatigue accumulates for both squads.
        /// </summary>
        public MatchData SimulateMatch(TeamData home, TeamData away, bool isNeutralVenue = false)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));
            if (away == null) throw new ArgumentNullException(nameof(away));

            // ── Pre-roll red cards ─────────────────────────────────────────────
            // 8 % base chance per team; stored as the game-minute they occur
            // (-1 = no red card this match).
            // rng.Next(5, 76) generates values in [5, 75] (76 is exclusive),
            // matching the guard in AddPrerolledRedCard which rejects minutes >= 76.
            const double redCardProb  = 0.08;
            int homeRedMinute = rng.NextDouble() < redCardProb ? rng.Next(5, 76) : -1;
            int awayRedMinute = rng.NextDouble() < redCardProb ? rng.Next(5, 76) : -1;

            // ── xG calculation ─────────────────────────────────────────────────
            float homeXG = CalculateExpectedGoals(home, away, isHome: !isNeutralVenue);
            float awayXG = CalculateExpectedGoals(away, home, isHome: false);

            // Apply red-card xG penalty: up to -35 % for an early dismissal
            homeXG = ApplyRedCardPenalty(homeXG, homeRedMinute);
            awayXG = ApplyRedCardPenalty(awayXG, awayRedMinute);

            // Clamp xG to sensible range
            homeXG = Mathf.Clamp(homeXG, 0.15f, 4.5f);
            awayXG = Mathf.Clamp(awayXG, 0.15f, 4.5f);

            int homeGoals = SampleFromPoisson(homeXG);
            int awayGoals = SampleFromPoisson(awayXG);

            List<MatchEvent> events = GenerateMatchEvents(
                home, away, homeGoals, awayGoals, homeXG, awayXG,
                homeRedMinute, awayRedMinute);

            MatchStatistics stats = CalculateMatchStatistics(home, away, homeXG, awayXG, events);

            // Post-match fatigue accumulation for both squads
            AccumulateMatchFatigue(home);
            AccumulateMatchFatigue(away);

            return new MatchData
            {
                id            = Guid.NewGuid().ToString(),
                homeTeamId    = home.id,
                awayTeamId    = away.id,
                homeScore     = homeGoals,
                awayScore     = awayGoals,
                matchDate     = DateTime.UtcNow,
                status        = MatchStatus.Completed,
                statistics    = stats,
                events        = events,
                currentMinute = Constants.MatchDurationMinutes
            };
        }

        // ── Red card helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Reduces <paramref name="xg"/> by up to 35 % based on how early the
        /// red card occurs.  A minute-5 red leaves 85/90 ≈ 94 % of the match
        /// to play at a disadvantage; a minute-75 red leaves only 15/90 ≈ 17 %.
        /// Formula: xg × (1 − remainingFraction × 0.35)
        /// </summary>
        private static float ApplyRedCardPenalty(float xg, int redCardMinute)
        {
            if (redCardMinute < 0) return xg;
            float matchLen          = Constants.MatchDurationMinutes;
            float remainingFraction = Mathf.Clamp01((matchLen - redCardMinute) / matchLen);
            return xg * (1f - remainingFraction * 0.35f);
        }

        // ── Post-match fatigue ─────────────────────────────────────────────────

        /// <summary>
        /// Accumulates match fatigue for <paramref name="team"/>'s players.
        /// Starters gain 15-25 fatigue; non-selected players recover 3-7.
        /// </summary>
        private void AccumulateMatchFatigue(TeamData team)
        {
            if (team?.squad == null) return;

            var starters   = team.GetStartingEleven(team.currentTactic);
            var starterIds = new HashSet<string>(starters.Select(p => p.id));

            foreach (var player in team.squad)
            {
                if (starterIds.Contains(player.id))
                    player.fatigue = Mathf.Min(100, player.fatigue + rng.Next(15, 26));
                else
                    player.fatigue = Mathf.Max(0, player.fatigue - rng.Next(3, 8));
            }
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
            float homeXG, float awayXG,
            int homeRedCardMinute, int awayRedCardMinute)
        {
            var events = new List<MatchEvent>();
            var usedMinutes = new HashSet<int>();

            // ── Goal events ────────────────────────────────────────────────────

            void AddGoals(TeamData team, int goalCount)
            {
                for (int i = 0; i < goalCount; i++)
                {
                    int minute = PickUniqueMinute(usedMinutes, 1, 90);
                    var scorer = SelectGoalScorer(team);
                    var assist = SelectAssistProvider(team, scorer);

                    // ~18% of goals come from the penalty spot
                    bool isPenalty = rng.NextDouble() < 0.18;
                    string desc    = isPenalty
                        ? $"{scorer?.name ?? "Unknown"} converts the penalty for {team.shortName}!"
                        : $"{scorer?.name ?? "Unknown"} scores for {team.shortName}!";

                    events.Add(new MatchEvent
                    {
                        minute         = minute,
                        type           = MatchEventType.Goal,
                        teamId         = team.id,
                        playerId       = scorer?.id,
                        assistPlayerId = isPenalty ? null : assist?.id,
                        description    = desc
                    });
                }
            }

            AddGoals(home, homeGoals);
            AddGoals(away, awayGoals);

            // ── Missed penalty events (narrative, do not change score) ─────────

            void MaybeAddMissedPenalty(TeamData team)
            {
                if (rng.NextDouble() < 0.14f) // ~14% chance of a missed pen per team
                {
                    int minute = PickUniqueMinute(usedMinutes, 5, 88);
                    var taker  = SelectGoalScorer(team);
                    events.Add(new MatchEvent
                    {
                        minute      = minute,
                        type        = MatchEventType.MissedPenalty,
                        teamId      = team.id,
                        playerId    = taker?.id,
                        description = $"PENALTY MISSED! {taker?.name ?? "Unknown"} fails to convert for {team.shortName}."
                    });
                }
            }

            MaybeAddMissedPenalty(home);
            MaybeAddMissedPenalty(away);

            // ── Yellow cards ──────────────────────────────────────────────────

            int homeYellows = SampleFromPoisson(1.8f);
            int awayYellows = SampleFromPoisson(1.8f);
            AddCardEvents(events, home, homeYellows, MatchEventType.YellowCard, usedMinutes);
            AddCardEvents(events, away, awayYellows, MatchEventType.YellowCard, usedMinutes);

            // ── Red cards (pre-rolled in SimulateMatch) ───────────────────────
            // Align ceiling with generation range rng.Next(5, 76) — both use minute < 76.
            void AddPrerolledRedCard(TeamData team, int redMinute)
            {
                if (redMinute < 0 || redMinute >= 76) return;

                var squad = team.GetAvailablePlayers()
                    .Where(p => !p.position.IsGoalkeeper())
                    .ToList();
                if (squad.Count == 0) return;

                var carded = squad[rng.Next(squad.Count)];
                int minute = redMinute;
                usedMinutes.Add(minute);

                events.Add(new MatchEvent
                {
                    minute      = minute,
                    type        = MatchEventType.RedCard,
                    teamId      = team.id,
                    playerId    = carded.id,
                    description = $"🟥 {carded.name} is SENT OFF! {team.shortName} are down to 10 men!"
                });
            }

            AddPrerolledRedCard(home, homeRedCardMinute);
            AddPrerolledRedCard(away, awayRedCardMinute);

            // ── In-match injuries (max 1 per team) ────────────────────────────

            void TryAddInjury(TeamData team)
            {
                var candidates = team.GetAvailablePlayers()
                    .Where(p => !p.position.IsGoalkeeper())
                    .ToList();

                foreach (var player in candidates)
                {
                    float prob = 0.025f
                               + player.injuryProneness / 100f * 0.03f
                               + player.fatigue         / 100f * 0.02f;

                    if (rng.NextDouble() < prob)
                    {
                        int minute                  = PickUniqueMinute(usedMinutes, 15, 85);
                        player.isAvailable         = false;
                        player.injuryDaysRemaining = rng.Next(7, 28);

                        events.Add(new MatchEvent
                        {
                            minute      = minute,
                            type        = MatchEventType.Injury,
                            teamId      = team.id,
                            playerId    = player.id,
                            description = $"{player.name} is injured and leaves the pitch!"
                        });
                        break; // max 1 injury per team per match
                    }
                }
            }

            TryAddInjury(home);
            TryAddInjury(away);

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

        // ── Penalty shootout ───────────────────────────────────────────────────

        /// <summary>
        /// Simulates a full penalty shootout between <paramref name="home"/> and
        /// <paramref name="away"/> (standard 5 kicks each, then sudden death).
        /// Returns the completed <see cref="PenaltyShootoutData"/>.
        /// </summary>
        public PenaltyShootoutData SimulatePenaltyShootout(TeamData home, TeamData away)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));
            if (away == null) throw new ArgumentNullException(nameof(away));

            var data        = new PenaltyShootoutData();
            int homePen     = 0;
            int awayPen     = 0;
            const int kicks = 5;

            var homeShooters = GetPenaltyShooters(home);
            var awayShooters = GetPenaltyShooters(away);

            // ── Standard 5 kicks ──────────────────────────────────────────────

            for (int i = 0; i < kicks; i++)
            {
                var hKicker    = homeShooters[i % homeShooters.Count];
                bool hScored   = TakePenaltyKick(hKicker);
                if (hScored) homePen++;
                data.kicks.Add(new PenaltyKick
                {
                    teamId     = home.id,
                    playerId   = hKicker.id,
                    playerName = hKicker.name,
                    scored     = hScored,
                    description = hScored
                        ? $"{hKicker.name} SCORED!"
                        : $"{hKicker.name} MISSED."
                });

                var aKicker    = awayShooters[i % awayShooters.Count];
                bool aScored   = TakePenaltyKick(aKicker);
                if (aScored) awayPen++;
                data.kicks.Add(new PenaltyKick
                {
                    teamId     = away.id,
                    playerId   = aKicker.id,
                    playerName = aKicker.name,
                    scored     = aScored,
                    description = aScored
                        ? $"{aKicker.name} SCORED!"
                        : $"{aKicker.name} MISSED."
                });
            }

            // ── Sudden death ──────────────────────────────────────────────────
            const int MaxSuddenDeathRounds = 20;
            int sdRound = 0;
            while (homePen == awayPen && sdRound < MaxSuddenDeathRounds)
            {
                int idx = sdRound % Math.Max(1, homeShooters.Count);

                var hKicker  = homeShooters[idx];
                bool hScored = TakePenaltyKick(hKicker);
                if (hScored) homePen++;
                data.kicks.Add(new PenaltyKick
                {
                    teamId     = home.id,
                    playerId   = hKicker.id,
                    playerName = hKicker.name,
                    scored     = hScored,
                    description = hScored ? $"SD: {hKicker.name} SCORED!" : $"SD: {hKicker.name} MISSED."
                });

                var aKicker  = awayShooters[idx];
                bool aScored = TakePenaltyKick(aKicker);
                if (aScored) awayPen++;
                data.kicks.Add(new PenaltyKick
                {
                    teamId     = away.id,
                    playerId   = aKicker.id,
                    playerName = aKicker.name,
                    scored     = aScored,
                    description = aScored ? $"SD: {aKicker.name} SCORED!" : $"SD: {aKicker.name} MISSED."
                });

                sdRound++;
            }

            data.homeScore    = homePen;
            data.awayScore    = awayPen;
            data.winnerTeamId = homePen > awayPen ? home.id : away.id;
            return data;
        }

        /// <summary>
        /// Returns the best penalty takers for <paramref name="team"/>,
        /// ordered by shooting attribute (excluding the goalkeeper).
        /// </summary>
        private List<PlayerData> GetPenaltyShooters(TeamData team)
        {
            var candidates = team.GetAvailablePlayers()
                .Where(p => !p.position.IsGoalkeeper())
                .OrderByDescending(p => p.attributes.shooting)
                .Take(8)
                .ToList();

            if (candidates.Count == 0)
                candidates = team.squad ?? new List<PlayerData>();

            return candidates;
        }

        /// <summary>
        /// Simulates a single penalty kick.  Conversion probability is
        /// 65 % base + up to 25 % from shooting attribute (range 65-90 %).
        /// </summary>
        private bool TakePenaltyKick(PlayerData kicker)
        {
            float shootingNorm = Mathf.Clamp(kicker?.attributes.shooting ?? 50, 0, 99) / 99f;
            float probability  = 0.65f + shootingNorm * 0.25f;
            return rng.NextDouble() < probability;
        }
    }
}
