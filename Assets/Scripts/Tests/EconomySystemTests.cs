using NUnit.Framework;
using System;
using System.Collections.Generic;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.Tests
{
    [TestFixture]
    public class EconomySystemTests
    {
        private EconomySystem economy;
        private TeamData team;

        [SetUp]
        public void Setup()
        {
            economy = new EconomySystem();
            team    = CreateTestTeam();
        }

        // ── Stadium revenue ───────────────────────────────────────────────────

        [Test]
        public void StadiumRevenue_CalculatedCorrectly()
        {
            team.infrastructure.stadium.capacity      = 30000;
            team.infrastructure.stadium.attendanceRate = 0.8f;
            team.infrastructure.stadium.ticketPrice   = 40f;
            team.infrastructure.stadium.condition     = 100;

            long revenue = economy.CalculateMatchDayRevenue(team);

            // Expected: 30000 * 0.8 * 40 * 1.0 = 960 000
            Assert.AreEqual(960_000L, revenue,
                "Stadium revenue formula does not match expected value.");
        }

        [Test]
        public void StadiumRevenue_ZeroAttendance_IsZero()
        {
            team.infrastructure.stadium.attendanceRate = 0f;
            long revenue = economy.CalculateMatchDayRevenue(team);
            Assert.AreEqual(0L, revenue);
        }

        // ── Wage bill ─────────────────────────────────────────────────────────

        [Test]
        public void WageBill_SumOfAllPlayerWages()
        {
            // Set explicit wages
            int expected = 0;
            foreach (var player in team.squad)
            {
                player.weeklyWage = 5000;
                expected += 5000;
            }

            economy.ProcessWeeklyFinances(team);
            Assert.AreEqual(expected, team.finances.weeklyWageBill,
                "Weekly wage bill must equal the sum of all player wages.");
        }

        // ── Bankruptcy ────────────────────────────────────────────────────────

        [Test]
        public void BankruptcyCheck_TriggersWhenBalanceNegative()
        {
            team.finances.balance = -1;
            bool bankrupt = economy.CheckBankruptcy(team);
            Assert.IsTrue(bankrupt, "Bankruptcy check should return true for negative balance.");
        }

        [Test]
        public void BankruptcyCheck_DoesNotTriggerWhenPositive()
        {
            team.finances.balance = 1_000_000;
            bool bankrupt = economy.CheckBankruptcy(team);
            Assert.IsFalse(bankrupt, "Bankruptcy check should return false for positive balance.");
        }

        // ── Transfer fee deduction ────────────────────────────────────────────

        [Test]
        public void TransferFee_DeductedFromBudget()
        {
            var transferSystem = new TransferMarketSystem();
            var buyer  = CreateTestTeam();
            var seller = CreateTestTeam();

            buyer.finances.balance        = 10_000_000;
            buyer.finances.transferBudget = 5_000_000;

            var player = seller.squad[0];
            player.marketValue = 1_000_000;
            player.overallRating = 70;
            player.CalculateOverall();

            int offer = 1_000_000;

            long budgetBefore = buyer.finances.transferBudget;
            bool success      = transferSystem.AttemptTransfer(buyer, seller, player, offer);

            Assert.IsTrue(success, "Transfer should succeed with sufficient budget.");
            Assert.Less(buyer.finances.transferBudget, budgetBefore,
                "Transfer budget must decrease after a successful purchase.");
        }

        // ── Prize money ───────────────────────────────────────────────────────

        [Test]
        public void PrizeMoney_FirstPlaceHigherThanLast()
        {
            long firstPrize = economy.CalculatePrizeMoney(team, 1);
            long lastPrize  = economy.CalculatePrizeMoney(team, 20);

            Assert.Greater(firstPrize, lastPrize,
                "First-place prize should exceed last-place prize.");
        }

        // ── Infrastructure upgrade ────────────────────────────────────────────

        [Test]
        public void UpgradeInfrastructure_FailsWhenCannotAfford()
        {
            team.finances.balance = 0;
            bool result = economy.UpgradeInfrastructure(team, InfrastructureType.Stadium);
            Assert.IsFalse(result, "Upgrade should fail when balance is zero.");
        }

        [Test]
        public void UpgradeInfrastructure_SucceedsWithEnoughFunds()
        {
            team.finances.balance          = 10_000_000;
            int levelBefore                = team.infrastructure.trainingFacility.level;
            bool result                    = economy.UpgradeInfrastructure(team, InfrastructureType.TrainingFacility);

            Assert.IsTrue(result, "Upgrade should succeed with sufficient balance.");
            Assert.Greater(team.infrastructure.trainingFacility.level, levelBefore,
                "Training facility level must increase.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TeamData CreateTestTeam()
        {
            var team = new TeamData
            {
                id       = Guid.NewGuid().ToString(),
                name     = "Test FC",
                shortName = "TST",
                morale   = 50,
                finances = new FinanceData
                {
                    balance        = 5_000_000,
                    transferBudget = 2_000_000,
                    wageBudget     = 50_000
                },
                infrastructure = new ClubInfrastructure
                {
                    stadium         = new StadiumData { capacity = 20000, attendanceRate = 0.75f, ticketPrice = 30f, condition = 80, upgradeLevel = 1 },
                    trainingFacility = new TrainingFacilityData { level = 1 },
                    youthAcademy    = new YouthAcademyData { level = 1 },
                    medicalCenter   = new MedicalCenterData { level = 1 }
                },
                squad        = new List<PlayerData>(),
                currentTactic = new TacticData()
            };

            for (int i = 0; i < 11; i++)
            {
                team.squad.Add(new PlayerData
                {
                    id          = Guid.NewGuid().ToString(),
                    name        = $"Player {i}",
                    age         = 24,
                    position    = i == 0 ? PlayerPosition.GK : PlayerPosition.CM,
                    isAvailable = true,
                    weeklyWage  = 5000,
                    marketValue = 1_000_000,
                    overallRating = 70,
                    attributes  = new PlayerAttributes { speed = 70, shooting = 70, passing = 70, defense = 70, physical = 70, intelligence = 70 }
                });
            }

            return team;
        }
    }
}
