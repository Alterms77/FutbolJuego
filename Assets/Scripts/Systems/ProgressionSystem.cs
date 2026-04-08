using System.Collections.Generic;
using UnityEngine;

namespace FutbolJuego.Systems
{
    /// <summary>Hard currency types available in the game economy.</summary>
    public enum CurrencyType { Coins, PremiumCurrency }

    /// <summary>
    /// Handles manager XP, levelling, achievement unlocks, and currency rewards.
    /// </summary>
    public class ProgressionSystem
    {
        private int currentXP;
        private int currentLevel;
        private readonly List<string> unlockedAchievements = new List<string>();

        // Level thresholds: XP required to reach each level
        private static readonly int[] LevelThresholds =
        {
              0,   100,   250,   450,   700,
           1000,  1400,  1900,  2500,  3200,
           4000,  5000,  6200,  7600,  9200,
          11000, 13000, 15500, 18500, 22000
        };

        // ── XP & Levelling ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the manager level for a given total XP amount.
        /// </summary>
        public int CalculateManagerLevel(int xp)
        {
            int level = 0;
            for (int i = LevelThresholds.Length - 1; i >= 0; i--)
            {
                if (xp >= LevelThresholds[i])
                {
                    level = i + 1;
                    break;
                }
            }
            return Mathf.Max(1, level);
        }

        /// <summary>
        /// Returns the total XP required to reach <paramref name="level"/>.
        /// </summary>
        public int GetXPForLevel(int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, LevelThresholds.Length - 1);
            return LevelThresholds[idx];
        }

        /// <summary>
        /// Grants XP based on the match result and score margin.
        /// </summary>
        public void AwardMatchXP(bool win, bool draw, int goalDifference)
        {
            int xp = 0;
            if (win)  xp = 50 + goalDifference * 10;
            else if (draw) xp = 20;
            else xp = 5;

            int oldLevel = currentLevel;
            currentXP  += xp;
            currentLevel = CalculateManagerLevel(currentXP);

            if (currentLevel > oldLevel)
                Debug.Log($"[Progression] Level up! Now level {currentLevel}.");

            Debug.Log($"[Progression] Awarded {xp} XP. Total: {currentXP}.");
        }

        /// <summary>
        /// Records an achievement unlock by ID.
        /// Returns <c>false</c> if already unlocked.
        /// </summary>
        public void AwardAchievement(string achievementId)
        {
            if (unlockedAchievements.Contains(achievementId)) return;
            unlockedAchievements.Add(achievementId);
            Debug.Log($"[Progression] Achievement unlocked: {achievementId}");
        }

        /// <summary>
        /// Returns features that become available at the given manager level.
        /// </summary>
        public List<string> GetUnlockedFeatures(int managerLevel)
        {
            var features = new List<string>();

            if (managerLevel >= 1)  features.Add("Basic Training");
            if (managerLevel >= 2)  features.Add("Transfer Market");
            if (managerLevel >= 3)  features.Add("Youth Academy Scouting");
            if (managerLevel >= 4)  features.Add("Advanced Tactics");
            if (managerLevel >= 5)  features.Add("International Transfers");
            if (managerLevel >= 7)  features.Add("Match Prediction Tool");
            if (managerLevel >= 10) features.Add("Continental Competitions");
            if (managerLevel >= 15) features.Add("Custom Training Plans");

            return features;
        }

        /// <summary>
        /// Grants currency of the specified type.  In production wires to the
        /// economy backend.
        /// </summary>
        public void AwardCurrency(CurrencyType type, int amount)
        {
            Debug.Log($"[Progression] Awarded {amount} {type}.");
        }
    }
}
