using Godot;

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
        public static readonly Color BackgroundDark = new Color(0.118f, 0.118f, 0.184f);

        /// <summary>Elevated card / panel background.</summary>
        public static readonly Color CardBackground = new Color(0.16f, 0.16f, 0.24f);

        // ── Accent colours ─────────────────────────────────────────────────────

        /// <summary>Pitch-green accent used for positive actions and pitch tint.</summary>
        public static readonly Color PitchGreen = new Color(0.2f, 0.65f, 0.32f);

        /// <summary>Gold accent for star/legend items and call-to-action buttons.</summary>
        public static readonly Color AccentGold = new Color(1f, 0.84f, 0f);

        // ── Text colours ───────────────────────────────────────────────────────

        /// <summary>Primary text colour.</summary>
        public static readonly Color TextWhite = Colors.White;

        /// <summary>Secondary / muted text colour.</summary>
        public static readonly Color TextGray = new Color(0.6f, 0.6f, 0.65f);

        // ── Rarity border colours ──────────────────────────────────────────────

        /// <summary>Border colour for Normal-rarity player cards.</summary>
        public static readonly Color RarityNormal = new Color(0.6f, 0.6f, 0.6f);

        /// <summary>Border colour for Silver-rarity player cards.</summary>
        public static readonly Color RaritySilver = new Color(0.75f, 0.75f, 0.8f);

        /// <summary>Border colour for Gold-rarity player cards.</summary>
        public static readonly Color RarityGold = new Color(1f, 0.84f, 0f);

        /// <summary>Border colour for Star-rarity player cards.</summary>
        public static readonly Color RarityStar = new Color(0.5f, 0f, 1f);

        /// <summary>Border colour for Legend-rarity player cards.</summary>
        public static readonly Color RarityLegend = new Color(1f, 0.2f, 0.2f);

        /// <summary>Border colour for AllTimeGreat-rarity player cards (holographic rainbow approximation).</summary>
        public static readonly Color RarityAllTimeGreat = new Color(1f, 0.84f, 0.2f);
    }
}
