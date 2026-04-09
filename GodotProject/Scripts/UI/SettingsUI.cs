using Godot;
using FutbolJuego.AI;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Settings screen: language, difficulty, audio, and notification toggles.
    /// Persists user preferences via <see cref="SettingsManager"/> (ConfigFile).
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

        // ── Current values (updated by callbacks before saving) ────────────────
        private string _language      = "en";
        private int    _difficulty    = 1;
        private bool   _soundEnabled  = true;
        private float  _musicVolume   = 1.0f;
        private float  _sfxVolume     = 1.0f;
        private bool   _notifications = true;

        private static readonly string[] LanguageCodes  = { "en", "es", "pt", "de", "fr", "it", "ja" };
        private static readonly string[] LanguageLabels = { "English", "Español", "Português", "Deutsch", "Français", "Italiano", "日本語" };

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            // Populate dropdowns
            if (languageDropdown != null)
            {
                languageDropdown.Clear();
                foreach (var lang in LanguageLabels)
                    languageDropdown.AddItem(lang);
            }

            if (difficultyDropdown != null)
            {
                difficultyDropdown.Clear();
                foreach (var diff in new[] { "Easy", "Medium", "Hard", "Expert" })
                    difficultyDropdown.AddItem(diff);
            }

            // Load persisted settings and apply to UI
            ApplySettings(SettingsManager.Load());
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        /// <summary>Called with a language code (e.g. "en", "es") to change the active language.</summary>
        public void OnLanguageChanged(string languageCode)
        {
            _language = languageCode;
            LocalizationManager.SetLanguage(languageCode);
            SaveCurrent();
            ShowStatus($"Language set to {languageCode}");
        }

        /// <summary>Called from the language dropdown.</summary>
        public void OnLanguageDropdownChanged(int index)
        {
            if (index < LanguageCodes.Length)
                OnLanguageChanged(LanguageCodes[index]);
        }

        /// <summary>Called when the player changes the difficulty level.</summary>
        public void OnDifficultyChanged(AIDifficulty difficulty)
        {
            _difficulty = (int)difficulty;
            SaveCurrent();
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
            _soundEnabled = enabled;
            ApplyMasterVolume(enabled);
            SaveCurrent();
            ShowStatus($"Sound {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Called when the notifications toggle changes.</summary>
        public void OnNotificationsToggle(bool enabled)
        {
            _notifications = enabled;
            SaveCurrent();
            ShowStatus($"Notifications {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>Adjusts music volume.</summary>
        public void OnMusicVolumeChanged(float value)
        {
            _musicVolume = value;
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("Music"),
                Mathf.LinearToDb(Mathf.Clamp(value, 0f, 1f)));
            SaveCurrent();
        }

        /// <summary>Adjusts SFX volume.</summary>
        public void OnSFXVolumeChanged(float value)
        {
            _sfxVolume = value;
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("SFX"),
                Mathf.LinearToDb(Mathf.Clamp(value, 0f, 1f)));
            SaveCurrent();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Applies <paramref name="data"/> to the UI controls.</summary>
        private void ApplySettings(SettingsData data)
        {
            _language      = data.Language;
            _difficulty    = data.Difficulty;
            _soundEnabled  = data.SoundEnabled;
            _musicVolume   = data.MusicVolume;
            _sfxVolume     = data.SfxVolume;
            _notifications = data.Notifications;

            // Sync UI controls
            if (languageDropdown != null)
            {
                int langIdx = System.Array.IndexOf(LanguageCodes, data.Language);
                languageDropdown.Select(langIdx >= 0 ? langIdx : 0);
            }

            if (difficultyDropdown != null)
                difficultyDropdown.Select(Mathf.Clamp(data.Difficulty, 0, 3));

            if (soundToggle != null)
                soundToggle.ButtonPressed = data.SoundEnabled;

            if (musicVolumeSlider != null)
                musicVolumeSlider.Value = data.MusicVolume;

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.Value = data.SfxVolume;

            if (notificationsToggle != null)
                notificationsToggle.ButtonPressed = data.Notifications;

            // Apply audio immediately
            ApplyMasterVolume(data.SoundEnabled);

            LocalizationManager.SetLanguage(data.Language);
        }

        /// <summary>Sets the Master audio bus volume based on whether sound is enabled.</summary>
        private static void ApplyMasterVolume(bool enabled)
        {
            AudioServer.SetBusVolumeDb(
                AudioServer.GetBusIndex("Master"),
                Mathf.LinearToDb(enabled ? 1f : 0f));
        }

        /// <summary>Persists the current field values via SettingsManager.</summary>
        private void SaveCurrent()
        {
            SettingsManager.Save(
                _language,
                _difficulty,
                _soundEnabled,
                _musicVolume,
                _sfxVolume,
                _notifications);
        }

        private void ShowStatus(string message)
        {
            if (statusLabel != null) statusLabel.Text = message;
        }
    }
}
