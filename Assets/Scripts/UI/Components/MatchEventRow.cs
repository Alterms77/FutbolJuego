using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    /// <summary>
    /// A single row in the minute-by-minute match event feed.
    /// Assign this component to the event-row prefab and call
    /// <see cref="Setup"/> when instantiating the row.
    /// </summary>
    public class MatchEventRow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI minuteText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image eventIcon;

        // ── Event type icon colours ────────────────────────────────────────────

        private static readonly Color GoalColor   = new Color(0.2f, 0.9f, 0.3f);
        private static readonly Color YellowCard  = new Color(1f,   0.9f, 0f  );
        private static readonly Color RedCard     = new Color(1f,   0.2f, 0.2f);
        private static readonly Color DefaultColor = Color.white;

        // ── Setup ──────────────────────────────────────────────────────────────

        /// <summary>Populates the row from a <see cref="MatchEvent"/>.</summary>
        public void Setup(MatchEvent evt)
        {
            if (evt == null) return;

            if (minuteText)      minuteText.text      = $"{evt.minute}'";
            if (descriptionText) descriptionText.text = evt.description;

            if (eventIcon)
                eventIcon.color = GetColorForEventType(evt.type);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static Color GetColorForEventType(MatchEventType type) => type switch
        {
            MatchEventType.Goal         => GoalColor,
            MatchEventType.YellowCard   => YellowCard,
            MatchEventType.RedCard      => RedCard,
            _                           => DefaultColor
        };
    }
}
