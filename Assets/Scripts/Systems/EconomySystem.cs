using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>Type of infrastructure upgrade.</summary>
    public enum InfrastructureType { Stadium, TrainingFacility, YouthAcademy, MedicalCenter }

    /// <summary>
    /// Handles all financial flows: match-day revenue, wages, sponsorship,
    /// prize money, and infrastructure upgrades.
    /// </summary>
    public class EconomySystem
    {
        private const float WeeksPerMonth = 4.333f;

        // ── Revenue ────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates income from a single home match.
        /// </summary>
        public long CalculateMatchDayRevenue(TeamData team)
        {
            if (team?.infrastructure?.stadium == null) return 0;
            return team.infrastructure.stadium.CalculateMatchDayRevenue();
        }

        /// <summary>
        /// Estimates weekly sponsor revenue based on league level, position,
        /// and stadium size.
        /// </summary>
        public long CalculateSponsorRevenue(TeamData team)
        {
            if (team?.finances == null) return 0;
            float baseSponsor = team.infrastructure?.stadium?.capacity ?? 10000;
            // Base: £2 per seat per week, scaled by league position
            float positionBonus = 1.0f + Mathf.Max(0f, (20 - team.leaguePosition) * 0.02f);
            return (long)(baseSponsor * 2f * positionBonus);
        }

        /// <summary>
        /// Returns the prize money awarded to a team finishing at
        /// <paramref name="leaguePosition"/>.
        /// </summary>
        public long CalculatePrizeMoney(TeamData team, int leaguePosition)
        {
            // Simple tiered prize pool (top-flight style)
            long[] prizes =
            {
                10_000_000, 8_000_000, 7_000_000, 6_500_000, 6_000_000,
                5_500_000,  5_000_000, 4_500_000, 4_000_000, 3_500_000,
                3_000_000,  2_800_000, 2_600_000, 2_400_000, 2_200_000,
                2_000_000,  1_800_000, 1_600_000, 1_400_000, 1_000_000
            };
            int idx = Mathf.Clamp(leaguePosition - 1, 0, prizes.Length - 1);
            return prizes[idx];
        }

        /// <summary>
        /// Processes all weekly financial transactions for the team:
        /// wages, sponsor payments, and auto-saves to finance history.
        /// </summary>
        public void ProcessWeeklyFinances(TeamData team)
        {
            if (team?.finances == null) return;

            // Wages out
            long wageBill = team.squad?.Sum(p => (long)p.weeklyWage) ?? 0L;
            team.finances.weeklyWageBill = wageBill;

            if (wageBill > 0)
            {
                team.finances.AddTransaction(new FinanceTransaction
                {
                    date        = DateTime.UtcNow,
                    type        = FinanceTransactionType.Salary,
                    amount      = wageBill,
                    description = "Weekly wage bill"
                });
            }

            // Sponsor income
            long sponsor = CalculateSponsorRevenue(team);
            if (sponsor > 0)
            {
                team.finances.sponsorRevenue = sponsor;
                team.finances.AddTransaction(new FinanceTransaction
                {
                    date        = DateTime.UtcNow,
                    type        = FinanceTransactionType.Income,
                    amount      = sponsor,
                    description = "Sponsor revenue"
                });
            }

            CheckBankruptcy(team);
        }

        /// <summary>
        /// Checks whether the team has gone bankrupt (balance &lt; 0) and logs
        /// a warning.  In a full implementation this triggers board intervention.
        /// </summary>
        public bool CheckBankruptcy(TeamData team)
        {
            if (team?.finances == null) return false;
            if (team.finances.balance < 0)
            {
                Debug.LogWarning($"[EconomySystem] {team.name} is BANKRUPT! Balance: {team.finances.balance}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to upgrade the specified <paramref name="infrastructureType"/>
        /// for the team.  Deducts cost from balance on success.
        /// Returns <c>true</c> if the upgrade was purchased.
        /// </summary>
        public bool UpgradeInfrastructure(TeamData team, InfrastructureType infrastructureType)
        {
            if (team?.finances == null || team.infrastructure == null) return false;

            long cost;
            string name;
            int currentLevel;

            switch (infrastructureType)
            {
                case InfrastructureType.Stadium:
                    currentLevel = team.infrastructure.stadium?.upgradeLevel ?? 1;
                    cost = team.infrastructure.stadium?.GetUpgradeCost() ?? long.MaxValue;
                    name = "Stadium";
                    break;
                case InfrastructureType.TrainingFacility:
                    currentLevel = team.infrastructure.trainingFacility?.level ?? 1;
                    cost = 200_000L * (long)Math.Pow(2, currentLevel - 1);
                    name = "Training Facility";
                    break;
                case InfrastructureType.YouthAcademy:
                    currentLevel = team.infrastructure.youthAcademy?.level ?? 1;
                    cost = 150_000L * (long)Math.Pow(2, currentLevel - 1);
                    name = "Youth Academy";
                    break;
                case InfrastructureType.MedicalCenter:
                    currentLevel = team.infrastructure.medicalCenter?.level ?? 1;
                    cost = 100_000L * (long)Math.Pow(2, currentLevel - 1);
                    name = "Medical Center";
                    break;
                default:
                    return false;
            }

            if (currentLevel >= 5)
            {
                Debug.Log($"[EconomySystem] {name} is already at maximum level.");
                return false;
            }

            if (!team.finances.CanAfford(cost))
            {
                Debug.Log($"[EconomySystem] Cannot afford {name} upgrade. Cost: {cost}, Balance: {team.finances.balance}");
                return false;
            }

            // Apply upgrade
            switch (infrastructureType)
            {
                case InfrastructureType.Stadium:
                    team.infrastructure.stadium.upgradeLevel++;
                    team.infrastructure.stadium.capacity = (int)(team.infrastructure.stadium.capacity * 1.3f);
                    break;
                case InfrastructureType.TrainingFacility:
                    team.infrastructure.trainingFacility.level++;
                    break;
                case InfrastructureType.YouthAcademy:
                    team.infrastructure.youthAcademy.level++;
                    break;
                case InfrastructureType.MedicalCenter:
                    team.infrastructure.medicalCenter.level++;
                    break;
            }

            team.finances.AddTransaction(new FinanceTransaction
            {
                date        = DateTime.UtcNow,
                type        = FinanceTransactionType.Expense,
                amount      = cost,
                description = $"{name} upgraded to level {currentLevel + 1}"
            });

            Debug.Log($"[EconomySystem] {team.name} upgraded {name} to level {currentLevel + 1}.");
            return true;
        }
    }
}
