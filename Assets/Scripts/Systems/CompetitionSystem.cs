using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Manages league and cup competitions: fixture generation, result
    /// processing, table sorting, promotion/relegation, and prize money.
    /// </summary>
    public class CompetitionSystem
    {
        private readonly System.Random rng = new System.Random();

        // ── Fixture generation ─────────────────────────────────────────────────

        /// <summary>
        /// Generates a full round-robin fixture schedule for
        /// <paramref name="league"/> (home and away legs).
        /// </summary>
        public void GenerateLeagueFixtures(LeagueData league)
        {
            if (league?.teamIds == null || league.teamIds.Count < 2) return;

            league.fixtures.Clear();

            var teams    = new List<string>(league.teamIds);
            int n        = teams.Count;
            bool hasBye  = n % 2 != 0;
            if (hasBye) teams.Add("BYE");

            int rounds     = teams.Count - 1;
            int matchesPerRound = teams.Count / 2;
            int matchday   = 1;

            for (int round = 0; round < rounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int home = match;
                    int away = teams.Count - 1 - match;

                    if (teams[home] != "BYE" && teams[away] != "BYE")
                    {
                        league.fixtures.Add(new FixtureData
                        {
                            id         = Guid.NewGuid().ToString(),
                            homeTeamId = teams[home],
                            awayTeamId = teams[away],
                            matchday   = matchday,
                            date       = DateTime.UtcNow.AddDays(matchday * 7)
                        });
                    }
                }

                matchday++;

                // Rotate teams (keep index 0 fixed)
                var last = teams[teams.Count - 1];
                teams.RemoveAt(teams.Count - 1);
                teams.Insert(1, last);
            }

            // Second leg — reverse home/away
            int firstLegCount = league.fixtures.Count;
            for (int i = 0; i < firstLegCount; i++)
            {
                var orig = league.fixtures[i];
                league.fixtures.Add(new FixtureData
                {
                    id         = Guid.NewGuid().ToString(),
                    homeTeamId = orig.awayTeamId,
                    awayTeamId = orig.homeTeamId,
                    matchday   = orig.matchday + rounds,
                    date       = orig.date.AddDays(rounds * 7)
                });
            }

            Debug.Log($"[CompetitionSystem] Generated {league.fixtures.Count} fixtures for {league.name}.");
        }

        // ── Result processing ──────────────────────────────────────────────────

        /// <summary>
        /// Updates the league table based on a completed match result and
        /// links the match ID to the fixture.
        /// </summary>
        public void ProcessMatchResult(MatchData match, LeagueData league)
        {
            if (match == null || league == null) return;

            league.UpdateTable(match);

            var fixture = league.fixtures.FirstOrDefault(
                f => f.homeTeamId == match.homeTeamId &&
                     f.awayTeamId == match.awayTeamId &&
                     string.IsNullOrEmpty(f.matchId));

            if (fixture != null) fixture.matchId = match.id;
        }

        // ── Table ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the current league table sorted by points → GD → GF.
        /// </summary>
        public List<LeagueTableEntry> GetLeagueTable(LeagueData league)
        {
            if (league?.table == null) return new List<LeagueTableEntry>();

            return league.table
                .OrderByDescending(e => e.points)
                .ThenByDescending(e => e.goalDifference)
                .ThenByDescending(e => e.goalsFor)
                .ToList();
        }

        // ── Cup bracket ────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a single-elimination cup bracket from the supplied teams.
        /// Teams are drawn randomly.  Returns the first-round fixture list.
        /// </summary>
        public List<FixtureData> GenerateCupBracket(List<TeamData> teams)
        {
            if (teams == null || teams.Count < 2) return new List<FixtureData>();

            var shuffled = teams.OrderBy(_ => rng.Next()).ToList();
            var fixtures = new List<FixtureData>();

            for (int i = 0; i + 1 < shuffled.Count; i += 2)
            {
                fixtures.Add(new FixtureData
                {
                    id         = Guid.NewGuid().ToString(),
                    homeTeamId = shuffled[i].id,
                    awayTeamId = shuffled[i + 1].id,
                    matchday   = 1,
                    date       = DateTime.UtcNow.AddDays(14)
                });
            }
            return fixtures;
        }

        // ── Promotion & relegation ─────────────────────────────────────────────

        /// <summary>
        /// Identifies teams that are promoted (top 2) or relegated (bottom 3)
        /// and returns them keyed by outcome.
        /// </summary>
        public Dictionary<string, List<string>> CheckPromotionRelegation(LeagueData league)
        {
            var result = new Dictionary<string, List<string>>
            {
                ["promoted"]  = new List<string>(),
                ["relegated"] = new List<string>()
            };

            if (league?.table == null || league.table.Count < 4) return result;

            var sorted = GetLeagueTable(league);

            // Top 2 promoted
            for (int i = 0; i < Mathf.Min(2, sorted.Count); i++)
                result["promoted"].Add(sorted[i].teamId);

            // Bottom 3 relegated
            for (int i = Mathf.Max(0, sorted.Count - 3); i < sorted.Count; i++)
                result["relegated"].Add(sorted[i].teamId);

            return result;
        }

        // ── Prize money ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a dictionary mapping team ID to prize money based on
        /// final league position.
        /// </summary>
        public Dictionary<string, long> CalculatePrizeMoney(LeagueData league)
        {
            var prizes = new Dictionary<string, long>();
            if (league?.table == null) return prizes;

            var sorted = GetLeagueTable(league);
            long[] pool =
            {
                10_000_000, 8_000_000, 7_000_000, 6_500_000, 6_000_000,
                5_500_000,  5_000_000, 4_500_000, 4_000_000, 3_500_000,
                3_000_000,  2_800_000, 2_600_000, 2_400_000, 2_200_000,
                2_000_000,  1_800_000, 1_600_000, 1_400_000, 1_000_000
            };

            for (int i = 0; i < sorted.Count; i++)
            {
                long prize = i < pool.Length ? pool[i] : 800_000L;
                prizes[sorted[i].teamId] = prize;
            }
            return prizes;
        }
    }
}
