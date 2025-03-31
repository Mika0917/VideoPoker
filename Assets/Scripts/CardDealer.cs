using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    public GameObject[] cardObjects;         // References to Card1–Card5 GameObjects
    public Sprite[] cardSprites;             // All your 52 card sprites (assigned in Inspector)

    private List<int> usedIndices = new List<int>();

    // Game state tracking
    private enum GamePhase
    {
        WaitingToDeal,
        WaitingToDraw
    }

    private GamePhase currentPhase = GamePhase.WaitingToDeal;

    // Called when the button is clicked
    public void OnDealOrDrawButtonClicked()
    {
        if (currentPhase == GamePhase.WaitingToDeal)
        {
            DealNewHand();
            currentPhase = GamePhase.WaitingToDraw;
        }
        else if (currentPhase == GamePhase.WaitingToDraw)
        {
            DrawCards();
            currentPhase = GamePhase.WaitingToDeal;
        }
    }

    public void DealNewHand()
    {
        usedIndices.Clear();

        for (int i = 0; i < cardObjects.Length; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, cardSprites.Length);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);

            SpriteRenderer sr = cardObjects[i].GetComponent<SpriteRenderer>();
            sr.sprite = cardSprites[randomIndex];

            // Reset hold
            CardBehavior cb = cardObjects[i].GetComponent<CardBehavior>();
            if (cb != null)
            {
                cb.isHeld = false;
                cb.holdText.SetActive(false);
            }
        }
    }

    public void DrawCards()
    {
        usedIndices.Clear();

        for (int i = 0; i < cardObjects.Length; i++)
        {
            CardBehavior cb = cardObjects[i].GetComponent<CardBehavior>();
            if (cb != null && cb.isHeld)
            {
                continue; // Skip held cards
            }

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, cardSprites.Length);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);

            SpriteRenderer sr = cardObjects[i].GetComponent<SpriteRenderer>();
            sr.sprite = cardSprites[randomIndex];
        }
    }
}