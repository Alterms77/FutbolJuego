using Godot;

namespace FutbolJuego.Utils
{
    /// <summary>
    /// Persists user preferences (language, difficulty, audio, notifications)
    /// using Godot's <see cref="ConfigFile"/> — the Godot equivalent of
    /// Unity's PlayerPrefs.  All settings are stored in
    /// <c>user://settings.cfg</c>.
    /// </summary>
    public static class SettingsManager
    {
        private const string FilePath = "user://settings.cfg";

        // ── Section keys ───────────────────────────────────────────────────────
        private const string SectionGeneral  = "general";
        private const string SectionAudio    = "audio";

        private const string KeyLanguage      = "language";
        private const string KeyDifficulty    = "difficulty";
        private const string KeySoundEnabled  = "sound_enabled";
        private const string KeyMusicVolume   = "music_volume";
        private const string KeySfxVolume     = "sfx_volume";
        private const string KeyNotifications = "notifications";

        // ── Defaults ───────────────────────────────────────────────────────────
        private const string DefaultLanguage  = "en";
        private const int    DefaultDifficulty = 1; // Medium
        private const bool   DefaultSound      = true;
        private const float  DefaultMusic      = 1.0f;
        private const float  DefaultSfx        = 1.0f;
        private const bool   DefaultNotifications = true;

        // ── Save ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes all provided settings to <c>user://settings.cfg</c>.
        /// </summary>
        public static void Save(
            string language,
            int    difficulty,
            bool   soundEnabled,
            float  musicVolume,
            float  sfxVolume,
            bool   notificationsEnabled)
        {
            var config = new ConfigFile();

            config.SetValue(SectionGeneral, KeyLanguage,      language);
            config.SetValue(SectionGeneral, KeyDifficulty,    difficulty);
            config.SetValue(SectionAudio,   KeySoundEnabled,  soundEnabled);
            config.SetValue(SectionAudio,   KeyMusicVolume,   musicVolume);
            config.SetValue(SectionAudio,   KeySfxVolume,     sfxVolume);
            config.SetValue(SectionGeneral, KeyNotifications, notificationsEnabled);

            Error err = config.Save(FilePath);
            if (err != Error.Ok)
                GD.PushError($"[SettingsManager] Failed to save settings: {err}");
            else
                GD.Print("[SettingsManager] Settings saved.");
        }

        // ── Load ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads persisted settings.  Returns a <see cref="SettingsData"/>
        /// filled with defaults when no save file exists.
        /// </summary>
        public static SettingsData Load()
        {
            var config = new ConfigFile();
            Error err = config.Load(FilePath);

            if (err != Error.Ok)
            {
                GD.Print("[SettingsManager] No settings file found — using defaults.");
                return new SettingsData();
            }

            return new SettingsData
            {
                Language      = (string) config.GetValue(SectionGeneral, KeyLanguage,      DefaultLanguage),
                Difficulty    = (int)    config.GetValue(SectionGeneral, KeyDifficulty,    DefaultDifficulty),
                SoundEnabled  = (bool)   config.GetValue(SectionAudio,   KeySoundEnabled,  DefaultSound),
                MusicVolume   = (float)  config.GetValue(SectionAudio,   KeyMusicVolume,   DefaultMusic),
                SfxVolume     = (float)  config.GetValue(SectionAudio,   KeySfxVolume,     DefaultSfx),
                Notifications = (bool)   config.GetValue(SectionGeneral, KeyNotifications, DefaultNotifications),
            };
        }

        // ── Reset ──────────────────────────────────────────────────────────────

        /// <summary>Deletes the settings file, reverting to defaults on next load.</summary>
        public static void Reset()
        {
            if (FileAccess.FileExists(FilePath))
            {
                DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(FilePath));
                GD.Print("[SettingsManager] Settings reset to defaults.");
            }
        }
    }

    // ── Data container ─────────────────────────────────────────────────────────

    /// <summary>Snapshot of all user-configurable settings.</summary>
    public class SettingsData
    {
        /// <summary>Active language code (e.g. "en", "es").</summary>
        public string Language      { get; set; } = "en";
        /// <summary>Difficulty index: 0=Easy, 1=Medium, 2=Hard, 3=Expert.</summary>
        public int    Difficulty    { get; set; } = 1;
        /// <summary>Whether master audio is enabled.</summary>
        public bool   SoundEnabled  { get; set; } = true;
        /// <summary>Music bus volume (0–1).</summary>
        public float  MusicVolume   { get; set; } = 1.0f;
        /// <summary>SFX bus volume (0–1).</summary>
        public float  SfxVolume     { get; set; } = 1.0f;
        /// <summary>Whether push notifications are enabled.</summary>
        public bool   Notifications { get; set; } = true;
    }
}
