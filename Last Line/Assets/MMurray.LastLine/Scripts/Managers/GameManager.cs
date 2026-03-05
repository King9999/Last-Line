using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;
using TMPro;


public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Game State
    /// </summary>
    public enum GameState
    {
        Normal,                 //when game state is normal the game is running. Otherwise the game is paused.
        UnitSelectX, UnitSelectY, CoordinatesSelect, ///player is entering coordinates
        DialogueStartOfLevel, DialogEndOfLevel,
        SelectingValueX, SelectingValueY,       //used when placing units on screen.
        BaseDestroyed, LevelComplete

    }

    public enum PlayerUnitType {NoUnitSelected, Base, StraightShooter, VerticalShooter, TrackingBeam, Bomb}
    public PlayerUnitType playerUnitType;
    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public List<Point> points;                //the locations on the map where all units are placed.

    [Header("---UI---")]
    public TextMeshProUGUI alertText;           //used to inform player of various things.
    //public Dialogue dialogueBox;
    public DialogueManager dialogueManager;
    public int triggerIndex;                //used to iterate through conditional triggers.

    

    //unit setup
    [Header("---Active Units---")]
    public List<PlayerUnit> activePlayerUnits;
    public List<EnemyUnit> activeEnemyUnits;
    public PlayerUnit testUnit;
    public Portal testPortal;
    public PlayerUnit_Base testBase;

    [Header("---Money---")]
    public int money;                           //how much money player has to create units.

    [Header("---Containers---")]
    public GameObject playerUnitContainer;      //all instantiated units are placed here
    public GameObject axisContainerX;
    public GameObject axisContainerY;

    [Header("---Prefabs---")]
    public Bullet bulletPrefab;
    public Laser laserPrefab;
    public PlayerUnit_Base basePrefab;
    public PlayerUnit_StraightShooter straightShooterPrefab;
    public PlayerUnit_VerticalShooter verticalShooterPrefab;
    public EnemyUnit scoutPrefab;
    public EnemyUnit strikerPrefab;

    /***Units that are destroyed are sent to an array, and recycled whenever a new unit needs to be generated. 
    This is to reduce the amount of calls to the garbage collector. */
    [Header("---Graveyard---")]
    public List<EnemyUnit> deadEnemyUnits;
    public List<PlayerUnit> deadPlayerUnits;

    /* coroutines. Having references to coroutines is ideal for performance and convenience, and StopCoroutine() actually works
    when coroutines are executed this way. */
    Coroutine alertCoroutine = null;
    Coroutine screenShakeCoroutine = null;     

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;

        //both values should be false
        enteringValueX = false;
        enteringValueY = false;

        Logger.DebugLogFormat("Testing {0}", activeEnemyUnits.Count);
        LoadLevel(0);
        //load save state
        Singleton s = Singleton.instance;
        if (s.saveStateFound)
        {
            s.saveState.LoadState(s.saveState.ReadState);
            //s.saveState.ReadState(s.gameData);
        }

        //dialogueManager.dialogueList[0].ShowDialogue(dialogueManager.dialogueList[0].dialogueKeyList[0]);
        SetGameState(gameState = GameState.DialogueStartOfLevel);

    }

    // Update is called once per frame
    void Update()
    {
        //check for right click or Escape key. They're used to cancel certain actions.
        if (CancelButtonPressed())
        {
            if (gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY)
            {
                //cancel actions and return game state to normal.
                //StopCoroutine(Alert("", 0));
                DisplayAlertWithDuration(GetText("cancelAction"));
                SetGameState(gameState = GameState.Normal);
            }
        }
    }

    bool CancelButtonPressed()
    {
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
    }

    public bool GameRunning()
    {
        return gameState == GameState.Normal || gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY || 
            gameState == GameState.CoordinatesSelect;
    }

    /* Load the specified level. NOTE: Save States only load the current level from the beginning, and not the state
    the level was left in. This is because the levels are going to be short enough that it's unnecessary to record
    every unit's position. Also, the game runs in real time so the player might get confused on what they were doing. */
    void LoadLevel(int level)
    {
        //get level from JSON
        //generate base
        //PlaceUnit(PlayerUnitType.Base, new Vector2(1, 3));

        //generate portals
        //each portal instantiates number of units.
        //EnemyUnit enemyUnit = null;
        for (int i = 0; i < testPortal.enemyCount; i++)
        {
            EnemyUnit enemyUnit = Instantiate(strikerPrefab as EnemyUnit_Striker, parent: testPortal.transform);
            //EnemyUnit enemyUnit = Instantiate(scoutPrefab as EnemyUnit_Scout, parent: testPortal.transform);
            enemyUnit.transform.position = testPortal.transform.position;
            activeEnemyUnits.Add(enemyUnit);
            enemyUnit.portal = testPortal;
            enemyUnit.gameObject.SetActive(false);
            testPortal.enemyUnits.Add(enemyUnit);
        }
    }

    /// <summary>
    /// Instantiates and places a player unit at the specified location.
    /// </summary>
    /// <param name="unit">The unit type to be deployed.</param>
    /// <param name="point">The location where the unit will be deployed.</param>
    public void PlaceUnit(PlayerUnitType unit, Point point)
    {
        if (unit == PlayerUnitType.NoUnitSelected)
        {
            //TODO: Place a target to show that the player selected a point. 
            //if space is occupied, activate context-sensitive ability. nothing happens if unit doesn't have one.
            if (point.unit != null)
            {
                point.unit.ActivateAbility();
                return;
            }
            else 
            {
                //TODO: Check for a crystal and give player money.
                DisplayAlertWithDuration(GetText("nothingAtPoint"));
            }
        }
        else
        {
            PlayerUnit newUnit = null;
            //check what player picked and instantiate
            switch (unit)
            {
                case PlayerUnitType.Base:   //there can only be 1
                    newUnit = Instantiate(basePrefab, playerUnitContainer.transform);
                    break;

                case PlayerUnitType.StraightShooter:
                    newUnit = Instantiate(straightShooterPrefab, playerUnitContainer.transform);
                    break;
                
                case PlayerUnitType.VerticalShooter:
                    newUnit = Instantiate(verticalShooterPrefab, playerUnitContainer.transform);
                    break;
            }

            //must convert the point's location to world coordinates.
            Vector2 newPos = Camera.main.ScreenToWorldPoint(point.transform.position);
            newUnit.transform.position = newPos;
            //Logger.AddPlayerId(newUnit.name);
            newUnit.UpdateHealthBar(newPos);        //TODO: Not quite sure why this fixes the healthbar positioning issue for player units.
            point.unit = newUnit;

            //add to list
            activePlayerUnits.Add(newUnit);

            //save unit's position
            Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
            playerUnitType = PlayerUnitType.NoUnitSelected;
        }     
    }

    /// <summary>
    /// Places unit at the specified point. The difference with this method is that the point is located first, then the 
    /// unit is generated. 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="point"></param>
    public void PlaceUnit(PlayerUnitType unit, Vector2 point)
    {
        bool pointFound = false;
        int i = 0;
        while(!pointFound && i < points.Count)
        {
            if (point == points[i].point)
            {
                pointFound = true;
            }
            else
            {
                i++;
            }
        }

        if (!pointFound)
            return;
        
        //if we got here, point was found. Generate unit.
        PlayerUnit newUnit = null;
        //check what player picked and instantiate
        switch (unit)
        {
            case PlayerUnitType.Base:   //there can only be 1
                newUnit = Instantiate(basePrefab, playerUnitContainer.transform);
                break;

            case PlayerUnitType.StraightShooter:
                newUnit = Instantiate(straightShooterPrefab, playerUnitContainer.transform);
                break;
            
            case PlayerUnitType.VerticalShooter:
                newUnit = Instantiate(verticalShooterPrefab, playerUnitContainer.transform);
                break;
        }

        //must convert the point's location to world coordinates.
        Vector2 newPos = Camera.main.ScreenToWorldPoint(points[i].transform.position);
        newUnit.transform.position = newPos;
        newUnit.UpdateHealthBar(newPos);        //TODO: Not quite sure why this fixes the healthbar positioning issue for player units.
        points[i].unit = newUnit;

        //add to list
        activePlayerUnits.Add(newUnit);
    }

    /// <summary>
    /// Send player unit to graveyard and remove reference to unit on grid.
    /// </summary>
    /// <param name="unit">The unit to be removed.</param>
    public void RemovePlayerUnit(PlayerUnit unit)
    {
        bool unitFound = false;
        int i = 0;
        while(!unitFound && i < points.Count)
        {
            if (points[i].unit == unit)
            {
                unitFound = true;
                deadPlayerUnits.Add(unit);
                activePlayerUnits.Remove(unit);
                unit.gameObject.SetActive(false);
                Logger.DebugLogFormat("Removing {0} from grid at point {1}", unit.name, points[i].point);
                points[i].unit = null;
            }
            else
            {
                i++;
            }
        }
    }

    /// <summary>
    /// Remove enemy unit and send to graveyard. Reference to unit's portal is also removed.
    /// </summary>
    /// <param name="unit">The unit to remove.</param>
    public void RemoveEnemyUnit(EnemyUnit unit)
    {
        deadEnemyUnits.Add(unit);
        unit.portal.enemyUnits.Remove(unit);
        unit.portal.ReduceEnemyCount();
        unit.portal = null;
        unit.gameObject.SetActive(false);
    }

    public void SetGameState(GameState state)
    {
        //gameState = state;
        Logger.DebugLogFormat("GameState changed to {0}", gameState);
        switch(state)
        {
            case GameState.Normal:
                //alertText.text = "";
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                playerUnitType = PlayerUnitType.NoUnitSelected;
                break;

            case GameState.UnitSelectX:
                //disable Y values, then disable X values.
                //TODO: Have dialogue during tutorial to explain how to set units
                DisplayAlert(GetText("unitSelect_x"));
                //alertText.text = GetText("unitSelect_x");
                axisContainerY.gameObject.SetActive(false);
                break;
            
            case GameState.UnitSelectY:
                //disable Y values, then disable X values.
                //TODO: in tutorial, make mention of origin and how it might be used
                DisplayAlert(GetText("unitSelect_y"));
                //alertText.text = GetText("unitSelect_y");
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(false);
                break;
            
            case GameState.CoordinatesSelect:
                break;

            case GameState.DialogueStartOfLevel:
                //find correct dialogue to display.
                bool dialogueFound = false;
                int j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    if (dialogueManager.condTriggerList[triggerIndex].dialogueId == dialogueManager.dialogueList[j].dialogueId)
                    {
                        dialogueFound = true;
                        string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        dialogueManager.dialogueList[j].ShowDialogue(dKey);
                    }
                    else
                    {
                        j++;
                    }
                }
                break;
            
            case GameState.DialogEndOfLevel:
                break;

            case GameState.BaseDestroyed:
                break;

            case GameState.LevelComplete:
                //if all levels complete, show congrats
                break;
        }
    }

    public void ScreenShake()
    {
        screenShakeCoroutine = StartCoroutine(ShakeCamera());
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void DisplayAlert(string msg)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        //StopCoroutine(Alert(""));   //just in case coroutine was running. Otherwise msg will disappear.
        alertText.text = msg;
    }

    public void DisplayAlertWithDuration(string msg, float duration = 4)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        alertCoroutine = StartCoroutine(Alert(msg, duration));
    }

    #region Coroutines

    //set the camera position to a random value temporarily, then set back to normal
    IEnumerator ShakeCamera()
    {
        float shakeValue = 0.3f;
        float xValue = Random.Range(-shakeValue, shakeValue);
        float yValue = Random.Range(-shakeValue, shakeValue);
        Vector3 originalPos = Camera.main.transform.position;

        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + xValue, Camera.main.transform.position.y + yValue, Camera.main.transform.position.z);
        yield return new WaitForSeconds(0.04f);

        Camera.main.transform.position = originalPos;
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
