namespace FutbolJuego.UI
{
    /// <summary>
    /// Centralised colour palette and style constants for all game screens.
    /// Keeps colours consistent without requiring designers to remember hex values.
    /// </summary>
    public static class UITheme
    {
        // ── Backgrounds ────────────────────────────────────────────────────────

        /// <summary>Primary dark background (#1E1E2F).</summary>
        public static readonly UnityEngine.Color BackgroundDark =
            new UnityEngine.Color(0.118f, 0.118f, 0.184f);

        /// <summary>Elevated card / panel background.</summary>
        public static readonly UnityEngine.Color CardBackground =
            new UnityEngine.Color(0.16f, 0.16f, 0.24f);

        // ── Accent colours ─────────────────────────────────────────────────────

        /// <summary>Pitch-green accent used for positive actions and pitch tint.</summary>
        public static readonly UnityEngine.Color PitchGreen =
            new UnityEngine.Color(0.2f, 0.65f, 0.32f);

        /// <summary>Gold accent for star/legend items and call-to-action buttons.</summary>
        public static readonly UnityEngine.Color AccentGold =
            new UnityEngine.Color(1f, 0.84f, 0f);

        // ── Text colours ───────────────────────────────────────────────────────

        /// <summary>Primary text colour.</summary>
        public static readonly UnityEngine.Color TextWhite = UnityEngine.Color.white;

        /// <summary>Secondary / muted text colour.</summary>
        public static readonly UnityEngine.Color TextGray =
            new UnityEngine.Color(0.6f, 0.6f, 0.65f);

        // ── Rarity border colours ──────────────────────────────────────────────

        /// <summary>Border colour for Normal-rarity player cards.</summary>
        public static readonly UnityEngine.Color RarityNormal =
            new UnityEngine.Color(0.6f, 0.6f, 0.6f);

        /// <summary>Border colour for Silver-rarity player cards.</summary>
        public static readonly UnityEngine.Color RaritySilver =
            new UnityEngine.Color(0.75f, 0.75f, 0.8f);

        /// <summary>Border colour for Gold-rarity player cards.</summary>
        public static readonly UnityEngine.Color RarityGold =
            new UnityEngine.Color(1f, 0.84f, 0f);

        /// <summary>Border colour for Star-rarity player cards.</summary>
        public static readonly UnityEngine.Color RarityStar =
            new UnityEngine.Color(0.5f, 0f, 1f);

        /// <summary>Border colour for Legend-rarity player cards.</summary>
        public static readonly UnityEngine.Color RarityLegend =
            new UnityEngine.Color(1f, 0.2f, 0.2f);

        /// <summary>Border colour for AllTimeGreat-rarity player cards (holographic rainbow approximation).</summary>
        public static readonly UnityEngine.Color RarityAllTimeGreat =
            new UnityEngine.Color(1f, 0.84f, 0.2f);
    }
}
