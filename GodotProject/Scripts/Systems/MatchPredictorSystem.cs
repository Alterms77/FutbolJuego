using System;
using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    // ── Result types ───────────────────────────────────────────────────────────

    /// <summary>Win/draw/loss probability output from a Monte Carlo simulation.</summary>
    [Serializable]
    public class MatchPrediction
    {
        /// <summary>Probability the home team wins (0-1).</summary>
        public float winProbability;
        /// <summary>Probability of a draw (0-1).</summary>
        public float drawProbability;
        /// <summary>Probability the home team loses (0-1).</summary>
        public float lossProbability;
        /// <summary>Mean expected goals for the home team.</summary>
        public float expectedGoalsHome;
        /// <summary>Mean expected goals for the away team.</summary>
        public float expectedGoalsAway;
        /// <summary>
        /// Confidence score (0-1); higher when the quality gap between teams
        /// is large and simulation variance is low.
        /// </summary>
        public float confidenceScore;
    }

    /// <summary>Analysis of the impact a tactical change would have.</summary>
    [Serializable]
    public class TacticImpactAnalysis
    {
        /// <summary>Change in win probability (positive = improvement).</summary>
        public float winProbabilityChange;
        /// <summary>Change in expected goals for the home team.</summary>
        public float expectedGoalChange;
        /// <summary>Biggest benefit of the proposed tactic.</summary>
        public string primaryBenefit;
        /// <summary>Biggest risk of the proposed tactic.</summary>
        public string primaryRisk;
        /// <summary>Ordered list of concrete recommendations.</summary>
        public List<string> recommendations = new List<string>();
    }

    // ── MatchPredictorSystem ───────────────────────────────────────────────────

    /// <summary>
    /// Uses Monte Carlo simulation (N × Poisson sampling) to forecast match
    /// outcomes and quantify the impact of tactical changes.
    /// </summary>
    public class MatchPredictorSystem
    {
        private readonly MatchSimulationEngine simulationEngine;
        private readonly TacticalSystem tacticalSystem;

        private const int DefaultSimulations = 1000;

        /// <summary>Initialises the system with injected dependencies.</summary>
        public MatchPredictorSystem(MatchSimulationEngine engine, TacticalSystem tactics)
        {
            simulationEngine = engine ?? throw new ArgumentNullException(nameof(engine));
            tacticalSystem   = tactics ?? throw new ArgumentNullException(nameof(tactics));
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a <see cref="MatchPrediction"/> for the match between
        /// <paramref name="home"/> and <paramref name="away"/> using the teams'
        /// current tactics.
        /// </summary>
        public MatchPrediction PredictMatch(TeamData home, TeamData away)
            => RunMonteCarloSimulation(home, away, DefaultSimulations);

        /// <summary>
        /// Returns a prediction assuming the home team uses
        /// <paramref name="newHomeTactic"/> instead of their current tactic.
        /// </summary>
        public MatchPrediction PredictWithTacticChange(TeamData home, TeamData away,
                                                        TacticData newHomeTactic)
        {
            var original = home.currentTactic;
            home.currentTactic = newHomeTactic;
            var prediction = RunMonteCarloSimulation(home, away, DefaultSimulations);
            home.currentTactic = original;
            return prediction;
        }

        /// <summary>
        /// Analyses how switching to <paramref name="proposedTactic"/> would
        /// affect the home team's predicted performance.
        /// </summary>
        public TacticImpactAnalysis AnalyzeTacticImpact(TeamData home, TeamData away,
                                                          TacticData proposedTactic)
        {
            var current  = PredictMatch(home, away);
            var proposed = PredictWithTacticChange(home, away, proposedTactic);

            float winDelta = proposed.winProbability - current.winProbability;
            float xgDelta  = proposed.expectedGoalsHome - current.expectedGoalsHome;

            var analysis = new TacticImpactAnalysis
            {
                winProbabilityChange = winDelta,
                expectedGoalChange   = xgDelta
            };

            // Primary benefit
            if (winDelta > 0.05f)
                analysis.primaryBenefit = $"Win probability increases by {winDelta:P0}";
            else if (xgDelta > 0.15f)
                analysis.primaryBenefit = $"Expected goals improve by {xgDelta:F2}";
            else
                analysis.primaryBenefit = "Tactical balance slightly improved";

            // Primary risk
            if (winDelta < -0.05f)
                analysis.primaryRisk = $"Win probability drops by {Mathf.Abs(winDelta):P0}";
            else if (proposed.expectedGoalsAway > current.expectedGoalsAway + 0.2f)
                analysis.primaryRisk = "Opponent likely to create more chances";
            else
                analysis.primaryRisk = "Minimal risk identified";

            // Recommendations
            if (proposedTactic.pressing < home.currentTactic?.pressing)
                analysis.recommendations.Add("Lower pressing may give opponents more time on the ball.");
            if (proposedTactic.defensiveLine > 75)
                analysis.recommendations.Add("Very high defensive line — vulnerable to pace in behind.");
            if (proposedTactic.playStyle == PlayStyle.ParkTheBus)
                analysis.recommendations.Add("Park The Bus limits own goal threat. Use only when protecting a lead.");
            if (proposedTactic.playStyle == PlayStyle.HighPress && home.GetAverageSquadRating() < 68)
                analysis.recommendations.Add("Squad stamina may be insufficient to sustain 90 min high press.");

            return analysis;
        }

        // ── Monte Carlo ────────────────────────────────────────────────────────

        /// <summary>
        /// Runs <paramref name="simulations"/> independent Poisson-based match
        /// simulations and aggregates win/draw/loss probabilities.
        /// </summary>
        private MatchPrediction RunMonteCarloSimulation(TeamData home, TeamData away,
                                                         int simulations = DefaultSimulations)
        {
            int homeWins = 0, draws = 0, awayWins = 0;
            float totalHomeXG = 0f, totalAwayXG = 0f;

            // We need xG per simulation — re-derive from engine internals via simulate
            for (int i = 0; i < simulations; i++)
            {
                var match = simulationEngine.SimulateMatch(home, away);
                totalHomeXG += match.statistics.homeXG;
                totalAwayXG += match.statistics.awayXG;

                if (match.homeScore > match.awayScore) homeWins++;
                else if (match.homeScore == match.awayScore) draws++;
                else awayWins++;
            }

            float n = simulations;
            float homeOvr = home.GetAverageSquadRating();
            float awayOvr = away.GetAverageSquadRating();
            float qualityGap = Mathf.Abs(homeOvr - awayOvr) / 30f; // 0-1

            return new MatchPrediction
            {
                winProbability     = homeWins / n,
                drawProbability    = draws    / n,
                lossProbability    = awayWins / n,
                expectedGoalsHome  = totalHomeXG / n,
                expectedGoalsAway  = totalAwayXG / n,
                confidenceScore    = Mathf.Clamp(0.5f + qualityGap * 0.5f, 0f, 1f)
            };
        }
    }
}
