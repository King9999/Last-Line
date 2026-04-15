using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using UnityEngine.UI;
using TMPro;
using MMurray.LastLine;
using MMurray.GenericCode;

//scene used to end the game when player completes all of the levels.
public class EndScreenManager : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI exitText;
    [SerializeField]private TextMeshProUGUI missionCompleteText, thanksText;  //says "MISSION ACCOMPLISHED" followed by "Thank you for playing"

    // Start is called before the first frame update
    void Start()
    {
        exitText.text = GetText("exitButtonText");
        missionCompleteText.text = GetText("missionComplete");
        thanksText.text = GetText("thanksForPlaying");

        //Must call the screen fade since the game begins with a black background on init.
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();
    }


    public void EndGame()
    {
        //LOLSDK.Instance.SubmitProgress(0, currentLevel, maxLevels);
        LOLSDK.Instance.CompleteGame();
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
