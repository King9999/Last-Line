using System.Collections;
using System.Collections.Generic;
using MMurray.GenericCode;
using MMurray.LastLine;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LoLSDK;
using TMPro;

/* Manager for the title screen. */
public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Must call the screen fade since the game begins with a black background on init.
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();

        /***save state check***/
        LOLSDK.Instance.LoadState<GameData>(state =>
        {
            if (state != null)
            {
                Singleton.instance.saveStateFound = true;
                Singleton.instance.gameData.level = state.data.level;       //will need this for when game manager loads the level.

                //saved universal settings are applied here
                Singleton.instance.ttsEnabled = state.data.tts_enabled;
                Singleton.instance.musicEnabled = state.data.music_enabled;
                //continueButton.gameObject.SetActive(true);
                Debug.LogWarning("Save state found and loaded.");
            }
            else
            {
                //hide continue button
                //continueButton.gameObject.SetActive(false);
                Debug.LogWarning("Save state not found");
            }
           
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
