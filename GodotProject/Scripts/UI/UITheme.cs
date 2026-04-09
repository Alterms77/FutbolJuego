using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Centralised colour palette matching the dark navy mobile design.
    /// </summary>
    public static class UITheme
    {
        // ── Backgrounds ────────────────────────────────────────────────────────
        public static readonly Color BackgroundDark  = new Color(0.11f,  0.13f,  0.20f);
        public static readonly Color CardBackground  = new Color(0.145f, 0.173f, 0.247f);
        public static readonly Color HeaderBackground= new Color(0.125f, 0.149f, 0.224f);

        // ── Accent ─────────────────────────────────────────────────────────────
        public static readonly Color AccentTeal      = new Color(0f,     0.722f, 0.58f);
        public static readonly Color AccentTealDark  = new Color(0f,     0.6f,   0.48f);
        public static readonly Color AccentGold      = new Color(1f,     0.84f,  0f);
        public static readonly Color PitchGreen      = new Color(0.18f,  0.55f,  0.27f);
        public static readonly Color PitchLineLt     = new Color(0.22f,  0.62f,  0.32f);

        // ── Text ───────────────────────────────────────────────────────────────
        public static readonly Color TextWhite       = Colors.White;
        public static readonly Color TextGray        = new Color(0.54f,  0.56f,  0.63f);
        public static readonly Color TextGold        = new Color(1f,     0.84f,  0f);

        // ── Position badge colours ─────────────────────────────────────────────
        public static readonly Color PositionGK      = new Color(0.204f, 0.596f, 0.859f);
        public static readonly Color PositionDef     = new Color(0f,     0.722f, 0.58f);
        public static readonly Color PositionMid     = new Color(0.545f, 0.451f, 0.329f);
        public static readonly Color PositionFwd     = new Color(0.906f, 0.298f, 0.235f);

        // ── Rarity border colours ──────────────────────────────────────────────
        public static readonly Color RarityNormal    = new Color(0.6f,   0.6f,   0.6f);
        public static readonly Color RaritySilver    = new Color(0.75f,  0.75f,  0.8f);
        public static readonly Color RarityGold      = new Color(1f,     0.84f,  0f);
        public static readonly Color RarityStar      = new Color(0.5f,   0f,     1f);
        public static readonly Color RarityLegend    = new Color(1f,     0.2f,   0.2f);
        public static readonly Color RarityAllTimeGreat = new Color(1f,  0.84f,  0.2f);

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Returns the position badge colour for a player position.</summary>
        public static Color GetPositionColor(PlayerPosition pos) => pos switch
        {
            PlayerPosition.GK                              => PositionGK,
            PlayerPosition.CB or PlayerPosition.LB
                or PlayerPosition.RB                      => PositionDef,
            PlayerPosition.CDM or PlayerPosition.CM
                or PlayerPosition.CAM or PlayerPosition.LM
                or PlayerPosition.RM                      => PositionMid,
            PlayerPosition.LW or PlayerPosition.RW
                or PlayerPosition.ST or PlayerPosition.CF => PositionFwd,
            _                                             => RarityNormal
        };

        /// <summary>Short position abbreviation used on position badges.</summary>
        public static string GetPositionShort(PlayerPosition pos) => pos switch
        {
            PlayerPosition.GK  => "P",
            PlayerPosition.CB  => "DC",
            PlayerPosition.LB  => "DL",
            PlayerPosition.RB  => "DL",
            PlayerPosition.CDM => "MC",
            PlayerPosition.CM  => "M",
            PlayerPosition.CAM => "MC",
            PlayerPosition.LM  => "M",
            PlayerPosition.RM  => "M",
            PlayerPosition.LW  => "EI",
            PlayerPosition.RW  => "ED",
            PlayerPosition.ST  => "Del",
            PlayerPosition.CF  => "Del",
            _                  => pos.ToString()
        };

        /// <summary>Returns the rarity border colour for a player.</summary>
        public static Color GetRarityColor(PlayerRarity rarity) => rarity switch
        {
            PlayerRarity.Silver       => RaritySilver,
            PlayerRarity.Gold         => RarityGold,
            PlayerRarity.Star         => RarityStar,
            PlayerRarity.Legend       => RarityLegend,
            PlayerRarity.AllTimeGreat => RarityAllTimeGreat,
            _                         => RarityNormal
        };

        // ── StyleBox helpers (applied in code) ────────────────────────────────

        /// <summary>Creates a dark card StyleBoxFlat with rounded corners.</summary>
        public static StyleBoxFlat MakeCardStyle(Color? bgColor = null, int radius = 10)
        {
            var sb = new StyleBoxFlat();
            sb.BgColor = bgColor ?? CardBackground;
            sb.SetCornerRadiusAll(radius);
            sb.ContentMarginLeft   = 12f;
            sb.ContentMarginRight  = 12f;
            sb.ContentMarginTop    = 10f;
            sb.ContentMarginBottom = 10f;
            return sb;
        }

        /// <summary>Applies the dark background color to a ColorRect.</summary>
        public static void ApplyBg(ColorRect rect)
        {
            if (rect != null) rect.Color = BackgroundDark;
        }
    }
}
