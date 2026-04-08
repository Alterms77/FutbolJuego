using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using FutbolJuego.Models;

namespace FutbolJuego.Systems
{
    // ── Transfer offer model ───────────────────────────────────────────────────

    /// <summary>An offer submitted by a buyer team for a specific player.</summary>
    [Serializable]
    public class TransferOffer
    {
        /// <summary>Buying team identifier.</summary>
        public string buyerTeamId;
        /// <summary>Selling team identifier.</summary>
        public string sellerTeamId;
        /// <summary>Player being offered for.</summary>
        public PlayerData player;
        /// <summary>Bid amount.</summary>
        public long offerAmount;
        /// <summary>Whether the bid was accepted.</summary>
        public bool isAccepted;
    }

    /// <summary>
    /// Manages all transfer-market logic: player valuation, negotiation,
    /// free agents, and procedural player generation.
    /// </summary>
    public class TransferMarketSystem
    {
        private readonly System.Random rng = new System.Random();

        // ── Name databases ─────────────────────────────────────────────────────

        private static readonly string[] FirstNames =
        {
            "Alejandro", "Carlos", "Diego", "Eduardo", "Felipe", "Gabriel", "Héctor",
            "Ignacio", "Javier", "Kevin", "Luis", "Marco", "Nicolas", "Oscar", "Pablo",
            "Rafael", "Sergio", "Tomás", "Victor", "William", "Andres", "Bruno", "César",
            "David", "Emilio", "Fernando", "Gonzalo", "Hugo", "Ivan", "Juan",
            "Luca", "Matteo", "Nathan", "Oliver", "Patrick", "Quinton", "Rodrigo",
            "Stefan", "Takeshi", "Umberto", "Valentino", "Xavier", "Yusuf", "Zach",
            "Alexei", "Bogdan", "Cristian", "Damian", "Esteban", "Fabio",
            "James", "Harry", "Jack", "Thomas", "Ryan", "Marcus", "Jamal",
            "Mohammed", "Ibrahim", "Karim", "Sadio", "Ousmane", "Cheikh",
            "Hiroshi", "Kenji", "Naoya", "Shinji", "Park", "Son", "Lee"
        };

        private static readonly string[] LastNames =
        {
            "García", "Martínez", "López", "González", "Rodríguez", "Hernández",
            "Pérez", "Sánchez", "Ramírez", "Torres", "Flores", "Rivera", "Gómez",
            "Díaz", "Reyes", "Morales", "Jiménez", "Álvarez", "Romero", "Castro",
            "Silva", "Costa", "Santos", "Ferreira", "Oliveira", "Carvalho", "Andrade",
            "Mueller", "Schmidt", "Fischer", "Weber", "Meyer", "Wagner", "Becker",
            "Smith", "Jones", "Williams", "Brown", "Taylor", "Davies", "Wilson",
            "Diallo", "Traoré", "Koné", "Mbaye", "Camara", "Coulibaly", "Sissoko",
            "Nakamura", "Tanaka", "Yamamoto", "Suzuki", "Watanabe", "Ito",
            "Benzema", "Kanté", "Mbappé", "Dembélé", "Pogba", "Umtiti",
            "De Bruyne", "Courtois", "Lukaku", "Hazard", "Vertonghen",
            "Salah", "Mané", "Firmino", "Henderson", "Alexander-Arnold",
            "Rashford", "Saka", "Mount", "Rice", "Bellingham", "Foden",
            "Lewandowski", "Müller", "Neuer", "Kimmich", "Goretzka", "Gnabry",
            "Haaland", "Ødegaard", "Dybala", "Immobile", "Barella", "Verratti"
        };

        private static readonly string[] Nationalities =
        {
            "ES", "BR", "AR", "FR", "DE", "EN", "PT", "IT", "NL", "BE",
            "NG", "SN", "CI", "GH", "CM", "ML", "EG", "MA",
            "JP", "KR", "CN", "AU",
            "MX", "CO", "UY", "CL", "PE", "VE",
            "US", "CA",
            "NO", "SE", "DK", "HR", "RS", "PL", "CZ", "TR"
        };

