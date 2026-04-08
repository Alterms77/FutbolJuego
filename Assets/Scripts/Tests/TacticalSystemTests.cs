using NUnit.Framework;
using System.Collections.Generic;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.Tests
{
    [TestFixture]
    public class TacticalSystemTests
    {
        private TacticalSystem tacticalSystem;

        [SetUp]
        public void Setup()
        {
            tacticalSystem = new TacticalSystem();
        }

        // ── Formation positions ───────────────────────────────────────────────

        [Test]
        public void FormationPositions_F433_ReturnsEleven()
        {
            var positions = tacticalSystem.GetFormationPositions(Formation.F433);
            Assert.AreEqual(11, positions.Count,
                "F433 must have exactly 11 position coordinates.");
        }

        [Test]
        public void FormationPositions_AllFormations_ReturnEleven()
        {
            foreach (Formation formation in System.Enum.GetValues(typeof(Formation)))
            {
                var positions = tacticalSystem.GetFormationPositions(formation);
                Assert.AreEqual(11, positions.Count,
                    $"{formation} did not return 11 positions.");
            }
        }

        [Test]
        public void FormationPositions_CoordinatesWithinBounds()
        {
            foreach (Formation formation in System.Enum.GetValues(typeof(Formation)))
            {
                var positions = tacticalSystem.GetFormationPositions(formation);
                foreach (var pos in positions)
                {
                    Assert.GreaterOrEqual(pos.x, 0f, $"{formation}: x < 0");
                    Assert.LessOrEqual(pos.x,   100f, $"{formation}: x > 100");
                    Assert.GreaterOrEqual(pos.y, 0f, $"{formation}: y < 0");
                    Assert.LessOrEqual(pos.y,   100f, $"{formation}: y > 100");
                }
            }
        }

        // ── Tactical effectiveness ────────────────────────────────────────────

        [Test]
        public void TacticalEffectiveness_HighPressingVsSlowTempo()
        {
            var highPressTactic = new TacticData
            {
                formation = Formation.F433,
                playStyle = PlayStyle.HighPress,
                pressing  = 80,
                tempo     = 70
            };
            var slowTempo = new TacticData
            {
                formation = Formation.F442,
                playStyle = PlayStyle.Possession,
                pressing  = 40,
                tempo     = 30
            };

            float effectiveness = tacticalSystem.CalculatePressingEffectiveness(
                highPressTactic.pressing, slowTempo.tempo);

            Assert.Greater(effectiveness, 0.5f,
                "High press vs low tempo should produce effectiveness > 0.5.");
        }

        [Test]
        public void TacticalEffectiveness_ReturnsValueInRange()
        {
            var tactic = new TacticData { formation = Formation.F433, playStyle = PlayStyle.Possession };
            var squad  = CreateTestSquad(11);

            float eff = tacticalSystem.CalculateTacticalEffectiveness(tactic, squad);

            Assert.GreaterOrEqual(eff, 0f, "Effectiveness must be ≥ 0.");
            Assert.LessOrEqual(eff,    1f, "Effectiveness must be ≤ 1.");
        }

        // ── Formation matchup ─────────────────────────────────────────────────

        [Test]
        public void FormationMatchup_F532_CountersF433()
        {
            // F532 is listed as countering F433 in the matchup table (0.93 for F433 vs F532)
            float mult = tacticalSystem.CalculateFormationMatchup(Formation.F433, Formation.F532);
            Assert.Less(mult, 1.0f,
                "F433 attacking vs F532 defending should yield a multiplier < 1.0.");
        }

        [Test]
        public void FormationMatchup_NeutralPairingReturnsOne()
        {
            // Pair not in matchup table → neutral 1.0
            float mult = tacticalSystem.CalculateFormationMatchup(Formation.F442, Formation.F343);
            Assert.AreEqual(1.0f, mult, 0.001f,
                "Unlisted formation pair should return neutral 1.0.");
        }

        [Test]
        public void FormationMatchup_HomeVsAway_MultiplierInRange()
        {
            foreach (Formation home in System.Enum.GetValues(typeof(Formation)))
            foreach (Formation away in System.Enum.GetValues(typeof(Formation)))
            {
                float mult = tacticalSystem.CalculateFormationMatchup(home, away);
                Assert.GreaterOrEqual(mult, 0.7f, $"Matchup {home} vs {away} below minimum.");
                Assert.LessOrEqual(mult,    1.3f, $"Matchup {home} vs {away} above maximum.");
            }
        }

        // ── Pressing effectiveness ────────────────────────────────────────────

        [Test]
        public void PressingEffectiveness_HighPressLowTempo_MaximumEffect()
        {
            float eff = tacticalSystem.CalculatePressingEffectiveness(100, 0f);
            Assert.AreEqual(1.0f, eff, 0.01f,
                "100% pressing vs 0 tempo should yield maximum effectiveness ≈ 1.0.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static List<PlayerData> CreateTestSquad(int count)
        {
            var squad = new List<PlayerData>();
            for (int i = 0; i < count; i++)
            {
                squad.Add(new PlayerData
                {
                    id          = $"p{i}",
                    name        = $"Player {i}",
                    age         = 25,
                    position    = i == 0 ? PlayerPosition.GK : PlayerPosition.CM,
                    overallRating = 72,
                    isAvailable = true,
                    attributes  = new PlayerAttributes
                    {
                        speed = 72, shooting = 72, passing = 72,
                        defense = 72, physical = 72, intelligence = 72
                    }
                });
            }
            return squad;
        }
    }
}
