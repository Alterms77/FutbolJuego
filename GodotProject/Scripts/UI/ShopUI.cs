using System.Collections.Generic;
using Godot;
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
    public partial class ShopUI : Control
    {
        [ExportGroup("Tabs")]
        [Export] public Button tabCoinsButton;
        [Export] public Button tabCurrencyButton;
        [Export] public Button tabPacksButton;

        [ExportGroup("Shop Items")]
        [Export] public Control itemContainer;
        [Export] public PackedScene itemCardPrefab;

        [ExportGroup("Feedback")]
        [Export] public Label feedbackText;
        [Export] public Label balanceText;
        [Export] public Label exchangeRateText;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        private static readonly List<ShopItem> CoinCatalogue = new List<ShopItem>
        {
            new ShopItem { id = "coin-500",    displayName = "500 Monedas",   description = "Paquete pequeño",  price = 99,  itemType = ShopItemType.CoinPack },
            new ShopItem { id = "coin-1200",   displayName = "1 200 Monedas", description = "Paquete popular",  price = 199, itemType = ShopItemType.CoinPack },
            new ShopItem { id = "coin-3000",   displayName = "3 000 Monedas", description = "Mejor valor",      price = 449, itemType = ShopItemType.CoinPack },
        };

        private static readonly List<ShopItem> PackCatalogue = new List<ShopItem>
        {
            new ShopItem { id = "pack-basic",    displayName = "Pack Básico",   description = "3 jugadores al azar",    price = 500,  itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "pack-premium",  displayName = "Pack Premium",  description = "5 jugadores (1 Gold+)",  price = 1200, itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "pack-elite",    displayName = "Pack Élite",    description = "3 cartas Gold/Estrella", price = 2500, itemType = ShopItemType.PlayerPack   },
            new ShopItem { id = "training-boost",displayName = "Boost Entreno", description = "+20% ganancias 7 días",  price = 300,  itemType = ShopItemType.TrainingBoost },
        };

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (backButton != null)        backButton.Pressed        += OnBack;
            if (tabCoinsButton != null)    tabCoinsButton.Pressed    += ShowCoinTab;
            if (tabCurrencyButton != null) tabCurrencyButton.Pressed += ShowCurrencyTab;
            if (tabPacksButton != null)    tabPacksButton.Pressed    += ShowPackTab;

            ShowCoinTab();
            RefreshBalance();
        }

        // ── Tabs ───────────────────────────────────────────────────────────────

        /// <summary>Shows coin bundle items.</summary>
        public void ShowCoinTab()  => BuildCatalogue(CoinCatalogue);

        /// <summary>Shows player pack items.</summary>
        public void ShowPackTab()  => BuildCatalogue(PackCatalogue);

        /// <summary>Shows currency exchange packs (premium coins → EUR/USD).</summary>
        public void ShowCurrencyTab()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            string symbol = career?.CurrencySymbol ?? "€";
            bool   isEur  = career?.currencyType != CurrencyType.USD;

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

            long rate = isEur ? CurrencySystem.EurPerPremiumCoin : CurrencySystem.UsdPerPremiumCoin;
            if (exchangeRateText != null)
                exchangeRateText.Text = $"1 moneda premium = {symbol} {rate:N0}";
        }

        // ── Catalogue builder ──────────────────────────────────────────────────

        /// <summary>Rebuilds the item grid from <paramref name="catalogue"/>.</summary>
        public void BuildCatalogue(List<ShopItem> catalogue)
        {
            if (itemContainer == null || itemCardPrefab == null) return;

            foreach (var child in itemContainer.GetChildren())
                child.QueueFree();

            if (exchangeRateText != null) exchangeRateText.Text = string.Empty;

            foreach (var item in catalogue)
            {
                var card  = itemCardPrefab.Instantiate<Control>();
                itemContainer.AddChild(card);
                var texts = card.FindChildren("*", "Label", true, false);

                if (texts.Count >= 1) (texts[0] as Label).Text = item.displayName;
                if (texts.Count >= 2) (texts[1] as Label).Text = item.description;
                if (texts.Count >= 3) (texts[2] as Label).Text = $"{item.price} 🪙";

                var btn = card.FindChild("Button", true, false) as Button ?? card as Button;
                if (btn != null)
                {
                    var captured = item;
                    btn.Pressed += () => OnBuyItem(captured);
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
                var career     = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
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

            ShowFeedback($"Abriendo flujo de compra para {item.displayName}…");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RefreshBalance()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (balanceText != null)
                balanceText.Text = career != null
                    ? $"🪙 {career.premiumCoins}  |  {career.FormattedBalance}"
                    : "Balance: —";
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null) feedbackText.Text = message;
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();
    }
}
