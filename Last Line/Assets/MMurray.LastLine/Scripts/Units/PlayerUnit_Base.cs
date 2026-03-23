using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* The base is a special unit that the player must protect. If the base is destroyed, the game is over. 
The base's health bar is larger than other units to highlight the unit's importance to the player. */
public class PlayerUnit_Base : PlayerUnit
{
    private const float checkTime = 0.5f;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    /*void Update()
    {
        //don't want to check every frame if base is destroyed to help with performance.
        if (Time.time > timer + checkTime)
        {
            timer = Time.time;
            if (BaseDestroyed())
            {
                //set state to game over
                Logger.DebugLog("Base destroyed");
            }
        }
    }*/

    bool BaseDestroyed()
    {
        return hitPoints <= 0;
    }
}
