using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains all of the sound effects and music in the game */
public class AudioManager : MonoBehaviour
{
    //audio sources can play while other sounds are playing without interruption
    [Header("---Audio Sources---")]
    public AudioSource mainMusic;       //plays during gameplay.
    public AudioSource buildSound;      //sound for when a unit is building.

    [Header("---Audio Clips---")]
    public AudioClip bulletSfx;        //used by straight shooter and striker
    public AudioClip laserSfx;          //used by vertical shooter       


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
