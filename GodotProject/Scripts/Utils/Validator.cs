using System.Collections.Generic;
using FutbolJuego.Models;

namespace FutbolJuego.Utils
{
    // ── Validation result ──────────────────────────────────────────────────────

    /// <summary>Holds the outcome of a data validation check.</summary>
    public class ValidationResult
    {
        /// <summary>Whether all validation rules passed.</summary>
        public bool IsValid;
        /// <summary>Human-readable error messages for failed rules.</summary>
        public List<string> Errors = new List<string>();

        /// <summary>Creates a passing result.</summary>
        public static ValidationResult Pass() => new ValidationResult { IsValid = true };

        /// <summary>Creates a failing result with one error message.</summary>
        public static ValidationResult Fail(string error) =>
            new ValidationResult { IsValid = false, Errors = new List<string> { error } };
    }

    // ── Validator ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Static helpers for validating model objects before simulation or
    /// persisting data.
    /// </summary>
    public static class Validator
    {
        // ── Primitives ─────────────────────────────────────────────────────────

        /// <summary>Returns <c>true</c> if <paramref name="value"/> is in [0, 99].</summary>
        public static bool IsValidStat(int value) => value >= 0 && value <= 99;

        /// <summary>Returns <c>true</c> if <paramref name="age"/> is in [15, 45].</summary>
        public static bool IsValidAge(int age) => age >= 15 && age <= 45;

        /// <summary>Returns <c>true</c> if <paramref name="value"/> is in [0, 100].</summary>
        public static bool IsValidPercentage(int value) => value >= 0 && value <= 100;

        // ── PlayerData ─────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that a <see cref="PlayerData"/> object has sensible
        /// attribute values and required fields.
        /// </summary>
        public static ValidationResult ValidatePlayer(PlayerData player)
        {
            var result = new ValidationResult { IsValid = true };

            if (player == null)
                return ValidationResult.Fail("PlayerData is null.");

            if (string.IsNullOrEmpty(player.id))
                result.Errors.Add("Player ID is empty.");

            if (string.IsNullOrEmpty(player.name))
                result.Errors.Add("Player name is empty.");

            if (!IsValidAge(player.age))
                result.Errors.Add($"Player age {player.age} is out of range [15, 45].");

            if (player.attributes != null)
            {
                CheckStat(result, "speed",        player.attributes.speed);
                CheckStat(result, "shooting",     player.attributes.shooting);
                CheckStat(result, "passing",      player.attributes.passing);
                CheckStat(result, "defense",      player.attributes.defense);
                CheckStat(result, "physical",     player.attributes.physical);
                CheckStat(result, "intelligence", player.attributes.intelligence);
            }
            else
            {
                result.Errors.Add("PlayerAttributes is null.");
            }

            if (!IsValidPercentage(player.morale))
                result.Errors.Add($"Morale {player.morale} out of range [0, 100].");

            if (!IsValidPercentage(player.fatigue))
                result.Errors.Add($"Fatigue {player.fatigue} out of range [0, 100].");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        // ── TacticData ─────────────────────────────────────────────────────────

        /// <summary>
        /// Validates a <see cref="TacticData"/> object and checks that the
        /// required positions are filled by members of <paramref name="squad"/>.
        /// </summary>
        public static ValidationResult ValidateTactic(TacticData tactic, List<PlayerData> squad)
        {
            var result = new ValidationResult { IsValid = true };

            if (tactic == null)
                return ValidationResult.Fail("TacticData is null.");

            CheckRange(result, "pressing",      tactic.pressing,      0, 100);
            CheckRange(result, "tempo",         tactic.tempo,         0, 100);
            CheckRange(result, "width",         tactic.width,         0, 100);
            CheckRange(result, "defensiveLine", tactic.defensiveLine, 0, 100);

            // Verify captain exists in squad
            if (!string.IsNullOrEmpty(tactic.captainId) && squad != null)
            {
                bool captainFound = squad.Exists(p => p.id == tactic.captainId);
                if (!captainFound)
                    result.Errors.Add($"Captain ID '{tactic.captainId}' not found in squad.");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        // ── FinanceData ────────────────────────────────────────────────────────

        /// <summary>
        /// Validates finance data for negative budgets or impossible values.
        /// </summary>
        public static ValidationResult ValidateFinances(FinanceData finance)
        {
            var result = new ValidationResult { IsValid = true };

            if (finance == null)
                return ValidationResult.Fail("FinanceData is null.");

            if (finance.weeklyWageBill < 0)
                result.Errors.Add("Weekly wage bill cannot be negative.");

            if (finance.transferBudget < 0)
                result.Errors.Add("Transfer budget cannot be negative.");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static void CheckStat(ValidationResult result, string name, int value)
        {
            if (!IsValidStat(value))
                result.Errors.Add($"Stat '{name}' = {value} is out of range [0, 99].");
        }

        private static void CheckRange(ValidationResult result, string name,
                                        int value, int min, int max)
        {
            if (value < min || value > max)
                result.Errors.Add($"Field '{name}' = {value} out of range [{min}, {max}].");
        }
    }
}
