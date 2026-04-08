using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FutbolJuego.Models;

namespace FutbolJuego.UI.Components
{
    /// <summary>
    /// Reusable player card component.  Assign the serialised fields in the
    /// prefab inspector, then call <see cref="Setup"/> at runtime.
    /// </summary>
    public class PlayerCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public TextMeshProUGUI playerNameText;
        [SerializeField] public TextMeshProUGUI positionText;
        [SerializeField] public TextMeshProUGUI ratingText;
        [SerializeField] public TextMeshProUGUI ageText;
        [SerializeField] public TextMeshProUGUI nationalityText;
        [SerializeField] public TextMeshProUGUI valueText;
        [SerializeField] public Image rarityBorder;
        [SerializeField] public Slider energyBar;
        [SerializeField] public Image avatarImage;

        // ── Setup ──────────────────────────────────────────────────────────────

        /// <summary>Populates all UI elements from <paramref name="player"/>.</summary>
        public void Setup(PlayerData player)
        {
            if (player == null) return;

            if (playerNameText)  playerNameText.text  = player.name;
            if (positionText)    positionText.text     = player.position.ToString();
            if (ratingText)      ratingText.text       = player.overallRating.ToString();
            if (ageText)         ageText.text          = $"{player.age} yrs";
            if (nationalityText) nationalityText.text  = player.nationality;
            if (valueText)       valueText.text        = FormatValue(player.marketValue);

            if (energyBar)
                energyBar.value = Mathf.Clamp01(player.energy / 100f);

            if (rarityBorder)
                rarityBorder.color = RarityToColor(player.rarity);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string FormatValue(long value)
        {
            if (value >= 1_000_000) return $"€{value / 1_000_000f:F1}M";
            if (value >= 1_000)     return $"€{value / 1_000f:F0}K";
            return $"€{value}";
        }

        private static Color RarityToColor(PlayerRarity rarity) => rarity switch
        {
            PlayerRarity.Silver => UITheme.RaritySilver,
            PlayerRarity.Gold   => UITheme.RarityGold,
            PlayerRarity.Star   => UITheme.RarityStar,
            PlayerRarity.Legend       => UITheme.RarityLegend,
            PlayerRarity.AllTimeGreat => UITheme.RarityAllTimeGreat,
            _                         => UITheme.RarityNormal
        };
    }
}
