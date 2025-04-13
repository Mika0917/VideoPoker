using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;    

public class CardDealer : MonoBehaviour
{
    public GameObject[] cardObjects;
    public Sprite[] cardSprites;

    public TMP_InputField[] betInputFields;

    private List<int> usedIndices = new List<int>();

    public TMPro.TextMeshProUGUI gameOverText;
    public TMPro.TextMeshProUGUI creditsText;
    public GameObject addCreditsButton;

    public UnityEngine.UI.Button betOneButton;
    public UnityEngine.UI.Button betMaxButton;

    public TextMeshProUGUI dealButtonText;

    public TextMeshProUGUI betDisplayText;

    private int credits = 100;
    private int creditsBet = 1;

    private enum GamePhase
    {
        WaitingToDeal,
        WaitingToDraw
    }

    private GamePhase currentPhase = GamePhase.WaitingToDeal;

    private Dictionary<string, int> payoutTable = new Dictionary<string, int>
    {
        { "Royal Flush", 250 },
        { "Straight Flush", 50 },
        { "Four of a Kind", 25 },
        { "Full House", 9 },
        { "Flush", 6 },
        { "Straight", 4 },
        { "Three of a Kind", 3 },
        { "Two Pair", 2 },
        { "Jacks or Better", 1 },
        { "No Win", 0 }
    };

    public void OnDealOrDrawButtonClicked()
    {
        if (currentPhase == GamePhase.WaitingToDeal)
        {
            DealNewHand();
            currentPhase = GamePhase.WaitingToDraw;
            dealButtonText.text = "Draw";
        }
        else if (currentPhase == GamePhase.WaitingToDraw)
        {
            DrawCards();
            currentPhase = GamePhase.WaitingToDeal;
            dealButtonText.text = "Deal";
        }
    }

    public void DealNewHand()
    {
        StartCoroutine(DealCardsSequentially());
    }

    private IEnumerator DealCardsSequentially()
    {

        if (creditsBet > credits)
        {
            creditsBet = credits;
            UpdateBetFieldUI();
        }

        usedIndices.Clear();

        betOneButton.interactable = false;
        betMaxButton.interactable = false;

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

            CardBehavior cb = cardObjects[i].GetComponent<CardBehavior>();
            if (cb != null)
            {
                cb.isHeld = false;
                cb.holdText.SetActive(false);
                cb.canHold = true;
            }

            yield return new WaitForSeconds(0.075f);
        }

        string result = EvaluateHand();
        int payout = payoutTable[result];

