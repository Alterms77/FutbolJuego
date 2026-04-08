using System;
using System.Collections.Generic;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Manages dynamic player rating changes driven by match performance.
    ///
    /// <list type="bullet">
    ///   <item>After each match, <see cref="UpdateRatingsAfterMatch"/> scans
    ///         the match events to credit goals/saves to individual players,
    ///         then applies small attribute adjustments proportional to how
    ///         far the player's performance was from the benchmark.</item>
    ///   <item>Ratings are always clamped to the bounds defined by the player's
    ///         <see cref="PlayerRatingCategory"/> (via
    ///         <see cref="PlayerRatingCategoryExtensions"/>).</item>
    ///   <item>The <see cref="PlayerRarity"/> is kept in sync with the new overall
    ///         via <see cref="SyncRarity"/>.</item>
    /// </list>
    ///
    /// League-tier multipliers for market-value calculation are also held here.
    /// Registered in <see cref="Core.GameManager"/> via
    /// <see cref="Core.ServiceLocator"/>.
    /// </summary>
    public class PlayerRatingSystem
    {
        // ── League value multipliers ───────────────────────────────────────────

        /// <summary>
        /// Multiplier applied to the base market value for each league.
        /// Higher-reputation leagues increase player values; lower leagues reduce them.
        /// </summary>
        private static readonly Dictionary<string, float> LeagueValueMultipliers =
            new Dictionary<string, float>
            {
                { "league-premier",     1.40f },
                { "league-laliga",      1.35f },
                { "league-seriea",      1.20f },
                { "league-brasileirao", 0.75f },
                { "league-liga-mx",     0.65f },
            };

        // ── Post-match rating update ───────────────────────────────────────────

        /// <summary>
        /// Updates every player in <paramref name="team"/> based on their individual
        /// contribution to <paramref name="match"/>.
        ///
        /// Events in <paramref name="match"/> are scanned for goals and saves to
        /// accumulate <see cref="PerformanceStats"/> for the match.  Then
        /// <see cref="ApplyPerformanceDelta"/> is called for each player, the
        /// category ceiling/floor is enforced, and <see cref="SyncRarity"/> is applied.
        ///
        /// <para>
        /// Only call this for the <em>player-managed</em> team; AI teams receive
        /// seasonal progression via <see cref="Models.PlayerData.SimulateGrowth"/>.
        /// </para>
        /// </summary>
        /// <param name="team">The managed team whose squad should be updated.</param>
        /// <param name="match">The completed match record.</param>
        /// <param name="isHomeTeam">Whether <paramref name="team"/> was the home side.</param>
        public void UpdateRatingsAfterMatch(TeamData team, MatchData match, bool isHomeTeam)
        {
            if (team?.squad == null || match == null) return;

            bool wonMatch  = isHomeTeam
                ? match.homeScore > match.awayScore
                : match.awayScore > match.homeScore;
            bool cleanSheet = isHomeTeam
                ? match.awayScore == 0
                : match.homeScore == 0;

            // Build per-player stat snapshots from match events
            var goalCountById  = new Dictionary<string, int>();
            var saveCountById  = new Dictionary<string, int>();
            var yellowById     = new Dictionary<string, int>();
            var redById        = new Dictionary<string, int>();

            foreach (var evt in match.events)
            {
                if (evt.teamId != team.id) continue;

                switch (evt.type)
                {
                    case MatchEventType.Goal:
                        Increment(goalCountById, evt.playerId);
                        break;
                    case MatchEventType.Save:
                        Increment(saveCountById, evt.playerId);
                        break;
                    case MatchEventType.YellowCard:
                        Increment(yellowById, evt.playerId);
                        break;
                    case MatchEventType.RedCard:
                        Increment(redById, evt.playerId);
                        break;
                }
            }

            foreach (var player in team.squad)
            {
                if (player == null) continue;

                // Accumulate into season stats
                var stats = player.seasonStats ??= new PerformanceStats();
                stats.matchesPlayed++;
                stats.minutesPlayed += 90; // approximation for non-subbed players

                if (goalCountById.TryGetValue(player.id, out int playerGoals))
                    stats.goals += playerGoals;

                if (saveCountById.TryGetValue(player.id, out int playerSaves))
                    stats.savesMade += playerSaves;

                if (yellowById.TryGetValue(player.id, out int yellows))
                    stats.yellowCards += yellows;

                if (redById.TryGetValue(player.id, out int reds))
                    stats.redCards += reds;

                if (cleanSheet && (player.position == PlayerPosition.GK ||
                                   player.position == PlayerPosition.CB  ||
                                   player.position == PlayerPosition.LB  ||
                                   player.position == PlayerPosition.RB))
                    stats.cleanSheets++;

                // Derive the rating change for this match only
                float delta = ComputeMatchDelta(player, goalCountById, saveCountById,
                                                redById, cleanSheet, wonMatch);
                ApplyPerformanceDelta(player, delta);
            }
        }

        // ── Attribute adjustment ───────────────────────────────────────────────

        /// <summary>
        /// Adjusts the relevant attributes of <paramref name="player"/> by a small
        /// amount proportional to <paramref name="delta"/> (range [–1, +1]).
        ///
        /// The maximum attribute change per match is ±0.5 points so that ratings
        /// only shift meaningfully over a full season of performances.
        /// </summary>
        public void ApplyPerformanceDelta(PlayerData player, float delta)
        {
            if (player == null) return;

            // Max ±0.5 attribute change per match for regular players;
            // legend tiers are less volatile (±0.2).
            float maxChange = player.ratingCategory == PlayerRatingCategory.Regular
                ? 0.5f : 0.2f;

            float change = Mathf.Clamp(delta, -1f, 1f) * maxChange;
            int   iChange = Mathf.RoundToInt(change);

            if (iChange == 0) return;

            ApplyPositionSpecificChange(player, iChange);
            EnforceRatingBounds(player);
            SyncRarity(player);
        }

        // ── Ceiling / floor enforcement ────────────────────────────────────────

        /// <summary>
        /// Clamps <see cref="PlayerData.overallRating"/> and all attributes to
        /// the floor and ceiling defined by the player's
        /// <see cref="PlayerRatingCategory"/>.  Also recalculates the cached
        /// <see cref="PlayerData.overallRating"/> via
        /// <see cref="PlayerData.CalculateOverall"/>.
        /// </summary>
        public void EnforceRatingBounds(PlayerData player)
        {
            if (player == null) return;

            int ceiling = player.ratingCategory.GetCeiling();
            int floor   = player.ratingCategory.GetFloor();

            // Clamp every attribute to [0, ceiling] so the weighted overall
            // can never exceed the category ceiling.
            int attrCeil = Mathf.Min(ceiling, 99);
            player.attributes.speed        = Mathf.Clamp(player.attributes.speed,        0, attrCeil);
            player.attributes.shooting     = Mathf.Clamp(player.attributes.shooting,     0, attrCeil);
            player.attributes.passing      = Mathf.Clamp(player.attributes.passing,      0, attrCeil);
            player.attributes.defense      = Mathf.Clamp(player.attributes.defense,      0, attrCeil);
            player.attributes.physical     = Mathf.Clamp(player.attributes.physical,     0, attrCeil);
            player.attributes.intelligence = Mathf.Clamp(player.attributes.intelligence, 0, attrCeil);
            player.attributes.goalkeeping  = Mathf.Clamp(player.attributes.goalkeeping,  0, attrCeil);

            player.CalculateOverall();
            player.overallRating = Mathf.Clamp(player.overallRating, floor, ceiling);
        }

        // ── Rarity sync ────────────────────────────────────────────────────────

        /// <summary>
        /// Updates <see cref="PlayerData.rarity"/> to match the current
        /// <see cref="PlayerData.overallRating"/>.
        /// <list type="bullet">
        ///   <item>≤69   → <see cref="PlayerRarity.Normal"/></item>
        ///   <item>70-79 → <see cref="PlayerRarity.Silver"/></item>
        ///   <item>80-84 → <see cref="PlayerRarity.Gold"/></item>
        ///   <item>85-89 → <see cref="PlayerRarity.Star"/></item>
        ///   <item>90-94 → <see cref="PlayerRarity.Legend"/></item>
        ///   <item>≥95   → <see cref="PlayerRarity.AllTimeGreat"/></item>
        /// </list>
        /// </summary>
        public static void SyncRarity(PlayerData player)
        {
            if (player == null) return;
            int ovr = player.overallRating;
            player.rarity = ovr switch
            {
                >= 95 => PlayerRarity.AllTimeGreat,
                >= 90 => PlayerRarity.Legend,
                >= 85 => PlayerRarity.Star,
                >= 80 => PlayerRarity.Gold,
                >= 70 => PlayerRarity.Silver,
                _     => PlayerRarity.Normal
            };
        }

        // ── Market value ───────────────────────────────────────────────────────

        /// <summary>
        /// Calculates a player's market value using overall rating, age, potential,
        /// and an optional league-tier multiplier.
        ///
        /// Base curve: OVR 60 ≈ €1 M, OVR 85 ≈ €50 M, OVR 99+ ≈ €200 M+.
        /// </summary>
        /// <param name="player">The player to value.</param>
        /// <param name="leagueId">
        /// Optional league identifier.  If null, the neutral multiplier (1.0) is used.
        /// </param>
        public long CalculateMarketValue(PlayerData player, string leagueId = null)
        {
            if (player == null) return 0;

            int overall = player.CalculateOverall();

            // Base exponential curve
            float baseValue = Mathf.Pow(1.12f, overall - 50) * 500_000f;

            // Age multiplier
            float ageMult = player.age switch
            {
                < 20 => 1.30f,
                < 23 => 1.15f,
                < 27 => 1.05f,
                < 30 => 1.00f,
                < 33 => 0.85f,
                < 35 => 0.65f,
                _    => 0.40f
            };

            // Potential bonus
            float potBonus = 1.0f + Mathf.Max(0f, player.potential - overall) * 0.005f;

            // Category premium: legend tiers carry a prestige premium
            float catBonus = player.ratingCategory switch
            {
                PlayerRatingCategory.HistoricLegend => 3.0f,
                PlayerRatingCategory.ModernLegend   => 1.8f,
                _                                   => 1.0f
            };

            // League multiplier
            float leagueMult = 1.0f;
            if (!string.IsNullOrEmpty(leagueId) &&
                LeagueValueMultipliers.TryGetValue(leagueId, out float lm))
                leagueMult = lm;

            long value = (long)(baseValue * ageMult * potBonus * catBonus * leagueMult);

            // Update cached field
            player.marketValue = value;
            return value;
        }

        /// <summary>Returns the league multiplier for the given league id (1.0 if unknown).</summary>
        public static float GetLeagueMultiplier(string leagueId)
        {
            if (!string.IsNullOrEmpty(leagueId) &&
                LeagueValueMultipliers.TryGetValue(leagueId, out float m))
                return m;
            return 1.0f;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Computes the single-match performance delta for <paramref name="player"/>.
        /// </summary>
        private static float ComputeMatchDelta(
            PlayerData                 player,
            Dictionary<string, int>    goalCountById,
            Dictionary<string, int>    saveCountById,
            Dictionary<string, int>    redById,
            bool                       cleanSheet,
            bool                       wonMatch)
        {
            goalCountById.TryGetValue(player.id, out int goals);
            saveCountById.TryGetValue(player.id, out int saves);
            redById.TryGetValue(player.id, out int reds);

            float delta = 0f;

            switch (player.position)
            {
                case PlayerPosition.GK:
                    delta += saves * 0.08f;
                    if (cleanSheet) delta += 0.25f;
                    delta -= reds   * 0.30f;
                    break;

                case PlayerPosition.CB:
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    if (cleanSheet) delta += 0.20f;
                    delta += goals * 0.15f;
                    delta -= reds  * 0.25f;
                    break;

                case PlayerPosition.CDM:
                case PlayerPosition.CM:
                    delta += goals * 0.15f;
                    if (wonMatch) delta += 0.05f;
                    delta -= reds  * 0.20f;
                    break;

                case PlayerPosition.CAM:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    delta += goals * 0.20f;
                    if (wonMatch) delta += 0.05f;
                    delta -= reds  * 0.20f;
                    break;

                case PlayerPosition.CF:
                case PlayerPosition.ST:
                    delta += goals * 0.25f;
                    if (wonMatch) delta += 0.05f;
                    delta -= reds  * 0.15f;
                    break;
            }

            // Small general morale boost / penalty for win / loss
            delta += wonMatch ? 0.03f : -0.03f;

            return Mathf.Clamp(delta, -1f, 1f);
        }

        /// <summary>Bumps the most relevant attribute(s) by <paramref name="change"/> points.</summary>
        private static void ApplyPositionSpecificChange(PlayerData player, int change)
        {
            var a = player.attributes;
            switch (player.position)
            {
                case PlayerPosition.GK:
                    a.goalkeeping  = Mathf.Clamp(a.goalkeeping  + change, 0, 99);
                    break;
                case PlayerPosition.CB:
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    a.defense      = Mathf.Clamp(a.defense      + change, 0, 99);
                    break;
                case PlayerPosition.CDM:
                case PlayerPosition.CM:
                    a.passing      = Mathf.Clamp(a.passing      + change, 0, 99);
                    a.intelligence = Mathf.Clamp(a.intelligence + change, 0, 99);
                    break;
                case PlayerPosition.CAM:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                    a.passing      = Mathf.Clamp(a.passing      + change, 0, 99);
                    a.shooting     = Mathf.Clamp(a.shooting     + change, 0, 99);
                    break;
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    a.speed        = Mathf.Clamp(a.speed        + change, 0, 99);
                    a.shooting     = Mathf.Clamp(a.shooting     + change, 0, 99);
                    break;
                case PlayerPosition.CF:
                case PlayerPosition.ST:
                    a.shooting     = Mathf.Clamp(a.shooting     + change, 0, 99);
                    a.physical     = Mathf.Clamp(a.physical     + change, 0, 99);
                    break;
            }
        }

        private static void Increment(Dictionary<string, int> dict, string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            dict.TryGetValue(key, out int v);
            dict[key] = v + 1;
        }
    }
}
