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

        // ── CPU matchday simulation ────────────────────────────────────────────

        /// <summary>
        /// Simulates all CPU vs CPU fixtures on the current matchday of
        /// <paramref name="league"/>, updates the table, and advances
        /// <see cref="LeagueData.currentMatchday"/>.
        ///
        /// The fixture where <paramref name="playerTeamId"/> plays is skipped
        /// (the player is expected to play it manually) unless
        /// <paramref name="forceSimulateAll"/> is <c>true</c>.
        ///
        /// Returns the list of all simulated <see cref="MatchData"/> records.
        /// </summary>
        public List<MatchData> SimulateMatchday(
            LeagueData            league,
            List<TeamData>        allTeams,
            MatchSimulationEngine engine,
            string                playerTeamId,
            bool                  forceSimulateAll = false)
        {
            if (league == null || allTeams == null || engine == null)
                return new List<MatchData>();

            var fixtures  = league.GetMatchdayFixtures(league.currentMatchday);
            var results   = new List<MatchData>();

            foreach (var fixture in fixtures)
            {
                bool isPlayerMatch = fixture.homeTeamId == playerTeamId
                                  || fixture.awayTeamId == playerTeamId;

                if (isPlayerMatch && !forceSimulateAll) continue;
                if (!string.IsNullOrEmpty(fixture.matchId))  continue; // already played

                var homeTeam = allTeams.Find(t => t.id == fixture.homeTeamId);
                var awayTeam = allTeams.Find(t => t.id == fixture.awayTeamId);
                if (homeTeam == null || awayTeam == null) continue;

                var match = engine.SimulateMatch(homeTeam, awayTeam);
                match.competitionId = league.id;
                match.matchType     = MatchType.League;

                ProcessMatchResult(match, league);
                fixture.matchId = match.id;
                results.Add(match);

                Debug.Log($"[CompetitionSystem] MD{league.currentMatchday} " +
                          $"{homeTeam.shortName} {match.homeScore}–{match.awayScore} {awayTeam.shortName}");
            }

            league.currentMatchday++;
            return results;
        }

        /// <summary>
        /// Returns <c>true</c> when every fixture in the league has been played.
        /// </summary>
        public bool IsLeagueSeasonComplete(LeagueData league)
        {
            if (league?.fixtures == null || league.fixtures.Count == 0) return false;
            return league.fixtures.All(f => !string.IsNullOrEmpty(f.matchId));
        }

        // ── Cup management ─────────────────────────────────────────────────────

        /// <summary>
        /// Draws the first-round fixtures for <paramref name="cup"/> from
        /// its <see cref="CupData.participatingTeamIds"/>.
        /// Call this once before the cup starts.
        /// </summary>
        public void InitializeCup(CupData cup)
        {
            if (cup?.participatingTeamIds == null || cup.participatingTeamIds.Count < 2)
                return;

            cup.remainingTeamIds    = new List<string>(cup.participatingTeamIds);
            cup.currentRound        = 1;
            cup.currentRoundName    = GetCupRoundName(cup.remainingTeamIds.Count);
            cup.currentRoundFixtures.Clear();

            GenerateCupRoundFixtures(cup);
        }

        /// <summary>
        /// Simulates a single cup fixture, including a penalty shootout if
        /// the match ends level and <see cref="CupData.usePenaltiesOnDraw"/> is set.
        /// Returns the completed <see cref="MatchData"/>.
        /// </summary>
        public MatchData SimulateCupFixture(
            FixtureData           fixture,
            CupData               cup,
            List<TeamData>        allTeams,
            MatchSimulationEngine engine)
        {
            if (fixture == null || cup == null || allTeams == null || engine == null)
                return null;

            var homeTeam = allTeams.Find(t => t.id == fixture.homeTeamId);
            var awayTeam = allTeams.Find(t => t.id == fixture.awayTeamId);
            if (homeTeam == null || awayTeam == null) return null;

            var match = engine.SimulateMatch(homeTeam, awayTeam, isNeutralVenue: true);
            match.competitionId = cup.id;
            match.matchType     = MatchType.Cup;

            // Resolve draw with penalties when configured
            if (match.homeScore == match.awayScore && cup.usePenaltiesOnDraw)
            {
                var shootout       = engine.SimulatePenaltyShootout(homeTeam, awayTeam);
                match.penaltyShootout = shootout;
                match.wentToPenalties = true;

                Debug.Log($"[CompetitionSystem] {cup.name} penalties: " +
                          $"{homeTeam.shortName} {shootout.homeScore}–{shootout.awayScore} {awayTeam.shortName}");
            }

            fixture.matchId = match.id;
            return match;
        }

        /// <summary>
        /// Determines the winner of each current-round cup fixture from
        /// <paramref name="results"/>, advances surviving teams, generates
        /// next-round fixtures, and sets <see cref="CupData.winnerId"/> if
        /// the final has been played.
        /// </summary>
        public void AdvanceCupRound(CupData cup, List<MatchData> results)
        {
            if (cup == null || results == null) return;

            var winnerIds = new List<string>();

            foreach (var fixture in cup.currentRoundFixtures)
            {
                var match = results.FirstOrDefault(m => m.id == fixture.matchId);
                if (match == null) continue;

                string winnerId;
                if (match.wentToPenalties && match.penaltyShootout != null)
                {
                    winnerId = match.penaltyShootout.winnerTeamId;
                }
                else if (match.homeScore != match.awayScore)
                {
                    // Clear winner on the day
                    winnerId = match.homeScore > match.awayScore
                        ? match.homeTeamId
                        : match.awayTeamId;
                }
                else
                {
                    // Draw with no penalty tie-break (e.g. usePenaltiesOnDraw = false).
                    // Replay situations are not modelled; award a coin-flip winner so the
                    // cup can always progress.
                    winnerId = rng.NextDouble() < 0.5
                        ? match.homeTeamId
                        : match.awayTeamId;
                    Debug.Log($"[CompetitionSystem] {cup.name} draw resolved by coin-flip " +
                              $"({winnerId} advances).");
                }

                if (!winnerIds.Contains(winnerId))
                    winnerIds.Add(winnerId);
            }

            // Teams that had a bye automatically advance
            foreach (var teamId in cup.remainingTeamIds)
            {
                if (!cup.currentRoundFixtures.Any(
                        f => f.homeTeamId == teamId || f.awayTeamId == teamId))
                    if (!winnerIds.Contains(teamId))
                        winnerIds.Add(teamId);
            }

            cup.remainingTeamIds = winnerIds;
            cup.currentRound++;

            if (winnerIds.Count == 1)
            {
                cup.winnerId         = winnerIds[0];
                cup.currentRoundName = "Winner";
                cup.currentRoundFixtures.Clear();
                Debug.Log($"[CompetitionSystem] {cup.name} won by {cup.winnerId}.");
                return;
            }

            cup.currentRoundName = GetCupRoundName(winnerIds.Count);
            cup.currentRoundFixtures.Clear();
            GenerateCupRoundFixtures(cup);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void GenerateCupRoundFixtures(CupData cup)
        {
            var shuffled = cup.remainingTeamIds.OrderBy(_ => rng.Next()).ToList();

            for (int i = 0; i + 1 < shuffled.Count; i += 2)
            {
                cup.currentRoundFixtures.Add(new FixtureData
                {
                    id         = Guid.NewGuid().ToString(),
                    homeTeamId = shuffled[i],
                    awayTeamId = shuffled[i + 1],
                    matchday   = cup.currentRound,
                    date       = DateTime.UtcNow.AddDays(cup.currentRound * 14)
                });
            }
            // Odd team gets a bye (no fixture generated)
        }

        private static string GetCupRoundName(int teamCount) => teamCount switch
        {
            2  => "Final",
            4  => "Semi-finals",
            8  => "Quarter-finals",
            16 => "Round of 16",
            32 => "Round of 32",
            _  => $"Round of {teamCount}"
        };
    }
}
