using Godot;
using FutbolJuego.Core;

namespace FutbolJuego.UI.Components
{
    public partial class NavigationBar : Control
    {
        [ExportGroup("Nav Buttons")]
        [Export] public Button dashboardButton;
        [Export] public Button squadButton;
        [Export] public Button tacticsButton;
        [Export] public Button transferMarketButton;
        [Export] public Button financesButton;
        [Export] public Button competitionsButton;
        [Export] public Button shopButton;

        public override void _Ready()
        {
            if (dashboardButton != null)      dashboardButton.Pressed      += OnDashboard;
            if (squadButton != null)          squadButton.Pressed          += OnSquad;
            if (tacticsButton != null)        tacticsButton.Pressed        += OnTactics;
            if (transferMarketButton != null) transferMarketButton.Pressed += OnTransferMarket;
            if (financesButton != null)       financesButton.Pressed       += OnFinances;
            if (competitionsButton != null)   competitionsButton.Pressed   += OnCompetitions;
            if (shopButton != null)           shopButton.Pressed           += OnShop;
        }

        private void OnDashboard()      => SceneNavigator.Instance?.GoToDashboard();
        private void OnSquad()          => SceneNavigator.Instance?.GoToSquad();
        private void OnTactics()        => SceneNavigator.Instance?.GoToTactics();
        private void OnTransferMarket() => SceneNavigator.Instance?.GoToTransferMarket();
        private void OnFinances()       => SceneNavigator.Instance?.GoToFinances();
        private void OnCompetitions()   => SceneNavigator.Instance?.GoToCompetitions();
        private void OnShop()           => SceneNavigator.Instance?.GoToShop();
    }
}
