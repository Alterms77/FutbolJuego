using System;
using System.Collections;
using UnityEngine;
using FutbolJuego.Systems;
using FutbolJuego.Data;

namespace FutbolJuego.Core
{
    /// <summary>
    /// Central singleton that owns the service locator, state machine, and system
    /// initialization sequence for the entire game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static GameManager instance;

        /// <summary>Gets the active GameManager instance.</summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogError("[GameManager] No instance found in scene.");
                return instance;
            }
        }

        // ── Public state ───────────────────────────────────────────────────────
        /// <summary>Raised whenever the game transitions to a new state.</summary>
        public static event Action<GameStateType> OnStateChanged;

        /// <summary>Current high-level game state.</summary>
        public GameStateType CurrentState { get; private set; } = GameStateType.MainMenu;

        // ── Private fields ─────────────────────────────────────────────────────
        private GameStateMachine stateMachine;
        private bool isInitialized;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServiceLocator();
        }

        private IEnumerator Start()
        {
            yield return StartCoroutine(InitializeSystems());
        }

        private void Update()
        {
            stateMachine?.Update();
        }

        // ── Initialization ─────────────────────────────────────────────────────

        /// <summary>Registers core services with the ServiceLocator.</summary>
        private void InitializeServiceLocator()
        {
            ServiceLocator.Register<EventBus>(new EventBus());
            ServiceLocator.Register<SaveSystem>(new SaveSystem());
        }

        /// <summary>Sequential system boot — yields one frame between each step.</summary>
        private IEnumerator InitializeSystems()
        {
            Debug.Log("[GameManager] Initializing systems…");

            // Data
            var dataLoader = new DataLoader();
            ServiceLocator.Register<DataLoader>(dataLoader);
            yield return null;

            // Gameplay systems
            var matchEngine = new MatchSimulationEngine();
            ServiceLocator.Register<MatchSimulationEngine>(matchEngine);

            var tacticalSystem = new TacticalSystem();
            ServiceLocator.Register<TacticalSystem>(tacticalSystem);

            var economySystem = new EconomySystem();
            ServiceLocator.Register<EconomySystem>(economySystem);

            var transferSystem = new TransferMarketSystem();
            ServiceLocator.Register<TransferMarketSystem>(transferSystem);

            var injurySystem = new InjurySystem();
            ServiceLocator.Register<InjurySystem>(injurySystem);

            var moraleSystem = new MoraleSystem();
            ServiceLocator.Register<MoraleSystem>(moraleSystem);

            var trainingSystem = new TrainingSystem();
            ServiceLocator.Register<TrainingSystem>(trainingSystem);

            var competitionSystem = new CompetitionSystem();
            ServiceLocator.Register<CompetitionSystem>(competitionSystem);

            var trophySystem = new TrophySystem();
            ServiceLocator.Register<TrophySystem>(trophySystem);

            var playerRatingSystem = new PlayerRatingSystem();
            ServiceLocator.Register<PlayerRatingSystem>(playerRatingSystem);

            var seasonSystem = new SeasonSystem(playerRatingSystem);
            ServiceLocator.Register<SeasonSystem>(seasonSystem);

            var careerSystem = new CareerSystem();
            ServiceLocator.Register<CareerSystem>(careerSystem);

            var currencySystem = new CurrencySystem();
            ServiceLocator.Register<CurrencySystem>(currencySystem);

            var progressionSystem = new ProgressionSystem();
            ServiceLocator.Register<ProgressionSystem>(progressionSystem);

            var liveEventSystem = new LiveEventSystem();
            ServiceLocator.Register<LiveEventSystem>(liveEventSystem);

            var predictorSystem = new MatchPredictorSystem(matchEngine, tacticalSystem);
            ServiceLocator.Register<MatchPredictorSystem>(predictorSystem);
            yield return null;

            // State machine
            stateMachine = new GameStateMachine();
            stateMachine.Initialize();
            yield return null;

            isInitialized = true;
            Debug.Log("[GameManager] All systems initialized.");

            TransitionToState(GameStateType.MainMenu);
        }

        // ── State transitions ──────────────────────────────────────────────────

        /// <summary>Requests a transition to <paramref name="newState"/>.</summary>
        public void TransitionToState(GameStateType newState)
        {
            if (!isInitialized && newState != GameStateType.MainMenu)
            {
                Debug.LogWarning($"[GameManager] Cannot transition to {newState} before initialization.");
                return;
            }

            CurrentState = newState;
            stateMachine?.TransitionTo(newState);
            OnStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] → {newState}");
        }

        /// <summary>Convenience: go to the main menu.</summary>
        public void GoToMainMenu() => TransitionToState(GameStateType.MainMenu);

        /// <summary>Convenience: start a new game session.</summary>
        public void StartGame() => TransitionToState(GameStateType.Loading);

        /// <summary>Convenience: enter live match mode.</summary>
        public void EnterMatch() => TransitionToState(GameStateType.Match);

        /// <summary>Pause / unpause.</summary>
        public void TogglePause()
        {
            if (CurrentState == GameStateType.InGame || CurrentState == GameStateType.Match)
                TransitionToState(GameStateType.Paused);
            else if (CurrentState == GameStateType.Paused)
                TransitionToState(GameStateType.InGame);
        }

        // ── Cleanup ────────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (instance == this)
            {
                ServiceLocator.Clear();
                EventBus.ClearAll();
                instance = null;
            }
        }
    }

    /// <summary>High-level game states driving the state machine.</summary>
    public enum GameStateType
    {
        MainMenu,
        Loading,
        InGame,
        Match,
        Paused
    }
}
