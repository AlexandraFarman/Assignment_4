using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib;

namespace GameCardLib
{
    public class GameManager
    {
        public List<Player> Players { get; set; }
        public Deck Deck { get; set; }
        public List<Card> UsedCards { get; set; }
        public GameState State { get; set; }

        public GameManager()
        {
            State = GameState.NotStarted;
        }

        public (Player dealer, List<Player> players) StartGame(int nbrOfPlayers, int nbrOfDecks, Action toFewPlayers, Action toFewDecks)
        {
            if (nbrOfPlayers < 1)
            {
                toFewPlayers();
                return default;
            }
            if (nbrOfDecks < 1)
            {
                toFewDecks();
                return default;
            }

            Players = new List<Player>();
            for (int i = 0; i <= nbrOfPlayers; i++)
            {
                if (i == 0)
                {
                    Players.Add(new Player("Dealer", i));
                }
                else
                {
                    Players.Add(new Player($"Player {i}", i));
                }
            }

            Deck = new Deck(nbrOfDecks);

            
            Players.ForEach(p =>
            {
                // Give dealer 2 cards, one faced down.
                if (p.PlayerId == 0)
                {
                    p.Hand.AddCard(Deck.Pop());
                    Card c = Deck.Pop();
                    c.FaceUp = false;
                    p.Hand.AddCard(c);
                }
                // Give all players 2 cards.
                else
                {
                    p.Hand.AddCard(Deck.Pop());
                    p.Hand.AddCard(Deck.Pop());
                    if (p.Hand.Score > 21) p.State = PlayerState.IsThick;
                }
            });


            State = GameState.Ongoing;
            List<Player> players = Players.Where(p => p.PlayerId != 0).ToList();
            Player dealer = Players.SingleOrDefault(p => p.PlayerId == 0);
            return (dealer, players);
        }


        public bool Shuffle(Action gameNotStarted)
        {
            if (State != GameState.Ongoing)
            {
                gameNotStarted();
                return false;
            }

            Deck.ShuffleDeck();
            return true;
        }

        public bool Hit(int playerId, Action gameNotStarted, Action lessThanQuarterLeft, Action playerIsThick)
        {
            if (State != GameState.Ongoing)
            {
                gameNotStarted();
                return false;
            }

            Player player = Players.Single(p => p.PlayerId == playerId);
            Card newCard = Deck.Pop();
            if (newCard == null)
            {
                Deck.AddUsedCardsandShuffle(UsedCards);
                UsedCards = new List<Card>();
                newCard = Deck.Pop();
            }
            player.Hand.AddCard(newCard);
            player.State = PlayerState.HasPlayedRound;
            if (player.Hand.Score > 21)
            {
                player.State = PlayerState.IsThick;
                playerIsThick();
            }

            if (Deck.LessThanQuarterLeft())
            {
                lessThanQuarterLeft();
            }

            return true;
        }

        public bool Stand(int playerId, Action gameNotStarted)
        {
            if (State != GameState.Ongoing)
            {
                gameNotStarted();
                return false;
            }

            Player player = Players.Single(p => p.PlayerId == playerId);
            player.State = PlayerState.IsStanding;
            return true;
        }

        public Player ContinueRound(Action<List<Player>> showWinner)
        {
            Player player = Players.SingleOrDefault(p => p.State == PlayerState.Ready && p.PlayerId != 0);
            if (player == null)
            {
                Players.ForEach(p =>
                {
                    if (p.State == PlayerState.HasPlayedRound && p.PlayerId != 0)
                        p.State = PlayerState.Ready;
                });
                player = Players.SingleOrDefault(p => p.State == PlayerState.Ready && p.PlayerId != 0);
            }

            // All players have state IsStanding or IsThick. Time for dealer to act.
            if(player == null)
            {
                PlayDealer();
                List<Player> winners = GetWinner();
                showWinner(winners);
                EndGame();
                return null;
            }

            return player;
        }

        private void PlayDealer()
        {
            Player dealer = Players.FirstOrDefault(p => p.PlayerId == 0);

            // turn the second card, that is faced down.
            dealer.Hand.Cards.Single(c => !c.FaceUp).FaceUp = true;

            while(dealer.Hand.Score < 17)
            {
                Card newCard = Deck.Pop();
                if (newCard == null)
                {
                    Deck.AddUsedCardsandShuffle(UsedCards);
                    UsedCards = new List<Card>();
                    newCard = Deck.Pop();
                }
                dealer.Hand.AddCard(newCard);
            }

            if (dealer.Hand.Score > 21) dealer.State = PlayerState.IsThick;
        }

        public List<Player> GetWinner()
        {
            Player dealer = Players.FirstOrDefault(p => p.PlayerId == 0);
            List<Player> winners = new List<Player>();

            if(dealer.State == PlayerState.IsThick)
            {
                winners.AddRange(Players.Where(p => p.State != PlayerState.IsThick));
            }
            else
            {
                int dealerScore = dealer.Hand.Score;
                winners.AddRange(Players.Where(p => p.State != PlayerState.IsThick
                && p.Hand.Score > dealerScore));

                if(winners.Count < 1)
                {
                    winners.AddRange(Players.Where(p => p.Hand.Score == dealerScore));
                }
            }

            return winners;
        }

        public void EndGame()
        {
            State = GameState.Finished;
        }
    }
}
