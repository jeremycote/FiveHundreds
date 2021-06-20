using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    private List<GameObject> visualCards = new List<GameObject>();
    public GameObject cardPrefab;
    [SerializeField]
    public int playerIndex;
    public bool isTurn = true;
    public bool isHuman = true;

    // Start is called before the first frame update
    void Start()
    {

    }
    public Card[] getCards()
    {
        return cards.ToArray();
    }
    public GameObject[] GetCardVisuals()
    {
        this.visualCards.Clear();
        CardVisual[] allChildren = GetComponentsInChildren<CardVisual>();
        foreach (CardVisual child in allChildren)
        {
            visualCards.Add(child.gameObject);
        }
        return visualCards.ToArray();
    }
    public void RemoveCardFromHand(Card card)
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
        } else
        {
            Debug.Log("Hand doesn't contain card");
        }
        
    }
    public void AddCardToHand(Card card)
    {
        cards.Add(card);
        visualCards.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            visualCards.Add(child.gameObject);
        }
        UpdateHandVisual();
        /*
        if (playerIndex == 0 || playerIndex == 2)
        {
            FitCardsHorizontal();
        } else
        {
            FitCardsVertical();
        }
        */
        //UpdateHandVisual();
    }
    void FitCardsHorizontal()
    {
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        var leftPoint = corners[0];
        var rightPoint = corners[3];


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
    void FitCardsVertical()
    {
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        var bottomPoint = corners[0];
        var topPoint = corners[1];


        //Debug.Log("corners[0]: " + corners[0] + " corners[1]: " + corners[1] + " corners[2]: " + corners[2] + " corners[3]: " + corners[3]);

        var delta = (topPoint - bottomPoint).magnitude;

        var howMany = visualCards.Count;

        var howManyGapsBetweenItems = howMany - 1;

        var theHighestIndex = howMany;

        var gapFromOneItemToTheNextOne = delta / howManyGapsBetweenItems;
        //Debug.Log(gapFromOneItemToTheNextOne);


        for (int i = 0; i < theHighestIndex; i++)
        {
            visualCards[i].transform.position = bottomPoint;
            visualCards[i].transform.position += new Vector3(150, (i * gapFromOneItemToTheNextOne), 0);
        }

    }
    public void SetHand(Card[] cards)
    {

        this.cards.Clear();

        for (int i = 0; i < cards.Length; i++)
        {
            this.cards.Add(cards[i]);
            //Debug.Log("New");
        }

        UpdateHandVisual();

    }

    public void UpdateHandVisual()
    {
        visualCards.Clear();
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        int n = 0;
        foreach (Card card in cards)
        {

            GameObject newCard = Instantiate(cardPrefab);
            RectTransform rectTransform = newCard.GetComponent<RectTransform>();
            rectTransform.rotation = Quaternion.Euler(0, 0, -90 * playerIndex);

            newCard.transform.SetParent(gameObject.transform, false);
            visualCards.Add(newCard);

            newCard.GetComponent<Image>().sprite = card.sprite;
            newCard.GetComponent<CardVisual>().card = card;
            //newCard.transform.localScale = Vector3.one;

            /*
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(0f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            */
            /*
            if (playerIndex == 0 || playerIndex == 2)
            {
                rectTransform.localPosition = new Vector2(100 * n - 400, rectTransform.localPosition.y);
            }
            else
            {
                rectTransform.localPosition = new Vector2(rectTransform.localPosition.x 300, 100 * n -400);
            }
            */
            n++;
        }
        if (playerIndex == 0 || playerIndex == 2)
        {
            FitCardsHorizontal();

        }
        else
        {
            FitCardsVertical();
        }
    }
}
