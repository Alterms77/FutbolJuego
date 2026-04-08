using Godot;
using FutbolJuego.Core;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Controls the main-menu screen: button callbacks, game-state subscriptions,
    /// and animated transitions.
    /// </summary>
    public partial class MainMenuUI : Control
    {
        [ExportGroup("Panels")]
        [Export] public Control mainPanel;
        [Export] public Control settingsPanel;
        [Export] public Control loadPanel;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            GameManager.OnStateChanged += HandleStateChanged;
        }

        public override void _ExitTree()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        // ── Button callbacks ───────────────────────────────────────────────────

        /// <summary>Starts a new game — navigates to team selection.</summary>
        public void OnPlayButton()
        {
            GameManager.Instance?.StartGame();
            SceneNavigator.Instance?.GoToTeamSelection();
        }

        /// <summary>Continues an existing career — navigates to dashboard.</summary>
        public void OnContinueButton()
        {
            SceneNavigator.Instance?.GoToDashboard();
        }

        /// <summary>Opens the settings overlay.</summary>
        public void OnSettingsButton()
        {
            SetPanelActive(settingsPanel);
        }

        /// <summary>Opens the load-game overlay.</summary>
        public void OnLoadGameButton()
        {
            SetPanelActive(loadPanel);
        }

        /// <summary>Returns to the main panel from any overlay.</summary>
        public void OnBackButton()
        {
            SetPanelActive(mainPanel);
        }

        // ── State handler ──────────────────────────────────────────────────────

        private void HandleStateChanged(GameStateType state)
        {
            Visible = state == GameStateType.MainMenu;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetPanelActive(Control target)
        {
            if (mainPanel != null)     mainPanel.Visible     = target == mainPanel;
            if (settingsPanel != null) settingsPanel.Visible = target == settingsPanel;
            if (loadPanel != null)     loadPanel.Visible     = target == loadPanel;
        }
    }
}
