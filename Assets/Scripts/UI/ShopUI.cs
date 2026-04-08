using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Core;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.UI
{
    // ── Shop item descriptor ───────────────────────────────────────────────────

    /// <summary>Descriptor for a single item on sale in the in-game shop.</summary>
    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public string displayName;
        public string description;
        public int price;
        public ShopItemType itemType;
    }

    /// <summary>Category of a shop item.</summary>
    public enum ShopItemType
    {
        CoinPack,
        PlayerPack,
        StadiumUpgrade,
        TrainingBoost,
        CurrencyPack
    }

    // ── ShopUI ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Shop scene controller.  Displays purchasable packs, coin bundles, and
    /// currency packs (premium coins → in-game EUR/USD), connecting to the
    /// <see cref="EconomySystem"/> and <see cref="CurrencySystem"/> for processing.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Button tabCoinsButton;
        [SerializeField] private Button tabCurrencyButton;
        [SerializeField] private Button tabPacksButton;

        [Header("Shop Items")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemCardPrefab;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI exchangeRateText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // Default coin / pack catalogue
        private static readonly List<ShopItem> CoinCatalogue = new List<ShopItem>
        {
            new ShopItem { id = "coin-500",    displayName = "500 Monedas",   description = "Paquete pequeño",  price = 99,  itemType = ShopItemType.CoinPack },
            new ShopItem { id = "coin-1200",   displayName = "1 200 Monedas", description = "Paquete popular",  price = 199, itemType = ShopItemType.CoinPack },
            new ShopItem { id = "coin-3000",   displayName = "3 000 Monedas", description = "Mejor valor",      price = 449, itemType = ShopItemType.CoinPack },
        };

        private static readonly List<ShopItem> PackCatalogue = new List<ShopItem>
        {
            new ShopItem { id = "pack-basic",   displayName = "Pack Básico",   description = "3 jugadores al azar",   price = 500,  itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "pack-premium", displayName = "Pack Premium",  description = "5 jugadores (1 Gold+)", price = 1200, itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "pack-elite",   displayName = "Pack Élite",    description = "3 cartas Gold/Estrella",price = 2500, itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "training-boost",displayName="Boost Entreno", description = "+20% ganancias 7 días", price = 300,  itemType = ShopItemType.TrainingBoost },
        };

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backButton)        backButton.onClick.AddListener(OnBack);
            if (tabCoinsButton)    tabCoinsButton.onClick.AddListener(ShowCoinTab);
            if (tabCurrencyButton) tabCurrencyButton.onClick.AddListener(ShowCurrencyTab);
            if (tabPacksButton)    tabPacksButton.onClick.AddListener(ShowPackTab);
        }

        private void Start()
        {
            ShowCoinTab();
            RefreshBalance();
        }

        // ── Tabs ───────────────────────────────────────────────────────────────

        /// <summary>Shows coin bundle items.</summary>
        public void ShowCoinTab()    => BuildCatalogue(CoinCatalogue);

        /// <summary>Shows player pack items.</summary>
        public void ShowPackTab()    => BuildCatalogue(PackCatalogue);

        /// <summary>Shows currency exchange packs (premium coins → EUR/USD).</summary>
        public void ShowCurrencyTab()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            string symbol = career?.CurrencySymbol ?? "€";
            bool   isEur  = career?.currencyType != CurrencyType.USD;

            // Build dynamic ShopItems from CurrencySystem.AvailablePacks
            var items = new List<ShopItem>();
            foreach (var pack in CurrencySystem.AvailablePacks)
            {
                long amount = isEur ? pack.eurAmount : pack.usdAmount;
                items.Add(new ShopItem
                {
                    id          = pack.id,
                    displayName = pack.displayName,
                    description = $"{symbol} {amount:N0} para transferencias",
                    price       = pack.premiumCoinCost,
                    itemType    = ShopItemType.CurrencyPack
                });
            }

            BuildCatalogue(items);

            // Show the exchange rate
            long rate = isEur ? CurrencySystem.EurPerPremiumCoin : CurrencySystem.UsdPerPremiumCoin;
            if (exchangeRateText)
                exchangeRateText.text =
                    $"1 moneda premium = {symbol} {rate:N0}";
        }

        // ── Catalogue builder ──────────────────────────────────────────────────

        /// <summary>Rebuilds the item grid from <paramref name="catalogue"/>.</summary>
        public void BuildCatalogue(List<ShopItem> catalogue)
        {
            if (itemContainer == null || itemCardPrefab == null) return;

            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);

            if (exchangeRateText) exchangeRateText.text = string.Empty;

            foreach (var item in catalogue)
            {
                var card  = Instantiate(itemCardPrefab, itemContainer);
                var texts = card.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 1) texts[0].text = item.displayName;
                if (texts.Length >= 2) texts[1].text = item.description;
                if (texts.Length >= 3) texts[2].text = $"{item.price} 🪙";

                var btn = card.GetComponentInChildren<Button>();
                if (btn)
                {
                    var captured = item;
                    btn.onClick.AddListener(() => OnBuyItem(captured));
                }
            }
        }

        // ── Purchase flow ──────────────────────────────────────────────────────

        /// <summary>Attempts to purchase <paramref name="item"/>.</summary>
        public void OnBuyItem(ShopItem item)
        {
            if (item == null) return;

            if (item.itemType == ShopItemType.CurrencyPack)
            {
                var career    = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
                var currSystem = ServiceLocator.Get<CurrencySystem>();

                if (career == null || currSystem == null)
                {
                    ShowFeedback("Inicia una carrera para comprar divisas.");
                    return;
                }

                bool success = currSystem.RedeemPack(career, item.id);
                ShowFeedback(success
                    ? $"✅ {item.displayName} canjeado. Nuevo saldo: {career.FormattedBalance}"
                    : "❌ Monedas insuficientes.");
                RefreshBalance();
                return;
            }

            // Non-currency items: platform IAP stub
            Debug.Log($"[ShopUI] Purchase: {item.displayName} ({item.price} 🪙)");
            ShowFeedback($"Abriendo flujo de compra para {item.displayName}…");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RefreshBalance()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (balanceText)
                balanceText.text = career != null
                    ? $"🪙 {career.premiumCoins}  |  {career.FormattedBalance}"
                    : "Balance: —";
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText) feedbackText.text = message;
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();
    }
}

