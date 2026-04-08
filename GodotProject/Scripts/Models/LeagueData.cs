using System;
using System.Collections.Generic;
using System.Linq;

namespace FutbolJuego.Models
{
    // ── LeagueData ─────────────────────────────────────────────────────────────

    /// <summary>Persistent data for a single league competition and season.</summary>
    [Serializable]
    public class LeagueData
    {
        /// <summary>Unique league GUID.</summary>
        public string id;
        /// <summary>Full league name (e.g. "Primera División").</summary>
        public string name;
        /// <summary>Country the league belongs to.</summary>
        public string country;
        /// <summary>Pyramid level (1 = top flight).</summary>
        public int level;
        /// <summary>IDs of all participating teams.</summary>
        public List<string> teamIds = new List<string>();
        /// <summary>Current season number (starts at 1).</summary>
        public int currentSeason = 1;
        /// <summary>Which matchday is currently being played (1-based).</summary>
        public int currentMatchday = 1;
        /// <summary>All fixtures for the season.</summary>
        public List<FixtureData> fixtures = new List<FixtureData>();
        /// <summary>Current league table, sorted by points.</summary>
        public List<LeagueTableEntry> table = new List<LeagueTableEntry>();

        // ── Public methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the table entry for <paramref name="teamId"/>, or
        /// <c>null</c> if the team is not in this league.
        /// </summary>
        public LeagueTableEntry GetTeamEntry(string teamId)
            => table.FirstOrDefault(e => e.teamId == teamId);

        /// <summary>
        /// Updates the league table with the result of a completed
        /// <paramref name="match"/> and returns the affected entries.
        /// </summary>
        public void UpdateTable(MatchData match)
        {
            if (match == null || match.status != MatchStatus.Completed) return;

            var homeEntry = GetOrCreateEntry(match.homeTeamId);
            var awayEntry = GetOrCreateEntry(match.awayTeamId);

            homeEntry.played++;
            awayEntry.played++;
            homeEntry.goalsFor      += match.homeScore;
            homeEntry.goalsAgainst  += match.awayScore;
            awayEntry.goalsFor      += match.awayScore;
            awayEntry.goalsAgainst  += match.homeScore;

            if (match.homeScore > match.awayScore)
            {
                homeEntry.wins++;  homeEntry.points += 3;
                awayEntry.losses++;
            }
            else if (match.homeScore < match.awayScore)
            {
                awayEntry.wins++;  awayEntry.points += 3;
                homeEntry.losses++;
            }
            else
            {
                homeEntry.draws++; homeEntry.points++;
                awayEntry.draws++; awayEntry.points++;
            }

            SortTable();
        }

        /// <summary>
        /// Returns all fixtures scheduled for <paramref name="matchday"/>.
        /// </summary>
        public List<FixtureData> GetMatchdayFixtures(int matchday)
            => fixtures.Where(f => f.matchday == matchday).ToList();

        // ── Private helpers ────────────────────────────────────────────────────

        private LeagueTableEntry GetOrCreateEntry(string teamId)
        {
            var entry = GetTeamEntry(teamId);
            if (entry == null)
            {
                entry = new LeagueTableEntry { teamId = teamId };
                table.Add(entry);
            }
            return entry;
        }

        private void SortTable()
        {
            table.Sort((a, b) =>
            {
                int pts = b.points.CompareTo(a.points);
                if (pts != 0) return pts;
                int gd = b.goalDifference.CompareTo(a.goalDifference);
                if (gd != 0) return gd;
                return b.goalsFor.CompareTo(a.goalsFor);
            });
        }
    }

    // ── LeagueTableEntry ───────────────────────────────────────────────────────

    /// <summary>One row in the league standings table.</summary>
    [Serializable]
    public class LeagueTableEntry
    {
        /// <summary>Team identifier this row belongs to.</summary>
        public string teamId;
        /// <summary>Matches played.</summary>
        public int played;
        /// <summary>Wins.</summary>
        public int wins;
        /// <summary>Draws.</summary>
        public int draws;
        /// <summary>Losses.</summary>
        public int losses;
        /// <summary>Goals scored.</summary>
        public int goalsFor;
        /// <summary>Goals conceded.</summary>
        public int goalsAgainst;
        /// <summary>League points.</summary>
        public int points;
        /// <summary>Computed goal difference.</summary>
        public int goalDifference => goalsFor - goalsAgainst;
    }

    // ── FixtureData ────────────────────────────────────────────────────────────

    /// <summary>A scheduled but not yet played match in the fixture list.</summary>
    [Serializable]
    public class FixtureData
    {
        /// <summary>Unique fixture GUID.</summary>
        public string id;
        /// <summary>Home team identifier.</summary>
        public string homeTeamId;
        /// <summary>Away team identifier.</summary>
        public string awayTeamId;
        /// <summary>Round / matchday number (1-based).</summary>
        public int matchday;
        /// <summary>Scheduled date/time (UTC).</summary>
        public DateTime date;
        /// <summary>Set to the generated <see cref="MatchData.id"/> once played.</summary>
        public string matchId;
    }
}
