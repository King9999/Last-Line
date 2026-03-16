using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMurray.LastLine;

public class UnitButton : MonoBehaviour
{
    public enum UnitType
    {
        StraightShooter = 2, VerticalShooter, TrackingBeam, Bomb    //values must match values in Game Manager's enum
    }

    public GameManager.PlayerUnitType unitType;
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

    public void OnMouseClick()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState != GameManager.GameState.Normal)   //prevent any other action while unit selected.
            return;

        gm.playerUnitType = unitType;

        //check if player has enough money
        int cost = 0;
        switch(unitType)
        {
            case GameManager.PlayerUnitType.StraightShooter:
                cost = gm.unitCosts[0];
                break;
            case GameManager.PlayerUnitType.VerticalShooter:
                cost = gm.unitCosts[1];
                break;
            case GameManager.PlayerUnitType.TrackingBeam:
                cost = gm.unitCosts[2];
                break;
            case GameManager.PlayerUnitType.Bomb:
                cost = gm.unitCosts[3];
                break;
        }

        Logger.DebugLogFormat("Unit cost: {0}", cost);

        if (cost > gm.money)
        {
            gm.DisplayAlertWithDuration(GetText("notEnoughMoney"));
        }
        else
        {
            gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectX);
        }
        //gm.alertText.text = "Select X value";
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
