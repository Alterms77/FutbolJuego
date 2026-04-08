using UnityEngine;
using UnityEngine.SceneManagement;

namespace FutbolJuego.Core
{
    /// <summary>
    /// Singleton scene-navigation helper.  Lives in the Bootstrap scene and
    /// persists across loads via <see cref="DontDestroyOnLoad"/>.
    ///
    /// All scene transitions in the game should go through this class so that
    /// analytics, loading screens, or transition animations can be added in
    /// one place.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        // ── Scene name constants ───────────────────────────────────────────────

        public const string SCENE_BOOTSTRAP       = "Bootstrap";
        public const string SCENE_MAIN_MENU       = "MainMenu";
        public const string SCENE_DASHBOARD       = "Dashboard";
        public const string SCENE_SQUAD           = "Squad";
        public const string SCENE_TACTICS         = "Tactics";
        public const string SCENE_MATCH           = "Match";
        public const string SCENE_TRANSFER_MARKET = "TransferMarket";
        public const string SCENE_FINANCES        = "Finances";
        public const string SCENE_COMPETITIONS    = "Competitions";
        public const string SCENE_SHOP            = "Shop";

        // ── Singleton ─────────────────────────────────────────────────────────

        private static SceneNavigator instance;

        /// <summary>The single, persistent instance of <see cref="SceneNavigator"/>.</summary>
        public static SceneNavigator Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Navigation API ─────────────────────────────────────────────────────

        /// <summary>Loads the scene with the given <paramref name="sceneName"/>.</summary>
        public void GoTo(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneNavigator] GoTo called with null or empty scene name.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        /// <summary>Loads the <c>MainMenu</c> scene.</summary>
        public void GoToMainMenu() => GoTo(SCENE_MAIN_MENU);

        /// <summary>Loads the <c>Dashboard</c> scene.</summary>
        public void GoToDashboard() => GoTo(SCENE_DASHBOARD);

        /// <summary>Loads the <c>Squad</c> scene.</summary>
        public void GoToSquad() => GoTo(SCENE_SQUAD);

        /// <summary>Loads the <c>Tactics</c> scene.</summary>
        public void GoToTactics() => GoTo(SCENE_TACTICS);

        /// <summary>Loads the <c>Match</c> scene.</summary>
        public void GoToMatch() => GoTo(SCENE_MATCH);

        /// <summary>Loads the <c>TransferMarket</c> scene.</summary>
        public void GoToTransferMarket() => GoTo(SCENE_TRANSFER_MARKET);

        /// <summary>Loads the <c>Finances</c> scene.</summary>
        public void GoToFinances() => GoTo(SCENE_FINANCES);

        /// <summary>Loads the <c>Competitions</c> scene.</summary>
        public void GoToCompetitions() => GoTo(SCENE_COMPETITIONS);

        /// <summary>Loads the <c>Shop</c> scene.</summary>
        public void GoToShop() => GoTo(SCENE_SHOP);
    }
}
