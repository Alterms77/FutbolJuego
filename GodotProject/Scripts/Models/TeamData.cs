using System;
using System.Collections.Generic;
using System.Linq;

namespace FutbolJuego.Models
{
    /// <summary>
    /// All persistent data for a single football club: squad, tactics,
    /// finances, infrastructure, and season results.
    /// </summary>
    [Serializable]
    public class TeamData
    {
        // ── Identity ───────────────────────────────────────────────────────────

        /// <summary>Unique team GUID.</summary>
        public string id;
        /// <summary>Full club name (e.g. "FC Metropolitan").</summary>
        public string name;
        /// <summary>3-4 letter abbreviation (e.g. "FCM").</summary>
        public string shortName;
        /// <summary>Home city.</summary>
        public string city;
        /// <summary>Country the club belongs to.</summary>
        public string country;

        // ── Squad ──────────────────────────────────────────────────────────────

        /// <summary>All players currently at the club.</summary>
        public List<PlayerData> squad = new List<PlayerData>();
        /// <summary>Currently active tactical setup.</summary>
        public TacticData currentTactic = new TacticData();

        // ── Club infrastructure & finances ─────────────────────────────────────

        /// <summary>Stadium, training facility, academy, and medical centre.</summary>
        public ClubInfrastructure infrastructure = new ClubInfrastructure();
        /// <summary>Financial state and history.</summary>
        public FinanceData finances = new FinanceData();

        // ── Season results ─────────────────────────────────────────────────────

        /// <summary>Current league position.</summary>
        public int leaguePosition;
        /// <summary>League wins this season.</summary>
        public int wins;
        /// <summary>League draws this season.</summary>
        public int draws;
        /// <summary>League losses this season.</summary>
        public int losses;
        /// <summary>Goals scored this season.</summary>
        public int goalsFor;
        /// <summary>Goals conceded this season.</summary>
        public int goalsAgainst;
        /// <summary>Team-wide morale (0-100; 50 = neutral).</summary>
        public int morale = 50;

        // ── Computed properties ────────────────────────────────────────────────

        /// <summary>Total league points (3 per win, 1 per draw).</summary>
        public int GetPoints() => wins * 3 + draws;

        /// <summary>Goals for minus goals against.</summary>
        public int GetGoalDifference() => goalsFor - goalsAgainst;

        /// <summary>
        /// Mean overall rating of all players currently in the squad.
        /// Returns 0 if the squad is empty.
        /// </summary>
        public float GetAverageSquadRating()
        {
            if (squad == null || squad.Count == 0) return 0f;
            return (float)squad.Average(p => p.CalculateOverall());
        }

        /// <summary>
        /// Returns all players that are fit to play (not injured, not suspended).
        /// </summary>
        public List<PlayerData> GetAvailablePlayers()
        {
            return squad?.Where(p => p.isAvailable && p.injuryDaysRemaining <= 0).ToList()
                   ?? new List<PlayerData>();
        }

        /// <summary>
        /// Picks the best eleven available players that fit
        /// <paramref name="tactic"/>'s formation.  Falls back to best-overall
        /// selection if there are fewer than 11 fit players.
        /// </summary>
        public List<PlayerData> GetStartingEleven(TacticData tactic)
        {
            var available = GetAvailablePlayers();
            if (available.Count < Utils.Constants.StartingEleven)
                return available.OrderByDescending(p => p.CalculateOverall())
                                .Take(Utils.Constants.StartingEleven)
                                .ToList();

            var starting = new List<PlayerData>();
            var used     = new HashSet<string>();

            if (tactic?.positionAssignments != null)
            {
                foreach (var assignment in tactic.positionAssignments)
                {
                    var best = available
                        .Where(p => !used.Contains(p.id) && p.position == assignment.position)
                        .OrderByDescending(p => p.CalculateOverall())
                        .FirstOrDefault();

                    if (best == null)
                    {
                        // Fallback: pick best unused player regardless of position
                        best = available
                            .Where(p => !used.Contains(p.id))
                            .OrderByDescending(p => p.CalculateOverall())
                            .FirstOrDefault();
                    }

                    if (best != null)
                    {
                        starting.Add(best);
                        used.Add(best.id);
                        if (starting.Count == Utils.Constants.StartingEleven) break;
                    }
                }
            }

            // Top up to 11 if position assignments were insufficient
            if (starting.Count < Utils.Constants.StartingEleven)
            {
                var extras = available
                    .Where(p => !used.Contains(p.id))
                    .OrderByDescending(p => p.CalculateOverall())
                    .Take(Utils.Constants.StartingEleven - starting.Count);
                starting.AddRange(extras);
            }

            return starting;
        }
    }
}
