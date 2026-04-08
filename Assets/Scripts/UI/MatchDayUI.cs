using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.UI.Components;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Match-day screen: pre-match preview, live score during animation,
    /// minute-by-minute event feed, stats panel, and full-time summary.
    /// </summary>
    public class MatchDayUI : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TextMeshProUGUI homeTeamLabel;
        [SerializeField] private TextMeshProUGUI awayTeamLabel;
        [SerializeField] private TextMeshProUGUI matchDateLabel;

        [Header("Live")]
        [SerializeField] private GameObject livePanel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private TextMeshProUGUI minuteLabel;
        [SerializeField] private Transform eventFeed;
        [SerializeField] private GameObject eventRowPrefab;

        [Header("Stats Panel")]
        [SerializeField] private TextMeshProUGUI possessionLabel;
        [SerializeField] private TextMeshProUGUI xGLabel;
        [SerializeField] private TextMeshProUGUI shotsLabel;

        [Header("Summary")]
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI summaryText;

        [Header("Buttons")]
        [SerializeField] private Button simulateButton;
        [SerializeField] private Button returnButton;

        private MatchData pendingMatch;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (simulateButton) simulateButton.onClick.AddListener(OnSimulateButton);
            if (returnButton)   returnButton.onClick.AddListener(OnReturn);
        }

        // ── Preview ────────────────────────────────────────────────────────────

        /// <summary>Shows the pre-match preview for <paramref name="match"/>.</summary>
        public void ShowMatchPreview(MatchData match)
        {
            pendingMatch = match;

            SetPanelActive(previewPanel);

            if (homeTeamLabel) homeTeamLabel.text = match.homeTeamId;
            if (awayTeamLabel) awayTeamLabel.text = match.awayTeamId;
            if (matchDateLabel) matchDateLabel.text = match.matchDate.ToString("ddd dd MMM yyyy");
        }

        // ── Simulate button ────────────────────────────────────────────────────

        /// <summary>Called by the Simulate button.</summary>
        public void OnSimulateButton()
        {
            if (pendingMatch == null) return;
            StartCoroutine(PlayMatchAnimation(pendingMatch));
        }

        // ── Live updates ───────────────────────────────────────────────────────

        /// <summary>Updates the live score and stats display.</summary>
        public void UpdateLiveScore(MatchData match)
        {
            if (scoreLabel)  scoreLabel.text  = $"{match.homeScore} – {match.awayScore}";
            if (minuteLabel) minuteLabel.text = $"{match.currentMinute}'";

            if (match.statistics != null)
                UpdateStatsPanel(match.statistics);
        }

        /// <summary>Updates the live stats panel (possession, xG, shots).</summary>
        public void UpdateStatsPanel(MatchStatistics stats)
        {
            if (possessionLabel)
                possessionLabel.text =
                    $"Possession  {stats.homePossession:F0}% – {stats.awayPossession:F0}%";

            if (xGLabel)
                xGLabel.text =
                    $"xG  {stats.homeXG:F2} – {stats.awayXG:F2}";

            if (shotsLabel)
                shotsLabel.text =
                    $"Shots  {stats.homeShots} ({stats.homeShotsOnTarget} on target) " +
                    $"– {stats.awayShots} ({stats.awayShotsOnTarget} on target)";
        }

        /// <summary>Appends an event row to the live feed.</summary>
        public void ShowMatchEvent(MatchEvent evt)
        {
            if (eventFeed == null || eventRowPrefab == null) return;

            var row = Instantiate(eventRowPrefab, eventFeed);

            // Try rich MatchEventRow component first
            var rowComp = row.GetComponent<MatchEventRow>();
            if (rowComp != null)
            {
                rowComp.Setup(evt);
            }
            else
            {
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                    label.text = $"{evt.minute}' — {evt.description}";
            }
        }

        // ── Full-time summary ──────────────────────────────────────────────────

        /// <summary>Shows the full-time result summary panel.</summary>
        public void ShowMatchSummary(MatchData match)
        {
            SetPanelActive(summaryPanel);

            if (summaryText == null) return;

            var s = match.statistics;
            summaryText.text =
                $"FULL TIME\n" +
                $"{match.homeTeamId}  {match.homeScore} – {match.awayScore}  {match.awayTeamId}\n\n" +
                $"xG: {s.homeXG:F2} – {s.awayXG:F2}\n" +
                $"Shots: {s.homeShots} – {s.awayShots}  " +
                $"(On Target: {s.homeShotsOnTarget} – {s.awayShotsOnTarget})\n" +
                $"Possession: {s.homePossession:F0}% – {s.awayPossession:F0}%\n" +
                $"Passes: {s.homePasses} – {s.awayPasses}\n" +
                $"Corners: {s.homeCorners} – {s.awayCorners}\n" +
                $"Yellow Cards: {s.homeYellowCards} – {s.awayYellowCards}";
        }

        // ── Animation coroutine ────────────────────────────────────────────────

        private IEnumerator PlayMatchAnimation(MatchData match)
        {
            SetPanelActive(livePanel);

            // Clear feed
            if (eventFeed)
                foreach (Transform child in eventFeed)
                    Destroy(child.gameObject);

            // Replay events in time order
            int lastMinute = 0;
            foreach (var evt in match.events)
            {
                if (evt.minute > lastMinute)
                {
                    lastMinute = evt.minute;
                    match.currentMinute = lastMinute;
                    UpdateLiveScore(match);
                    yield return new WaitForSeconds(0.08f);
                }
                ShowMatchEvent(evt);
            }

            match.currentMinute = 90;
            UpdateLiveScore(match);
            yield return new WaitForSeconds(0.5f);

            ShowMatchSummary(match);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void OnReturn() => SceneNavigator.Instance?.GoToDashboard();

        private void SetPanelActive(GameObject active)
        {
            if (previewPanel)  previewPanel.SetActive(active == previewPanel);
            if (livePanel)     livePanel.SetActive(active == livePanel);
            if (summaryPanel)  summaryPanel.SetActive(active == summaryPanel);
        }
    }
}
