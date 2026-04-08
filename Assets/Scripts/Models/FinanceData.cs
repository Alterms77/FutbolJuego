using System;
using System.Collections.Generic;
using System.Linq;

namespace FutbolJuego.Models
{
    // ── FinanceData ────────────────────────────────────────────────────────────

    /// <summary>Full financial state for a club including budgets and history.</summary>
    [Serializable]
    public class FinanceData
    {
        /// <summary>Current bank balance.</summary>
        public long balance;
        /// <summary>Total weekly wage bill (sum of all contracted players).</summary>
        public long weeklyWageBill;
        /// <summary>Weekly income from stadium match-day revenue.</summary>
        public long stadiumRevenue;
        /// <summary>Weekly income from shirt/commercial sponsors.</summary>
        public long sponsorRevenue;
        /// <summary>Seasonal prize money for league / cup position.</summary>
        public long prizeMoneyRevenue;
        /// <summary>Available budget for incoming transfers.</summary>
        public long transferBudget;
        /// <summary>Maximum weekly wage the club can offer a new player.</summary>
        public long wageBudget;
        /// <summary>Ordered history of financial transactions.</summary>
        public List<FinanceTransaction> transactionHistory = new List<FinanceTransaction>();

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Appends <paramref name="transaction"/> to history and adjusts
        /// <see cref="balance"/> accordingly.
        /// </summary>
        public void AddTransaction(FinanceTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            transactionHistory.Add(transaction);

            switch (transaction.type)
            {
                case FinanceTransactionType.Income:
                case FinanceTransactionType.Prize:
                    balance += transaction.amount;
                    break;
                case FinanceTransactionType.Expense:
                case FinanceTransactionType.Salary:
                    balance -= transaction.amount;
                    break;
                case FinanceTransactionType.Transfer:
                    // Positive amount = sold player (income), negative = bought player (expense)
                    balance += transaction.amount;
                    if (transaction.amount < 0)
                        transferBudget += transaction.amount;
                    else
                        transferBudget += transaction.amount;
                    break;
            }
        }

        /// <summary>
        /// Total monthly income estimate based on current revenue streams.
        /// (Stadium + sponsor revenue * ~4.3 weeks per month + seasonal prize / 12.)
        /// </summary>
        public long GetMonthlyIncome()
        {
            const float weeksPerMonth = 4.333f;
            long weekly = stadiumRevenue + sponsorRevenue;
            long monthly = (long)(weekly * weeksPerMonth) + prizeMoneyRevenue / 12;
            return monthly;
        }

        /// <summary>
        /// Total monthly expense estimate.
        /// (Wage bill × ~4.3 weeks.)
        /// </summary>
        public long GetMonthlyExpenses()
        {
            const float weeksPerMonth = 4.333f;
            return (long)(weeklyWageBill * weeksPerMonth);
        }

        /// <summary>
        /// Returns <c>true</c> if the current balance covers
        /// <paramref name="amount"/>.
        /// </summary>
        public bool CanAfford(long amount) => balance >= amount;
    }

    // ── FinanceTransaction ─────────────────────────────────────────────────────

    /// <summary>A single financial event recorded in the club ledger.</summary>
    [Serializable]
    public class FinanceTransaction
    {
        /// <summary>UTC date of the transaction.</summary>
        public DateTime date;
        /// <summary>Category of the transaction.</summary>
        public FinanceTransactionType type;
        /// <summary>
        /// Amount in the game's currency unit.  Always positive for non-Transfer
        /// types; transfers use sign convention (positive = sale).
        /// </summary>
        public long amount;
        /// <summary>Human-readable description for the ledger UI.</summary>
        public string description;
    }

    // ── Enumeration ────────────────────────────────────────────────────────────

    /// <summary>Category for a financial transaction.</summary>
    public enum FinanceTransactionType
    {
        Income,
        Expense,
        Transfer,
        Salary,
        Prize
    }
}
