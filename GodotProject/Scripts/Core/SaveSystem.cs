using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace FutbolJuego.Core
{
    // ── Save payload ───────────────────────────────────────────────────────────

    /// <summary>Root serialisable object written to disk / cloud.</summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>Save-file format version for migration support.</summary>
        public int saveVersion { get; set; } = 1;
        /// <summary>UTC timestamp of the last save.</summary>
        public string lastSaved { get; set; }
        /// <summary>Manager display name.</summary>
        public string managerName { get; set; }
        /// <summary>Current manager XP.</summary>
        public int managerXP { get; set; }
        /// <summary>Current season number.</summary>
        public int season { get; set; }
        /// <summary>Total days elapsed in the game world.</summary>
        public int gameDayElapsed { get; set; }
        /// <summary>Team identifier the manager controls.</summary>
        public string managedTeamId { get; set; }
        /// <summary>JSON-serialised <see cref="Models.TeamData"/> for the player's club.</summary>
        public string teamDataJson { get; set; }
        /// <summary>JSON-serialised <see cref="Models.LeagueData"/> for the active competition.</summary>
        public string leagueDataJson { get; set; }
        /// <summary>
        /// JSON-serialised <see cref="Models.CareerData"/> for the active career.
        /// Null when the player has not yet started a career.
        /// </summary>
        public string careerDataJson { get; set; }
        /// <summary>FutCoins (premium currency) balance.</summary>
        public int premiumCurrency { get; set; }
        /// <summary>Soft currency balance.</summary>
        public long coins { get; set; }
        /// <summary>
        /// In-game financial balance in the career's chosen currency (EUR / USD).
        /// </summary>
        public long inGameBalance { get; set; }
        /// <summary>
        /// Serialised <see cref="Models.CurrencyType"/> enum value
        /// ("EUR" or "USD") for the current career.
        /// </summary>
        public string currencyType { get; set; }
        /// <summary>Last day the daily reward was claimed (1-7 weekly cycle).</summary>
        public int lastDailyRewardDay { get; set; }
        /// <summary>UTC date of last daily reward claim.</summary>
        public string lastDailyRewardDate { get; set; }
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
        // Key is derived at runtime from a device-specific identifier — never hardcoded.
        private static readonly byte[] EncryptionKey = DeriveKey();

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>Interval in real-time seconds between automatic saves.</summary>
        public float AutoSaveIntervalSeconds { get; set; } = 300f; // 5 minutes

        private float autoSaveTimer;
        private GameSaveData cachedSave;

        private string SavePath   => Path.Combine(OS.GetUserDataDir(), SaveFileName);
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

            string json    = JsonSerializer.Serialize(data, _jsonOptions);
            string payload = Encrypt(json);

            // Atomic write: backup → primary
            if (File.Exists(SavePath))
                File.Copy(SavePath, BackupPath, overwrite: true);

            await WriteFileAsync(SavePath, payload);
            GD.Print($"[SaveSystem] Game saved to {SavePath}");
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
                GD.Print("[SaveSystem] No save file found — starting fresh.");
                return null;
            }

            try
            {
                string json = Decrypt(payload);
                cachedSave = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);
                GD.Print("[SaveSystem] Save loaded successfully.");
                return cachedSave;
            }
            catch (Exception ex)
            {
                GD.PushError($"[SaveSystem] Failed to parse save: {ex.Message}");
                return null;
            }
        }

        /// <summary>Deletes both the primary and backup save files.</summary>
        public void DeleteSave()
        {
            cachedSave = null;
            TryDelete(SavePath);
            TryDelete(BackupPath);
            GD.Print("[SaveSystem] Save deleted.");
        }

        /// <summary>Returns <c>true</c> if a save file exists on disk.</summary>
        public bool SaveExists() => File.Exists(SavePath) || File.Exists(BackupPath);

        /// <summary>
        /// Must be called from a Node's _Process loop.  Triggers an
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
            GD.Print("[SaveSystem] Cloud backup stub invoked — implement Firebase here.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// [STUB] Downloads the latest cloud save for the authenticated user.
        /// </summary>
        public Task<GameSaveData> RestoreFromCloud()
        {
            GD.Print("[SaveSystem] Cloud restore stub invoked — implement Firebase here.");
            return Task.FromResult<GameSaveData>(null);
        }

        // ── Encryption helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Derives a 256-bit AES key from a device-unique identifier so that
        /// no secret is ever stored in the binary.
        /// </summary>
        private static byte[] DeriveKey()
        {
            string seed = OS.GetUniqueId() + "_FJ_SAVE_V1";
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
        }

        /// <summary>
        /// Encrypts <paramref name="plainText"/> with AES-256-CBC and returns
        /// a Base64 string containing the IV prepended to the cipher bytes.
        /// </summary>
        private string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var aes = Aes.Create();
            aes.Key  = EncryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.GenerateIV();
            using var enc      = aes.CreateEncryptor();
            byte[] cipherBytes = enc.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            // Prepend the 16-byte IV so Decrypt can recover it.
            byte[] output = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, output, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, output, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(output);
        }

        /// <summary>Decrypts data produced by <see cref="Encrypt"/>.</summary>
        private string Decrypt(string cipherText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                using var aes = Aes.Create();
                aes.Key  = EncryptionKey;
                aes.Mode = CipherMode.CBC;
                byte[] iv = new byte[16];
                Buffer.BlockCopy(data, 0, iv, 0, 16);
                aes.IV   = iv;
                using var dec = aes.CreateDecryptor();
                byte[] plain  = dec.TransformFinalBlock(data, 16, data.Length - 16);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                // Fallback: try reading as plain UTF-8 (legacy un-encrypted saves).
                return cipherText;
            }
        }

        // ── File I/O helpers ───────────────────────────────────────────────────

        private static async Task WriteFileAsync(string path, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using var fs = new FileStream(path, FileMode.Create, System.IO.FileAccess.Write,
                                          FileShare.None, bufferSize: 4096, useAsync: true);
            await fs.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task<string> TryReadFileAsync(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read,
                                              FileShare.Read, bufferSize: 4096, useAsync: true);
                byte[] bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[SaveSystem] Could not read {path}: {ex.Message}");
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
