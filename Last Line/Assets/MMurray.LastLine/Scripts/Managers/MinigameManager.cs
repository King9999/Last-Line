using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;
using TMPro;

/* Similar to game manager except much smaller. */
public class MinigameManager : MonoBehaviour
{
    public enum GameState
    {
        Normal,               //when game state is normal the game is running. Otherwise the game is paused.
        UnitSelectX, UnitSelectY,               //player is entering coordinates
        StartLevel, DialogueStartOfLevel, DialogueEndOfLevel,
        LevelComplete, GoToNextLevel
    }

    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
