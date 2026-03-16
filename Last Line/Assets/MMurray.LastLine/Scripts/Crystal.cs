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
}
