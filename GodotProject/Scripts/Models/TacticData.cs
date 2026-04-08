using System;
using System.Collections.Generic;
using Godot;

namespace FutbolJuego.Models
{
    // ── Enumerations ───────────────────────────────────────────────────────────

    /// <summary>Supported match formations.</summary>
    public enum Formation { F433, F442, F352, F4231, F532, F4141, F343 }

    /// <summary>High-level team play style.</summary>
    public enum PlayStyle
    {
        Possession,
        CounterAttack,
        Direct,
        HighPress,
        ParkTheBus
    }

    // ── TacticData ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Complete tactical setup used for both match simulation and visual
    /// pitch display.
    /// </summary>
    [Serializable]
    public class TacticData
    {
        /// <summary>Selected formation shape.</summary>
        public Formation formation = Formation.F433;
        /// <summary>Overall play style instruction.</summary>
        public PlayStyle playStyle = PlayStyle.Possession;

        /// <summary>Defensive pressing intensity (0-100).</summary>
        public int pressing = 50;
        /// <summary>Passing tempo / speed of play (0-100).</summary>
        public int tempo = 50;
        /// <summary>Width of attacking shape (0-100).</summary>
        public int width = 50;
        /// <summary>Defensive line height (0-100; 100 = very high).</summary>
        public int defensiveLine = 50;

        /// <summary>Player ID of the designated captain.</summary>
        public string captainId;
        /// <summary>Player ID responsible for taking free kicks.</summary>
        public string freekickTakerId;
        /// <summary>Player ID responsible for taking penalties.</summary>
        public string penaltyTakerId;
        /// <summary>Player ID responsible for taking corners.</summary>
        public string cornerTakerId;

        /// <summary>Explicit position assignments for each outfield slot.</summary>
        public List<PlayerPositionAssignment> positionAssignments = new List<PlayerPositionAssignment>();

        /// <summary>Returns a deep copy of this tactic.</summary>
        public TacticData Clone()
        {
            var copy = new TacticData
            {
                formation       = formation,
                playStyle       = playStyle,
                pressing        = pressing,
                tempo           = tempo,
                width           = width,
                defensiveLine   = defensiveLine,
                captainId       = captainId,
                freekickTakerId = freekickTakerId,
                penaltyTakerId  = penaltyTakerId,
                cornerTakerId   = cornerTakerId,
                positionAssignments = new List<PlayerPositionAssignment>()
            };

            foreach (var a in positionAssignments)
                copy.positionAssignments.Add(new PlayerPositionAssignment
                {
                    playerId        = a.playerId,
                    position        = a.position,
                    pitchCoordinate = a.pitchCoordinate
                });

            return copy;
        }
    }

    // ── PlayerPositionAssignment ───────────────────────────────────────────────

    /// <summary>
    /// Maps a player to a specific slot and grid coordinate on the pitch.
    /// </summary>
    [Serializable]
    public class PlayerPositionAssignment
    {
        /// <summary>ID of the assigned player.</summary>
        public string playerId;
        /// <summary>Role / position for this slot.</summary>
        public PlayerPosition position;
        /// <summary>
        /// Position on a 0-100 × 0-100 pitch grid.
        /// X = 0 is the team's own goal line, X = 100 is the opponent's.
        /// Y = 0 is the left touchline, Y = 100 is the right.
        /// </summary>
        public Vector2 pitchCoordinate;
    }
}
