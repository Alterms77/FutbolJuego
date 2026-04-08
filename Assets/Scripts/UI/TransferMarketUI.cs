using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Data;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    // ── Transfer filter ────────────────────────────────────────────────────────

    /// <summary>Filter criteria for the transfer-market listing.</summary>
    public class TransferFilter
    {
        /// <summary>Minimum overall rating (inclusive).</summary>
        public int MinOverall = 0;
        /// <summary>Maximum overall rating (inclusive).</summary>
        public int MaxOverall = 100;
        /// <summary>Maximum market value. 0 = no limit.</summary>
        public long MaxPrice = 0;
        /// <summary>If set, only show this position.</summary>
        public PlayerPosition? Position = null;
        /// <summary>If set, only show players from this league.</summary>
        public string LeagueId = null;
    }

    // ── TransferMarketUI ───────────────────────────────────────────────────────

    /// <summary>
    /// Transfer-market screen: two tabs (Buy / Sell), position/price/league
    /// filters, bid flow, and sell confirmation overlay.
    ///
    /// <list type="bullet">
    ///   <item><b>Buy tab</b> — shows market players; deducts from
    ///         <see cref="CareerData.inGameBalance"/>.</item>
    ///   <item><b>Sell tab</b> — shows the managed squad; credits
    ///         <see cref="CareerData.inGameBalance"/> on sale.</item>
    /// </list>
    ///
    /// The currency symbol (€ / $) is always taken from the active
    /// <see cref="CareerData.CurrencySymbol"/>.
    /// </summary>
    public class TransferMarketUI : MonoBehaviour
    {
        // ── Tabs ───────────────────────────────────────────────────────────────

        [Header("Tabs")]
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button sellTabButton;

        // ── Player list ────────────────────────────────────────────────────────

        [Header("List")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerRowPrefab;

        // ── Filters ────────────────────────────────────────────────────────────

        [Header("Filters")]
        [SerializeField] private TMP_Dropdown positionFilterDropdown;
        [SerializeField] private TMP_InputField maxPriceInput;
        [SerializeField] private Button applyFilterButton;
        [SerializeField] private Button clearFilterButton;

        // ── Buy negotiation panel ──────────────────────────────────────────────

        [Header("Buy Negotiation")]
        [SerializeField] private GameObject negotiationPanel;
        [SerializeField] private TextMeshProUGUI negotiationText;
        [SerializeField] private TMP_InputField bidInput;
        [SerializeField] private Button confirmBidButton;
        [SerializeField] private Button cancelBidButton;

        // ── Sell confirmation panel ────────────────────────────────────────────

        [Header("Sell Confirmation")]
        [SerializeField] private GameObject sellPanel;
        [SerializeField] private TextMeshProUGUI sellDetailText;
        [SerializeField] private Button confirmSellButton;
        [SerializeField] private Button cancelSellButton;

        // ── Balance + result display ───────────────────────────────────────────

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI resultMessage;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // ── Private state ──────────────────────────────────────────────────────

        private List<PlayerData> listedPlayers  = new List<PlayerData>();
        private PlayerData       selectedForBid;
        private PlayerData       selectedForSell;
        private bool             isBuyMode = true;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (applyFilterButton)  applyFilterButton.onClick.AddListener(OnApplyFilter);
            if (clearFilterButton)  clearFilterButton.onClick.AddListener(OnClearFilter);
            if (confirmBidButton)   confirmBidButton.onClick.AddListener(OnConfirmBid);
            if (cancelBidButton)    cancelBidButton.onClick.AddListener(OnCancelBid);
            if (confirmSellButton)  confirmSellButton.onClick.AddListener(OnConfirmSell);
            if (cancelSellButton)   cancelSellButton.onClick.AddListener(OnCancelSell);
            if (buyTabButton)       buyTabButton.onClick.AddListener(ShowBuyTab);
            if (sellTabButton)      sellTabButton.onClick.AddListener(ShowSellTab);
            if (backButton)         backButton.onClick.AddListener(OnBack);

            if (positionFilterDropdown != null)
            {
                positionFilterDropdown.ClearOptions();
                var options = new List<string> { "Todas las posiciones" };
                foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
                    options.Add(pos.ToString());
                positionFilterDropdown.AddOptions(options);
            }
        }

        private void Start()
        {
            ShowBuyTab();
        }

        // ── Tab switching ──────────────────────────────────────────────────────

        /// <summary>Shows the Buy tab (market players).</summary>
        public void ShowBuyTab()
        {
            isBuyMode = true;
            var market = ServiceLocator.Get<TransferMarketSystem>();
            listedPlayers = market?.GetAvailableFreePlayers(30) ?? new List<PlayerData>();

            // Refresh values with career league
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (career != null)
            {
                var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();
                foreach (var p in listedPlayers)
                    ratingSystem?.CalculateMarketValue(p, career.managedLeagueId);
            }

            RebuildList(listedPlayers);
            RefreshBalance();
        }

        /// <summary>Shows the Sell tab (squad players).</summary>
        public void ShowSellTab()
        {
            isBuyMode = false;
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            var teams  = DataLoader.LoadAllTeams();
            TeamData managedTeam = null;

            if (career != null)
                managedTeam = teams?.Find(t => t.id == career.managedTeamId);

            listedPlayers = managedTeam?.squad ?? new List<PlayerData>();

            // Refresh market values
            if (career != null)
            {
                var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();
                foreach (var p in listedPlayers)
                    ratingSystem?.CalculateMarketValue(p, career.managedLeagueId);
            }

            RebuildList(listedPlayers);
            RefreshBalance();
        }

        // ── Display ────────────────────────────────────────────────────────────

        /// <summary>Builds the market listing from <paramref name="players"/>.</summary>
        public void ShowAvailablePlayers(List<PlayerData> players)
        {
            listedPlayers = players ?? new List<PlayerData>();
            RebuildList(listedPlayers);
        }

        /// <summary>Applies <paramref name="filter"/> and rebuilds the list.</summary>
        public void FilterMarket(TransferFilter filter)
        {
            if (filter == null)
            {
                RebuildList(listedPlayers);
                return;
            }

            var filtered = listedPlayers
                .Where(p => p.CalculateOverall() >= filter.MinOverall &&
                            p.CalculateOverall() <= filter.MaxOverall)
                .Where(p => filter.MaxPrice <= 0 || p.marketValue <= filter.MaxPrice)
                .Where(p => filter.Position == null || p.position == filter.Position)
                .ToList();

            RebuildList(filtered);
        }

        // ── Buy flow ───────────────────────────────────────────────────────────

        /// <summary>Opens the bid panel for <paramref name="player"/>.</summary>
        public void OnPlayerBidButton(PlayerData player)
        {
            if (!isBuyMode) { OnPlayerSellButton(player); return; }

            selectedForBid = player;
            string symbol  = GetCurrencySymbol();

            if (negotiationPanel) negotiationPanel.SetActive(true);
            if (negotiationText)
                negotiationText.text =
                    $"{player.name}\n" +
                    $"Posición: {player.position}  |  OVR: {player.CalculateOverall()}  " +
                    $"|  {player.rarity}  |  {player.ratingCategory.GetLabel()}\n" +
                    $"Edad: {player.age}  |  Valor: {symbol}{player.marketValue:N0}\n" +
                    $"Salario: {symbol}{player.weeklyWage:N0}/sem";

            if (bidInput) bidInput.text = player.marketValue.ToString();
        }

        /// <summary>Confirms the bid entered by the player.</summary>
        public void OnConfirmBid()
        {
            if (selectedForBid == null) return;

            if (!long.TryParse(bidInput?.text ?? "", out long offer) || offer <= 0)
            {
                ShowResult("Cantidad de oferta no válida.");
                return;
            }

            var career       = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            var transferSys  = ServiceLocator.Get<TransferMarketSystem>();
            var teams        = DataLoader.LoadAllTeams();
            TeamData managed = career != null ? teams?.Find(t => t.id == career.managedTeamId) : null;

            bool success = career != null && managed != null
                ? transferSys.BuyPlayerForCareer(managed, career, selectedForBid, offer)
                : transferSys.AttemptTransfer(null, null, selectedForBid, offer);

            string symbol = GetCurrencySymbol();
            ShowResult(success
                ? $"✅ Fichado {selectedForBid.name} por {symbol}{offer:N0}."
                : $"❌ Oferta rechazada. Prueba con una oferta mayor.");

            if (success)
            {
                if (negotiationPanel) negotiationPanel.SetActive(false);
                RefreshBalance();
                ShowBuyTab();
            }
        }

        /// <summary>Cancels the negotiation overlay.</summary>
        public void OnCancelBid()
        {
            selectedForBid = null;
            if (negotiationPanel) negotiationPanel.SetActive(false);
        }

        /// <summary>Populates the UI from a transfer offer originating from the AI.</summary>
        public void ShowTransferNegotiation(TransferOffer offer)
        {
            if (offer?.player == null) return;
            OnPlayerBidButton(offer.player);
            if (bidInput) bidInput.text = offer.offerAmount.ToString();
        }

        // ── Sell flow ──────────────────────────────────────────────────────────

        /// <summary>Opens the sell confirmation panel for <paramref name="player"/>.</summary>
        public void OnPlayerSellButton(PlayerData player)
        {
            selectedForSell = player;
            var career      = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            string symbol   = GetCurrencySymbol();

            var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();
            long value    = ratingSystem?.CalculateMarketValue(player, career?.managedLeagueId)
                            ?? player.marketValue;
            long proceeds = (long)(value * 0.80f);

            if (sellPanel) sellPanel.SetActive(true);
            if (sellDetailText)
                sellDetailText.text =
                    $"¿Vender a {player.name}?\n\n" +
                    $"Posición: {player.position}  |  OVR: {player.CalculateOverall()}  " +
                    $"|  Edad: {player.age}\n" +
                    $"Valor de mercado: {symbol}{value:N0}\n" +
                    $"Recibirás: {symbol}{proceeds:N0} (80%)";
        }

        /// <summary>Confirms the sale of the selected player.</summary>
        public void OnConfirmSell()
        {
            if (selectedForSell == null) return;

            var career      = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            var transferSys = ServiceLocator.Get<TransferMarketSystem>();
            var teams       = DataLoader.LoadAllTeams();
            TeamData managed = career != null ? teams?.Find(t => t.id == career.managedTeamId) : null;

            if (career == null || managed == null)
            {
                ShowResult("Inicia una carrera para vender jugadores.");
                if (sellPanel) sellPanel.SetActive(false);
                return;
            }

            long proceeds = transferSys.SellPlayerFromCareer(managed, career, selectedForSell);
            string symbol = GetCurrencySymbol();

            ShowResult(proceeds > 0
                ? $"✅ {selectedForSell.name} vendido por {symbol}{proceeds:N0}."
                : "❌ La venta no pudo completarse.");

            if (proceeds > 0)
            {
                if (sellPanel) sellPanel.SetActive(false);
                RefreshBalance();
                ShowSellTab();
            }
        }

        /// <summary>Cancels the sell overlay.</summary>
        public void OnCancelSell()
        {
            selectedForSell = null;
            if (sellPanel) sellPanel.SetActive(false);
        }

        // ── Filter button handlers ─────────────────────────────────────────────

        private void OnApplyFilter()
        {
            var filter = new TransferFilter();

            if (positionFilterDropdown != null && positionFilterDropdown.value > 0)
            {
                var positions = (PlayerPosition[])Enum.GetValues(typeof(PlayerPosition));
                int idx = positionFilterDropdown.value - 1;
                if (idx < positions.Length)
                    filter.Position = positions[idx];
            }

            if (maxPriceInput != null &&
                long.TryParse(maxPriceInput.text, out long maxPrice) && maxPrice > 0)
                filter.MaxPrice = maxPrice;

            FilterMarket(filter);
        }

        private void OnClearFilter()
        {
            if (positionFilterDropdown) positionFilterDropdown.value = 0;
            if (maxPriceInput)          maxPriceInput.text           = "";
            RebuildList(listedPlayers);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void RebuildList(List<PlayerData> players)
        {
            if (playerListContainer == null || playerRowPrefab == null) return;

            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);

            string symbol = GetCurrencySymbol();

            foreach (var player in players)
            {
                var row = Instantiate(playerRowPrefab, playerListContainer);

                // Try rich PlayerCard component first
                var card = row.GetComponent<PlayerCard>();
                if (card != null)
                {
                    card.Setup(player);
                }
                else
                {
                    var label = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (label)
                    {
                        string action = isBuyMode ? "Fichar" : "Vender";
                        label.text =
                            $"{player.name}  {player.position}  " +
                            $"OVR {player.CalculateOverall()}  {player.rarity}  " +
                            $"Edad {player.age}  {symbol}{player.marketValue:N0}  [{action}]";
                    }
                }

                var btn = row.GetComponent<Button>();
                if (btn)
                {
                    var captured = player;
                    btn.onClick.AddListener(() => OnPlayerBidButton(captured));
                }
            }
        }

        private void RefreshBalance()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (balanceText)
                balanceText.text = career != null
                    ? $"Presupuesto: {career.FormattedBalance}  |  🪙 {career.premiumCoins}"
                    : "Presupuesto: —";
        }

        private void ShowResult(string message)
        {
            if (resultMessage) resultMessage.text = message;
        }

        private static string GetCurrencySymbol()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            return career?.CurrencySymbol ?? "€";
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();
    }
}

