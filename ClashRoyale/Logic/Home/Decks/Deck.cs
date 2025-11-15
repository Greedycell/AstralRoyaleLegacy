using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using ClashRoyale.Logic.Home.Decks.Items;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;
using Newtonsoft.Json;

namespace ClashRoyale.Logic.Home.Decks
{
    public class Deck : List<Card>
    {
        [JsonIgnore] public Home Home { get; set; }

        public void Initialize()
        {
            // what was inc thinking when making the original default deck... but atleast me and my contributors fixed it.

            var defaultDeckCards = new List<(int ClassId, int InstanceId)>
            {
                (26, 0),  // Knight
                (26, 1),  // Archers
                (26, 13), // Bomber
                (28, 1),  // Arrows
                (28, 0),  // Fireball
                (26, 3),  // Giant
            };

            // One random rare
            var rareCards = new List<(int ClassId, int InstanceId)>
            {
                (26, 18),  // Mini Pekka
                (26, 14),  // Musketeer
            };

            // One random epic
            var epicCards = new List<(int ClassId, int InstanceId)>
            {
                (26, 16), // Prince
                (26, 15), // Baby Dragon
                (26, 12), // Skeleton Army
                (26, 7), // Witch
            };

            var random = new Random();

            for (var i = 0; i < 6 && i < defaultDeckCards.Count; i++)
            {
                var (classId, instanceId) = defaultDeckCards[i];
                var card = new Card(classId, instanceId, false);
                Add(card);
                foreach (var deck in Home.Decks) deck[i] = card.GlobalId;
            }

            var selectedRare = rareCards[random.Next(rareCards.Count)];
            var rareCard = new Card(selectedRare.ClassId, selectedRare.InstanceId, false);
            Add(rareCard);
            foreach (var deck in Home.Decks) deck[6] = rareCard.GlobalId;

            var selectedEpic = epicCards[random.Next(epicCards.Count)];
            var epicCard = new Card(selectedEpic.ClassId, selectedEpic.InstanceId, false);
            Add(epicCard);
            foreach (var deck in Home.Decks) deck[7] = epicCard.GlobalId;
        }

        /// <summary>
        /// Add a card if we have it already in collection just add the ammount of material
        /// </summary>
        /// <param name="card"></param>
        public new void Add(Card card)
        {
            var index = FindIndex(c => c.ClassId == card.ClassId && c.InstanceId == card.InstanceId);

            if (index <= -1)
                base.Add(card);
            else
                this[index].Count += card.Count;
        }

        /// <summary>
        /// Encodes the whole collection
        /// </summary>
        /// <param name="packet"></param>
        public void Encode(IByteBuffer packet)
        {
            packet.WriteVInt(Home.Decks.Length); // DeckCount

            foreach (var deck in Home.Decks)
            {
                packet.WriteVInt(deck.Length);

                foreach (var globalId in deck)
                    packet.WriteVInt(globalId);
            }

            packet.WriteByte(255);

            foreach (var card in GetRange(0, 8))
                card.Encode(packet);

            packet.WriteVInt(Count - 8);

            foreach (var card in this.Skip(8))
                card.Encode(packet);

            packet.WriteVInt(Home.SelectedDeck); // CurrentSlot
        }

        /// <summary>
        /// Switch between 5 decks
        /// </summary>
        /// <param name="deckIndex"></param>
        public void SwitchDeck(int deckIndex)
        {
            if (deckIndex > 4) return;

            for (var i = 0; i < Home.Decks[deckIndex].Length; i++)
            {
                var card = Home.Decks[deckIndex][i];
                var newDeckCard = GetCard(card);
                var oldDeckCard = this[i];

                var newOldCardIndex = IndexOf(newDeckCard);

                this[newOldCardIndex] = oldDeckCard;
                this[i] = newDeckCard;
            }

            Home.SelectedDeck = deckIndex;
        }

        /// <summary>
        /// Encodes this deck for a battle
        /// </summary>
        /// <param name="packet"></param>
        public void EncodeAttack(IByteBuffer packet)
        {
            foreach (var card in GetRange(0, 8))
                card.EncodeAttack(packet);
        }

        /// <summary>
        /// Get a card by it's class and instance id
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public Card GetCard(int classId, int instanceId)
        {
            var index = FindIndex(c => c.ClassId == classId && c.InstanceId == instanceId);
            return index > -1 ? this[index] : null;
        }

        /// <summary>
        /// Get a card by it's globalId
        /// </summary>
        /// <param name="globalId"></param>
        /// <returns></returns>
        public Card GetCard(int globalId)
        {
            var index = FindIndex(c => c.GlobalId == globalId);
            return index > -1 ? this[index] : null;
        }

        /// <summary>
        /// Returns the card offset in the collection
        /// </summary>
        /// <param name="globalId"></param>
        /// <returns></returns>
        public int GetCardOffset(int globalId)
        {
            var index = FindIndex(c => c.GlobalId == globalId);
            return index;
        }

        /// <summary>
        /// Swap cards in deck
        /// </summary>
        /// <param name="cardOffset"></param>
        /// <param name="deckOffset"></param>
        public void SwapCard(int cardOffset, int deckOffset)
        {
            var currentDeck = Home.Decks[Home.SelectedDeck];
            currentDeck[deckOffset] = this[cardOffset + 8].GlobalId;

            var old = this[deckOffset];
            this[deckOffset] = this[cardOffset + 8];
            this[cardOffset + 8] = old;
        }

