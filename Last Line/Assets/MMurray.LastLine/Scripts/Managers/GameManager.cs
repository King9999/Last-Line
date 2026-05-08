using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEditor;
using MMurray.GenericCode;


public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Game State
    /// </summary>
    public enum GameState
    {
        Normal, EnemyPreparing,                //when game state is normal the game is running. Otherwise the game is paused.
        UnitSelectX, UnitSelectY,               //player is entering coordinates
        StartLevel, DialogueStartOfLevel, DialogueEndOfLevel, DialogueGameOver,
        SelectingValueX, SelectingValueY,       //used when placing units on screen.
        BaseDestroyed, LevelComplete, GoToNextLevel, GameCompleted
    }

    public enum TutorialState
    {
        Normal, UnitSelectX, UnitSelectY, DialogueStartOfLevel, DialogueEndOfLevel,
        UnitSelectSecondUnit, UnitSelectSecondUnitX, UnitSelectSecondUnitY, BuildingFirstUnit, BuildingSecondUnit, 
        ActivateAbility, ActivateAbilitySelectX, ActivateAbilitySelectY,
        DialogueEnemyIncoming, DeployEnemyUnits, BaseDestroyed, LevelComplete,
        CrystalIntro
    }

    public enum PlayerUnitType {NoUnitSelected, /*Base,*/ StraightShooter, VerticalShooter, TrackingBeam, Bomb}
    public PlayerUnitType playerUnitType;
    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public TutorialState tutorialState, previousTutorialState;
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public List<Point> points;                //the locations on the map where all units are placed.
    public int currentLevel;
    public int maxLevels {get;} = 8;
    public bool inTutorial;                 //when true, units can be created for free and the game doesn't run normally.
    public bool gameStarted;                //while false, the "ready to play" button appears
    public int totalEnemies;                //when this value is 0, the level is over.

    [Header("---Level File---")]
    [SerializeField]private TextAsset levelFile;
    Levels levelData;

    [Header("---UI---")]
    public TextMeshProUGUI alertText;           //used to inform player of various things.
    //public Dialogue dialogueBox;
    public DialogueManager dialogueManager;
    public int triggerIndex;                //used to iterate through conditional triggers.
    [SerializeField]private ProgressBar[] progressBars;  //used to show build times for units.
    private int progressIndex, activeBarIndex;              //keeps track of which progress bar is active and filling up.
    private int availableProgressIndex;     //always points to an inactive progress bar.
    public Laser testLaser;
    [SerializeField]private List<MoneyText> moneyTexts;    //is displayed when an enemy unit is destroyed.
    public Button readyButton;     //used to advance game by 60 seconds to start early.
    //public Button exitButton;      //ends game when last level completed.
    [SerializeField]private TextMeshProUGUI readyLabel;
    [SerializeField]private TextMeshProUGUI exitGameLabel;
    [SerializeField]private EndLevelText endLevelText;       //displays "BASE DEFENDED"
    [SerializeField]private GameOverText gameOverText;       //displays "BASE DESTROYED"
    [SerializeField]private TargetCursor targetCursor;

    

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
    public PowerUpSpace powerUpSpace;               //1 per level.

    [Header("---Particle Effects---")]
    [SerializeField]private List<Explosion> explosionList;
    [SerializeField]private BaseExplosion baseExplosion;
    [SerializeField]private BombExplosion bombExplosion;


    [Header("---Money---")]
    public int money;                               //how much money player has to create units. In game, it's called scrap.
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
    [SerializeField]private GameObject explosionContainer;          //handles explosion particle effect

    [Header("---Planets---")]
    [SerializeField]GameObject smallPlanet;
    [SerializeField]GameObject mediumPlanet;
    [SerializeField]GameObject largePlanet;

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
    [SerializeField]private Explosion explosionPrefab;


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
    Coroutine explodeBase = null;

    /****bools for various tutorial checks****/
    public bool crystalTutorialViewed;     

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
        money = 0;
        crystal.gameObject.SetActive(false);

        //UI setup
        alertText.text = "";
        readyLabel.text = GetText("readyButtonText");
        exitGameLabel.text = GetText("exitButtonText");
        //exitButton.gameObject.SetActive(false);     //only appears at end of game.
        targetCursor.gameObject.SetActive(false);

        //load save state
        Singleton s = Singleton.instance;
        if (s.saveStateFound)
        {
            s.saveState.LoadState(s.saveState.ReadState);
            //s.saveState.ReadState(s.gameData);
        }

        //currentLevel = 2;
        LoadLevel(currentLevel);

        //dialogueManager.dialogueList[0].ShowDialogue(dialogueManager.dialogueList[0].dialogueKeyList[0]);
        if (inTutorial)
        {
            SetGameState(tutorialState = TutorialState.DialogueStartOfLevel);

        }
        else
        {
            SetGameState(gameState = GameState.StartLevel);
        }
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

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Restart"))
        {
            Logger.DebugLog("Restarting level");
            Singleton.instance.ScreenFade.LoadLevelFadeOut(currentLevel);
        }
    }

    /// <summary>
    /// Used to check when the Update loop should stop running when the game enters a state where the player
    /// should no longer play.
    /// </summary>
    /// <returns></returns>
    public bool UnplayableState()
    {
        bool cannotPlay = false;
        if (gameState == GameState.BaseDestroyed || gameState == GameState.LevelComplete || gameState == GameState.DialogueEndOfLevel ||
            gameState == GameState.GameCompleted)
            cannotPlay = true;
        
        if (inTutorial && (tutorialState == TutorialState.LevelComplete || tutorialState == TutorialState.DialogueEndOfLevel))
            cannotPlay = true;
        
        //Logger.DebugLogFormat("Game is in unplayable state? {0}", cannotPlay);
        return cannotPlay;
    }

    //if at least 1 portal is ready to send enemies, return true.
    bool PortalReady()
    {
        bool portalReady = false;
        int i = 0;
        while(!portalReady && i < activePortals.Count)
        {
            if (activePortals[i].timer.TimeUp())
            {
                portalReady = true;
            }
            else
            {
                i++;
            }
        }

        return portalReady;
    }

    public void PlayMusic(bool toggle)
    {
        AudioManager am = Singleton.instance.AudioManager;
        if (toggle == true)
            am.mainMusic.Play();
        else
            am.mainMusic.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (UnplayableState())
            return;

        if (!inTutorial && !gameStarted)
        {
            if (PortalReady())
            {
                //game has started, enemies start attacking.
                gameStarted = true;
                //readyButton.gameObject.SetActive(false);
                SetGameState(gameState = GameState.Normal);
            }

            //if (!gameStarted)
                //return;
        }

        if (totalEnemies <= 0 && (GameRunning() /*gameState == GameState.Normal*/ || (inTutorial && tutorialState == TutorialState.Normal))) //gameState != GameState.EnemyPreparing && gameState != GameState.LevelComplete && gameState != GameState.GameCompleted && 
            //gameState != GameState.BaseDestroyed)
        {
            //level is over.
            //if base is destroyed, then it's game over.
            if (!inTutorial)
            {
                if (playerBase.hitPoints <= 0 && gameState != GameState.BaseDestroyed)
                {
                    SetGameState(gameState = GameState.BaseDestroyed);
                }
                else
                    SetGameState(gameState = GameState.LevelComplete);
            }
            else
            {
                SetGameState(tutorialState = TutorialState.LevelComplete);
            }

            return;
        }


        //check for right click or Escape key. They're used to cancel certain actions.
        if (CancelButtonPressed())
        {
            if (gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY)
            {
                //cancel actions and return game state to normal.
                //StopCoroutine(Alert("", 0));

                DisplayAlertWithDuration(GetText("cancelAction"));

                //if game hasn't started yet, we don't go to normal state.
                SetGameState(gameState = gameStarted ? GameState.Normal : GameState.EnemyPreparing);
            }
        }

        //update build time for units
        if (buildQueue.Count > 0)
        {
            PlayerUnit unit = buildQueue.Peek();

            //update progress bar
            Menu menu = Singleton.instance.Menu;
            progressBars[progressIndex].radialBar.fillAmount = (Time.time - buildTimer[progressIndex]) / unit.buildTime;

            //update sound
            AudioManager am = Singleton.instance.AudioManager;
            if (!am.buildSound.isPlaying)
                am.buildSound.Play();

            if (Time.time > buildTimer[progressIndex] + unit.buildTime)
            {
                //deploy unit.
                am.buildSound.Stop();
                SpeakText("unitDeployed");
                activePlayerUnits.Add(unit);
                unit.gameObject.SetActive(true);
                buildQueue.Dequeue();

                /****TUTORIAL CHECK*****/
                //after the first unit is built, open dialogue to start building second unit.
                if (inTutorial)
                {
                    switch(tutorialState)
                    {
                        case TutorialState.BuildingFirstUnit:
                            SetGameState(tutorialState = TutorialState.UnitSelectSecondUnit);
                            break;

                        case TutorialState.BuildingSecondUnit:
                            SetGameState(tutorialState = TutorialState.ActivateAbility);
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
        money = (money + amount <= 999) ? money + amount : 999;

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

            //play sound
            AudioManager am = Singleton.instance.AudioManager;
            am.soundSource.PlayOneShot(am.crystalSpawned, 1);

            //generate a crystal and it to random point. On level 2, the crystal appears at a fixed point.
            if (currentLevel == 2)
            {
                //place crystal at origin
                crystal.transform.position = Camera.main.ScreenToWorldPoint(points[25].transform.position);
                crystal.transform.position = new Vector3(crystal.transform.position.x, crystal.transform.position.y, 1);
                points[25].crystal = crystal;
                Logger.DebugLogFormat("Crystal generated at {0}", points[25].pointAxes);

                //pause game and open dialogue explaining crystals.
                if (!crystalTutorialViewed)
                {
                    crystalTutorialViewed = true;
                    inTutorial = true;
                    SetGameState(tutorialState = TutorialState.CrystalIntro);
                }

                
            }
            else
            {
                int i = 0;
                do
                {
                    i = Random.Range(0, points.Count);
                    crystal.transform.position = Camera.main.ScreenToWorldPoint(points[i].transform.position);
                    crystal.transform.position = new Vector3(crystal.transform.position.x, crystal.transform.position.y, 1);    //must change Z so crystal is visible.
                }
                while (!points[i].IsEmpty());

                points[i].crystal = crystal;
                Logger.DebugLogFormat("Crystal generated at {0}", points[i].pointAxes);
            }

            if (!crystal.gameObject.activeSelf)
                crystal.gameObject.SetActive(true);
               
        }

    }

    /// <summary>
    /// removes all player units and other visible objects in preparation for the next level.
    /// </summary>
    public void Cleanup()
    {
        //deactivate current portals for use in the next level.
        foreach (Portal portal in activePortals)
        {
            //cleanup any enemy units in case of a game over
            for (int i = 0; i < portal.enemyUnits.Count; i++)
            {
                portal.enemyUnits[i].gameObject.SetActive(false);
                deadEnemyUnits.Add(portal.enemyUnits[i]);
                portal.enemyUnits.Remove(portal.enemyUnits[i]);
                i--;
            }
            portal.gameObject.SetActive(false);
        }

        //remove active player units. Must also check the build queue for any units not yet built.
        for (int i = 0; i < activePlayerUnits.Count; i++)
        {
            activePlayerUnits[i].gameObject.SetActive(false);
            deadPlayerUnits.Add(activePlayerUnits[i]);
            activePlayerUnits.Remove(activePlayerUnits[i]);
            i--;
        }

        for (int i = 0; i < buildQueue.Count; i++)
        {
            PlayerUnit unit = buildQueue.Dequeue();
            deadPlayerUnits.Add(unit);
            i--;
        }

        foreach(Point point in points)
        {
            point.ClearPoint();
        }
        

        crystal.gameObject.SetActive(false);
        powerUpSpace.gameObject.SetActive(false);
        playerBase.gameObject.SetActive(false);
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
        //bool gameRunning = false;
        if (inTutorial)
        {
            if (tutorialState == TutorialState.CrystalIntro)
                return false;
            else
                return true; 
        }
        
        return gameState == GameState.Normal || gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY || 
            gameState == GameState.EnemyPreparing;
                //gameRunning = true;
        
        //return !inTutorial && (gameState == GameState.Normal || gameState == GameState.UnitSelectX || gameState == GameState.UnitSelectY || 
            //gameState == GameState.EnemyPreparing);
        //return gameRunning;
    }

    /* Load the specified level. NOTE: Save States only load the current level from the beginning, and not the state
    the level was left in. This is because the levels are going to be short enough that it's unnecessary to record
    every unit's position. Also, the game runs in real time so the player might get confused on what they were doing. */
    public void LoadLevel(int level)
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
        else
           inTutorial = false;

        /*******SAVE DATA********/
        //This is done here because only the current level is saved and nothing else.
        //The player resumes from the beginning of the level.
        Singleton.instance.saveState.WriteState(Singleton.instance.gameData);


        //ready button check. button is disabled by default.
        gameStarted = false;
        readyButton.gameObject.SetActive(false);

        /*if (!inTutorial)
        {
            readyButton.gameObject.SetActive(true);
        }
        else
        {
            //disable ready button by default
            readyButton.gameObject.SetActive(false);
        }*/

        Logger.DebugLogFormat("Level Count: {0}", levelData.levels.Length);
        totalEnemies = 0;

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
            totalEnemies += portal.enemyCount;
            

            /******Enemy setup*****/
            //EnemyUnit unit = null;
            switch(portal.enemyType)
            {
                case Portal.EnemyType.Scout:
                    //rename portal
                    portal.name = "Portal - Scout";
                    //EnemyUnit_Scout scout = unit as EnemyUnit_Scout;
                    InstantiateEnemyUnit(UnitData.UnitType.Scout, portal);
                    break;

                case Portal.EnemyType.Striker:
                    portal.name = "Portal - Striker";
                    //EnemyUnit_Striker striker = unit as EnemyUnit_Striker;
                    InstantiateEnemyUnit(UnitData.UnitType.Striker, portal);
                    break;
                
                case Portal.EnemyType.Dozer:
                    portal.name = "Portal - Dozer";
                    //EnemyUnit_Dozer dozer = unit as EnemyUnit_Dozer;
                    InstantiateEnemyUnit(UnitData.UnitType.Dozer, portal);
                    break;

                case Portal.EnemyType.Juggernaut:
                    portal.name = "Portal - Juggernaut";
                    //EnemyUnit_Juggernaut jugg = unit as EnemyUnit_Juggernaut;
                    InstantiateEnemyUnit(UnitData.UnitType.Juggernaut, portal);
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

        //make base visible in case it was destroyed
        if (!playerBase.gameObject.activeSelf)
            playerBase.gameObject.SetActive(true);

        playerBase.transform.position = Camera.main.ScreenToWorldPoint(newLevel.player_base_position);
        playerBase.transform.position = new Vector3(playerBase.transform.position.x, playerBase.transform.position.y, 0);
        SetPointRef(playerBase, newLevel.base_point_axes);
        playerBase.hitPoints = playerBase.maxHitPoints;
        playerBase.UpdateHealthBar(playerBase.transform.position);

        


        /*******CRYSTAL & STARTING MONEY SETUP******/
        Menu menu = Singleton.instance.Menu;
        crystalThreshold = newLevel.crystal_threshold;
        crystal.SetMoneyValue(newLevel.crystal_value);
        money = newLevel.starting_money;
        menu.UpdateMoneyText();

        /****LEVEL NUMBER SETUP***/
        menu.UpdateLevel(level);

        /*****POWER UP SPACE SETUP******/
        if (levelData.levels[level].power_up_space_active)
        {
            powerUpSpace.gameObject.SetActive(true);
            powerUpSpace.transform.position = Camera.main.ScreenToWorldPoint(newLevel.power_up_space_position);
            powerUpSpace.transform.position = new Vector3(powerUpSpace.transform.position.x, powerUpSpace.transform.position.y, 0);
            SetPointRef(powerUpSpace, newLevel.power_up_space_point_axes);
        }
        else
        {
            powerUpSpace.gameObject.SetActive(false);
        }

        /******AVAILABLE PLAYER UNITS******/
        //by default, all unit buttons are visible.
        for (int i = 0; i < newLevel.toggle_unit.Length; i++)
        {
            menu.unitButtons[i].gameObject.SetActive(newLevel.toggle_unit[i]);
        }

        /*****PLANETS******/
        smallPlanet.gameObject.SetActive(newLevel.show_small_planet);
        mediumPlanet.gameObject.SetActive(newLevel.show_medium_planet);
        largePlanet.gameObject.SetActive(newLevel.show_large_planet);

        /********MUSIC***********/
        AudioManager am = Singleton.instance.AudioManager;
        if (!am.mainMusic.isPlaying)
            PlayMusic(Singleton.instance.musicEnabled);

    }

    void CallExplosion(Vector2 location)
    {
        AudioManager am = Singleton.instance.AudioManager;
        am.soundSource.PlayOneShot(am.unitExplode, am.soundVolume);

        //instantiate explosion only if one doesn't exist already in the explosion list.
        if (explosionList.Count <= 0)
        {
            Explosion explosion = Instantiate(explosionPrefab, explosionContainer.transform);
            explosion.Play(location);
            explosionList.Add(explosion);
        }
        else
        {
            //search for existing particle
            bool found = false;
            int i = 0;
            while(!found && i < explosionList.Count)
            {
                if (!explosionList[i].IsPlaying())
                {
                    found = true;
                    explosionList[i].Play(location);
                }
                else
                {
                    i++;
                }
            }

            //if no particle available, instantiate new one.
            if (!found)
            {
                Explosion explosion = Instantiate(explosionPrefab, explosionContainer.transform);
                explosion.Play(location);
                explosionList.Add(explosion);
            }
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
            targetCursor.ShowCursor(point.transform.position);

            if (point.unit != null && point.unit.gameObject.activeSelf)     //prevents unit ability from activating when it's not active.
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
                SetGameState(gameState = gameStarted ? GameState.Normal : GameState.EnemyPreparing);
                return;
            }

            if (!point.IsEmpty() /*point.unit != null || point.crystal != null || point.portal != null*/)
            {
                //unit is occupying this space or is being built, or a crystal is there.
                DisplayAlertWithDuration(GetText("spaceOccupied"));
                SetGameState(gameState = gameStarted ? GameState.Normal : GameState.EnemyPreparing);
                return;
            }

            PlayerUnit newUnit = null;
            //check what player picked and instantiate
            bool unitFound = false;
            int i = 0;
            while(!unitFound && i < deadPlayerUnits.Count)
            {
                if ((int)deadPlayerUnits[i].unitType == (int)unit)  //cast is used to compare the enums' values
                {
                    unitFound = true;
                    newUnit = deadPlayerUnits[i];
                    newUnit.GetUnitData(newUnit.unitData);
                    deadPlayerUnits.Remove(newUnit);
                }
                else
                {
                    i++;
                }
            }

            if (!unitFound)
            {
                switch (unit)
                {
                    /*case PlayerUnitType.Base:   //there can only be 1
                        newUnit = Instantiate(basePrefab, playerUnitContainer.transform);
                        break;*/

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
            if (!inTutorial)    //units are free in tutorial.
            {
                money -= newUnit.unitCost;
                menu.UpdateMoneyText();
            }

            if (buildQueue.Count <= 0)  //make sure progressIndex is reset when build queue is empty
            {
                progressIndex = 0;
                activeBarIndex = 0;
            }

            buildQueue.Enqueue(newUnit);
            

            //add the unit's image in menu's build queue
            menu.queueIndex = buildQueue.Count - 1;     //this ensures that index points to correct spot

            //we do this check to prevent a build in progress from getting its time reset whenever additional units
            //beyond the 1st enter the queue. The build timer will get updated in the update loop.
            if (buildQueue.Count == 1)
                buildTimer[menu.queueIndex] = Time.time;

            menu.unitsInQueue[menu.queueIndex].enabled = true;
            menu.unitsInQueue[menu.queueIndex].sprite = newUnit.uiSprite;

            //active bar index always points to the next bar to activate.
            progressBars[activeBarIndex].DisplayProgressBar(true, point, newUnit.uiSprite);
            activeBarIndex = (activeBarIndex + 1 < progressBars.Length) ? activeBarIndex + 1 : 0;

            //save unit's position
            SetGameState(gameState = gameStarted ? GameState.Normal : GameState.EnemyPreparing);
        }     
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
        points[i].unit = playerBase;
    }

    void SetPointRef(PowerUpSpace powerSpace, Vector2 pointAxes)
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

        powerSpace.point = points[i];
        points[i].powerUpSpace = powerSpace;
    }

    /// <summary>
    /// Instantiates enemy unit to a given portal. Instantiate is skipped if an enemy unit
    /// exists in the graveyard.
    /// </summary>
    /// <param name="enemyType">The type of enemy to instantiate.</param>
    /// <param name="portal">The portal to where the enemy will be assigned.</param>
    void InstantiateEnemyUnit(UnitData.UnitType enemyType, Portal portal)
    {
        if (portal == null)
        {
            Logger.DebugLogWarning("null portal!");
            return;
        }

        EnemyUnit unit = null;
        for (int i = 0; i < portal.enemyCount; i++)
        {
            //search the graveyard for enemy unit first.
            //Logger.DebugLogFormat("Enemy Type: {0}", enemy.GetType());
            bool enemyFound = false;
            int j = 0;
            while(!enemyFound && j < deadEnemyUnits.Count)
            {
                if (deadEnemyUnits[j].unitType == enemyType)
                {
                    enemyFound = true;
                    unit = deadEnemyUnits[j];
                    unit.GetUnitData(unit.unitData);
                    unit.portal = portal;
                    unit.transform.position = portal.transform.position;
                    portal.enemyUnits.Add(unit);
                    deadEnemyUnits.Remove(unit);
                }
                else
                {
                    j++;
                }
            }
            /*if (deadEnemyUnits.Contains(enemy))
            {
                //remove unit from graveyard
                enemy.GetUnitData(enemy.unitData);
                enemy.portal = portal;
                portal.enemyUnits.Add(enemy);
                deadEnemyUnits.Remove(enemy);
            }*/
            if (!enemyFound) //else
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
                ScreenShake();          //only for player units.
                if (unit is PlayerUnit_Bomb)
                {
                    PlayerUnit_Bomb bomb = unit as PlayerUnit_Bomb;
                    AudioManager am = Singleton.instance.AudioManager;
                    am.bombExplode.Play();
                    bombExplosion.Play(unit.transform.position, bomb.BombRange() * 2);
                }
                else
                    CallExplosion(unit.transform.position);
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
        if (unit == null)
            return;
            
        deadEnemyUnits.Add(unit);
        unit.portal.enemyUnits.Remove(unit);
        unit.portal.ReduceEnemyCount();
        unit.portal = null;
        Logger.DebugLogFormat("Removing {0}", unit.name);
        unit.gameObject.SetActive(false);
        CallExplosion(unit.transform.position);
        totalEnemies--;
        Logger.DebugLogFormat("Enemies remaining: {0}", totalEnemies);
    }
    #endregion

    #region Game States
    public void SetGameState(GameState state)
    {
        //gameState = state;
        Logger.DebugLogFormat("GameState changed to {0}", gameState);
        ArrowGuideManager am = Singleton.instance.ArrowGuideManager;
        switch(state)
        {
            case GameState.Normal:
                //alertText.text = "";
                am.HideGuideArrows();
                readyButton.gameObject.SetActive(false);
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                enteringValueX = false;
                enteringValueY = false;
                playerUnitType = PlayerUnitType.NoUnitSelected;
                break;
            
            case GameState.EnemyPreparing:
                //show ready button
                readyButton.gameObject.SetActive(true);
                am.HideGuideArrows();
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                enteringValueX = false;
                enteringValueY = false;
                playerUnitType = PlayerUnitType.NoUnitSelected;
                //DisplayAlertWithDuration(GetText("enemyPreparing"), 10);

                //start portal timers
                foreach(Portal portal in activePortals)
                {
                    portal.timer.StartTimer();
                }
                break;

            case GameState.UnitSelectX:
                //disable Y values, then disable X values.
                //TODO: Have dialogue during tutorial to explain how to set units
                DisplayAlert(GetText("unitSelect_x"));
                axisContainerY.gameObject.SetActive(false);
                break;
            
            case GameState.UnitSelectY:
                //disable Y values, then disable X values.
                //TODO: in tutorial, make mention of origin and how it might be used
                DisplayAlert(GetText("unitSelect_y"));
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(false);
                break;
            

            case GameState.StartLevel:
                //look for any opening dialogue. If there's none, game resumes.
                bool triggerFound = false;
                int i = 0;
                while (!triggerFound && i < dialogueManager.condTriggerList.Count)
                {
                    if (dialogueManager.condTriggerList[i].level == currentLevel &&
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
                    SetGameState(gameState = GameState.EnemyPreparing);
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
            
            case GameState.DialogueGameOver:
                dialogueFound = false;
                j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    int dialogueId = dialogueManager.dialogueList[j].dialogueId;
                    if (dialogueId > 0 && dialogueId == dialogueManager.gameOverDialogueList[triggerIndex].dialogueId)
                    {
                        dialogueFound = true;
                        Dialogue.DialogueKeyList keyList = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        dialogueManager.dialogueList[j].ShowDialogue(keyList);
                    }
                    else
                    {
                        j++;
                    }
                }
                break;

            case GameState.BaseDestroyed:
                //have hitstop then blow up base
                DestroyBase();
                //playerBase.gameObject.SetActive(false);
                break;

            case GameState.LevelComplete:
                //stop any sounds that are playing
                AudioManager audio = Singleton.instance.AudioManager;
                audio.StopAllSounds();

                endLevelText.ShowText();
                Logger.DebugLogFormat("Level {0} complete", currentLevel);
                
                break;

            case GameState.GoToNextLevel:
                if (currentLevel + 1 <= maxLevels)
                {
                    Singleton.instance.ScreenFade.LoadLevelFadeOut(++currentLevel);
                }
                else
                {
                    //game complete
                    SetGameState(gameState = GameState.GameCompleted);
                }
                break;
            
            case GameState.GameCompleted:
                //go to end screen
                Singleton.instance.saveState.WriteState(Singleton.instance.gameData);   //save progress
                ScreenFade sf = Singleton.instance.ScreenFade;
                sf.ChangeSceneFadeOut("End Screen");
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
            case TutorialState.BuildingFirstUnit:
            case TutorialState.BuildingSecondUnit:
                //hide any arrow guides that were shown
                am.HideGuideArrows();
                readyButton.gameObject.SetActive(false);
                axisContainerY.gameObject.SetActive(true);
                axisContainerX.gameObject.SetActive(true);
                enteringValueX = false;
                enteringValueY = false;
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
                am.arrowUnitSelectX.gameObject.SetActive(true);
                am.arrowUnitSelectX.PointArrow(am.arrowUnitSelectX.arrowLocation, am.arrowUnitSelectX.direction);
                break;
            
            case TutorialState.UnitSelectSecondUnitY:
                //show arrow pointing to value. Player cannot pick other values.
                am.arrowUnitSelectX.gameObject.SetActive(false);
                am.arrowUnitSelectY.gameObject.SetActive(true);
                am.arrowUnitSelectY.PointArrow(am.arrowUnitSelectY.arrowLocation, am.arrowUnitSelectY.direction);
                break;
            
            case TutorialState.ActivateAbility:
                //show dialogue informing player to redirect unit to fire in opposite direction.
                am.arrowUnitSelectY.gameObject.SetActive(false);
                keyList = dialogueManager.dialogueList[DialogueManager.ACTIVATING_UNIT_ABILITY].dialogueKeyList[0];
                dialogueManager.dialogueList[DialogueManager.ACTIVATING_UNIT_ABILITY].ShowDialogue(keyList);
                break;
            
            case TutorialState.ActivateAbilitySelectX:
                am.arrowUnitSelectX.gameObject.SetActive(true);
                am.arrowUnitSelectX.PointArrow(am.arrowUnitSelectX.arrowLocation, am.arrowUnitSelectX.direction);
                break;
            
            case TutorialState.ActivateAbilitySelectY:
                am.arrowUnitSelectX.gameObject.SetActive(false);
                am.arrowUnitSelectY.gameObject.SetActive(true);
                am.arrowUnitSelectY.PointArrow(am.arrowUnitSelectY.arrowLocation, am.arrowUnitSelectY.direction);
                break;

            case TutorialState.DialogueEnemyIncoming:
                //inform player what they did and to reinforce how ordered pairs are made. Also have dialogue from mother base warning
                //player that enemy is about to attack.
                am.arrowUnitSelectY.gameObject.SetActive(false);
                keyList = dialogueManager.dialogueList[DialogueManager.ENEMY_INCOMING].dialogueKeyList[0];
                dialogueManager.dialogueList[DialogueManager.ENEMY_INCOMING].ShowDialogue(keyList);
                break;
            
            case TutorialState.DeployEnemyUnits:
                //there's only 1 portal in the tutorial
                activePortals[0].timer.SetTimer(0);
                activePortals[0].timer.StartTimer();
                tutorialState = TutorialState.Normal;   //nothing more needs to be done here.
                break;

            case TutorialState.CrystalIntro:
                keyList = dialogueManager.dialogueList[DialogueManager.CRYSTAL_INTRO].dialogueKeyList[0];
                dialogueManager.dialogueList[DialogueManager.CRYSTAL_INTRO].ShowDialogue(keyList);
                break;
            

            case TutorialState.DialogueStartOfLevel:
                //find correct dialogue to display.
                bool dialogueFound = false;
                int j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    if (dialogueManager.condTriggerList[triggerIndex].dialogueId == dialogueManager.dialogueList[j].dialogueId)
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
            
            case TutorialState.DialogueEndOfLevel:
                //find correct dialogue to display.
                dialogueFound = false;
                j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    int dialogueId = dialogueManager.dialogueList[j].dialogueId;
                    if (dialogueId > 0 && dialogueId == dialogueManager.condTriggerList[triggerIndex].dialogueId)
                    {
                        dialogueFound = true;
                        keyList = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        dialogueManager.dialogueList[j].ShowDialogue(keyList);
                    }
                    else
                    {
                        j++;
                    }
                }
                break;

            case TutorialState.BaseDestroyed:
                break;

            case TutorialState.LevelComplete:
                //deactivate current portals for use in the next level.
                foreach (Portal portal in activePortals)
                {
                    portal.gameObject.SetActive(false);
                }

                endLevelText.ShowText();
                //display dialogue if any
                Logger.DebugLogFormat("Level {0} complete", currentLevel);
                break;
        }
    }
    #endregion

#region Coroutine calls
    public void ScreenShake()
    {
        if (screenShakeCoroutine != null)
            StopCoroutine(screenShakeCoroutine);
        
        screenShakeCoroutine = StartCoroutine(ShakeCamera());
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

    void DestroyBase()
    {
        explodeBase = StartCoroutine(ExplodeBase());
    }
#endregion

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    void SpeakText(string key)
    {
        if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(key);
            Logger.DebugLog("TTS is playing");
        }
    }

#region Button Methods
    /// <summary>
    /// Starts game early. Used by ready button.
    /// </summary>
    public void AdvancePortalTimers()
    {
        foreach(Portal portal in activePortals)
        {
            portal.timer.ReduceTime(60);
        }

        readyButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Ends game once all levels are completed.
    /// </summary>
    /// <returns></returns>
    /*public void EndGame()
    {
        LOLSDK.Instance.SubmitProgress(0, currentLevel, maxLevels);
        LOLSDK.Instance.CompleteGame();
    }*/
#endregion

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

    //pauses game for a duration, then base explodes, followed by a "BASE DESTROYED" message
    IEnumerator ExplodeBase()
    {
        AudioManager am = Singleton.instance.AudioManager;
        am.mainMusic.Stop();
        am.StopAllSounds();
        yield return new WaitForSeconds(1);
        playerBase.gameObject.SetActive(false);
        am.soundSource.PlayOneShot(am.baseExplode, am.soundVolume - 0.1f);
        baseExplosion.Play(playerBase.transform.position);
        //yield return new WaitForSeconds(2);
        gameOverText.ShowText();
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
