using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardAssetManager : MonoBehaviour
{
    public Sprite[] hearts;
    public Sprite[] diamonds;
    public Sprite[] clubs;
    public Sprite[] spades;
    public Sprite joker;
    public Sprite back;
    //private Sprite[,] cardAssets = new Sprite[5, 10];

    public Sprite getCardSprite(Suit suit, int number)
    {
        //Debug.Log("Getting asset for card: " + suit + number);
        if ((number < 15 && number > 3) || suit == Suit.joker)
        {
            //Debug.Log("Requesting for " + number);

            switch (suit)
            {
                case Suit.heart:
                    return hearts[number-4];
                case Suit.diamond:
                    return diamonds[number-4];
                case Suit.club:
                    return clubs[number-4];
                case Suit.spade:
                    return spades[number-4];
                case Suit.joker:
                    return joker;
                default:
                    Debug.Log(number - 4);
                    return back;
            }
        } else
        {
            return back;
        }

    }
}
