using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;
using TMPro;
using System;

/* Similar to game manager except much smaller. 
The minigame works like this:
-The player must collect 3 out of 5 crystals by entering the crystals' coordinates. 
-Each crystal has a different value.
-The player is told where the crystals are located, but they don't know the value.
-There is no time limit, but the minigame ends once 3 crystals are collected.
-The total amount of scrap collected is carried over into the next level.
*/
public class MinigameManager : MonoBehaviour
{
    public enum GameState
    {
        Normal,               //when game state is normal the game is running. Otherwise the game is paused.
        UnitSelectX, UnitSelectY,               //player is entering coordinates
        StartLevel, DialogueStartOfLevel, DialogueEndOfLevel,
        LevelComplete, GoToNextLevel
    }

    [Serializable]
    public struct Crystal
    {
        public Vector2 crystalLocation;
        public int crystalValue;
    }

    [Serializable]
    public struct MinigameLevelData
    {
        public int level;
        public List<Crystal> crystals;
    }

    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public int level;               //there are 2 levels.

    [Header("---Minigame---")]
    [SerializeField]List<MinigameLevelData> minigameLevelData;

    [Header("---UI---")]
    [SerializeField]private TextMeshProUGUI alertText;           //used to inform player of various things.
    [SerializeField]private TextMeshProUGUI hintText;           //gives hints at start of level and when crystal is on screen.
    public TextMeshProUGUI xCoordinateText;        //used to let player know which X coordinate they picked
    public DialogueManager dialogueManager;
    [SerializeField]private MoneyText moneyText;    //is displayed when an enemy unit is destroyed.

    public float screenWidthScale {get;} = (float)Screen.width / 1024;
    public float screenHeightScale {get;} = (float)Screen.height / 576;

    //coroutine
    Coroutine animateCrystal;           //called when a crystal is found

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.MinigameManager = this;
        SetLevel(Singleton.instance.minigameLevel);

        /****crystal setup
        Level 1 coordinates:
            (1,4), (5,0), (2,2), (3,3), (0,5)
        *****/
        if (level == 1)
        {
            
        }
        else
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(int level)
    {
        if (level < 1 || level > 2)
        {
            Logger.DebugLogWarning("Invalid minigame level!");
            return;
        }

        this.level = level;
    }
}
