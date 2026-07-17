using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* Displays text of money awarded to player when an enemy is defeated. Whenever the text is displayed, it animates upward
for a duration. */
public class MoneyText : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI moneyText;
    Coroutine animateMoneyCoroutine = null;
    float animateTime = 1.5f;
    float textAlpha;
    /*void OnEnable()
    {
        moneyText.alpha = 1;
        animateMoneyCoroutine = StartCoroutine(AnimateMoneyText(moneyText.text, animateTime));
    }*/

    //if useWorldToScreenPoint is false, that means the position is already using screen position and does not need to be converted.
    public void ShowMoneyRewardText(int amount, Vector2 position, bool useWorldToScreenPoint = true)
    {
        gameObject.SetActive(true);
        moneyText.text = string.Format("+{0}", amount);
        moneyText.transform.position = useWorldToScreenPoint == true ? Camera.main.WorldToScreenPoint(position) : position;
        //moneyText.alpha = 1;
        animateMoneyCoroutine = StartCoroutine(AnimateMoneyText(animateTime));
    }
   
    //money text moves upward for a duration, then alpha fades gradually.
    IEnumerator AnimateMoneyText(float duration)
    {
        float time = 0;
        while (time < duration)
        {
           time += Time.deltaTime;

           Vector2 textPos = moneyText.transform.position;
           float yPos = textPos.y + (20 * Time.deltaTime);
           moneyText.transform.position = new Vector2(textPos.x, yPos);
           yield return null;
        }
        //moneyText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
