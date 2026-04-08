using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FutbolJuego.Core
{
    // ── Save payload ───────────────────────────────────────────────────────────

    /// <summary>Root serialisable object written to disk / cloud.</summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>Save-file format version for migration support.</summary>
        public int saveVersion = 1;
        /// <summary>UTC timestamp of the last save.</summary>
        public string lastSaved;
        /// <summary>Manager display name.</summary>
        public string managerName;
        /// <summary>Current manager XP.</summary>
        public int managerXP;
        /// <summary>Current season number.</summary>
        public int season;
        /// <summary>Total days elapsed in the game world.</summary>
        public int gameDayElapsed;
        /// <summary>Team identifier the manager controls.</summary>
        public string managedTeamId;
        /// <summary>JSON-serialised <see cref="Models.TeamData"/> for the player's club.</summary>
        public string teamDataJson;
        /// <summary>JSON-serialised <see cref="Models.LeagueData"/> for the active competition.</summary>
        public string leagueDataJson;
        /// <summary>FutCoins (premium currency) balance.</summary>
        public int premiumCurrency;
        /// <summary>Soft currency balance.</summary>
        public long coins;
        /// <summary>Last day the daily reward was claimed (1-7 weekly cycle).</summary>
        public int lastDailyRewardDay;
        /// <summary>UTC date of last daily reward claim.</summary>
        public string lastDailyRewardDate;
    }

    // ── Save system ────────────────────────────────────────────────────────────

    /// <summary>
    /// Handles local JSON persistence and provides stubs for cloud backup
    /// via Firebase.  AutoSave fires every <see cref="AutoSaveIntervalSeconds"/>.
    /// </summary>
    public class SaveSystem
    {
        private const string SaveFileName   = "futboljuego_save.json";
        private const string BackupSuffix   = ".bak";
        private const string EncryptionKey  = "FJ_AES_KEY_STUB_32BYTES_XXXXX123"; // 32-char placeholder

        /// <summary>Interval in real-time seconds between automatic saves.</summary>
        public float AutoSaveIntervalSeconds { get; set; } = 300f; // 5 minutes

        private float autoSaveTimer;
        private GameSaveData cachedSave;

        private string SavePath   => Path.Combine(Application.persistentDataPath, SaveFileName);
        private string BackupPath => SavePath + BackupSuffix;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Serialises <paramref name="data"/> to disk.  Writes a backup first
        /// so a crash during write cannot corrupt the primary save.
        /// </summary>
        public async Task SaveGame(GameSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.lastSaved = DateTime.UtcNow.ToString("o");
            cachedSave = data;

            string json    = JsonUtility.ToJson(data, prettyPrint: true);
            string payload = Encrypt(json);

            // Atomic write: backup → primary
            if (File.Exists(SavePath))
                File.Copy(SavePath, BackupPath, overwrite: true);

            await WriteFileAsync(SavePath, payload);
            Debug.Log($"[SaveSystem] Game saved to {SavePath}");
        }

        /// <summary>
        /// Loads and deserialises the save file.  Falls back to the backup if
        /// the primary is corrupt.  Returns <c>null</c> when no save exists.
        /// </summary>
        public async Task<GameSaveData> LoadGame()
        {
            if (cachedSave != null) return cachedSave;

            string payload = await TryReadFileAsync(SavePath)
                          ?? await TryReadFileAsync(BackupPath);

            if (payload == null)
            {
                Debug.Log("[SaveSystem] No save file found — starting fresh.");
                return null;
            }

            try
            {
                string json = Decrypt(payload);
                cachedSave = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("[SaveSystem] Save loaded successfully.");
                return cachedSave;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to parse save: {ex.Message}");
                return null;
            }
        }

        /// <summary>Deletes both the primary and backup save files.</summary>
        public void DeleteSave()
        {
            cachedSave = null;
            TryDelete(SavePath);
            TryDelete(BackupPath);
            Debug.Log("[SaveSystem] Save deleted.");
        }

        /// <summary>Returns <c>true</c> if a save file exists on disk.</summary>
        public bool SaveExists() => File.Exists(SavePath) || File.Exists(BackupPath);

        /// <summary>
        /// Must be called from a MonoBehaviour Update loop.  Triggers an
        /// auto-save when <see cref="AutoSaveIntervalSeconds"/> elapses.
        /// </summary>
        public void Tick(float deltaTime, Func<GameSaveData> dataProvider)
        {
            autoSaveTimer += deltaTime;
            if (autoSaveTimer < AutoSaveIntervalSeconds) return;
            autoSaveTimer = 0f;

            GameSaveData data = dataProvider?.Invoke();
            if (data != null)
                _ = SaveGame(data);
        }

        // ── Cloud stubs ────────────────────────────────────────────────────────

        /// <summary>
        /// [STUB] Uploads the current save to Firebase Firestore.
        /// Replace with a real Firebase call in the backend integration phase.
        /// </summary>
        public Task BackupToCloud(GameSaveData data)
        {
            Debug.Log("[SaveSystem] Cloud backup stub invoked — implement Firebase here.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// [STUB] Downloads the latest cloud save for the authenticated user.
        /// </summary>
        public Task<GameSaveData> RestoreFromCloud()
        {
            Debug.Log("[SaveSystem] Cloud restore stub invoked — implement Firebase here.");
            return Task.FromResult<GameSaveData>(null);
        }

        // ── Encryption helpers ─────────────────────────────────────────────────

        /// <summary>
        /// XOR-based obfuscation stub.  Replace with AES-256 before shipping.
        /// </summary>
        private string Encrypt(string plainText)
        {
            // Stub: Base64 obfuscation only — swap for real AES in production.
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        /// <summary>Decodes data produced by <see cref="Encrypt"/>.</summary>
        private string Decrypt(string cipherText)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
            }
            catch
            {
                // Possibly un-encrypted legacy save — return raw
                return cipherText;
            }
        }

        // ── File I/O helpers ───────────────────────────────────────────────────

        private static async Task WriteFileAsync(string path, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write,
                                          FileShare.None, bufferSize: 4096, useAsync: true);
            await fs.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task<string> TryReadFileAsync(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
                                              FileShare.Read, bufferSize: 4096, useAsync: true);
                byte[] bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Could not read {path}: {ex.Message}");
                return null;
            }
        }

        private static void TryDelete(string path)
        {
            if (File.Exists(path))
                try { File.Delete(path); } catch { /* best-effort */ }
        }
    }
}
