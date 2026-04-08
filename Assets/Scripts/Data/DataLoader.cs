using System.Collections.Generic;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Data
{
    /// <summary>
    /// Loads serialised game data from the Unity Resources/Data/ folder
    /// and deserialises it into model objects.
    /// </summary>
    public class DataLoader
    {
        private const string TeamsPath       = "Data/teams";
        private const string PlayersPath     = "Data/players";
        private const string LeaguePath      = "Data/competitions";
        private const string FormationsPath  = "Data/formations";

        // ── Single object loading ──────────────────────────────────────────────

        /// <summary>Deserialises a <see cref="TeamData"/> from a JSON string.</summary>
        public static TeamData LoadTeamFromJSON(string json)
            => JsonHandler.Deserialize<TeamData>(json);

        /// <summary>Deserialises a <see cref="PlayerData"/> from a JSON string.</summary>
        public static PlayerData LoadPlayerFromJSON(string json)
            => JsonHandler.Deserialize<PlayerData>(json);

        /// <summary>Deserialises a <see cref="LeagueData"/> from a JSON string.</summary>
        public static LeagueData LoadLeagueFromJSON(string json)
            => JsonHandler.Deserialize<LeagueData>(json);

        // ── Bulk loading from Resources ────────────────────────────────────────

        /// <summary>
        /// Loads all teams defined in <c>Resources/Data/teams.json</c>.
        /// Returns an empty list if the file is missing or malformed.
        /// </summary>
        public static List<TeamData> LoadAllTeams()
        {
            var wrapper = JsonHandler.LoadFromResources<TeamListWrapper>(TeamsPath);
            return wrapper?.teams ?? new List<TeamData>();
        }

        /// <summary>
        /// Loads all players defined in <c>Resources/Data/players.json</c>.
        /// </summary>
        public static List<PlayerData> LoadAllPlayers()
        {
            var wrapper = JsonHandler.LoadFromResources<PlayerListWrapper>(PlayersPath);
            return wrapper?.players ?? new List<PlayerData>();
        }

        /// <summary>
        /// Loads the first league defined in <c>Resources/Data/competitions.json</c>.
        /// </summary>
        public static LeagueData LoadDefaultLeague()
        {
            var wrapper = JsonHandler.LoadFromResources<LeagueListWrapper>(LeaguePath);
            return wrapper?.leagues?.Count > 0 ? wrapper.leagues[0] : null;
        }

        // ── Wrapper types for JSON arrays ──────────────────────────────────────

        [System.Serializable]
        private class TeamListWrapper
        {
            public List<TeamData> teams;
        }

        [System.Serializable]
        private class PlayerListWrapper
        {
            public List<PlayerData> players;
        }

        [System.Serializable]
        private class LeagueListWrapper
        {
            public List<LeagueData> leagues;
        }
    }
}
