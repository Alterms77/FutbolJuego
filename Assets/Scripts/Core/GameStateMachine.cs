using System.Collections.Generic;
using UnityEngine;

namespace FutbolJuego.Core
{
    // ── State interface ────────────────────────────────────────────────────────

    /// <summary>Contract that every concrete game state must implement.</summary>
    public interface IGameState
    {
        /// <summary>Called once when this state becomes active.</summary>
        void Enter();

        /// <summary>Called every frame while this state is active.</summary>
        void Update();

        /// <summary>Called once just before this state is replaced.</summary>
        void Exit();
    }

    // ── Concrete states ────────────────────────────────────────────────────────

    /// <summary>State active while the main-menu scene is shown.</summary>
    public class MainMenuState : IGameState
    {
        /// <inheritdoc/>
        public void Enter() => Debug.Log("[State] Enter MainMenu");
        /// <inheritdoc/>
        public void Update() { }
        /// <inheritdoc/>
        public void Exit()   => Debug.Log("[State] Exit MainMenu");
    }

    /// <summary>State active while assets and save data are being loaded.</summary>
    public class LoadingState : IGameState
    {
        /// <inheritdoc/>
        public void Enter() => Debug.Log("[State] Enter Loading");
        /// <inheritdoc/>
        public void Update() { }
        /// <inheritdoc/>
        public void Exit()   => Debug.Log("[State] Exit Loading");
    }

    /// <summary>State active during the main manager loop (squad, tactics, transfers…).</summary>
    public class InGameState : IGameState
    {
        /// <inheritdoc/>
        public void Enter() => Debug.Log("[State] Enter InGame");
        /// <inheritdoc/>
        public void Update() { }
        /// <inheritdoc/>
        public void Exit()   => Debug.Log("[State] Exit InGame");
    }

    /// <summary>State active while a match is being simulated or animated.</summary>
    public class MatchState : IGameState
    {
        /// <inheritdoc/>
        public void Enter() => Debug.Log("[State] Enter Match");
        /// <inheritdoc/>
        public void Update() { }
        /// <inheritdoc/>
        public void Exit()   => Debug.Log("[State] Exit Match");
    }

    /// <summary>State pushed on top when the user pauses mid-session.</summary>
    public class PausedState : IGameState
    {
        /// <inheritdoc/>
        public void Enter()
        {
            Time.timeScale = 0f;
            Debug.Log("[State] Enter Paused");
        }

        /// <inheritdoc/>
        public void Update() { }

        /// <inheritdoc/>
        public void Exit()
        {
            Time.timeScale = 1f;
            Debug.Log("[State] Exit Paused");
        }
    }

    // ── State machine ──────────────────────────────────────────────────────────

    /// <summary>
    /// Manages transitions between <see cref="IGameState"/> instances.
    /// Kept separate from <see cref="GameManager"/> to honour SRP.
    /// </summary>
    public class GameStateMachine
    {
        private IGameState currentState;

        private readonly Dictionary<GameStateType, IGameState> stateMap =
            new Dictionary<GameStateType, IGameState>();

        /// <summary>The currently active state, or <c>null</c> before initialisation.</summary>
        public IGameState CurrentState => currentState;

        /// <summary>Builds the state map. Must be called once before any transitions.</summary>
        public void Initialize()
        {
            stateMap[GameStateType.MainMenu] = new MainMenuState();
            stateMap[GameStateType.Loading]  = new LoadingState();
            stateMap[GameStateType.InGame]   = new InGameState();
            stateMap[GameStateType.Match]    = new MatchState();
            stateMap[GameStateType.Paused]   = new PausedState();
        }

        /// <summary>
        /// Exits the current state (if any) and enters the state mapped to
        /// <paramref name="newStateType"/>.
        /// </summary>
        public void TransitionTo(GameStateType newStateType)
        {
            if (!stateMap.TryGetValue(newStateType, out IGameState newState))
            {
                Debug.LogError($"[GameStateMachine] No state registered for {newStateType}.");
                return;
            }

            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        /// <summary>Forwards the Unity Update tick to the active state.</summary>
        public void Update() => currentState?.Update();
    }
}
