using System;
using System.Threading.Tasks;
using Godot;
using FutbolJuego.AI;
using FutbolJuego.Systems;
using FutbolJuego.Data;

namespace FutbolJuego.Core
{
    public partial class GameManager : Node
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public static event Action<GameStateType> OnStateChanged;
        public GameStateType CurrentState { get; private set; } = GameStateType.MainMenu;

        private GameStateMachine _stateMachine;
        private bool _isInitialized;

        public override void _Ready()
        {
            if (_instance != null && _instance != this) { QueueFree(); return; }
            _instance = this;
            InitializeServiceLocator();
            _ = InitializeSystemsAsync();
        }

        public override void _Process(double delta)
        {
            _stateMachine?.Update();
        }

        private void InitializeServiceLocator()
        {
            ServiceLocator.Register<EventBus>(new EventBus());
            ServiceLocator.Register<SaveSystem>(new SaveSystem());
        }

        private async Task InitializeSystemsAsync()
        {
            GD.Print("[GameManager] Initializing systems...");
            var dataLoader = new DataLoader();
            ServiceLocator.Register<DataLoader>(dataLoader);
            var careerSystem = new CareerSystem();
            ServiceLocator.Register<CareerSystem>(careerSystem);
            var matchEngine = new MatchSimulationEngine();
            ServiceLocator.Register<MatchSimulationEngine>(matchEngine);
            var tacticalSystem = new TacticalSystem();
            ServiceLocator.Register<TacticalSystem>(tacticalSystem);
            var transferSystem = new TransferMarketSystem();
            ServiceLocator.Register<TransferMarketSystem>(transferSystem);
            var trainingSystem = new TrainingSystem();
            ServiceLocator.Register<TrainingSystem>(trainingSystem);
            var injurySystem = new InjurySystem();
            ServiceLocator.Register<InjurySystem>(injurySystem);
            var moraleSystem = new MoraleSystem();
            ServiceLocator.Register<MoraleSystem>(moraleSystem);
            var economySystem = new EconomySystem();
            ServiceLocator.Register<EconomySystem>(economySystem);
            var playerRatingSystem = new PlayerRatingSystem();
            ServiceLocator.Register<PlayerRatingSystem>(playerRatingSystem);
            var seasonSystem = new SeasonSystem(playerRatingSystem);
            ServiceLocator.Register<SeasonSystem>(seasonSystem);
            var competitionSystem = new CompetitionSystem();
            ServiceLocator.Register<CompetitionSystem>(competitionSystem);
            var trophySystem = new TrophySystem();
            ServiceLocator.Register<TrophySystem>(trophySystem);
            var progressionSystem = new ProgressionSystem();
            ServiceLocator.Register<ProgressionSystem>(progressionSystem);
            var currencySystem = new CurrencySystem();
            ServiceLocator.Register<CurrencySystem>(currencySystem);
            var liveEventSystem = new LiveEventSystem();
            ServiceLocator.Register<LiveEventSystem>(liveEventSystem);
            var predictorSystem = new MatchPredictorSystem(matchEngine, tacticalSystem);
            ServiceLocator.Register<MatchPredictorSystem>(predictorSystem);
            var aiManager = new AIManager(AIDifficulty.Medium, tacticalSystem, transferSystem);
            ServiceLocator.Register<AIManager>(aiManager);
            await Task.Yield();
            _stateMachine = new GameStateMachine();
            _stateMachine.Initialize();
            _isInitialized = true;
            GD.Print("[GameManager] All systems initialized.");
            TransitionToState(GameStateType.MainMenu);
        }

        public void TransitionToState(GameStateType newState)
        {
            if (CurrentState == newState) return;
            GD.Print($"[GameManager] State: {CurrentState} -> {newState}");
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void GoToMainMenu() => TransitionToState(GameStateType.MainMenu);
        public void StartGame() => TransitionToState(GameStateType.Loading);
        public void EnterMatch() => TransitionToState(GameStateType.Match);

        public void TogglePause()
        {
            if (CurrentState == GameStateType.Paused) TransitionToState(GameStateType.InGame);
            else if (CurrentState == GameStateType.InGame) TransitionToState(GameStateType.Paused);
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                ServiceLocator.Clear();
                EventBus.ClearAll();
                _instance = null;
            }
        }
    }

    public enum GameStateType { MainMenu, Loading, InGame, Match, Paused }
}
