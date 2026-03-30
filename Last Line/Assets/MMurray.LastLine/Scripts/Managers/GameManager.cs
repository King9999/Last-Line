using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEditor;


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
        BaseDestroyed, LevelComplete,
    }

    public enum TutorialState
    {
        Normal, UnitSelectX, UnitSelectY, DialogueStartOfLevel, DialogEndOfLevel,
        UnitSelectSecondUnit, UnitSelectSecondUnitX, UnitSelectSecondUnitY, BuildingSecondUnit, 
        ActivateAbilitySelectX, ActivateAbilitySelectY,
        BaseDestroyed, LevelComplete,
    }

    public enum PlayerUnitType {NoUnitSelected, Base, StraightShooter, VerticalShooter, TrackingBeam, Bomb}
    public PlayerUnitType playerUnitType;
    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public TutorialState tutorialState, previousTutorialState;
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public List<Point> points;                //the locations on the map where all units are placed.
    public int currentLevel;
    public bool inTutorial;                 //when true, units can be created for free and the game doesn't run normally.

    [Header("---Level File---")]
    [SerializeField]private TextAsset levelFile;
    Levels levelData;

    [Header("---UI---")]
    public TextMeshProUGUI alertText;           //used to inform player of various things.
    //public Dialogue dialogueBox;
    public DialogueManager dialogueManager;
    public int triggerIndex;                //used to iterate through conditional triggers.
    [SerializeField]private ProgressBar[] progressBars;  //used to show build times for units.
    private int progressIndex;              //keeps track of which progress bar is active and filling up.
    private int availableProgressIndex;     //always points to an inactive progress bar.
    public Laser testLaser;
    [SerializeField]private List<MoneyText> moneyTexts;    //is displayed when an enemy unit is destroyed.

    

    //unit setup
    [Header("---Active Units & Portals---")]
    [SerializeField]private List<PlayerUnit> activePlayerUnits;
    [SerializeField]private List<EnemyUnit> activeEnemyUnits;
    [SerializeField]private List<Portal> activePortals;
    private Queue<PlayerUnit> buildQueue;        //the list of units to build after build time.
    private float[] buildTimer;
    private int buildLimit { get; } = 3;          //player can only build 3 units at a time.
    public PlayerUnit testUnit;
    public Portal testPortal;
    public PlayerUnit_Base playerBase;              //there's only 1 of these

    [Header("---Money---")]
    public int money;                               //how much money player has to create units.
    [SerializeField]private int crystalThreshold;   //the amount of money needed for a crystal to appear. Resets once threshold is hit.
    private int accumulatedMoney;
    public Crystal crystal;
    //public int[] unitCosts;

    [Header("---Containers---")]
    [SerializeField]private GameObject playerUnitContainer;      //all instantiated units are placed here
    //[SerializeField]private GameObject enemyUnitContainer;      //all instantiated units are placed here
    [SerializeField]private GameObject axisContainerX;
    [SerializeField]private GameObject axisContainerY;
    [SerializeField]private GameObject moneyTextContainer;

    [Header("---Prefabs---")]
    public Bullet bulletPrefab;
    public Laser laserPrefab;
    [SerializeField]private Portal portalPrefab;
    [SerializeField]private PlayerUnit_Base basePrefab;
    [SerializeField]private PlayerUnit_StraightShooter straightShooterPrefab;
    [SerializeField]private PlayerUnit_VerticalShooter verticalShooterPrefab;
    [SerializeField]private PlayerUnit_TrackingBeam trackingBeamPrefab;
    [SerializeField]private PlayerUnit_Bomb bombPrefab;
    [SerializeField]private EnemyUnit scoutPrefab;
    [SerializeField]private EnemyUnit strikerPrefab;
    [SerializeField]private EnemyUnit dozerPrefab;
    [SerializeField]private EnemyUnit juggPrefab;
    [SerializeField]private PowerUpSpace powerUpPrefab;
    [SerializeField]private MoneyText moneyTextPrefab;


    /***Units that are destroyed are sent to an array, and recycled whenever a new unit needs to be generated. 
    This is to reduce the amount of calls to the garbage collector. */
    [Header("---Graveyard---")]
    public List<EnemyUnit> deadEnemyUnits;
    public List<PlayerUnit> deadPlayerUnits;

    /* coroutines. Having references to coroutines is ideal for performance and convenience, and StopCoroutine() actually works
    when coroutines are executed this way. */
    Coroutine alertCoroutine = null;
    Coroutine screenShakeCoroutine = null;
    Coroutine animateMoneyCoroutine = null;     

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;
        buildQueue = new Queue<PlayerUnit>();
        buildTimer = new float[buildLimit];
        progressIndex = 0;
        availableProgressIndex = 0;

        //both values should be false
        enteringValueX = false;
        enteringValueY = false;

        //crystal setup
        crystal.gameObject.SetActive(false);
        /*unitCosts = new int[4];
        unitCosts[0] = straightShooterPrefab.unitData.unitCost;
        unitCosts[1] = verticalShooterPrefab.unitData.unitCost;
        unitCosts[2] = trackingBeamPrefab.unitData.unitCost;
        unitCosts[3] = bombPrefab.unitData.unitCost;*/

        //UI setup
        alertText.text = "";

        //load save state
        Singleton s = Singleton.instance;
        if (s.saveStateFound)
        {
            s.saveState.LoadState(s.saveState.ReadState);
            //s.saveState.ReadState(s.gameData);
        }
        LoadLevel(currentLevel);

        //dialogueManager.dialogueList[0].ShowDialogue(dialogueManager.dialogueList[0].dialogueKeyList[0]);
        if (inTutorial)
            SetGameState(tutorialState = TutorialState.DialogueStartOfLevel);
        else
            SetGameState(gameState = GameState.DialogueStartOfLevel);
        //ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
        //am.arrowGuides[0].arrowList[0].PointArrow(am.arrowGuides[0].arrowList[0].targetedObject, Vector2.right);

        //TODO: ATTEMPTED TO ROTATE LASER AND HAVE IT TRAVEL IN THAT DIRECTION. ALMOST WORKING!
        /*testLaser = Instantiate(laserPrefab);
        //testLaser.gameObject.SetActive(false);
        testLaser.transform.position = new Vector2(-3, -3);
        testLaser.moveSpeed = 0.1f;

        //testLaser.forwardDirection = testLaser.transform.rotation * Vector2.up;
        //testLaser.transform.Rotate(new Vector3(0, 0, 90));
        
        Vector2 fireDir = new Vector2(testPortal.transform.position.x - testLaser.transform.position.x, 
             testPortal.transform.position.y - testLaser.transform.position.y);
        float z = testLaser.GetAngle(testLaser.transform.position, fireDir);
        testLaser.FireWeapon(fireDir, z);*/
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

        //update build time for units
        if (buildQueue.Count > 0)
        {
            PlayerUnit unit = buildQueue.Peek();

            //update progress bar
            Menu menu = Singleton.instance.Menu;
            progressBars[progressIndex].radialBar.fillAmount = (Time.time - buildTimer[progressIndex]) / unit.buildTime;

            if (Time.time > buildTimer[progressIndex] + unit.buildTime)
            {
                //deploy unit.
                activePlayerUnits.Add(unit);
                unit.gameObject.SetActive(true);
                buildQueue.Dequeue();

                /****TUTORIAL CHECK*****/
                //after the first unit is built, open dialogue to start building second unit.
                if (inTutorial)
                {
                    switch(tutorialState)
                    {
                        case TutorialState.Normal:
                            SetGameState(tutorialState = TutorialState.UnitSelectSecondUnit);
                            break;
                    }
                }

                //update build queue in menu by shifting all images to the left.
                menu.queueIndex = buildQueue.Count;     //this ensures that index points to correct spot
                if (menu.queueIndex > 0)
                {
                    for (int i = menu.queueIndex; i > 0; i--)
                    {
                        //copy image and replace image in front 
                        menu.unitsInQueue[menu.queueIndex - 1].sprite = menu.unitsInQueue[menu.queueIndex].sprite;
                        menu.unitsInQueue[menu.queueIndex].enabled = false;
                        progressBars[progressIndex].DisplayProgressBar(false);
                    }
                }
                else
                {
                    menu.unitsInQueue[menu.queueIndex].enabled = false;
                    progressBars[progressIndex].DisplayProgressBar(false);
                }
                
                progressIndex = (progressIndex + 1 < buildLimit) ? progressIndex + 1 : 0;
                buildTimer[progressIndex] = Time.time;     //build next unit if applicable.
            }
        }
    }

    /*gives money to player and at at the same time calls a coroutine to display amount on screen. Also handles generating
    a crystal if threshold is met. If isCrystal = true, player collected money from a crystal and doesn't contribute to
    threshold. */
    public void AddMoney(int amount, Vector2 position, bool isCrystal = false)
    {
        if (amount < 0 || money >= 999)
            return;

        //int oldAmount = money;
        //int newAmount = money + amount;
        //animateMoneyCoroutine = StartCoroutine(AnimateMoney(newAmount));
        money += amount;
        if (money > 999)
            money = 999;

        Menu menu = Singleton.instance.Menu;
        menu.UpdateMoneyText();

        if (!isCrystal)
            accumulatedMoney += amount;

        /* find an inactive money text object and activate it. If there are none available, then instantiate a new one */
        if (moneyTexts.Count <= 0)
        {
            MoneyText text = Instantiate(moneyTextPrefab, moneyTextContainer.transform);
            text.ShowMoneyRewardText(amount, position);
            moneyTexts.Add(text);
            //moneyTexts[moneyTexts.Count - 1].ShowMoneyRewardText(amount, position);
        }
        else
        {
            //search the list
            bool found = false;
            int i = 0;
            while(!found && i < moneyTexts.Count)
            {
                if(!moneyTexts[i].gameObject.activeSelf)
                {
                    //moneyTexts[i].gameObject.SetActive(true);
                    moneyTexts[i].ShowMoneyRewardText(amount, position);
                    found = true;
                }
                else
                {
                    i++;
                }
            }

            if (!found)
            {
                MoneyText text = Instantiate(moneyTextPrefab, moneyTextContainer.transform);
                text.ShowMoneyRewardText(amount, position);
                moneyTexts.Add(text);
            }
        }

        /******Crystal Generation******/
        if (crystalThreshold > 0 && accumulatedMoney >= crystalThreshold)
        {
            accumulatedMoney = 0;

            //generate a crystal and it to random point

            int i = 0;
            do
            {
                i = Random.Range(0, points.Count);
                crystal.transform.position = Camera.main.ScreenToWorldPoint(points[i].transform.position);
            }
            while (points[i].unit != null);

            points[i].crystal = crystal;

            if (!crystal.gameObject.activeSelf)
                crystal.gameObject.SetActive(true);
               
        }

    }

    bool CancelButtonPressed()
    {
        //cancel button is disabled in tutorial
        if (inTutorial)
            return false;

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

        /************get level from JSON***********/
        levelData = JsonUtility.FromJson<Levels>(levelFile.text);

        //which level?
        if (level < 0 || level >= levelData.levels.Length)
        {
            Logger.DebugLogWarning("Invalid level!");
            return;
        }

        //tutorial check
        if (level == 0)
           inTutorial = true;

        Logger.DebugLogFormat("Level Count: {0}", levelData.levels.Length);
        //Logger.DebugLog("Levels: " + levelData.levels[1].starting_money);
        //generate base
        //PlaceUnit(PlayerUnitType.Base, new Vector2(1, 3));

        //generate portals
        //each portal instantiates number of units.
        //EnemyUnit enemyUnit = null;
        //testPortal.enemyCount = 3;
        /*for (int i = 0; i < testPortal.enemyCount; i++)
        {
            EnemyUnit enemyUnit = Instantiate(scoutPrefab, parent: testPortal.transform);
            enemyUnit.transform.position = testPortal.transform.position;
            activeEnemyUnits.Add(enemyUnit);
            enemyUnit.portal = testPortal;
            enemyUnit.gameObject.SetActive(false);
            testPortal.enemyUnits.Add(enemyUnit);
        }*/
        for (int i = 0; i < levelData.levels[level].portals.Count; i++)
        {
            /******PORTAL SETUP*******/
            //set portal's point location
            PortalData portalData = levelData.levels[level].portals[i];
            Portal portal = null;
            //reuse existing portals if able
            if (i < activePortals.Count)
            {
                //search for an inactive portal and activate it
                portal = activePortals[i];
                portal.gameObject.SetActive(true);
            }
            else
            {
                portal = Instantiate(portalPrefab);
                //portal.SetupPortal();       //important to call this
                activePortals.Add(portal);
            }
            portal.pointAxes = portalData.point_axes;
            portal.transform.position = Camera.main.ScreenToWorldPoint(portalData.point_position);

            //have to set portal's Z to 0 since it takes the camera's Z value as its own, which is -10
            portal.transform.position = new Vector3(portal.transform.position.x, portal.transform.position.y, 0);
            SetPointRef(portal, portal.pointAxes);

            //set enemy type and count
            portal.enemyType = portalData.enemy_type;
            portal.enemyCount = portalData.enemy_count;
            

            /******Enemy setup*****/
            EnemyUnit unit = null;
            switch(portal.enemyType)
            {
                case Portal.EnemyType.Scout:
                    //rename portal
                    portal.name = "Portal - Scout";
                    EnemyUnit_Scout scout = unit as EnemyUnit_Scout;
                    InstantiateEnemyUnit(scout, portal);
                    break;

                case Portal.EnemyType.Striker:
                    portal.name = "Portal - Striker";
                    break;
                
                case Portal.EnemyType.Dozer:
                    portal.name = "Portal - Dozer";
                    break;

                case Portal.EnemyType.Juggernaut:
                    portal.name = "Portal - Juggernaut";
                    break;
            }
           

            //set enemy path
            portal.enemyPath.positionCount = portalData.enemy_path_points.Length;
            portal.enemyPath.SetPositions(portalData.enemy_path_points);
            //portal.SetEnemyPath();
            //portal.enemyPath.SetPosition(0, portalData.enemy_path_points[0]);

            //set timer and cooldown
            portal.timer.SetTimer(portalData.deploy_time);
            portal.deployCooldown = portalData.deploy_cooldown;

            portal.SetupPortal();       //important to call this
            //activePortals.Add(portal);

        }

        /******BASE SETUP******/
        LevelData newLevel = levelData.levels[level];
        playerBase.transform.position = Camera.main.ScreenToWorldPoint(newLevel.player_base_position);
        playerBase.transform.position = new Vector3(playerBase.transform.position.x, playerBase.transform.position.y, 0);
        SetPointRef(playerBase, newLevel.base_point_axes);


        /*******CRYSTAL & STARTING MONEY SETUP******/
        crystalThreshold = newLevel.crystal_threshold;
        crystal.SetMoneyValue(newLevel.crystal_value);

        /******AVAILABLE PLAYER UNITS******/
        //by default, all unit buttons are visible.
        Menu menu = Singleton.instance.Menu;
        for (int i = 0; i < newLevel.toggle_unit.Length; i++)
        {
            menu.unitButtons[i].gameObject.SetActive(newLevel.toggle_unit[i]);
        }

    }

    #region Unit/Portal Management

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
                //return;
            }
            else if (point.crystal != null)
            {
                //give player money.
                DisplayAlertWithDuration(GetText("crystalCollected"));
                AddMoney(crystal.GetMoneyValue(), crystal.transform.position, isCrystal: true);
                point.crystal = null;
                crystal.gameObject.SetActive(false);
            }
            else
            {
                DisplayAlertWithDuration(GetText("nothingAtPoint"));
            }
        }
        else
        {
            if (buildQueue.Count >= buildLimit)
            {
                //Logger.DebugLog("Build queue full!");
                DisplayAlertWithDuration(GetText("buildQueueFull"));
                SetGameState(gameState = GameState.Normal);
                return;
            }

            if (!point.IsEmpty() /*point.unit != null || point.crystal != null || point.portal != null*/)
            {
                //unit is occupying this space or is being built, or a crystal is there.
                DisplayAlertWithDuration(GetText("spaceOccupied"));
                SetGameState(gameState = GameState.Normal);
                return;
            }

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

                case PlayerUnitType.TrackingBeam:
                    newUnit = Instantiate(trackingBeamPrefab, playerUnitContainer.transform);
                    break;

                case PlayerUnitType.Bomb:
                    newUnit = Instantiate(bombPrefab, playerUnitContainer.transform);
                    break;
            }


            //must convert the point's location to world coordinates.
            Vector2 newPos = Camera.main.ScreenToWorldPoint(point.transform.position);
            newUnit.transform.position = newPos;
            //Logger.AddPlayerId(newUnit.name);
            //newUnit.UpdateHealthBar(newPos);        //TODO: Not quite sure why this fixes the healthbar positioning issue for player units.
            point.unit = newUnit;
            newUnit.gameObject.SetActive(false);    //stay hidden until finished building.

            //add to list
            DisplayAlertWithDuration(GetText("buildingUnit"));

            //spend money
            Menu menu = Singleton.instance.Menu;
            money -= newUnit.unitCost;
            menu.UpdateMoneyText();

            if (buildQueue.Count <= 0)  //make sure progressIndex is reset when build queue is empty
                progressIndex = 0;

            buildQueue.Enqueue(newUnit);
            

            //add the unit's image in menu's build queue
            menu.queueIndex = buildQueue.Count - 1;     //this ensures that index points to correct spot

            //we do this check to prevent a build in progress from getting its time reset whenever additional units
            //beyond the 1st enter the queue. The build timer will get updated in the update loop.
            if (buildQueue.Count == 1)
                buildTimer[menu.queueIndex] = Time.time;

            menu.unitsInQueue[menu.queueIndex].enabled = true;
            menu.unitsInQueue[menu.queueIndex].sprite = newUnit.unitSprite;

            //find an available progress bar and activate it.
            bool barFound = false;
            int i = 0;
            while (!barFound && i < progressBars.Length)
            {
                if (!progressBars[i].ProgressBarActive())
                {
                    //availableProgressIndex = i;
                    barFound = true;
                }
                else
                {
                    i++;
                }
            }

            if (i < progressBars.Length)
                progressBars[i].DisplayProgressBar(true, point);
            //activePlayerUnits.Add(newUnit);

            //save unit's position
            Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
            //playerUnitType = PlayerUnitType.NoUnitSelected;
            SetGameState(gameState = GameState.Normal);
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
            if (point == points[i].pointAxes)
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
        //newUnit.UpdateHealthBar(newPos);        //TODO: Not quite sure why this fixes the healthbar positioning issue for player units.
        points[i].unit = newUnit;

        //add to list
        activePlayerUnits.Add(newUnit);
    }

    /// <summary>
    /// Sets portal's reference to a point using the given axes
    /// </summary>
    /// <param name="portal"></param>
    /// <param name="pointAxes"></param>
    void SetPointRef(Portal portal, Vector2 pointAxes)
    {
        bool pointFound = false;
        int i = 0;
        while(!pointFound && i < points.Count)
        {
            if (pointAxes == points[i].pointAxes)
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

        points[i].portal = portal;
        portal.point = points[i];
        //portal.transform.position = points[i].WorldPoint();

        //portal.pointAxes = portalData.point_axes;
        //portal.transform.position = Camera.main.ScreenToWorldPoint(portalData.point_position);

        //have to set portal's Z to 0 since it takes the camera's Z value as its own, which is -10
        //portal.transform.position = new Vector3(portal.transform.position.x, portal.transform.position.y, 0);
    }

    void SetPointRef(PlayerUnit_Base playerBase, Vector2 pointAxes)
    {
        bool pointFound = false;
        int i = 0;
        while(!pointFound && i < points.Count)
        {
            if (pointAxes == points[i].pointAxes)
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

        playerBase.point = points[i];
    }

    /// <summary>
    /// Instantiates enemy unit to a given portal. Instantiate is skipped if an enemy unit
    /// exists in the graveyard.
    /// </summary>
    /// <param name="enemyType">The type of enemy to instantiate.</param>
    /// <param name="portal">The portal to where the enemy will be assigned.</param>
    void InstantiateEnemyUnit(EnemyUnit enemy, Portal portal)
    {
        if (portal == null)
        {
            Logger.DebugLogWarning("null portal!");
            return;
        }

        EnemyUnit unit = null;
        for (int i = 0; i < portal.enemyCount; i++)
        {
            if (deadEnemyUnits.Contains(enemy))
            {
                //remove unit from graveyard
                enemy.GetUnitData(enemy.unitData);
                enemy.portal = portal;
                portal.enemyUnits.Add(enemy);
                deadEnemyUnits.Remove(enemy);
            }
            else
            {
                //get the enemy type and instantiate correct enemy
                switch(portal.enemyType)
                {
                    case Portal.EnemyType.Scout:
                        unit = Instantiate(scoutPrefab, portal.transform);
                        break;

                    case Portal.EnemyType.Striker:
                        unit = Instantiate(strikerPrefab, portal.transform);
                        break;
                    
                    case Portal.EnemyType.Dozer:
                        unit = Instantiate(dozerPrefab, portal.transform);
                        break;

                    case Portal.EnemyType.Juggernaut:
                        unit = Instantiate(juggPrefab, portal.transform);
                        break; 
                }
                
                unit.portal = portal;
                unit.transform.position = portal.transform.position;
                unit.gameObject.SetActive(false);
                portal.enemyUnits.Add(unit);
            }
        }
        /*switch(enemyType)
        {
            case Portal.EnemyType.Scout:
                //rename portal
                portal.name = "Portal - Scout";
                EnemyUnit_Scout scout = unit as EnemyUnit_Scout;
                if (deadEnemyUnits.Contains(scout))
                {
                    //remove unit from graveyard
                    scout.GetUnitData(scout.unitData);
                    scout.portal = portal;
                    activeEnemyUnits.Add(scout);
                    deadEnemyUnits.Remove(scout);
                }
                else
                {
                    unit = Instantiate(scoutPrefab, portal.transform);
                    unit.portal = portal;
                    unit.gameObject.SetActive(false);
                    activeEnemyUnits.Add(unit);
                }
                break;

            case Portal.EnemyType.Striker:
                break;
            
            case Portal.EnemyType.Dozer:
                break;

            case Portal.EnemyType.Juggernaut:
                break;
        }*/
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
                Logger.DebugLogFormat("Removing {0} from grid at point {1}", unit.name, points[i].pointAxes);
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
        Logger.DebugLogFormat("Removing {0}", unit.name);
        unit.gameObject.SetActive(false);
    }
    #endregion

    #region Game States
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

                //if we're in the tutorial, do some extra steps
                /*if (inTutorial)
                {
                    //display dialogue
                    previousState = state;  //need this so the unit selection isn't lost.
                    string dKey = dialogueManager.dialogueList[1].dialogueKeyList[0];
                    dialogueManager.dialogueList[1].ShowDialogue(dKey);
                }*/
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
                    if (dialogueManager.condTriggerList[currentLevel].dialogueId == dialogueManager.dialogueList[j].dialogueId)
                    {
                        dialogueFound = true;
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
            
            case GameState.DialogEndOfLevel:
                break;

            case GameState.BaseDestroyed:
                break;

            case GameState.LevelComplete:
                //deactivate current portals for use in the next level.
                foreach (Portal portal in activePortals)
                {
                    portal.gameObject.SetActive(false);
                }
                //load next level
                //if all levels complete, show congrats
                break;
        }
    }

    public void SetGameState(TutorialState state)
    {
        Logger.DebugLogFormat("Tutorial State changed to {0}", tutorialState);
        ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
        Dialogue.DialogueKeyList keyList;       //going to call this a lot

        switch(state)
        {
            case TutorialState.Normal:
                //hide any arrow guides that were shown
                am.HideGuideArrows();

                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                playerUnitType = PlayerUnitType.NoUnitSelected;
                break;

            case TutorialState.UnitSelectX:
                //disable Y values, then disable X values.
                DisplayAlert(GetText("unitSelect_x"));
                axisContainerY.gameObject.SetActive(false);

                //if we're in the tutorial, do some extra steps
                //if (inTutorial)
                //{
                    //hide any arrow guides that were shown
                    //ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
                    //am.HideGuideArrows();

                    //display dialogue
                    keyList = dialogueManager.dialogueList[DialogueManager.SELECTING_X_VALUE].dialogueKeyList[0];
                    //string dKey = dialogueManager.dialogueList[DialogueManager.SELECTING_X_VALUE].dialogueKeyList[0].key;
                    dialogueManager.dialogueList[DialogueManager.SELECTING_X_VALUE].ShowDialogue(keyList);
                //}
                break;
            
            case TutorialState.UnitSelectY:
                //disable Y values, then disable X values.
                //TODO: in tutorial, make mention of origin and how it might be used
                DisplayAlert(GetText("unitSelect_y"));
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(false);
                if (inTutorial)
                {
                    //hide any arrow guides that were shown
                    //ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
                    am.HideGuideArrows();

                    //display dialogue
                    keyList = dialogueManager.dialogueList[DialogueManager.SELECTING_Y_VALUE].dialogueKeyList[0];
                    //string dKey = dialogueManager.dialogueList[DialogueManager.SELECTING_Y_VALUE].dialogueKeyList[0].key;
                    dialogueManager.dialogueList[DialogueManager.SELECTING_Y_VALUE].ShowDialogue(keyList);
                }
                break;
            
            case TutorialState.UnitSelectSecondUnit:
                //player must now select second unit. Show dialogue from mother base
                //am.HideGuideArrows();
                keyList = dialogueManager.dialogueList[DialogueManager.SELECTING_SECOND_UNIT].dialogueKeyList[0];
                dialogueManager.dialogueList[DialogueManager.SELECTING_SECOND_UNIT].ShowDialogue(keyList);
                break;

            case TutorialState.UnitSelectSecondUnitX:
                //show arrow pointing to value. Player cannot pick other values.
                am.HideGuideArrows();
                am.arrowUnitSelectX.PointArrow(am.arrowUnitSelectX.targetedObject, am.arrowUnitSelectX.direction);
                break;
            
            case TutorialState.UnitSelectSecondUnitY:
                //show arrow pointing to value. Player cannot pick other values.
                am.arrowUnitSelectX.gameObject.SetActive(false);
                am.arrowUnitSelectY.PointArrow(am.arrowUnitSelectY.targetedObject, am.arrowUnitSelectY.direction);
                break;
            
            case TutorialState.BuildingSecondUnit:
                //show dialogue informing player to redirect unit to fire in opposite direction.
                am.arrowUnitSelectY.gameObject.SetActive(false);
                keyList = dialogueManager.dialogueList[DialogueManager.ACTIVATING_UNIT_ABILITY].dialogueKeyList[0];
                dialogueManager.dialogueList[DialogueManager.ACTIVATING_UNIT_ABILITY].ShowDialogue(keyList);
                break;
            
            case TutorialState.ActivateAbilitySelectX:
                am.arrowUnitSelectX.PointArrow(am.arrowUnitSelectX.targetedObject, am.arrowUnitSelectX.direction);
                break;
            
            case TutorialState.ActivateAbilitySelectY:
                am.arrowUnitSelectX.gameObject.SetActive(false);
                am.arrowUnitSelectY.PointArrow(am.arrowUnitSelectY.targetedObject, am.arrowUnitSelectY.direction);
                break;
            

            case TutorialState.DialogueStartOfLevel:
                //find correct dialogue to display.
                bool dialogueFound = false;
                int j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    if (dialogueManager.condTriggerList[currentLevel].dialogueId == dialogueManager.dialogueList[j].dialogueId)
                    {
                        dialogueFound = true;
                        keyList = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        //string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0].key;
                        dialogueManager.dialogueList[j].ShowDialogue(keyList);
                    }
                    else
                    {
                        j++;
                    }
                }
                break;
            
            case TutorialState.DialogEndOfLevel:
                break;

            case TutorialState.BaseDestroyed:
                break;

            case TutorialState.LevelComplete:
                //deactivate current portals for use in the next level.
                foreach (Portal portal in activePortals)
                {
                    portal.gameObject.SetActive(false);
                }
                //load next level
                break;
        }
    }
    #endregion

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
        SpeakText(msg);
    }

    public void DisplayAlertWithDuration(string msg, float duration = 4)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);
        alertCoroutine = StartCoroutine(Alert(msg, duration));
        SpeakText(msg);
    }

    void SpeakText(string key)
    {
        if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(key);
            Logger.DebugLog("TTS is playing");
        }
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

    /*IEnumerator AnimateMoney(int newAmount)
    {
        //int currentMoney = oldAmount;
        Menu menu = Singleton.instance.Menu;
        while (money < newAmount)
        {
            money++;
            menu.UpdateMoneyText();
            yield return new WaitForSeconds(0.05f);
        }
    }*/
    #endregion
}
