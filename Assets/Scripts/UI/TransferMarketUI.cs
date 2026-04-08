using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    // ── Transfer filter ────────────────────────────────────────────────────────

    /// <summary>Filter criteria for the transfer-market listing.</summary>
    public class TransferFilter
    {
        /// <summary>Minimum overall rating (inclusive).</summary>
        public int MinOverall = 0;
        /// <summary>Maximum overall rating (inclusive).</summary>
        public int MaxOverall = 99;
        /// <summary>Maximum market value. 0 = no limit.</summary>
        public long MaxPrice = 0;
        /// <summary>If set, only show this position.</summary>
        public PlayerPosition? Position = null;
    }

    // ── TransferMarketUI ───────────────────────────────────────────────────────

    /// <summary>
    /// Transfer-market screen: player listing, bid flow, negotiation overlay.
    /// </summary>
    public class TransferMarketUI : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerRowPrefab;

        [Header("Negotiation")]
        [SerializeField] private GameObject negotiationPanel;
        [SerializeField] private TextMeshProUGUI negotiationText;
        [SerializeField] private TMP_InputField bidInput;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI resultMessage;

        private List<PlayerData> listedPlayers = new List<PlayerData>();
        private PlayerData selectedForBid;

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

        // ── Bid flow ───────────────────────────────────────────────────────────

        /// <summary>Opens the bid panel for <paramref name="player"/>.</summary>
        public void OnPlayerBidButton(PlayerData player)
        {
            selectedForBid = player;
            if (negotiationPanel) negotiationPanel.SetActive(true);

            if (negotiationText)
                negotiationText.text =
                    $"{player.name}\n" +
                    $"Position: {player.position}  |  OVR: {player.CalculateOverall()}\n" +
                    $"Age: {player.age}  |  Value: £{player.marketValue:N0}\n" +
                    $"Wage: £{player.weeklyWage:N0}/wk";

            if (bidInput) bidInput.text = player.marketValue.ToString();
        }

        /// <summary>Confirms the bid entered by the player.</summary>
        public void OnConfirmBid()
        {
            if (selectedForBid == null) return;

            if (!int.TryParse(bidInput?.text ?? "", out int offer) || offer <= 0)
            {
                ShowResult("Invalid bid amount.");
                return;
            }

            var transferSystem = ServiceLocator.Get<TransferMarketSystem>();
            bool success = transferSystem.AttemptTransfer(null, null, selectedForBid, offer);

            ShowResult(success
                ? $"Transfer complete! Signed {selectedForBid.name} for £{offer:N0}."
                : $"Bid rejected. Try a higher offer.");

            if (success && negotiationPanel)
                negotiationPanel.SetActive(false);
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

        // ── Private helpers ────────────────────────────────────────────────────

        private void RebuildList(List<PlayerData> players)
        {
            if (playerListContainer == null || playerRowPrefab == null) return;

            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);

            foreach (var player in players)
            {
                var row   = Instantiate(playerRowPrefab, playerListContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text = $"{player.name}  {player.position}  OVR {player.CalculateOverall()}  £{player.marketValue:N0}";

                var btn = row.GetComponent<Button>();
                if (btn)
                {
                    var captured = player;
                    btn.onClick.AddListener(() => OnPlayerBidButton(captured));
                }
            }
        }

        private void ShowResult(string message)
        {
            if (resultMessage) resultMessage.text = message;
        }
    }
}
