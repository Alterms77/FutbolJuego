using UnityEngine;
using UnityEngine.UI;
using FutbolJuego.Core;

namespace FutbolJuego.UI.Components
{
    /// <summary>
    /// Reusable bottom navigation bar.  Wire up the serialised
    /// <see cref="Button"/> fields in the prefab inspector; each button will
    /// trigger a scene transition via <see cref="SceneNavigator"/>.
    /// </summary>
    public class NavigationBar : MonoBehaviour
    {
        [Header("Nav Buttons")]
        [SerializeField] private Button dashboardButton;
        [SerializeField] private Button squadButton;
        [SerializeField] private Button tacticsButton;
        [SerializeField] private Button transferMarketButton;
        [SerializeField] private Button financesButton;
        [SerializeField] private Button competitionsButton;
        [SerializeField] private Button shopButton;

        private void Awake()
        {
            if (dashboardButton)      dashboardButton.onClick.AddListener(OnDashboard);
            if (squadButton)          squadButton.onClick.AddListener(OnSquad);
            if (tacticsButton)        tacticsButton.onClick.AddListener(OnTactics);
            if (transferMarketButton) transferMarketButton.onClick.AddListener(OnTransferMarket);
            if (financesButton)       financesButton.onClick.AddListener(OnFinances);
            if (competitionsButton)   competitionsButton.onClick.AddListener(OnCompetitions);
            if (shopButton)           shopButton.onClick.AddListener(OnShop);
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
