using Godot;

namespace FutbolJuego.Core
{
    public partial class SceneNavigator : Node
    {
        private static SceneNavigator _instance;
        public static SceneNavigator Instance => _instance;

        public override void _Ready()
        {
            if (_instance != null && _instance != this) { QueueFree(); return; }
            _instance = this;
        }

        public void LoadScene(string sceneName)
        {
            GD.Print($"[SceneNavigator] Loading scene: {sceneName}");
            GetTree().ChangeSceneToFile($"res://Scenes/{sceneName}.tscn");
        }

        public void GoToMainMenu() => LoadScene("MainMenu");
        public void GoToDashboard() => LoadScene("Dashboard");
        public void GoToSquad() => LoadScene("Squad");
        public void GoToTactics() => LoadScene("Tactics");
        public void GoToMatch() => LoadScene("Match");
        public void GoToTransferMarket() => LoadScene("TransferMarket");
        public void GoToFinances() => LoadScene("Finances");
        public void GoToCompetitions() => LoadScene("Competitions");
        public void GoToShop() => LoadScene("Shop");
        public void GoToLegends() => LoadScene("Legends");
        public void GoToTeamSelection() => LoadScene("TeamSelection");
        public void GoToTraining() => LoadScene("Training");

        public override void _ExitTree()
        {
            if (_instance == this) _instance = null;
        }
    }
}
