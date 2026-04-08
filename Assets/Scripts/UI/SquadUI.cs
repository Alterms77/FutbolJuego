using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Displays the player squad list, handles player selection, and provides
    /// position filtering.  Supports both simple row prefabs and rich
    /// <see cref="PlayerCard"/> prefabs.
    /// </summary>
    public class SquadUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerRowPrefab;
        [SerializeField] private TextMeshProUGUI playerDetailName;
        [SerializeField] private TextMeshProUGUI playerDetailStats;
        [SerializeField] private TextMeshProUGUI playerDetailContract;

        [Header("Filters")]
        [SerializeField] private TMP_Dropdown positionFilterDropdown;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        private List<PlayerData> allPlayers = new List<PlayerData>();
        private PlayerData selectedPlayer;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backButton) backButton.onClick.AddListener(OnBack);
            if (positionFilterDropdown != null)
            {
                positionFilterDropdown.ClearOptions();
                var options = new List<string> { "Todos" };
                foreach (PlayerPosition pos in System.Enum.GetValues(typeof(PlayerPosition)))
                    options.Add(pos.ToString());
                positionFilterDropdown.AddOptions(options);
                positionFilterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }
        }

        // ── Navigation ─────────────────────────────────────────────────────────

        public void OnBack() => FutbolJuego.Core.SceneNavigator.Instance?.GoToDashboard();

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Populates the squad list with <paramref name="players"/>.</summary>
        public void DisplaySquad(List<PlayerData> players)
        {
            allPlayers = players ?? new List<PlayerData>();
            RebuildList(allPlayers);
        }

        /// <summary>Filters the visible list to the given position.</summary>
        public void FilterByPosition(PlayerPosition pos)
        {
            var filtered = allPlayers.Where(p => p.position == pos).ToList();
            RebuildList(filtered);
        }

        /// <summary>Shows all positions (clears position filter).</summary>
        public void ClearFilter() => RebuildList(allPlayers);

        // ── Player selection ───────────────────────────────────────────────────

        /// <summary>Called when the player taps a row in the squad list.</summary>
        public void OnPlayerSelected(PlayerData player)
        {
            selectedPlayer = player;
            ShowPlayerDetails(player);
        }

        /// <summary>Populates the detail panel with data for <paramref name="player"/>.</summary>
        public void ShowPlayerDetails(PlayerData player)
        {
            if (player == null) return;

            if (playerDetailName)
                playerDetailName.text = $"{player.name}  OVR {player.CalculateOverall()}";

            if (playerDetailStats)
                playerDetailStats.text =
                    $"SPD {player.attributes.speed}  " +
                    $"SHT {player.attributes.shooting}  " +
                    $"PAS {player.attributes.passing}  " +
                    $"DEF {player.attributes.defense}  " +
                    $"PHY {player.attributes.physical}  " +
                    $"INT {player.attributes.intelligence}\n" +
                    $"Age: {player.age}  |  {player.nationality}  |  {player.position}\n" +
                    $"Energy: {player.energy}  Morale: {player.morale}  Fatigue: {player.fatigue}  " +
                    $"{(player.injuryDaysRemaining > 0 ? $"Injured ({player.injuryDaysRemaining}d)" : "Fit")}\n" +
                    $"Rarity: {player.rarity}";

            if (playerDetailContract)
                playerDetailContract.text =
                    $"Wage: £{player.weeklyWage:N0}/wk  " +
                    $"Value: £{player.marketValue:N0}  " +
                    $"Contract until: {player.contractExpiry:MMM yyyy}";
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void OnFilterChanged(int index)
        {
            if (index == 0)
            {
                ClearFilter();
                return;
            }

            var positions = (PlayerPosition[])System.Enum.GetValues(typeof(PlayerPosition));
            if (index - 1 < positions.Length)
                FilterByPosition(positions[index - 1]);
        }

        private void RebuildList(List<PlayerData> players)
        {
            if (playerListContainer == null || playerRowPrefab == null) return;

            // Clear existing rows
            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);

            foreach (var player in players)
            {
                var row = Instantiate(playerRowPrefab, playerListContainer);

                // Try rich PlayerCard component first
                var card = row.GetComponent<PlayerCard>();
                if (card != null)
                {
                    card.Setup(player);
                }
                else
                {
                    // Fallback: plain text label
                    var label = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (label)
                        label.text = $"{player.name}  |  {player.position}  |  OVR {player.CalculateOverall()}  |  ⚡{player.energy}";
                }

                var btn = row.GetComponent<Button>();
                if (btn)
                {
                    var captured = player;
                    btn.onClick.AddListener(() => OnPlayerSelected(captured));
                }
            }
        }
    }
}
