using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Tracks and awards trophies for league and cup victories.
    /// Registered as a service via <see cref="Core.ServiceLocator"/>.
    /// </summary>
    public class TrophySystem
    {
        private readonly List<TrophyData> trophies = new List<TrophyData>();

        // ── League trophies ────────────────────────────────────────────────────

        /// <summary>
        /// Awards the league title to the team at the top of
        /// <paramref name="league"/>'s table.  Returns the created trophy, or
        /// <c>null</c> if the table is empty.
        /// </summary>
        public TrophyData AwardLeagueTrophy(LeagueData league, int season)
        {
            if (league?.table == null || league.table.Count == 0) return null;

            var sorted = league.table
                .OrderByDescending(e => e.points)
                .ThenByDescending(e => e.goalDifference)
                .ThenByDescending(e => e.goalsFor)
                .ToList();

            string winnerId = sorted[0].teamId;

            var trophy = new TrophyData
            {
                id            = Guid.NewGuid().ToString(),
                name          = $"{league.name} – Season {season}",
                displayName   = league.name,
                competitionId = league.id,
                teamId        = winnerId,
                season        = season,
                type          = TrophyType.League,
                dateWon       = DateTime.UtcNow
            };

            trophies.Add(trophy);
            GD.Print($"[TrophySystem] {league.name} Season {season} won by {winnerId}.");
            return trophy;
        }

        // ── Cup trophies ───────────────────────────────────────────────────────

        /// <summary>
        /// Awards the cup trophy to <see cref="CupData.winnerId"/>.
        /// Returns <c>null</c> if the cup has no winner yet.
        /// </summary>
        public TrophyData AwardCupTrophy(CupData cup)
        {
            if (cup == null || string.IsNullOrEmpty(cup.winnerId)) return null;

            var trophy = new TrophyData
            {
                id            = Guid.NewGuid().ToString(),
                name          = $"{cup.name} – Season {cup.season}",
                displayName   = cup.name,
                competitionId = cup.id,
                teamId        = cup.winnerId,
                season        = cup.season,
                type          = TrophyType.Cup,
                dateWon       = DateTime.UtcNow
            };

            trophies.Add(trophy);
            GD.Print($"[TrophySystem] {cup.name} Season {cup.season} won by {cup.winnerId}.");
            return trophy;
        }

        // ── Queries ────────────────────────────────────────────────────────────

        /// <summary>Returns all trophies won by <paramref name="teamId"/>.</summary>
        public List<TrophyData> GetTeamTrophies(string teamId)
            => trophies.Where(t => t.teamId == teamId).ToList();

        /// <summary>Returns a snapshot of all trophies (all teams).</summary>
        public List<TrophyData> GetAllTrophies()
            => new List<TrophyData>(trophies);

        // ── Persistence support ────────────────────────────────────────────────

        /// <summary>
        /// Restores the trophy list from a saved game snapshot.
        /// </summary>
        public void LoadTrophies(List<TrophyData> savedTrophies)
        {
            trophies.Clear();
            if (savedTrophies != null)
                trophies.AddRange(savedTrophies);
        }
    }
}
