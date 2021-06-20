using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Suit {
    spade = 0,
    club = 1,
    diamond = 2,
    heart = 3,
    nil = 4,
    joker = 5
}

public class Card
{

    //0 for 4, 1 for 5, 6 for 10, 7 for jack, 8 for queen, 9 for king, 10 for ace, 0 for joker
    public Suit suit;
    public int number;
    public int originalValue;
    public Suit originalSuit;
    public readonly Sprite sprite;

    public Card(Suit suit, int card)
    {
        this.suit = suit;
        this.originalValue = card;
        this.number = card;
        this.originalSuit = suit;
        this.sprite = GameObject.FindGameObjectsWithTag("CardAssetManager")[0].GetComponent<CardAssetManager>().getCardSprite(suit: suit, number: card);

    }

    public void ConformToTrump()
    {
        if (number == 11)
        {
            GameController gameController = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
            if (suit == gameController.trump)
            {
                number = 16;
            } else if (suit == Suit.joker) {
                suit = gameController.trump;
                number = 17;
            } else
            {
                Suit altSuit = Suit.nil;
                switch (gameController.trump)
                {
                    case Suit.heart:
                        altSuit = Suit.diamond;
                        break;
                    case Suit.diamond:
                        altSuit = Suit.heart;
                        break;
                    case Suit.club:
                        altSuit = Suit.spade;
                        break;
                    case Suit.spade:
                        altSuit = Suit.club;
                        break;
                    case Suit.joker:
                        break;
                    default:
                        return;
                }

                if (suit == altSuit)
                {
                    //Debug.Log("Aternate Bower");
                    number = 15;
                    suit = gameController.trump;
                } else
                {
                    //Debug.Log("Not Alternate, altSuit is " + altSuit + ". But card is " + suit);
                }
            }
        }
    }
}
