using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LoLSDK;
using MMurray.LastLine;
using TMPro;
using System;
using MMurray.GenericCode;
using UnityEngine.EventSystems;
using System.Linq;

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
    Singleton s = Singleton.instance;       //used mainly to call GetText
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
    public int crystalsFound;
    [SerializeField]TargetCursor targetCursor;
    public List<Point> points;

    [Header("---Minigame---")]
    public List<MinigameLevelData> minigameLevelData;
    //public Sprite planet1;      //background image.
    //public Sprite planet2;
    public SpriteRenderer planet1;
    public SpriteRenderer planet2;
    public GameObject crystalObject;        //displayed when a crystal is found.

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
    public SystemTooltip inputHelpTooltip;   //input help is for the coordinates input field.

    [Header("---Dialogue---")]
    public int triggerIndex;                //used to iterate through conditional triggers.

    public float screenWidthScale {get;} = (float)Screen.width / 1024;
    public float screenHeightScale {get;} = (float)Screen.height / 576;

    //coroutine
    Coroutine animateCrystal;           //called when a crystal is found
    Coroutine animateCoordinates;
    Coroutine alertCoroutine;

    void Awake()
    {
        Singleton.instance.MinigameManager = this;
        Singleton.instance.inMinigame = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //load save state
        Singleton s = Singleton.instance;
        if (s.saveStateFound)
        {
            //s.saveState.LoadState(s.saveState.ReadState);
            s.currentLevel = s.gameData.level;
            
            //s.saveState.ReadState(s.gameData);
        }

        s.minigameLevel = (s.currentLevel == 3) ? 1 : 2;      //minigame 2 occurs at level 6. currentLevel is 2 because level hasn't advanced to 3 yet.
        SetLevel(s.minigameLevel);
        MinigameMenu menu = Singleton.instance.MinigameMenu;
        menu.SetupCheckboxes();
        crystalsFound = 0;

        /****crystal setup
        Level 1 coordinates:
            (1,4), (5,0), (2,2), (3,3), (0,5)
        *****/

        alertText.text = "";
        hintText.text = "";
        xCoordinateText.text = "";
        coordinatesText.text = "";
        //exitGameLabel.text = GetText("exitButtonText");
        coordinatesLabel.text = s.GetText("coordinatesLabel");
        enteringValueX = false;
        enteringValueY = false;
        targetCursor.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(false);
        crystalObject.gameObject.SetActive(false);

        //input field setup
        coordinatesInputField.characterLimit = 2;

        /********MUSIC***********/
        AudioManager am = Singleton.instance.AudioManager;
        if (!am.mainMusic.isPlaying)
            PlayMusic(Singleton.instance.musicEnabled);

        SetGameState(gameState = GameState.StartLevel);
    }

    // Update is called once per frame
    void Update()
    {
        //if any keyboard input is recognized, the player can immediately input coordinates in the input field.
        if (Input.anyKeyDown)
        {
            EventSystem.current.SetSelectedGameObject(coordinatesInputField.gameObject, null);
        }

        //check for right click or Escape key. They're used to cancel certain actions.
        if (CancelButtonPressed())
        {
            if (gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY)
            {

                DisplayAlertWithDuration(s.GetText("cancelAction"));
                SetGameState(gameState = GameState.Normal);
            }
        }
    }

    public void PlayMusic(bool toggle)
    {
        AudioManager am = Singleton.instance.AudioManager;
        if (toggle == true)
            am.mainMusic.Play();
        else
            am.mainMusic.Stop();
    }

    bool CancelButtonPressed()
    {
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
    }


    public void SetLevel(int level)
    {
        if (level < 1 || level > 2)
        {
            Logger.DebugLogWarning("Invalid minigame level!");
            return;
        }

        this.level = level;

        //set up background
        //SpriteRenderer planetRenderer = GetComponent<SpriteRenderer>();
        if (level == 1)
        {
            planet1.gameObject.SetActive(true);
            planet2.gameObject.SetActive(false);
        }
        else
        {
            planet1.gameObject.SetActive(false);
            planet2.gameObject.SetActive(true);
        }
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

        //check for a crystal 
        bool crystalFound = false;
        int i = 0;
        while (!crystalFound && i < minigameLevelData[level - 1].crystals.Count)
        {
            Crystal crystal = minigameLevelData[level - 1].crystals[i];
            if (point.pointAxes == crystal.crystalLocation && !crystal.crystalFound)
            {
                crystalFound = true;
                crystal.crystalFound = true;
                minigameLevelData[level - 1].crystals[i] = crystal;
                AddMoney(crystal.crystalValue, point.transform.position);
                DisplayAlertWithDuration(s.GetText("crystalFound"));
                
                //check correct box
                MinigameMenu menu = Singleton.instance.MinigameMenu;
                bool boxFound = false;
                int j = 0;
                while (!boxFound && j < menu.crystalLocationCheckboxes.Count)
                {
                    if (crystal.crystalLocation == menu.crystalLocationCheckboxes[j].crystalLocation)
                    {
                        boxFound = true;
                        menu.ToggleCheckBox(menu.crystalLocationCheckboxes[j], true);
                        crystalsFound++;
                    }
                    else
                    {
                        j++;
                    }
                }

                //remove crystal from
                Logger.DebugLog("Crystal Found!");
            }
            else
            {
                i++;
            }

        }

        if (!crystalFound)
        {
            DisplayAlertWithDuration(s.GetText("nothingAtPoint"));
        }

        //once 3 crystals are found, minigame ends.
        if (crystalsFound >= 3)
        {
            SetGameState(gameState = GameState.LevelComplete);
        }
        else
        {
            SetGameState(gameState = GameState.Normal);
        }
    }

    /*checks coordinates input field to ensure the characters entered are valid.
    Only characters in specific positions are checked because it's known that the other
    characters aren't valid and don't need to be checked. */
    public void ValidateCoordinates()
    {
        if (gameState != GameState.Normal && gameState != GameState.UnitSelectX && gameState != GameState.UnitSelectY)
        {
            coordinatesInputField.text = "";
            return;
        }

        List<int> validNums = new List<int>();

        if (coordinatesInputField.text.Count() > 0)
        {
            xCoordinateText.text = "";
            alertText.text = "";
            axisContainerX.gameObject.SetActive(true);
            axisContainerY.gameObject.SetActive(true);
            //show grid
            if (!points[0].pointImage.enabled)
            {
                foreach (Point p in points)
                {
                    p.ShowPoint(true);
                }
            }
        }
        else
        {
            if (points[0].pointImage.enabled)
            {
                foreach (Point p in points)
                {
                    p.ShowPoint(false);
                }
            }
        }

        for (int i = 0; i < coordinatesInputField.text.Count(); i++)
        {
            if(int.TryParse(coordinatesInputField.text.Substring(i, 1), out int num) && num <= 5)
            {
                validNums.Add(num);
                Logger.DebugLogFormat("Added {0} to validNums", num);
            }
            else
            {
                Logger.DebugLog("Can't parse");
                coordinatesInputField.text = coordinatesInputField.text.Replace(coordinatesInputField.text.Substring(i, 1), "");
            }
        }

        //if there are 2 valid numbers, then they're accepted and the point is targeted
        if (validNums.Count() >= 2)
        {
            Logger.DebugLog("2 valid nums");
            coordinatesInputField.text = "";
            //find the point matching the values.
            playerCoordinates = new Vector2(validNums[0], validNums[1]);

            int i = 0;
            bool pointFound = false;
            while(!pointFound && i < points.Count())
            {
                if (playerCoordinates == points[i].pointAxes)
                {
                    pointFound = true;
                    CheckLocation(points[i]);
                }
                else
                {
                    i++;
                }
            }
        }

    }

    public void AddMoney(int amount, Vector2 position)
    {
        if (amount < 0 || money >= 999)
            return;

        money = (money + amount <= 999) ? money + amount : 999;

        MinigameMenu menu = Singleton.instance.MinigameMenu;
        menu.UpdateMoneyText();
        moneyText.ShowMoneyRewardText(amount, position, useWorldToScreenPoint: false);
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
            
            case GameState.UnitSelectX:
                //disable Y values, then disable X values.
                //TODO: Have dialogue during tutorial to explain how to set units
                DisplayAlert(s.GetText("unitSelect_x"));
                coordinatesInputField.text = "";
                //coordinatesInputContainer.gameObject.SetActive(false);
                if (!points[0].pointImage.enabled)
                {
                    foreach (Point p in points)
                    {
                        p.ShowPoint(true);
                    }
                }
                axisContainerY.gameObject.SetActive(false);
                break;
            
            case GameState.UnitSelectY:
                //disable Y values, then disable X values.
                //TODO: in tutorial, make mention of origin and how it might be used
                DisplayAlert(s.GetText("unitSelect_y"));
                coordinatesInputField.text = "";
                //coordinatesInputContainer.gameObject.SetActive(false);
                //show points
                if (!points[0].pointImage.enabled)
                {
                    foreach (Point p in points)
                    {
                        p.ShowPoint(true);
                    }
                }
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(false);
                break;
            

            case GameState.StartLevel:
                //look for any opening dialogue. If there's none, game resumes.
                bool triggerFound = false;
                int i = 0;
                while (!triggerFound && i < dialogueManager.condTriggerList.Count)
                {
                    if (dialogueManager.condTriggerList[i].level == Singleton.instance.currentLevel &&
                        dialogueManager.condTriggerList[i].condition == DialogueManager.ConditionalTrigger.Condition.LevelStart)
                    {
                        triggerFound = true;
                    }
                    else
                    {
                        i++;
                    }

                }


                if (triggerFound)
                {
                    triggerIndex = i;
                    SetGameState(gameState = GameState.DialogueStartOfLevel);
                }
                else
                {
                    SetGameState(gameState = GameState.Normal);
                }
                break;

            case GameState.DialogueStartOfLevel:
                //find correct dialogue to display.
                bool dialogueFound = false;
                int j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    int dialogueId = dialogueManager.dialogueList[j].dialogueId;
                    if (dialogueId > 0 && dialogueId == dialogueManager.condTriggerList[triggerIndex].dialogueId /*== dialogueManager.dialogueList[j].dialogueId*/)
                    {
                        dialogueFound = true;
                        coordinatesInputContainer.gameObject.SetActive(false);
                        Dialogue.DialogueKeyList keyList = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        //string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0].key;
                        dialogueManager.dialogueList[j].ShowDialogue(keyList);
                    }
                    else
                    {
                        j++;
                    }
                }

                //if no dialogue is found, then just start the game
                if (!dialogueFound)
                    SetGameState(gameState = GameState.Normal);
                break;
            
            case GameState.DialogueEndOfLevel:
                //find correct dialogue to display.
                dialogueFound = false;
                j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    int dialogueId = dialogueManager.dialogueList[j].dialogueId;
                    if (dialogueId > 0 && dialogueId == dialogueManager.condTriggerList[triggerIndex].dialogueId)
                    {
                        dialogueFound = true;
                        coordinatesInputContainer.gameObject.SetActive(false);
                        Dialogue.DialogueKeyList keyList = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        //string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0].key;
                        dialogueManager.dialogueList[j].ShowDialogue(keyList);
                    }
                    else
                    {
                        j++;
                    }
                }
                break;
            
            case GameState.LevelComplete:
                hintText.text = "";
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
                Logger.DebugLog("Minigame complete");

                //check for end of level dialogue
                triggerFound = false;
                j = 0;
                while (!triggerFound && j < dialogueManager.condTriggerList.Count)
                {
                    if (dialogueManager.condTriggerList[j].level == Singleton.instance.currentLevel &&
                        dialogueManager.condTriggerList[j].condition == DialogueManager.ConditionalTrigger.Condition.LevelEnd)
                    {
                        triggerFound = true;
                    }
                    else
                    {
                        j++;
                    }

                }
                

                if (triggerFound)
                {
                    triggerIndex = j;
                    SetGameState(gameState = GameState.DialogueEndOfLevel);
                }
                else
                {
                    SetGameState(gameState = GameState.GoToNextLevel);
                }
                break;
            
            case GameState.GoToNextLevel:
                ScreenFade sf = Singleton.instance.ScreenFade;
                Singleton.instance.minigameMoney = money;
                Singleton.instance.currentLevel++;
                //Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
                Logger.DebugLogFormat("Next level: {0}", Singleton.instance.currentLevel);
                sf.ChangeSceneFadeOut("Game");
                break;
        }
    }

    public void DisplayAlert(string msg)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        alertText.text = msg;
    }

    public void DisplayAlertWithDuration(string msg, float duration = 4)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        alertCoroutine = StartCoroutine(Alert(msg, duration));
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/
        
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
