using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Team-selection screen shown at career start (and when the manager
    /// resigns to join a new club).
    ///
    /// Flow:
    ///   1. Player picks a league from the dropdown.
    ///   2. The team list for that league is displayed.
    ///   3. Player selects a team; the info panel shows budget / currency.
    ///   4. Confirm → <see cref="CareerSystem.StartCareer"/> or
    ///      <see cref="CareerSystem.JoinTeam"/> is called.
    /// </summary>
    public class TeamSelectionUI : MonoBehaviour
    {
        [Header("League selector")]
        [SerializeField] private TMP_Dropdown leagueDropdown;
        [SerializeField] private TextMeshProUGUI leagueInfoText;

        [Header("Team list")]
        [SerializeField] private Transform teamListContainer;
        [SerializeField] private GameObject teamButtonPrefab;

        [Header("Team info panel")]
        [SerializeField] private GameObject teamInfoPanel;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI teamBudgetText;
        [SerializeField] private TextMeshProUGUI teamCurrencyText;
        [SerializeField] private TextMeshProUGUI teamBalanceText;
        [SerializeField] private TextMeshProUGUI teamStadiumText;
        [SerializeField] private TextMeshProUGUI previousClubsText;

        [Header("Action")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        // ── Private state ──────────────────────────────────────────────────────

        private List<LeagueMetadata> leaguesMeta = new List<LeagueMetadata>();
        private List<LeagueData>     leagues     = new List<LeagueData>();
        private List<TeamData>       allTeams    = new List<TeamData>();
        private LeagueData           selectedLeague;
        private TeamData             selectedTeam;
        private bool                 isNewCareer = true;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (confirmButton) confirmButton.onClick.AddListener(OnConfirm);
            if (backButton)    backButton.onClick.AddListener(OnBack);
            if (leagueDropdown) leagueDropdown.onValueChanged.AddListener(OnLeagueChanged);
        }

        private void Start()
        {
            leaguesMeta = DataLoader.LoadLeagueMetadata();
            leagues     = DataLoader.LoadAllLeagues();
            allTeams    = DataLoader.LoadAllTeams();

            BuildLeagueDropdown();
            if (teamInfoPanel) teamInfoPanel.SetActive(false);
            if (confirmButton) confirmButton.interactable = false;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Call with <c>false</c> when showing this screen after a resignation;
        /// the back button will return to the dashboard instead of closing.
        /// </summary>
        public void SetIsNewCareer(bool newCareer) => isNewCareer = newCareer;

        // ── League ─────────────────────────────────────────────────────────────

        private void BuildLeagueDropdown()
        {
            if (leagueDropdown == null) return;
            leagueDropdown.ClearOptions();
            var names = leaguesMeta.Select(l => l.name).ToList();
            leagueDropdown.AddOptions(names);
            OnLeagueChanged(0);
        }

        private void OnLeagueChanged(int index)
        {
            if (leaguesMeta == null || index < 0 || index >= leaguesMeta.Count) return;

            var meta   = leaguesMeta[index];
            selectedLeague = leagues.Find(l => l.id == meta.id);

            if (leagueInfoText)
                leagueInfoText.text =
                    $"{meta.name}  |  {meta.country}  |  " +
                    $"Dificultad: {meta.difficulty}/5  |  Rep: {meta.reputation}/100\n" +
                    $"Estilo: {meta.playStyle}  |  Presupuesto promedio: {meta.averageBudget:N0}";

            BuildTeamList(meta.id);
            selectedTeam = null;
            if (teamInfoPanel) teamInfoPanel.SetActive(false);
            if (confirmButton) confirmButton.interactable = false;
        }

        // ── Team list ──────────────────────────────────────────────────────────

        private void BuildTeamList(string leagueId)
        {
            if (teamListContainer == null || teamButtonPrefab == null) return;

            foreach (Transform child in teamListContainer)
                Destroy(child.gameObject);

            // Filter teams belonging to this league by country + leagueId conventions
            // (teams.json uses leagueId in the id prefix convention)
            var teamsInLeague = allTeams.Where(t => TeamBelongsToLeague(t, leagueId)).ToList();

            foreach (var team in teamsInLeague)
            {
                var btn  = Instantiate(teamButtonPrefab, teamListContainer);
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = team.name;

                var button  = btn.GetComponent<Button>() ?? btn.GetComponentInChildren<Button>();
                var capture = team;
                if (button) button.onClick.AddListener(() => SelectTeam(capture, leagueId));
            }
        }

        private void SelectTeam(TeamData team, string leagueId)
        {
            selectedTeam = team;

            var careerSystem = ServiceLocator.Get<CareerSystem>();
            var (currency, budget) = careerSystem?.GetLeagueBudgetDefaults(leagueId)
                ?? (CurrencyType.EUR, 30_000_000L);

            long effective = careerSystem?.CalculateStartingBudget(team, leagueId) ?? budget;
            string symbol  = currency == CurrencyType.EUR ? "€" : "$";

            if (teamInfoPanel)   teamInfoPanel.SetActive(true);
            if (teamNameText)    teamNameText.text    = team.name;
            if (teamBudgetText)  teamBudgetText.text  = $"Presupuesto inicial: {symbol} {effective:N0}";
            if (teamCurrencyText)teamCurrencyText.text= $"Moneda: {currency}";
            if (teamBalanceText) teamBalanceText.text =
                $"Balance actual del club: {symbol} {team.finances?.balance:N0}";
            if (teamStadiumText) teamStadiumText.text =
                $"Estadio: {team.infrastructure?.stadium?.name ?? "—"}  " +
                $"(cap. {team.infrastructure?.stadium?.capacity:N0})";

            // Show any previous clubs for context
            var active = careerSystem?.ActiveCareer;
            if (previousClubsText)
            {
                if (active != null && active.previousTeamIds?.Count > 0)
                {
                    string prev = string.Join(", ", active.previousTeamIds);
                    previousClubsText.text = $"Anteriores clubes: {prev}";
                }
                else
                {
                    previousClubsText.text = string.Empty;
                }
            }

            if (confirmButton) confirmButton.interactable = true;
            SetFeedback(string.Empty);
        }

        // ── Confirm ────────────────────────────────────────────────────────────

        private void OnConfirm()
        {
            if (selectedTeam == null || selectedLeague == null)
            {
                SetFeedback("Selecciona una liga y un equipo.");
                return;
            }

            var careerSystem = ServiceLocator.Get<CareerSystem>();
            if (careerSystem == null)
            {
                SetFeedback("Sistema de carrera no disponible.");
                return;
            }

            if (isNewCareer || careerSystem.ActiveCareer == null)
                careerSystem.StartCareer(selectedLeague, selectedTeam);
            else
                careerSystem.JoinTeam(selectedLeague, selectedTeam);

            SetFeedback($"¡Bienvenido a {selectedTeam.name}!");

            // Navigate to dashboard after a brief moment
            SceneNavigator.Instance?.GoToDashboard();
        }

        // ── Navigation ─────────────────────────────────────────────────────────

        private void OnBack()
        {
            if (isNewCareer)
                SceneNavigator.Instance?.GoToMainMenu();
            else
                SceneNavigator.Instance?.GoToDashboard();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetFeedback(string msg)
        {
            if (feedbackText) feedbackText.text = msg;
        }

        /// <summary>
        /// Determines whether a team belongs to a league based on the team id
        /// prefix convention (e.g. "team-america" → "league-liga-mx" via
        /// country code stored on the team).
        /// </summary>
        private static bool TeamBelongsToLeague(TeamData team, string leagueId)
        {
            // Convention: team.country maps to league via this lookup table.
            return leagueId switch
            {
                "league-liga-mx"     => team.country == "MX",
                "league-brasileirao" => team.country == "BR",
                "league-laliga"      => team.country == "ES",
                "league-premier"     => team.country == "GB",
                "league-seriea"      => team.country == "IT",
                _                    => false
            };
        }
    }
}
