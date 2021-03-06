﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Casino.TwentyOne
{
    public class TwentyOne : Game, IWalkAway
    {
        public TwentyOneDealer Dealer { get; set; }
        //Implementing the abstract method "play" from the Game class.
        public override void Play()
        {
            Dealer = new TwentyOneDealer();
            foreach (Player player in Players)
            {
                player.Hand = new List<Card>();
                player.Stay = false;
            }
            Dealer.Hand = new List<Card>();
            Dealer.Stay = false;
            Dealer.Deck = new Deck();
            Dealer.Deck.Shuffle(3);
            Console.WriteLine("\nThe game is 21 and you have a balance of {0}.", Players.First().Balance);            
            foreach (Player player in Players)
            {
                bool validAnswer = false;
                int bet = 0;
                while (!validAnswer)
                {
                    Console.WriteLine("Please place your bets!"); 
                    validAnswer = int.TryParse(Console.ReadLine(), out bet);
                    if (!validAnswer) { Console.WriteLine("Please enter digits only, no decimals."); }
                }
                if (bet <= 0)
                {
                    throw new FraudException("Security! Remove this person immediately for attempting to defraud this casino!");
                }
                bool successfullyBet = player.Bet(bet);
                if (!successfullyBet)
                {
                    return;
                }
                Bets[player] = bet;
            }
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine("\nDealing...");
                foreach (Player player in Players)
                {
                    Console.Write("{0}: ", player.Name);
                    Dealer.Deal(player.Hand);
                    if (i == 1)
                    {
                        bool blackJack = TwentyOneRules.CheckForBlackJack(player.Hand);
                        if (blackJack)
                        {
                            //Error with this balance
                            //Now this balance works
                            player.Balance += Convert.ToInt32((Bets[player] * 1.5) + Bets[player]);
                            Console.WriteLine("BlackJack! {0} wins {1}. Your balance is {2}.", player.Name, Bets[player], player.Balance);
                            return;
                        }
                    }
                }
                Console.Write("Dealer: ");
                Dealer.Deal(Dealer.Hand);
                if (i == 1)
                {
                    bool blackJack = TwentyOneRules.CheckForBlackJack(Dealer.Hand);
                    if (blackJack)
                    {
                        foreach (KeyValuePair<Player,int> entry in Bets)
                        {
                            Dealer.Balance += entry.Value;
                        }
                        Console.WriteLine("Dealer has BlackJack! Everyone loses. Your balance is {0}", Players.First().Balance);
                        return;
                    }
                }
            }
            foreach (Player player in Players)
            {
                while (!player.Stay)
                {
                    Console.WriteLine("\nYour cards are: ");
                    foreach (Card card in player.Hand)
                    {
                        Console.WriteLine("{0}. ", card.ToString());
                    }
                    foreach (int handPossibilities in TwentyOneRules.GetAllPossibleHandValues(player.Hand))
                    {
                        Console.WriteLine("\nYour hand's value is: {0}", handPossibilities);
                    }
                    foreach (int handPossibilities in TwentyOneRules.GetAllPossibleHandValues(Dealer.Hand))
                    {
                        Console.WriteLine("The dealer's hand is worth: {0}", handPossibilities);
                    }
                    Console.WriteLine("\nHit or stay?");
                    string answer = Console.ReadLine().ToLower();
                    if (answer == "stay")
                    {
                        player.Stay = true;
                        break;
                    }
                    else if (answer =="hit")
                    {
                        Dealer.Deal(player.Hand);
                    }
                    else
                    {
                        Console.WriteLine("\nI didn't understand. Do you want to hit or stay?");
                    }
                    bool busted = TwentyOneRules.isBusted(player.Hand);
                    if (busted)
                    {
                        Dealer.Balance += Bets[player];
                        Console.WriteLine("\n{0} busted! You lost your bet of {1}. Your balance is now {2}.", player.Name, Bets[player], player.Balance);
                        Console.WriteLine("Do you want to play again?");
                        answer = Console.ReadLine().ToLower();
                        if (answer == "yes" || answer == "ok" || answer == "okay" || answer == "sure" || answer == "yeah" || answer == "y" || answer == "yep" || answer == "yup" || answer == "ya")
                        {
                            player.isActivelyPlaying = true;
                            return;
                        }
                        else
                        {
                            player.isActivelyPlaying = false;
                            return;
                        }
                    }
                }
            }
            Dealer.isBusted = TwentyOneRules.isBusted(Dealer.Hand);
            Dealer.Stay = TwentyOneRules.ShouldDealerStay(Dealer.Hand);
            while (!Dealer.Stay && !Dealer.isBusted)
            {
                Console.WriteLine("\nDealer is hitting...");
                Dealer.Deal(Dealer.Hand);
                Dealer.isBusted = TwentyOneRules.isBusted(Dealer.Hand);
                Dealer.Stay = TwentyOneRules.ShouldDealerStay(Dealer.Hand);
            }
            if (Dealer.Stay)
            {
                Console.WriteLine("\nDealer is staying.");
                foreach (int handPossibilities in TwentyOneRules.GetAllPossibleHandValues(Dealer.Hand))
                {
                    Console.WriteLine("The dealer's hand is worth: {0}", handPossibilities);
                }
            }
            if (Dealer.isBusted)
            {
                Console.WriteLine("Dealer busted!");
                foreach (KeyValuePair<Player, int> entry in Bets)
                {
                    Dealer.Balance -= entry.Value;
                    Players.Where(x => x.Name == entry.Key.Name).First().Balance += (entry.Value * 2);
                    Console.WriteLine("{0} won {1}! Your balance is {2}", entry.Key.Name, entry.Value, Players.First().Balance);
                }
                return;
            }
            foreach (Player player in Players)
            {
                bool? playerWon = TwentyOneRules.CompareHands(player.Hand, Dealer.Hand);
                if (playerWon == null)
                {
                    Console.WriteLine("Push! No one wins. Your balance is {0}", player.Balance);
                    player.Balance = +Bets[player];
                    Bets.Remove(player);
                }
                else if (playerWon == true)
                {
                    player.Balance += (Bets[player] * 2);
                    Dealer.Balance -= Bets[player];
                    Console.WriteLine("{0} won {1}! Your balance is {2}", player.Name, Bets[player], player.Balance);
                }
                else
                {
                    Dealer.Balance += Bets[player];
                    Console.WriteLine("I'm sorry, dealer wins {0}! Your balance is {1}", Bets[player], player.Balance);
                }
                Console.WriteLine("Play again?");
                string answer = Console.ReadLine().ToLower();
                if (answer == "yes" || answer == "ok" || answer == "okay" || answer == "sure" || answer == "yeah" || answer == "y" || answer == "yep" || answer == "yup" || answer == "ya")
                {
                    player.isActivelyPlaying = true;
                }
                else
                {
                    player.isActivelyPlaying = false;
                }
            }
        }
        public override void ListPlayers()
        {
            Console.WriteLine("21 Players:");
            base.ListPlayers();
        }

        public void WalkAway(Player player)
        {
            throw new NotImplementedException();
        }
    }
}
