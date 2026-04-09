using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Displays the player squad list, handles player selection, and provides
    /// position filtering.  Supports both simple row prefabs and rich
    /// <see cref="PlayerCard"/> prefabs.
    /// </summary>
    public partial class SquadUI : Control
    {
        [ExportGroup("References")]
        [Export] public Control playerListContainer;
        [Export] public PackedScene playerRowPrefab;
        [Export] public Label playerDetailName;
        [Export] public Label playerDetailStats;
        [Export] public Label playerDetailContract;

        [ExportGroup("Filters")]
        [Export] public OptionButton positionFilterDropdown;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        [ExportGroup("Team Header")]
        [Export] public Label teamNameHeaderLabel;
        [Export] public Label leagueMatchHeaderLabel;
        [Export] public Label squadCountLabel;
        [Export] public Label balanceHeaderLabel;

        [ExportGroup("Bottom Bar")]
        [Export] public Button playMatchBottomButton;

        private List<PlayerData> allPlayers = new List<PlayerData>();
        private PlayerData selectedPlayer;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (backButton != null) backButton.Pressed += OnBack;
            if (positionFilterDropdown != null)
            {
                positionFilterDropdown.Clear();
                positionFilterDropdown.AddItem("Todos");
                foreach (PlayerPosition pos in System.Enum.GetValues(typeof(PlayerPosition)))
                    positionFilterDropdown.AddItem(pos.ToString());
                positionFilterDropdown.ItemSelected += (idx) => OnFilterChanged((int)idx);
            }

            if (playMatchBottomButton != null)
                playMatchBottomButton.Pressed += () => FutbolJuego.Core.SceneNavigator.Instance?.GoToMatch();

            // Style the play button teal
            var playBtn = GetNodeOrNull<Button>("BG/VBox/BottomBar/PlayBtn");
            if (playBtn != null)
            {
                var sb = UITheme.MakeCardStyle(UITheme.AccentTeal, 10);
                sb.ContentMarginTop    = 12f;
                sb.ContentMarginBottom = 12f;
                playBtn.AddThemeStyleboxOverride("normal",  sb);
                playBtn.AddThemeStyleboxOverride("hover",   UITheme.MakeCardStyle(UITheme.AccentTealDark, 10));
                playBtn.AddThemeStyleboxOverride("pressed", UITheme.MakeCardStyle(UITheme.AccentTealDark, 10));
            }

            RefreshHeader();
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

            if (playerDetailName != null)
                playerDetailName.Text = $"{player.name}  OVR {player.CalculateOverall()}";

            if (playerDetailStats != null)
                playerDetailStats.Text =
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

            if (playerDetailContract != null)
                playerDetailContract.Text =
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

            foreach (var child in playerListContainer.GetChildren())
                child.QueueFree();

            foreach (var player in players)
            {
                var row = playerRowPrefab.Instantiate<Control>();
                playerListContainer.AddChild(row);

                // Rich row
                var rowItem = row as FutbolJuego.UI.Components.PlayerRowItem;
                if (rowItem != null)
                {
                    rowItem.Setup(player);
                    var captured = player;
                    if (rowItem.selectButton != null)
                        rowItem.selectButton.Pressed += () => OnPlayerSelected(captured);
                    continue;
                }

                // PlayerCard fallback
                var card = row as FutbolJuego.UI.Components.PlayerCard;
                if (card != null)
                {
                    card.Setup(player);
                }
                else
                {
                    var label = row.FindChild("Label", true, false) as Label;
                    if (label != null)
                        label.Text = $"{player.name}  |  {player.position}  |  OVR {player.CalculateOverall()}  |  ⚡{player.energy}";
                }

                var btn = row as Button ?? row.FindChild("Button", true, false) as Button;
                if (btn != null)
                {
                    var captured = player;
                    btn.Pressed += () => OnPlayerSelected(captured);
                }
            }
        }

        private void RefreshHeader()
        {
            var teams  = FutbolJuego.Data.DataLoader.LoadAllTeams();
            var career = FutbolJuego.Core.ServiceLocator.Get<FutbolJuego.Systems.CareerSystem>()?.ActiveCareer;
            if (teams == null || teams.Count == 0) return;
            var team = teams[0];

            if (teamNameHeaderLabel != null) teamNameHeaderLabel.Text = team.name;
            string currSymbol = career?.CurrencySymbol ?? "€";
            if (balanceHeaderLabel != null)
                balanceHeaderLabel.Text = career != null ? career.FormattedBalance : $"{currSymbol}—";
            int squadSize = team.playerIds?.Count ?? 0;
            if (squadCountLabel != null) squadCountLabel.Text = $"{squadSize}/29";

            var leagues = FutbolJuego.Data.DataLoader.LoadAllLeagues();
            foreach (var l in leagues)
            {
                if (l.teamIds != null && l.teamIds.Contains(team.id))
                {
                    if (leagueMatchHeaderLabel != null)
                        leagueMatchHeaderLabel.Text = $"MD {l.currentMatchday} — {l.name}";
                    break;
                }
            }
        }
    }
}
