using System;
using System.Collections.Generic;
using FutbolJuego.Models;
using Godot;

namespace FutbolJuego.Systems
{
    // ── Event reward ───────────────────────────────────────────────────────────

    /// <summary>A reward item for completing a live event.</summary>
    [Serializable]
    public class EventReward
    {
        /// <summary>Type of currency awarded.</summary>
        public RewardCurrencyType currencyType;
        /// <summary>Amount of currency.</summary>
        public int amount;
        /// <summary>Optional: item/asset reward ID.</summary>
        public string itemId;
    }

    // ── Live event ─────────────────────────────────────────────────────────────

    /// <summary>A timed live event with rewards for participation.</summary>
    [Serializable]
    public class LiveEvent
    {
        /// <summary>Unique event identifier.</summary>
        public string id;
        /// <summary>Display name of the event.</summary>
        public string name;
        /// <summary>UTC start time.</summary>
        public DateTime startDate;
        /// <summary>UTC end time.</summary>
        public DateTime endDate;
        /// <summary>Player-facing description.</summary>
        public string description;
        /// <summary>Rewards available on completion.</summary>
        public List<EventReward> rewards = new List<EventReward>();

        /// <summary>Whether this event is currently active.</summary>
        public bool IsActive => DateTime.UtcNow >= startDate && DateTime.UtcNow <= endDate;
    }

    // ── Daily reward ───────────────────────────────────────────────────────────

    /// <summary>A single day's reward in the weekly login cycle.</summary>
    [Serializable]
    public class DailyReward
    {
        /// <summary>Day number in the 7-day weekly cycle (1-7).</summary>
        public int day;
        /// <summary>Type of currency awarded.</summary>
        public RewardCurrencyType currencyType;
        /// <summary>Amount of currency.</summary>
        public int amount;
    }

    // ── Weekly challenge ───────────────────────────────────────────────────────

    /// <summary>A weekly challenge with a target and reward.</summary>
    [Serializable]
    public class WeeklyChallenge
    {
        /// <summary>Unique challenge ID.</summary>
        public string id;
        /// <summary>Human-readable description.</summary>
        public string description;
        /// <summary>Target value (e.g. goals to score).</summary>
        public int targetValue;
        /// <summary>Current progress toward the target.</summary>
        public int currentProgress;
        /// <summary>Whether the reward has been claimed.</summary>
        public bool isClaimed;
        /// <summary>Challenge reward.</summary>
        public EventReward reward;
    }

    // ── LiveEventSystem ────────────────────────────────────────────────────────

    /// <summary>
    /// Manages live events, daily login rewards, and weekly challenges.
    /// </summary>
    public class LiveEventSystem
    {
        private readonly List<LiveEvent> activeEvents = new List<LiveEvent>();
        private readonly HashSet<string> claimedDays = new HashSet<string>();
        private WeeklyChallenge currentChallenge;

        private static readonly DailyReward[] WeeklyRewards =
        {
            new DailyReward { day = 1, currencyType = RewardCurrencyType.Coins,           amount = 500   },
            new DailyReward { day = 2, currencyType = RewardCurrencyType.Coins,           amount = 750   },
            new DailyReward { day = 3, currencyType = RewardCurrencyType.PremiumCurrency, amount = 10    },
            new DailyReward { day = 4, currencyType = RewardCurrencyType.Coins,           amount = 1000  },
            new DailyReward { day = 5, currencyType = RewardCurrencyType.Coins,           amount = 1500  },
            new DailyReward { day = 6, currencyType = RewardCurrencyType.PremiumCurrency, amount = 25    },
            new DailyReward { day = 7, currencyType = RewardCurrencyType.PremiumCurrency, amount = 50    },
        };

        /// <summary>
        /// Returns all currently active live events.
        /// </summary>
        public List<LiveEvent> GetActiveEvents()
        {
            return activeEvents.FindAll(e => e.IsActive);
        }

        /// <summary>
        /// Retrieves the daily reward definition for a given day (1-7).
        /// </summary>
        public DailyReward GetDailyReward(int day)
        {
            int idx = day - 1;
            if (idx < 0 || idx >= WeeklyRewards.Length) return null;
            return WeeklyRewards[idx];
        }

        /// <summary>
        /// Claims the daily reward for <paramref name="day"/>.
        /// Returns <c>false</c> if already claimed today.
        /// </summary>
        public bool ClaimDailyReward(int day)
        {
            string key = $"{DateTime.UtcNow:yyyy-MM-dd}-day{day}";
            if (claimedDays.Contains(key)) return false;

            DailyReward reward = GetDailyReward(day);
            if (reward == null) return false;

            claimedDays.Add(key);
            GD.Print($"[LiveEvents] Claimed day {day} reward: {reward.amount} {reward.currencyType}");
            return true;
        }

        /// <summary>Returns the current weekly challenge.</summary>
        public WeeklyChallenge GetCurrentWeeklyChallenge()
        {
            if (currentChallenge == null)
                GenerateNewWeeklyChallenge();
            return currentChallenge;
        }

        /// <summary>
        /// Checks whether the match result advances progress on the current
        /// weekly challenge.
        /// </summary>
        public bool CheckChallengeProgress(WeeklyChallenge challenge, MatchData match)
        {
            if (challenge == null || match == null || challenge.isClaimed) return false;

            // Example: goal-scoring challenge
            int goalsScored = match.homeScore; // simplified — caller provides correct side
            challenge.currentProgress += goalsScored;

            if (challenge.currentProgress >= challenge.targetValue)
            {
                GD.Print($"[LiveEvents] Challenge '{challenge.description}' completed!");
                return true;
            }
            return false;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void GenerateNewWeeklyChallenge()
        {
            currentChallenge = new WeeklyChallenge
            {
                id          = Guid.NewGuid().ToString(),
                description = "Score 10 goals in league matches",
                targetValue = 10,
                currentProgress = 0,
                isClaimed   = false,
                reward      = new EventReward { currencyType = RewardCurrencyType.PremiumCurrency, amount = 30 }
            };
        }
    }
}
