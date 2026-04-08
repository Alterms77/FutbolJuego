using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Displays the tactics setup screen: formation selector, sliders, pitch
    /// visualisation with player rating and energy badges, and the pre-match
    /// prediction panel.  Supports basic drag-and-drop repositioning of player
    /// dots on the pitch canvas.
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
        private List<PlayerData> currentSquad = new List<PlayerData>();

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Start()
        {
            if (pressingSlider)      pressingSlider.onValueChanged.AddListener(v      => OnTacticParameterChanged("pressing",      (int)v));
            if (tempoSlider)         tempoSlider.onValueChanged.AddListener(v          => OnTacticParameterChanged("tempo",         (int)v));
            if (widthSlider)         widthSlider.onValueChanged.AddListener(v          => OnTacticParameterChanged("width",         (int)v));
            if (defensiveLineSlider) defensiveLineSlider.onValueChanged.AddListener(v  => OnTacticParameterChanged("defensiveLine", (int)v));
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

        /// <summary>Provides the squad used to annotate pitch nodes.</summary>
        public void SetSquad(List<PlayerData> squad)
        {
            currentSquad = squad ?? new List<PlayerData>();
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

        /// <summary>Redraws player dots on the pitch canvas with rating and energy labels.</summary>
        public void UpdatePitchVisualization()
        {
            if (pitchContainer == null || playerDotPrefab == null || currentTactic == null) return;

            foreach (Transform child in pitchContainer)
                Destroy(child.gameObject);

            if (!ServiceLocator.TryGet<TacticalSystem>(out var system)) return;

            var positions = system.GetFormationPositions(currentTactic.formation);
            var rect      = pitchContainer.GetComponent<RectTransform>();

            int assignmentCount = currentTactic.positionAssignments?.Count ?? 0;

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                var dot = Instantiate(playerDotPrefab, pitchContainer);
                var rt  = dot.GetComponent<RectTransform>();

                if (rt && rect)
                {
                    float x = (pos.x / 100f) * rect.rect.width  - rect.rect.width  * 0.5f;
                    float y = (pos.y / 100f) * rect.rect.height - rect.rect.height * 0.5f;
                    rt.anchoredPosition = new Vector2(x, y);
                }

                // Annotate dot with player rating and energy if an assignment exists
                PlayerData assignedPlayer = null;
                if (assignmentCount > i && currentTactic.positionAssignments[i] != null)
                {
                    string pid = currentTactic.positionAssignments[i].playerId;
                    foreach (var p in currentSquad)
                        if (p.id == pid) { assignedPlayer = p; break; }
                }
                else if (currentSquad.Count > i)
                {
                    assignedPlayer = currentSquad[i];
                }

                if (assignedPlayer != null)
                {
                    var labels = dot.GetComponentsInChildren<TextMeshProUGUI>();
                    if (labels.Length >= 1)
                        labels[0].text = $"{assignedPlayer.overallRating}";
                    if (labels.Length >= 2)
                        labels[1].text = $"⚡{assignedPlayer.energy}";

                    // Colour the dot border by rarity
                    var img = dot.GetComponent<Image>();
                    if (img) img.color = RarityToColor(assignedPlayer.rarity);
                }

                // Add drag handler for repositioning
                AddDragHandler(dot, rt, rect);
            }
        }

        // ── Drag-and-drop ──────────────────────────────────────────────────────

        private void AddDragHandler(GameObject dot, RectTransform dotRT, RectTransform containerRT)
        {
            var trigger = dot.GetComponent<EventTrigger>();
            if (trigger == null) trigger = dot.AddComponent<EventTrigger>();

            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener(data =>
            {
                if (containerRT == null || dotRT == null) return;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    containerRT,
                    ((PointerEventData)data).position,
                    null,
                    out Vector2 localPoint);
                dotRT.anchoredPosition = localPoint;
            });
            trigger.triggers.Add(dragEntry);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static Color RarityToColor(PlayerRarity rarity) => rarity switch
        {
            PlayerRarity.Silver => UITheme.RaritySilver,
            PlayerRarity.Gold   => UITheme.RarityGold,
            PlayerRarity.Star   => UITheme.RarityStar,
            PlayerRarity.Legend => UITheme.RarityLegend,
            _                   => UITheme.RarityNormal
        };
    }
}