        /// <summary>
        /// Upgrade all cards if an upgrade is available and enough gold
        /// </summary>
        public void UpgradeAll()
        {
            foreach (var card in this) UpgradeCard(card);
        }

        /// <summary>
        /// Upgrade a card by it's class and instance id
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="instanceId"></param>
        /// <param name="force"></param>
        public void UpgradeCard(int classId, int instanceId, bool force = false)
        {
            var card = GetCard(classId, instanceId);

            if (card != null)
                UpgradeCard(card, force);
        }

        /// <summary>
        /// Upgrade a card and check if enough cards and gold are available to use or force an upgrade
        /// </summary>
        /// <param name="card"></param>
        /// <param name="force"></param>
        public void UpgradeCard(Card card, bool force = false)
        {
            var data = card.GetRarityData;
            if (data == null) return;

            if (card.Level >= data.UpgradeMaterialCount.Length - 1) return;

            if (!force)
            {
                var materialCount = data.UpgradeMaterialCount[card.Level];

                if (materialCount > card.Count) return;
                if (!Home.UseGold(data.UpgradeCost[card.Level])) return;

                card.Count -= materialCount;
            }

            Home.AddExpPoints(data.UpgradeExp[card.Level]);
            card.Level++;
        }

        /// <summary>
        /// When a card is new and a player taps on it the first time in it's collection
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="instanceId"></param>
        public void SawCard(int classId, int instanceId)
        {
            var card = GetCard(classId, instanceId);
            card.IsNew = false;
        }

                public Deck GenerateRandomDeck(bool rdm_deck, bool rdm_lvl)
        {
            Deck randomDeck = new Deck();
            if (rdm_deck == false)
                return Home.Deck;

            Random random = new Random();
            var chestArenas = Home.Arena.GetChestArenaNames(); // Get arenas for random cards

            for (int i = 0; i < 8; i++)
            {
                Card card = null;
                while (card == null)
                {
                    card = Cards.RandomByArena(Card.Rarity.Common, chestArenas) ??
                           Cards.RandomByArena(Card.Rarity.Rare, chestArenas) ??
                           Cards.RandomByArena(Card.Rarity.Epic, chestArenas) ??
                           Cards.RandomByArena(Card.Rarity.Legendary, chestArenas) ??
                           Cards.Random();
                }

                if (rdm_lvl == false)
                {
                    switch (card.CardRarity)
                    {
                        case Card.Rarity.Rare:
                            card.Level = 10;
                            break;
                        case Card.Rarity.Epic:
                            card.Level = 7;
                            break;
                        case Card.Rarity.Common:
                            card.Level = 12;
                            break;
                        case Card.Rarity.Legendary:
                            card.Level = 4;
                            break;
                    }
                }
                else
                {
                    switch (card.CardRarity)
                    {
                        case Card.Rarity.Rare:
                            card.Level = random.Next(1, 11);
                            break;
                        case Card.Rarity.Epic:
                            card.Level = random.Next(1, 8);
                            break;
                        case Card.Rarity.Common:
                            card.Level = random.Next(1, 13);
                            break;
                        case Card.Rarity.Legendary:
                            card.Level = random.Next(1, 5);
                            break;
                    }
                }

                randomDeck.Add(card);
            }

            return randomDeck;
        }

        /// <summary>
        /// Generate a random deck where all cards are set to a specific level,
        /// and only cards unlockable in the provided arenas are used.
        /// </summary>
        /// <param name="level">The level to set for all cards (1-based).</param>
        /// <param name="allowedArenas">A list of allowed arena names for card unlocks.</param>
        /// <returns>Deck</returns>
        public static Deck GenerateRandomDeckAtLevel(int level, List<string> allowedArenas)
        {
            if (level < 1) level = 1;
            if (level > 13) level = 13;

            Deck randomDeck = new Deck();
            Random random = new Random();

            var cards = Cards.GetAllCards()
                .Where(card =>
                {
                    var unlockArena = card.UnlockArenaName;
                    return unlockArena == null || allowedArenas.Contains(unlockArena);
                })
                .ToList();

            cards = cards.OrderBy(x => random.Next()).ToList();

            var selected = new HashSet<int>();
            int i = 0;
            while (randomDeck.Count < 8 && i < cards.Count)
            {
                var card = cards[i];

                int cardMaxLevel = GetMaxLevelForRarity(card.CardRarity);
                int effectiveLevel = Math.Min(level, cardMaxLevel);

                var cardCopy = new Card(card.ClassId, card.InstanceId, false);
                cardCopy.Level = effectiveLevel;

                if (!selected.Contains(card.GlobalId))
                {
                    randomDeck.Add(cardCopy);
                    selected.Add(card.GlobalId);
                }
                i++;
            }
            while (randomDeck.Count < 8 && cards.Count > 0)
            {
                var card = cards[random.Next(cards.Count)];
                int cardMaxLevel = GetMaxLevelForRarity(card.CardRarity);
                int effectiveLevel = Math.Min(level, cardMaxLevel);

                var cardCopy = new Card(card.ClassId, card.InstanceId, false);
                cardCopy.Level = effectiveLevel;
                randomDeck.Add(cardCopy);
            }
            return randomDeck;
        }

        private static int GetMaxLevelForRarity(Card.Rarity rarity)
        {
            switch (rarity)
            {
                case Card.Rarity.Common: return 12;
                case Card.Rarity.Rare: return 10;
                case Card.Rarity.Epic: return 7;
                case Card.Rarity.Legendary: return 4;
                default: return 1;
            }
        }
    }
}