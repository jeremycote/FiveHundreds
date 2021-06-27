using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public Hand[] hands;
    public Kitty kitty;
    public Deck deck;
    public List<Tuple<int, int, Card>> discard = new List<Tuple<int, int, Card>>();
    public List<Card> playedCards = new List<Card>();
    private int currentPlayer = 0;
    private int numberOfPlayers = 4;
    private Card lastMove;
    private Card firstMove;
    private bool isFirstMove = true;
    private int moveCounter = 0;
    public GameObject discardPile;
    public Suit trump = Suit.heart;
    //4 players, so 4 scores
    public int[] score = new int[4] { 0, 0, 0, 0 };
    public int bid = 10;
    public Score scoreBoard;
    public bool isBidding = true;
    public BidManager bidManager;
    public Tuple<int, Suit> bestBid = Tuple.Create<int, Suit>(0, Suit.nil);
    public int bestBidder = 0;
    private bool kittyModifiable = false;
    public ComAgent[] agent;
    public bool[] isHuman;
    public bool humanObserver;
    public float animSpeedBot;
    public float animSpeedHuman;

    public float GetAnimSpeed()
    {
        if (humanObserver == true)
        {
            return animSpeedHuman;
        } else
        {
            return animSpeedBot;
        }
    }
    public void Start()
    {
        Card tCard = new Card(Suit.joker, 0);
        Debug.Log(CardToInt(card: tCard));
        Debug.Log(IntToCard(CardToInt(card: tCard)).suit + " " + IntToCard(CardToInt(card: tCard)).number);
        //Debug.Log("num: " + IntToCard(56).number);
        //Debug.Log("num1: " + CardToInt(IntToCard(56)));
        //Debug.Log("suit: " + IntToCard(56).originalSuit);

        StartGame();
    }
    public void StartKittyModification()
    {
        kitty.gameObject.SetActive(true);
        SetKittyModifiable(true);
        RequestMove();

    }
    public void EndKittyModification()
    {
        SetKittyModifiable(false);
        kitty.gameObject.SetActive(false);
        Debug.Log("Kitty mod done");
        RequestMove();
    }
    public void SetKittyModifiable(bool isModifiable)
    {
        kittyModifiable = isModifiable;
    }
    public bool GetKittyModifiable()
    {
        return kittyModifiable;
    }
    public void StartGame()
    {
        StartCoroutine(DelayedStart());
    }
    public void ResetScore()
    {
        for (int i = 0; i < score.Length; i++)
        {
            score[i] = 0;
        }
    }
    IEnumerator DelayedStart()
    {
        //Small delay to assure that cards have been determined
        yield return new WaitForSeconds(0.01f);
        ResetScore();
        System.Random r = new System.Random();
        currentPlayer = r.Next(0, 3);
        deck.deal();
        StartBidding();
        RequestMove();
    }
    public void SetTrump(Suit suit)
    {
        trump = suit;
    }
    public void StartBidding()
    {
        isBidding = true;
        bidManager.gameObject.SetActive(true);
        bidManager.StartBidding();
    }
    public void EndBidding()
    {
        //Debug.Log("Bidding complete");
        SetTrump(bestBid.Item2);
        SetCurrentPlayer(bestBidder);
        isBidding = false;
        GameObject[] cardObjects = GameObject.FindGameObjectsWithTag("Card");
        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardObjects[i].GetComponent<CardVisual>().card.ConformToTrump();
        }
        StartKittyModification();
    }

    public int PlayCard(Card card, Hand hand, bool isAddition)
    {
        if (isBidding == false && GetKittyModifiable() == false)
        {
            if (hand.playerIndex == currentPlayer && isFirstMove == true)
            {
                isFirstMove = false;
                firstMove = card;
                executeMove(card: card, hand: hand);
                return 1;
            }

            if (hand.playerIndex == currentPlayer && isFirstMove == false)
            {
                if (IsMoveLegal(card: card) == true)
                {
                    executeMove(card: card, hand: hand);
                    return 1;
                }
            }
        }
        if (GetKittyModifiable() == true && hand.playerIndex == currentPlayer)
        {
           
            if (isAddition == false)
            {
                //Debug.Log(isAddition);
                if (kitty.AddToKitty(card: card, instantiate: false) == true)
                {
                    //Debug.Log("Transfering To Kitty");
                    hand.RemoveCardFromHand(card: card);
                    return 2;
                } else
                {
                    //Debug.Log(0);
                    return 0;
                }
            } else
            {
                kitty.RemoveFromKitty(card: card);
                hand.AddCardToHand(card: card);
                return 3;
            }

        }
        return 0;

    }
    public void CardPlayedCallback()
    {
        Debug.Log("Callback received");
        if (GetKittyModifiable() == false)
        {
            IncrementMoveCounter();
        } else
        {
            agent[currentPlayer].RequestAction();
        }
    }
    public void executeMove(Card card, Hand hand)
    {
        Debug.Log("Card: " + card.number + " was played by " + hand.playerIndex);
        hand.RemoveCardFromHand(card);
        discard.Add(Tuple.Create(moveCounter, currentPlayer, card));
        playedCards.Add(card);
        NextPlayer();
        lastMove = card;
    }
    public bool IsMoveLegal(Card card)
    {
        if (isFirstMove == true)
        {
            return true;
        }
        Card[] currentCards = hands[currentPlayer].getCards();
        int numberOfSameSuit = 0;
        for (int i = 0; i < currentCards.Length; i++)
        {
            if (currentCards[i].suit == firstMove.suit)
            {
                numberOfSameSuit++;
            }
            else if (currentCards[i].suit == Suit.joker && firstMove.suit == trump)
            {
                //Never triggered?
                numberOfSameSuit++;
            }
        }
        if (numberOfSameSuit > 0 && card.suit != firstMove.suit)
        {
            if (firstMove.suit == trump && card.suit == Suit.joker)
            {
                return true;
            }
            return false;
        }
        return true;
    }
    private void IncrementMoveCounter()
    {
        moveCounter++;
        if (moveCounter >= 4)
        {
            moveCounter = 0;
            foreach (Transform child in discardPile.transform)
            {
                Destroy(child.gameObject);
            }

            isFirstMove = true;
            if (EvaluateTrickWinner() == true)
            {
                discard.Clear();
                return;
            }
            discard.Clear();
        }
        Debug.Log("Requesting move from IncrementMoveCounter");
        RequestMove();

    }
    public void RequestMove()
    {
        if (isHuman[currentPlayer] == false)
        {
            agent[currentPlayer].RequestDecision();

        }
        //Debug.Log("RequestDecision called");
    }
    private bool EvaluateTrickWinner()
    {
        //Inital value doesn't matter because it is always set
        Tuple<int, int, Card> bestMove = Tuple.Create(5, 5, new Card(suit: Suit.spade, card: 4));

        //Sets first card
        int i = 0;
        foreach (Tuple<int, int, Card> tuple in discard)
        {
            if (tuple.Item1 == 0)
            {
                bestMove = tuple;
                //Debug.Log("Set Tuple");
            }

            i++;
        }

        i = 0;
        foreach (Tuple<int, int, Card> tuple in discard)
        {
            if (tuple.Item1 != 0)
            {
                if (tuple.Item3.suit == Suit.joker)
                {
                    bestMove = tuple;
                }
                else
                {
                    if (tuple.Item3.suit == trump)
                    {
                        if (bestMove.Item3.suit == trump && tuple.Item3.number > bestMove.Item3.number)
                        {
                            bestMove = tuple;
                        }
                        else if (bestMove.Item3.suit != trump)
                        {
                            bestMove = tuple;
                        }
                    }
                    else if (tuple.Item3.suit == bestMove.Item3.suit && tuple.Item3.number > bestMove.Item3.number)
                    {
                        bestMove = tuple;
                    }
                }
            }

            i++;
        }
        //Debug.Log("The best card was played by player " + bestMove.Item2 + " with " + bestMove.Item3.number);
        currentPlayer = bestMove.Item2;
        if (WinTrick(bestMove.Item2) == true)
        {
            return true;
        } else
        {
            return false;
        }
    }
    public bool WinTrick(int player)
    {
        score[player]++;
        scoreBoard.setScores(score);
        //Debug.Log("Player won the trick");
        if (score[0] + score[1] + score[2] + score[3] >= 10)
        {
            if (score[0] + score [2] >= bid && (bestBidder == 0 || bestBidder == 2))
            {
                //Team 0+2 won
                Debug.Log("Team 0+2 won");
                AssignRewardsForWin(0);
            } else if (score[1] + score[3] >= bid && (bestBidder == 1 || bestBidder == 3))
            {
                //Team 1+3 won
                Debug.Log("Team 1+3 won");
                AssignRewardsForWin(1);
            } else if (bestBidder == 0 || bestBidder == 2)
            {
                //Team 1+3 won
                Debug.Log("Team 1+3 won");
                AssignRewardsForWin(1);
            } else if (bestBidder == 1 || bestBidder == 3)
            {
                //Team 0+2 won
                AssignRewardsForWin(0);
                Debug.Log("Team 0+2 won");
            } else
            {
                //Everyone lost?
                Debug.Log("Confused about winner");
            }
            return true;
        }
        return false;
    }
    private void AssignRewardsForWin(int team)
    {
        switch (team)
        {
            case 0:
                agent[0].SetReward(1);
                agent[2].SetReward(1);

                agent[1].AddReward(-1f);
                agent[3].AddReward(-1f);
                break;
            case 1:
                agent[1].SetReward(1);
                agent[3].SetReward(1);

                agent[0].AddReward(-1f);
                agent[2].AddReward(-1f);
                break;
            default:
                break;
        }
        for (int i = 0; i < agent.Length; i++)
        {
            agent[i].EndEpisode();
        }
    }
    public void NextPlayer()
    {
        if (currentPlayer + 1 < numberOfPlayers)
        {
            currentPlayer++;
        }
        else
        {
            currentPlayer = 0;
        }
        //RequestMove();

    }
    public int GetCurrentPlayer()
    {
        return currentPlayer;
    }
    public void SetCurrentPlayer(int player)
    {
        currentPlayer = player;
    }
    public void SetBestBid(int player, Tuple<int,Suit> bid)
    {
        bestBid = bid;
        bestBidder = player;
    }

    public Card IntToCard(int card)
    {
        switch (card) {
            case 0:
                return new Card(suit: Suit.spade, card: 0);
            case 1:
                return new Card(suit: Suit.spade, card: 1);
            case 2:
                return new Card(suit: Suit.spade, card: 2);
            case 3:
                return new Card(suit: Suit.spade, card: 3);
            case 4:
                return new Card(suit: Suit.spade, card: 4);
            case 5:
                return new Card(suit: Suit.spade, card: 5);
            case 6:
                return new Card(suit: Suit.spade, card: 6);
            case 7:
                return new Card(suit: Suit.spade, card: 7);
            case 8:
                return new Card(suit: Suit.spade, card: 8);
            case 9:
                return new Card(suit: Suit.spade, card: 9);
            case 10:
                return new Card(suit: Suit.spade, card: 10);
            case 11:
                return new Card(suit: Suit.club, card: 0);
            case 12:
                return new Card(suit: Suit.club, card: 1);
            case 13:
                return new Card(suit: Suit.club, card: 2);
            case 14:
                return new Card(suit: Suit.club, card: 3);
            case 15:
                return new Card(suit: Suit.club, card: 4);
            case 16:
                return new Card(suit: Suit.club, card: 5);
            case 17:
                return new Card(suit: Suit.club, card: 6);
            case 18:
                return new Card(suit: Suit.club, card: 7);
            case 19:
                return new Card(suit: Suit.club, card: 8);
            case 20:
                return new Card(suit: Suit.club, card: 9);
            case 21:
                return new Card(suit: Suit.club, card: 10);
            case 22:
                return new Card(suit: Suit.diamond, card: 0);
            case 23:
                return new Card(suit: Suit.diamond, card: 1);
            case 24:
                return new Card(suit: Suit.diamond, card: 2);
            case 25:
                return new Card(suit: Suit.diamond, card: 3);
            case 26:
                return new Card(suit: Suit.diamond, card: 4);
            case 27:
                return new Card(suit: Suit.diamond, card: 5);
            case 28:
                return new Card(suit: Suit.diamond, card: 6);
            case 29:
                return new Card(suit: Suit.diamond, card: 7);
            case 30:
                return new Card(suit: Suit.diamond, card: 8);
            case 31:
                return new Card(suit: Suit.diamond, card: 9);
            case 32:
                return new Card(suit: Suit.diamond, card: 10);
            case 33:
                return new Card(suit: Suit.diamond, card: 0);
            case 34:
                return new Card(suit: Suit.heart, card: 1);
            case 35:
                return new Card(suit: Suit.heart, card: 2);
            case 36:
                return new Card(suit: Suit.heart, card: 3);
            case 37:
                return new Card(suit: Suit.heart, card: 4);
            case 38:
                return new Card(suit: Suit.heart, card: 5);
            case 39:
                return new Card(suit: Suit.heart, card: 6);
            case 40:
                return new Card(suit: Suit.heart, card: 7);
            case 41:
                return new Card(suit: Suit.heart, card: 8);
            case 42:
                return new Card(suit: Suit.heart, card: 9);
            case 43:
                return new Card(suit: Suit.heart, card: 10);
            case 44:
                return new Card(suit: Suit.joker, card: 0);
            default:
                Debug.LogError("Invalid card integer: " + card);
                return new Card(suit: Suit.spade, card: 0);
        }
        /*
        //14
        float rem = (float)card / 10;
        float remF = (float)Math.Floor(rem);
        rem = (float)Math.Ceiling(rem);
        //print(remF);
        //print(rem);

        Card returnCard = new Card(suit: Suit.heart, card: 4);
        //Debug.Log(remF);
        switch (rem)
        {
            case 0:
                returnCard = new Card(suit: Suit.spade, card: Convert.ToInt32(card - remF * 10));
                break;
            case 1:
                returnCard = new Card(suit: Suit.club, card: Convert.ToInt32(card - remF * 10));
                break;
            case 2:
                returnCard = new Card(suit: Suit.diamond, card: Convert.ToInt32(card - remF * 10));
                break;
            case 3:
                returnCard = new Card(suit: Suit.heart, card: Convert.ToInt32(card - remF * 10));
                break;
            case 4:
                returnCard = new Card(suit: Suit.joker, card: 0);
                break;
            default:
                break;
        }
        //returnCard.ConformToTrump();
        return returnCard;
        */
    }
    public int CardToInt(Card card)
    {

        switch (card.originalSuit) {
            case Suit.spade:
                switch (card.originalValue) {
                    case 0:
                        return 0;
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 3;
                    case 4:
                        return 4;
                    case 5:
                        return 5;
                    case 6:
                        return 6;
                    case 7:
                        return 7;
                    case 8:
                        return 8;
                    case 9:
                        return 9;
                    case 10:
                        return 10;
                    default:
                        Debug.Log("Illegal value");
                        return 0;
                }
            case Suit.club:
                switch (card.originalValue)
                {
                    case 0:
                        return 11;
                    case 1:
                        return 12;
                    case 2:
                        return 13;
                    case 3:
                        return 14;
                    case 4:
                        return 15;
                    case 5:
                        return 16;
                    case 6:
                        return 17;
                    case 7:
                        return 18;
                    case 8:
                        return 19;
                    case 9:
                        return 20;
                    case 10:
                        return 21;
                    default:
                        Debug.Log("Illegal value");
                        return 0;
                }
            case Suit.diamond:
                switch (card.originalValue)
                {
                    case 0:
                        return 22;
                    case 1:
                        return 23;
                    case 2:
                        return 24;
                    case 3:
                        return 25;
                    case 4:
                        return 26;
                    case 5:
                        return 27;
                    case 6:
                        return 28;
                    case 7:
                        return 29;
                    case 8:
                        return 30;
                    case 9:
                        return 31;
                    case 10:
                        return 32;
                    default:
                        Debug.Log("Illegal value");
                        return 0;
                }
            case Suit.heart:
                switch (card.originalValue)
                {
                    case 0:
                        return 33;
                    case 1:
                        return 34;
                    case 2:
                        return 35;
                    case 3:
                        return 36;
                    case 4:
                        return 37;
                    case 5:
                        return 38;
                    case 6:
                        return 39;
                    case 7:
                        return 40;
                    case 8:
                        return 41;
                    case 9:
                        return 42;
                    case 10:
                        return 43;
                    default:
                        Debug.Log("Illegal value");
                        return 0;
                }
            case Suit.nil:
                Debug.Log("Nil card suit is not possible");
                return 0;
            case Suit.joker:
                return 44;
            default:
                Debug.Log("Unkown suit");
                return 0;
        }

        
        /*
         * Rewritten as Switch because logic doesn't work
        //10 of clubs: suit = 1. Number = 7

        int returnCard = 0;
        if (card.originalSuit == Suit.joker)
        {
            returnCard = 40;
        } else if (card.originalSuit == Suit.club && card.originalValue == 10)
        {
            returnCard = 10 * Convert.ToInt32(card.originalSuit);
            returnCard = returnCard + card.originalValue;
            Debug.Log("For the Ace of Clubs -> int value is " + returnCard);
        }
        else
        {
            returnCard = 10 * Convert.ToInt32(card.originalSuit);
            returnCard = returnCard + card.originalValue;
        }
        
        
        return returnCard;
        */
    }
}

/*
 *spade = 0,
    club = 1,
    diamond = 2,
    heart = 3,
    nil = 4,
    joker = 5
 * cards: 5,6,7,8,9,10,11,12,13,14
 * int: 0,1,2,3,4,5,6,7,8,9
 *
 * 41 cards in deck
 */