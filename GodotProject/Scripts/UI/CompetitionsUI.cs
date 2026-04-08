using System.Collections.Generic;
using Godot;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Competitions scene controller.  Displays:
    ///   • League standings (with matchday info)
    ///   • Cup competition brackets
    ///   • Trophy cabinet for any team
    /// Tabs are selected with <see cref="ShowLeagueTab"/>,
    /// <see cref="ShowCupsTab"/>, and <see cref="ShowTrophiesTab"/>.
    /// </summary>
    public partial class CompetitionsUI : Control
    {
        [ExportGroup("League Selector")]
        [Export] public OptionButton leagueDropdown;

        [ExportGroup("League Info")]
        [Export] public Label leagueNameText;
        [Export] public Label matchdayText;

        [ExportGroup("League Table")]
        [Export] public Control tableContainer;
        [Export] public PackedScene tableRowPrefab;

        [ExportGroup("Cups Panel")]
        [Export] public Control cupsPanel;
        [Export] public Control cupListContainer;
        [Export] public PackedScene cupRowPrefab;

        [ExportGroup("Trophies Panel")]
        [Export] public Control trophiesPanel;
        [Export] public Control trophyContainer;
        [Export] public PackedScene trophyRowPrefab;
        [Export] public LineEdit trophyTeamInput;
        [Export] public Button trophySearchButton;

        [ExportGroup("Tab Buttons")]
        [Export] public Button tabLeagueButton;
        [Export] public Button tabCupsButton;
        [Export] public Button tabTrophiesButton;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        // ── Private state ──────────────────────────────────────────────────────

        private List<LeagueData> leagues = new List<LeagueData>();
        private List<TeamData>   teams   = new List<TeamData>();
        private List<CupData>    cups    = new List<CupData>();
        private int selectedLeagueIndex  = 0;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            teams   = DataLoader.LoadAllTeams();
            leagues = DataLoader.LoadAllLeagues();
            cups    = DataLoader.LoadAllCups();

            if (leagueDropdown != null)
            {
                leagueDropdown.Clear();
                foreach (var l in leagues) leagueDropdown.AddItem(l.name);
                leagueDropdown.ItemSelected += (idx) => OnLeagueSelected((int)idx);
            }

            if (tabLeagueButton != null)   tabLeagueButton.Pressed   += ShowLeagueTab;
            if (tabCupsButton != null)     tabCupsButton.Pressed     += ShowCupsTab;
            if (tabTrophiesButton != null) tabTrophiesButton.Pressed += ShowTrophiesTab;
            if (trophySearchButton != null) trophySearchButton.Pressed += OnTrophySearch;
            if (backButton != null) backButton.Pressed += OnBack;

            ShowLeagueTab();
        }

        // ── Tab switching ──────────────────────────────────────────────────────

        /// <summary>Shows the league standings tab.</summary>
        public void ShowLeagueTab()
        {
            SetPanelVisible(cupsPanel, false);
            SetPanelVisible(trophiesPanel, false);
            ShowLeague(selectedLeagueIndex);
        }

        /// <summary>Shows the cups bracket tab for the selected league.</summary>
        public void ShowCupsTab()
        {
            SetPanelVisible(trophiesPanel, false);
            SetPanelVisible(cupsPanel, true);
            BuildCupsList(selectedLeagueIndex);
        }

        /// <summary>Shows the trophy cabinet tab.</summary>
        public void ShowTrophiesTab()
        {
            SetPanelVisible(cupsPanel, false);
            SetPanelVisible(trophiesPanel, true);
            BuildTrophyList(string.Empty);
        }

        // ── League ────────────────────────────────────────────────────────────

        /// <summary>Shows the standings for the league at <paramref name="index"/>.</summary>
        public void ShowLeague(int index)
        {
            if (leagues == null || index < 0 || index >= leagues.Count) return;
            selectedLeagueIndex = index;
            var league = leagues[index];

            if (leagueNameText != null) leagueNameText.Text = league.name;
            if (matchdayText != null)   matchdayText.Text   = $"Matchday {league.currentMatchday}";

            BuildTable(league);
        }

        // ── Cups ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Populates the cups panel with all cups belonging to the league
        /// at <paramref name="leagueIndex"/>.
        /// </summary>
        public void BuildCupsList(int leagueIndex)
        {
            if (cupListContainer == null || cupRowPrefab == null) return;

            foreach (var child in cupListContainer.GetChildren()) child.QueueFree();

            if (leagues == null || leagueIndex < 0 || leagueIndex >= leagues.Count) return;
            string leagueId = leagues[leagueIndex].id;

            foreach (var cup in cups)
            {
                if (cup.leagueId != leagueId) continue;

                var row   = cupRowPrefab.Instantiate<Control>();
                cupListContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label == null) continue;

                string status = string.IsNullOrEmpty(cup.winnerId)
                    ? $"{cup.currentRoundName} (Round {cup.currentRound})"
                    : $"WON by {ResolveTeamName(cup.winnerId)}";

                label.Text = $"🏆 {cup.name}  –  {status}";

                foreach (var fixture in cup.currentRoundFixtures)
                {
                    var fRow   = cupRowPrefab.Instantiate<Control>();
                    cupListContainer.AddChild(fRow);
                    var fLabel = fRow.FindChild("Label", true, false) as Label;
                    if (fLabel != null)
                        fLabel.Text = $"   {ResolveTeamName(fixture.homeTeamId)} vs " +
                                      $"{ResolveTeamName(fixture.awayTeamId)}";
                }
            }
        }

        // ── Trophies ──────────────────────────────────────────────────────────

        /// <summary>
        /// Populates the trophy cabinet.  Pass an empty string to show all teams.
        /// </summary>
        public void BuildTrophyList(string filterTeamId)
        {
            if (trophyContainer == null || trophyRowPrefab == null) return;
            foreach (var child in trophyContainer.GetChildren()) child.QueueFree();

            var trophySystem = ServiceLocator.Get<TrophySystem>();
            if (trophySystem == null)
            {
                AppendTrophyRow("Trophy system not yet initialised.");
                return;
            }

            var all = string.IsNullOrEmpty(filterTeamId)
                ? trophySystem.GetAllTrophies()
                : trophySystem.GetTeamTrophies(filterTeamId);

            if (all.Count == 0)
            {
                AppendTrophyRow(string.IsNullOrEmpty(filterTeamId)
                    ? "No trophies awarded yet."
                    : $"No trophies for {ResolveTeamName(filterTeamId)}.");
                return;
            }

            foreach (var trophy in all)
            {
                var row   = trophyRowPrefab.Instantiate<Control>();
                trophyContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text = $"🏅 {trophy.name}  ({ResolveTeamName(trophy.teamId)})";
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void OnLeagueSelected(int index)
        {
            selectedLeagueIndex = index;
            ShowLeague(index);
        }

        private void OnTrophySearch()
        {
            string query = trophyTeamInput != null ? trophyTeamInput.Text.Trim() : string.Empty;
            var found = teams?.Find(t =>
                t.name.ToLower().Contains(query.ToLower()) ||
                t.id.ToLower().Contains(query.ToLower()));

            BuildTrophyList(found?.id ?? query);
        }

        private void BuildTable(LeagueData league)
        {
            if (tableContainer == null || tableRowPrefab == null) return;
            foreach (var child in tableContainer.GetChildren()) child.QueueFree();

            var table = league.table;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                var row   = tableRowPrefab.Instantiate<Control>();
                tableContainer.AddChild(row);

                var rowComp = row as LeagueTableRow;
                if (rowComp != null)
                {
                    rowComp.Setup(i + 1, ResolveTeamName(entry.teamId), entry);
                }
                else
                {
                    var label = row.FindChild("Label", true, false) as Label;
                    if (label != null)
                        label.Text =
                            $"{i + 1}. {ResolveTeamName(entry.teamId)}  " +
                            $"P{entry.played} W{entry.wins} D{entry.draws} L{entry.losses} " +
                            $"GD{entry.goalDifference} Pts{entry.points}";
                }
            }
        }

        private void AppendTrophyRow(string text)
        {
            var row   = trophyRowPrefab.Instantiate<Control>();
            trophyContainer.AddChild(row);
            var label = row.FindChild("Label", true, false) as Label;
            if (label != null) label.Text = text;
        }

        private string ResolveTeamName(string teamId)
        {
            if (teams == null) return teamId;
            foreach (var t in teams)
                if (t.id == teamId) return t.name;
            return teamId;
        }

        private static void SetPanelVisible(Control panel, bool visible)
        {
            if (panel != null) panel.Visible = visible;
        }

        private void OnBack() => FutbolJuego.Core.SceneNavigator.Instance?.GoToDashboard();
    }
}
