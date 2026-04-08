using System;
using System.Collections.Generic;
using UnityEngine;

namespace FutbolJuego.Models
{
    // ── Enumerations ───────────────────────────────────────────────────────────

    /// <summary>All legal positions a player can occupy.</summary>
    public enum PlayerPosition
    {
        GK,  // Goalkeeper
        CB,  // Centre-back
        LB,  // Left-back
        RB,  // Right-back
        CDM, // Defensive midfielder
        CM,  // Central midfielder
        CAM, // Attacking midfielder
        LM,  // Left midfielder / winger
        RM,  // Right midfielder / winger
        LW,  // Left winger
        RW,  // Right winger
        CF,  // Centre-forward / striker
        ST   // Striker
    }

    // ── PlayerData ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Full data model for a single player: visible stats, hidden attributes,
    /// contract info, and transient match-day state.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // ── Visible identity ───────────────────────────────────────────────────

        /// <summary>Unique player GUID.</summary>
        public string id;
        /// <summary>Full display name.</summary>
        public string name;
        /// <summary>Age in years.</summary>
        public int age;
        /// <summary>ISO 3166-1 alpha-2 nationality code (e.g. "BR", "DE").</summary>
        public string nationality;
        /// <summary>Primary playing position.</summary>
        public PlayerPosition position;
        /// <summary>Cached overall rating (0-99). Recalculate with <see cref="CalculateOverall"/>.</summary>
        public int overallRating;

        // ── Attributes ─────────────────────────────────────────────────────────

        /// <summary>Six-stat block (see <see cref="PlayerAttributes"/>).</summary>
        public PlayerAttributes attributes = new PlayerAttributes();

        // ── Hidden / scouting ──────────────────────────────────────────────────

        /// <summary>Maximum reachable overall (0-99). Hidden until scouted.</summary>
        public int potential;
        /// <summary>How consistently the player performs near their rating (0-100).</summary>
        public int consistency;
        /// <summary>Likelihood of sustaining injuries (0-100; higher = more prone).</summary>
        public int injuryProneness;

        // ── Contract ───────────────────────────────────────────────────────────

        /// <summary>Weekly wage in the game's currency unit.</summary>
        public int weeklyWage;
        /// <summary>Contract expiry date in UTC.</summary>
        public DateTime contractExpiry;
        /// <summary>Current market valuation in transfer currency.</summary>
        public int marketValue;

        // ── Match-day state ────────────────────────────────────────────────────

        /// <summary>Player morale (0-100; 50 = neutral).</summary>
        public int morale = 50;
        /// <summary>Accumulated fatigue (0-100; 0 = fully rested).</summary>
        public int fatigue;
        /// <summary>Days until the player returns from injury (0 = fit).</summary>
        public int injuryDaysRemaining;
        /// <summary>Whether the player can be selected for matches.</summary>
        public bool isAvailable = true;

        // ── Card system ────────────────────────────────────────────────────────

        /// <summary>
        /// Energy level (0-100; 100 = fully energised). Consumed during matches
        /// and restored by rest/training.
        /// </summary>
        public int energy = 100;

        /// <summary>
        /// Card rarity tier. Set in data or derived from <see cref="overallRating"/>:
        /// ≤69 Normal, 70-79 Silver, 80-84 Gold, 85-89 Star, ≥90 Legend.
        /// </summary>
        public PlayerRarity rarity = PlayerRarity.Normal;

        // ── Methods ────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates an overall rating (0-99) using position-specific
        /// attribute weights.
        /// </summary>
        public int CalculateOverall()
        {
            float raw = attributes.GetWeightedOverall(position);
            overallRating = Mathf.Clamp(Mathf.RoundToInt(raw), 0, 99);
            return overallRating;
        }

        /// <summary>
        /// Simulates attribute growth or decline over <paramref name="months"/>
        /// in-game months.  Players younger than peak improve; older players
        /// slowly decline.
        /// </summary>
        public void SimulateGrowth(int months)
        {
            if (months <= 0) return;

            float ageModifier   = GetAgeModifier();
            float potentialGap  = Mathf.Max(0f, potential - overallRating);
            // Max yearly growth proportional to potential gap
            float maxYearlyGain = potentialGap * 0.25f * ageModifier;
            float monthlyGain   = maxYearlyGain / 12f * months;

            var rng = new System.Random();

            void BumpStat(ref int stat, float weight)
            {
                float gain = monthlyGain * weight;
                // Add small noise ±30%
                gain *= (float)(0.7 + rng.NextDouble() * 0.6);
                stat  = Mathf.Clamp(stat + Mathf.RoundToInt(gain), 0, 99);
            }

            // Growth distributed by position weights
            switch (position)
            {
                case PlayerPosition.GK:
                    BumpStat(ref attributes.goalkeeping, 0.6f);
                    BumpStat(ref attributes.physical,    0.2f);
                    BumpStat(ref attributes.intelligence,0.2f);
                    break;
                case PlayerPosition.CB:
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    BumpStat(ref attributes.defense,     0.4f);
                    BumpStat(ref attributes.physical,    0.3f);
                    BumpStat(ref attributes.intelligence,0.2f);
                    BumpStat(ref attributes.passing,     0.1f);
                    break;
                case PlayerPosition.CDM:
                case PlayerPosition.CM:
                    BumpStat(ref attributes.passing,     0.3f);
                    BumpStat(ref attributes.intelligence,0.3f);
                    BumpStat(ref attributes.defense,     0.2f);
                    BumpStat(ref attributes.physical,    0.2f);
                    break;
                case PlayerPosition.CAM:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                    BumpStat(ref attributes.passing,     0.35f);
                    BumpStat(ref attributes.intelligence,0.25f);
                    BumpStat(ref attributes.speed,       0.2f);
                    BumpStat(ref attributes.shooting,    0.2f);
                    break;
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    BumpStat(ref attributes.speed,       0.35f);
                    BumpStat(ref attributes.shooting,    0.25f);
                    BumpStat(ref attributes.passing,     0.2f);
                    BumpStat(ref attributes.intelligence,0.2f);
                    break;
                case PlayerPosition.CF:
                case PlayerPosition.ST:
                    BumpStat(ref attributes.shooting,    0.4f);
                    BumpStat(ref attributes.physical,    0.25f);
                    BumpStat(ref attributes.speed,       0.2f);
                    BumpStat(ref attributes.intelligence,0.15f);
                    break;
            }

            CalculateOverall();
        }

        /// <summary>
        /// Returns an age-based growth multiplier.
        /// Peak window (26-30): 1.0.  Younger players grow faster (up to 1.5).
        /// Veterans decline (-0.05 per year after 30, floor 0.1).
        /// </summary>
        public float GetAgeModifier()
        {
            const int peakMin = 26;
            const int peakMax = 30;

            if (age < peakMin)
            {
                // Up to 1.5× for a 17-year-old, tapering to 1.0 at peak
                float yearsFromPeak = peakMin - age;
                return Mathf.Min(1.5f, 1.0f + yearsFromPeak * 0.1f);
            }
            if (age <= peakMax)
                return 1.0f;

            // Decline post-peak
            float yearsOver = age - peakMax;
            return Mathf.Max(0.1f, 1.0f - yearsOver * 0.05f);
        }
    }
}
