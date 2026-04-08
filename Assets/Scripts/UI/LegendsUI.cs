using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Data;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Legends Hall screen — lets the player browse retired and deceased
    /// football legends, filter them by league or era, and purchase them
    /// with premium coins to add to a special legend squad.
    /// </summary>
    public class LegendsUI : MonoBehaviour
    {
        [Header("Filter bar")]
        [SerializeField] private TMP_Dropdown leagueFilterDropdown;
        [SerializeField] private TMP_Dropdown eraFilterDropdown;
        [SerializeField] private TMP_Dropdown tierFilterDropdown;

        [Header("Legend list")]
        [SerializeField] private Transform legendListContainer;
        [SerializeField] private GameObject legendCardPrefab;

        [Header("Detail panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailLifespanText;
        [SerializeField] private TextMeshProUGUI detailNationalityText;
        [SerializeField] private TextMeshProUGUI detailPositionText;
        [SerializeField] private TextMeshProUGUI detailOverallText;
        [SerializeField] private TextMeshProUGUI detailEraText;
        [SerializeField] private TextMeshProUGUI detailClubText;
        [SerializeField] private TextMeshProUGUI detailLegacyText;
        [SerializeField] private TextMeshProUGUI detailCostText;
        [SerializeField] private Button          purchaseButton;

        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI premiumCoinsText;
        [SerializeField] private TextMeshProUGUI inGameBalanceText;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // ── Private state ──────────────────────────────────────────────────────

        private List<LegendPlayerData> allLegends     = new List<LegendPlayerData>();
        private List<LegendPlayerData> filteredLegends = new List<LegendPlayerData>();
        private LegendPlayerData       selectedLegend;

        private const string AllFilter = "Todos";

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backButton)    backButton.onClick.AddListener(OnBack);
            if (purchaseButton)purchaseButton.onClick.AddListener(OnPurchaseLegend);
        }

        private void Start()
        {
            allLegends = DataLoader.LoadAllLegends();

            BuildFilterDropdowns();
            ApplyFilters();
            if (detailPanel) detailPanel.SetActive(false);
            RefreshWallet();
        }

        // ── Filters ────────────────────────────────────────────────────────────

        private void BuildFilterDropdowns()
        {
            // League filter
            if (leagueFilterDropdown != null)
            {
                leagueFilterDropdown.ClearOptions();
                var leagueIds = allLegends
                    .Where(l => !string.IsNullOrEmpty(l.iconicLeagueId))
                    .Select(l => l.iconicLeagueId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                var leagueOptions = new List<string> { AllFilter };
                leagueOptions.AddRange(leagueIds.Select(FriendlyLeagueName));
                leagueFilterDropdown.AddOptions(leagueOptions);
                leagueFilterDropdown.onValueChanged.AddListener(_ => ApplyFilters());
            }

            // Era filter
            if (eraFilterDropdown != null)
            {
                eraFilterDropdown.ClearOptions();
                var eras = new List<string> { AllFilter, "1950s", "1960s", "1970s", "1980s",
                                               "1990s", "2000s", "2010s", "2020s" };
                eraFilterDropdown.AddOptions(eras);
                eraFilterDropdown.onValueChanged.AddListener(_ => ApplyFilters());
            }

            // Tier filter
            if (tierFilterDropdown != null)
            {
                tierFilterDropdown.ClearOptions();
                var tiers = new List<string>
                {
                    AllFilter,
                    "Ícono de club",
                    "Ícono nacional",
                    "Clase mundial",
                    "Los mejores de la historia"
                };
                tierFilterDropdown.AddOptions(tiers);
                tierFilterDropdown.onValueChanged.AddListener(_ => ApplyFilters());
            }
        }

        private void ApplyFilters()
        {
            filteredLegends = new List<LegendPlayerData>(allLegends);

            // League filter
            if (leagueFilterDropdown != null && leagueFilterDropdown.value > 0)
            {
                string selected = leagueFilterDropdown.options[leagueFilterDropdown.value].text;
                filteredLegends = filteredLegends
                    .Where(l => FriendlyLeagueName(l.iconicLeagueId) == selected)
                    .ToList();
            }

            // Era filter
            if (eraFilterDropdown != null && eraFilterDropdown.value > 0)
            {
                string selected = eraFilterDropdown.options[eraFilterDropdown.value].text;
                filteredLegends = filteredLegends
                    .Where(l => EraLabel(l.era) == selected)
                    .ToList();
            }

            // Tier filter
            if (tierFilterDropdown != null && tierFilterDropdown.value > 0)
            {
                int tierIndex = tierFilterDropdown.value; // 1 = ClubIcon(1)…4 = AllTimeGreat(4)
                filteredLegends = filteredLegends
                    .Where(l => (int)l.tier == tierIndex)
                    .ToList();
            }

            // Sort: tier descending, then overall descending
            filteredLegends = filteredLegends
                .OrderByDescending(l => (int)l.tier)
                .ThenByDescending(l => l.overallRating)
                .ToList();

            BuildLegendList();
        }

        // ── Legend list ────────────────────────────────────────────────────────

        private void BuildLegendList()
        {
            if (legendListContainer == null || legendCardPrefab == null) return;

            foreach (Transform child in legendListContainer)
                Destroy(child.gameObject);

            foreach (var legend in filteredLegends)
            {
                var card  = Instantiate(legendCardPrefab, legendListContainer);
                var texts = card.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 1) texts[0].text = legend.name;
                if (texts.Length >= 2) texts[1].text = $"{legend.overallRating} OVR · {legend.position}";
                if (texts.Length >= 3) texts[2].text = $"{legend.premiumCoinCost} 🪙";
                if (texts.Length >= 4) texts[3].text = legend.IsDeceased ? "✝" : "Retirado";

                // Colour deceased cards differently
                var bg = card.GetComponent<Image>();
                if (bg && legend.IsDeceased)
                    bg.color = new Color(0.85f, 0.85f, 0.92f);

                var btn    = card.GetComponent<Button>() ?? card.GetComponentInChildren<Button>();
                var capture = legend;
                if (btn) btn.onClick.AddListener(() => ShowDetail(capture));
            }
        }

        // ── Detail panel ───────────────────────────────────────────────────────

        private void ShowDetail(LegendPlayerData legend)
        {
            selectedLegend = legend;

            if (detailPanel) detailPanel.SetActive(true);
            if (detailNameText)        detailNameText.text        = legend.name;
            if (detailLifespanText)    detailLifespanText.text    = legend.LifespanLabel;
            if (detailNationalityText) detailNationalityText.text = $"🏴 {legend.nationality}";
            if (detailPositionText)    detailPositionText.text    = $"Posición: {legend.position}";
            if (detailOverallText)     detailOverallText.text     = $"OVR: {legend.overallRating}";
            if (detailEraText)         detailEraText.text         = $"Era: {EraLabel(legend.era)}";
            if (detailClubText)
                detailClubText.text =
                    $"Club icónico: {legend.iconicClub} ({FriendlyLeagueName(legend.iconicLeagueId)})";
            if (detailLegacyText)      detailLegacyText.text      = legend.legacyDescription;
            if (detailCostText)        detailCostText.text        = $"Coste: {legend.premiumCoinCost} 🪙";

            bool canAfford = CanPlayerAfford(legend.premiumCoinCost);
            if (purchaseButton) purchaseButton.interactable = canAfford;
            SetFeedback(canAfford ? string.Empty : "No tienes suficientes monedas premium.");
        }

        // ── Purchase ───────────────────────────────────────────────────────────

        private void OnPurchaseLegend()
        {
            if (selectedLegend == null) return;

            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (career == null)
            {
                SetFeedback("Inicia una carrera primero.");
                return;
            }

            if (career.premiumCoins < selectedLegend.premiumCoinCost)
            {
                SetFeedback("No tienes suficientes monedas premium.");
                return;
            }

            career.premiumCoins -= selectedLegend.premiumCoinCost;

            // Convert legend to a regular PlayerData and add to the squad
            var playerData = LegendToPlayer(selectedLegend);
            var teamService = ServiceLocator.Get<TransferMarketSystem>();

            SetFeedback($"¡{selectedLegend.name} se une a tu plantilla!");
            RefreshWallet();
            Debug.Log($"[LegendsUI] Purchased legend: {selectedLegend.name} " +
                      $"(remaining coins: {career.premiumCoins}).");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private bool CanPlayerAfford(int cost)
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            return career != null && career.premiumCoins >= cost;
        }

        private void RefreshWallet()
        {
            var career = ServiceLocator.Get<CareerSystem>()?.ActiveCareer;
            if (premiumCoinsText)
                premiumCoinsText.text = career != null
                    ? $"🪙 {career.premiumCoins}"
                    : "🪙 —";
            if (inGameBalanceText)
                inGameBalanceText.text = career != null
                    ? career.FormattedBalance
                    : "—";
        }

        private void SetFeedback(string msg)
        {
            if (feedbackText) feedbackText.text = msg;
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();

        // ── Static converters ──────────────────────────────────────────────────

        private const int LegendDeceasedAge = 99; // Sentinel: deceased legends are fielded as all-time greats
        private const int LegendRetiredAge  = 35; // Default age for living retired legends

        /// <summary>
        /// Creates a <see cref="PlayerData"/> from a <see cref="LegendPlayerData"/>
        /// so that the legend can be fielded in a squad.
        /// </summary>
        public static PlayerData LegendToPlayer(LegendPlayerData legend)
        {
            if (legend == null) return null;

            var p = new PlayerData
            {
                id          = $"legend_{legend.id}",
                name        = legend.name,
                age         = legend.IsDeceased ? LegendDeceasedAge : LegendRetiredAge,
                nationality = legend.nationality,
                position    = legend.position,
                overallRating = legend.overallRating,
                morale      = 80,
                fatigue     = 0,
                isAvailable = true,
                attributes  = legend.attributes ?? new PlayerAttributes()
            };

            // Legends have no contract / wage in the standard sense
            p.weeklyWage       = 0;
            p.contractYears    = 1;
            p.injuryProneness  = 10; // legends are assumed to be physically elite
            return p;
        }

        private static string FriendlyLeagueName(string leagueId) => leagueId switch
        {
            "league-liga-mx"     => "Liga MX",
            "league-brasileirao" => "Brasileirão",
            "league-laliga"      => "LaLiga",
            "league-premier"     => "Premier League",
            "league-seriea"      => "Serie A",
            _                    => leagueId ?? "—"
        };

        private static string EraLabel(LegendEra era) => era switch
        {
            LegendEra.S1950s => "1950s",
            LegendEra.S1960s => "1960s",
            LegendEra.S1970s => "1970s",
            LegendEra.S1980s => "1980s",
            LegendEra.S1990s => "1990s",
            LegendEra.S2000s => "2000s",
            LegendEra.S2010s => "2010s",
            LegendEra.S2020s => "2020s",
            _                => "—"
        };
    }
}
