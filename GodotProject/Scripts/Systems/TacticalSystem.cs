using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.Utils;

namespace FutbolJuego.Systems
{
    /// <summary>Additional context provided to the tactical AI for in-match decisions.</summary>
    public class MatchContext
    {
        /// <summary>Current match minute.</summary>
        public int Minute;
        /// <summary>Score difference from the home team's perspective.</summary>
        public int ScoreDifference;
        /// <summary>Whether the team is playing at home.</summary>
        public bool IsHome;
    }

    /// <summary>
    /// Manages formation data and all tactical effectiveness calculations.
    /// Formation pitch coordinates use a 0-100 × 0-100 grid where
    /// (0, 50) is the team's own goal and (100, 50) is the opponent's.
    /// </summary>
    public class TacticalSystem
    {
        // ── Formation position data ────────────────────────────────────────────

        private static readonly Dictionary<Formation, List<Vector2>> FormationPositions
            = new Dictionary<Formation, List<Vector2>>
        {
            // 4-3-3 ─────────────────────────────────────────────────────────────
            [Formation.F433] = new List<Vector2>
            {
                new Vector2(10, 50),  // GK
                new Vector2(28, 15),  // LB
                new Vector2(28, 35),  // CB
                new Vector2(28, 65),  // CB
                new Vector2(28, 85),  // RB
                new Vector2(50, 30),  // CM
                new Vector2(50, 50),  // CM
                new Vector2(50, 70),  // CM
                new Vector2(75, 15),  // LW
                new Vector2(80, 50),  // ST
                new Vector2(75, 85),  // RW
            },
            // 4-4-2 ─────────────────────────────────────────────────────────────
            [Formation.F442] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(28, 15),
                new Vector2(28, 35),
                new Vector2(28, 65),
                new Vector2(28, 85),
                new Vector2(52, 15),
                new Vector2(52, 38),
                new Vector2(52, 62),
                new Vector2(52, 85),
                new Vector2(78, 35),
                new Vector2(78, 65),
            },
            // 3-5-2 ─────────────────────────────────────────────────────────────
            [Formation.F352] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(28, 25),
                new Vector2(28, 50),
                new Vector2(28, 75),
                new Vector2(52, 10),
                new Vector2(52, 30),
                new Vector2(52, 50),
                new Vector2(52, 70),
                new Vector2(52, 90),
                new Vector2(78, 35),
                new Vector2(78, 65),
            },
            // 4-2-3-1 ───────────────────────────────────────────────────────────
            [Formation.F4231] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(28, 15),
                new Vector2(28, 35),
                new Vector2(28, 65),
                new Vector2(28, 85),
                new Vector2(46, 35),
                new Vector2(46, 65),
                new Vector2(65, 15),
                new Vector2(65, 50),
                new Vector2(65, 85),
                new Vector2(82, 50),
            },
            // 5-3-2 ─────────────────────────────────────────────────────────────
            [Formation.F532] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(25, 10),
                new Vector2(25, 30),
                new Vector2(25, 50),
                new Vector2(25, 70),
                new Vector2(25, 90),
                new Vector2(52, 25),
                new Vector2(52, 50),
                new Vector2(52, 75),
                new Vector2(78, 35),
                new Vector2(78, 65),
            },
            // 4-1-4-1 ───────────────────────────────────────────────────────────
            [Formation.F4141] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(28, 15),
                new Vector2(28, 35),
                new Vector2(28, 65),
                new Vector2(28, 85),
                new Vector2(44, 50),
                new Vector2(62, 10),
                new Vector2(62, 33),
                new Vector2(62, 67),
                new Vector2(62, 90),
                new Vector2(82, 50),
            },
            // 3-4-3 ─────────────────────────────────────────────────────────────
            [Formation.F343] = new List<Vector2>
            {
                new Vector2(10, 50),
                new Vector2(28, 25),
                new Vector2(28, 50),
                new Vector2(28, 75),
                new Vector2(52, 15),
                new Vector2(52, 38),
                new Vector2(52, 62),
                new Vector2(52, 85),
                new Vector2(75, 20),
                new Vector2(80, 50),
                new Vector2(75, 80),
            },
        };

        // ── Formation counters table ───────────────────────────────────────────
        // Multiplier from 0.80 to 1.20 — row = attacking, column = defending.
        private static readonly Dictionary<(Formation, Formation), float> MatchupTable
            = new Dictionary<(Formation, Formation), float>
        {
            { (Formation.F433,  Formation.F442),  1.05f },
            { (Formation.F433,  Formation.F532),  0.93f },
            { (Formation.F442,  Formation.F433),  0.95f },
            { (Formation.F442,  Formation.F352),  1.08f },
            { (Formation.F4231, Formation.F442),  1.07f },
            { (Formation.F4231, Formation.F532),  0.90f },
            { (Formation.F352,  Formation.F4231), 1.10f },
            { (Formation.F532,  Formation.F433),  1.06f },
            { (Formation.F343,  Formation.F532),  0.88f },
            { (Formation.F343,  Formation.F442),  1.12f },
        };

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns pitch-grid coordinates for all 11 positions in
        /// <paramref name="formation"/>.
        /// </summary>
        public List<Vector2> GetFormationPositions(Formation formation)
        {
            return FormationPositions.TryGetValue(formation, out var positions)
                ? positions
                : FormationPositions[Formation.F433];
        }

        /// <summary>
        /// Returns a formation-matchup multiplier (0.80 – 1.20) representing
        /// how well <paramref name="home"/> counters <paramref name="away"/>.
        /// </summary>
        public float CalculateFormationMatchup(Formation home, Formation away)
        {
            if (MatchupTable.TryGetValue((home, away), out float mult))
                return mult;
            return 1.0f; // neutral if no specific counter
        }

        /// <summary>
        /// Calculates how effective a tactic is for the given squad,
        /// returning a 0-1 score.
        /// </summary>
        public float CalculateTacticalEffectiveness(TacticData tactic, List<Models.PlayerData> squad)
        {
            if (tactic == null || squad == null || squad.Count == 0) return 0.5f;

            float score = 0f;
            float total = 0f;

            foreach (var player in squad)
            {
                float fit = GetPositionalFit(player, tactic.formation);
                score += fit * player.CalculateOverall();
                total += player.CalculateOverall();
            }

            return total > 0f ? Mathf.Clamp(score / total, 0f, 1f) : 0.5f;
        }

        /// <summary>
        /// Returns the pressing effectiveness multiplier based on opponent tempo.
        /// </summary>
        public float CalculatePressingEffectiveness(int pressing, float opponentTempo)
        {
            float pressingNorm = pressing / 100f;
            float tempoNorm    = opponentTempo / 100f;
            // High press vs high tempo is less effective; high press vs low tempo is very effective
            float effectiveness = pressingNorm * (1.0f - tempoNorm * 0.4f);
            return Mathf.Clamp(effectiveness, 0.5f, 1.5f);
        }

        /// <summary>
        /// Generates a reasonable AI tactic based on team strengths and
        /// opponent characteristics.
        /// </summary>
        public TacticData GenerateAITactic(TeamData team, TeamData opponent, MatchContext context)
        {
            if (team == null) return new TacticData();

            var tactic = team.currentTactic?.Clone() ?? new TacticData();

            float teamOvr = team.GetAverageSquadRating();
            float oppOvr  = opponent?.GetAverageSquadRating() ?? 70f;

            if (teamOvr > oppOvr + 5f)
            {
                // Stronger team — dominant possession
                tactic.playStyle    = PlayStyle.Possession;
                tactic.pressing     = 65;
                tactic.tempo        = 60;
                tactic.defensiveLine = 65;
            }
            else if (teamOvr < oppOvr - 5f)
            {
                // Weaker team — defensive counter
                tactic.playStyle    = PlayStyle.CounterAttack;
                tactic.pressing     = 35;
                tactic.tempo        = 55;
                tactic.defensiveLine = 35;
            }
            else
            {
                // Balanced
                tactic.playStyle    = PlayStyle.Direct;
                tactic.pressing     = 50;
                tactic.tempo        = 55;
                tactic.defensiveLine = 50;
            }

            // In-match adaptation
            if (context != null)
            {
                if (context.ScoreDifference < -1 && context.Minute > 60)
                {
                    tactic.pressing  = Mathf.Min(85, tactic.pressing + 20);
                    tactic.tempo     = Mathf.Min(85, tactic.tempo + 20);
                }
                else if (context.ScoreDifference > 1 && context.Minute > 70)
                {
                    tactic.playStyle  = PlayStyle.ParkTheBus;
                    tactic.defensiveLine = Mathf.Max(20, tactic.defensiveLine - 20);
                }
            }

            return tactic;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static float GetPositionalFit(Models.PlayerData player, Formation formation)
        {
            // GKs always fit
            if (player.position == PlayerPosition.GK) return 1.0f;

            return formation switch
            {
                Formation.F433  => player.position.IsMidfielder() || player.position.IsForward() ? 0.9f : 0.75f,
                Formation.F442  => player.position.IsDefender() || player.position.IsMidfielder() ? 0.9f : 0.8f,
                Formation.F352  => player.position.IsMidfielder() ? 0.95f : 0.8f,
                Formation.F4231 => player.position == PlayerPosition.CAM ? 1.0f : 0.85f,
                Formation.F532  => player.position.IsDefender() ? 0.95f : 0.8f,
                Formation.F4141 => player.position == PlayerPosition.CDM ? 1.0f : 0.85f,
                Formation.F343  => player.position.IsForward() || player.position.IsMidfielder() ? 0.9f : 0.8f,
                _               => 0.85f
            };
        }
    }
}
