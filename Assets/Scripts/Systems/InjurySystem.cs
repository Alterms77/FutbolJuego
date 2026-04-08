using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>Severity level of a player injury.</summary>
    public enum InjurySeverity { Minor, Moderate, Serious, Severe }

    /// <summary>
    /// Determines injury probability, severity, and manages daily recovery.
    /// </summary>
    public class InjurySystem
    {
        private readonly System.Random rng = new System.Random();

        // ── Injury probability ─────────────────────────────────────────────────

        /// <summary>
        /// Rolls for a match-time injury.  Probability driven by fatigue,
        /// injuryProneness, and match intensity.
        /// Returns <c>true</c> if the player sustains an injury.
        /// </summary>
        public bool CheckMatchInjury(PlayerData player, float matchIntensity)
        {
            if (player == null) return false;

            float baseProbability  = 0.02f;                             // 2% base per match
            float fatigueFactor    = player.fatigue / 100f * 0.03f;    // up to +3%
            float proneFactor      = player.injuryProneness / 100f * 0.04f; // up to +4%
            float intensityFactor  = matchIntensity * 0.02f;            // up to +2%

            float probability = baseProbability + fatigueFactor + proneFactor + intensityFactor;
            bool injured      = rng.NextDouble() < probability;

            if (injured)
            {
                ApplyInjury(player, null);
            }
            return injured;
        }

        /// <summary>
        /// Rolls for a training injury.  Lower probability than match injuries.
        /// </summary>
        public bool CheckTrainingInjury(PlayerData player, int trainingIntensity)
        {
            if (player == null) return false;

            float intensityNorm   = trainingIntensity / 100f;
            float probability     = 0.005f
                                  + player.fatigue / 100f * 0.02f
                                  + player.injuryProneness / 100f * 0.02f
                                  + intensityNorm * 0.01f;

            bool injured = rng.NextDouble() < probability;
            if (injured) ApplyInjury(player, null);
            return injured;
        }

        /// <summary>
        /// Returns the expected recovery duration (in days) for the given
        /// severity, reduced by the medical centre quality.
        /// </summary>
        public int GetInjuryDuration(InjurySeverity severity, MedicalCenterData medicalCenter)
        {
            int baseDays = severity switch
            {
                InjurySeverity.Minor    => rng.Next(3, 8),
                InjurySeverity.Moderate => rng.Next(10, 22),
                InjurySeverity.Serious  => rng.Next(25, 50),
                InjurySeverity.Severe   => rng.Next(55, 120),
                _                       => 7
            };

            float recoveryMult = medicalCenter?.recoveryMultiplier ?? 1.0f;
            return Mathf.Max(1, Mathf.RoundToInt(baseDays / recoveryMult));
        }

        /// <summary>
        /// Advances injury recovery by one day.  Sets
        /// <see cref="PlayerData.isAvailable"/> once fully healed.
        /// </summary>
        public void ProcessInjuryRecovery(PlayerData player, MedicalCenterData medicalCenter)
        {
            if (player == null || player.injuryDaysRemaining <= 0) return;

            float recoveryMult = medicalCenter?.recoveryMultiplier ?? 1.0f;
            int reduction      = Mathf.Max(1, Mathf.RoundToInt(1f * recoveryMult));

            player.injuryDaysRemaining = Mathf.Max(0, player.injuryDaysRemaining - reduction);

            if (player.injuryDaysRemaining <= 0)
            {
                player.isAvailable = true;
                player.fatigue     = Mathf.Max(0, player.fatigue - 10); // rest during injury
                Debug.Log($"[InjurySystem] {player.name} has returned from injury.");
            }
        }

        /// <summary>
        /// Randomly rolls an injury severity.
        /// Minor (50%), Moderate (30%), Serious (15%), Severe (5%).
        /// </summary>
        public InjurySeverity RollInjurySeverity()
        {
            double roll = rng.NextDouble();
            if (roll < 0.50) return InjurySeverity.Minor;
            if (roll < 0.80) return InjurySeverity.Moderate;
            if (roll < 0.95) return InjurySeverity.Serious;
            return InjurySeverity.Severe;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void ApplyInjury(PlayerData player, MedicalCenterData medicalCenter)
        {
            InjurySeverity severity         = RollInjurySeverity();
            int days                        = GetInjuryDuration(severity, medicalCenter);
            player.injuryDaysRemaining      = days;
            player.isAvailable              = false;

            Debug.Log($"[InjurySystem] {player.name} sustained a {severity} injury ({days} days).");
        }
    }
}
