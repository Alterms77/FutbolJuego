namespace FutbolJuego.Utils
{
    /// <summary>
    /// Global compile-time constants shared across all game systems.
    /// Change values here to affect the entire simulation.
    /// </summary>
    public static class Constants
    {
        // ── Squad / match ──────────────────────────────────────────────────────

        /// <summary>Maximum number of players allowed in a squad.</summary>
        public const int MaxSquadSize = 25;

        /// <summary>Number of players in a starting eleven.</summary>
        public const int StartingEleven = 11;

        /// <summary>Standard match duration in minutes.</summary>
        public const int MatchDurationMinutes = 90;

        /// <summary>Maximum in-game substitutions per match.</summary>
        public const int MaxSubstitutions = 3;

        // ── Statistics ─────────────────────────────────────────────────────────

        /// <summary>Minimum value for any numeric player attribute.</summary>
        public const int StatMin = 0;

        /// <summary>Maximum value for any numeric player attribute.</summary>
        public const int StatMax = 99;

        // ── Simulation modifiers ───────────────────────────────────────────────

        /// <summary>
        /// Multiplier applied to xG for the home team.
        /// Based on real-world home advantage studies (~6-8% more goals).
        /// </summary>
        public const float HomeAdvantage = 1.08f;

        /// <summary>Younger bound of the player peak-age window.</summary>
        public const int PeakAgeMin = 26;

        /// <summary>Upper bound of the player peak-age window.</summary>
        public const int PeakAgeMax = 30;

        // ── Finance ────────────────────────────────────────────────────────────

        /// <summary>Starting transfer budget for a new manager save.</summary>
        public const long StartingTransferBudget = 5_000_000L;

        /// <summary>Starting bank balance for a new manager save.</summary>
        public const long StartingBalance = 10_000_000L;

        // ── Formation strings (display names) ──────────────────────────────────

        /// <summary>Display names for each formation enum value.</summary>
        public static readonly System.Collections.Generic.Dictionary<
            Models.Formation, string> FormationNames =
            new System.Collections.Generic.Dictionary<Models.Formation, string>
            {
                { Models.Formation.F433,  "4-3-3"   },
                { Models.Formation.F442,  "4-4-2"   },
                { Models.Formation.F352,  "3-5-2"   },
                { Models.Formation.F4231, "4-2-3-1" },
                { Models.Formation.F532,  "5-3-2"   },
                { Models.Formation.F4141, "4-1-4-1" },
                { Models.Formation.F343,  "3-4-3"   },
            };

        // ── Position groups ────────────────────────────────────────────────────

        /// <summary>Positions considered as defenders (including LB, RB).</summary>
        public static readonly Models.PlayerPosition[] DefenderPositions =
        {
            Models.PlayerPosition.CB,
            Models.PlayerPosition.LB,
            Models.PlayerPosition.RB
        };

        /// <summary>Positions considered as midfielders.</summary>
        public static readonly Models.PlayerPosition[] MidfielderPositions =
        {
            Models.PlayerPosition.CDM,
            Models.PlayerPosition.CM,
            Models.PlayerPosition.CAM,
            Models.PlayerPosition.LM,
            Models.PlayerPosition.RM
        };

        /// <summary>Positions considered as forwards / attackers.</summary>
        public static readonly Models.PlayerPosition[] ForwardPositions =
        {
            Models.PlayerPosition.LW,
            Models.PlayerPosition.RW,
            Models.PlayerPosition.CF,
            Models.PlayerPosition.ST
        };

        // ── XP thresholds ──────────────────────────────────────────────────────

        /// <summary>XP awarded per league win.</summary>
        public const int XpPerWin = 50;

        /// <summary>XP awarded per league draw.</summary>
        public const int XpPerDraw = 20;

        /// <summary>XP awarded per league loss.</summary>
        public const int XpPerLoss = 5;
    }
}
