using Godot;

namespace FutbolJuego.Models
{
    /// <summary>
    /// Seasonal performance counters accumulated by a player during a season.
    /// Reset at the start of each new season via <see cref="Reset"/>.
    /// Used by <see cref="FutbolJuego.Systems.PlayerRatingSystem"/> to derive
    /// post-match rating adjustments.
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        // ── Counters ───────────────────────────────────────────────────────────

        /// <summary>Number of matches the player appeared in.</summary>
        public int matchesPlayed;
        /// <summary>Approximate minutes on the pitch.</summary>
        public int minutesPlayed;

        // Field-player stats
        /// <summary>Goals scored (all competitions).</summary>
        public int goals;
        /// <summary>Assists (directly led to a goal).</summary>
        public int assists;

        // Goalkeeper / defender stats
        /// <summary>Matches without conceding a goal (relevant for GK and DEF).</summary>
        public int cleanSheets;
        /// <summary>On-target shots stopped by the goalkeeper.</summary>
        public int savesMade;

        // Discipline
        /// <summary>Yellow cards received.</summary>
        public int yellowCards;
        /// <summary>Red cards received.</summary>
        public int redCards;

        /// <summary>Man-of-the-match awards.</summary>
        public int manOfTheMatchAwards;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Resets all counters to zero (call at season start).</summary>
        public void Reset()
        {
            matchesPlayed        = 0;
            minutesPlayed        = 0;
            goals                = 0;
            assists              = 0;
            cleanSheets          = 0;
            savesMade            = 0;
            yellowCards          = 0;
            redCards             = 0;
            manOfTheMatchAwards  = 0;
        }

        /// <summary>
        /// Returns a normalised performance delta in the range [–1, +1].
        /// Positive values indicate above-average performance (rating should rise);
        /// negative values indicate below-average performance (rating should fall).
        ///
        /// The metric is position-specific:
        /// <list type="bullet">
        ///   <item>GK  — saves-per-match and clean-sheet rate.</item>
        ///   <item>DEF — clean-sheet rate and low red-card frequency.</item>
        ///   <item>MID — combined goals + assists rate.</item>
        ///   <item>FWD — goals-per-match rate.</item>
        /// </list>
        /// Returns 0 when <see cref="matchesPlayed"/> is zero.
        /// </summary>
        public float GetPerformanceDelta(PlayerPosition position)
        {
            if (matchesPlayed <= 0) return 0f;

            float delta;

            switch (position)
            {
                case PlayerPosition.GK:
                    // Benchmark: ~3 saves/match and 0.35 clean-sheet rate = average (delta 0)
                    float savesPerMatch = (float)savesMade / matchesPlayed;
                    float csRateGK     = (float)cleanSheets / matchesPlayed;
                    delta = (savesPerMatch - 3f) * 0.08f + (csRateGK - 0.35f) * 0.5f;
                    break;

                case PlayerPosition.CB:
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    // Benchmark: 0.30 clean-sheet rate = average
                    float csRateDef  = (float)cleanSheets / matchesPlayed;
                    float redPenalty = redCards * 0.15f;
                    delta = (csRateDef - 0.30f) * 0.6f - redPenalty;
                    break;

                case PlayerPosition.CDM:
                case PlayerPosition.CM:
                    // Benchmark: 0.25 goal-contributions/match (goals + assists)
                    float contribMid = (float)(goals + assists) / matchesPlayed;
                    delta = (contribMid - 0.25f) * 0.5f;
                    break;

                case PlayerPosition.CAM:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    // Benchmark: 0.40 goal-contributions/match
                    float contribWing = (float)(goals + assists) / matchesPlayed;
                    delta = (contribWing - 0.40f) * 0.5f;
                    break;

                case PlayerPosition.CF:
                case PlayerPosition.ST:
                    // Benchmark: 0.50 goals/match
                    float goalsPerMatch = (float)goals / matchesPlayed;
                    delta = (goalsPerMatch - 0.50f) * 0.6f;
                    break;

                default:
                    float genericRate = (float)(goals + assists) / matchesPlayed;
                    delta = (genericRate - 0.30f) * 0.4f;
                    break;
            }

            // Discipline penalty (red cards are harsh)
            delta -= redCards * 0.10f;

            return Mathf.Clamp(delta, -1f, 1f);
        }
    }
}
