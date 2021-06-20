using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

public class BidManager : MonoBehaviour
{
    private GameController gameController;
    public Tuple<int, Suit> currentBid = Tuple.Create<int, Suit>(0, Suit.nil);

    public GameObject BidLabel;
    public GameObject BestBidLabel;
    public GameObject CurrentPlayerLabel;
    List<Tuple<int, Suit>> legalBids = new List<Tuple<int, Suit>>();

    private int bidCounter = 0;

    private void Start()
    {
        gameController = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
        //StartBidding();
    }

    public void BidSuitChosen(int suit)
    {
        currentBid = Tuple.Create<int, Suit>(currentBid.Item1, (Suit)suit);
        UpdateCurrentBidText();
        //Debug.Log(currentBid);
        //Debug.Log((Suit)suit);
    }
    public void BidIncrementChosen(int increment)
    {
        currentBid = Tuple.Create<int, Suit>(currentBid.Item1 + increment, currentBid.Item2);
        UpdateCurrentBidText();
    }
    public void ChooseAndSubmit(Tuple<int, Suit> bid, bool isPass)
    {
        //Debug.Log("ChooseAndSubmit Function debug");
        currentBid = bid;
        UpdateCurrentBidText();
        SubmitBid(isPass);
    }
    public void SubmitBid(bool isPass)
    {
        if (bidCounter == 0 && isPass == true)
        {
            return;
        }
        else if (isPass == true)
        {
            BidConfirmed();
        }
        UpdateLegalBids();
        //Debug.Log(currentBid);
        if (legalBids.Contains(currentBid) == true)
        {
            Debug.Log("Submited bid is legal!");
            gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
            UpdateLegalBids();
            UpdateBestBidText();
            BidConfirmed();
        } else
        {
            Debug.Log("Illigal bid detected: " + currentBid.Item1 + currentBid.Item2);
            //gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
            //UpdateLegalBids();
            //UpdateBestBidText();
            BidConfirmed();
        }
        //Debug.Log(bidCounter + " n bids");

        /*
        if (isPass != true)
        {
            if (currentBid.Item1 == gameController.bestBid.Item1 && Convert.ToInt32(currentBid.Item2) > Convert.ToInt32(gameController.bestBid.Item2))
            {
                gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
            } else if (currentBid.Item1 > gameController.bestBid.Item1 && Convert.ToInt32(currentBid.Item2) == Convert.ToInt32(gameController.bestBid.Item2))
            {
                gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
            } else if (currentBid.Item1 == gameController.bestBid.Item1 && Convert.ToInt32(currentBid.Item2) == Convert.ToInt32(gameController.bestBid.Item2))
            {
                gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
            }
        }
        */


    }
    private void BidConfirmed()
    {
        //Debug.Log(gameController.GetCurrentPlayer());
        gameController.NextPlayer();
        UpdateCurrentPlayerText();
        IncrementBidCounter();

    }
    public void IncrementBidCounter()
    {
        bidCounter++;
        if (bidCounter >= 4)
        {
            gameController.EndBidding();
            //bidCounter = 0;
            Debug.Log("Bidding complete");
            gameObject.SetActive(false);
        } else
        {
            if (gameController.isHuman[gameController.GetCurrentPlayer()] == false)
            {
                gameController.RequestMove();
            }
        }
    }
    public void UpdateBestBidText()
    {
        BestBidLabel.GetComponent<Text>().text = ConvertBidToString(gameController.bestBid);
    }
    public void UpdateCurrentBidText()
    {
        BidLabel.GetComponent<Text>().text = ConvertBidToString(currentBid);
    }
    public string ConvertBidToString(Tuple<int, Suit> bid)
    {
        string bidText = bid.Item1.ToString();

        switch (bid.Item2)
        {
            case Suit.heart:
                bidText = bidText + " hearts";
                break;
            case Suit.diamond:
                bidText = bidText + " diamonds";
                break;
            case Suit.club:
                bidText = bidText + " clubs";
                break;
            case Suit.spade:
                bidText = bidText + " spades";
                break;
            case Suit.nil:
                bidText = bidText + " no trump";
                break;
            default:
                bidText = "Unkown bid";
                break;
        }
        //Debug.Log(bidText);
        return bidText;
    }
    public void StartBidding()
    {
        gameObject.SetActive(true);
        gameController.isBidding = true;
        UpdateCurrentPlayerText();
        currentBid = new Tuple<int, Suit>(0, Suit.spade);
        gameController.SetBestBid(gameController.GetCurrentPlayer(), currentBid);
        ResetLegalBids();
    }

    public Tuple<int, Suit>[] GetLegalBids(bool forceUpdate)
    {
        if (forceUpdate == true)
        {
            UpdateLegalBids();
        }
        return legalBids.ToArray();
    }
    public void UpdateCurrentPlayerText()
    {
        CurrentPlayerLabel.GetComponent<Text>().text = GetGameController().GetCurrentPlayer().ToString();
    }
    public GameController GetGameController()
    {
        if (gameController == null)
        {
            gameController = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
        }

        return gameController;
    }
    public void UpdateLegalBids()
    {
        ResetLegalBids();

        List<Tuple<int, Suit>> toRemove = new List<Tuple<int, Suit>>();
        foreach (Tuple<int, Suit> bid in legalBids)
        {
            if (bid.Item1 < gameController.bestBid.Item1)
            {
                toRemove.Add(bid);
            }
            else if (bid.Item1 == gameController.bestBid.Item1 && Convert.ToInt32(bid.Item2) <= Convert.ToInt32(gameController.bestBid.Item2))
            {
                toRemove.Add(bid);
            }
        }
        foreach (Tuple<int, Suit> tuple in toRemove)
        {
            legalBids.Remove(tuple);
        }

        legalBids.Remove(gameController.bestBid);
    }
    public void ResetLegalBids()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int n = 0; n < 5; n++)
            {
                legalBids.Add(Tuple.Create<int, Suit>(i + 6, (Suit)n));
            }
        }
    }
    public int BidToInt(Tuple<int, Suit> bid)
    {
        int returnInt = Convert.ToInt32(bid.Item2) * 5;
        returnInt = returnInt + bid.Item1;
        return returnInt;
    }
    public Tuple<int, Suit> IntToBid(int bid)
    {
        float rem = (float)bid / 5;
        float remF = (float)Math.Floor(rem);
        rem = (float)Math.Ceiling(rem);
        
        switch (rem)
        {
            case 1:
                return Tuple.Create<int, Suit>(Convert.ToInt32(6 + bid - remF * 5), Suit.spade);
            case 2:
                return Tuple.Create<int, Suit>(Convert.ToInt32(6 + bid - remF * 5), Suit.club);
            case 3:
                return Tuple.Create<int, Suit>(Convert.ToInt32(6 + bid - remF * 5), Suit.diamond);
            case 4:
                return Tuple.Create<int, Suit>(Convert.ToInt32(6 + bid - remF*5), Suit.heart);
            case 5:
                return Tuple.Create<int, Suit>(Convert.ToInt32(6 + bid - remF * 5), Suit.nil);
        }
        return Tuple.Create<int, Suit>(10, Suit.nil);
    }
}
