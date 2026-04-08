namespace FutbolJuego.Models
{
    /// <summary>Card rarity tier for a player, derived from their overall rating.</summary>
    public enum PlayerRarity
    {
        Normal,       // overall <= 69
        Silver,       // overall 70-79
        Gold,         // overall 80-84
        Star,         // overall 85-89
        Legend,       // overall 90-94
        AllTimeGreat  // overall >= 95
    }
}
