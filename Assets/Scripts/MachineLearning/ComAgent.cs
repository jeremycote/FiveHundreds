using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;
public class ComAgent : Agent
{
    // Start is called before the first frame update
    private GameController gameController;
    public int playerIndex;
    public int counter = 0;
    private int limit = 53;
    void Start()
    {
        gameController = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
    }
    public override void OnEpisodeBegin()
    {
        counter = 0;
        gameController.StartGame();
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var descreteActionsOut = actionsOut.DiscreteActions;

        if (gameController.isBidding)
        {
            Tuple<int, Suit>[] legalBids = gameController.bidManager.GetLegalBids(forceUpdate: true);
            if (legalBids.Length > 0)
            {
                Tuple<int, Suit> legalBid = legalBids[UnityEngine.Random.Range(0, legalBids.Length)];
                //Debug.Log("Attempting: " + gameController.bidManager.BidToInt(legalBid));
                descreteActionsOut[0] = gameController.bidManager.BidToInt(legalBid);
            } else {
                descreteActionsOut[0] = limit;
            }
        } else {
            Card[] cardsInHand = gameController.hands[playerIndex].getCards();
            if (gameController.GetKittyModifiable())
            {
                descreteActionsOut[0] = limit;
                //descreteActionsOut[0] = -1;
                //descreteActionsOut[2] = -1;
                //descreteActionsOut[1] = UnityEngine.Random.Range(0, cardsInHand.Length);
            } else
            {
                List<int> legalMoves = new List<int>();
                for (int i = 0; i < cardsInHand.Length; i++)
                {
                    if (gameController.IsMoveLegal(cardsInHand[i]))
                    {
                        legalMoves.Add(gameController.CardToInt(cardsInHand[i]));
                    }
                }
                //descreteActionsOut[0] = -1;
                //descreteActionsOut[1] = -1;
                if (legalMoves.Count == 0)
                {
                    Debug.Log("No cards left");
                } else {
                    //int choice = legalMoves.ToArray()[UnityEngine.Random.Range(0, legalMoves.Count)];
                    int choice = legalMoves[0];
                    //Debug.Log("Picked: " + choice);
                    descreteActionsOut[0] = choice;
                    Debug.Log("Computer requested for " + choice);
                    if ( gameController.IsMoveLegal(gameController.IntToCard(choice)) == false) {
                        Debug.Log("Computer chose illegal move! Choice is " + choice);
                    }
                    
                }
                
            }
        }
        Debug.Log("action: " + descreteActionsOut[0]);
        //Debug.Log("Heuristic called");

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations");
        //Adds the player's hand to observations
        Card[] cards = gameController.hands[playerIndex].getCards();
        for (int i = 0; i < cards.Length; i++)
        {
            sensor.AddObservation(observation: gameController.CardToInt(cards[i]));
        }
        //Adds played cards
        Tuple<int,int,Card>[] discard = gameController.discard.ToArray();
        for (int i = 0; i < cards.Length; i++)
        {
            //sensor.AddObservation(observation: new Vector3(x: discard[i].Item1, y: discard[i].Item2, z: gameController.CardToInt(discard[i].Item3)));
        }
        sensor.AddObservation(observation: Convert.ToInt32(gameController.trump));
        sensor.AddObservation(observation: gameController.isBidding);
        sensor.AddObservation(observation: gameController.GetKittyModifiable());
        sensor.AddObservation(observation: gameController.bidManager.BidToInt(gameController.bestBid));
        sensor.AddObservation(observation: gameController.bestBidder);

        if (gameController.GetCurrentPlayer() == playerIndex && gameController.GetKittyModifiable() == true)
        {
            Card[] kitCards = gameController.kitty.GetCards().ToArray();
            for (int i = 0; i < kitCards.Length; i++)
            {
                sensor.AddObservation(observation: gameController.CardToInt(kitCards[i]));
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log("Action received");
        if (gameController.GetCurrentPlayer() != playerIndex)
        {
            Debug.Log("Not your turn p" + playerIndex + "!");
            return;
        }
        if (gameController.isBidding)
        {
            if (actionBuffers.DiscreteActions[0] == limit)
            {
                gameController.bidManager.SubmitBid(isPass: true);
            }
            else
            {
                gameController.bidManager.ChooseAndSubmit(bid: gameController.bidManager.IntToBid(actionBuffers.DiscreteActions[0]), isPass: false);
                //gameController.RequestMove();

            }
        }
        else
        {
            if (gameController.GetKittyModifiable())
            {
                if (actionBuffers.DiscreteActions[0] == limit)
                {
                    gameController.kitty.FinishModifications();
                }
                else {
                    GameObject[] visualCardObjects = gameController.kitty.GetRenewedVisuals();
                    CardVisual[] visualCard = new CardVisual[visualCardObjects.Length];
                    for (int i = 0; i < visualCard.Length; i++) {
                        visualCard[i] = visualCardObjects[i].GetComponent<CardVisual>();
                    }
                    bool found = false;
                    for (int i = 0; i < visualCard.Length; i++)
                    {
                        if (visualCard[i] != null)
                        {
                            //Debug.Log("VisualCard " + visualCard[0].card.number);
                            if (visualCard[i].card == gameController.IntToCard(actionBuffers.DiscreteActions[0]))
                            {
                                visualCard[actionBuffers.DiscreteActions[0]].CardClicked();
                                found = true;
                                break;
                            }
                        } else
                        {
                            Debug.Log(i + "is too large");
                        }
                        
                    }
                    if (found == false)
                    {
                        GameObject[] visualCardHand = gameController.hands[playerIndex].GetCardVisuals();
                        for (int i = 0; i < visualCard.Length; i++)
                        {
                            Card cardP = gameController.IntToCard(actionBuffers.DiscreteActions[0]);
                            
                            //Debug.Log("Looking to play " + actionBuffers.DiscreteActions[0] + ". This translates to " + cardP.number + " of " + cardP.suit);
                            if (visualCardHand[i].GetComponent<CardVisual>().card == gameController.IntToCard(actionBuffers.DiscreteActions[0]))
                            {
                                visualCardHand[actionBuffers.DiscreteActions[0]].GetComponent<CardVisual>().CardClicked();
                                break;
                            }
                        }
                    }
                    counter++;
                    if (counter >= 100)
                    {
                        AddReward(-0.1f);
                        EndEpisode();

                    }
                    gameController.RequestMove();
                }
            }
            else
            {
                //Touch Hand DiscreteActions[4]
                //Debug.Log("acting on play");
                //Card desiredCard = gameController.IntToCard(actionBuffers.DiscreteActions[0]);
                //Debug.Log("Looking for " + desiredCard.number);
                GameObject[] visualCard = gameController.hands[playerIndex].GetCardVisuals();
                Debug.Log("There are " + visualCard.Length + " cards");
                for (int i = 0; i < visualCard.Length; i++)
                {
                    Card card = visualCard[i].GetComponent<CardVisual>().card;
                    if (gameController.CardToInt(card) == actionBuffers.DiscreteActions[0])
                    {
                        Debug.Log("Starting click. Clicked on " + card.originalValue + " of " + card.originalSuit + ". The move is legal: " + gameController.IsMoveLegal(card) + ". As Int: " + gameController.CardToInt(card) + ". Request is: " + actionBuffers.DiscreteActions[0]);
                        visualCard[i].GetComponent<CardVisual>().CardClicked();
                        break;
                    } else
                    {
                        //Debug.Log(gameController.CardToInt(card) + " doesnt match: " + actionBuffers.DiscreteActions[0]);
                    }
                }
                
                
            }

        }
        //Debug.Log("ActionReceived called");


    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        Debug.Log("Writing Mask");
        for (int i = 0; i < limit+1; i++)
        {
            actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: false);
        }

        if (gameController.isBidding)
        {
            Tuple<int, Suit>[] legalBids = gameController.bidManager.GetLegalBids(forceUpdate: true);
            //Populate allowed
            for (int i = 0; i < legalBids.Length; i++)
            {
                actionMask.SetActionEnabled(branch: 0, actionIndex: gameController.bidManager.BidToInt(legalBids[i]), isEnabled: true);
            }
            actionMask.SetActionEnabled(branch: 0, actionIndex: limit, isEnabled: true);

        }
        else if (gameController.GetKittyModifiable())
        {
            int lenght = gameController.hands[playerIndex].getCards().Length;

            //Stops hand between 9 and 10
            if (lenght == 10)
            {
                Card[] cards = gameController.hands[playerIndex].getCards();
                for (int i = 0; i < cards.Length; i++)
                {
                    Debug.Log(cards[i].number + " " + cards[i].suit + "|| Index: " + gameController.CardToInt(cards[i]));
                    actionMask.SetActionEnabled(branch: 0, actionIndex: gameController.CardToInt(cards[i]), isEnabled: true);
                }
                actionMask.SetActionEnabled(branch: 0, actionIndex: limit, isEnabled: true);
            }
            else
            {
                Card[] cards = gameController.kitty.GetCards().ToArray();
                for (int i = 0; i < cards.Length; i++)
                {
                    actionMask.SetActionEnabled(branch: 0, actionIndex: gameController.CardToInt(cards[i]), isEnabled: true);
                }
            }


        }
        else
        {
            Card[] cardsInHand = gameController.hands[playerIndex].getCards();
            List<int> legalMoves = new List<int>();
            for (int i = 0; i < cardsInHand.Length; i++)
            {
                if (gameController.IsMoveLegal(cardsInHand[i]))
                {
                    legalMoves.Add(gameController.CardToInt(cardsInHand[i]));
                }
            }

            if (legalMoves.Count == 0)
            {
                Debug.Log("No cards left");
            }
            else
            {
                foreach (int move in legalMoves)
                {
                    Debug.Log(move);
                    actionMask.SetActionEnabled(branch: 0, actionIndex: move, isEnabled: true);
                }
                Debug.Log(legalMoves.Count);
            }
        }
    }


}


/*
 * if (gameController.isBidding)
        {
            Tuple<int, Suit>[] legalBids = gameController.bidManager.GetLegalBids(forceUpdate: false);
            for (int i = 0; i < legalBids.Length; i++)
            {
                actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: true);
            }
            for (int i = 25 - legalBids.Length; i < (26); i++)
            {
                actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 10; i++)
            {
                actionMask.SetActionEnabled(branch: 1, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 6; i++)
            {
                actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: false);
            }
            actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: false);
            for (int i = 0; i < 10; i++)
            {
                actionMask.SetActionEnabled(branch: 4, actionIndex: i, isEnabled: false);
            }

            //To Disable
            for (int i = 0; i < 10; i++)
            {
                actionMask.SetActionEnabled(branch: 1, actionIndex: i, isEnabled: false);
                actionMask.SetActionEnabled(branch: 4, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 4; i++)
            {
                actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: false);
            }
            actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: false);

        }
        else if (gameController.GetKittyModifiable())
        {
            int lenght = gameController.hands[playerIndex].getCards().Length;
            int kLenght = gameController.kitty.GetCards().Count;

            if (lenght >= 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    actionMask.SetActionEnabled(branch: 1, actionIndex: i, isEnabled: true);
                }
            } else
            {
                for (int i = 0; i < 10; i++)
                {
                    actionMask.SetActionEnabled(branch: 1, actionIndex: i, isEnabled: false);
                }
            }
            if (kLenght >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: true);
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: false);
                }
            }

            if (kLenght == 3)
            {
                actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: true);
            } else
            {
                actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: false);
            }
            //To Disable
            for (int i = 0; i < 26; i++)
            {
                actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 4; i++)
            {
                actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: false);
            }
            actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: false);
            for (int i = 0; i < 10; i++)
            {
                actionMask.SetActionEnabled(branch: 4, actionIndex: i, isEnabled: false);
            }
        } else
        {
            Card[] cardsInHand = gameController.hands[playerIndex].getCards();
            List<int> legalMoves = new List<int>();
            List<int> illegalMoves = new List<int>();
            for (int i = 0; i < cardsInHand.Length; i++)
            {
                if (gameController.IsMoveLegal(cardsInHand[i]))
                {
                    legalMoves.Add(i);
                } else
                {
                    illegalMoves.Add(i);
                }
            }
            foreach(int move in legalMoves)
            {
                actionMask.SetActionEnabled(branch: 4, actionIndex: move, isEnabled: true);
            }
            foreach (int move in illegalMoves)
            {
                actionMask.SetActionEnabled(branch: 4, actionIndex: move, isEnabled: false);
            }

            for (int i = 0; i < 26; i++)
            {
                actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 10; i++)
            {
                actionMask.SetActionEnabled(branch: 1, actionIndex: i, isEnabled: false);
            }
            for (int i = 0; i < 4; i++)
            {
                actionMask.SetActionEnabled(branch: 2, actionIndex: i, isEnabled: false);
            }
            actionMask.SetActionEnabled(branch: 3, actionIndex: 0, isEnabled: false);

        }





            for (int i = 0; i < 10; i++)
            {
                if (lenght == 10)
                {
                    actionMask.SetActionEnabled(branch: 0, actionIndex: , isEnabled: true);
                } else
                {
                    actionMask.SetActionEnabled(branch: 0, actionIndex: i, isEnabled: false);
                }
            }
            //Stops kit between 3 and 4
            for (int i = 0; i < 4; i++)
            {
                if (lenght == 9)
                {
                    actionMask.SetActionEnabled(branch: 0, actionIndex: 10 + i, isEnabled: true);
                }
                else
                {
                    actionMask.SetActionEnabled(branch: 0, actionIndex: 10 + i, isEnabled: false);
                }
            }
*/