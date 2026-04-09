using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    /// <summary>
    /// Rich squad-list row matching the dark-mobile reference design.
    /// Shows: avatar placeholder | name + nationality | position badge | salary | age | value | OVR circle.
    /// </summary>
    public partial class PlayerRowItem : Control
    {
        [ExportGroup("Avatar")]
        [Export] public ColorRect avatarRect;
        [Export] public ColorRect rarityAccent;

        [ExportGroup("Info")]
        [Export] public Label nameLabel;
        [Export] public Label nationalityLabel;
        [Export] public Label energyLabel;

        [ExportGroup("Position Badge")]
        [Export] public ColorRect positionBadgeRect;
        [Export] public Label positionBadgeLabel;
        [Export] public Label positionSideLabel;

        [ExportGroup("Stats")]
        [Export] public Label salaryLabel;
        [Export] public Label ageLabel;
        [Export] public Label valueLabel;
        [Export] public Label overallLabel;

        [ExportGroup("Interaction")]
        [Export] public Button selectButton;

        // ── Setup ──────────────────────────────────────────────────────────────

        public void Setup(PlayerData player)
        {
            if (player == null) return;

            // Avatar placeholder — tinted by rarity
            if (avatarRect  != null) avatarRect.Color  = UITheme.GetRarityColor(player.rarity);
            if (rarityAccent != null) rarityAccent.Color = UITheme.GetRarityColor(player.rarity);

            // Name & nationality
            if (nameLabel        != null) nameLabel.Text        = player.name;
            if (nationalityLabel != null) nationalityLabel.Text = player.nationality;
            if (energyLabel      != null) energyLabel.Text      = $"⚡{player.energy}";

            // Position badge
            Color posCol   = UITheme.GetPositionColor(player.position);
            string posAbbr = UITheme.GetPositionShort(player.position);
            string posSide = player.preferredSide ?? string.Empty;
            if (positionBadgeRect  != null) positionBadgeRect.Color = posCol;
            if (positionBadgeLabel != null) positionBadgeLabel.Text  = posAbbr;
            if (positionSideLabel  != null) positionSideLabel.Text   = posSide;

            // Stats columns
            long salary = player.weeklyWage;
            if (salaryLabel != null)
                salaryLabel.Text = salary >= 1_000 ? $"{salary / 1_000f:F0}K" : salary.ToString();
            if (ageLabel     != null) ageLabel.Text     = player.age.ToString();
            if (valueLabel   != null) valueLabel.Text   = FormatValue(player.marketValue);
            if (overallLabel != null) overallLabel.Text = player.overallRating.ToString();
        }

        private static string FormatValue(long v)
        {
            if (v >= 1_000_000) return $"€{v / 1_000_000f:F1}M";
            if (v >= 1_000)     return $"€{v / 1_000f:F0}K";
            return $"€{v}";
        }
    }
}
