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
        public bool crystalFound;           //if true, player already found this crystal and can't add its value to the total.
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
    public int money;               //money acquired. It's added to the starting money in the next level.
    [SerializeField]TargetCursor targetCursor;
    public List<Point> points;

    [Header("---Minigame---")]
    [SerializeField]List<MinigameLevelData> minigameLevelData;

    [Header("---Containers---")]
    [SerializeField]private GameObject axisContainerX;
    [SerializeField]private GameObject axisContainerY;

    [Header("---UI---")]
    [SerializeField]private TextMeshProUGUI alertText;           //used to inform player of various things.
    [SerializeField]private TextMeshProUGUI hintText;           //gives hints at start of level and when crystal is on screen.
    public TextMeshProUGUI xCoordinateText;        //used to let player know which X coordinate they picked
    public TextMeshProUGUI coordinatesText;
    public DialogueManager dialogueManager;
    [SerializeField]private MoneyText moneyText;    //is displayed when an enemy unit is destroyed.
    public GameObject coordinatesInputContainer;
    [SerializeField]private TMP_InputField coordinatesInputField;
    [SerializeField]private TextMeshProUGUI coordinatesLabel;

    public float screenWidthScale {get;} = (float)Screen.width / 1024;
    public float screenHeightScale {get;} = (float)Screen.height / 576;

    //coroutine
    Coroutine animateCrystal;           //called when a crystal is found
    Coroutine animateCoordinates;
    Coroutine alertCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.MinigameManager = this;
        SetLevel(Singleton.instance.minigameLevel);
        //SetLevel(1);

        /****crystal setup
        Level 1 coordinates:
            (1,4), (5,0), (2,2), (3,3), (0,5)
        *****/

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

    /*Check a given location on the grid for a crystal. If a crystal is found, money is added to the total. */
    public void CheckLocation(Point point)
    {
        //check if location matches a point in the current level
        //if there's a match, money is added to total. Prevent player from scanning same location.

        targetCursor.ShowCursor(point.transform.position);
        if (animateCoordinates != null)
                StopCoroutine(animateCoordinates);
            animateCoordinates = StartCoroutine(ShowCoordinates(point));
    }

    public void AddMoney(int amount, Vector2 position)
    {
        if (amount < 0 || money >= 999)
            return;

        money = (money + amount <= 999) ? money + amount : 999;

        MinigameMenu menu = Singleton.instance.MinigameMenu;
        menu.UpdateMoneyText();
        moneyText.ShowMoneyRewardText(amount, position);
    }

    public void SetGameState(GameState state)
    {
        Logger.DebugLogFormat("GameState changed to {0}", gameState);
        ArrowGuideManager am = Singleton.instance.ArrowGuideManager;

        switch(state)
        {
            case GameState.Normal:
                //alertText.text = "";
                hintText.text = "";
                am.HideGuideArrows();
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                enteringValueX = false;
                enteringValueY = false;
                xCoordinateText.text = "";
                coordinatesInputField.text = "";


                //hide points
                if (points[0].pointImage.enabled)
                {
                    foreach (Point p in points)
                    {
                        p.ShowPoint(false);
                    }
                }
                break;
        }
    }

    public void DisplayAlertWithDuration(string msg, float duration = 4)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        alertCoroutine = StartCoroutine(Alert(msg, duration));
    }
        
#region Coroutines
    //coordinates animate upwards for a duration
    IEnumerator ShowCoordinates(Point point, float duration = 1)
    {
        coordinatesText.transform.position = new Vector2(targetCursor.transform.position.x, targetCursor.transform.position.y + 50);
        float time = 0;
        float moveSpeed = 10;
        coordinatesText.text = point.pointAxes.ToString("F0");  //F0 removes decimal places in a float.
        while (time < duration)
        {
            time += Time.deltaTime;
            Vector2 newPos = coordinatesText.transform.position;
            coordinatesText.transform.position = new Vector2(newPos.x, newPos.y + moveSpeed * Time.deltaTime);
            yield return null;
        }
        coordinatesText.text = "";
    }

    //shows alert for a duration. Used when an alert only needs to be displayed temporarily.
    IEnumerator Alert(string msg, float duration = 4)
    {
        alertText.text = msg;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }

        alertText.text = "";
    }
    #endregion
}
