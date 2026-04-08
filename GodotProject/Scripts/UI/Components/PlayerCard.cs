using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    public partial class PlayerCard : Control
    {
        [ExportGroup("UI References")]
        [Export] public Label playerNameText;
        [Export] public Label positionText;
        [Export] public Label ratingText;
        [Export] public Label ageText;
        [Export] public Label nationalityText;
        [Export] public Label valueText;
        [Export] public ColorRect rarityBorder;
        [Export] public ProgressBar energyBar;
        [Export] public ColorRect avatarImage;

        public void Setup(PlayerData player)
        {
            if (player == null) return;

            if (playerNameText != null)  playerNameText.Text  = player.name;
            if (positionText != null)    positionText.Text    = player.position.ToString();
            if (ratingText != null)      ratingText.Text      = player.overallRating.ToString();
            if (ageText != null)         ageText.Text         = $"{player.age} yrs";
            if (nationalityText != null) nationalityText.Text = player.nationality;
            if (valueText != null)       valueText.Text       = FormatValue(player.marketValue);

            if (energyBar != null)
                energyBar.Value = Mathf.Clamp(player.energy / 100f, 0f, 1f);

            if (rarityBorder != null)
                rarityBorder.Color = RarityToColor(player.rarity);
        }

        private static string FormatValue(long value)
        {
            if (value >= 1_000_000) return $"€{value / 1_000_000f:F1}M";
            if (value >= 1_000)     return $"€{value / 1_000f:F0}K";
            return $"€{value}";
        }

        private static Color RarityToColor(PlayerRarity rarity) => rarity switch
        {
            PlayerRarity.Silver       => FutbolJuego.UI.UITheme.RaritySilver,
            PlayerRarity.Gold         => FutbolJuego.UI.UITheme.RarityGold,
            PlayerRarity.Star         => FutbolJuego.UI.UITheme.RarityStar,
            PlayerRarity.Legend       => FutbolJuego.UI.UITheme.RarityLegend,
            PlayerRarity.AllTimeGreat => FutbolJuego.UI.UITheme.RarityAllTimeGreat,
            _                         => FutbolJuego.UI.UITheme.RarityNormal
        };
    }
}
