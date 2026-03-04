using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using UnityEngine.Animations;

/* All this script does is hold a value for each button on the X and Y axis. When the button is clicked, the value
it holds is passed on elsewhere to create an ordered pair. */
public class AxisButton : MonoBehaviour
{
    [SerializeField]private int value;
    public enum AxisValue {X_AXIS, Y_AXIS, ORIGIN}
    /// <summary>
    /// Label for determining if a value is on the X or Y axis.
    /// </summary>
    public AxisValue axisValue;

    public void GetValue()
    {
        GameManager gm = Singleton.instance.GameManager;

        //check which axis we're on and record the value. A special case is made for the origin.
        if (axisValue == AxisValue.ORIGIN)
        {
            if (!gm.enteringValueX && !gm.enteringValueY)
            {
                gm.enteringValueX = true;       //always start with X
                gm.playerCoordinates.x = value;

                //change state
                gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
                Logger.DebugLogFormat("player X: {0}", gm.playerCoordinates.x);
            }
            else
            {
                gm.playerCoordinates.y = value;
                gm.enteringValueY = true;
                //change state
                //gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
                Logger.DebugLogFormat("player Y: {0}", gm.playerCoordinates.y);
            }
            

            //if both X and Y are selected, do the next action.
            /*if (gm.enteringValueX && gm.enteringValueY)
            {
                //highlight the point selected.
                Logger.DebugLog("We're in the origin");
                gm.enteringValueX = false;
                gm.enteringValueY = false;
                //change state
                gm.SetGameState(gm.gameState = GameManager.GameState.Normal);
            }*/
            //return;
        }

        else if (axisValue == AxisValue.X_AXIS)
        {
            gm.playerCoordinates.x = value;
            gm.enteringValueX = true;
            gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
            Logger.DebugLogFormat("player X: {0}", gm.playerCoordinates.x);
        }
        else
        {
            //cannot proceed if player is trying to enter a Y value before X
            if (!gm.enteringValueX)
            {
                gm.DisplayAlertWithDuration(GetText("cannotEnterYBeforeX"));
                Logger.DebugLogWarning("Cannot enter Y value before X!");
                return;
            }
            gm.playerCoordinates.y = value;
            gm.enteringValueY = true;
            //gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
            Logger.DebugLogFormat("player Y: {0}", gm.playerCoordinates.y);
        }
        
        //if both X and Y are selected, do the next action.
        if (gm.enteringValueX && gm.enteringValueY)
        {
            //highlight the point selected.
            bool pointFound = false;
            int i = 0;
            while (!pointFound && i < gm.points.Count)
            {
                if (gm.points[i].point == gm.playerCoordinates)
                {
                    pointFound = true;
                    gm.PlaceUnit(gm.playerUnitType, gm.points[i]);
                    gm.SetGameState(gm.gameState = GameManager.GameState.Normal);
                    Logger.DebugLogFormat("Player entered {0}", gm.points[i].point);
                }
                else
                {
                    i++;
                }
            }
            gm.enteringValueX = false;
            gm.enteringValueY = false;
        }
        //Logger.DebugLogFormat("player Coordinates {0}", gm.playerCoordinates);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
