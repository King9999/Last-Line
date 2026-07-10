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
        DialogueManager dm = Singleton.instance.DialogueManager;

        if (dm.dialogueOpen || gm.UnplayableState())
            return;
        
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.buttonClick, audio.soundVolume);

        //check which axis we're on and record the value. A special case is made for the origin.
        if (axisValue == AxisValue.ORIGIN)
        {
            //tutorial check. Exception is made if we're on level 2 to pick up crystal and explain the origin.
            if (gm.inTutorial)
            {
                if (gm.currentLevel == 2)
                {
                    if (!gm.enteringValueX && !gm.enteringValueY)
                    {
                        gm.enteringValueX = true;       //always start with X
                        gm.playerCoordinates.x = value;
                        gm.xCoordinateText.text = GetText("xCoordinateSelected") + value;

                        //change state
                        gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
                        Logger.DebugLogFormat("player X: {0}", gm.playerCoordinates.x);
                    }
                    else
                    {
                        gm.playerCoordinates.y = value;
                        gm.enteringValueY = true;
                        Logger.DebugLogFormat("player Y: {0}", gm.playerCoordinates.y);
                    }
                }
            }
            else
            {
                if (!gm.enteringValueX && !gm.enteringValueY)
                {
                    gm.enteringValueX = true;       //always start with X
                    gm.playerCoordinates.x = value;
                    gm.xCoordinateText.text = GetText("xCoordinateSelected") + value;

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
            }

        }

        else if (axisValue == AxisValue.X_AXIS)
        {
            gm.playerCoordinates.x = value;
            gm.enteringValueX = true;

            //tutorial check
            if (gm.inTutorial)
            {
                //check which value player picked. 
                switch(gm.tutorialState)
                {
                    case GameManager.TutorialState.UnitSelectX:
                        //if player picked incorrect value, we don't continue.
                        if (value != 3)
                            return;
                        gm.xCoordinateText.text = GetText("xCoordinateSelected") + value;
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.UnitSelectY);
                        break;
                    
                    case GameManager.TutorialState.UnitSelectSecondUnitX:
                        //if player picked incorrect value, we don't continue.
                        if (value != 5)
                            return;
                        gm.xCoordinateText.text = GetText("xCoordinateSelected") + value;
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.UnitSelectSecondUnitY);
                        break;
                    
                    case GameManager.TutorialState.ActivateAbilitySelectX:
                        //if player picked incorrect value, we don't continue.
                        if (value != 5)
                            return;
                        gm.xCoordinateText.text = GetText("xCoordinateSelected") + value;
                        gm.SetGameState(gm.tutorialState = GameManager.TutorialState.ActivateAbilitySelectY);
                        break;
                    
                    default:
                        break;

                }
            }
            else
            {
                //gm.playerCoordinates.x = value;
                //gm.enteringValueX = true;
                gm.xCoordinateText.text = GetText("xCoordinateSelected") + " " + value;
                gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
            }
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

             //tutorial check
            if (gm.inTutorial)
            {
                //check which value player picked. 
                switch(gm.tutorialState)
                {
                    case GameManager.TutorialState.UnitSelectY:
                        //if player picked incorrect value, we don't continue.
                        if (value != 2)
                            return;

                        gm.enteringValueY = true;
                        break;
                    
                    case GameManager.TutorialState.UnitSelectSecondUnitY:
                    case GameManager.TutorialState.ActivateAbilitySelectY:
                        //if player picked incorrect value, we don't continue.
                        if (value != 3)
                            return;

                        gm.enteringValueY = true;
                        break;

                    default:
                        break;
                    
                    /*case GameManager.TutorialState.ActivateAbilitySelectY:
                        //if player picked incorrect value, we don't continue.
                        if (value != 3)
                            return;

                        gm.enteringValueY = true;
                        //gm.SetGameState(gm.tutorialState = GameManager.TutorialState.DialogueEnemyIncoming);
                        break;*/

                }
            }
            else
            {
                gm.enteringValueY = true;
            }
            //gm.SetGameState(gm.gameState = GameManager.GameState.UnitSelectY);
            Logger.DebugLogFormat("player Y: {0}", gm.playerCoordinates.y);
        }
        
        //if both X and Y are selected, do the next action.
        if (!Singleton.instance.inMinigame)
        {
            if (gm.enteringValueX && gm.enteringValueY)
            {
                //highlight the point selected.
                bool pointFound = false;
                int i = 0;
                while (!pointFound && i < gm.points.Count)
                {
                    if (gm.points[i].pointAxes == gm.playerCoordinates)
                    {
                        pointFound = true;
                        gm.PlaceUnit(gm.playerUnitType, gm.points[i]);

                        //tutorial check
                        if (gm.inTutorial)
                        {
                            switch(gm.tutorialState)
                            {
                                case GameManager.TutorialState.UnitSelectY:
                                    gm.SetGameState(gm.tutorialState = GameManager.TutorialState.BuildingFirstUnit);
                                    break;
                                
                                case GameManager.TutorialState.UnitSelectSecondUnitY:
                                    gm.SetGameState(gm.tutorialState = GameManager.TutorialState.BuildingSecondUnit);
                                    break;
                                
                                case GameManager.TutorialState.ActivateAbilitySelectY:
                                    gm.SetGameState(gm.tutorialState = GameManager.TutorialState.DialogueEnemyIncoming);
                                    break;
                                
                                default:
                                    break;

                            }

                            //level 2 check
                            if (gm.currentLevel == 2)
                            {
                                gm.SetGameState(gm.gameState = GameManager.GameState.Normal);
                            }
                        }
                        else
                        {
                            //if enemy is preparing, game hasn't begun yet.
                            gm.SetGameState(gm.gameState = gm.gameStarted ? GameManager.GameState.Normal : GameManager.GameState.EnemyPreparing);
                        }
                        Logger.DebugLogFormat("Player entered {0}", gm.points[i].pointAxes);
                    }
                    else
                    {
                        i++;
                    }
                }
                gm.enteringValueX = false;
                gm.enteringValueY = false;
            }
        }
        else
        {
            //we're in mining minigame
            //check the selected coordinates for a crystal.
            MinigameManager mm = Singleton.instance.MinigameManager;
            
            if (mm.enteringValueX && mm.enteringValueY)
            {
                //highlight the point selected.
                bool pointFound = false;
                int i = 0;
                while (!pointFound && i < mm.points.Count)
                {
                    if (mm.points[i].pointAxes == mm.playerCoordinates)
                    {
                        pointFound = true;
                        mm.CheckLocation(mm.playerCoordinates);                        
                        mm.SetGameState(mm.gameState = MinigameManager.GameState.Normal);
                        Logger.DebugLogFormat("Player entered {0}", mm.points[i].pointAxes);
                    }
                    else
                    {
                        i++;
                    }
                }
                mm.enteringValueX = false;
                mm.enteringValueY = false;
            }
        }
        //Logger.DebugLogFormat("player Coordinates {0}", gm.playerCoordinates);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
