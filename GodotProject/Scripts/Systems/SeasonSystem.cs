using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Result returned by <see cref="SeasonSystem.AdvanceSeason"/> describing
    /// what happened to a single player at season end.
    /// </summary>
    public class PlayerSeasonEndResult
    {
        /// <summary>The player this result describes.</summary>
        public PlayerData Player;
        /// <summary>Age before advancement.</summary>
        public int AgeBefore;
        /// <summary>Age after advancement.</summary>
        public int AgeAfter;
        /// <summary>Overall rating before advancement.</summary>
        public int OverallBefore;
        /// <summary>Overall rating after advancement.</summary>
        public int OverallAfter;
        /// <summary>Whether the player is now retirable (age ≥ 35).</summary>
        public bool IsRetirable;
        /// <summary>Whether the player's contract expired this season.</summary>
        public bool ContractExpired;
    }

    /// <summary>
    /// Handles all logic that occurs at the end of each in-game season:
    /// <list type="bullet">
    ///   <item>Ages every player in the managed squad by 1 year.</item>
    ///   <item>Rolls over <see cref="PerformanceStats"/> into
    ///         <see cref="PlayerCareerStats"/> (via <see cref="PlayerData.AdvanceSeason"/>).</item>
    ///   <item>Awards championship titles to every player in the squad if the
    ///         manager won the league / cup this season.</item>
    ///   <item>Detects players who are now retirable or whose contracts have expired.</item>
    ///   <item>Increments the career season counter on <see cref="CareerData"/>.</item>
    /// </list>
    ///
    /// Registered in <see cref="Core.GameManager"/> via
    /// <see cref="Core.ServiceLocator"/>.
    /// </summary>
    public class SeasonSystem
    {
        private readonly PlayerRatingSystem _ratingSystem;

        public SeasonSystem(PlayerRatingSystem ratingSystem)
        {
            _ratingSystem = ratingSystem
                ?? throw new ArgumentNullException(nameof(ratingSystem));
        }

        // ── Season advancement ─────────────────────────────────────────────────

        /// <summary>
        /// Advances the season for every player in <paramref name="squad"/>:
        /// ages each player +1, rolls over season stats, updates market values,
        /// increments the career season counter, and returns a per-player summary
        /// including retirement and contract-expiry flags.
        /// </summary>
        /// <param name="squad">Players in the managed squad.</param>
        /// <param name="career">Active career data (season counter is incremented).</param>
        /// <param name="leagueId">
        /// Optional league identifier used for market-value recalculation.
        /// </param>
        /// <returns>
        /// A list of <see cref="PlayerSeasonEndResult"/> — one per player, ordered
        /// by <em>IsRetirable</em> first, then by age descending.
        /// </returns>
        public List<PlayerSeasonEndResult> AdvanceSeason(
            List<PlayerData> squad,
            CareerData career,
            string leagueId = null)
        {
            var results = new List<PlayerSeasonEndResult>();

            if (squad != null)
            {
                foreach (var player in squad)
                {
                    if (player == null) continue;

                    int ageBefore      = player.age;
                    int overallBefore  = player.CalculateOverall();

                    player.AdvanceSeason();                          // age +1, rollover stats
                    player.SimulateGrowth(12);                       // annual growth / decline
                    _ratingSystem.EnforceRatingBounds(player);
                    PlayerRatingSystem.SyncRarity(player);
                    _ratingSystem.CalculateMarketValue(player, leagueId);

                    bool contractExpired = player.contractYears <= 0;

                    results.Add(new PlayerSeasonEndResult
                    {
                        Player          = player,
                        AgeBefore       = ageBefore,
                        AgeAfter        = player.age,
                        OverallBefore   = overallBefore,
                        OverallAfter    = player.overallRating,
                        IsRetirable     = player.IsRetirable,
                        ContractExpired = contractExpired
                    });
                }
            }

            // Increment career season counter
            if (career != null) career.season++;

            GD.Print($"[SeasonSystem] Season advanced to {career?.season ?? 0}. " +
                      $"Retirable: {results.Count(r => r.IsRetirable)}  " +
                      $"Expired contracts: {results.Count(r => r.ContractExpired)}");

            return results
                .OrderByDescending(r => r.IsRetirable)
                .ThenByDescending(r => r.AgeAfter)
                .ToList();
        }

        // ── Trophy / championship awards ───────────────────────────────────────

        /// <summary>
        /// Awards a league title to every player currently in <paramref name="squad"/>.
        /// Increments <see cref="PlayerCareerStats.leagueTitles"/> for each player
        /// and <see cref="CareerData.managerLeagueTitles"/> for the manager.
        /// </summary>
        public void AwardLeagueTitle(List<PlayerData> squad, CareerData career = null)
        {
            if (squad == null) return;
            foreach (var player in squad)
            {
                if (player == null) continue;
                player.careerStats ??= new PlayerCareerStats();
                player.careerStats.leagueTitles++;
            }
            if (career != null) career.managerLeagueTitles++;
            GD.Print($"[SeasonSystem] League title awarded to {squad.Count} players.");
        }

        /// <summary>
        /// Awards a domestic cup to every player currently in <paramref name="squad"/>.
        /// </summary>
        public void AwardCupTitle(List<PlayerData> squad, CareerData career = null)
        {
            if (squad == null) return;
            foreach (var player in squad)
            {
                if (player == null) continue;
                player.careerStats ??= new PlayerCareerStats();
                player.careerStats.cupTitles++;
            }
            if (career != null) career.managerCupTitles++;
            GD.Print($"[SeasonSystem] Cup title awarded to {squad.Count} players.");
        }

        /// <summary>
        /// Awards a continental / international title to every player in
        /// <paramref name="squad"/>.
        /// </summary>
        public void AwardContinentalTitle(List<PlayerData> squad, CareerData career = null)
        {
            if (squad == null) return;
            foreach (var player in squad)
            {
                if (player == null) continue;
                player.careerStats ??= new PlayerCareerStats();
                player.careerStats.continentalTitles++;
            }
            if (career != null) career.managerContinentalTitles++;
            GD.Print($"[SeasonSystem] Continental title awarded to {squad.Count} players.");
        }

        // ── Individual award helpers ───────────────────────────────────────────

        /// <summary>Awards the season top-scorer (Golden Boot) to a single player.</summary>
        public static void AwardGoldenBoot(PlayerData player)
        {
            if (player == null) return;
            player.careerStats ??= new PlayerCareerStats();
            player.careerStats.goldenBootAwards++;
            GD.Print($"[SeasonSystem] Golden Boot awarded to {player.name}.");
        }

        /// <summary>Awards the season best-goalkeeper (Golden Glove) to a single player.</summary>
        public static void AwardGoldenGlove(PlayerData player)
        {
            if (player == null) return;
            player.careerStats ??= new PlayerCareerStats();
            player.careerStats.goldenGloveAwards++;
            GD.Print($"[SeasonSystem] Golden Glove awarded to {player.name}.");
        }

        // ── Retirement ─────────────────────────────────────────────────────────

        /// <summary>
        /// Retires <paramref name="player"/> from <paramref name="squad"/>.
        /// The player is removed from the squad but their career stats are preserved.
        /// Returns <c>false</c> if the player is not retirable or not in the squad.
        /// </summary>
        public bool RetirePlayer(PlayerData player, List<PlayerData> squad)
        {
            if (player == null || squad == null) return false;
            if (!player.IsRetirable)
            {
                GD.PushWarning($"[SeasonSystem] {player.name} (age {player.age}) is too young to retire.");
                return false;
            }
            if (!squad.Contains(player))
            {
                GD.PushWarning($"[SeasonSystem] {player.name} is not in the squad.");
                return false;
            }

            squad.Remove(player);
            player.isAvailable = false;

            GD.Print($"[SeasonSystem] {player.name} (age {player.age}) retired. " +
                      $"Career: {player.careerStats?.ToSummaryString() ?? "—"}");
            return true;
        }
    }
}
