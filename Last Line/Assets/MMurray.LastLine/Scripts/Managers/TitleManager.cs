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
    [SerializeField]private Button startButton;
    [SerializeField]private Button continueButton;
    [SerializeField]private Button creditsButton;
    [SerializeField]private TextMeshProUGUI startButtonText, continueButtonText, creditsButtonText;
    [SerializeField]private TextMeshProUGUI version;

    // Start is called before the first frame update
    void Start()
    {
        Logger.DebugLogFormat("Screen dimensions: {0}, {1}", Screen.width, Screen.height);
        //Must call the screen fade since the game begins with a black background on init.
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();

        //button text setup
        startButtonText.text = GetText("startButton");
        continueButtonText.text = GetText("continueButton");
        creditsButtonText.text = GetText("creditsButton");
        version.text = GetText("versionNumber");

        /***save state check***/
        LOLSDK.Instance.LoadState<GameData>(state =>
        {
            if (state != null)
            {
                Singleton.instance.saveStateFound = true;
                Singleton.instance.gameData.level = state.data.level;       //will need this for when game manager loads the level.
                Singleton.instance.gameData.in_minigame = state.data.in_minigame;
                Singleton.instance.inMinigame = Singleton.instance.gameData.in_minigame;

                //saved universal settings are applied here
                Singleton.instance.ttsEnabled = state.data.tts_enabled;
                Singleton.instance.musicEnabled = state.data.music_enabled;

                continueButton.gameObject.SetActive(true);
                Logger.DebugLogWarning("Save state found and loaded.");
            }
            else
            {
                //hide continue button
                continueButton.gameObject.SetActive(false);
                Logger.DebugLogWarning("Save state not found");
            }
           
        });
    }


    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void StartButtonClicked()
    {
        ScreenFade sf = Singleton.instance.ScreenFade;

        if (sf.coroutineOn)
            return;

        //the save state is deleted when starting a new game.
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClickStartGame, audio.soundVolume);
        PlayerPrefs.DeleteAll();
        Singleton.instance.musicEnabled = true;
        Singleton.instance.ttsEnabled = true;
        Singleton.instance.saveStateFound = false;
        sf.ChangeSceneFadeOut("Game");
        //Singleton.instance.inMinigame = true;
        //sf.ChangeSceneFadeOut("MiniGame");
    }

    public void ContinueButtonClicked()
    {
        ScreenFade sf = Singleton.instance.ScreenFade;

        if (sf.coroutineOn)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClickStartGame, audio.soundVolume);

        //TODO: Check which level we're on. If it's level 3 or 6, load minigame.
        Logger.DebugLogFormat("current level: {0}", Singleton.instance.gameData.level);
        Logger.DebugLogFormat("In minigame: {0}", Singleton.instance.gameData.in_minigame);
        if (Singleton.instance.gameData.level == 3 || Singleton.instance.gameData.level == 6)
            sf.ChangeSceneFadeOut("MiniGame");
        else
            sf.ChangeSceneFadeOut("Game");
    }

    public void CreditsButtonClicked()
    {
        ScreenFade sf = Singleton.instance.ScreenFade;

        if (sf.coroutineOn)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClickStartGame, audio.soundVolume);
        sf.ChangeSceneFadeOut("Credits");
    }


}
