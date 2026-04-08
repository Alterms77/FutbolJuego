using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.AI;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Settings screen: language, difficulty, audio, and notification toggles.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Language")]
        [SerializeField] private TMPro.TMP_Dropdown languageDropdown;

        [Header("Difficulty")]
        [SerializeField] private TMPro.TMP_Dropdown difficultyDropdown;

        [Header("Audio")]
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Notifications")]
        [SerializeField] private Toggle notificationsToggle;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI statusLabel;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Start()
        {
            // Populate language options
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new System.Collections.Generic.List<string>
                    { "English", "Español", "Português", "Deutsch", "Français", "Italiano", "日本語" });
            }

            // Populate difficulty options
            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
                    { "Easy", "Medium", "Hard", "Expert" });
                difficultyDropdown.value = 1; // Default Medium
            }
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        /// <summary>Called when the player selects a language.</summary>
        public void OnLanguageChanged(string lang)
        {
            LocalizationManager.SetLanguage(lang);
            ShowStatus($"Language set to {lang}");
        }

        /// <summary>Called from the language dropdown.</summary>
        public void OnLanguageDropdownChanged(int index)
        {
            string[] codes = { "en", "es", "pt", "de", "fr", "it", "ja" };
            if (index < codes.Length)
                OnLanguageChanged(codes[index]);
        }

        /// <summary>Called when the player changes the difficulty level.</summary>
        public void OnDifficultyChanged(AIDifficulty difficulty)
        {
            ShowStatus($"Difficulty set to {difficulty}");
            Debug.Log($"[Settings] Difficulty → {difficulty}");
        }

        /// <summary>Called from the difficulty dropdown.</summary>
        public void OnDifficultyDropdownChanged(int index)
        {
            var difficulties = new[] { AIDifficulty.Easy, AIDifficulty.Medium,
                                       AIDifficulty.Hard, AIDifficulty.Expert };
            if (index < difficulties.Length)
                OnDifficultyChanged(difficulties[index]);
        }

        /// <summary>Called when the sound toggle changes.</summary>
        public void OnSoundToggle(bool enabled)
        {
            AudioListener.volume = enabled ? 1f : 0f;
            ShowStatus($"Sound {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Called when the notifications toggle changes.</summary>
        public void OnNotificationsToggle(bool enabled)
        {
            Debug.Log($"[Settings] Notifications {(enabled ? "enabled" : "disabled")}");
            ShowStatus($"Notifications {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Adjusts music volume.</summary>
        public void OnMusicVolumeChanged(float value)
        {
            // Wire to AudioMixer in production
            Debug.Log($"[Settings] Music volume → {value:F2}");
        }

        /// <summary>Adjusts SFX volume.</summary>
        public void OnSFXVolumeChanged(float value)
        {
            Debug.Log($"[Settings] SFX volume → {value:F2}");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void ShowStatus(string message)
        {
            if (statusLabel) statusLabel.text = message;
        }
    }
}
