using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MMurray.LastLine;

/* Displays an animated "BASE DEFENDED" text. Each letter fades in, then fades out in the same order after a duration. */
public class EndLevelText : MonoBehaviour
{
    public List<TextMeshProUGUI> text;
    public TextMeshProUGUI textPrefab;
    Coroutine animateText;
    // Start is called before the first frame update
    void Start()
    {
        float charPos = 40;
        float xOffset = 300;
        //find the end level text key and populate the list. Must count the number of characters and instantiate object for each one.
        string clearText = GetText("levelCompleted");
        for (int i = 0; i < clearText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = clearText[i].ToString();

            //set character's position and rotation. 
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            text.Add(textChar);
        }

        //StartCoroutine(AnimateClearText());
        //textPrefab.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        //set alpha of each char to 0
        foreach(TextMeshProUGUI c in text)
        {
            c.alpha = 0;
        }
    }

    void OnEnable()
    {
        textPrefab.gameObject.SetActive(false);
    }


    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowText()
    {
        gameObject.SetActive(true);
        animateText = StartCoroutine(AnimateClearText());
    }

    IEnumerator AnimateClearText()
    {
        /* each letter fades in one at a time. When one letter is fully visible, the next letter fades in. */

        yield return new WaitForSeconds(2);     //wait for any other UI or other game elements to complete

        for (int i = 0; i < text.Count; i++)
        {
            if (text[i].text.Equals(" ")) 
                continue;

            float currentAlpha = 0;
            while(text[i].alpha < 1)
            {
                text[i].alpha += 2 * Time.deltaTime;
                //Color charColor = text[i].color;
                //text[i].color = new Color(charColor.r, charColor.g, charColor.b, currentAlpha);
                yield return new WaitForSeconds(0.08f);
            }
        }

        //move to next level.
        //check for conditional trigger to activate dialogue
        yield return new WaitForSeconds(1);
        //gameObject.SetActive(false);
        GameManager gm = Singleton.instance.GameManager;
        bool triggerFound = false;
        int j = 0;
        while (!triggerFound && j < gm.dialogueManager.condTriggerList.Count)
        {
            if (gm.dialogueManager.condTriggerList[j].level == gm.currentLevel &&
                gm.dialogueManager.condTriggerList[j].condition == DialogueManager.ConditionalTrigger.Condition.LevelEnd)
            {
                triggerFound = true;
            }
            else
            {
                j++;
            }

        }


        if (triggerFound)
        {
            gm.triggerIndex = j;
            gm.SetGameState(gm.gameState = GameManager.GameState.DialogueEndOfLevel);
        }
        else
        {
            gm.SetGameState(gm.gameState = GameManager.GameState.GoToNextLevel);
        }
        //yield return null;
    }
}
