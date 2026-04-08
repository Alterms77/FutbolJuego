using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Core;
using FutbolJuego.Data;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Dashboard scene controller.  Shows a summary of the player's club:
    /// team name, league, position, budget, next opponent, and overall rating.
    /// Also provides navigation buttons to all other scenes.
    /// </summary>
    public class DashboardUI : MonoBehaviour
    {
        [Header("Team Info")]
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI leagueText;
        [SerializeField] private TextMeshProUGUI leaguePositionText;
        [SerializeField] private TextMeshProUGUI budgetText;
        [SerializeField] private TextMeshProUGUI overallRatingText;

        [Header("Next Match")]
        [SerializeField] private TextMeshProUGUI nextMatchText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button playMatchButton;
        [SerializeField] private Button squadButton;
        [SerializeField] private Button tacticsButton;
        [SerializeField] private Button marketButton;
        [SerializeField] private Button financesButton;
        [SerializeField] private Button competitionsButton;
        [SerializeField] private Button shopButton;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (playMatchButton)     playMatchButton.onClick.AddListener(OnPlayMatch);
            if (squadButton)         squadButton.onClick.AddListener(OnSquad);
            if (tacticsButton)       tacticsButton.onClick.AddListener(OnTactics);
            if (marketButton)        marketButton.onClick.AddListener(OnMarket);
            if (financesButton)      financesButton.onClick.AddListener(OnFinances);
            if (competitionsButton)  competitionsButton.onClick.AddListener(OnCompetitions);
            if (shopButton)          shopButton.onClick.AddListener(OnShop);
        }

        private void Start()
        {
            Refresh();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Refreshes all dashboard labels from the current game state.</summary>
        public void Refresh()
        {
            var teams = DataLoader.LoadAllTeams();
            if (teams == null || teams.Count == 0) return;

            // Use the first team as the player's team for now.
            // TODO: replace with GameManager.Instance.PlayerTeam once wired.
            TeamData team = teams[0];

            if (teamNameText)
                teamNameText.text = team.name;

            if (overallRatingText)
                overallRatingText.text = $"OVR  {Mathf.RoundToInt(team.GetAverageSquadRating())}";

            if (budgetText && team.finances != null)
                budgetText.text = FormatCurrency(team.finances.transferBudget);

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

            if (leagueText)
                leagueText.text = myLeague?.name ?? "—";

            if (leaguePositionText)
                leaguePositionText.text = $"#{team.leaguePosition}";

            // Show next fixture if available
            if (nextMatchText && myLeague != null)
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
                    nextMatchText.text =
                        $"MD {myLeague.currentMatchday} — {venue} {opponentName}\n" +
                        $"{teamFixture.date:ddd dd MMM}";
                }
                else
                {
                    nextMatchText.text = "No upcoming fixtures";
                }
            }
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private void OnPlayMatch()      => SceneNavigator.Instance?.GoToMatch();
        private void OnSquad()          => SceneNavigator.Instance?.GoToSquad();
        private void OnTactics()        => SceneNavigator.Instance?.GoToTactics();
        private void OnMarket()         => SceneNavigator.Instance?.GoToTransferMarket();
        private void OnFinances()       => SceneNavigator.Instance?.GoToFinances();
        private void OnCompetitions()   => SceneNavigator.Instance?.GoToCompetitions();
        private void OnShop()           => SceneNavigator.Instance?.GoToShop();

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string FormatCurrency(long value)
        {
            if (value >= 1_000_000) return $"€{value / 1_000_000f:F1}M";
            if (value >= 1_000)     return $"€{value / 1_000f:F0}K";
            return $"€{value}";
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
