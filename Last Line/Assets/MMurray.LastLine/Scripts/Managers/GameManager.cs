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
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public PlayerUnit testUnit;

    [Header("---Prefabs---")]
    public Bullet bulletPrefab;
    public PlayerUnit_StraightShooter straightShooterPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;
        //enteringValueX = true;

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

    /// <summary>
    /// Places a player unit at the specified location.
    /// </summary>
    /// <param name="unit">The unit to be deployed.</param>
    /// <param name="point">The location where the unit will be deployed.</param>
    public void PlaceUnit(PlayerUnitType unit, Point point)
    {
        //if space is occupied, do nothing
        if (point.unit != null)
            return;

        PlayerUnit newUnit = null;
        //check what player picked and instantiate
        switch (unit)
        {
            case PlayerUnitType.StraightShooter:
                newUnit = Instantiate(straightShooterPrefab);
                break;
            
            case PlayerUnitType.VerticalShooter:
                break;
        }

        //must convert the point's location to world coordinates.
        Vector2 newPos = Camera.main.ScreenToWorldPoint(point.transform.position);
        newUnit.transform.position = newPos;
        point.unit = newUnit;

        //save unit's position
        Singleton.instance.saveState.WriteState(Singleton.instance.gameData);     
    }
}
