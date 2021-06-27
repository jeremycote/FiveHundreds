using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private Card[] cards = new Card[45];
    public GameController gameController;

    private void Start()
    {
        restock();
    }

    public void deal()
    {
        Card[] shuffled = cards;
        System.Random random = new System.Random();
        shuffled = shuffled.OrderBy(x => random.Next()).ToArray();

        int numberOfHands = gameController.hands.Length;
        //Debug.Log("Dealing out cards");
        IEnumerable<IEnumerable<Card>> splited = shuffled.Split<Card>(10);

        int i = 0;
        foreach (IEnumerable<Card> s in splited)
        {
            if (i < numberOfHands)
            {
                gameController.hands[i].SetHand(s.ToArray());
            }
            else
            {
                gameController.kitty.SetKitty(s.ToArray());
                //Debug.Log(gameController.kitty.GetCards());
            }

            i++;
        }
    }
    public void restock()
    {
        for (int n = 0; n < 11; n++)
        {
            //0..9 = 0,1,2,3,4,5,6,7,8,9,10
            cards[n] = new Card(suit: Suit.heart, card: n);
            //10..19 = 5,6,7,8,9,10,11,12,13,14
            cards[n + 11] = new Card(suit: Suit.diamond, card: n);
            //20..29 = 5,6,7,8,9,10,11,12,13,14
            cards[n + 22] = new Card(suit: Suit.club, card: n);
            //30..39 = 5,6,7,8,9,10,11,12,13,14
            cards[n + 33] = new Card(suit: Suit.spade, card: n);

        }
        //cards[40] = new Card(suit: Suit.heart, 4);
        //cards[41] = new Card(suit: Suit.diamond, 4);
        //cards[42] = new Card(suit: Suit.spade, 4);
        //cards[43] = new Card(suit: Suit.club, 4);
        cards[44] = new Card(suit: Suit.joker, card: 0);
    }
}
public static class MyArrayExtensions
{
    /// <summary>
    /// Splits an array into several smaller arrays.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="size">The size of the smaller arrays.</param>
    /// <returns>An array containing smaller arrays.</returns>
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
        {
            yield return array.Skip(i * size).Take(size);
        }
    }
}