using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    /// <summary>Discrete events that affect player or team morale.</summary>
    public enum MoraleEvent
    {
        Win,
        Loss,
        Draw,
        BigWin,
        HeavyLoss,
        PlayerSold,
        ManagerPraise,
        ManagerCriticism,
        PromotionRelegation,
        TrophyWon
    }

    /// <summary>
    /// Updates player and team morale in response to match results and
    /// manager interactions.
    /// </summary>
    public class MoraleSystem
    {
        // ── Match result morale updates ────────────────────────────────────────

        /// <summary>
        /// Adjusts team and player morale after a completed match.
        /// </summary>
        public void UpdateMoraleAfterMatch(TeamData team, MatchData match)
        {
            if (team == null || match == null) return;

            bool isHome      = match.homeTeamId == team.id;
            int teamScore    = isHome ? match.homeScore : match.awayScore;
            int oppScore     = isHome ? match.awayScore : match.homeScore;
            int diff         = teamScore - oppScore;

            MoraleEvent evt;
            int magnitude;

            if (diff >= 3)       { evt = MoraleEvent.BigWin;    magnitude = 15; }
            else if (diff > 0)   { evt = MoraleEvent.Win;       magnitude = 8;  }
            else if (diff == 0)  { evt = MoraleEvent.Draw;      magnitude = 2;  }
            else if (diff >= -2) { evt = MoraleEvent.Loss;      magnitude = 7;  }
            else                 { evt = MoraleEvent.HeavyLoss; magnitude = 14; }

            UpdateTeamMorale(team, evt, magnitude);

            foreach (var player in team.squad ?? new System.Collections.Generic.List<PlayerData>())
                UpdatePlayerMorale(player, evt, magnitude / 2);
        }

        // ── Individual updates ─────────────────────────────────────────────────

        /// <summary>
        /// Adjusts a single player's morale by <paramref name="magnitude"/>
        /// (positive for good events, negative for bad).
        /// </summary>
        public void UpdatePlayerMorale(PlayerData player, MoraleEvent moraleEvent, int magnitude)
        {
            if (player == null) return;
            int delta = GetDelta(moraleEvent, magnitude);
            player.morale = Mathf.Clamp(player.morale + delta, 0, 100);
        }

        /// <summary>
        /// Adjusts the team-wide morale by <paramref name="magnitude"/>.
        /// </summary>
        public void UpdateTeamMorale(TeamData team, MoraleEvent moraleEvent, int magnitude)
        {
            if (team == null) return;
            int delta = GetDelta(moraleEvent, magnitude);
            team.morale = Mathf.Clamp(team.morale + delta, 0, 100);
        }

        /// <summary>
        /// Returns a match performance multiplier based on morale.
        /// Range: 0.85 (morale 0) – 1.15 (morale 100).
        /// </summary>
        public float GetMoraleMatchModifier(int morale)
        {
            return 0.85f + (morale / 100f) * 0.30f;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static int GetDelta(MoraleEvent moraleEvent, int magnitude) => moraleEvent switch
        {
            MoraleEvent.Win               =>  magnitude,
            MoraleEvent.BigWin            =>  magnitude,
            MoraleEvent.TrophyWon         =>  magnitude + 10,
            MoraleEvent.ManagerPraise     =>  magnitude,
            MoraleEvent.Draw              =>  (int)(magnitude * 0.3f),
            MoraleEvent.Loss              => -magnitude,
            MoraleEvent.HeavyLoss         => -magnitude,
            MoraleEvent.PlayerSold        => -magnitude / 2,
            MoraleEvent.ManagerCriticism  => -magnitude,
            MoraleEvent.PromotionRelegation => -magnitude + 5,
            _                             => 0
        };
    }
}
