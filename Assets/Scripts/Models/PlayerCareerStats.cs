namespace FutbolJuego.Models
{
    /// <summary>
    /// Permanent, career-wide statistics accumulated by a player across every season
    /// they have been part of a managed squad.
    ///
    /// Unlike <see cref="PerformanceStats"/> (which is reset each season),
    /// <see cref="PlayerCareerStats"/> is never reset — values are additive and
    /// represent the player's full career record.
    ///
    /// Championship / trophy counters are incremented at the end of each season
    /// via <see cref="FutbolJuego.Systems.SeasonSystem.AdvanceSeason"/>.
    /// </summary>
    [System.Serializable]
    public class PlayerCareerStats
    {
        // ── Seasons ────────────────────────────────────────────────────────────

        /// <summary>Total seasons the player has appeared in the managed squad.</summary>
        public int seasonsPlayed;

        // ── Match activity ─────────────────────────────────────────────────────

        /// <summary>Total career matches played (all competitions).</summary>
        public int totalMatchesPlayed;
        /// <summary>Total career minutes on the pitch.</summary>
        public int totalMinutesPlayed;

        // ── Attacking / playmaking ──────────────────────────────────────────────

        /// <summary>Total career goals scored (all competitions).</summary>
        public int totalGoals;
        /// <summary>Total career assists.</summary>
        public int totalAssists;

        // ── Defending / goalkeeping ─────────────────────────────────────────────

        /// <summary>Total career clean sheets (GK and defenders).</summary>
        public int totalCleanSheets;
        /// <summary>Total career saves made (GK only).</summary>
        public int totalSavesMade;

        // ── Discipline ─────────────────────────────────────────────────────────

        /// <summary>Total career yellow cards received.</summary>
        public int totalYellowCards;
        /// <summary>Total career red cards received.</summary>
        public int totalRedCards;

        // ── Accolades ─────────────────────────────────────────────────────────

        /// <summary>Number of league title medals won.</summary>
        public int leagueTitles;
        /// <summary>Number of domestic cup winner medals won.</summary>
        public int cupTitles;
        /// <summary>Number of continental / international cup winner medals.</summary>
        public int continentalTitles;
        /// <summary>Man-of-the-match awards over the entire career.</summary>
        public int totalManOfTheMatchAwards;
        /// <summary>Season top-scorer awards (golden boot equivalent).</summary>
        public int goldenBootAwards;
        /// <summary>Season best-goalkeeper awards (golden glove equivalent).</summary>
        public int goldenGloveAwards;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Accumulates a completed season's <see cref="PerformanceStats"/> into
        /// this career record.  Called by
        /// <see cref="FutbolJuego.Systems.SeasonSystem.AdvanceSeason"/> at
        /// the end of every season.
        /// </summary>
        public void AccumulateSeason(PerformanceStats season)
        {
            if (season == null) return;

            seasonsPlayed++;
            totalMatchesPlayed    += season.matchesPlayed;
            totalMinutesPlayed    += season.minutesPlayed;
            totalGoals            += season.goals;
            totalAssists          += season.assists;
            totalCleanSheets      += season.cleanSheets;
            totalSavesMade        += season.savesMade;
            totalYellowCards      += season.yellowCards;
            totalRedCards         += season.redCards;
            totalManOfTheMatchAwards += season.manOfTheMatchAwards;
        }

        /// <summary>Returns a one-line career summary for tooltips and UI lists.</summary>
        public string ToSummaryString()
        {
            return
                $"{seasonsPlayed} temp  |  {totalMatchesPlayed} PJ  |  " +
                $"⚽ {totalGoals}  🎯 {totalAssists}  " +
                $"🏆 {leagueTitles + cupTitles + continentalTitles}  " +
                $"🟨 {totalYellowCards}  🟥 {totalRedCards}";
        }
    }
}
