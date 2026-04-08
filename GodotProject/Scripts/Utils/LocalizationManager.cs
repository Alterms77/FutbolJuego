using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace FutbolJuego.Utils
{
    /// <summary>
    /// Simple localisation manager that resolves string keys to the
    /// appropriate language string.  Language data is loaded from
    /// res://Localisation/{languageCode}.json at runtime.
    /// </summary>
    public class LocalizationManager
    {
        private static Dictionary<string, string> currentStrings =
            new Dictionary<string, string>();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        /// <summary>The active language code (e.g. "en", "es", "pt").</summary>
        public static string CurrentLanguage { get; private set; } = "en";

        // ── Look-up ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the localised string for <paramref name="key"/>, or the
        /// key itself when no mapping exists.
        /// </summary>
        public static string Get(string key)
        {
            if (currentStrings.TryGetValue(key, out string value))
                return value;

            GD.PushWarning($"[Localisation] Missing key '{key}' for language '{CurrentLanguage}'.");
            return key;
        }

        // ── Language switching ─────────────────────────────────────────────────

        /// <summary>
        /// Loads the language file for <paramref name="languageCode"/> and
        /// rebuilds the string dictionary.
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            CurrentLanguage = languageCode;

            string fullPath = $"res://Localisation/{languageCode}.json";
            if (!FileAccess.FileExists(fullPath))
            {
                GD.PushWarning($"[Localisation] No file found for language '{languageCode}'. Using fallback.");
                LoadFallback();
                return;
            }

            try
            {
                using var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
                string json = file.GetAsText();
                var wrapper = JsonSerializer.Deserialize<LocalisationWrapper>(json, JsonOptions);
                currentStrings = new Dictionary<string, string>();
                if (wrapper?.entries != null)
                    foreach (var entry in wrapper.entries)
                        currentStrings[entry.key] = entry.value;

                GD.Print($"[Localisation] Loaded {currentStrings.Count} strings for '{languageCode}'.");
            }
            catch
            {
                GD.PushError($"[Localisation] Failed to parse localisation file for '{languageCode}'.");
                LoadFallback();
            }
        }

        // ── Built-in fallback (English hard-coded) ─────────────────────────────

        private static void LoadFallback()
        {
            currentStrings = new Dictionary<string, string>
            {
                { Keys.MainMenuPlay,   "Play"              },
                { Keys.MainMenuLoad,   "Load Game"         },
                { Keys.MainMenuSettings, "Settings"        },
                { Keys.SquadTitle,     "Your Squad"        },
                { Keys.TacticsTitle,   "Tactics"           },
                { Keys.TransferTitle,  "Transfer Market"   },
                { Keys.FinancesTitle,  "Club Finances"     },
                { Keys.TrainingTitle,  "Training"          },
                { Keys.LeagueTitle,    "League Table"      },
                { Keys.MatchdayTitle,  "Match Day"         },
                { Keys.SettingsTitle,  "Settings"          },
                { Keys.ButtonSimulate, "Simulate Match"    },
                { Keys.ButtonConfirm,  "Confirm"           },
                { Keys.ButtonCancel,   "Cancel"            },
                { Keys.Win,            "Victory!"          },
                { Keys.Draw,           "Draw"              },
                { Keys.Loss,           "Defeat"            },
            };
        }

        // ── JSON wrapper types ─────────────────────────────────────────────────

        [System.Serializable]
        private class LocalisationWrapper
        {
            public List<LocalisationEntry> entries;
        }

        [System.Serializable]
        private class LocalisationEntry
        {
            public string key;
            public string value;
        }

        // ── Key constants ──────────────────────────────────────────────────────

        /// <summary>All localisation keys used in the game.</summary>
        public static class Keys
        {
            public const string MainMenuPlay     = "main_menu_play";
            public const string MainMenuLoad     = "main_menu_load";
            public const string MainMenuSettings = "main_menu_settings";
            public const string SquadTitle       = "squad_title";
            public const string TacticsTitle     = "tactics_title";
            public const string TransferTitle    = "transfer_title";
            public const string FinancesTitle    = "finances_title";
            public const string TrainingTitle    = "training_title";
            public const string LeagueTitle      = "league_title";
            public const string MatchdayTitle    = "matchday_title";
            public const string SettingsTitle    = "settings_title";
            public const string ButtonSimulate   = "button_simulate";
            public const string ButtonConfirm    = "button_confirm";
            public const string ButtonCancel     = "button_cancel";
            public const string Win              = "result_win";
            public const string Draw             = "result_draw";
            public const string Loss             = "result_loss";
        }
    }
}
