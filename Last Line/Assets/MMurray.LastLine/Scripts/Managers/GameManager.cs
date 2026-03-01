using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.LastLine;


public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Game State
    /// </summary>
    public enum GameState
    {
        UNIT_SELECT,
        COORDINATES_SELECT, ///player is entering coordinates
    }

    public enum PlayerUnitType {StraightShooter, VerticalShooter}
    public PlayerUnitType playerUnitType;
    public GameState gameState;
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public List<Point> points;                //the locations on the map where all units are placed.

    //unit setup
    public List<PlayerUnit> activePlayerUnits;
    public List<EnemyUnit> activeEnemyUnits;
    public PlayerUnit testUnit;
    public Portal testPortal;

    [Header("---Containers---")]
    public GameObject playerUnitContainer;      //all instantiated units are placed here

    [Header("---Prefabs---")]
    public Bullet bulletPrefab;
    public PlayerUnit_StraightShooter straightShooterPrefab;
    public EnemyUnit scoutPrefab;

    /***Units that are destroyed are sent to an array, and recycled whenever a new unit needs to be generated. 
    This is to reduce the amount of calls to the garbage collector. */
    [Header("---Graveyard---")]
    public List<EnemyUnit> deadEnemyUnits;
    public List<PlayerUnit> deadPlayerUnits;

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;
        //enteringValueX = true;

        Logger.DebugLogFormat("Testing {0}", activeEnemyUnits.Count);
        LoadLevel(0);
        //load save state
        Singleton s = Singleton.instance;
        if (s.saveStateFound)
        {
            s.saveState.LoadState(s.saveState.ReadState);
            //s.saveState.ReadState(s.gameData);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Load the specified level. NOTE: Save States only load the current level from the beginning, and not the state
    the level was left in. This is because the levels are going to be short enough that it's unnecessary to record
    every unit's position. Also, the game runs in real time so the player might get confused on what they were doing. */
    void LoadLevel(int level)
    {
        //get level from JSON
        //generate portals
        //each portal instantiates number of units.
        //EnemyUnit enemyUnit = null;
        for (int i = 0; i < testPortal.enemyCount; i++)
        {
            EnemyUnit enemyUnit = Instantiate(scoutPrefab as EnemyUnit_Scout, parent: testPortal.transform);
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
        //if space is occupied, activate context-sensitive ability. nothing happens if unit doesn't have one.
        if (point.unit != null)
        {
            point.unit.ActivateAbility();
            return;
        }

        PlayerUnit newUnit = null;
        //check what player picked and instantiate
        switch (unit)
        {
            case PlayerUnitType.StraightShooter:
                newUnit = Instantiate(straightShooterPrefab, Camera.main.transform);
                break;
            
            case PlayerUnitType.VerticalShooter:
                break;
        }

        //must convert the point's location to world coordinates.
        Vector2 newPos = Camera.main.ScreenToWorldPoint(point.transform.position);
        newUnit.transform.position = newPos;
        Logger.AddPlayerId(newUnit.name);
        newUnit.UpdateHealthBar(newPos);        //TODO: Not quite sure why this fixes the healthbar positioning issue for player units.
        point.unit = newUnit;

        //add to list
        activePlayerUnits.Add(newUnit);

        //save unit's position
        Singleton.instance.saveState.WriteState(Singleton.instance.gameData);     
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
}
