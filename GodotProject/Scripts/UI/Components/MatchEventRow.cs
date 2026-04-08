using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    public partial class MatchEventRow : HBoxContainer
    {
        [ExportGroup("UI References")]
        [Export] public Label minuteText;
        [Export] public Label descriptionText;
        [Export] public ColorRect eventIcon;

        private static readonly Color GoalColor    = new Color(0.2f, 0.9f, 0.3f);
        private static readonly Color YellowCard   = new Color(1f,   0.9f, 0f  );
        private static readonly Color RedCard      = new Color(1f,   0.2f, 0.2f);
        private static readonly Color DefaultColor = Colors.White;

        public void Setup(MatchEvent evt)
        {
            if (evt == null) return;

            if (minuteText != null)      minuteText.Text      = $"{evt.minute}'";
            if (descriptionText != null) descriptionText.Text = evt.description;
            if (eventIcon != null)       eventIcon.Color      = GetColorForEventType(evt.type);
        }

        private static Color GetColorForEventType(MatchEventType type) => type switch
        {
            MatchEventType.Goal        => GoalColor,
            MatchEventType.PenaltyGoal => GoalColor,
            MatchEventType.YellowCard  => YellowCard,
            MatchEventType.RedCard     => RedCard,
            _                          => DefaultColor
        };
    }
}
