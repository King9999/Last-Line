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
    public GameState gameState;
    public Vector2 playerCoordinates;                 //the coordinates manually entered by player
    public bool enteringValueX, enteringValueY; //determines which axis value is being entered. X is always entered first.
    public List<Point> points;                //the locations on the map where all units are placed.

    //unit setup
    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    public PlayerUnit testUnit;

    // Start is called before the first frame update
    void Start()
    {
        //must always make sure singleton grabs reference to the managers
        Singleton.instance.GameManager = this;
        //enteringValueX = true;
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
    public void PlaceUnit(PlayerUnit unit, Point point)
    {
        //generate an instance of selected unit

        //must convert the point's location to world coordinates.
        Vector2 newPos = Camera.main.ScreenToWorldPoint(point.transform.position);
        unit.transform.position = newPos;     
    }
}
