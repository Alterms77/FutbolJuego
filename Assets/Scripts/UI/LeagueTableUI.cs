using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// League table and fixture screen: standings rows, upcoming fixtures,
    /// and past results.
    /// </summary>
    public class LeagueTableUI : MonoBehaviour
    {
        [Header("Table")]
        [SerializeField] private Transform tableContainer;
        [SerializeField] private GameObject tableRowPrefab;

        [Header("Fixtures")]
        [SerializeField] private Transform fixturesContainer;
        [SerializeField] private GameObject fixtureRowPrefab;

        [Header("Results")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private GameObject resultRowPrefab;

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>
        /// Renders the league table from sorted
        /// <see cref="LeagueData.table"/>.
        /// </summary>
        public void ShowLeagueTable(LeagueData league)
        {
            if (league == null || tableContainer == null || tableRowPrefab == null) return;

            foreach (Transform child in tableContainer)
                Destroy(child.gameObject);

            int position = 1;
            foreach (var entry in league.table)
            {
                var row   = Instantiate(tableRowPrefab, tableContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text =
                        $"{position++,2}. {entry.teamId,-20} " +
                        $"P{entry.played,2} " +
                        $"W{entry.wins,2} D{entry.draws,2} L{entry.losses,2} " +
                        $"GD{entry.goalDifference,+4} " +
                        $"Pts {entry.points,3}";
            }
        }

        /// <summary>Renders upcoming fixture rows.</summary>
        public void ShowFixtures(List<FixtureData> fixtures)
        {
            if (fixtures == null || fixturesContainer == null || fixtureRowPrefab == null) return;

            foreach (Transform child in fixturesContainer)
                Destroy(child.gameObject);

            foreach (var fixture in fixtures)
            {
                var row   = Instantiate(fixtureRowPrefab, fixturesContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text =
                        $"{fixture.date:dd/MM}  " +
                        $"{fixture.homeTeamId}  vs  {fixture.awayTeamId}" +
                        (string.IsNullOrEmpty(fixture.matchId) ? "" : "  ✓");
            }
        }

        /// <summary>Renders completed match result rows.</summary>
        public void ShowMatchResults(List<MatchData> results)
        {
            if (results == null || resultsContainer == null || resultRowPrefab == null) return;

            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);

            foreach (var match in results)
            {
                if (match.status != MatchStatus.Completed) continue;

                var row   = Instantiate(resultRowPrefab, resultsContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text =
                        $"{match.matchDate:dd/MM}  " +
                        $"{match.homeTeamId}  {match.homeScore} – {match.awayScore}  " +
                        $"{match.awayTeamId}";
            }
        }

        /// <summary>Overload that requires no data (placeholder for button wiring).</summary>
        public void ShowMatchResults()
        {
            Debug.Log("[LeagueTableUI] ShowMatchResults called — pass result data from controller.");
        }
    }
}
