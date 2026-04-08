using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// League table and fixture screen: standings rows, upcoming fixtures,
    /// and past results.
    /// </summary>
    public partial class LeagueTableUI : Control
    {
        [ExportGroup("Table")]
        [Export] public Control tableContainer;
        [Export] public PackedScene tableRowPrefab;

        [ExportGroup("Fixtures")]
        [Export] public Control fixturesContainer;
        [Export] public PackedScene fixtureRowPrefab;

        [ExportGroup("Results")]
        [Export] public Control resultsContainer;
        [Export] public PackedScene resultRowPrefab;

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>
        /// Renders the league table from sorted
        /// <see cref="LeagueData.table"/>.
        /// </summary>
        public void ShowLeagueTable(LeagueData league)
        {
            if (league == null || tableContainer == null || tableRowPrefab == null) return;

            foreach (var child in tableContainer.GetChildren())
                child.QueueFree();

            int position = 1;
            foreach (var entry in league.table)
            {
                var row   = tableRowPrefab.Instantiate<Control>();
                tableContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text =
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

            foreach (var child in fixturesContainer.GetChildren())
                child.QueueFree();

            foreach (var fixture in fixtures)
            {
                var row   = fixtureRowPrefab.Instantiate<Control>();
                fixturesContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text =
                        $"{fixture.date:dd/MM}  " +
                        $"{fixture.homeTeamId}  vs  {fixture.awayTeamId}" +
                        (string.IsNullOrEmpty(fixture.matchId) ? "" : "  ✓");
            }
        }

        /// <summary>Renders completed match result rows.</summary>
        public void ShowMatchResults(List<MatchData> results)
        {
            if (results == null || resultsContainer == null || resultRowPrefab == null) return;

            foreach (var child in resultsContainer.GetChildren())
                child.QueueFree();

            foreach (var match in results)
            {
                if (match.status != MatchStatus.Completed) continue;

                var row   = resultRowPrefab.Instantiate<Control>();
                resultsContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text =
                        $"{match.matchDate:dd/MM}  " +
                        $"{match.homeTeamId}  {match.homeScore} – {match.awayScore}  " +
                        $"{match.awayTeamId}";
            }
        }

        /// <summary>Overload that requires no data (placeholder for button wiring).</summary>
        public void ShowMatchResults()
        {
            GD.Print("[LeagueTableUI] ShowMatchResults called — pass result data from controller.");
        }
    }
}
