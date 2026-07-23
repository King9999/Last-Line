using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MMurray.LastLine;

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
        float crystalOffset;       //used during minigame to display crystal object beside money text.
        MinigameManager mm = Singleton.instance.MinigameManager;
        if (mm != null)
        {
            mm.crystalObject.gameObject.SetActive(true);
        }
        while (time < duration)
        {
           time += Time.deltaTime;

           Vector2 textPos = moneyText.transform.position;
           float yPos = textPos.y + (20 * Time.deltaTime);
           moneyText.transform.position = new Vector2(textPos.x, yPos);

           //show crystal if in minigame
           if (mm != null)
            {
                crystalOffset = 50 * mm.screenWidthScale;
                mm.crystalObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(moneyText.transform.position.x - crystalOffset, moneyText.transform.position.y, 1));
                //mm.crystalObject.transform.position = new Vector3(mm.crystalObject.transform.position.x, mm.crystalObject.transform.position.y, 1);
            }
           yield return null;
        }
        //moneyText.gameObject.SetActive(false);
        gameObject.SetActive(false);
        if (mm != null)
            mm.crystalObject.gameObject.SetActive(false);
    }
}
