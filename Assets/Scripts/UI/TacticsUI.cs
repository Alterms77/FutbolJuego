using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Displays the tactics setup screen: formation selector, sliders, pitch
    /// visualisation, and the pre-match prediction panel.
    /// </summary>
    public class TacticsUI : MonoBehaviour
    {
        [Header("Formation")]
        [SerializeField] private TMP_Dropdown formationDropdown;
        [SerializeField] private Transform pitchContainer;
        [SerializeField] private GameObject playerDotPrefab;

        [Header("Sliders")]
        [SerializeField] private Slider pressingSlider;
        [SerializeField] private Slider tempoSlider;
        [SerializeField] private Slider widthSlider;
        [SerializeField] private Slider defensiveLineSlider;

        [Header("Prediction")]
        [SerializeField] private TextMeshProUGUI predictionText;
        [SerializeField] private GameObject predictionPanel;

        private TacticData currentTactic;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Start()
        {
            if (pressingSlider)     pressingSlider.onValueChanged.AddListener(v => OnTacticParameterChanged("pressing",      (int)v));
            if (tempoSlider)        tempoSlider.onValueChanged.AddListener(v     => OnTacticParameterChanged("tempo",         (int)v));
            if (widthSlider)        widthSlider.onValueChanged.AddListener(v     => OnTacticParameterChanged("width",         (int)v));
            if (defensiveLineSlider) defensiveLineSlider.onValueChanged.AddListener(v => OnTacticParameterChanged("defensiveLine", (int)v));
        }

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Refreshes the UI to show <paramref name="tactic"/>.</summary>
        public void DisplayFormation(TacticData tactic)
        {
            if (tactic == null) return;
            currentTactic = tactic;

            if (pressingSlider)      pressingSlider.value      = tactic.pressing;
            if (tempoSlider)         tempoSlider.value         = tactic.tempo;
            if (widthSlider)         widthSlider.value         = tactic.width;
            if (defensiveLineSlider) defensiveLineSlider.value = tactic.defensiveLine;

            UpdatePitchVisualization();
        }

        /// <summary>Updates the floating prediction panel.</summary>
        public void ShowPrediction(MatchPrediction prediction)
        {
            if (prediction == null || predictionPanel == null) return;
            predictionPanel.SetActive(true);

            if (predictionText)
                predictionText.text =
                    $"Win: {prediction.winProbability.ToPercentageString()}  " +
                    $"Draw: {prediction.drawProbability.ToPercentageString()}  " +
                    $"Loss: {prediction.lossProbability.ToPercentageString()}\n" +
                    $"xG: {prediction.expectedGoalsHome:F2} – {prediction.expectedGoalsAway:F2}";
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        /// <summary>Called when the player selects a different formation.</summary>
        public void OnFormationChanged(Formation formation)
        {
            if (currentTactic == null) currentTactic = new TacticData();
            currentTactic.formation = formation;
            UpdatePitchVisualization();
        }

        /// <summary>Called by sliders when any tactical parameter changes.</summary>
        public void OnTacticParameterChanged(string param, int value)
        {
            if (currentTactic == null) return;

            switch (param)
            {
                case "pressing":      currentTactic.pressing      = value; break;
                case "tempo":         currentTactic.tempo         = value; break;
                case "width":         currentTactic.width         = value; break;
                case "defensiveLine": currentTactic.defensiveLine = value; break;
            }
        }

        /// <summary>Redraws player dots on the pitch canvas.</summary>
        public void UpdatePitchVisualization()
        {
            if (pitchContainer == null || playerDotPrefab == null || currentTactic == null) return;

            foreach (Transform child in pitchContainer)
                Destroy(child.gameObject);

            var system = ServiceLocator.TryGet<TacticalSystem>(out var ts) ? ts : null;
            if (system == null) return;

            var positions = system.GetFormationPositions(currentTactic.formation);
            var rect      = pitchContainer.GetComponent<RectTransform>();

            foreach (var pos in positions)
            {
                var dot = Instantiate(playerDotPrefab, pitchContainer);
                var rt  = dot.GetComponent<RectTransform>();
                if (rt && rect)
                {
                    float x = (pos.x / 100f) * rect.rect.width  - rect.rect.width  * 0.5f;
                    float y = (pos.y / 100f) * rect.rect.height - rect.rect.height * 0.5f;
                    rt.anchoredPosition = new Vector2(x, y);
                }
            }
        }
    }
}
