using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    public bool isHeld = false;
    public GameObject holdText;

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
        // Toggle held state
        isHeld = !isHeld;

        // Show or hide HOLD text
        if (holdText != null)
        {
            holdText.SetActive(isHeld);
        }

        //Debug.Log(gameObject.name + " isHeld: " + isHeld);
    }
}
