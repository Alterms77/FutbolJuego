using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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
    public partial class MatchDayUI : Control
    {
        [ExportGroup("Preview")]
        [Export] public Control previewPanel;
        [Export] public Label homeTeamLabel;
        [Export] public Label awayTeamLabel;
        [Export] public Label matchDateLabel;

        [ExportGroup("Simulation Speed")]
        [Export] public Button speed3MinButton;
        [Export] public Button speed4MinButton;
        [Export] public Button speed6MinButton;
        [Export] public Label speedIndicatorText;

        [ExportGroup("Live")]
        [Export] public Control livePanel;
        [Export] public Label scoreLabel;
        [Export] public Label minuteLabel;
        [Export] public Control eventFeed;
        [Export] public PackedScene eventRowPrefab;

        [ExportGroup("Man Advantage")]
        [Export] public Label manAdvantageLabel;

        [ExportGroup("Stats Panel")]
        [Export] public Label possessionLabel;
        [Export] public Label xGLabel;
        [Export] public Label shotsLabel;

        [ExportGroup("Summary")]
        [Export] public Control summaryPanel;
        [Export] public Label summaryText;

        [ExportGroup("Buttons")]
        [Export] public Button simulateButton;
        [Export] public Button returnButton;

        // ── Private state ──────────────────────────────────────────────────────

        private MatchData       pendingMatch;
        private SimulationSpeed selectedSpeed = SimulationSpeed.FourMinutes;

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (simulateButton != null) simulateButton.Pressed += OnSimulateButton;
            if (returnButton != null)   returnButton.Pressed   += OnReturn;

            if (speed3MinButton != null) speed3MinButton.Pressed +=
                () => SetSpeed(SimulationSpeed.ThreeMinutes);
            if (speed4MinButton != null) speed4MinButton.Pressed +=
                () => SetSpeed(SimulationSpeed.FourMinutes);
            if (speed6MinButton != null) speed6MinButton.Pressed +=
                () => SetSpeed(SimulationSpeed.SixMinutes);

            SetSpeed(SimulationSpeed.FourMinutes);
        }

        // ── Speed selection ────────────────────────────────────────────────────

        /// <summary>Sets the simulation animation speed and updates the indicator.</summary>
        public void SetSpeed(SimulationSpeed speed)
        {
            selectedSpeed = speed;
            if (speedIndicatorText != null)
                speedIndicatorText.Text = speed switch
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

            if (homeTeamLabel != null)  homeTeamLabel.Text  = match.homeTeamId;
            if (awayTeamLabel != null)  awayTeamLabel.Text  = match.awayTeamId;
            if (matchDateLabel != null) matchDateLabel.Text = match.matchDate.ToString("ddd dd MMM yyyy");
            if (manAdvantageLabel != null) manAdvantageLabel.Text = string.Empty;
        }

        // ── Simulate button ────────────────────────────────────────────────────

        /// <summary>Called by the Simulate button.</summary>
        public void OnSimulateButton()
        {
            if (pendingMatch == null) return;
            _ = PlayMatchAnimation(pendingMatch);
        }

        // ── Live score ─────────────────────────────────────────────────────────

        /// <summary>Updates the score and minute labels with the current running score.</summary>
        public void UpdateLiveScore(int homeScore, int awayScore, int minute,
                                    int homeReds, int awayReds)
        {
            string homeSuffix = homeReds > 0 ? $" ({11 - homeReds})" : string.Empty;
            string awaySuffix = awayReds > 0 ? $" ({11 - awayReds})" : string.Empty;

            if (scoreLabel != null)
                scoreLabel.Text = $"{homeScore}{homeSuffix}  –  {awayScore}{awaySuffix}";
            if (minuteLabel != null)
                minuteLabel.Text = $"{minute}'";
        }

        /// <summary>Updates the live stats panel (possession, xG, shots).</summary>
        public void UpdateStatsPanel(MatchStatistics stats)
        {
            if (possessionLabel != null)
                possessionLabel.Text =
                    $"Possession  {stats.homePossession:F0}% – {stats.awayPossession:F0}%";
            if (xGLabel != null)
                xGLabel.Text = $"xG  {stats.homeXG:F2} – {stats.awayXG:F2}";
            if (shotsLabel != null)
                shotsLabel.Text =
                    $"Shots  {stats.homeShots} ({stats.homeShotsOnTarget} on target) " +
                    $"– {stats.awayShots} ({stats.awayShotsOnTarget} on target)";
        }

        /// <summary>Appends an event row to the live feed.</summary>
        public void ShowMatchEvent(MatchEvent evt)
        {
            if (eventFeed == null || eventRowPrefab == null) return;

            var row     = eventRowPrefab.Instantiate<Control>();
            eventFeed.AddChild(row);
            var rowComp = row as MatchEventRow;
            if (rowComp != null)
                rowComp.Setup(evt);
            else
            {
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null) label.Text = $"{evt.minute}' — {evt.description}";
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

            summaryText.Text =
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

        // ── Animation task ─────────────────────────────────────────────────────

        private async Task PlayMatchAnimation(MatchData match)
        {
            SetPanelActive(livePanel);
            if (manAdvantageLabel != null) manAdvantageLabel.Text = string.Empty;

            if (eventFeed != null)
                foreach (var child in eventFeed.GetChildren())
                    child.QueueFree();

            float secondsPerMinute = SecondsPerMinuteFor(selectedSpeed);

            var byMinute = new Dictionary<int, List<MatchEvent>>();
            foreach (var evt in match.events)
            {
                if (!byMinute.ContainsKey(evt.minute))
                    byMinute[evt.minute] = new List<MatchEvent>();
                byMinute[evt.minute].Add(evt);
            }

            int runningHome = 0;
            int runningAway = 0;
            int homeReds    = 0;
            int awayReds    = 0;

            for (int minute = 1; minute <= Constants.MatchDurationMinutes; minute++)
            {
                if (byMinute.TryGetValue(minute, out var eventsNow))
                {
                    foreach (var evt in eventsNow)
                    {
                        if (evt.type == MatchEventType.Goal)
                        {
                            if (evt.teamId == match.homeTeamId) runningHome++;
                            else                                runningAway++;
                        }

                        if (evt.type == MatchEventType.RedCard)
                        {
                            if (evt.teamId == match.homeTeamId) homeReds++;
                            else                                awayReds++;

                            string shortTeam = evt.teamId == match.homeTeamId
                                ? match.homeTeamId : match.awayTeamId;
                            if (manAdvantageLabel != null)
                                manAdvantageLabel.Text =
                                    $"🟥 {shortTeam} down to 10 men! ({minute}')";
                        }

                        ShowMatchEvent(evt);
                    }
                }

                UpdateLiveScore(runningHome, runningAway, minute, homeReds, awayReds);
                if (match.statistics != null) UpdateStatsPanel(match.statistics);

                await Task.Delay((int)(secondsPerMinute * 1000));
            }

            UpdateLiveScore(runningHome, runningAway, 90, homeReds, awayReds);
            await Task.Delay((int)(0.5f * 1000));

            if (match.wentToPenalties && match.penaltyShootout != null)
            {
                if (minuteLabel != null) minuteLabel.Text = "PSO";
                if (manAdvantageLabel != null) manAdvantageLabel.Text = "⚽ PENALTY SHOOTOUT";

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

                    if (scoreLabel != null)
                        scoreLabel.Text =
                            $"{match.homeScore} ({penHome}) – ({penAway}) {match.awayScore}";

                    await Task.Delay((int)(kickDelay * 1000));
                }
                await Task.Delay((int)(0.5f * 1000));
            }

            ShowMatchSummary(match);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void OnReturn() => SceneNavigator.Instance?.GoToDashboard();

        private void SetPanelActive(Control active)
        {
            if (previewPanel != null)  previewPanel.Visible  = active == previewPanel;
            if (livePanel != null)     livePanel.Visible     = active == livePanel;
            if (summaryPanel != null)  summaryPanel.Visible  = active == summaryPanel;
        }

        private static float SecondsPerMinuteFor(SimulationSpeed speed) => speed switch
        {
            SimulationSpeed.ThreeMinutes => 3f * 60f / Constants.MatchDurationMinutes,
            SimulationSpeed.FourMinutes  => 4f * 60f / Constants.MatchDurationMinutes,
            SimulationSpeed.SixMinutes   => 6f * 60f / Constants.MatchDurationMinutes,
            _                            => 4f * 60f / Constants.MatchDurationMinutes
        };
    }
}
