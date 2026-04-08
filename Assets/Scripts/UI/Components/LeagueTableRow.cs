using UnityEngine;
using TMPro;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    /// <summary>
    /// A single row in the league standings table.
    /// Assign this component to the league-table-row prefab and call
    /// <see cref="Setup"/> when instantiating the row.
    /// </summary>
    public class LeagueTableRow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI playedText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI drawsText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI goalsForText;
        [SerializeField] private TextMeshProUGUI goalsAgainstText;
        [SerializeField] private TextMeshProUGUI goalDiffText;
        [SerializeField] private TextMeshProUGUI pointsText;

        // ── Setup ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Populates the row from a <see cref="LeagueTableEntry"/>.
        /// <paramref name="position"/> is the 1-based rank in the table.
        /// <paramref name="teamName"/> is the resolved display name.
        /// </summary>
        public void Setup(int position, string teamName, LeagueTableEntry entry)
        {
            if (entry == null) return;

            if (positionText)     positionText.text     = position.ToString();
            if (teamNameText)     teamNameText.text      = teamName ?? entry.teamId;
            if (playedText)       playedText.text        = entry.played.ToString();
            if (winsText)         winsText.text          = entry.wins.ToString();
            if (drawsText)        drawsText.text         = entry.draws.ToString();
            if (lossesText)       lossesText.text        = entry.losses.ToString();
            if (goalsForText)     goalsForText.text      = entry.goalsFor.ToString();
            if (goalsAgainstText) goalsAgainstText.text  = entry.goalsAgainst.ToString();
            if (goalDiffText)
            {
                int gd = entry.goalDifference;
                goalDiffText.text = gd > 0 ? $"+{gd}" : gd.ToString();
            }
            if (pointsText)       pointsText.text        = entry.points.ToString();
        }
    }
}
