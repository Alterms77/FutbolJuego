using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Data;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    public class TransferFilter
    {
        public int MinOverall = 0;
        public int MaxOverall = 100;
        public long MaxPrice = 0;
        public PlayerPosition? Position = null;
        public string LeagueId = null;
    }

    public partial class TransferMarketUI : Control
    {
        [ExportGroup("Tabs")]
        [Export] public Button buyTabButton;
        [Export] public Button sellTabButton;

        [ExportGroup("List")]
        [Export] public Control playerListContainer;
        [Export] public PackedScene playerRowPrefab;

        [ExportGroup("Filters")]
        [Export] public OptionButton positionFilterDropdown;
        [Export] public LineEdit maxPriceInput;
        [Export] public Button applyFilterButton;
        [Export] public Button clearFilterButton;

        [ExportGroup("Buy Negotiation")]
        [Export] public Control negotiationPanel;
        [Export] public Label negotiationText;
        [Export] public LineEdit bidInput;
        [Export] public Button confirmBidButton;
        [Export] public Button cancelBidButton;

        [ExportGroup("Sell Confirmation")]
        [Export] public Control sellPanel;
        [Export] public Label sellDetailText;
        [Export] public Button confirmSellButton;
        [Export] public Button cancelSellButton;

        [ExportGroup("Status")]
        [Export] public Label balanceText;
        [Export] public Label resultMessage;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        private List<PlayerData> listedPlayers  = new List<PlayerData>();
        private PlayerData       selectedForBid;
        private PlayerData       selectedForSell;
        private bool             isBuyMode = true;

        public override void _Ready()
        {
            if (applyFilterButton != null)  applyFilterButton.Pressed  += OnApplyFilter;
            if (clearFilterButton != null)  clearFilterButton.Pressed  += OnClearFilter;
            if (confirmBidButton != null)   confirmBidButton.Pressed   += OnConfirmBid;
            if (cancelBidButton != null)    cancelBidButton.Pressed    += OnCancelBid;
            if (confirmSellButton != null)  confirmSellButton.Pressed  += OnConfirmSell;
            if (cancelSellButton != null)   cancelSellButton.Pressed   += OnCancelSell;
            if (buyTabButton != null)       buyTabButton.Pressed       += ShowBuyTab;
            if (sellTabButton != null)      sellTabButton.Pressed      += ShowSellTab;
            if (backButton != null)         backButton.Pressed         += OnBack;

            if (positionFilterDropdown != null)
            {
                positionFilterDropdown.Clear();
                positionFilterDropdown.AddItem("Todas las posiciones");
                foreach (PlayerPosition pos in Enum.GetValues(typeof(PlayerPosition)))
                    positionFilterDropdown.AddItem(pos.ToString());
            }

            ShowBuyTab();
        }

        public void ShowBuyTab()
        {
            isBuyMode = true;
            var market = ServiceLocator.Get<TransferMarketSystem>();
            listedPlayers = market?.GetAvailableFreePlayers(30) ?? new List<PlayerData>();

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

        public void ShowSellTab()
        {
            isBuyMode = false;
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            var teams  = DataLoader.LoadAllTeams();
            TeamData managedTeam = career != null ? teams?.Find(t => t.id == career.managedTeamId) : null;
            listedPlayers = managedTeam?.squad ?? new List<PlayerData>();

            if (career != null)
            {
                var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();
                foreach (var p in listedPlayers)
                    ratingSystem?.CalculateMarketValue(p, career.managedLeagueId);
            }

            RebuildList(listedPlayers);
            RefreshBalance();
        }

        public void ShowAvailablePlayers(List<PlayerData> players)
        {
            listedPlayers = players ?? new List<PlayerData>();
            RebuildList(listedPlayers);
        }

        public void FilterMarket(TransferFilter filter)
        {
            if (filter == null) { RebuildList(listedPlayers); return; }

            var filtered = listedPlayers
                .Where(p => p.CalculateOverall() >= filter.MinOverall &&
                            p.CalculateOverall() <= filter.MaxOverall)
                .Where(p => filter.MaxPrice <= 0 || p.marketValue <= filter.MaxPrice)
                .Where(p => filter.Position == null || p.position == filter.Position)
                .ToList();

            RebuildList(filtered);
        }

        public void OnPlayerBidButton(PlayerData player)
        {
            if (!isBuyMode) { OnPlayerSellButton(player); return; }

            selectedForBid = player;
            string symbol  = GetCurrencySymbol();

            if (negotiationPanel != null) negotiationPanel.Visible = true;
            if (negotiationText != null)
                negotiationText.Text =
                    $"{player.name}\n" +
                    $"Posición: {player.position}  |  OVR: {player.CalculateOverall()}  " +
                    $"|  {player.rarity}  |  {player.ratingCategory.GetLabel()}\n" +
                    $"Edad: {player.age}  |  Valor: {symbol}{player.marketValue:N0}\n" +
                    $"Salario: {symbol}{player.weeklyWage:N0}/sem";

            if (bidInput != null) bidInput.Text = player.marketValue.ToString();
        }

        public void OnConfirmBid()
        {
            if (selectedForBid == null) return;

            if (!long.TryParse(bidInput?.Text ?? "", out long offer) || offer <= 0)
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
                if (negotiationPanel != null) negotiationPanel.Visible = false;
                RefreshBalance();
                ShowBuyTab();
            }
        }

        public void OnCancelBid()
        {
            selectedForBid = null;
            if (negotiationPanel != null) negotiationPanel.Visible = false;
        }

        public void ShowTransferNegotiation(TransferOffer offer)
        {
            if (offer?.player == null) return;
            OnPlayerBidButton(offer.player);
            if (bidInput != null) bidInput.Text = offer.offerAmount.ToString();
        }

        public void OnPlayerSellButton(PlayerData player)
        {
            selectedForSell = player;
            var career      = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            string symbol   = GetCurrencySymbol();

            var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();
            long value    = ratingSystem?.CalculateMarketValue(player, career?.managedLeagueId) ?? player.marketValue;
            long proceeds = (long)(value * 0.80f);

            if (sellPanel != null) sellPanel.Visible = true;
            if (sellDetailText != null)
                sellDetailText.Text =
                    $"¿Vender a {player.name}?\n\n" +
                    $"Posición: {player.position}  |  OVR: {player.CalculateOverall()}  |  Edad: {player.age}\n" +
                    $"Valor de mercado: {symbol}{value:N0}\n" +
                    $"Recibirás: {symbol}{proceeds:N0} (80%)";
        }

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
                if (sellPanel != null) sellPanel.Visible = false;
                return;
            }

            long proceeds = transferSys.SellPlayerFromCareer(managed, career, selectedForSell);
            string symbol = GetCurrencySymbol();

            ShowResult(proceeds > 0
                ? $"✅ {selectedForSell.name} vendido por {symbol}{proceeds:N0}."
                : "❌ La venta no pudo completarse.");

            if (proceeds > 0)
            {
                if (sellPanel != null) sellPanel.Visible = false;
                RefreshBalance();
                ShowSellTab();
            }
        }

        public void OnCancelSell()
        {
            selectedForSell = null;
            if (sellPanel != null) sellPanel.Visible = false;
        }

        private void OnApplyFilter()
        {
            var filter = new TransferFilter();

            if (positionFilterDropdown != null && positionFilterDropdown.Selected > 0)
            {
                var positions = (PlayerPosition[])Enum.GetValues(typeof(PlayerPosition));
                int idx = positionFilterDropdown.Selected - 1;
                if (idx < positions.Length)
                    filter.Position = positions[idx];
            }

            if (maxPriceInput != null &&
                long.TryParse(maxPriceInput.Text, out long maxPrice) && maxPrice > 0)
                filter.MaxPrice = maxPrice;

            FilterMarket(filter);
        }

        private void OnClearFilter()
        {
            if (positionFilterDropdown != null) positionFilterDropdown.Select(0);
            if (maxPriceInput != null)          maxPriceInput.Text = "";
            RebuildList(listedPlayers);
        }

        private void RebuildList(List<PlayerData> players)
        {
            if (playerListContainer == null || playerRowPrefab == null) return;
            foreach (var child in playerListContainer.GetChildren()) child.QueueFree();

            string symbol = GetCurrencySymbol();
            foreach (var player in players)
            {
                var row  = playerRowPrefab.Instantiate<Control>();
                playerListContainer.AddChild(row);

                var card = row as PlayerCard;
                if (card != null)
                {
                    card.Setup(player);
                }
                else
                {
                    var label = row.FindChild("Label", true, false) as Label;
                    if (label != null)
                    {
                        string action = isBuyMode ? "Fichar" : "Vender";
                        label.Text =
                            $"{player.name}  {player.position}  " +
                            $"OVR {player.CalculateOverall()}  {player.rarity}  " +
                            $"Edad {player.age}  {symbol}{player.marketValue:N0}  [{action}]";
                    }
                }

                var btn = row as Button ?? row.FindChild("Button", true, false) as Button;
                if (btn != null)
                {
                    var captured = player;
                    btn.Pressed += () => OnPlayerBidButton(captured);
                }
            }
        }

        private void RefreshBalance()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (balanceText != null)
                balanceText.Text = career != null
                    ? $"Presupuesto: {career.FormattedBalance}  |  🪙 {career.premiumCoins}"
                    : "Presupuesto: —";
        }

        private void ShowResult(string message)
        {
            if (resultMessage != null) resultMessage.Text = message;
        }

        private static string GetCurrencySymbol()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            return career?.CurrencySymbol ?? "€";
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();
    }
}
