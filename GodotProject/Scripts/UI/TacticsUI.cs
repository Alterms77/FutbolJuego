using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;
using FutbolJuego.Utils;

namespace FutbolJuego.UI
{
    /// <summary>
    /// Displays the tactics setup screen: formation selector, sliders, pitch
    /// visualisation with player rating and energy badges, and the pre-match
    /// prediction panel.  Supports basic drag repositioning of player dots.
    /// </summary>
    public partial class TacticsUI : Control
    {
        [ExportGroup("Formation")]
        [Export] public OptionButton formationDropdown;
        [Export] public Control pitchContainer;
        [Export] public PackedScene playerDotPrefab;

        [ExportGroup("Sliders")]
        [Export] public ProgressBar pressingSlider;
        [Export] public ProgressBar tempoSlider;
        [Export] public ProgressBar widthSlider;
        [Export] public ProgressBar defensiveLineSlider;

        [ExportGroup("Prediction")]
        [Export] public Label predictionText;
        [Export] public Control predictionPanel;

        [ExportGroup("Navigation")]
        [Export] public Button backButton;

        [ExportGroup("Top Controls")]
        [Export] public Label captainNameLabel;
        [Export] public OptionButton captainDropdown;
        [Export] public Label pressingLabel;
        [Export] public Label formationLabel;

        [ExportGroup("Substitutes")]
        [Export] public Control substituteContainer;
        [Export] public PackedScene substituteDotPrefab;

        [ExportGroup("Bottom Bar")]
        [Export] public Button playMatchButton;
        [Export] public Label teamRatingLabel;
        [Export] public Label opponentRatingLabel;

        private TacticData currentTactic;
        private List<PlayerData> currentSquad = new List<PlayerData>();

        // ── Godot lifecycle ────────────────────────────────────────────────────

        public override void _Ready()
        {
            if (backButton != null) backButton.Pressed += OnBack;
            if (pressingSlider != null)      pressingSlider.ValueChanged      += v => OnTacticParameterChanged("pressing",      (int)v);
            if (tempoSlider != null)         tempoSlider.ValueChanged         += v => OnTacticParameterChanged("tempo",         (int)v);
            if (widthSlider != null)         widthSlider.ValueChanged         += v => OnTacticParameterChanged("width",         (int)v);
            if (defensiveLineSlider != null) defensiveLineSlider.ValueChanged += v => OnTacticParameterChanged("defensiveLine", (int)v);

            if (playMatchButton != null)
                playMatchButton.Pressed += () => FutbolJuego.Core.SceneNavigator.Instance?.GoToMatch();
            if (formationDropdown != null)
                formationDropdown.ItemSelected += (idx) =>
                {
                    var formations = (Formation[])System.Enum.GetValues(typeof(Formation));
                    if (idx < formations.Length) OnFormationChanged(formations[idx]);
                };

            // Style the play button teal
            var playBtn2 = GetNodeOrNull<Button>("BG/VBox/BottomBar/PlayButton");
            if (playBtn2 != null)
            {
                var sb2 = UITheme.MakeCardStyle(UITheme.AccentTeal, 10);
                sb2.ContentMarginTop    = 14f;
                sb2.ContentMarginBottom = 14f;
                playBtn2.AddThemeStyleboxOverride("normal",  sb2);
                playBtn2.AddThemeStyleboxOverride("hover",   UITheme.MakeCardStyle(UITheme.AccentTealDark, 10));
                playBtn2.AddThemeStyleboxOverride("pressed", UITheme.MakeCardStyle(UITheme.AccentTealDark, 10));
            }

            RefreshTopControls();
        }

        // ── Display methods ────────────────────────────────────────────────────

        /// <summary>Refreshes the UI to show <paramref name="tactic"/>.</summary>
        public void DisplayFormation(TacticData tactic)
        {
            if (tactic == null) return;
            currentTactic = tactic;

            if (pressingSlider != null)      pressingSlider.Value      = tactic.pressing;
            if (tempoSlider != null)         tempoSlider.Value         = tactic.tempo;
            if (widthSlider != null)         widthSlider.Value         = tactic.width;
            if (defensiveLineSlider != null) defensiveLineSlider.Value = tactic.defensiveLine;

            UpdatePitchVisualization();
        }

        /// <summary>Provides the squad used to annotate pitch nodes.</summary>
        public void SetSquad(List<PlayerData> squad)
        {
            currentSquad = squad ?? new List<PlayerData>();
        }

