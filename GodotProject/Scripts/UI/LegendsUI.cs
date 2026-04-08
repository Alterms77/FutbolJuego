using System.Collections.Generic;
using System.Linq;
using Godot;
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
    public partial class LegendsUI : Control
    {
        [ExportGroup("Filter bar")]
        [Export] public OptionButton leagueFilterDropdown;
        [Export] public OptionButton eraFilterDropdown;
        [Export] public OptionButton tierFilterDropdown;

        [ExportGroup("Legend list")]
        [Export] public Control legendListContainer;
        [Export] public PackedScene legendCardPrefab;

        [ExportGroup("Detail panel")]
        [Export] public Control detailPanel;
        [Export] public Label detailNameText;
        [Export] public Label detailLifespanText;
        [Export] public Label detailNationalityText;
        [Export] public Label detailPositionText;
        [Export] public Label detailOverallText;
        [Export] public Label detailEraText;
        [Export] public Label detailClubText;
        [Export] public Label detailLegacyText;
        [Export] public Label detailCostText;
        [Export] public Button purchaseButton;

        [ExportGroup("Wallet")]
        [Export] public Label premiumCoinsText;
        [Export] public Label inGameBalanceText;

        [ExportGroup("Feedback")]
        [Export] public Label feedbackText;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        // ── Private state ──────────────────────────────────────────────────────

        private List<LegendPlayerData> allLegends      = new List<LegendPlayerData>();
        private List<LegendPlayerData> filteredLegends = new List<LegendPlayerData>();
        private LegendPlayerData       selectedLegend;

        private const string AllFilter = "Todos";

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (backButton != null)     backButton.Pressed     += OnBack;
            if (purchaseButton != null) purchaseButton.Pressed += OnPurchaseLegend;

            allLegends = DataLoader.LoadAllLegends();

            BuildFilterDropdowns();
            ApplyFilters();
            if (detailPanel != null) detailPanel.Visible = false;
            RefreshWallet();
        }

        // ── Filters ────────────────────────────────────────────────────────────

        private void BuildFilterDropdowns()
        {
            if (leagueFilterDropdown != null)
            {
                leagueFilterDropdown.Clear();
                var leagueIds = allLegends
                    .Where(l => !string.IsNullOrEmpty(l.iconicLeagueId))
                    .Select(l => l.iconicLeagueId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                leagueFilterDropdown.AddItem(AllFilter);
                foreach (var id in leagueIds) leagueFilterDropdown.AddItem(FriendlyLeagueName(id));
                leagueFilterDropdown.ItemSelected += (_) => ApplyFilters();
            }

            if (eraFilterDropdown != null)
            {
                eraFilterDropdown.Clear();
                foreach (var era in new[] { AllFilter, "1950s", "1960s", "1970s", "1980s",
                                            "1990s", "2000s", "2010s", "2020s" })
                    eraFilterDropdown.AddItem(era);
                eraFilterDropdown.ItemSelected += (_) => ApplyFilters();
            }

            if (tierFilterDropdown != null)
            {
                tierFilterDropdown.Clear();
                foreach (var tier in new[] { AllFilter, "Ícono de club", "Ícono nacional",
                                             "Clase mundial", "Los mejores de la historia" })
                    tierFilterDropdown.AddItem(tier);
                tierFilterDropdown.ItemSelected += (_) => ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            filteredLegends = new List<LegendPlayerData>(allLegends);

            if (leagueFilterDropdown != null && leagueFilterDropdown.Selected > 0)
            {
                string selected = leagueFilterDropdown.GetItemText(leagueFilterDropdown.Selected);
                filteredLegends = filteredLegends
                    .Where(l => FriendlyLeagueName(l.iconicLeagueId) == selected)
                    .ToList();
            }

            if (eraFilterDropdown != null && eraFilterDropdown.Selected > 0)
            {
                string selected = eraFilterDropdown.GetItemText(eraFilterDropdown.Selected);
                filteredLegends = filteredLegends
                    .Where(l => EraLabel(l.era) == selected)
                    .ToList();
            }

            if (tierFilterDropdown != null && tierFilterDropdown.Selected > 0)
            {
                int tierIndex = tierFilterDropdown.Selected;
                filteredLegends = filteredLegends
                    .Where(l => (int)l.tier == tierIndex)
                    .ToList();
            }

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

            foreach (var child in legendListContainer.GetChildren())
                child.QueueFree();

            foreach (var legend in filteredLegends)
            {
                var card  = legendCardPrefab.Instantiate<Control>();
                legendListContainer.AddChild(card);
                var texts = card.FindChildren("*", "Label", true, false);

                if (texts.Count >= 1) (texts[0] as Label).Text = legend.name;
                if (texts.Count >= 2) (texts[1] as Label).Text = $"{legend.overallRating} OVR · {legend.position}";
                if (texts.Count >= 3) (texts[2] as Label).Text = $"{legend.premiumCoinCost} 🪙";
                if (texts.Count >= 4) (texts[3] as Label).Text = legend.IsDeceased ? "✝" : "Retirado";

                var bg = card.FindChild("ColorRect", true, false) as ColorRect;
                if (bg != null && legend.IsDeceased)
                    bg.Color = new Color(0.85f, 0.85f, 0.92f);

                var btn = card as Button ?? card.FindChild("Button", true, false) as Button;
                var capture = legend;
                if (btn != null) btn.Pressed += () => ShowDetail(capture);
            }
        }

        // ── Detail panel ───────────────────────────────────────────────────────

        private void ShowDetail(LegendPlayerData legend)
        {
            selectedLegend = legend;

            if (detailPanel != null) detailPanel.Visible = true;
            if (detailNameText != null)        detailNameText.Text        = legend.name;
            if (detailLifespanText != null)    detailLifespanText.Text    = legend.LifespanLabel;
            if (detailNationalityText != null) detailNationalityText.Text = $"🏴 {legend.nationality}";
            if (detailPositionText != null)    detailPositionText.Text    = $"Posición: {legend.position}";
            if (detailOverallText != null)     detailOverallText.Text     = $"OVR: {legend.overallRating}";
            if (detailEraText != null)         detailEraText.Text         = $"Era: {EraLabel(legend.era)}";
            if (detailClubText != null)
                detailClubText.Text =
                    $"Club icónico: {legend.iconicClub} ({FriendlyLeagueName(legend.iconicLeagueId)})";
            if (detailLegacyText != null) detailLegacyText.Text = legend.legacyDescription;
            if (detailCostText != null)   detailCostText.Text   = $"Coste: {legend.premiumCoinCost} 🪙";

            bool canAfford = CanPlayerAfford(legend.premiumCoinCost);
            if (purchaseButton != null) purchaseButton.Disabled = !canAfford;
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

            var playerData  = LegendToPlayer(selectedLegend);
            var teamService = ServiceLocator.Get<TransferMarketSystem>();

            SetFeedback($"¡{selectedLegend.name} se une a tu plantilla!");
            RefreshWallet();
            GD.Print($"[LegendsUI] Purchased legend: {selectedLegend.name} " +
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
            if (premiumCoinsText != null)
                premiumCoinsText.Text = career != null
                    ? $"🪙 {career.premiumCoins}"
                    : "🪙 —";
            if (inGameBalanceText != null)
                inGameBalanceText.Text = career != null
                    ? career.FormattedBalance
                    : "—";
        }

        private void SetFeedback(string msg)
        {
            if (feedbackText != null) feedbackText.Text = msg;
        }

        private void OnBack() => SceneNavigator.Instance?.GoToDashboard();

        // ── Static converters ──────────────────────────────────────────────────

        private const int LegendDeceasedAge = 99;
        private const int LegendRetiredAge  = 35;

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

            p.weeklyWage      = 0;
            p.contractYears   = 1;
            p.injuryProneness = 10;
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
