using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Finances screen: summary panel, transaction history list, and budget
    /// allocation display.
    /// </summary>
    public class FinancesUI : MonoBehaviour
    {
        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI balanceLabel;
        [SerializeField] private TextMeshProUGUI transferBudgetLabel;
        [SerializeField] private TextMeshProUGUI wageBudgetLabel;
        [SerializeField] private TextMeshProUGUI monthlyIncomeLabel;
        [SerializeField] private TextMeshProUGUI monthlyExpenseLabel;

        [Header("History")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private GameObject historyRowPrefab;

        [Header("Budget")]
        [SerializeField] private TextMeshProUGUI budgetAllocationText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backButton) backButton.onClick.AddListener(OnBack);
        }

        // ── Navigation ─────────────────────────────────────────────────────────

        public void OnBack() => FutbolJuego.Core.SceneNavigator.Instance?.GoToDashboard();

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Updates all summary labels with data from <paramref name="finances"/>.</summary>
        public void ShowFinancialSummary(FinanceData finances)
        {
            if (finances == null) return;

            if (balanceLabel)        balanceLabel.text        = $"Balance:        £{finances.balance:N0}";
            if (transferBudgetLabel) transferBudgetLabel.text = $"Transfer Budget: £{finances.transferBudget:N0}";
            if (wageBudgetLabel)     wageBudgetLabel.text     = $"Wage Budget:    £{finances.wageBudget:N0}/wk";
            if (monthlyIncomeLabel)  monthlyIncomeLabel.text  = $"Monthly Income:  £{finances.GetMonthlyIncome():N0}";
            if (monthlyExpenseLabel) monthlyExpenseLabel.text = $"Monthly Expenses: £{finances.GetMonthlyExpenses():N0}";
        }

        /// <summary>Builds the transaction history list (newest first).</summary>
        public void ShowTransactionHistory()
        {
            // Caller must supply a FinanceData reference; placeholder shown here
            Debug.Log("[FinancesUI] ShowTransactionHistory called — wire up FinanceData reference.");
        }

        /// <summary>Shows transaction history from an explicit finance object.</summary>
        public void ShowTransactionHistory(FinanceData finances)
        {
            if (finances == null || historyContainer == null || historyRowPrefab == null) return;

            foreach (Transform child in historyContainer)
                Destroy(child.gameObject);

            var transactions = finances.transactionHistory
                .OrderByDescending(t => t.date)
                .Take(50);

            foreach (var tx in transactions)
            {
                var row   = Instantiate(historyRowPrefab, historyContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text = $"{tx.date:dd/MM/yy}  {tx.type}  " +
                                 $"{(tx.amount >= 0 ? "+" : "")}£{tx.amount:N0}  {tx.description}";
            }
        }

        /// <summary>Shows a budget breakdown in the allocation panel.</summary>
        public void ShowBudgetAllocation(FinanceData finances)
        {
            if (finances == null || budgetAllocationText == null) return;

            long income   = finances.GetMonthlyIncome();
            long expenses = finances.GetMonthlyExpenses();
            long net      = income - expenses;

            budgetAllocationText.text =
                $"Monthly Net: £{net:N0}  " +
                $"({(net >= 0 ? "Profit" : "Loss")})\n" +
                $"Wage Bill: £{finances.weeklyWageBill:N0}/wk\n" +
                $"Sponsor:   £{finances.sponsorRevenue:N0}/wk\n" +
                $"Stadium:   £{finances.stadiumRevenue:N0}/match";
        }
    }
}