        /// <summary>Updates the floating prediction panel.</summary>
        public void ShowPrediction(MatchPrediction prediction)
        {
            if (prediction == null || predictionPanel == null) return;
            predictionPanel.Visible = true;

            if (predictionText != null)
                predictionText.Text =
                    $"Win: {prediction.winProbability.ToPercentageString()}  " +
                    $"Draw: {prediction.drawProbability.ToPercentageString()}  " +
                    $"Loss: {prediction.lossProbability.ToPercentageString()}\n" +
                    $"xG: {prediction.expectedGoalsHome:F2} – {prediction.expectedGoalsAway:F2}";
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        /// <summary>Called when the player selects a different formation.</summary>
        public void OnFormationChanged(Formation formation)
        {
            if (currentTactic == null) currentTactic = new TacticData();
            currentTactic.formation = formation;
            UpdatePitchVisualization();
        }

        /// <summary>Called by sliders when any tactical parameter changes.</summary>
        public void OnTacticParameterChanged(string param, int value)
        {
            if (currentTactic == null) return;

            switch (param)
            {
                case "pressing":      currentTactic.pressing      = value; break;
                case "tempo":         currentTactic.tempo         = value; break;
                case "width":         currentTactic.width         = value; break;
                case "defensiveLine": currentTactic.defensiveLine = value; break;
            }
        }

        /// <summary>Redraws player dots on the pitch canvas with rating and energy labels.</summary>
        public void UpdatePitchVisualization()
        {
            if (pitchContainer == null || playerDotPrefab == null || currentTactic == null) return;

            foreach (var child in pitchContainer.GetChildren())
                child.QueueFree();

            if (!ServiceLocator.TryGet<TacticalSystem>(out var system)) return;

            var positions = system.GetFormationPositions(currentTactic.formation);

            int assignmentCount = currentTactic.positionAssignments?.Count ?? 0;

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                var dot = playerDotPrefab.Instantiate<Control>();
                pitchContainer.AddChild(dot);

                dot.Position = new Vector2(
                    (pos.X / 100f) * pitchContainer.Size.X - pitchContainer.Size.X * 0.5f,
                    (pos.Y / 100f) * pitchContainer.Size.Y - pitchContainer.Size.Y * 0.5f);

                PlayerData assignedPlayer = null;
                if (assignmentCount > i && currentTactic.positionAssignments[i] != null)
                {
                    string pid = currentTactic.positionAssignments[i].playerId;
                    foreach (var p in currentSquad)
                        if (p.id == pid) { assignedPlayer = p; break; }
                }
                else if (currentSquad.Count > i)
                {
                    assignedPlayer = currentSquad[i];
                }

                if (assignedPlayer != null)
                {
                    var labels = dot.FindChildren("*", "Label", true, false);
                    if (labels.Count >= 1) (labels[0] as Label).Text = $"{assignedPlayer.overallRating}";
                    if (labels.Count >= 2) (labels[1] as Label).Text = $"⚡{assignedPlayer.energy}";

                    var img = dot.FindChild("ColorRect", true, false) as ColorRect;
                    if (img != null) img.Color = RarityToColor(assignedPlayer.rarity);
                }

                AddDragHandler(dot);
            }
        }

        // ── Drag-and-drop ──────────────────────────────────────────────────────

        private void AddDragHandler(Control dot)
        {
            // In Godot, drag repositioning is handled via GuiInput signal.
            dot.GuiInput += (InputEvent inputEvent) =>
            {
                if (inputEvent is InputEventMouseMotion mouseMotion &&
                    Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    dot.Position += mouseMotion.Relative;
                }
            };
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static Color RarityToColor(PlayerRarity rarity) => rarity switch
        {
            PlayerRarity.Silver       => UITheme.RaritySilver,
            PlayerRarity.Gold         => UITheme.RarityGold,
            PlayerRarity.Star         => UITheme.RarityStar,
            PlayerRarity.Legend       => UITheme.RarityLegend,
            PlayerRarity.AllTimeGreat => UITheme.RarityAllTimeGreat,
            _                         => UITheme.RarityNormal
        };

        public void RefreshTopControls()
        {
            if (currentTactic == null) return;
            if (formationLabel != null)
                formationLabel.Text = currentTactic.formation.ToString().Replace("F", "").Replace("_", "-");
            if (pressingLabel != null)
                pressingLabel.Text = currentTactic.pressing switch
                {
                    <= 30 => "Suave",
                    <= 60 => "Normal",
                    _     => "Alto"
                };
        }

        public void SetTeamRatings(int myOvr, int opponentOvr)
        {
            if (teamRatingLabel     != null) teamRatingLabel.Text     = myOvr.ToString();
            if (opponentRatingLabel != null) opponentRatingLabel.Text = opponentOvr.ToString();
        }

        private void OnBack() => FutbolJuego.Core.SceneNavigator.Instance?.GoToDashboard();
    }
}
