using System;
using System.Collections.Generic;

namespace FutbolJuego.Models
{
    /// <summary>The decade in which the player was at their peak.</summary>
    public enum LegendEra
    {
        S1950s,
        S1960s,
        S1970s,
        S1980s,
        S1990s,
        S2000s,
        S2010s,
        S2020s
    }

    /// <summary>Tier of legend status (how iconic the player is).</summary>
    public enum LegendTier
    {
        /// <summary>Club legend — significant but primarily local fame.</summary>
        ClubIcon = 1,
        /// <summary>National legend — among the best of their country.</summary>
        NationalIcon = 2,
        /// <summary>World legend — one of the greatest in the history of the sport.</summary>
        WorldClass = 3,
        /// <summary>All-time greatest — Ballon d'Or winners, GOAT candidates.</summary>
        AllTimeGreat = 4
    }

    // ── LegendPlayerData ───────────────────────────────────────────────────────

    /// <summary>
    /// Data model for a retired or deceased football legend.
    /// Legends can be browsed in the Legends Hall screen and used in
    /// special card packs / team-building modes.
    /// </summary>
    [Serializable]
    public class LegendPlayerData
    {
        // ── Identity ───────────────────────────────────────────────────────────

        /// <summary>Unique legend identifier (e.g. "legend-maradona").</summary>
        public string id;
        /// <summary>Full display name.</summary>
        public string name;
        /// <summary>Birth year.</summary>
        public int birthYear;
        /// <summary>Death year; 0 if still alive (retired).</summary>
        public int deathYear;
        /// <summary>ISO 3166-1 alpha-2 nationality code.</summary>
        public string nationality;
        /// <summary>Primary playing position.</summary>
        public PlayerPosition position;
        /// <summary>Overall rating (peak career).</summary>
        public int overallRating;
        /// <summary>Decade of peak performance.</summary>
        public LegendEra era;
        /// <summary>How iconic this player is globally.</summary>
        public LegendTier tier;
        /// <summary>Name of the club most associated with this player.</summary>
        public string iconicClub;
        /// <summary>League the iconic club belongs to (matches league id).</summary>
        public string iconicLeagueId;
        /// <summary>Short biography / career highlight for the UI.</summary>
        public string legacyDescription;
        /// <summary>In-game acquisition cost in premium coins.</summary>
        public int premiumCoinCost;

        // ── Peak attributes ────────────────────────────────────────────────────

        /// <summary>Peak career attributes (used if the legend is added to a squad).</summary>
        public PlayerAttributes attributes = new PlayerAttributes();

        // ── Computed helpers ───────────────────────────────────────────────────

        /// <summary>Returns <c>true</c> if the player is deceased.</summary>
        public bool IsDeceased => deathYear > 0;

        /// <summary>Returns a formatted lifespan string (e.g. "1960–2021").</summary>
        public string LifespanLabel => IsDeceased
            ? $"{birthYear}–{deathYear}"
            : $"b. {birthYear}";
    }
}
