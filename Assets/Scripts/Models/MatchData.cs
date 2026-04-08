using System;
using System.Collections.Generic;

namespace FutbolJuego.Models
{
    // ── Enumerations ───────────────────────────────────────────────────────────

    /// <summary>Lifecycle status of a match.</summary>
    public enum MatchStatus { Scheduled, InProgress, Completed, Postponed }

    /// <summary>Competition type of a match.</summary>
    public enum MatchType { League, Cup, Friendly }

    /// <summary>Discrete events that can occur during a match.</summary>
    public enum MatchEventType
    {
        Goal,
        YellowCard,
        RedCard,
        Substitution,
        Injury,
        MissedPenalty,
        Save,
        /// <summary>A penalty kick scored during a shootout (not counted in 90-min score).</summary>
        PenaltyGoal
    }

    // ── MatchData ──────────────────────────────────────────────────────────────

    /// <summary>Complete record for a single match: result, events, statistics.</summary>
    [Serializable]
    public class MatchData
    {
        /// <summary>Unique match GUID.</summary>
        public string id;
        /// <summary>Home team identifier.</summary>
        public string homeTeamId;
        /// <summary>Away team identifier.</summary>
        public string awayTeamId;
        /// <summary>Home team goals at full-time (or current minute).</summary>
        public int homeScore;
        /// <summary>Away team goals at full-time (or current minute).</summary>
        public int awayScore;
        /// <summary>Scheduled kick-off date/time (UTC).</summary>
        public DateTime matchDate;
        /// <summary>Current match lifecycle status.</summary>
        public MatchStatus status;
        /// <summary>Aggregate statistics for the match.</summary>
        public MatchStatistics statistics = new MatchStatistics();
        /// <summary>Timeline of discrete events.</summary>
        public List<MatchEvent> events = new List<MatchEvent>();
        /// <summary>Simulated game minute (0-90+).</summary>
        public int currentMinute;
        /// <summary>Competition type (league, cup, friendly).</summary>
        public MatchType matchType;
        /// <summary>Parent competition identifier.</summary>
        public string competitionId;
        /// <summary>Whether the match was decided by a penalty shootout.</summary>
        public bool wentToPenalties;
        /// <summary>Penalty shootout result; non-null only when <see cref="wentToPenalties"/> is true.</summary>
        public PenaltyShootoutData penaltyShootout;
    }

    // ── MatchStatistics ────────────────────────────────────────────────────────

    /// <summary>Aggregate in-match statistics for both teams.</summary>
    [Serializable]
    public class MatchStatistics
    {
        /// <summary>Expected goals — home team.</summary>
        public float homeXG;
        /// <summary>Expected goals — away team.</summary>
        public float awayXG;
        /// <summary>Total shots — home.</summary>
        public int homeShots;
        /// <summary>Total shots — away.</summary>
        public int awayShots;
        /// <summary>Shots on target — home.</summary>
        public int homeShotsOnTarget;
        /// <summary>Shots on target — away.</summary>
        public int awayShotsOnTarget;
        /// <summary>Ball possession percentage — home (0-100).</summary>
        public float homePossession;
        /// <summary>Ball possession percentage — away (0-100).</summary>
        public float awayPossession;
        /// <summary>Passes completed — home.</summary>
        public int homePasses;
        /// <summary>Passes completed — away.</summary>
        public int awayPasses;
        /// <summary>Fouls committed — home.</summary>
        public int homeFouls;
        /// <summary>Fouls committed — away.</summary>
        public int awayFouls;
        /// <summary>Yellow cards — home.</summary>
        public int homeYellowCards;
        /// <summary>Yellow cards — away.</summary>
        public int awayYellowCards;
        /// <summary>Red cards — home.</summary>
        public int homeRedCards;
        /// <summary>Red cards — away.</summary>
        public int awayRedCards;
        /// <summary>Corner kicks — home.</summary>
        public int homeCorners;
        /// <summary>Corner kicks — away.</summary>
        public int awayCorners;
    }

    // ── MatchEvent ─────────────────────────────────────────────────────────────

    /// <summary>A single timestamped event during a match.</summary>
    [Serializable]
    public class MatchEvent
    {
        /// <summary>Match minute the event occurred (1-90+).</summary>
        public int minute;
        /// <summary>Category of the event.</summary>
        public MatchEventType type;
        /// <summary>Primary player involved (scorer, carded player, etc.).</summary>
        public string playerId;
        /// <summary>Player providing the assist (Goal events only).</summary>
        public string assistPlayerId;
        /// <summary>Team the primary player belongs to.</summary>
        public string teamId;
        /// <summary>Human-readable description for the feed.</summary>
        public string description;
    }

    // ── PenaltyShootoutData ────────────────────────────────────────────────────

    /// <summary>Full record of a penalty-shootout that followed a draw at 90 minutes.</summary>
    [Serializable]
    public class PenaltyShootoutData
    {
        /// <summary>Team ID of the shootout winner.</summary>
        public string winnerTeamId;
        /// <summary>Goals scored by the home team in the shootout.</summary>
        public int homeScore;
        /// <summary>Goals scored by the away team in the shootout.</summary>
        public int awayScore;
        /// <summary>Ordered list of all penalty kicks taken.</summary>
        public List<PenaltyKick> kicks = new List<PenaltyKick>();
    }

    /// <summary>A single penalty kick taken during a shootout.</summary>
    [Serializable]
    public class PenaltyKick
    {
        /// <summary>Team the kicker plays for.</summary>
        public string teamId;
        /// <summary>Kicker player identifier.</summary>
        public string playerId;
        /// <summary>Kicker display name (cached for UI).</summary>
        public string playerName;
        /// <summary>Whether the kick resulted in a goal.</summary>
        public bool scored;
        /// <summary>Human-readable description for the feed.</summary>
        public string description;
    }
}
