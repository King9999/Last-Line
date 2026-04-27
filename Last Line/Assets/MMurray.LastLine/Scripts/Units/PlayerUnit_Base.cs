using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* The base is a special unit that the player must protect. If the base is destroyed, the game is over. 
The base's health bar is larger than other units to highlight the unit's importance to the player. */
public class PlayerUnit_Base : PlayerUnit
{
    [SerializeField]ParticleSystem baseParticles;       //displays when base is hit. Colour changes as base is damaged.
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

    //update particle color based on base's hit points.
    public void PlayParticles()
    {
        var particleMain = baseParticles.main;

        if (hitPoints >= maxHitPoints - 3)
        {
            particleMain.startColor = Color.white;
        }
        else if (hitPoints >= maxHitPoints - 6)
        {
            particleMain.startColor = Color.yellow;
        }
        else
        {
            particleMain.startColor = Color.red;
        }

        baseParticles.Play();
    }
}
