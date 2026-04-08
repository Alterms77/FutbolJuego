using System.Collections.Generic;
using Godot;
using FutbolJuego.Core;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Dashboard scene controller.  Shows a summary of the player's club:
    /// team name, league, position, budget, next opponent, and overall rating.
    /// Also provides navigation buttons to all other scenes.
    /// </summary>
    public partial class DashboardUI : Control
    {
        [ExportGroup("Team Info")]
        [Export] public Label teamNameText;
        [Export] public Label leagueText;
        [Export] public Label leaguePositionText;
        [Export] public Label budgetText;
        [Export] public Label overallRatingText;

        [ExportGroup("Next Match")]
        [Export] public Label nextMatchText;

        [ExportGroup("Navigation Buttons")]
        [Export] public Button playMatchButton;
        [Export] public Button squadButton;
        [Export] public Button tacticsButton;
        [Export] public Button marketButton;
        [Export] public Button financesButton;
        [Export] public Button competitionsButton;
        [Export] public Button shopButton;
        [Export] public Button legendsButton;
        [Export] public Button resignButton;

        [ExportGroup("Career Info")]
        [Export] public Label careerBalanceText;
        [Export] public Label premiumCoinsText;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (playMatchButton != null)    playMatchButton.Pressed    += OnPlayMatch;
            if (squadButton != null)        squadButton.Pressed        += OnSquad;
            if (tacticsButton != null)      tacticsButton.Pressed      += OnTactics;
            if (marketButton != null)       marketButton.Pressed       += OnMarket;
            if (financesButton != null)     financesButton.Pressed     += OnFinances;
            if (competitionsButton != null) competitionsButton.Pressed += OnCompetitions;
            if (shopButton != null)         shopButton.Pressed         += OnShop;
            if (legendsButton != null)      legendsButton.Pressed      += OnLegends;
            if (resignButton != null)       resignButton.Pressed       += OnResign;

            Refresh();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Refreshes all dashboard labels from the current game state.</summary>
        public void Refresh()
        {
            var teams = DataLoader.LoadAllTeams();
            if (teams == null || teams.Count == 0) return;

            TeamData team = teams[0];

            if (teamNameText != null)
                teamNameText.Text = team.name;

            if (overallRatingText != null)
                overallRatingText.Text = $"OVR  {Mathf.RoundToInt(team.GetAverageSquadRating())}";

            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            string currSymbol = career?.CurrencySymbol ?? "€";

            if (budgetText != null && team.finances != null)
                budgetText.Text = FormatCurrency(team.finances.transferBudget, currSymbol);

            if (careerBalanceText != null)
                careerBalanceText.Text = career != null
                    ? career.FormattedBalance
                    : FormatCurrency(team.finances?.transferBudget ?? 0, currSymbol);
            if (premiumCoinsText != null)
                premiumCoinsText.Text = career != null ? $"🪙 {career.premiumCoins}" : "🪙 —";

            var leagues = DataLoader.LoadAllLeagues();
            LeagueData myLeague = null;
            foreach (var l in leagues)
            {
                if (l.teamIds != null && l.teamIds.Contains(team.id))
                {
                    myLeague = l;
                    break;
                }
            }

            if (leagueText != null)
                leagueText.Text = myLeague?.name ?? "—";

            if (leaguePositionText != null)
                leaguePositionText.Text = $"#{team.leaguePosition}";

            if (nextMatchText != null && myLeague != null)
            {
                var nextFixture = myLeague.GetMatchdayFixtures(myLeague.currentMatchday);
                FixtureData teamFixture = null;
                foreach (var f in nextFixture)
                {
                    if (f.homeTeamId == team.id || f.awayTeamId == team.id)
                    {
                        teamFixture = f;
                        break;
                    }
                }

                if (teamFixture != null)
                {
                    bool isHome = teamFixture.homeTeamId == team.id;
                    string opponentId = isHome ? teamFixture.awayTeamId : teamFixture.homeTeamId;
                    string opponentName = ResolveTeamName(teams, opponentId);
                    string venue = isHome ? "vs" : "@";
                    nextMatchText.Text =
                        $"MD {myLeague.currentMatchday} — {venue} {opponentName}\n" +
                        $"{teamFixture.date:ddd dd MMM}";
                }
                else
                {
                    nextMatchText.Text = "No upcoming fixtures";
                }
            }
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private void OnPlayMatch()    => SceneNavigator.Instance?.GoToMatch();
        private void OnSquad()        => SceneNavigator.Instance?.GoToSquad();
        private void OnTactics()      => SceneNavigator.Instance?.GoToTactics();
        private void OnMarket()       => SceneNavigator.Instance?.GoToTransferMarket();
        private void OnFinances()     => SceneNavigator.Instance?.GoToFinances();
        private void OnCompetitions() => SceneNavigator.Instance?.GoToCompetitions();
        private void OnShop()         => SceneNavigator.Instance?.GoToShop();
        private void OnLegends()      => SceneNavigator.Instance?.GoToLegends();

        private void OnResign()
        {
            var careerSystem = ServiceLocator.Get<CareerSystem>();
            careerSystem?.ResignFromTeam();
            SceneNavigator.Instance?.GoToTeamSelection();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string FormatCurrency(long value, string symbol = "€")
        {
            if (value >= 1_000_000) return $"{symbol}{value / 1_000_000f:F1}M";
            if (value >= 1_000)     return $"{symbol}{value / 1_000f:F0}K";
            return $"{symbol}{value}";
        }

        private static string ResolveTeamName(List<TeamData> teams, string teamId)
        {
            if (teams == null) return teamId;
            foreach (var t in teams)
                if (t.id == teamId) return t.name;
            return teamId;
        }
    }
}
