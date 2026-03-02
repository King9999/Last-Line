using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMurray.LastLine;

public class UnitButton : MonoBehaviour
{
    public enum UnitType
    {
        StraightShooter, VerticalShooter, TrackingBeam, Bomb
    }

    public UnitType unitType;
    public string key;                 //the string to display from language json.

    /// <summary>
    /// Opens a tooltip window displaying info about a button when mouse hovers over it.
    /// </summary>
    public void OnMouseHoverEnter()
    {
        Logger.DebugLog("Hovering over button");
        Menu menu = Singleton.instance.Menu;
        if (menu.tooltip.gameObject.activeSelf)
            return;

        menu.selectedButton = this;     //pass information that game manager will use to create desired unit.
        menu.tooltip.DisplayUnitTooltip(true, GetText(key));
    }

    /// <summary>
    /// Hides tooltip when mouse leaves button.
    /// </summary>
    public void OnMouseHoverExit()
    {
        Logger.DebugLog("Leaving button");
        Menu menu = Singleton.instance.Menu;
        menu.tooltip.DisplayUnitTooltip(false);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
