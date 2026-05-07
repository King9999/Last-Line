using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.LastLine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreditsManager : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI backButtonText;

    [Header("---Credits---")]
    [SerializeField]private TextMeshProUGUI mainCreditText;
    [SerializeField]private TextMeshProUGUI gameDesignText;
    [SerializeField]private TextMeshProUGUI programmerText;
    [SerializeField]private TextMeshProUGUI artText;
    [SerializeField]private TextMeshProUGUI uiText;
    [SerializeField]private TextMeshProUGUI musicText;
    [SerializeField]private TextMeshProUGUI soundText;
    [SerializeField]private TextMeshProUGUI disclaimerText;     //disclaimer that no AI was used. Shouldn't have to state this

    // Start is called before the first frame update
    void Start()
    {
        //Must call the screen fade since the game begins with a black background on init.
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();

        UpdateLanguage();
    }

    void UpdateLanguage()
    {
        backButtonText.text = GetText("backButton");
        mainCreditText.text = GetText("credit_main");
        gameDesignText.text = GetText("credit_gameDesign");
        programmerText.text = GetText("credit_program");
        artText.text = GetText("credit_art");
        uiText.text = GetText("credit_ui");
        musicText.text = GetText("credit_music");
        soundText.text = GetText("credit_sfx");
        disclaimerText.text = GetText("disclaimer");
    }

    public void BackButtonClicked()
    {
        //return to title
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClick, audio.soundVolume);
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.ChangeSceneFadeOut("Title");
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

}