        // ── Valuation ──────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates a player's market value using overall rating, age, potential,
        /// and an optional league-tier multiplier (higher leagues = higher values).
        ///
        /// Delegates to <see cref="PlayerRatingSystem.CalculateMarketValue"/> so
        /// the valuation formula is shared across both systems.
        /// </summary>
        /// <param name="player">The player to value.</param>
        /// <param name="leagueId">Optional league id — see <see cref="PlayerRatingSystem.LeagueValueMultipliers"/>.</param>
        public long CalculatePlayerValue(PlayerData player, string leagueId = null)
        {
            if (player == null) return 0;
            var ratingSystem = new PlayerRatingSystem();
            return ratingSystem.CalculateMarketValue(player, leagueId);
        }

        /// <summary>
        /// Attempts a transfer of <paramref name="player"/> from
        /// <paramref name="seller"/> to <paramref name="buyer"/> for
        /// <paramref name="offerAmount"/>.
        /// Returns <c>true</c> on success.
        /// </summary>
        public bool AttemptTransfer(TeamData buyer, TeamData seller,
                                    PlayerData player, long offerAmount)
        {
            if (buyer == null || seller == null || player == null) return false;

            long playerValue = CalculatePlayerValue(player);

            // Seller accepts if offer ≥ 90% of value (or they want to sell)
            bool sellerAccepts = offerAmount >= playerValue * 0.9f;
            if (!sellerAccepts)
            {
                GD.Print($"[Transfer] {seller.name} rejected offer of {offerAmount:N0} for {player.name} (value: {playerValue:N0})");
                return false;
            }

            // Buyer must be able to afford
            if (!buyer.finances.CanAfford(offerAmount))
            {
                GD.Print($"[Transfer] {buyer.name} cannot afford {offerAmount:N0} for {player.name}");
                return false;
            }

            // Squad size limit
            if (buyer.squad != null && buyer.squad.Count >= Utils.Constants.MaxSquadSize)
            {
                GD.Print($"[Transfer] {buyer.name} squad is full.");
                return false;
            }

            ExecuteTransfer(buyer, seller, player, offerAmount);
            return true;
        }

        /// <summary>
        /// Buys <paramref name="player"/> for the career's managed team, deducting the
        /// fee from <see cref="CareerData.inGameBalance"/>.
        /// Returns <c>true</c> on success.
        /// </summary>
        /// <param name="buyerTeam">The managed team making the purchase.</param>
        /// <param name="career">Active career (provides balance and leagueId).</param>
        /// <param name="player">Player to buy.</param>
        /// <param name="offerAmount">Fee offered (in the career currency unit).</param>
        public bool BuyPlayerForCareer(TeamData buyerTeam, CareerData career,
                                       PlayerData player, long offerAmount)
        {
            if (buyerTeam == null || career == null || player == null) return false;

            long value = CalculatePlayerValue(player, career.managedLeagueId);

            if (offerAmount < value * 0.9f)
            {
                GD.Print($"[Transfer] Offer {offerAmount:N0} too low for {player.name} (value {value:N0}).");
                return false;
            }

            if (career.inGameBalance < offerAmount)
            {
                GD.Print($"[Transfer] Insufficient career balance ({career.inGameBalance:N0}) for {player.name}.");
                return false;
            }

            if (buyerTeam.squad != null && buyerTeam.squad.Count >= Utils.Constants.MaxSquadSize)
            {
                GD.Print($"[Transfer] Squad is full — cannot buy {player.name}.");
                return false;
            }

            career.inGameBalance -= offerAmount;
            buyerTeam.squad      ??= new List<PlayerData>();
            buyerTeam.squad.Add(player);

            if (buyerTeam.finances != null)
            {
                buyerTeam.finances.AddTransaction(new FinanceTransaction
                {
                    date        = DateTime.UtcNow,
                    type        = FinanceTransactionType.Transfer,
                    amount      = -offerAmount,
                    description = $"Fichado: {player.name}"
                });
                buyerTeam.finances.transferBudget -= offerAmount;
            }

            PlayerRatingSystem.SyncRarity(player);
            GD.Print($"[Transfer] Bought {player.name} for {career.CurrencySymbol}{offerAmount:N0}. " +
                      $"Remaining balance: {career.FormattedBalance}.");
            return true;
        }

