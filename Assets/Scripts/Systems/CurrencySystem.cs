using System;
using System.Collections.Generic;
using UnityEngine;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>
    /// Handles conversion between premium coins and in-game currency (EUR/USD),
    /// and tracks the player's premium-coin balance.
    /// Registered as a service via <see cref="Core.ServiceLocator"/>.
    /// </summary>
    public class CurrencySystem
    {
        // ── Exchange rates ─────────────────────────────────────────────────────

        /// <summary>Amount of EUR awarded per premium coin spent.</summary>
        public const long EurPerPremiumCoin = 5_000L;

        /// <summary>Amount of USD awarded per premium coin spent.</summary>
        public const long UsdPerPremiumCoin = 5_500L;

        // ── Currency packs available in the shop ───────────────────────────────

        /// <summary>
        /// Pre-defined currency packs the player can buy with premium coins.
        /// Each pack specifies the premium-coin cost and the in-game amount
        /// awarded in both EUR and USD.
        /// </summary>
        public static readonly List<CurrencyPack> AvailablePacks = new List<CurrencyPack>
        {
            new CurrencyPack { id = "curr-s",  displayName = "Small Fund",    premiumCoinCost = 50,   eurAmount =   250_000L, usdAmount =   275_000L },
            new CurrencyPack { id = "curr-m",  displayName = "Medium Fund",   premiumCoinCost = 100,  eurAmount =   600_000L, usdAmount =   660_000L },
            new CurrencyPack { id = "curr-l",  displayName = "Large Fund",    premiumCoinCost = 200,  eurAmount = 1_200_000L, usdAmount = 1_320_000L },
            new CurrencyPack { id = "curr-xl", displayName = "Mega Fund",     premiumCoinCost = 500,  eurAmount = 3_500_000L, usdAmount = 3_850_000L },
            new CurrencyPack { id = "curr-xx", displayName = "Transfer War Chest", premiumCoinCost = 1_000, eurAmount = 8_000_000L, usdAmount = 8_800_000L },
        };

        // ── Conversion ─────────────────────────────────────────────────────────

        /// <summary>
        /// Converts <paramref name="premiumCoinsToSpend"/> premium coins to in-game
        /// currency and adds it to <paramref name="career"/>'s balance.
        /// Deducts the premium coins from <paramref name="career.premiumCoins"/>.
        ///
        /// Returns <c>true</c> on success; <c>false</c> if the player cannot afford it.
        /// </summary>
        public bool ConvertPremiumToInGame(CareerData career, int premiumCoinsToSpend)
        {
            if (career == null) throw new ArgumentNullException(nameof(career));
            if (premiumCoinsToSpend <= 0) return false;

            if (career.premiumCoins < premiumCoinsToSpend)
            {
                Debug.LogWarning($"[CurrencySystem] Not enough premium coins. " +
                                 $"Have {career.premiumCoins}, need {premiumCoinsToSpend}.");
                return false;
            }

            long gain = career.currencyType == CurrencyType.EUR
                ? premiumCoinsToSpend * EurPerPremiumCoin
                : premiumCoinsToSpend * UsdPerPremiumCoin;

            career.premiumCoins  -= premiumCoinsToSpend;
            career.inGameBalance += gain;

            Debug.Log($"[CurrencySystem] Converted {premiumCoinsToSpend} coins → " +
                      $"{career.CurrencySymbol}{gain:N0}. " +
                      $"New balance: {career.FormattedBalance}.");
            return true;
        }

        /// <summary>
        /// Redeems a <see cref="CurrencyPack"/> from <see cref="AvailablePacks"/>.
        /// Deducts premium coins and credits in-game balance.
        /// Returns <c>true</c> on success.
        /// </summary>
        public bool RedeemPack(CareerData career, string packId)
        {
            if (career == null) throw new ArgumentNullException(nameof(career));

            var pack = AvailablePacks.Find(p => p.id == packId);
            if (pack == null)
            {
                Debug.LogWarning($"[CurrencySystem] Unknown pack id: {packId}");
                return false;
            }

            if (career.premiumCoins < pack.premiumCoinCost)
            {
                Debug.LogWarning($"[CurrencySystem] Not enough premium coins for {pack.displayName}.");
                return false;
            }

            long gain = career.currencyType == CurrencyType.EUR
                ? pack.eurAmount : pack.usdAmount;

            career.premiumCoins  -= pack.premiumCoinCost;
            career.inGameBalance += gain;

            Debug.Log($"[CurrencySystem] Redeemed '{pack.displayName}' → " +
                      $"{career.CurrencySymbol}{gain:N0}.");
            return true;
        }

        /// <summary>
        /// Adds premium coins to <paramref name="career"/> (e.g. after an IAP).
        /// </summary>
        public void AddPremiumCoins(CareerData career, int amount)
        {
            if (career == null) throw new ArgumentNullException(nameof(career));
            if (amount <= 0) return;
            career.premiumCoins += amount;
            Debug.Log($"[CurrencySystem] +{amount} premium coins. Total: {career.premiumCoins}.");
        }

        /// <summary>
        /// Returns the in-game value a single premium coin is worth for the
        /// given <paramref name="currencyType"/>.
        /// </summary>
        public static long CoinValueIn(CurrencyType currencyType)
            => currencyType == CurrencyType.EUR ? EurPerPremiumCoin : UsdPerPremiumCoin;
    }

    // ── CurrencyPack ───────────────────────────────────────────────────────────

    /// <summary>
    /// Describes a single currency exchange pack available in the shop.
    /// </summary>
    [Serializable]
    public class CurrencyPack
    {
        /// <summary>Unique pack identifier.</summary>
        public string id;
        /// <summary>Display label shown in the shop.</summary>
        public string displayName;
        /// <summary>Premium-coin price of this pack.</summary>
        public int premiumCoinCost;
        /// <summary>In-game EUR awarded.</summary>
        public long eurAmount;
        /// <summary>In-game USD awarded.</summary>
        public long usdAmount;
    }
}
