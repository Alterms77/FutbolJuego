using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>Focus areas a manager can assign to a training session.</summary>
    public enum TrainingFocus { Shooting, Passing, Defense, Speed, Physical, Tactical }

    /// <summary>
    /// Processes training sessions: calculates attribute gains, accumulates
    /// fatigue, and handles rest recovery.
    /// </summary>
    public class TrainingSystem
    {
        private const int MaxStatGain = 2;  // Max points per session
        private readonly System.Random rng = new System.Random();

        // ── Training session ───────────────────────────────────────────────────

        /// <summary>
        /// Applies a single training session to <paramref name="player"/>
        /// under the given <paramref name="focus"/> and facility quality.
        /// </summary>
        public void ProcessTrainingSession(PlayerData player, TrainingFocus focus,
                                           TrainingFacilityData facility)
        {
            if (player == null) return;
            if (!player.isAvailable || player.injuryDaysRemaining > 0) return;

            float gain    = CalculateTrainingGain(player, focus);
            float quality = facility?.qualityMultiplier ?? 1.0f;
            int finalGain = Mathf.Clamp(Mathf.RoundToInt(gain * quality), 0, MaxStatGain);

            ApplyGain(player, focus, finalGain);

            // Standard session adds 8-12 fatigue
            AccumulateFatigue(player, rng.Next(8, 13));

            player.CalculateOverall();
        }

        /// <summary>
        /// Calculates the raw training gain (in attribute points) for
        /// <paramref name="player"/> on the given focus.
        /// Gain is higher for young players and those far below their potential.
        /// </summary>
        public float CalculateTrainingGain(PlayerData player, TrainingFocus focus)
        {
            if (player == null) return 0f;

            float ageModifier     = player.GetAgeModifier();
            float potentialGap    = Mathf.Max(0f, player.potential - player.overallRating);
            float baseGain        = potentialGap * 0.04f * ageModifier; // 0-~2 points raw

            // Consistency affects whether the player actually trains well today
            float consistencyRoll = (float)rng.NextDouble();
            float consistencyMod  = (consistencyRoll < player.consistency / 100f) ? 1.0f : 0.5f;

            // Fatigue reduces training effectiveness
            float fatiguePenalty = 1.0f - (player.fatigue / 100f) * 0.4f;

            return baseGain * consistencyMod * fatiguePenalty;
        }

        // ── Fatigue ────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds <paramref name="sessionIntensity"/> fatigue points to the
        /// player (clamped to [0, 100]).
        /// </summary>
        public void AccumulateFatigue(PlayerData player, int sessionIntensity)
        {
            if (player == null) return;
            player.fatigue = Mathf.Clamp(player.fatigue + sessionIntensity, 0, 100);
        }

        /// <summary>
        /// Reduces player fatigue after <paramref name="restDays"/> days of rest.
        /// Full recovery (30+ days) returns fatigue to 0.
        /// </summary>
        public void RecoverFatigue(PlayerData player, int restDays)
        {
            if (player == null || restDays <= 0) return;
            // Recover ~4 fatigue per rest day
            int recovery = restDays * 4;
            player.fatigue = Mathf.Clamp(player.fatigue - recovery, 0, 100);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static void ApplyGain(PlayerData player, TrainingFocus focus, int gain)
        {
            if (gain <= 0) return;

            switch (focus)
            {
                case TrainingFocus.Shooting:
                    player.attributes.shooting = Mathf.Clamp(player.attributes.shooting + gain, 0, 99);
                    break;
                case TrainingFocus.Passing:
                    player.attributes.passing = Mathf.Clamp(player.attributes.passing + gain, 0, 99);
                    break;
                case TrainingFocus.Defense:
                    player.attributes.defense = Mathf.Clamp(player.attributes.defense + gain, 0, 99);
                    break;
                case TrainingFocus.Speed:
                    player.attributes.speed = Mathf.Clamp(player.attributes.speed + gain, 0, 99);
                    break;
                case TrainingFocus.Physical:
                    player.attributes.physical = Mathf.Clamp(player.attributes.physical + gain, 0, 99);
                    break;
                case TrainingFocus.Tactical:
                    player.attributes.intelligence = Mathf.Clamp(player.attributes.intelligence + gain, 0, 99);
                    break;
            }
        }
    }
}