        if (payout > 0)
        {
            gameOverText.text = $"<size=50><b><color=#000000>Initial Deal: {result}</color></b></size>";
            gameOverText.alignment = TMPro.TextAlignmentOptions.Center;
            gameOverText.gameObject.SetActive(true);
        }
        else
        {
            gameOverText.gameObject.SetActive(false);
        }

    }


    public void DrawCards()
    {
        StartCoroutine(DrawCardsSequentially());
    }

    private IEnumerator DrawCardsSequentially()
    {

        for (int i = 0; i < cardObjects.Length; i++)
        {
            CardBehavior cb = cardObjects[i].GetComponent<CardBehavior>();
            if (cb != null && cb.isHeld)
                continue;

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, cardSprites.Length);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);

            SpriteRenderer sr = cardObjects[i].GetComponent<SpriteRenderer>();
            sr.sprite = cardSprites[randomIndex];

            yield return new WaitForSeconds(0.075f);
        }

        string result = EvaluateHand();

        int payout;
        if (result == "Royal Flush" && creditsBet == 5)
            payout = 4000;
        else
            payout = creditsBet * payoutTable[result];

        credits += payout - creditsBet;
        credits = Mathf.Max(credits, 0);
        creditsText.text = "Credits: " + credits;

        string popupText = "";

        if (payout > 0)
        {
            popupText += $"<b><size=50><color=black>{result}</color></size></b>\n";
            popupText += $"<b><size=50><color=blue>You won {payout} credits!</color></size></b>\n";
        }
        else
        {
            popupText += $"<b><size=50><color=black>{result}</color></size></b>\n";
            popupText += $"<b><size=50><color=black>No Payout</color></size></b>\n";
        }

        popupText += "<b><size=50><color=red>GAME OVER</color></size></b>";

        gameOverText.text = popupText;
        gameOverText.alignment = TMPro.TextAlignmentOptions.Center;
        gameOverText.gameObject.SetActive(true);

        foreach (GameObject card in cardObjects)
        {
            CardBehavior c = card.GetComponent<CardBehavior>();
            if (c != null)
            {
                c.isHeld = false;
                c.holdText.SetActive(false);
                c.canHold = false;
            }
        }

        CheckCredits();
    }



    private string EvaluateHand()
    {
        List<int> ranks = new List<int>();
        List<char> suits = new List<char>();    

        foreach (GameObject card in cardObjects)
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            string spriteName = sr.sprite.name;

            int rank = ParseRank(spriteName[0], spriteName.Length == 3 ? spriteName.Substring(0, 2) : spriteName.Substring(0, 1));
            char suit = spriteName[spriteName.Length - 1];

            ranks.Add(rank);
            suits.Add(suit);
        }

        ranks.Sort();

        bool isFlush = suits.TrueForAll(s => s == suits[0]);
        bool isStraight = IsStraight(ranks);
        Dictionary<int, int> rankCounts = GetRankCounts(ranks);

        if (isFlush && ranks.SequenceEqual(new List<int> { 10, 11, 12, 13, 14 }))
            return "Royal Flush";
        if (isFlush && isStraight)
            return "Straight Flush";
        if (rankCounts.ContainsValue(4))
            return "Four of a Kind";
        if (rankCounts.ContainsValue(3) && rankCounts.ContainsValue(2))
            return "Full House";
        if (isFlush)
            return "Flush";
        if (isStraight)
            return "Straight";
        if (rankCounts.ContainsValue(3))
            return "Three of a Kind";
        if (CountPairs(rankCounts) == 2)
            return "Two Pair";
        if (rankCounts.Any(pair => pair.Value == 2 && pair.Key >= 11))
            return "Jacks or Better";

        return "No Win";
    }

    private int ParseRank(char firstChar, string fullRank)
    {
        switch (fullRank)
        {
            case "A": return 14;
            case "K": return 13;
            case "Q": return 12;
            case "J": return 11;
            case "10": return 10;
            default: return int.Parse(fullRank); 
        }
    }

    private bool IsStraight(List<int> sortedRanks)
    {
         if (sortedRanks.SequenceEqual(new List<int> { 2, 3, 4, 5, 14 })) 
         {
            return true;
         }

        for (int i = 0; i < sortedRanks.Count - 1; i++)
        {
            if (sortedRanks[i + 1] != sortedRanks[i] + 1)
                return false;
        }

        return true;
    }

    private Dictionary<int, int> GetRankCounts(List<int> ranks)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (int rank in ranks)
        {
            if (!counts.ContainsKey(rank))
                counts[rank] = 1;
            else
                counts[rank]++;
        }
        return counts;
    }

    private int CountPairs(Dictionary<int, int> rankCounts)
    {
        int count = 0;
        foreach (int val in rankCounts.Values)
        {
            if (val == 2)
                count++;
        }
        return count;
    }

    public void DealTest()
    {
    string[] testHand = { "2H", "2C", "JH", "KH", "AH" }; 

    for (int i = 0; i < cardObjects.Length; i++)
    {

        Sprite match = cardSprites.FirstOrDefault(s => s.name == testHand[i]);
        if (match != null)
        {
            cardObjects[i].GetComponent<SpriteRenderer>().sprite = match;
        }

        CardBehavior cb = cardObjects[i].GetComponent<CardBehavior>();
        if (cb != null)
        {
            cb.isHeld = false;
            cb.holdText.SetActive(false);
        }
    }

    string result = EvaluateHand();
    Debug.Log("Test Result: " + result);
}

    public void BetOne()
    {
        if (creditsBet < Mathf.Min(5, credits))
        {
            creditsBet += 1;
        }
        else
        {
            creditsBet = 1;
        }
        UpdateBetFieldUI();
    }


    public void BetMax() 
    {
        creditsBet = Mathf.Min(5, credits);
        UpdateBetFieldUI();
    }

    private void UpdateBetFieldUI()
    {
        for (int i = 0; i < betInputFields.Length; i++)
        {
            Image bgImage = betInputFields[i].GetComponent<Image>();
            if (i == creditsBet - 1)
                bgImage.color = new Color32(255, 0, 0, 255);
            else
                bgImage.color = new Color32(150, 150, 150, 255);
        }
        if (betDisplayText != null)
        {
            betDisplayText.text = "Bet: " + creditsBet;
        }
    }

    public void OnAddCreditsClicked()
    {
        credits = 100;
        creditsBet = 1;
        creditsText.text = "Credits: " + credits;
        UpdateBetFieldUI();
        CheckCredits();
    }

    private void CheckCredits()
    {
        if (creditsBet > credits)
        {
            creditsBet = credits;
            UpdateBetFieldUI();
        }

        bool hasCredits = credits > 0;

        betOneButton.interactable = hasCredits;
        betMaxButton.interactable = hasCredits;
        dealButtonText.transform.parent.GetComponent<Button>().interactable = hasCredits;

        addCreditsButton.SetActive(!hasCredits);
    }


}

