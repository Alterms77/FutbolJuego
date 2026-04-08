using System;
using System.Collections.Generic;

namespace FutbolJuego.Models
{
    /// <summary>Category of a trophy.</summary>
    public enum TrophyType { League, Cup }

    // ── TrophyData ─────────────────────────────────────────────────────────────

    /// <summary>
    /// A trophy earned by a team at the end of a league season or cup competition.
    /// </summary>
    [Serializable]
    public class TrophyData
    {
        /// <summary>Unique trophy GUID.</summary>
        public string id;
        /// <summary>Full trophy label (e.g. "Premier League – Season 1").</summary>
        public string name;
        /// <summary>Short UI name (e.g. "Premier League").</summary>
        public string displayName;
        /// <summary>Competition this trophy belongs to.</summary>
        public string competitionId;
        /// <summary>Team that won.</summary>
        public string teamId;
        /// <summary>In-game season number.</summary>
        public int season;
        /// <summary>League or Cup trophy.</summary>
        public TrophyType type;
        /// <summary>UTC date the trophy was awarded.</summary>
        public DateTime dateWon;
    }

    // ── CupData ────────────────────────────────────────────────────────────────

    /// <summary>
    /// State for a single-elimination knockout cup competition.
    /// Serialisable so it can be embedded in <c>competitions.json</c>.
    /// </summary>
    [Serializable]
    public class CupData
    {
        /// <summary>Unique cup identifier (e.g. "cup-carabao").</summary>
        public string id;
        /// <summary>Full cup name (e.g. "Carabao Cup").</summary>
        public string name;
        /// <summary>Parent league identifier.</summary>
        public string leagueId;
        /// <summary>Whether draws at 90 min are resolved by penalty shootout.</summary>
        public bool usePenaltiesOnDraw;
        /// <summary>Current round number (1-based).</summary>
        public int currentRound;
        /// <summary>Human-readable label for the current round (e.g. "Semi-finals").</summary>
        public string currentRoundName;
        /// <summary>In-game season this cup instance belongs to.</summary>
        public int season;
        /// <summary>All teams entered in this cup at the start.</summary>
        public List<string> participatingTeamIds = new List<string>();
        /// <summary>Teams still in the competition (eliminated teams removed).</summary>
        public List<string> remainingTeamIds = new List<string>();
        /// <summary>Fixtures for the current round (empty until generated).</summary>
        public List<FixtureData> currentRoundFixtures = new List<FixtureData>();
        /// <summary>Team ID of the cup winner; null until the final is played.</summary>
        public string winnerId;
    }
}
