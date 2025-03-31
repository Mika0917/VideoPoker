using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    public GameObject[] cardObjects;         // References to Card1–Card5 GameObjects
    public Sprite[] cardSprites;             // All your 52 card sprites (assigned in Inspector)

    private List<int> usedIndices = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        DealNewHand();
    }

    public void DealNewHand()
    {
        usedIndices.Clear();  // Reset used cards

        for (int i = 0; i < cardObjects.Length; i++)
        {
            int randomIndex;

            // Ensure each card is unique (no repeats)
            do
            {
                randomIndex = Random.Range(0, cardSprites.Length);
            }
            while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);

            // Assign the sprite to the card’s SpriteRenderer
            SpriteRenderer sr = cardObjects[i].GetComponent<SpriteRenderer>();
            sr.sprite = cardSprites[randomIndex];
        }
    }
}
