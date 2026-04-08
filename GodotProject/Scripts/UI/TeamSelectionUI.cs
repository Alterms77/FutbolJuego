using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    public partial class TeamSelectionUI : Control
    {
        [ExportGroup("League selector")]
        [Export] public OptionButton leagueDropdown;
        [Export] public Label leagueInfoText;

        [ExportGroup("Team list")]
        [Export] public Control teamListContainer;
        [Export] public PackedScene teamButtonPrefab;

        [ExportGroup("Team info panel")]
        [Export] public Control teamInfoPanel;
        [Export] public Label teamNameText;
        [Export] public Label teamBudgetText;
        [Export] public Label teamCurrencyText;
        [Export] public Label teamBalanceText;
        [Export] public Label teamStadiumText;
        [Export] public Label previousClubsText;

        [ExportGroup("Action")]
        [Export] public Button confirmButton;
        [Export] public Button backButton;

        [ExportGroup("Feedback")]
        [Export] public Label feedbackText;

        private List<LeagueMetadata> leaguesMeta = new List<LeagueMetadata>();
        private List<LeagueData>     leagues     = new List<LeagueData>();
        private List<TeamData>       allTeams    = new List<TeamData>();
        private LeagueData           selectedLeague;
        private TeamData             selectedTeam;
        private bool                 isNewCareer = true;

        public override void _Ready()
        {
            if (confirmButton != null) confirmButton.Pressed += OnConfirm;
            if (backButton != null)    backButton.Pressed    += OnBack;
            if (leagueDropdown != null) leagueDropdown.ItemSelected += (idx) => OnLeagueChanged((int)idx);

            leaguesMeta = DataLoader.LoadLeagueMetadata();
            leagues     = DataLoader.LoadAllLeagues();
            allTeams    = DataLoader.LoadAllTeams();

            BuildLeagueDropdown();
            if (teamInfoPanel != null) teamInfoPanel.Visible = false;
            if (confirmButton != null) confirmButton.Disabled = true;
        }

        public void SetIsNewCareer(bool newCareer) => isNewCareer = newCareer;

        private void BuildLeagueDropdown()
        {
            if (leagueDropdown == null) return;
            leagueDropdown.Clear();
            foreach (var l in leaguesMeta) leagueDropdown.AddItem(l.name);
            OnLeagueChanged(0);
        }

        private void OnLeagueChanged(int index)
        {
            if (leaguesMeta == null || index < 0 || index >= leaguesMeta.Count) return;
            var meta = leaguesMeta[index];
            selectedLeague = leagues.Find(l => l.id == meta.id);

            if (leagueInfoText != null)
                leagueInfoText.Text =
                    $"{meta.name}  |  {meta.country}  |  " +
                    $"Dificultad: {meta.difficulty}/5  |  Rep: {meta.reputation}/100\n" +
                    $"Estilo: {meta.playStyle}  |  Presupuesto promedio: {meta.averageBudget:N0}";

            BuildTeamList(meta.id);
            selectedTeam = null;
            if (teamInfoPanel != null) teamInfoPanel.Visible = false;
            if (confirmButton != null) confirmButton.Disabled = true;
        }

        private void BuildTeamList(string leagueId)
        {
            if (teamListContainer == null || teamButtonPrefab == null) return;
            foreach (var child in teamListContainer.GetChildren()) child.QueueFree();

            var teamsInLeague = allTeams.Where(t => TeamBelongsToLeague(t, leagueId)).ToList();
            foreach (var team in teamsInLeague)
            {
                var btn  = teamButtonPrefab.Instantiate<Control>();
                teamListContainer.AddChild(btn);
                var label = btn.FindChild("Label", true, false) as Label;
                if (label != null) label.Text = team.name;

                var button  = btn as Button ?? btn.FindChild("Button", true, false) as Button;
                var capture = team;
                if (button != null) button.Pressed += () => SelectTeam(capture, leagueId);
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

            if (teamInfoPanel != null)   teamInfoPanel.Visible   = true;
            if (teamNameText != null)    teamNameText.Text    = team.name;
            if (teamBudgetText != null)  teamBudgetText.Text  = $"Presupuesto inicial: {symbol} {effective:N0}";
            if (teamCurrencyText != null)teamCurrencyText.Text= $"Moneda: {currency}";
            if (teamBalanceText != null) teamBalanceText.Text =
                $"Balance actual del club: {symbol} {team.finances?.balance:N0}";
            if (teamStadiumText != null) teamStadiumText.Text =
                $"Estadio: {team.infrastructure?.stadium?.name ?? "—"}  " +
                $"(cap. {team.infrastructure?.stadium?.capacity:N0})";

            var active = careerSystem?.ActiveCareer;
            if (previousClubsText != null)
            {
                if (active != null && active.previousTeamIds?.Count > 0)
                    previousClubsText.Text = $"Anteriores clubes: {string.Join(", ", active.previousTeamIds)}";
                else
                    previousClubsText.Text = string.Empty;
            }

            if (confirmButton != null) confirmButton.Disabled = false;
            SetFeedback(string.Empty);
        }

        private void OnConfirm()
        {
            if (selectedTeam == null || selectedLeague == null)
            {
                SetFeedback("Selecciona una liga y un equipo.");
                return;
            }

            var careerSystem = ServiceLocator.Get<CareerSystem>();
            if (careerSystem == null) { SetFeedback("Sistema de carrera no disponible."); return; }

            if (isNewCareer || careerSystem.ActiveCareer == null)
                careerSystem.StartCareer(selectedLeague, selectedTeam);
            else
                careerSystem.JoinTeam(selectedLeague, selectedTeam);

            SetFeedback($"¡Bienvenido a {selectedTeam.name}!");
            SceneNavigator.Instance?.GoToDashboard();
        }

        private void OnBack()
        {
            if (isNewCareer) SceneNavigator.Instance?.GoToMainMenu();
            else             SceneNavigator.Instance?.GoToDashboard();
        }

        private void SetFeedback(string msg) { if (feedbackText != null) feedbackText.Text = msg; }

        private static bool TeamBelongsToLeague(TeamData team, string leagueId) => leagueId switch
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
