using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Collections;
using UnityEngine;

/* crystals grant money to the player if they're targeted. Only 1 is displayed at a time. If the player reaches a threshold
by accumulating money from destroying units, then the crystal will appear at a random empty point. If the player doesn't grab the crystal
before the next one appears, the old crystal is replaced with the new one. */
public class Crystal : MonoBehaviour
{
    [SerializeField]private int moneyValue;
    SpriteRenderer sr;
    Coroutine animateCrystal;
    bool coroutineRunning;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void SetMoneyValue(int amount)
    {
        if (amount < 0)
            return;

        moneyValue = amount;
    }

    public int GetMoneyValue()
    {
        return moneyValue;
    }

    void Update()
    {
        if (!coroutineRunning)
        {
            coroutineRunning = true;
            animateCrystal = StartCoroutine(Animate());
        }
    }

    //float unit up and down. Only the sprite itself moves to prevent hitboxes from being affected.
    IEnumerator Animate()
    {
        float yDestination = 0.1f;
        float moveSpeed = 0.1f;
        Vector2 origPos = sr.transform.position;

        //float up
        while (sr.transform.position.y < origPos.y + yDestination)
        {
            Vector2 crystalPos = sr.transform.position;
            sr.transform.position = new Vector2(crystalPos.x, crystalPos.y + moveSpeed * Time.deltaTime);
            yield return null;
        }

        //float down
        while (sr.transform.position.y > origPos.y)
        {
            Vector2 crystalPos = sr.transform.position;
            sr.transform.position = new Vector2(crystalPos.x, crystalPos.y - moveSpeed * Time.deltaTime);
            yield return null;
        }
        sr.transform.position = origPos;
        coroutineRunning = false;
    }
}
