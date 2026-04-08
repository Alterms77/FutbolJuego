using System;

namespace FutbolJuego.Models
{
    /// <summary>
    /// The real-world currency used to denominate club finances.
    /// Liga MX / Brazil use USD, European leagues use EUR.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>United States Dollar – used by Liga MX and Brasileirão.</summary>
        USD,
        /// <summary>Euro – used by LaLiga, Premier League, and Serie A.</summary>
        EUR
    }

    // ── CareerData ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Persistent state for the player's career: which club they manage,
    /// which league they are in, financial balances, and progression.
    /// </summary>
    [Serializable]
    public class CareerData
    {
        // ── Club assignment ────────────────────────────────────────────────────

        /// <summary>ID of the club the player currently manages.</summary>
        public string managedTeamId;
        /// <summary>ID of the league the managed club competes in.</summary>
        public string managedLeagueId;
        /// <summary>Human-readable name of the managed club (cached for UI).</summary>
        public string managedTeamName;
        /// <summary>Human-readable name of the league (cached for UI).</summary>
        public string managedLeagueName;

        // ── Season ─────────────────────────────────────────────────────────────

        /// <summary>Current career season number (starts at 1).</summary>
        public int season = 1;
        /// <summary>UTC date when the career was started.</summary>
        public string careerStartDate;
        /// <summary>Total matches managed across all seasons.</summary>
        public int totalMatchesManaged;

        // ── In-game finances ───────────────────────────────────────────────────

        /// <summary>Currency denomination for this career's finances.</summary>
        public CurrencyType currencyType = CurrencyType.EUR;
        /// <summary>
        /// Current in-game financial balance in the chosen currency.
        /// This mirrors and supplements <see cref="Models.FinanceData.balance"/>.
        /// </summary>
        public long inGameBalance;
        /// <summary>Transfer budget available at the start of the career.</summary>
        public long startingTransferBudget;

        // ── Premium currency ───────────────────────────────────────────────────

        /// <summary>
        /// FutCoins (premium currency) held by the player.
        /// Can be converted to in-game EUR/USD via <see cref="Systems.CurrencySystem"/>.
        /// </summary>
        public int premiumCoins;

        // ── Career history ─────────────────────────────────────────────────────

        /// <summary>Number of times the manager has resigned from a club.</summary>
        public int resignCount;
        /// <summary>Previous club IDs managed (ordered, oldest first).</summary>
        public System.Collections.Generic.List<string> previousTeamIds
            = new System.Collections.Generic.List<string>();

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the currency symbol for the current denomination.
        /// </summary>
        public string CurrencySymbol => currencyType == CurrencyType.EUR ? "€" : "$";

        /// <summary>
        /// Returns a human-readable balance string (e.g. "€ 25,000,000").
        /// </summary>
        public string FormattedBalance =>
            $"{CurrencySymbol} {inGameBalance:N0}";
    }
}
