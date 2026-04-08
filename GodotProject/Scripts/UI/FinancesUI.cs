using System.Linq;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Finances screen: summary panel, transaction history list, and budget
    /// allocation display.
    /// </summary>
    public partial class FinancesUI : Control
    {
        [ExportGroup("Summary")]
        [Export] public Label balanceLabel;
        [Export] public Label transferBudgetLabel;
        [Export] public Label wageBudgetLabel;
        [Export] public Label monthlyIncomeLabel;
        [Export] public Label monthlyExpenseLabel;

        [ExportGroup("History")]
        [Export] public Control historyContainer;
        [Export] public PackedScene historyRowPrefab;

        [ExportGroup("Budget")]
        [Export] public Label budgetAllocationText;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (backButton != null) backButton.Pressed += OnBack;
        }

        // ── Navigation ─────────────────────────────────────────────────────────

        public void OnBack() => FutbolJuego.Core.SceneNavigator.Instance?.GoToDashboard();

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Updates all summary labels with data from <paramref name="finances"/>.</summary>
        public void ShowFinancialSummary(FinanceData finances)
        {
            if (finances == null) return;

            if (balanceLabel != null)        balanceLabel.Text        = $"Balance:        £{finances.balance:N0}";
            if (transferBudgetLabel != null) transferBudgetLabel.Text = $"Transfer Budget: £{finances.transferBudget:N0}";
            if (wageBudgetLabel != null)     wageBudgetLabel.Text     = $"Wage Budget:    £{finances.wageBudget:N0}/wk";
            if (monthlyIncomeLabel != null)  monthlyIncomeLabel.Text  = $"Monthly Income:  £{finances.GetMonthlyIncome():N0}";
            if (monthlyExpenseLabel != null) monthlyExpenseLabel.Text = $"Monthly Expenses: £{finances.GetMonthlyExpenses():N0}";
        }

        /// <summary>Builds the transaction history list (newest first).</summary>
        public void ShowTransactionHistory()
        {
            GD.Print("[FinancesUI] ShowTransactionHistory called — wire up FinanceData reference.");
        }

        /// <summary>Shows transaction history from an explicit finance object.</summary>
        public void ShowTransactionHistory(FinanceData finances)
        {
            if (finances == null || historyContainer == null || historyRowPrefab == null) return;

            foreach (var child in historyContainer.GetChildren())
                child.QueueFree();

            var transactions = finances.transactionHistory
                .OrderByDescending(t => t.date)
                .Take(50);

            foreach (var tx in transactions)
            {
                var row   = historyRowPrefab.Instantiate<Control>();
                historyContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text = $"{tx.date:dd/MM/yy}  {tx.type}  " +
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

            budgetAllocationText.Text =
                $"Monthly Net: £{net:N0}  " +
                $"({(net >= 0 ? "Profit" : "Loss")})\n" +
                $"Wage Bill: £{finances.weeklyWageBill:N0}/wk\n" +
                $"Sponsor:   £{finances.sponsorRevenue:N0}/wk\n" +
                $"Stadium:   £{finances.stadiumRevenue:N0}/match";
        }
    }
}
