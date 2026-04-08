using System;
using System.Collections.Generic;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Manages the player's career: starting a career with a chosen team,
    /// resigning, joining a new club, and assigning league-appropriate budgets.
    /// Registered as a service via <see cref="Core.ServiceLocator"/>.
    /// </summary>
    public class CareerSystem
    {
        // ── Active career ──────────────────────────────────────────────────────

        /// <summary>The current career session; null before a career is started.</summary>
        public CareerData ActiveCareer { get; private set; }

        /// <summary>Raised when the manager joins a new club.</summary>
        public event Action<CareerData> OnTeamChanged;

        // ── Starting budgets by league reputation ──────────────────────────────

        // Keyed on league id → (currencyType, startingTransferBudget)
        private static readonly Dictionary<string, (CurrencyType currency, long budget)>
            LeagueBudgetDefaults = new Dictionary<string, (CurrencyType, long)>
            {
                { "league-liga-mx",      (CurrencyType.USD, 20_000_000L) },
                { "league-brasileirao",  (CurrencyType.USD, 25_000_000L) },
                { "league-laliga",       (CurrencyType.EUR, 80_000_000L) },
                { "league-premier",      (CurrencyType.EUR, 100_000_000L) },
                { "league-seriea",       (CurrencyType.EUR, 60_000_000L) },
            };

        // ── Career lifecycle ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a new career for the specified league and team.
        /// The manager receives the team's existing transfer budget plus
        /// a league-tier starting bonus.
        /// </summary>
        /// <param name="league">League the team belongs to.</param>
        /// <param name="team">The club the manager will control.</param>
        /// <returns>The newly created <see cref="CareerData"/>.</returns>
        public CareerData StartCareer(LeagueData league, TeamData team)
        {
            if (league == null) throw new ArgumentNullException(nameof(league));
            if (team   == null) throw new ArgumentNullException(nameof(team));

            var (currency, baseLeagueBudget) = GetLeagueBudgetDefaults(league.id);
            long teamBudget = team.finances?.transferBudget ?? 0L;
            long totalBudget = Math.Max(teamBudget, baseLeagueBudget);

            ActiveCareer = new CareerData
            {
                managedTeamId          = team.id,
                managedLeagueId        = league.id,
                managedTeamName        = team.name,
                managedLeagueName      = league.name,
                season                 = 1,
                careerStartDate        = DateTime.UtcNow.ToString("o"),
                currencyType           = currency,
                inGameBalance          = totalBudget,
                startingTransferBudget = totalBudget,
                premiumCoins           = 0,
                resignCount            = 0,
                previousTeamIds        = new System.Collections.Generic.List<string>()
            };

            // Sync the club's finance data with the career budget
            if (team.finances != null)
            {
                team.finances.transferBudget = totalBudget;
                team.finances.balance        = Math.Max(team.finances.balance, totalBudget);
            }

            Debug.Log($"[CareerSystem] Career started: {team.name} in {league.name} " +
                      $"({currency} {totalBudget:N0}).");
            OnTeamChanged?.Invoke(ActiveCareer);
            return ActiveCareer;
        }

        /// <summary>
        /// Resigns from the current club.  Preserves career stats and records
        /// the outgoing team in <see cref="CareerData.previousTeamIds"/>.
        /// </summary>
        public void ResignFromTeam()
        {
            if (ActiveCareer == null)
            {
                Debug.LogWarning("[CareerSystem] No active career to resign from.");
                return;
            }

            ActiveCareer.previousTeamIds.Add(ActiveCareer.managedTeamId);
            ActiveCareer.resignCount++;
            string oldTeam = ActiveCareer.managedTeamName;

            ActiveCareer.managedTeamId   = null;
            ActiveCareer.managedTeamName = null;

            Debug.Log($"[CareerSystem] Manager resigned from {oldTeam}. " +
                      $"Total resignations: {ActiveCareer.resignCount}.");
        }

        /// <summary>
        /// Assigns the manager to a new club after resigning.
        /// Recalculates budget based on the new league / team.
        /// </summary>
        /// <param name="league">New league.</param>
        /// <param name="team">New club.</param>
        public void JoinTeam(LeagueData league, TeamData team)
        {
            if (league == null) throw new ArgumentNullException(nameof(league));
            if (team   == null) throw new ArgumentNullException(nameof(team));
            if (ActiveCareer == null)
            {
                // Create a fresh career if none exists
                StartCareer(league, team);
                return;
            }

            var (currency, baseLeagueBudget) = GetLeagueBudgetDefaults(league.id);
            long teamBudget = team.finances?.transferBudget ?? 0L;
            long newBudget  = Math.Max(teamBudget, baseLeagueBudget);

            ActiveCareer.managedTeamId    = team.id;
            ActiveCareer.managedLeagueId  = league.id;
            ActiveCareer.managedTeamName  = team.name;
            ActiveCareer.managedLeagueName = league.name;
            ActiveCareer.currencyType      = currency;
            ActiveCareer.inGameBalance     = newBudget;
            ActiveCareer.startingTransferBudget = newBudget;

            if (team.finances != null)
            {
                team.finances.transferBudget = newBudget;
                team.finances.balance        = Math.Max(team.finances.balance, newBudget);
            }

            Debug.Log($"[CareerSystem] Manager joined {team.name} in {league.name} " +
                      $"({currency} {newBudget:N0}).");
            OnTeamChanged?.Invoke(ActiveCareer);
        }

        // ── Budget helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the starting budget and currency for the given league.
        /// Falls back to EUR / 30 000 000 for unknown leagues.
        /// </summary>
        public (CurrencyType currency, long budget) GetLeagueBudgetDefaults(string leagueId)
        {
            if (LeagueBudgetDefaults.TryGetValue(leagueId, out var defaults))
                return defaults;
            return (CurrencyType.EUR, 30_000_000L);
        }

        /// <summary>
        /// Fraction of the league floor budget allocated to smaller clubs that
        /// don't already have a higher own transfer budget.
        /// </summary>
        private const float SmallClubBudgetFraction = 0.5f;

        /// <summary>
        /// Calculates a team-specific starting budget taking the team's own
        /// <see cref="FinanceData.transferBudget"/> and the league default
        /// into account.  Larger clubs receive their own budget; smaller clubs
        /// receive the league-floor minimum × <see cref="SmallClubBudgetFraction"/>.
        /// </summary>
        public long CalculateStartingBudget(TeamData team, string leagueId)
        {
            var (_, leagueBudget) = GetLeagueBudgetDefaults(leagueId);
            long teamBudget = team?.finances?.transferBudget ?? 0L;
            return Math.Max(teamBudget, (long)(leagueBudget * SmallClubBudgetFraction));
        }

        // ── Persistence ────────────────────────────────────────────────────────

        /// <summary>Restores a career from a saved JSON snapshot.</summary>
        public void LoadCareer(CareerData saved)
        {
            ActiveCareer = saved;
        }
    }
}
