using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;

public class GameManager : MonoBehaviour
{
    public Vector2 coordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;
        //enteringValueX = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
