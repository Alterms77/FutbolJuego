using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Competitions scene controller.  Displays the league standings table
    /// and current matchday information for all loaded leagues.
    /// </summary>
    public class CompetitionsUI : MonoBehaviour
    {
        [Header("League Selector")]
        [SerializeField] private TMP_Dropdown leagueDropdown;

        [Header("League Info")]
        [SerializeField] private TextMeshProUGUI leagueNameText;
        [SerializeField] private TextMeshProUGUI matchdayText;

        [Header("Table")]
        [SerializeField] private Transform tableContainer;
        [SerializeField] private GameObject tableRowPrefab;

        private List<LeagueData> leagues = new List<LeagueData>();
        private List<TeamData>   teams   = new List<TeamData>();
        private int selectedLeagueIndex  = 0;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Start()
        {
            teams   = DataLoader.LoadAllTeams();
            leagues = DataLoader.LoadAllLeagues();

            if (leagueDropdown != null)
            {
                leagueDropdown.ClearOptions();
                var options = new List<string>();
                foreach (var l in leagues)
                    options.Add(l.name);
                leagueDropdown.AddOptions(options);
                leagueDropdown.onValueChanged.AddListener(OnLeagueSelected);
            }

            ShowLeague(0);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Shows the standings for league at <paramref name="index"/>.</summary>
        public void ShowLeague(int index)
        {
            if (leagues == null || index < 0 || index >= leagues.Count) return;

            selectedLeagueIndex = index;
            var league = leagues[index];

            if (leagueNameText)  leagueNameText.text  = league.name;
            if (matchdayText)    matchdayText.text     = $"Matchday {league.currentMatchday}";

            BuildTable(league);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void OnLeagueSelected(int index) => ShowLeague(index);

        private void BuildTable(LeagueData league)
        {
            if (tableContainer == null || tableRowPrefab == null) return;

            foreach (Transform child in tableContainer)
                Destroy(child.gameObject);

            var table = league.table;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                var row   = Instantiate(tableRowPrefab, tableContainer);

                // Try to use the LeagueTableRow component for rich display
                var rowComp = row.GetComponent<LeagueTableRow>();
                if (rowComp != null)
                {
                    rowComp.Setup(i + 1, ResolveTeamName(entry.teamId), entry);
                }
                else
                {
                    // Fallback: single TextMeshPro label
                    var label = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (label)
                        label.text =
                            $"{i + 1}. {ResolveTeamName(entry.teamId)}  " +
                            $"P{entry.played} W{entry.wins} D{entry.draws} L{entry.losses} " +
                            $"GD{entry.goalDifference} Pts{entry.points}";
                }
            }
        }

        private string ResolveTeamName(string teamId)
        {
            if (teams == null) return teamId;
            foreach (var t in teams)
                if (t.id == teamId) return t.name;
            return teamId;
        }
    }
}
