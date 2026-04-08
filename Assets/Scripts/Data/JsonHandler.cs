using UnityEngine;

namespace FutbolJuego.Data
{
    /// <summary>
    /// Thin wrapper around <see cref="JsonUtility"/> providing generic
    /// serialise / deserialise helpers and a Resource-load shortcut.
    /// </summary>
    public static class JsonHandler
    {
        /// <summary>
        /// Deserialises <paramref name="json"/> into an object of type
        /// <typeparamref name="T"/> using Unity's JsonUtility.
        /// Returns <c>default</c> on error.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[JsonHandler] Deserialize received null/empty JSON.");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JsonHandler] Deserialize failed for {typeof(T).Name}: {ex.Message}");
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
                Debug.LogWarning("[JsonHandler] Serialize received null object.");
                return "{}";
            }
            return JsonUtility.ToJson(obj, prettyPrint: true);
        }

        /// <summary>
        /// Performs a rudimentary JSON validation check (non-empty and starts
        /// with '{' or '[').  For production use a full JSON parser.
        /// </summary>
        public static bool ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            string trimmed = json.TrimStart();
            return trimmed.StartsWith("{") || trimmed.StartsWith("[");
        }

        /// <summary>
        /// Loads a TextAsset from <c>Resources/<paramref name="path"/></c>
        /// and deserialises its text as <typeparamref name="T"/>.
        /// Returns <c>default</c> if the asset is not found.
        /// </summary>
        public static T LoadFromResources<T>(string path)
        {
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[JsonHandler] Resource not found at path: {path}");
                return default;
            }
            return Deserialize<T>(asset.text);
        }
    }
}
