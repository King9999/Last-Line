using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using MMurray.GenericCode;
using UnityEngine.SceneManagement;
using LoLSDK;

/* Contains functionality for the Music, TTS and Exit Game buttons. */
public class SystemButton : MonoBehaviour
{
    [SerializeField]private string key;
    public enum Options { Music, TTS, ExitGame, Help}
    public Options option;
    
    public void OnMouseClick()
    {
        GameManager gm = Singleton.instance.GameManager;
        Menu menu = Singleton.instance.Menu;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClick, audio.soundVolume);

        //check which button was clicked.
        switch (option)
        {
            case Options.Music:
                Singleton.instance.musicEnabled = !Singleton.instance.musicEnabled;
                Logger.DebugLogFormat("Music enabled: {0}", Singleton.instance.musicEnabled);
                //TODO: button graphics should change to reflect state
                if (!Singleton.instance.musicEnabled)
                {
                    gm.DisplayAlertWithDuration(GetText("musicOff"));
                }
                else
                {
                    gm.DisplayAlertWithDuration(GetText("musicOn"));
                }

                gm.PlayMusic(Singleton.instance.musicEnabled);

                //update button sprites
                menu.UpdateSystemButtonSprites();
                Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
                break;

            case Options.TTS:
                Singleton.instance.ttsEnabled = !Singleton.instance.ttsEnabled;
                Logger.DebugLogFormat("TTS enabled: {0}", Singleton.instance.ttsEnabled);
                if (!Singleton.instance.ttsEnabled)
                {
                    gm.DisplayAlertWithDuration(GetText("ttsOff"));
                }
                else
                {
                    gm.DisplayAlertWithDuration(GetText("ttsOn"));
                }
                //update button sprites
                menu.UpdateSystemButtonSprites();
                Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
                break;

            case Options.ExitGame:
                gm.PlayMusic(false);
                //save current state before changing screen
                Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
                ScreenFade sf = Singleton.instance.ScreenFade;
                sf.ChangeSceneFadeOut("Title");
                break;
        }
        
    }

    public void OnMouseHoverEnter()
    {
        Logger.DebugLog("Hovering over system button");
        Menu menu = Singleton.instance.Menu;
        if (menu.systemTooltip.gameObject.activeSelf)
            return;

        menu.systemTooltip.ShowTooltip(true, GetText(key), this);
    }

    public void OnMouseHoverExit()
    {
        Logger.DebugLog("Leaving system button");
        Menu menu = Singleton.instance.Menu;
        menu.systemTooltip.ShowTooltip(false);
    }

    public void OnMouseHoverEnterInputHelp()
    {
        Menu menu = Singleton.instance.Menu;
        if (menu.inputHelpTooltip.gameObject.activeSelf)
            return;

        menu.inputHelpTooltip.ShowTooltip(true, GetText(key), this);
    }

    public void OnMouseHoverExitInputHelp()
    {
        Menu menu = Singleton.instance.Menu;
        menu.inputHelpTooltip.ShowTooltip(false);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
