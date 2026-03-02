using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MMurray.LastLine;
using LoLSDK;

/* Consists of a window and text. Dialogue appears at certain points. Uses keys to call dialogue. */
public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    [SerializeField]private TextMeshProUGUI promptText;
    [SerializeField]private const float textSpeed = 0.032f;
    bool canAdvanceDialogue;                //this must be true before player can press Space to advnace text.
    public int displayIndex;            //the index of the dialogue to display the image/video.
    public List<string> dialogueKeyList;    //each dialogue can have multiple batches of text to display.
    int currentKey;                     //iterator for dialogueKeyList
    [SerializeField]private float promptTimer;           //how much time must pass before player is prompted to go to next dialogue.
    // Start is called before the first frame update
    void Start()
    {
        canAdvanceDialogue = false;
        currentKey = 0;
    }


    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowDialogue(string key)
    {
        gameObject.SetActive(true);

        dialogueText.text = GetText(key);


        Logger.DebugLogFormat("Dialogue text key is {0}", key);
        Logger.DebugLogFormat("TTS State: {0}", Singleton.instance.ttsEnabled);

        if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(key);
            Logger.DebugLog("TTS is playing");
        }

        StartCoroutine(AnimateText(textSpeed, dialogueText.text));
    }

    //Displays text one letter at a time. Should not run again once the text is fully displayed.
    IEnumerator AnimateText(float scrollSpeed, string textToAnimate)
    {
        List<string> copy = new List<string>();
        int i = 0;
        string p = "";
        promptText.text = "";
        while (i < textToAnimate.Length)
        {
            //if there's a color tag, the entire tag must be treated as one character so the entire tag is displayed at once.
            if (textToAnimate.Substring(i, 1).Equals("<"))
            {
                //keep incrementing i until we reach the end of tag
                string tag = "";
                do
                {
                    tag += textToAnimate.Substring(i, 1);
                    i++;
                }
                while (!textToAnimate.Substring(i, 1).Equals(">"));
                tag += textToAnimate.Substring(i++, 1); //adding the >
                copy.Add(tag);
            }
            else
            {
                p = textToAnimate.Substring(i, 1);
                copy.Add(p);
                //Debug.Log(textToAnimate.Substring(i, 1)); 
                i++;
            }
        }


        dialogueText.text = "";
        i = 0;
        while (i < copy.Count)
        {
            dialogueText.text += copy[i];
            i++;
            yield return new WaitForSeconds(scrollSpeed);
        }

        //show prompt to close dialogue box.
        yield return new WaitForSeconds(promptTimer);
        yield return ShowPrompt();

    }

    //TODO: NEED TO ADD CODE TO ADVANCE DIALOGUE IF THERE'S MORE TO SHOW!


    IEnumerator ShowPrompt()
    {
        promptText.text = GetText("dialoguePrompt");
        canAdvanceDialogue = true;
        yield return null;
    }

    void OnAdvance()
    {
        
    }


}
