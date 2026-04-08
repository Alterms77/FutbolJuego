using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.Systems;
using FutbolJuego.Core;

namespace FutbolJuego.UI
{
    public partial class TrainingUI : Control
    {
        [ExportGroup("Options")]
        [Export] public Control optionsContainer;
        [Export] public PackedScene optionButtonPrefab;

        [ExportGroup("Fatigue")]
        [Export] public Control fatigueContainer;
        [Export] public PackedScene fatigueRowPrefab;

        [ExportGroup("Info")]
        [Export] public Label infoLabel;

        private TrainingFocus selectedFocus = TrainingFocus.Tactical;

        public override void _Ready() => ShowTrainingOptions();

        public void ShowTrainingOptions()
        {
            if (optionsContainer == null || optionButtonPrefab == null) return;
            foreach (var child in optionsContainer.GetChildren()) child.QueueFree();

            foreach (TrainingFocus focus in System.Enum.GetValues(typeof(TrainingFocus)))
            {
                var btn   = optionButtonPrefab.Instantiate<Control>();
                optionsContainer.AddChild(btn);
                var label = btn.FindChild("Label", true, false) as Label;
                if (label != null) label.Text = focus.ToString();

                var button = btn as Button ?? btn.FindChild("Button", true, false) as Button;
                if (button != null)
                {
                    var captured = focus;
                    button.Pressed += () => OnTrainingFocusSelected(captured);
                }
            }
        }

        public void ShowPlayerFatigue(List<PlayerData> players)
        {
            if (fatigueContainer == null || fatigueRowPrefab == null) return;
            foreach (var child in fatigueContainer.GetChildren()) child.QueueFree();

            foreach (var player in players)
            {
                var row   = fatigueRowPrefab.Instantiate<Control>();
                fatigueContainer.AddChild(row);
                var label = row.FindChild("Label", true, false) as Label;
                if (label != null)
                    label.Text = $"{player.name,-24}  Fatigue: {player.fatigue,3}/100  " +
                                 $"{(player.injuryDaysRemaining > 0 ? $"INJURED ({player.injuryDaysRemaining}d)" : "Fit")}";

                var slider = row.FindChild("ProgressBar", true, false) as ProgressBar;
                if (slider != null) slider.Value = player.fatigue / 100f;
            }
        }

        public void OnTrainingFocusSelected(TrainingFocus focus)
        {
            selectedFocus = focus;
            if (infoLabel != null)
                infoLabel.Text = $"Focus: {focus}  — affects {GetFocusDescription(focus)}";
        }

        public void OnStartTrainingButton()
        {
            // Wire this to the TrainingSystem via the game controller
        }

        private static string GetFocusDescription(TrainingFocus focus) => focus switch
        {
            TrainingFocus.Shooting  => "Finishing & Long shots",
            TrainingFocus.Passing   => "Short passing & Vision",
            TrainingFocus.Defense   => "Tackling & Positioning",
            TrainingFocus.Speed     => "Sprint speed & Acceleration",
            TrainingFocus.Physical  => "Strength & Stamina",
            TrainingFocus.Tactical  => "Decision-making & Intelligence",
            _                       => focus.ToString()
        };
    }
}
