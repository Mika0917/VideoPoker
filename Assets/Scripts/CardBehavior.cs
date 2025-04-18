using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    public bool isHeld = false;
    public GameObject holdText;

    [HideInInspector] public bool canHold = false;

    void Start()
    {
        isHeld = false;
        if (holdText != null)
        {
            holdText.SetActive(false);
        }
    }



    private void OnMouseDown()
    {

        if (!canHold) return;

        isHeld = !isHeld;

        if (holdText != null)
        {
            holdText.SetActive(isHeld);
        }

    }
}