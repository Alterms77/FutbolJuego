using System;

namespace FutbolJuego.Models
{
    /// <summary>
    /// Supplementary display metadata for a league, loaded from
    /// <c>Resources/Data/leagues.json</c>.  Not used in match simulation —
    /// only for UI rendering (league-selection screen, difficulty indicators, etc.).
    /// </summary>
    [Serializable]
    public class LeagueMetadata
    {
        /// <summary>Matches the id in competitions.json / teams.json.</summary>
        public string id;
        /// <summary>Full league display name.</summary>
        public string name;
        /// <summary>Country name in Spanish (for UI).</summary>
        public string country;
        /// <summary>ISO 3166-1 alpha-2 country code.</summary>
        public string countryCode;
        /// <summary>Difficulty rating 1-5 (5 = hardest).</summary>
        public int difficulty;
        /// <summary>Global reputation 0-100.</summary>
        public int reputation;
        /// <summary>Descriptive play-style label (e.g. "Posesión-Técnico").</summary>
        public string playStyle;
        /// <summary>Average team transfer budget in the game's currency unit.</summary>
        public int averageBudget;
    }
}
