using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public class CompetitionsUI : MonoBehaviour
    {
        [Header("League Selector")]
        [SerializeField] private TMP_Dropdown leagueDropdown;

        [Header("League Info")]
        [SerializeField] private TextMeshProUGUI leagueNameText;
        [SerializeField] private TextMeshProUGUI matchdayText;

        [Header("League Table")]
        [SerializeField] private Transform tableContainer;
        [SerializeField] private GameObject tableRowPrefab;

        [Header("Cups Panel")]
        [SerializeField] private GameObject cupsPanel;
        [SerializeField] private Transform  cupListContainer;
        [SerializeField] private GameObject cupRowPrefab;

        [Header("Trophies Panel")]
        [SerializeField] private GameObject trophiesPanel;
        [SerializeField] private Transform  trophyContainer;
        [SerializeField] private GameObject trophyRowPrefab;
        [SerializeField] private TMP_InputField trophyTeamInput;
        [SerializeField] private Button         trophySearchButton;

        [Header("Tab Buttons")]
        [SerializeField] private Button tabLeagueButton;
        [SerializeField] private Button tabCupsButton;
        [SerializeField] private Button tabTrophiesButton;

        // ── Private state ──────────────────────────────────────────────────────

        private List<LeagueData> leagues = new List<LeagueData>();
        private List<TeamData>   teams   = new List<TeamData>();
        private List<CupData>    cups    = new List<CupData>();
        private int selectedLeagueIndex  = 0;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Start()
        {
            teams   = DataLoader.LoadAllTeams();
            leagues = DataLoader.LoadAllLeagues();
            cups    = DataLoader.LoadAllCups();

            if (leagueDropdown != null)
            {
                leagueDropdown.ClearOptions();
                var options = new List<string>();
                foreach (var l in leagues) options.Add(l.name);
                leagueDropdown.AddOptions(options);
                leagueDropdown.onValueChanged.AddListener(OnLeagueSelected);
            }

            if (tabLeagueButton)  tabLeagueButton.onClick.AddListener(ShowLeagueTab);
            if (tabCupsButton)    tabCupsButton.onClick.AddListener(ShowCupsTab);
            if (tabTrophiesButton)tabTrophiesButton.onClick.AddListener(ShowTrophiesTab);
            if (trophySearchButton) trophySearchButton.onClick.AddListener(OnTrophySearch);

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
            BuildTrophyList(string.Empty); // show all by default
        }

        // ── League ────────────────────────────────────────────────────────────

        /// <summary>Shows the standings for the league at <paramref name="index"/>.</summary>
        public void ShowLeague(int index)
        {
            if (leagues == null || index < 0 || index >= leagues.Count) return;
            selectedLeagueIndex = index;
            var league = leagues[index];

            if (leagueNameText) leagueNameText.text = league.name;
            if (matchdayText)   matchdayText.text   = $"Matchday {league.currentMatchday}";

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

            foreach (Transform child in cupListContainer) Destroy(child.gameObject);

            if (leagues == null || leagueIndex < 0 || leagueIndex >= leagues.Count) return;
            string leagueId = leagues[leagueIndex].id;

            foreach (var cup in cups)
            {
                if (cup.leagueId != leagueId) continue;

                var row   = Instantiate(cupRowPrefab, cupListContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label == null) continue;

                string status = string.IsNullOrEmpty(cup.winnerId)
                    ? $"{cup.currentRoundName} (Round {cup.currentRound})"
                    : $"WON by {ResolveTeamName(cup.winnerId)}";

                label.text = $"🏆 {cup.name}  –  {status}";

                // Fixtures sub-list
                foreach (var fixture in cup.currentRoundFixtures)
                {
                    var fRow   = Instantiate(cupRowPrefab, cupListContainer);
                    var fLabel = fRow.GetComponentInChildren<TextMeshProUGUI>();
                    if (fLabel != null)
                        fLabel.text = $"   {ResolveTeamName(fixture.homeTeamId)} vs " +
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
            foreach (Transform child in trophyContainer) Destroy(child.gameObject);

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
                var row   = Instantiate(trophyRowPrefab, trophyContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"🏅 {trophy.name}  ({ResolveTeamName(trophy.teamId)})";
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
            string query = trophyTeamInput != null ? trophyTeamInput.text.Trim() : string.Empty;
            // Try to match by name first, fall back to ID
            var found = teams?.Find(t =>
                t.name.ToLower().Contains(query.ToLower()) ||
                t.id.ToLower().Contains(query.ToLower()));

            BuildTrophyList(found?.id ?? query);
        }

        private void BuildTable(LeagueData league)
        {
            if (tableContainer == null || tableRowPrefab == null) return;
            foreach (Transform child in tableContainer) Destroy(child.gameObject);

            var table = league.table;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                var row   = Instantiate(tableRowPrefab, tableContainer);

                var rowComp = row.GetComponent<LeagueTableRow>();
                if (rowComp != null)
                {
                    rowComp.Setup(i + 1, ResolveTeamName(entry.teamId), entry);
                }
                else
                {
                    var label = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (label)
                        label.text =
                            $"{i + 1}. {ResolveTeamName(entry.teamId)}  " +
                            $"P{entry.played} W{entry.wins} D{entry.draws} L{entry.losses} " +
                            $"GD{entry.goalDifference} Pts{entry.points}";
                }
            }
        }

        private void AppendTrophyRow(string text)
        {
            var row   = Instantiate(trophyRowPrefab, trophyContainer);
            var label = row.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = text;
        }

        private string ResolveTeamName(string teamId)
        {
            if (teams == null) return teamId;
            foreach (var t in teams)
                if (t.id == teamId) return t.name;
            return teamId;
        }

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel) panel.SetActive(visible);
        }
    }
}
