using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* The menu for the main gameplay. Contains buttons for selecting units and game options. */
public class Menu : MonoBehaviour
{
    public Tooltip tooltip;
    [SerializeField]private TextMeshProUGUI unitLabel, systemLabel, buildQueueLabel;

    [Header("---Money---")]
    [SerializeField]private GameObject moneyContainer;
    [SerializeField]private TextMeshProUGUI moneyLabel;
    [SerializeField]private TextMeshProUGUI moneyValue;

    [Header("---Unit Buttons---")]
    public List<UnitButton> unitButtons;
    public UnitButton selectedButton;       //the button the mouse is hovering on.

    [Header("---Build Queue Images---")]
    public Image[] unitsInQueue;
    public int queueIndex;                  //used to point to correct image in list.

    [Header("---System Buttons---")]
    [SerializeField]private Button ttsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button exitGameButton;
    // Start is called before the first frame update
    void Awake()
    {
        Singleton.instance.Menu = this;
        //GameManager gm = Singleton.instance.GameManager;
        /*unitLabel.text = GetText("unitLabel");
        moneyLabel.text = GetText("moneyLabel");
        systemLabel.text = GetText("systemLabel");
        buildQueueLabel.text = GetText("buildQueueLabel");*/
        //moneyValue.text = gm.money + " CR";
       
    }

    void Start()
    {
        unitLabel.text = GetText("unitLabel");
        moneyLabel.text = GetText("moneyLabel");
        systemLabel.text = GetText("systemLabel");
        buildQueueLabel.text = GetText("buildQueueLabel");
        //UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        GameManager gm = Singleton.instance.GameManager;
        moneyValue.text = string.Format("{0:000}", gm.money); //gm.money + " CR";
    }
    /// <summary>
    /// Opens a tooltip window displaying info about a button when mouse hovers over it.
    /// </summary>
    /*public void OnMouseHoverEnter()
    {
        Logger.DebugLog("Hovering over button");
        if (tooltip.gameObject.activeSelf)
            return;

       
        tooltip.DisplayUnitTooltip(true);

        
    }

    /// <summary>
    /// Hides tooltip when mouse leaves button.
    /// </summary>
    public void OnMouseHoverExit()
    {
        Logger.DebugLog("Leaving button");
        tooltip.DisplayUnitTooltip(false);
    }*/

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
    
}