        /// <summary>
        /// Sells <paramref name="player"/> from the career's managed team, crediting
        /// 80% of the market value to <see cref="CareerData.inGameBalance"/>.
        /// Returns the amount received, or 0 if the sale failed.
        /// </summary>
        /// <param name="sellerTeam">The managed team selling the player.</param>
        /// <param name="career">Active career (provides balance and leagueId).</param>
        /// <param name="player">Player to sell.</param>
        public long SellPlayerFromCareer(TeamData sellerTeam, CareerData career, PlayerData player)
        {
            if (sellerTeam == null || career == null || player == null) return 0;
            if (sellerTeam.squad == null || !sellerTeam.squad.Contains(player))
            {
                GD.PushWarning($"[Transfer] {player.name} is not in squad.");
                return 0;
            }

            long value    = CalculatePlayerValue(player, career.managedLeagueId);
            long proceeds = (long)(value * 0.80f); // 80% fee recovery

            sellerTeam.squad.Remove(player);
            career.inGameBalance += proceeds;

            if (sellerTeam.finances != null)
            {
                sellerTeam.finances.AddTransaction(new FinanceTransaction
                {
                    date        = DateTime.UtcNow,
                    type        = FinanceTransactionType.Transfer,
                    amount      = proceeds,
                    description = $"Vendido: {player.name}"
                });
                sellerTeam.finances.transferBudget += proceeds;
            }

            GD.Print($"[Transfer] Sold {player.name} for {career.CurrencySymbol}{proceeds:N0}. " +
                      $"New balance: {career.FormattedBalance}.");
            return proceeds;
        }
        public PlayerData GenerateRandomPlayer(int minOverall, int maxOverall,
                                               PlayerPosition? position = null)
        {
            int target   = rng.Next(minOverall, maxOverall + 1);
            var pos      = position ?? RandomPosition();
            return GenerateProceduralPlayer(target, pos);
        }

        /// <summary>
        /// Returns a list of free agents (players without clubs) for the
        /// transfer market.
        /// </summary>
        public List<PlayerData> GetAvailableFreePlayers(int count = 20)
        {
            var players = new List<PlayerData>();
            var positions = (PlayerPosition[])Enum.GetValues(typeof(PlayerPosition));

            for (int i = 0; i < count; i++)
            {
                int overall = rng.Next(55, 76);
                var pos     = positions[rng.Next(positions.Length)];
                var player  = GenerateProceduralPlayer(overall, pos);
                player.weeklyWage = CalculateWage(overall);
                players.Add(player);
            }
            return players;
        }

        /// <summary>
        /// Simulates AI scouting — returns a list of transfer interests an AI
        /// team might pursue.
        /// </summary>
        public List<TransferOffer> GenerateAITransferInterests(TeamData aiTeam,
                                                                List<PlayerData> available)
        {
            var offers = new List<TransferOffer>();
            if (aiTeam?.finances == null || available == null) return offers;

            int budget = (int)aiTeam.finances.transferBudget;

            foreach (var player in available.Take(5))
            {
                long value = CalculatePlayerValue(player);
                if (value > budget) continue;

                long offer = (long)(value * (0.90f + (float)rng.NextDouble() * 0.15f));
                offers.Add(new TransferOffer
                {
                    buyerTeamId  = aiTeam.id,
                    sellerTeamId = null,
                    player       = player,
                    offerAmount  = offer
                });
            }
            return offers;
        }

