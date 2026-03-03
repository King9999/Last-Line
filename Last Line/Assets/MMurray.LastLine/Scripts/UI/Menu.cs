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
    public TextMeshProUGUI unitLabel;

    [Header("---Money---")]
    public GameObject moneyContainer;
    public TextMeshProUGUI moneyLabel;
    public TextMeshProUGUI moneyValue;

    [Header("---Unit Buttons---")]
    [SerializeField]private List<UnitButton> unitButtons;
    public UnitButton selectedButton;       //the button the mouse is hovering on.

    [Header("---System Buttons---")]
    [SerializeField]private Button ttsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button exitGameButton;
    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.Menu = this;
        GameManager gm = Singleton.instance.GameManager;
        unitLabel.text = GetText("unitLabel");
        moneyLabel.text = GetText("moneyLabel");
        moneyValue.text = string.Format("{0} CR", gm.money.ToString());
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