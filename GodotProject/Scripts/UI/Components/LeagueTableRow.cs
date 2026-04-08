using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    public partial class LeagueTableRow : HBoxContainer
    {
        [ExportGroup("UI References")]
        [Export] public Label positionText;
        [Export] public Label teamNameText;
        [Export] public Label playedText;
        [Export] public Label winsText;
        [Export] public Label drawsText;
        [Export] public Label lossesText;
        [Export] public Label goalsForText;
        [Export] public Label goalsAgainstText;
        [Export] public Label goalDiffText;
        [Export] public Label pointsText;

        public void Setup(int position, string teamName, LeagueTableEntry entry)
        {
            if (entry == null) return;

            if (positionText != null)     positionText.Text     = position.ToString();
            if (teamNameText != null)     teamNameText.Text     = teamName ?? entry.teamId;
            if (playedText != null)       playedText.Text       = entry.played.ToString();
            if (winsText != null)         winsText.Text         = entry.wins.ToString();
            if (drawsText != null)        drawsText.Text        = entry.draws.ToString();
            if (lossesText != null)       lossesText.Text       = entry.losses.ToString();
            if (goalsForText != null)     goalsForText.Text     = entry.goalsFor.ToString();
            if (goalsAgainstText != null) goalsAgainstText.Text = entry.goalsAgainst.ToString();
            if (goalDiffText != null)
            {
                int gd = entry.goalDifference;
                goalDiffText.Text = gd > 0 ? $"+{gd}" : gd.ToString();
            }
            if (pointsText != null) pointsText.Text = entry.points.ToString();
        }
    }
}
