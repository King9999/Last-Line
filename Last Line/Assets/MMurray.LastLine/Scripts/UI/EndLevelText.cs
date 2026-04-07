using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* Displays an animated "BASE DEFENDED" text. Each letter fades in, then fades out in the same order after a duration. */
public class EndLevelText : MonoBehaviour
{
    public List<TextMeshProUGUI> text;
    public TextMeshProUGUI textPrefab;
    Coroutine animateText;
    // Start is called before the first frame update
    void Start()
    {
        float charPos = 60;
        float xOffset = 300;
        //find the end level text key and populate the list. Must count the number of characters and instantiate object for each one.
        string clearText = GetText("levelCompleted");
        for (int i = 0; i < clearText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = clearText[i].ToString();

            //set character's position and rotation. Each character starts at a 90 degree angle on the Y axis
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            //textChar.transform.Rotate(0, 90, 0);
            text.Add(textChar);
        }

        //StartCoroutine(AnimateClearText());
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowText()
    {
        animateText = StartCoroutine(AnimateClearText());
    }

    IEnumerator AnimateClearText()
    {
        yield return null;
    }
}
