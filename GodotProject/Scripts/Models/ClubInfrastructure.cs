using System;

namespace FutbolJuego.Models
{
    // ── ClubInfrastructure ─────────────────────────────────────────────────────

    /// <summary>
    /// Aggregates all physical club facilities that affect match-day revenue,
    /// player development, and injury recovery.
    /// </summary>
    [Serializable]
    public class ClubInfrastructure
    {
        /// <summary>The club's home stadium.</summary>
        public StadiumData stadium = new StadiumData();
        /// <summary>Training ground and daily training quality.</summary>
        public TrainingFacilityData trainingFacility = new TrainingFacilityData();
        /// <summary>Youth academy that generates young talent.</summary>
        public YouthAcademyData youthAcademy = new YouthAcademyData();
        /// <summary>Medical centre that speeds recovery from injury.</summary>
        public MedicalCenterData medicalCenter = new MedicalCenterData();

        // ── Multiplier helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Returns a training-quality multiplier based on the training facility
        /// level (1.0 – 1.4 for levels 1-5).
        /// </summary>
        public float GetTrainingQualityMultiplier()
            => trainingFacility?.qualityMultiplier ?? 1.0f;

        /// <summary>
        /// Returns a recovery speed multiplier based on the medical centre
        /// level (1.15 – 1.75 for levels 1-5).
        /// </summary>
        public float GetRecoverySpeedMultiplier()
            => medicalCenter?.recoveryMultiplier ?? 1.0f;

        /// <summary>
        /// Returns a bonus to youth player generation quality based on the
        /// academy level (0-35 points bonus to min/max).
        /// </summary>
        public float GetYouthPlayerQualityBonus()
            => youthAcademy != null ? youthAcademy.level * 7f : 0f;
    }

    // ── StadiumData ────────────────────────────────────────────────────────────

    /// <summary>Stadium capacity, condition, and match-day revenue.</summary>
    [Serializable]
    public class StadiumData
    {
        /// <summary>Stadium display name.</summary>
        public string name = "Home Ground";
        /// <summary>Total seated capacity.</summary>
        public int capacity = 15000;
        /// <summary>Structural condition (0-100; below 40 reduces attendance).</summary>
        public int condition = 80;
        /// <summary>Current upgrade level (1-5).</summary>
        public int upgradeLevel = 1;
        /// <summary>Average ticket price.</summary>
        public float ticketPrice = 25f;
        /// <summary>Attendance fill rate (0-1; influenced by league position and morale).</summary>
        public float attendanceRate = 0.70f;

        /// <summary>
        /// Estimated income for a single home match.
        /// Formula: capacity × attendanceRate × ticketPrice × condition modifier.
        /// </summary>
        public long CalculateMatchDayRevenue()
        {
            float conditionMod = 0.7f + (condition / 100f) * 0.3f; // 0.70 – 1.00
            long revenue = (long)(capacity * attendanceRate * ticketPrice * conditionMod);
            return Math.Max(0, revenue);
        }

        /// <summary>
        /// Cost to upgrade the stadium to the next level.
        /// Scales exponentially: level 1→2 costs 500 000, doubling each step.
        /// </summary>
        public long GetUpgradeCost()
        {
            if (upgradeLevel >= 5) return long.MaxValue;
            return 500_000L * (long)Math.Pow(2, upgradeLevel - 1);
        }
    }

    // ── TrainingFacilityData ───────────────────────────────────────────────────

    /// <summary>Training ground data affecting daily attribute growth.</summary>
    [Serializable]
    public class TrainingFacilityData
    {
        /// <summary>Facility level (1-5).</summary>
        public int level = 1;

        /// <summary>
        /// Quality multiplier applied to training gains.
        /// Formula: 0.8 + level × 0.1  →  0.9 (L1) … 1.3 (L5).
        /// </summary>
        public float qualityMultiplier => 0.8f + level * 0.1f;
    }

    // ── YouthAcademyData ───────────────────────────────────────────────────────

    /// <summary>Youth academy data controlling generated youngster quality.</summary>
    [Serializable]
    public class YouthAcademyData
    {
        /// <summary>Academy level (1-5).</summary>
        public int level = 1;

        /// <summary>Minimum overall of generated youth players.</summary>
        public int youthPlayerQualityMin => 40 + level * 5;

        /// <summary>Maximum overall of generated youth players.</summary>
        public int youthPlayerQualityMax => 60 + level * 7;
    }

    // ── MedicalCenterData ──────────────────────────────────────────────────────

    /// <summary>Medical centre data affecting injury recovery speed.</summary>
    [Serializable]
    public class MedicalCenterData
    {
        /// <summary>Medical centre level (1-5).</summary>
        public int level = 1;

        /// <summary>
        /// Recovery speed multiplier applied to injury duration.
        /// Formula: 1.0 + level × 0.15  →  1.15 (L1) … 1.75 (L5).
        /// </summary>
        public float recoveryMultiplier => 1.0f + level * 0.15f;
    }
}
