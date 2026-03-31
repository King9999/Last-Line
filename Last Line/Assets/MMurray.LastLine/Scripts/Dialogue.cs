using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MMurray.LastLine;
using LoLSDK;
using System;

/* Consists of a window and text. Dialogue appears at certain points. Uses keys to call dialogue. */
public class Dialogue : MonoBehaviour
{
    public int dialogueId;            //matches the corresponding dialogue ID in a conditional trigger. See Dialogue Manager script.
    public TextMeshProUGUI dialogueText;
    [SerializeField]private TextMeshProUGUI promptText;
    [SerializeField]private float textSpeed = 0.032f;
    bool canAdvanceDialogue;                //this must be true before player can press Space to advnace text.
    public int displayIndex;            //the index of the dialogue to display the image/video.
    public List<DialogueKeyList> dialogueKeyList;    //each dialogue can have multiple batches of text to display.
    int currentKey;                     //iterator for dialogueKeyList
    [SerializeField]private float promptTimer;           //how much time must pass before player is prompted to go to next dialogue.
    private List<ArrowGuide> arrows;        //reference to any active arrow guides on screen. Used to disable them when advancing dialogue.

    private Coroutine animateTextCoroutine = null;

    [Serializable]
    public struct DialogueKeyList
    {
        public string key;          //the key linked to the text in language JSON
        public bool showArrowGuides;    //if true, arrow guides are shown.
    }
    // Start is called before the first frame update
    void Start()
    {
        canAdvanceDialogue = false;
        currentKey = 0;
        //arrows = new List<ArrowGuide>();
        promptTimer = 1;
    }

    void Update()
    {
        //check for input
        if (canAdvanceDialogue && InputPressed())
        {
            Logger.DebugLog("Advancing Dialogue");
            AdvanceDialogue();
        }
    }

    bool InputPressed()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0);
    }


    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowDialogue(DialogueKeyList keyList)
    {
        gameObject.SetActive(true);

        dialogueText.text = GetText(keyList.key);


        Logger.DebugLogFormat("Dialogue text key is {0}", keyList.key);
        Logger.DebugLogFormat("TTS State: {0}", Singleton.instance.ttsEnabled);

        if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(keyList.key);
            Logger.DebugLog("TTS is playing");
        }

        //check if arrow guides need to be displayed.
        if (keyList.showArrowGuides == true)
        {
            ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
            int i = 0;
            bool keyFound = false;
            while (!keyFound && i < am.arrowGuides.Count)
            {
                if (am.arrowGuides[i].key.Equals(keyList.key))
                {
                    keyFound = true;
                    //arrows = new List<ArrowGuide>();    //keep reference to these arrows.
                    foreach (ArrowGuide arrow in am.arrowGuides[i].arrowList)
                    {
                        arrow.PointArrow(arrow.arrowLocation, arrow.direction);
                        //arrows.Add(arrow);
                        am.activeArrows.Add(arrow);
                    }
                }
                else
                {
                    i++;
                }
            }
        }

        animateTextCoroutine = StartCoroutine(AnimateText(textSpeed, dialogueText.text));
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

    //check for mouse or keyboard input to advance or close dialogue.
    void AdvanceDialogue()
    {
        canAdvanceDialogue = false;
        currentKey++;

        //hide any arrow guides that were shown
        /*ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
        if (am.activeArrows.Count > 0)
        {
            for (int i = 0; i < am.activeArrows.Count; i++)
            {
                am.activeArrows[i].gameObject.SetActive(false);
                am.activeArrows.Remove(am.activeArrows[i]);
                i--;
            }
        }
        if (arrows.Count > 0)
        {
            foreach(ArrowGuide arrow in arrows)
            {
                arrow.gameObject.SetActive(false);
            }
            arrows.Clear();
        }*/

        if (currentKey >= dialogueKeyList.Count)
        {
            //close dialogue
            Singleton.instance.DialogueManager.dialogueOpen = false;
            currentKey = 0;
            gameObject.SetActive(false);
            Logger.DebugLog("Closing Window");

            //TODO: Check game state and proceed depending on what it is.
            GameManager gm = Singleton.instance.GameManager;
            if (gm.inTutorial)
            {
                //gm.SetGameState(gm.tutorialState = GameManager.TutorialState.Normal);
                switch(gm.tutorialState)
                {
                    case GameManager.TutorialState.DialogueStartOfLevel:
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.Normal);
                        break;

                    case GameManager.TutorialState.UnitSelectX:
                    case GameManager.TutorialState.UnitSelectY:
                        //the dialogue box should close but we remain on this state so the player can
                        //choose the X and Y value.
                        break;
                    
                    case GameManager.TutorialState.ActivateAbility:
                        //activating second unit's ability
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.ActivateAbilitySelectX);
                        break;

                    case GameManager.TutorialState.DialogueEnemyIncoming:
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.DeployEnemyUnits);
                        break;

                    case GameManager.TutorialState.DialogueEndOfLevel:
                        //move to next level
                        gm.SetGameState(gm.gameState = GameManager.GameState.GoToNextLevel);
                        break;
                    
                }
                //gm.SetGameState(gm.tutorialState = gm.previousTutorialState);
            }
            else
                gm.SetGameState(gm.gameState = GameManager.GameState.Normal);
            
        }
        else
        {
            //go to next dialogue
            ShowDialogue(dialogueKeyList[currentKey]);
        }
    }

}
