using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kitty : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    private List<GameObject> visualCards = new List<GameObject>();
    public GameObject cardPrefab;
    public Vector3 initalPosition;
    public GameObject kittyConfirmButton;
    public List<Card> GetCards()
    {
        return cards;
    }
    public List<GameObject> GetCardVisuals()
    {
  
        return visualCards;
    }
    public GameObject[] GetRenewedVisuals()
    {

        visualCards.Clear();
        CardVisual[] children = this.gameObject.GetComponentsInChildren<CardVisual>();
        foreach (CardVisual transform in children)
        {
            visualCards.Add(transform.gameObject);
            Debug.Log("Added Visual");
        }
        Debug.Log(children.Length + " Produced " + visualCards.Count + " Which becomes " + visualCards.ToArray().Length);
        return visualCards.ToArray();
    }
    public void FinishModifications()
    {
        if (cards.Count == 5)
        {
            GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>().EndKittyModification();
            kittyConfirmButton.SetActive(false);
        } else
        {
            Debug.Log(cards.Count);
        }
    }
    public bool AddToKitty(Card card, bool instantiate = false)
    {
        if ((this.cards.Count + 1) >= 7)
        {
            //Not enough space
            return false;
        }
        this.cards.Add(card);
        if (instantiate == true)
        {
            GameObject newCard = Instantiate(cardPrefab);
            newCard.transform.SetParent(gameObject.transform, false);
            visualCards.Add(newCard);

            newCard.GetComponent<Image>().sprite = card.sprite;
        }

        this.visualCards.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            visualCards.Add(child.gameObject);
        }
        FitCardsHorizontal();
        Debug.Log("There are " + cards.Count + " cards in the kitty.");
        return true;
    }
    public void RemoveFromKitty(Card card)
    {

        if (cards.Contains(card) == true)
        {
            cards.Remove(card);
            /*
            List<GameObject> toRemove = new List<GameObject>();
            foreach (GameObject visual in visualCards)
            {
                if (visual.GetComponent<CardVisual>().card == card)
                {
                    toRemove.Add(visual);
                }
            }
            foreach (GameObject visual in toRemove)
            {
                visualCards.Remove(visual);
            }
            */
            this.visualCards.Clear();
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                visualCards.Add(child.gameObject);
            }
            //UpdateHandVisual();
        }
        else
        {
            Debug.Log("Hand doesn't contain card");
        }

        FitCardsHorizontal();
    }
    public void SetKitty(Card[] cards)
    {
        this.cards.Clear();
        for (int i = 0; i < cards.Length; i++)
        {
            this.cards.Add(cards[i]);
        }
        foreach (GameObject visual in visualCards)
        {
            Destroy(visual.gameObject);
        }
        visualCards.Clear();
        foreach (Card card in cards)
        {
            GameObject newCard = Instantiate(cardPrefab);
            newCard.transform.SetParent(gameObject.transform, false);
            visualCards.Add(newCard);
            newCard.GetComponent<CardVisual>().card = card;
            newCard.GetComponent<Image>().sprite = card.sprite;
        }

        FitCardsHorizontal();
        kittyConfirmButton.SetActive(true);
    }

    void FitCardsHorizontal()
    {
        gameObject.transform.localPosition = initalPosition;
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        var leftPoint = corners[0];
        var rightPoint = corners[3];
        //var leftPoint = new Vector3(-500,150,0);
        //var rightPoint = new Vector3(-100, 150, 0);

        //Debug.Log("corners[0]: " + corners[0] + " corners[1]: " + corners[1] + " corners[2]: " + corners[2] + " corners[3]: " + corners[3]);

        var delta = (rightPoint - leftPoint).magnitude;

        var howMany = visualCards.Count;

        var howManyGapsBetweenItems = howMany - 1;

        var theHighestIndex = howMany;

        var gapFromOneItemToTheNextOne = delta / howManyGapsBetweenItems;
        //Debug.Log(gapFromOneItemToTheNextOne);


        for (int i = 0; i < theHighestIndex; i++)
        {
            visualCards[i].transform.position = leftPoint;
            visualCards[i].transform.position += new Vector3((i * gapFromOneItemToTheNextOne), 150, 0);
        }

    }
}