using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MMurray.LastLine;

//this tooltip shows what the system buttons do, and also provides help on how to input coordinates using the keyboard. Player only has to input 2 values: 1 for x and 1 for y.
public class SystemTooltip : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI helpText; 
    private const float tooltipYOffset = -80;
    private const float tooltipXOffset = -50;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowTooltip(bool toggle, string details = "", SystemButton button = null)
    {
        gameObject.SetActive(toggle);

        if (toggle == true)
        {
            //place the tooltip around the mouse's position
            if (!Singleton.instance.inMinigame)
            {
                GameManager gm = Singleton.instance.GameManager;
                gameObject.transform.position = new Vector2(button.transform.position.x + tooltipXOffset * gm.screenWidthScale, button.transform.position.y + tooltipYOffset * gm.screenHeightScale);
            }
            else
            {
                MinigameManager mm = Singleton.instance.MinigameManager;
                gameObject.transform.position = new Vector2(button.transform.position.x + tooltipXOffset * mm.screenWidthScale, button.transform.position.y + tooltipYOffset * mm.screenHeightScale);

            }
            helpText.text = details;
        }
    }
}
