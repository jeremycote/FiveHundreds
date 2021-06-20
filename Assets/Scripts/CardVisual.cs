using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    public Card card;
    private GameController gameController = null;

    public void CardClicked()
    {
        if (gameController == null)
        {
            gameController = GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<GameController>();
        }
        Hand hand = gameObject.GetComponentInParent<Hand>();

        if (hand != null)
        {
            //Debug.Log("Hand is not null");
            int returnCode = gameController.PlayCard(card: card, hand: hand, isAddition: false);
            //Debug.Log(returnCode + " is the return code");
            if (returnCode == 1)
            {
                //Debug.Log("Moving card");
                GameObject discard = GameObject.FindGameObjectsWithTag("Discard")[0];
                TransferCard(discard, true);
            }
            else if (returnCode == 2)
            {
                GameObject kitty = GameObject.FindGameObjectsWithTag("Kitty")[0];
                TransferCard(kitty, true);
            }

        } else
        {
            //Debug.Log("Hand is null");
            Kitty kitty = gameObject.GetComponentInParent<Kitty>();
            hand = gameController.hands[gameController.GetCurrentPlayer()];
            int returnCode = gameController.PlayCard(card: card, hand: hand, isAddition: true);

            if (kitty != null)
            {
                if (returnCode == 3)
                {
                    TransferCard(hand.gameObject, false);

                }
                //Card is inside the kitty
            }
        }
    }
    public void TransferCard(GameObject destination, bool isPlay)
    {
        gameObject.transform.SetParent(destination.transform);
        StartCoroutine(moveCard(transform: gameObject.transform, isPlay: isPlay));
        gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
    }
    public IEnumerator moveCard(Transform transform, bool isPlay)
    {
        float totalMovementTime = gameController.GetAnimSpeed(); //the amount of time you want the movement to take
        float currentMovementTime = 0f;//The amount of time that has passed
        while (Vector3.Distance(transform.localPosition, new Vector3(0, 0, 0)) > 0)
        {
            currentMovementTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 0, 0), currentMovementTime / totalMovementTime);
            //transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, 0, 0, 0), Time.time * totalMovementTime);
            yield return null;
        }
        
        gameController.CardPlayedCallback();

        if (isPlay == false)
        {
            Destroy(this.gameObject);
        }

    }
}
