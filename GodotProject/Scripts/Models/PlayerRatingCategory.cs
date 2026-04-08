namespace FutbolJuego.Models
{
    /// <summary>
    /// Classification that determines the minimum and maximum overall rating
    /// a player can reach through performance and training.
    ///
    /// <list type="bullet">
    ///   <item><see cref="Regular"/>       – ordinary players (floor 45, ceiling 98).</item>
    ///   <item><see cref="ModernLegend"/>  – club/national icons such as Agüero or Riquelme (floor 88, ceiling 95).</item>
    ///   <item><see cref="HistoricLegend"/>– all-time greats such as Maradona or Pelé (floor 95, ceiling 100).</item>
    /// </list>
    ///
    /// Use <see cref="PlayerRatingCategoryExtensions"/> to retrieve the numeric bounds.
    /// </summary>
    public enum PlayerRatingCategory
    {
        Regular        = 0,
        ModernLegend   = 1,
        HistoricLegend = 2
    }

    // ── Static bounds ──────────────────────────────────────────────────────────

    /// <summary>
    /// Extension methods and constants for <see cref="PlayerRatingCategory"/>.
    /// Placing bounds here keeps <see cref="Models.PlayerData"/> independent of
    /// any system class.
    /// </summary>
    public static class PlayerRatingCategoryExtensions
    {
        // ── Floor values ───────────────────────────────────────────────────────

        /// <summary>Minimum overall a regular player may reach.</summary>
        public const int RegularFloor          = 45;
        /// <summary>Minimum overall a modern legend may reach.</summary>
        public const int ModernLegendFloor     = 88;
        /// <summary>Minimum overall a historic legend may reach.</summary>
        public const int HistoricLegendFloor   = 95;

        // ── Ceiling values ─────────────────────────────────────────────────────

        /// <summary>Maximum overall a regular player may reach.</summary>
        public const int RegularCeiling        = 98;
        /// <summary>
        /// Maximum overall a modern legend may reach.
        /// Note: intentionally lower than <see cref="RegularCeiling"/> —
        /// modern legends start high (floor 88) but their career ceiling is
        /// capped at 95, reserving 96-100 exclusively for historic all-time greats.
        /// A regular standout can reach 98 through exceptional sustained form,
        /// but cannot cross into legend territory without reclassification.
        /// </summary>
        public const int ModernLegendCeiling   = 95;
        /// <summary>Maximum overall a historic legend may reach.</summary>
        public const int HistoricLegendCeiling = 100;

        // ── Extension methods ──────────────────────────────────────────────────

        /// <summary>Returns the lowest overall rating allowed for this category.</summary>
        public static int GetFloor(this PlayerRatingCategory category) => category switch
        {
            PlayerRatingCategory.HistoricLegend => HistoricLegendFloor,
            PlayerRatingCategory.ModernLegend   => ModernLegendFloor,
            _                                   => RegularFloor
        };

        /// <summary>Returns the highest overall rating allowed for this category.</summary>
        public static int GetCeiling(this PlayerRatingCategory category) => category switch
        {
            PlayerRatingCategory.HistoricLegend => HistoricLegendCeiling,
            PlayerRatingCategory.ModernLegend   => ModernLegendCeiling,
            _                                   => RegularCeiling
        };

        /// <summary>
        /// Returns a display-friendly label for the category (Spanish UI).
        /// </summary>
        public static string GetLabel(this PlayerRatingCategory category) => category switch
        {
            PlayerRatingCategory.HistoricLegend => "Leyenda Histórica",
            PlayerRatingCategory.ModernLegend   => "Leyenda Moderna",
            _                                   => "Regular"
        };
    }
}
