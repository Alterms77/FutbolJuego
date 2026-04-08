using FutbolJuego.Models;
using FutbolJuego.Systems;
using UnityEngine;

namespace FutbolJuego.AI
{
    /// <summary>
    /// Adapts tactics dynamically during a match based on scoreline, time
    /// remaining, and opponent formation.
    /// </summary>
    public class AITacticalEngine
    {
        private readonly TacticalSystem tacticalSystem;

        /// <summary>Initialises the engine with a shared tactical system.</summary>
        public AITacticalEngine(TacticalSystem tactics)
        {
            tacticalSystem = tactics;
        }

        // ── In-match adaptation ────────────────────────────────────────────────

        /// <summary>
        /// Returns an adapted tactic based on the current score difference and
        /// match minute.
        /// </summary>
        public TacticData AdaptTacticToMatchState(TacticData currentTactic,
                                                   int scoreDifference, int minute)
        {
            if (currentTactic == null) return new TacticData();

            var adapted = currentTactic.Clone();

            // Losing — push forward
            if (scoreDifference < -1 && minute > 60)
            {
                adapted.playStyle    = PlayStyle.HighPress;
                adapted.pressing     = Mathf.Min(90, adapted.pressing + 25);
                adapted.tempo        = Mathf.Min(90, adapted.tempo    + 20);
                adapted.defensiveLine = Mathf.Min(80, adapted.defensiveLine + 20);
                adapted.formation    = Formation.F343; // extra attacker
            }
            else if (scoreDifference < 0 && minute > 70)
            {
                adapted.pressing     = Mathf.Min(80, adapted.pressing + 15);
                adapted.tempo        = Mathf.Min(80, adapted.tempo    + 10);
            }
            // Winning — protect lead
            else if (scoreDifference > 1 && minute > 70)
            {
                adapted.playStyle    = PlayStyle.ParkTheBus;
                adapted.pressing     = Mathf.Max(20, adapted.pressing     - 20);
                adapted.defensiveLine = Mathf.Max(20, adapted.defensiveLine - 25);
                adapted.formation    = Formation.F532; // extra defender
            }
            else if (scoreDifference > 0 && minute > 75)
            {
                adapted.playStyle    = PlayStyle.CounterAttack;
                adapted.pressing     = Mathf.Max(30, adapted.pressing     - 10);
                adapted.defensiveLine = Mathf.Max(30, adapted.defensiveLine - 10);
            }

            return adapted;
        }

        // ── Formation counter ──────────────────────────────────────────────────

        /// <summary>
        /// Suggests the best counter formation against the opponent's shape.
        /// </summary>
        public TacticData CounterOpponentFormation(Formation opponentFormation,
                                                    TeamData aiTeam)
        {
            Formation counter = GenerateFormationCounter(opponentFormation);
            var tactic = aiTeam.currentTactic?.Clone() ?? new TacticData();
            tactic.formation = counter;
            return tactic;
        }

        /// <summary>
        /// Returns the recommended formation to counter
        /// <paramref name="enemyFormation"/>.
        /// </summary>
        public Formation GenerateFormationCounter(Formation enemyFormation) => enemyFormation switch
        {
            Formation.F433  => Formation.F532,  // extra defender vs 3 forwards
            Formation.F442  => Formation.F433,  // press their midfield
            Formation.F352  => Formation.F4231, // overload central mid
            Formation.F4231 => Formation.F442,  // match their double pivot
            Formation.F532  => Formation.F343,  // attack their high back-five
            Formation.F4141 => Formation.F352,  // flood midfield
            Formation.F343  => Formation.F532,  // defensive solidity
            _               => Formation.F433
        };

        // ── Substitution heuristic ─────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the AI should consider a substitution,
        /// based on fatigue levels and match state.
        /// </summary>
        public bool ShouldMakeSubstitution(TeamData aiTeam, MatchData match)
        {
            if (aiTeam?.squad == null || match == null) return false;
            if (match.currentMinute < 55) return false;

            var available = aiTeam.GetAvailablePlayers();
            return available.Exists(p => p.fatigue > 78);
        }
    }
}
