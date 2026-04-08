using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Core;

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
        TrainingBoost
    }

    // ── ShopUI ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Shop scene controller.  Displays purchasable packs and coin bundles,
    /// connecting to the <see cref="EconomySystem"/> to process payments.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Shop Items")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemCardPrefab;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // Default catalogue shown when no external data is provided.
        private static readonly List<ShopItem> DefaultCatalogue = new List<ShopItem>
        {
            new ShopItem { id = "coin-500",       displayName = "500 Coins",          description = "Small coin bundle",   price = 99,     itemType = ShopItemType.CoinPack       },
            new ShopItem { id = "coin-1200",      displayName = "1 200 Coins",         description = "Popular coin bundle", price = 199,    itemType = ShopItemType.CoinPack       },
            new ShopItem { id = "coin-3000",      displayName = "3 000 Coins",         description = "Best value bundle",   price = 449,    itemType = ShopItemType.CoinPack       },
            new ShopItem { id = "pack-basic",     displayName = "Basic Player Pack",   description = "3 random players",   price = 500,    itemType = ShopItemType.PlayerPack     },
            new ShopItem { id = "pack-premium",   displayName = "Premium Player Pack", description = "5 players (1 Gold+)", price = 1200,   itemType = ShopItemType.PlayerPack     },
            new ShopItem { id = "pack-elite",     displayName = "Elite Player Pack",   description = "3 Gold / Star cards", price = 2500,   itemType = ShopItemType.PlayerPack     },
            new ShopItem { id = "training-boost", displayName = "Training Boost",      description = "+20% training gains for 7 days", price = 300, itemType = ShopItemType.TrainingBoost },
        };

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backButton) backButton.onClick.AddListener(OnBack);
        }

        private void Start()
        {
            BuildCatalogue(DefaultCatalogue);
            RefreshBalance();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Rebuilds the item grid from <paramref name="catalogue"/>.</summary>
        public void BuildCatalogue(List<ShopItem> catalogue)
        {
            if (itemContainer == null || itemCardPrefab == null) return;

            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);

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

            // Shop purchases (real-money packs / coin bundles) are platform-specific.
            // This stub logs the intent; wire up a payment provider (IAP, ads, etc.)
            // in a platform-specific service when integrating into production.
            Debug.Log($"[ShopUI] Purchase requested: {item.displayName} ({item.price} coins)");
            ShowFeedback($"Opening purchase flow for {item.displayName}…");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RefreshBalance()
        {
            // Balance display is a future integration point with an IAP/coin service.
            if (balanceText) balanceText.text = "Balance: —";
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText) feedbackText.text = message;
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();
    }
}
