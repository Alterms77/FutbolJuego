using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// Match-day screen: pre-match preview, live simulation at selectable speed
    /// (3 / 4 / 6 real minutes), running score, event feed, red-card man-advantage
    /// alert, penalty shootout, stats panel, and full-time summary.
    /// </summary>
    public class MatchDayUI : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private GameObject         previewPanel;
        [SerializeField] private TextMeshProUGUI    homeTeamLabel;
        [SerializeField] private TextMeshProUGUI    awayTeamLabel;
        [SerializeField] private TextMeshProUGUI    matchDateLabel;

        [Header("Simulation Speed")]
        [SerializeField] private Button             speed3MinButton;
        [SerializeField] private Button             speed4MinButton;
        [SerializeField] private Button             speed6MinButton;
        [SerializeField] private TextMeshProUGUI    speedIndicatorText;

        [Header("Live")]
        [SerializeField] private GameObject         livePanel;
        [SerializeField] private TextMeshProUGUI    scoreLabel;
        [SerializeField] private TextMeshProUGUI    minuteLabel;
        [SerializeField] private Transform          eventFeed;
        [SerializeField] private GameObject         eventRowPrefab;

        [Header("Man Advantage")]
        [SerializeField] private TextMeshProUGUI    manAdvantageLabel;

        [Header("Stats Panel")]
        [SerializeField] private TextMeshProUGUI    possessionLabel;
        [SerializeField] private TextMeshProUGUI    xGLabel;
        [SerializeField] private TextMeshProUGUI    shotsLabel;

        [Header("Summary")]
        [SerializeField] private GameObject         summaryPanel;
        [SerializeField] private TextMeshProUGUI    summaryText;

        [Header("Buttons")]
        [SerializeField] private Button             simulateButton;
        [SerializeField] private Button             returnButton;

        // ── Private state ──────────────────────────────────────────────────────

        private MatchData        pendingMatch;
        private SimulationSpeed  selectedSpeed = SimulationSpeed.FourMinutes;

        // ── MonoBehaviour ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (simulateButton) simulateButton.onClick.AddListener(OnSimulateButton);
            if (returnButton)   returnButton.onClick.AddListener(OnReturn);

            if (speed3MinButton) speed3MinButton.onClick.AddListener(
                () => SetSpeed(SimulationSpeed.ThreeMinutes));
            if (speed4MinButton) speed4MinButton.onClick.AddListener(
                () => SetSpeed(SimulationSpeed.FourMinutes));
            if (speed6MinButton) speed6MinButton.onClick.AddListener(
                () => SetSpeed(SimulationSpeed.SixMinutes));

            SetSpeed(SimulationSpeed.FourMinutes);
        }

        // ── Speed selection ────────────────────────────────────────────────────

        /// <summary>Sets the simulation animation speed and updates the indicator.</summary>
        public void SetSpeed(SimulationSpeed speed)
        {
            selectedSpeed = speed;
            if (speedIndicatorText)
                speedIndicatorText.text = speed switch
                {
                    SimulationSpeed.ThreeMinutes => "⚡ 3 min",
                    SimulationSpeed.FourMinutes  => "▶ 4 min",
                    SimulationSpeed.SixMinutes   => "🔍 6 min",
                    _                            => "▶ 4 min"
                };
        }

        // ── Preview ────────────────────────────────────────────────────────────

        /// <summary>Shows the pre-match preview for <paramref name="match"/>.</summary>
        public void ShowMatchPreview(MatchData match)
        {
            pendingMatch = match;
            SetPanelActive(previewPanel);

            if (homeTeamLabel)  homeTeamLabel.text  = match.homeTeamId;
            if (awayTeamLabel)  awayTeamLabel.text  = match.awayTeamId;
            if (matchDateLabel) matchDateLabel.text = match.matchDate.ToString("ddd dd MMM yyyy");
            if (manAdvantageLabel) manAdvantageLabel.text = string.Empty;
        }

        // ── Simulate button ────────────────────────────────────────────────────

        /// <summary>Called by the Simulate button.</summary>
        public void OnSimulateButton()
        {
            if (pendingMatch == null) return;
            StartCoroutine(PlayMatchAnimation(pendingMatch));
        }

        // ── Live score ─────────────────────────────────────────────────────────

        /// <summary>Updates the score and minute labels with the current running score.</summary>
        public void UpdateLiveScore(int homeScore, int awayScore, int minute,
                                    int homeReds, int awayReds)
        {
            string homeSuffix = homeReds > 0 ? " (10)" : string.Empty;
            string awaySuffix = awayReds > 0 ? " (10)" : string.Empty;

            if (scoreLabel)
                scoreLabel.text = $"{homeScore}{homeSuffix}  –  {awayScore}{awaySuffix}";
            if (minuteLabel)
                minuteLabel.text = $"{minute}'";
        }

        /// <summary>Updates the live stats panel (possession, xG, shots).</summary>
        public void UpdateStatsPanel(MatchStatistics stats)
        {
            if (possessionLabel)
                possessionLabel.text =
                    $"Possession  {stats.homePossession:F0}% – {stats.awayPossession:F0}%";
            if (xGLabel)
                xGLabel.text = $"xG  {stats.homeXG:F2} – {stats.awayXG:F2}";
            if (shotsLabel)
                shotsLabel.text =
                    $"Shots  {stats.homeShots} ({stats.homeShotsOnTarget} on target) " +
                    $"– {stats.awayShots} ({stats.awayShotsOnTarget} on target)";
        }

        /// <summary>Appends an event row to the live feed.</summary>
        public void ShowMatchEvent(MatchEvent evt)
        {
            if (eventFeed == null || eventRowPrefab == null) return;

            var row    = Instantiate(eventRowPrefab, eventFeed);
            var rowComp = row.GetComponent<MatchEventRow>();
            if (rowComp != null)
                rowComp.Setup(evt);
            else
            {
                var label = row.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = $"{evt.minute}' — {evt.description}";
            }
        }

        // ── Full-time summary ──────────────────────────────────────────────────

        /// <summary>Shows the full-time result summary panel.</summary>
        public void ShowMatchSummary(MatchData match)
        {
            SetPanelActive(summaryPanel);
            if (summaryText == null) return;

            var s = match.statistics;
            string result = match.homeScore > match.awayScore ? "WIN"
                          : match.homeScore < match.awayScore ? "LOSS"
                          : "DRAW";

            string penaltyLine = string.Empty;
            if (match.wentToPenalties && match.penaltyShootout != null)
            {
                var ps = match.penaltyShootout;
                penaltyLine = $"\nPenalties: {match.homeTeamId} {ps.homeScore}–{ps.awayScore} {match.awayTeamId}" +
                              $"  → Winner: {ps.winnerTeamId}\n";
            }

            summaryText.text =
                $"⏱ FULL TIME  [{result}]\n" +
                $"{match.homeTeamId}  {match.homeScore} – {match.awayScore}  {match.awayTeamId}" +
                penaltyLine + "\n" +
                $"xG: {s.homeXG:F2} – {s.awayXG:F2}\n" +
                $"Shots: {s.homeShots} – {s.awayShots}  " +
                $"(SoT: {s.homeShotsOnTarget} – {s.awayShotsOnTarget})\n" +
                $"Possession: {s.homePossession:F0}% – {s.awayPossession:F0}%\n" +
                $"Passes: {s.homePasses} – {s.awayPasses}\n" +
                $"Corners: {s.homeCorners} – {s.awayCorners}\n" +
                $"Yellow Cards: {s.homeYellowCards} – {s.awayYellowCards}\n" +
                $"Red Cards: {s.homeRedCards} – {s.awayRedCards}";
        }

        // ── Animation coroutine ────────────────────────────────────────────────

        private IEnumerator PlayMatchAnimation(MatchData match)
        {
            SetPanelActive(livePanel);
            if (manAdvantageLabel) manAdvantageLabel.text = string.Empty;

            // Clear event feed
            if (eventFeed)
                foreach (Transform child in eventFeed)
                    Destroy(child.gameObject);

            // Real-time seconds allocated per game minute
            // 3 min → 180 s / 90 = 2.00 s/min
            // 4 min → 240 s / 90 = 2.67 s/min
            // 6 min → 360 s / 90 = 4.00 s/min
            float secondsPerMinute = (int)selectedSpeed * 60f / Constants.MatchDurationMinutes;

            // Build a minute → events look-up for O(1) access in the loop
            var byMinute = new Dictionary<int, List<MatchEvent>>();
            foreach (var evt in match.events)
            {
                if (!byMinute.ContainsKey(evt.minute))
                    byMinute[evt.minute] = new List<MatchEvent>();
                byMinute[evt.minute].Add(evt);
            }

            // Running score accumulation (so score builds up live, not final from start)
            int runningHome = 0;
            int runningAway = 0;
            int homeReds    = 0;
            int awayReds    = 0;

            for (int minute = 1; minute <= Constants.MatchDurationMinutes; minute++)
            {
                // Process events at this game minute
                if (byMinute.TryGetValue(minute, out var eventsNow))
                {
                    foreach (var evt in eventsNow)
                    {
                        // Accumulate running score
                        if (evt.type == MatchEventType.Goal)
                        {
                            if (evt.teamId == match.homeTeamId) runningHome++;
                            else                                runningAway++;
                        }

                        // Track man disadvantage
                        if (evt.type == MatchEventType.RedCard)
                        {
                            if (evt.teamId == match.homeTeamId) homeReds++;
                            else                                awayReds++;

                            string shortTeam = evt.teamId == match.homeTeamId
                                ? match.homeTeamId : match.awayTeamId;
                            if (manAdvantageLabel)
                                manAdvantageLabel.text =
                                    $"🟥 {shortTeam} down to 10 men! ({minute}')";
                        }

                        ShowMatchEvent(evt);
                    }
                }

                UpdateLiveScore(runningHome, runningAway, minute, homeReds, awayReds);
                if (match.statistics != null) UpdateStatsPanel(match.statistics);

                yield return new WaitForSeconds(secondsPerMinute);
            }

            // Final state after 90'
            UpdateLiveScore(runningHome, runningAway, 90, homeReds, awayReds);
            yield return new WaitForSeconds(0.5f);

            // Penalty shootout animation (for cup matches ending level)
            if (match.wentToPenalties && match.penaltyShootout != null)
            {
                if (minuteLabel) minuteLabel.text = "PSO";
                if (manAdvantageLabel) manAdvantageLabel.text = "⚽ PENALTY SHOOTOUT";

                float kickDelay = Mathf.Max(0.8f, secondsPerMinute * 0.4f);
                int   penHome   = 0;
                int   penAway   = 0;

                foreach (var kick in match.penaltyShootout.kicks)
                {
                    if (kick.scored)
                    {
                        if (kick.teamId == match.homeTeamId) penHome++;
                        else                                 penAway++;
                    }

                    ShowMatchEvent(new MatchEvent
                    {
                        minute      = 91,
                        type        = kick.scored
                                    ? MatchEventType.PenaltyGoal
                                    : MatchEventType.MissedPenalty,
                        teamId      = kick.teamId,
                        playerId    = kick.playerId,
                        description = kick.description
                    });

                    if (scoreLabel)
                        scoreLabel.text =
                            $"{match.homeScore} ({penHome}) – ({penAway}) {match.awayScore}";

                    yield return new WaitForSeconds(kickDelay);
                }
                yield return new WaitForSeconds(0.5f);
            }

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
