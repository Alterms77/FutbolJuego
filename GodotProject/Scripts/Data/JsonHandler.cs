using Godot;
using System;
using System.Text.Json;

namespace FutbolJuego.Data
{
    /// <summary>
    /// Thin wrapper around System.Text.Json providing generic
    /// serialise / deserialise helpers and a Resource-load shortcut.
    /// </summary>
    public static class JsonHandler
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        /// <summary>
        /// Deserialises <paramref name="json"/> into an object of type
        /// <typeparamref name="T"/> using System.Text.Json.
        /// Returns <c>default</c> on error.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                GD.PushWarning("[JsonHandler] Deserialize received null/empty JSON.");
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, Options);
            }
            catch (Exception ex)
            {
                GD.PushError($"[JsonHandler] Deserialize failed for {typeof(T).Name}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Serialises <paramref name="obj"/> to a JSON string.
        /// Uses pretty-print by default for readability.
        /// </summary>
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
            {
                GD.PushWarning("[JsonHandler] Serialize received null object.");
                return "{}";
            }
            return JsonSerializer.Serialize(obj, Options);
        }

        /// <summary>
        /// Performs a rudimentary JSON validation check (non-empty and starts
        /// with '{'  or '[').  For production use a full JSON parser.
        /// </summary>
        public static bool ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            string trimmed = json.TrimStart();
            return trimmed.StartsWith("{") || trimmed.StartsWith("[");
        }

        /// <summary>
        /// Loads a JSON file from <c>res://Data/{filename}.json</c>
        /// and deserialises its text as <typeparamref name="T"/>.
        /// Returns <c>default</c> if the file is not found.
        /// </summary>
        public static T LoadFromResources<T>(string path)
        {
            string fullPath = $"res://Data/{System.IO.Path.GetFileName(path)}.json";
            if (!FileAccess.FileExists(fullPath))
            {
                GD.PushWarning($"[JsonHandler] Resource not found at path: {fullPath}");
                return default;
            }
            using var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            return Deserialize<T>(json);
        }
    }
}
