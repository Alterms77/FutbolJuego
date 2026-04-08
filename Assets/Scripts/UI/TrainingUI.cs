using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Training screen: training focus selection, fatigue overview per player.
    /// </summary>
    public class TrainingUI : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Fatigue")]
        [SerializeField] private Transform fatigueContainer;
        [SerializeField] private GameObject fatigueRowPrefab;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI infoLabel;

        private TrainingFocus selectedFocus = TrainingFocus.Tactical;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Start() => ShowTrainingOptions();

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Builds the training focus option buttons.</summary>
        public void ShowTrainingOptions()
        {
            if (optionsContainer == null || optionButtonPrefab == null) return;

            foreach (Transform child in optionsContainer)
                Destroy(child.gameObject);

            foreach (TrainingFocus focus in System.Enum.GetValues(typeof(TrainingFocus)))
            {
                var btn   = Instantiate(optionButtonPrefab, optionsContainer);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = focus.ToString();

                var toggle = btn.GetComponent<Button>();
                if (toggle)
                {
                    var captured = focus;
                    toggle.onClick.AddListener(() => OnTrainingFocusSelected(captured));
                }
            }
        }

        /// <summary>Populates the fatigue overview for all players.</summary>
        public void ShowPlayerFatigue(List<PlayerData> players)
        {
            if (fatigueContainer == null || fatigueRowPrefab == null) return;

            foreach (Transform child in fatigueContainer)
                Destroy(child.gameObject);

            foreach (var player in players)
            {
                var row   = Instantiate(fatigueRowPrefab, fatigueContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text = $"{player.name,-24}  Fatigue: {player.fatigue,3}/100  " +
                                 $"{(player.injuryDaysRemaining > 0 ? $"INJURED ({player.injuryDaysRemaining}d)" : "Fit")}";

                // Optional: progress bar fill
                var slider = row.GetComponentInChildren<Slider>();
                if (slider) slider.value = player.fatigue / 100f;
            }
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        /// <summary>Called when the user selects a training focus.</summary>
        public void OnTrainingFocusSelected(TrainingFocus focus)
        {
            selectedFocus = focus;
            if (infoLabel)
                infoLabel.text = $"Focus: {focus}  — affects {GetFocusDescription(focus)}";

            Debug.Log($"[TrainingUI] Focus set to {focus}");
        }

        /// <summary>Triggers a training session for all available players.</summary>
        public void OnStartTrainingButton()
        {
            Debug.Log($"[TrainingUI] Starting training with focus {selectedFocus}");
            // Wire this to the TrainingSystem via the game controller
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string GetFocusDescription(TrainingFocus focus) => focus switch
        {
            TrainingFocus.Shooting  => "Finishing & Long shots",
            TrainingFocus.Passing   => "Short passing & Vision",
            TrainingFocus.Defense   => "Tackling & Positioning",
            TrainingFocus.Speed     => "Sprint speed & Acceleration",
            TrainingFocus.Physical  => "Strength & Stamina",
            TrainingFocus.Tactical  => "Decision-making & Intelligence",
            _                       => focus.ToString()
        };
    }
}
