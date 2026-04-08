using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Utils
{
    /// <summary>Extension methods for <see cref="List{T}"/>.</summary>
    public static class ListExtensions
    {
        private static readonly System.Random Rng = new System.Random();

        /// <summary>Shuffles the list in-place using the Fisher-Yates algorithm.</summary>
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>Returns a random element from the list, or <c>default</c> if empty.</summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Rng.Next(list.Count)];
        }

        /// <summary>
        /// Returns the element at <paramref name="index"/>, or <c>default</c>
        /// if the index is out of range (avoids <see cref="ArgumentOutOfRangeException"/>).
        /// </summary>
        public static T SafeGet<T>(this List<T> list, int index)
        {
            if (list == null || index < 0 || index >= list.Count) return default;
            return list[index];
        }
    }

    /// <summary>Extension methods for <see cref="int"/>.</summary>
    public static class IntExtensions
    {
        /// <summary>Clamps the value to [0, 99] — the legal stat range.</summary>
        public static int ClampStat(this int value) =>
            Mathf.Clamp(value, Constants.StatMin, Constants.StatMax);

        /// <summary>Clamps the value to [<paramref name="min"/>, <paramref name="max"/>].</summary>
        public static int Clamp(this int value, int min, int max) =>
            Mathf.Clamp(value, min, max);
    }

    /// <summary>Extension methods for <see cref="float"/>.</summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Converts the float to a percentage string, e.g. 0.753 → "75.3%".
        /// </summary>
        public static string ToPercentageString(this float value, int decimals = 1)
        {
            return (value * 100f).ToString($"F{decimals}") + "%";
        }

        /// <summary>Clamps the value to [0, 1].</summary>
        public static float Clamp01(this float value) => Mathf.Clamp01(value);
    }

    /// <summary>Extension methods for <see cref="PlayerData"/>.</summary>
    public static class PlayerDataExtensions
    {
        /// <summary>Returns <c>true</c> if the player is a goalkeeper.</summary>
        public static bool IsGoalkeeper(this PlayerPosition position) =>
            position == PlayerPosition.GK;

        /// <summary>Returns <c>true</c> if the player plays in defence.</summary>
        public static bool IsDefender(this PlayerPosition position) =>
            position == PlayerPosition.CB ||
            position == PlayerPosition.LB ||
            position == PlayerPosition.RB;

        /// <summary>Returns <c>true</c> if the player plays in midfield.</summary>
        public static bool IsMidfielder(this PlayerPosition position) =>
            position == PlayerPosition.CDM ||
            position == PlayerPosition.CM  ||
            position == PlayerPosition.CAM ||
            position == PlayerPosition.LM  ||
            position == PlayerPosition.RM;

        /// <summary>Returns <c>true</c> if the player plays in attack.</summary>
        public static bool IsForward(this PlayerPosition position) =>
            position == PlayerPosition.LW ||
            position == PlayerPosition.RW ||
            position == PlayerPosition.CF ||
            position == PlayerPosition.ST;
    }
}
