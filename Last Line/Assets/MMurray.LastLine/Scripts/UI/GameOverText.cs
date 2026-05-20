using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MMurray.LastLine;

//displays "BASE DESTROYED" when base is destroyed.
public class GameOverText : MonoBehaviour
{
    [SerializeField]private List<TextMeshProUGUI> gameOverText;
    [SerializeField]TextMeshProUGUI textPrefab;
    Coroutine animateText = null;

    // Start is called before the first frame update
    /*void Start()
    {
        float charPos = 40;
        float xOffset = 300;
        //find the end level text key and populate the list. Must count the number of characters and instantiate object for each one.
        string clearText = GetText("gameOver");
        for (int i = 0; i < clearText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = clearText[i].ToString();

            //set character's position and rotation. 
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            textChar.transform.localScale = Vector2.zero;         //invisible by default
            gameOverText.Add(textChar);
        }
    }*/
    
    //must be called by GameManager on start
    public void SetupText()
    {
        GameManager gm = Singleton.instance.GameManager;
        float charPos = 40 * gm.screenWidthScale;
        float xOffset = 300 * gm.screenWidthScale;
        float yOffset = 50;
        //find the end level text key and populate the list. Must count the number of characters and instantiate object for each one.
        string clearText = GetText("gameOver");
        for (int i = 0; i < clearText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = clearText[i].ToString();

            //set character's position and rotation. 
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + yOffset, 0);
            textChar.transform.localScale = Vector2.zero;         //invisible by default
            gameOverText.Add(textChar);
        }
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowText()
    {
        animateText = StartCoroutine(AnimateText());
    }

    //each char increases in scale until it reaches 1.
    IEnumerator AnimateText()
    {
        yield return new WaitForSeconds(1);     //wait for any other UI or other game elements to complete

        //fade in
        float scaleSpeed = 100;                 //fade in/out rate
        float duration = 0.03f;
        for (int i = 0; i < gameOverText.Count; i++)
        {
            if (gameOverText[i].text.Equals(" ")) 
                continue;

            while(gameOverText[i].transform.localScale.x < 1)
            {
                Vector2 textScale = gameOverText[i].transform.localScale;
                gameOverText[i].transform.localScale = new Vector2(textScale.x + scaleSpeed * Time.deltaTime, textScale.y + scaleSpeed * Time.deltaTime);
                yield return new WaitForSeconds(duration);
            }
            //correct the scale back to 1
            gameOverText[i].transform.localScale = Vector2.one;

        }

        yield return new WaitForSeconds(2);
        //set alpha of each char to 0
        foreach(TextMeshProUGUI c in gameOverText)
        {
            c.transform.localScale = Vector2.zero;
        }

        //go to next game state. Check for any dialogue to show
        GameManager gm = Singleton.instance.GameManager;
        DialogueManager dm = Singleton.instance.DialogueManager;
        bool dialogueFound = false;
        int j = 0;
        while(!dialogueFound && j < dm.gameOverDialogueList.Count)
        {
            if (gm.currentLevel == dm.gameOverDialogueList[j].level)
            {
                dialogueFound = true;
                gm.triggerIndex = j;
            }
            else
            {
                j++;
            }
        }

        if (dialogueFound)
        {
            gm.SetGameState(gm.gameState = GameManager.GameState.DialogueGameOver);
        }
        else
        {
            //restart level
            Singleton.instance.ScreenFade.LoadLevelFadeOut(gm.currentLevel);
        }
    }
}
