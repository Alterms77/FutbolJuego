using System.Collections;
using UnityEngine;

namespace FutbolJuego.Core
{
    /// <summary>
    /// Entry-point MonoBehaviour for the <c>Bootstrap</c> scene.
    ///
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>Ensures <see cref="GameManager"/> is initialised.</item>
    ///   <item>Ensures <see cref="SceneNavigator"/> is initialised.</item>
    ///   <item>Transitions to the <c>MainMenu</c> scene after a short delay.</item>
    /// </list>
    ///
    /// Attach this script to a single GameObject in the Bootstrap scene.
    /// The Bootstrap scene should be the first scene in the Build Settings list.
    /// </summary>
    public class BootstrapController : MonoBehaviour
    {
        [Tooltip("Seconds to wait before loading MainMenu (useful for showing a splash logo).")]
        [SerializeField] private float splashDuration = 1.5f;

        private void Awake()
        {
            // GameManager initialises itself in its own Awake via DontDestroyOnLoad.
            // Ensure it exists in the scene by requiring it as a sibling component
            // or by checking the singleton; if the Bootstrap prefab has it as a
            // child GameObject, Unity will have already called its Awake.

            // Guarantee SceneNavigator exists.  If it is already alive from a
            // previous load it will destroy itself via its own Awake guard.
            if (SceneNavigator.Instance == null)
            {
                var navigatorGO = new GameObject("SceneNavigator");
                navigatorGO.AddComponent<SceneNavigator>();
            }
        }

        private void Start()
        {
            StartCoroutine(LoadMainMenuAfterDelay());
        }

        private IEnumerator LoadMainMenuAfterDelay()
        {
            yield return new WaitForSeconds(splashDuration);
            SceneNavigator.Instance.GoToMainMenu();
        }
    }
}
