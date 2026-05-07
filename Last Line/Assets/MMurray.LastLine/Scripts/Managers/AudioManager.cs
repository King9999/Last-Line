using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains all of the sound effects and music in the game */
public class AudioManager : MonoBehaviour
{
    public float soundVolume;
    //audio sources can play while other sounds are playing without interruption
    [Header("---Audio Sources---")]
    public AudioSource mainMusic;       //plays during gameplay.
    public AudioSource buildSound;      //sound for when a unit is building.
    public AudioSource soundSource;     //for general sounds
    public AudioSource trackingBeamShot;

    [Header("---Audio Clips---")]
    public AudioClip bulletSfx;        //used by straight shooter and striker
    public AudioClip laserSfx;          //used by vertical shooter
    public AudioClip buttonClick;
    public AudioClip buttonClickStartGame;  //used when player starts game or continues from a save state
    public AudioClip targetCursorSound;
    public AudioClip baseExplode;
    public AudioClip unitExplode;
    public AudioClip trackingBeamCharging;
                 


    // Start is called before the first frame update
    void Start()
    {
        soundSource.volume = soundVolume;
    }

    public void StopAllSounds()
    {
        mainMusic.Stop();
        buildSound.Stop();
        soundSource.Stop();
    }
}
