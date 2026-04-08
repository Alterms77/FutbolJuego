using System;
using System.Collections.Generic;
using Godot;
using FutbolJuego.Models;
using FutbolJuego.Systems;

namespace FutbolJuego.AI
{
    // ── Supporting types ───────────────────────────────────────────────────────

    /// <summary>Encapsulates all runtime context the AI needs to make decisions.</summary>
    public class GameContext
    {
        /// <summary>Current in-game date.</summary>
        public DateTime GameDate;
        /// <summary>Current season number.</summary>
        public int Season;
        /// <summary>Current league position of the AI team.</summary>
        public int LeaguePosition;
        /// <summary>Days remaining in the current transfer window.</summary>
        public int TransferWindowDaysRemaining;
    }

    /// <summary>A substitution decision made by the AI.</summary>
    [Serializable]
    public class SubstitutionDecision
    {
        /// <summary>Player being substituted off.</summary>
        public PlayerData playerOut;
        /// <summary>Player coming on.</summary>
        public PlayerData playerIn;
        /// <summary>Minute of the substitution.</summary>
        public int minute;
        /// <summary>Plain-language reason for the change.</summary>
        public string reason;
    }

    /// <summary>A transfer action decided by the AI (buy or release).</summary>
    [Serializable]
    public class TransferDecision
    {
        /// <summary>Player targeted for signing.</summary>
        public PlayerData targetPlayer;
        /// <summary>Proposed offer amount.</summary>
        public int offerAmount;
        /// <summary>Whether this is a buy (true) or release (false).</summary>
        public bool isBuy;
        /// <summary>Plain-language reason.</summary>
        public string reason;
    }

    // ── AIManager ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Orchestrates all AI decisions for a computer-controlled club:
    /// tactics, substitutions, training, and transfer market behaviour.
    /// </summary>
    public class AIManager
    {
        /// <summary>Difficulty level for this AI instance.</summary>
        public AIDifficulty Difficulty;

        private readonly TacticalSystem tacticalSystem;
        private readonly TransferMarketSystem transferSystem;
        private readonly System.Random rng = new System.Random();

        /// <summary>Creates an AI manager with the given difficulty.</summary>
        public AIManager(AIDifficulty difficulty, TacticalSystem tactics,
                         TransferMarketSystem transfers)
        {
            Difficulty     = difficulty;
            tacticalSystem = tactics;
            transferSystem = transfers;
        }

        // ── Difficulty ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the AI performance multiplier for the current difficulty.
        /// Easy = 0.80, Medium = 1.00, Hard = 1.15, Expert = 1.30.
        /// </summary>
        public float GetDifficultyMultiplier() => Difficulty switch
        {
            AIDifficulty.Easy   => 0.80f,
            AIDifficulty.Medium => 1.00f,
            AIDifficulty.Hard   => 1.15f,
            AIDifficulty.Expert => 1.30f,
            _                   => 1.00f
        };

        // ── Main AI turn ───────────────────────────────────────────────────────

        /// <summary>
        /// Processes a full AI turn: sets tactics, handles transfers if the
        /// window is open, and runs morale-management heuristics.
        /// </summary>
        public void ProcessAITurn(TeamData aiTeam, GameContext gameContext)
        {
            if (aiTeam == null) return;

            // Set best tactic
            aiTeam.currentTactic = DecideMatchTactic(aiTeam, null);

            // Transfer activity when window is open
            if (gameContext?.TransferWindowDaysRemaining > 0)
            {
                var freeAgents = transferSystem.GetAvailableFreePlayers(10);
                var decision   = DecideTransferAction(aiTeam, freeAgents);

                if (decision?.isBuy == true && decision.targetPlayer != null)
                {
                    GD.Print($"[AI] {aiTeam.name} attempting to sign {decision.targetPlayer.name}.");
                    // Actual transfer execution happens via TransferMarketSystem
                }
            }
        }

        // ── Tactical decision ──────────────────────────────────────────────────

        /// <summary>
        /// Generates the best tactic for <paramref name="aiTeam"/> considering
        /// <paramref name="opponent"/> (may be null for pre-match setup).
        /// </summary>
        public TacticData DecideMatchTactic(TeamData aiTeam, TeamData opponent)
        {
            var context = new MatchContext { Minute = 0, ScoreDifference = 0 };
            var tactic  = tacticalSystem.GenerateAITactic(aiTeam, opponent, context);

            // Apply difficulty: higher difficulty AI picks tactically smarter formations
            if (Difficulty >= AIDifficulty.Hard && opponent != null)
            {
                float mult = CalculateFormationCounter(aiTeam, opponent);
                tactic.pressing = (int)(tactic.pressing * mult);
            }

            return tactic;
        }

        // ── Substitution decision ──────────────────────────────────────────────

        /// <summary>
        /// Decides whether and what substitution to make based on the
        /// current match state.
        /// </summary>
        public SubstitutionDecision DecideSubstitution(TeamData aiTeam, MatchData match)
        {
            if (aiTeam?.squad == null || match == null) return null;

            var available  = aiTeam.GetAvailablePlayers();
            var fatigued   = available.FindAll(p => p.fatigue > 75);

            if (fatigued.Count == 0 || match.currentMinute < 60) return null;

            var playerOut  = fatigued[rng.Next(fatigued.Count)];
            var fresh      = available.Find(p => p.fatigue < 40 && p.id != playerOut.id);
            if (fresh == null) return null;

            return new SubstitutionDecision
            {
                playerOut = playerOut,
                playerIn  = fresh,
                minute    = match.currentMinute,
                reason    = $"{playerOut.name} showing signs of fatigue."
            };
        }

        // ── Transfer decision ──────────────────────────────────────────────────

        /// <summary>
        /// Analyses the squad and market to decide whether to make a transfer
        /// move.
        /// </summary>
        public TransferDecision DecideTransferAction(TeamData aiTeam,
                                                      List<PlayerData> market)
        {
            if (aiTeam?.finances == null || market == null) return null;

            // Find weakest position in squad
            float avgRating = aiTeam.GetAverageSquadRating();
            long budget     = aiTeam.finances.transferBudget;

            foreach (var candidate in market)
            {
                int candidateValue = transferSystem.CalculatePlayerValue(candidate);
                if (candidateValue > budget) continue;
                if (candidate.CalculateOverall() < avgRating) continue;

                int offer = (int)(candidateValue * (0.90f + (float)rng.NextDouble() * 0.10f));

                return new TransferDecision
                {
                    targetPlayer = candidate,
                    offerAmount  = offer,
                    isBuy        = true,
                    reason       = $"{candidate.name} improves squad quality."
                };
            }

            return null;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private float CalculateFormationCounter(TeamData aiTeam, TeamData opponent)
        {
            if (opponent?.currentTactic == null) return 1.0f;
            return tacticalSystem.CalculateFormationMatchup(
                aiTeam.currentTactic?.formation ?? Formation.F433,
                opponent.currentTactic.formation);
        }
    }

    /// <summary>AI opponent difficulty presets.</summary>
    public enum AIDifficulty { Easy, Medium, Hard, Expert }
}
