using UnityEngine;
using FutbolJuego.Core;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Controls the main-menu screen: button callbacks, game-state subscriptions,
    /// and animated transitions.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loadPanel;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        // ── Button callbacks (called by Unity UI) ──────────────────────────────

        /// <summary>Starts a new game session.</summary>
        public void OnPlayButton()
        {
            Debug.Log("[MainMenuUI] Play pressed.");
            GameManager.Instance?.StartGame();
        }

        /// <summary>Opens the settings overlay.</summary>
        public void OnSettingsButton()
        {
            Debug.Log("[MainMenuUI] Settings pressed.");
            SetPanelActive(settingsPanel);
        }

        /// <summary>Opens the load-game overlay.</summary>
        public void OnLoadGameButton()
        {
            Debug.Log("[MainMenuUI] Load Game pressed.");
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
            gameObject.SetActive(state == GameStateType.MainMenu);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetPanelActive(GameObject target)
        {
            if (mainPanel)    mainPanel.SetActive(target == mainPanel);
            if (settingsPanel) settingsPanel.SetActive(target == settingsPanel);
            if (loadPanel)    loadPanel.SetActive(target == loadPanel);
        }
    }
}