        /// <summary>
        /// Generates a fully-populated procedural player to the given
        /// overall target for the specified position.
        /// </summary>
        public PlayerData GenerateProceduralPlayer(int targetOverall, PlayerPosition position)
        {
            targetOverall = Mathf.Clamp(targetOverall, 40, 99);

            string firstName  = FirstNames[rng.Next(FirstNames.Length)];
            string lastName   = LastNames[rng.Next(LastNames.Length)];
            string nationality = Nationalities[rng.Next(Nationalities.Length)];
            int age           = rng.Next(17, 37);

            var attributes = GenerateAttributes(targetOverall, position);
            int potential  = Mathf.Min(99, targetOverall + rng.Next(0, 15));
            if (age < 22) potential = Mathf.Min(99, potential + rng.Next(5, 15));

            var player = new PlayerData
            {
                id              = Guid.NewGuid().ToString(),
                name            = $"{firstName} {lastName}",
                age             = age,
                nationality     = nationality,
                position        = position,
                potential       = potential,
                consistency     = rng.Next(50, 95),
                injuryProneness = rng.Next(10, 60),
                weeklyWage      = CalculateWage(targetOverall),
                contractExpiry  = DateTime.UtcNow.AddYears(rng.Next(1, 5)),
                morale          = 50,
                fatigue         = 0,
                isAvailable     = true,
                attributes      = attributes
            };

            player.marketValue = CalculatePlayerValue(player);
            player.CalculateOverall();
            return player;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private PlayerAttributes GenerateAttributes(int targetOverall, PlayerPosition position)
        {
            var a = new PlayerAttributes();
            int variance = 10;

            int Stat(float weight) =>
                Mathf.Clamp(Mathf.RoundToInt(targetOverall * weight + rng.Next(-variance, variance)), 30, 99);

            switch (position)
            {
                case PlayerPosition.GK:
                    a.goalkeeping  = Stat(1.10f);
                    a.physical     = Stat(0.90f);
                    a.intelligence = Stat(0.85f);
                    a.passing      = Stat(0.75f);
                    a.speed        = Stat(0.60f);
                    a.shooting     = Stat(0.30f);
                    a.defense      = Stat(0.50f);
                    break;
                case PlayerPosition.CB:
                    a.defense      = Stat(1.10f);
                    a.physical     = Stat(1.00f);
                    a.intelligence = Stat(0.90f);
                    a.passing      = Stat(0.80f);
                    a.speed        = Stat(0.75f);
                    a.shooting     = Stat(0.50f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    a.defense      = Stat(1.00f);
                    a.speed        = Stat(1.00f);
                    a.physical     = Stat(0.90f);
                    a.passing      = Stat(0.90f);
                    a.intelligence = Stat(0.85f);
                    a.shooting     = Stat(0.60f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.CDM:
                    a.defense      = Stat(1.00f);
                    a.passing      = Stat(1.00f);
                    a.intelligence = Stat(0.95f);
                    a.physical     = Stat(0.90f);
                    a.speed        = Stat(0.80f);
                    a.shooting     = Stat(0.65f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.CM:
                    a.passing      = Stat(1.05f);
                    a.intelligence = Stat(1.00f);
                    a.physical     = Stat(0.90f);
                    a.defense      = Stat(0.85f);
                    a.speed        = Stat(0.85f);
                    a.shooting     = Stat(0.80f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.CAM:
                    a.passing      = Stat(1.05f);
                    a.intelligence = Stat(1.05f);
                    a.shooting     = Stat(1.00f);
                    a.speed        = Stat(0.85f);
                    a.physical     = Stat(0.75f);
                    a.defense      = Stat(0.55f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                    a.speed        = Stat(1.10f);
                    a.shooting     = Stat(1.00f);
                    a.passing      = Stat(0.95f);
                    a.intelligence = Stat(0.90f);
                    a.physical     = Stat(0.75f);
                    a.defense      = Stat(0.55f);
                    a.goalkeeping  = 20;
                    break;
                case PlayerPosition.ST:
                case PlayerPosition.CF:
                    a.shooting     = Stat(1.15f);
                    a.physical     = Stat(1.00f);
                    a.speed        = Stat(0.95f);
                    a.intelligence = Stat(0.90f);
                    a.passing      = Stat(0.75f);
                    a.defense      = Stat(0.45f);
                    a.goalkeeping  = 20;
                    break;
            }

            a.Clamp();
            return a;
        }

        private int CalculateWage(int overall)
        {
            // Rough wage curve: OVR 60 ≈ £5 000 /wk, OVR 85 ≈ £70 000 /wk
            return Mathf.RoundToInt(500f * Mathf.Pow(1.10f, overall - 55));
        }

        private PlayerPosition RandomPosition()
        {
            var positions = (PlayerPosition[])Enum.GetValues(typeof(PlayerPosition));
            return positions[rng.Next(positions.Length)];
        }

        private static void ExecuteTransfer(TeamData buyer, TeamData seller,
                                            PlayerData player, long fee)
        {
            seller.squad?.Remove(player);
            buyer.squad ??= new List<PlayerData>();
            buyer.squad.Add(player);

            buyer.finances.AddTransaction(new FinanceTransaction
            {
                date        = DateTime.UtcNow,
                type        = FinanceTransactionType.Transfer,
                amount      = -fee,
                description = $"Signed {player.name} from {seller.name}"
            });

            seller.finances.AddTransaction(new FinanceTransaction
            {
                date        = DateTime.UtcNow,
                type        = FinanceTransactionType.Transfer,
                amount      = fee,
                description = $"Sold {player.name} to {buyer.name}"
            });

            buyer.finances.transferBudget  -= fee;
            seller.finances.transferBudget += (long)(fee * 0.8f); // 80% reinvestable

            PlayerRatingSystem.SyncRarity(player);
            GD.Print($"[Transfer] {player.name} moved: {seller.name} → {buyer.name} for {fee:N0}");
        }
    }
}
