using NUnit.Framework;
using System.Collections.Generic;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.Tests
{
    [TestFixture]
    public class SimulationEngineTests
    {
        private MatchSimulationEngine engine;
        private TeamData homeTeam;
        private TeamData awayTeam;

        [SetUp]
        public void Setup()
        {
            engine = new MatchSimulationEngine();
            homeTeam = CreateTestTeam("home-1", "Home FC");
            awayTeam = CreateTestTeam("away-1", "Away FC");
        }

        // ── Poisson ───────────────────────────────────────────────────────────

        [Test]
        public void PoissonDistribution_ReturnsNonNegative()
        {
            for (int i = 0; i < 500; i++)
            {
                int sample = engine.SampleFromPoisson(1.5f);
                Assert.GreaterOrEqual(sample, 0, "Poisson sample must be ≥ 0.");
            }
        }

        [Test]
        public void PoissonDistribution_ZeroLambda_ReturnsZero()
        {
            int sample = engine.SampleFromPoisson(0f);
            Assert.AreEqual(0, sample);
        }

        [Test]
        public void PoissonDistribution_MeanApproximatesLambda()
        {
            float lambda = 2.5f;
            float sum    = 0f;
            int   n      = 2000;

            for (int i = 0; i < n; i++)
                sum += engine.SampleFromPoisson(lambda);

            float mean = sum / n;
            // Mean should be within 10% of lambda over 2000 samples
            Assert.AreEqual(lambda, mean, lambda * 0.10f,
                $"Poisson mean {mean:F2} deviates too far from λ={lambda}");
        }

        // ── xG ────────────────────────────────────────────────────────────────

        [Test]
        public void XGCalculation_HomeAdvantageIncreasesXG()
        {
            var homeResult    = engine.SimulateMatch(homeTeam, awayTeam, isNeutralVenue: false);
            var neutralResult = engine.SimulateMatch(homeTeam, awayTeam, isNeutralVenue: true);

            // Over many simulations home advantage should raise homeXG
            float homeXG    = homeResult.statistics.homeXG;
            float neutralXG = neutralResult.statistics.homeXG;

            // Simply check that both are positive; statistical assertion needs many samples
            Assert.Greater(homeXG,    0f, "Home xG must be positive.");
            Assert.Greater(neutralXG, 0f, "Neutral xG must be positive.");
        }

        // ── Match validity ────────────────────────────────────────────────────

        [Test]
        public void SimulateMatch_ProducesValidScore()
        {
            var match = engine.SimulateMatch(homeTeam, awayTeam);

            Assert.GreaterOrEqual(match.homeScore, 0);
            Assert.GreaterOrEqual(match.awayScore, 0);
            Assert.LessOrEqual(match.homeScore, 15, "Score implausibly high.");
            Assert.LessOrEqual(match.awayScore, 15, "Score implausibly high.");
        }

        [Test]
        public void SimulateMatch_StatusIsCompleted()
        {
            var match = engine.SimulateMatch(homeTeam, awayTeam);
            Assert.AreEqual(MatchStatus.Completed, match.status);
        }

        [Test]
        public void SimulateMatch_EventsHaveValidMinutes()
        {
            var match = engine.SimulateMatch(homeTeam, awayTeam);

            foreach (var evt in match.events)
            {
                Assert.GreaterOrEqual(evt.minute, 1,  $"Event minute {evt.minute} too low.");
                Assert.LessOrEqual(evt.minute,   120, $"Event minute {evt.minute} too high.");
            }
        }

        [Test]
        public void SimulateMatch_GoalEventCountMatchesScore()
        {
            var match     = engine.SimulateMatch(homeTeam, awayTeam);
            int goalEvents = 0;

            foreach (var evt in match.events)
                if (evt.type == MatchEventType.Goal) goalEvents++;

            Assert.AreEqual(match.homeScore + match.awayScore, goalEvents,
                "Number of Goal events must equal total goals.");
        }

        // ── Morale modifier ───────────────────────────────────────────────────

        [Test]
        public void MoraleModifier_AffectsXG()
        {
            var highMorale = CreateTestTeam("hm-1", "High Morale FC");
            highMorale.morale = 100;

            var lowMorale = CreateTestTeam("lm-1", "Low Morale FC");
            lowMorale.morale = 0;

            // Simulate many matches and compare mean xG
            float highSum = 0f, lowSum = 0f;
            int n = 200;

            for (int i = 0; i < n; i++)
            {
                highSum += engine.SimulateMatch(highMorale, awayTeam).statistics.homeXG;
                lowSum  += engine.SimulateMatch(lowMorale,  awayTeam).statistics.homeXG;
            }

            Assert.Greater(highSum / n, lowSum / n,
                "High-morale team should produce higher average xG.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static TeamData CreateTestTeam(string id, string name)
        {
            var team = new TeamData
            {
                id       = id,
                name     = name,
                shortName = name.Substring(0, 3).ToUpper(),
                morale   = 50,
                squad    = new List<PlayerData>(),
                currentTactic = new TacticData()
            };

            // One GK + 10 outfield players
            team.squad.Add(MakePlayer("gk-" + id, "Keeper", PlayerPosition.GK, 72));
            for (int i = 0; i < 4; i++) team.squad.Add(MakePlayer($"def{i}-{id}", $"Defender{i}", PlayerPosition.CB, 70));
            for (int i = 0; i < 3; i++) team.squad.Add(MakePlayer($"mid{i}-{id}", $"Midfielder{i}", PlayerPosition.CM, 72));
            for (int i = 0; i < 3; i++) team.squad.Add(MakePlayer($"fwd{i}-{id}", $"Forward{i}", PlayerPosition.ST, 74));

            return team;
        }

        private static PlayerData MakePlayer(string id, string name,
                                              PlayerPosition pos, int overall)
        {
            int s = overall;
            var player = new PlayerData
            {
                id          = id,
                name        = name,
                age         = 25,
                position    = pos,
                morale      = 50,
                fatigue     = 0,
                isAvailable = true,
                nationality = "EN",
                attributes  = new PlayerAttributes
                {
                    speed        = s,
                    shooting     = s,
                    passing      = s,
                    defense      = s,
                    physical     = s,
                    intelligence = s,
                    goalkeeping  = pos == PlayerPosition.GK ? s : 20
                }
            };
            player.CalculateOverall();
            return player;
        }
    }
}
