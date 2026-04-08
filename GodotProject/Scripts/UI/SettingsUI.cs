using System.Collections.Generic;
using Godot;
using FutbolJuego.AI;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Settings screen: language, difficulty, audio, and notification toggles.
    /// </summary>
    public partial class SettingsUI : Control
    {
        [ExportGroup("Language")]
        [Export] public OptionButton languageDropdown;

        [ExportGroup("Difficulty")]
        [Export] public OptionButton difficultyDropdown;

        [ExportGroup("Audio")]
        [Export] public CheckBox soundToggle;
        [Export] public ProgressBar musicVolumeSlider;
        [Export] public ProgressBar sfxVolumeSlider;

        [ExportGroup("Notifications")]
        [Export] public CheckBox notificationsToggle;

        [ExportGroup("Info")]
        [Export] public Label statusLabel;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (languageDropdown != null)
            {
                languageDropdown.Clear();
                foreach (var lang in new[] { "English", "Español", "Português", "Deutsch", "Français", "Italiano", "日本語" })
                    languageDropdown.AddItem(lang);
            }

            if (difficultyDropdown != null)
            {
                difficultyDropdown.Clear();
                foreach (var diff in new[] { "Easy", "Medium", "Hard", "Expert" })
                    difficultyDropdown.AddItem(diff);
                difficultyDropdown.Select(1);
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
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("Master"),
                Mathf.LinearToDb(enabled ? 1f : 0f));
            ShowStatus($"Sound {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Called when the notifications toggle changes.</summary>
        public void OnNotificationsToggle(bool enabled)
        {
            ShowStatus($"Notifications {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Adjusts music volume.</summary>
        public void OnMusicVolumeChanged(float value)
        {
            // Wire to AudioBus in production
        }

        /// <summary>Adjusts SFX volume.</summary>
        public void OnSFXVolumeChanged(float value)
        {
            // Wire to AudioBus in production
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void ShowStatus(string message)
        {
            if (statusLabel != null) statusLabel.Text = message;
        }
    }
}
