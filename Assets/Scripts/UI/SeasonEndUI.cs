using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Data;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Season-end summary screen displayed after the last fixture of each season.
    ///
    /// Shows:
    /// <list type="bullet">
    ///   <item>Season number and updated career balance.</item>
    ///   <item>Players who have reached retirement age, each with a
    ///         <em>Retirar</em> / <em>Continuar</em> button.</item>
    ///   <item>Players whose contracts have expired.</item>
    ///   <item>Per-player age change and rating change.</item>
    ///   <item>Season top-scorer and best GK (golden awards).</item>
    /// </list>
    ///
    /// Wire up the <see cref="AdvanceSeason"/> call from the match calendar or
    /// competition manager once the last match of the season is completed.
    /// </summary>
    public class SeasonEndUI : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI seasonTitleText;
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("Awards")]
        [SerializeField] private TextMeshProUGUI goldenBootText;
        [SerializeField] private TextMeshProUGUI goldenGloveText;

        [Header("Player list")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerRowPrefab;

        [Header("Navigation")]
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI resultMessage;

        // ── Private state ──────────────────────────────────────────────────────

        private List<PlayerSeasonEndResult> seasonResults;
        private List<PlayerData>            squad;
        private CareerData                  career;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (continueButton) continueButton.onClick.AddListener(OnContinue);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Triggers the season advance and populates the UI.
        /// Call this after the last match of the current season completes.
        /// </summary>
        /// <param name="wonLeague">Whether the managed team won the league title.</param>
        /// <param name="wonCup">Whether the managed team won the domestic cup.</param>
        public void AdvanceSeason(bool wonLeague = false, bool wonCup = false)
        {
            var seasonSys   = ServiceLocator.Get<SeasonSystem>();
            var careerSys   = ServiceLocator.Get<CareerSystem>();
            var ratingSystem = ServiceLocator.Get<PlayerRatingSystem>();

            career = careerSys?.ActiveCareer;
            if (career == null)
            {
                ShowMessage("No hay carrera activa.");
                return;
            }

            var teams   = DataLoader.LoadAllTeams();
            var managed = teams?.Find(t => t.id == career.managedTeamId);
            squad       = managed?.squad ?? new List<PlayerData>();

            // Award titles before advancing (so current squad gets credit)
            if (wonLeague) seasonSys?.AwardLeagueTitle(squad, career);
            if (wonCup)    seasonSys?.AwardCupTitle(squad, career);

            // Determine top scorer and best GK from current season stats
            PlayerData topScorer = squad
                .Where(p => p.position != PlayerPosition.GK)
                .OrderByDescending(p => p.seasonStats?.goals ?? 0)
                .FirstOrDefault();

            PlayerData topGK = squad
                .Where(p => p.position == PlayerPosition.GK)
                .OrderByDescending(p => p.seasonStats?.savesMade ?? 0)
                .FirstOrDefault();

            if (topScorer != null) SeasonSystem.AwardGoldenBoot(topScorer);
            if (topGK     != null) SeasonSystem.AwardGoldenGlove(topGK);

            // Advance the season (ages players, rolls stats, increments season counter)
            seasonResults = seasonSys?.AdvanceSeason(squad, career, career.managedLeagueId)
                            ?? new List<PlayerSeasonEndResult>();

            // Refresh market values
            if (ratingSystem != null)
                foreach (var p in squad)
                    ratingSystem.CalculateMarketValue(p, career.managedLeagueId);

            RefreshUI(topScorer, topGK);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void RefreshUI(PlayerData topScorer, PlayerData topGK)
        {
            string symbol = career?.CurrencySymbol ?? "€";
            int    season = career?.season ?? 1;

            if (seasonTitleText)
                seasonTitleText.text = $"Fin de Temporada {season - 1}  →  Temporada {season}";

            if (balanceText)
                balanceText.text = $"Saldo: {career?.FormattedBalance ?? "—"}";

            if (goldenBootText)
                goldenBootText.text = topScorer != null
                    ? $"⚽ Bota de Oro: {topScorer.name} ({topScorer.seasonStats?.goals ?? 0} goles)"
                    : "⚽ Bota de Oro: —";

            if (goldenGloveText)
                goldenGloveText.text = topGK != null
                    ? $"🧤 Guante de Oro: {topGK.name} ({topGK.seasonStats?.savesMade ?? 0} paradas)"
                    : "🧤 Guante de Oro: —";

            RebuildPlayerList(symbol);
        }

        private void RebuildPlayerList(string symbol)
        {
            if (playerListContainer == null || playerRowPrefab == null) return;

            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);

            if (seasonResults == null) return;

            foreach (var result in seasonResults)
            {
                var row   = Instantiate(playerRowPrefab, playerListContainer);
                var label = row.GetComponentInChildren<TextMeshProUGUI>();

                string ratingChange = result.OverallAfter > result.OverallBefore
                    ? $"<color=#44FF44>+{result.OverallAfter - result.OverallBefore}</color>"
                    : result.OverallAfter < result.OverallBefore
                        ? $"<color=#FF4444>{result.OverallAfter - result.OverallBefore}</color>"
                        : "<color=#AAAAAA>=</color>";

                string rowText =
                    $"{result.Player.name}  " +
                    $"Edad {result.AgeBefore}→{result.AgeAfter}  " +
                    $"OVR {result.OverallBefore}→{result.OverallAfter} ({ratingChange})  " +
                    $"{symbol}{result.Player.marketValue / 1_000_000f:F0}M";

                if (result.IsRetirable)
                    rowText += "  <color=#FFD700>[RETIRABLE]</color>";
                if (result.ContractExpired)
                    rowText += "  <color=#FF8800>[CONTRATO VENCIDO]</color>";

                if (label) label.text = rowText;

                // Retire button (only for retirable players)
                if (result.IsRetirable)
                {
                    var btn = row.GetComponentInChildren<Button>();
                    if (btn)
                    {
                        var captured = result.Player;
                        btn.GetComponentInChildren<TextMeshProUGUI>().text = "Retirar";
                        btn.onClick.AddListener(() => OnRetirePlayer(captured, row));
                    }
                }
            }
        }

        private void OnRetirePlayer(PlayerData player, GameObject row)
        {
            var seasonSys = ServiceLocator.Get<SeasonSystem>();
            bool retired  = seasonSys?.RetirePlayer(player, squad) ?? false;

            if (retired)
            {
                ShowMessage($"{player.name} se ha retirado. " +
                            $"Carrera: {player.careerStats?.ToSummaryString() ?? "—"}");
                if (row) Destroy(row);
            }
            else
            {
                ShowMessage($"No se puede retirar a {player.name}.");
            }
        }

        private void OnContinue()
        {
            SceneNavigator.Instance?.GoToDashboard();
        }

        private void ShowMessage(string message)
        {
            if (resultMessage) resultMessage.text = message;
        }
    }
}
