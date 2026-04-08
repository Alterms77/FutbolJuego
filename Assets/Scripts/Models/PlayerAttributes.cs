using System;
using UnityEngine;

namespace FutbolJuego.Models
{
    /// <summary>
    /// The six core numerical attributes (0-99) used in all simulation maths,
    /// plus the goalkeeper-specific attribute.
    /// </summary>
    [Serializable]
    public class PlayerAttributes
    {
        /// <summary>Sprint speed and acceleration (0-99).</summary>
        public int speed;
        /// <summary>Finishing, long-shots, heading (0-99).</summary>
        public int shooting;
        /// <summary>Short/long passing, vision, crossing (0-99).</summary>
        public int passing;
        /// <summary>Tackling, marking, positioning out of possession (0-99).</summary>
        public int defense;
        /// <summary>Strength, stamina, aerial ability (0-99).</summary>
        public int physical;
        /// <summary>Decision-making, positioning, off-the-ball movement (0-99).</summary>
        public int intelligence;
        /// <summary>Reflexes, handling, positioning in goal (0-99). Meaningful only for GKs.</summary>
        public int goalkeeping;

        // ── Weights per position ───────────────────────────────────────────────

        // [speed, shooting, passing, defense, physical, intelligence, goalkeeping]
        private static readonly float[] WeightsGK  = { 0.05f, 0.00f, 0.10f, 0.05f, 0.10f, 0.15f, 0.55f };
        private static readonly float[] WeightsCB  = { 0.10f, 0.05f, 0.15f, 0.40f, 0.20f, 0.10f, 0.00f };
        private static readonly float[] WeightsLBRB= { 0.20f, 0.05f, 0.20f, 0.30f, 0.15f, 0.10f, 0.00f };
        private static readonly float[] WeightsCDM = { 0.10f, 0.05f, 0.25f, 0.30f, 0.15f, 0.15f, 0.00f };
        private static readonly float[] WeightsCM  = { 0.10f, 0.10f, 0.30f, 0.20f, 0.15f, 0.15f, 0.00f };
        private static readonly float[] WeightsCAM = { 0.10f, 0.20f, 0.30f, 0.05f, 0.10f, 0.25f, 0.00f };
        private static readonly float[] WeightsWing= { 0.25f, 0.20f, 0.20f, 0.05f, 0.10f, 0.20f, 0.00f };
        private static readonly float[] WeightsST  = { 0.20f, 0.35f, 0.10f, 0.05f, 0.20f, 0.10f, 0.00f };
        private static readonly float[] WeightsCF  = { 0.15f, 0.25f, 0.20f, 0.05f, 0.15f, 0.20f, 0.00f };

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a weighted overall rating (0-99) appropriate for
        /// <paramref name="position"/>.
        /// </summary>
        public int GetWeightedOverall(PlayerPosition position)
        {
            float[] w = GetWeights(position);
            float[] stats = { speed, shooting, passing, defense, physical, intelligence, goalkeeping };

            float sum = 0f;
            for (int i = 0; i < stats.Length; i++)
                sum += stats[i] * w[i];

            return Mathf.Clamp(Mathf.RoundToInt(sum), 0, 99);
        }

        /// <summary>Returns a shallow copy of this attribute block.</summary>
        public PlayerAttributes Clone()
        {
            return new PlayerAttributes
            {
                speed        = speed,
                shooting     = shooting,
                passing      = passing,
                defense      = defense,
                physical     = physical,
                intelligence = intelligence,
                goalkeeping  = goalkeeping
            };
        }

        /// <summary>Clamps all stats to the legal range [0, 99].</summary>
        public void Clamp()
        {
            speed        = Mathf.Clamp(speed,        0, 99);
            shooting     = Mathf.Clamp(shooting,     0, 99);
            passing      = Mathf.Clamp(passing,      0, 99);
            defense      = Mathf.Clamp(defense,      0, 99);
            physical     = Mathf.Clamp(physical,     0, 99);
            intelligence = Mathf.Clamp(intelligence, 0, 99);
            goalkeeping  = Mathf.Clamp(goalkeeping,  0, 99);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static float[] GetWeights(PlayerPosition pos) => pos switch
        {
            PlayerPosition.GK  => WeightsGK,
            PlayerPosition.CB  => WeightsCB,
            PlayerPosition.LB  => WeightsLBRB,
            PlayerPosition.RB  => WeightsLBRB,
            PlayerPosition.CDM => WeightsCDM,
            PlayerPosition.CM  => WeightsCM,
            PlayerPosition.CAM => WeightsCAM,
            PlayerPosition.LM  => WeightsWing,
            PlayerPosition.RM  => WeightsWing,
            PlayerPosition.LW  => WeightsWing,
            PlayerPosition.RW  => WeightsWing,
            PlayerPosition.ST  => WeightsST,
            PlayerPosition.CF  => WeightsCF,
            _                  => WeightsCM
        };
    }
}
