using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.LastLine;
using TMPro;
using System;
using UnityEngine.UIElements;

public class Portal : MonoBehaviour
{
    public Vector2 pointAxes;       //the vector from the Point script where the portal is located.
    public Point point;                 //used by editor to obtain point vector.
    public enum EnemyType { Scout, Striker, Dozer, Juggernaut }
    public EnemyType enemyType;         //used to determine which units to instantiate.
    public Timer timer;                 //time in seconds before portal starts sending units.
    public List<EnemyUnit> enemyUnits;      //the unit that's deployed.
    public float deployCooldown;        //the time that must pass before another unit is generated.
    private float currentDeployTime;
    public int enemyCount;              //the number of enemies in the portal.
    private int nextEnemy;              //iterator for the enemy unit list.
    [SerializeField]private GameObject unitIcon;    //appears above portal to show player which enemy is deployed.
    [Header("---Path---")]
    public LineRenderer enemyPath;
    public List<Vector3>pathPoints;
    [SerializeField]private TextMeshProUGUI enemyCountText;

    // Start is called before the first frame update
    void Start()
    {
        /*nextEnemy = 0;
        currentDeployTime = 0;

        //icon setup
        SpriteRenderer renderer = unitIcon.GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = enemyUnits[0].GetComponentInChildren<SpriteRenderer>().sprite;
        enemyCountText.text = "x" + enemyCount;
        enemyCountText.transform.position = Camera.main.WorldToScreenPoint(new Vector2(unitIcon.transform.position.x + 0.5f,
            unitIcon.transform.position.y - 0.1f));
        

        //path setup TODO: Why can't I just get a reference to the line renderer's vertices directly?
        for (int i = 0; i < enemyPath.positionCount; i++)
        {
            pathPoints.Add(enemyPath.GetPosition(i));
        }
        Logger.DebugLogFormat("Vertex count in portal {0}: {1}", gameObject.name, pathPoints.Count);

        //timer setup
        timer.timerUI.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        timer.timeFormat = Timer.TimeFormat.Seconds;
        timer.StartTimer();*/
    }

    void OnEnable()
    {
        enemyUnits.Clear();
        pathPoints.Clear();
        point = null;
    }

    /*****USE THIS TO SETUP PORTAL UPON INSTANTIATION INSTEAD OF THE START METHOD*****
    Sets the timer's position and sets up enemy path points, as well as the enemy icon */
    public void SetupPortal()
    {
        nextEnemy = 0;
        currentDeployTime = 0;

        //enemy setup


        //icon setup
        SpriteRenderer renderer = unitIcon.GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = enemyUnits[0].GetComponentInChildren<SpriteRenderer>().sprite;
        enemyCountText.text = "x" + enemyCount;
        enemyCountText.transform.position = Camera.main.WorldToScreenPoint(new Vector2(unitIcon.transform.position.x + 0.5f,
            unitIcon.transform.position.y - 0.1f));
        

        //path setup TODO: Why can't I just get a reference to the line renderer's vertices directly?
        for (int i = 0; i < enemyPath.positionCount; i++)
        {
            //add the portal's position to each point so that the enemy moves accurately along the path.
            Vector3 newPoint = enemyPath.GetPosition(i);
            Vector3 newPathPoint = new Vector3(newPoint.x + transform.position.x, newPoint.y + transform.position.y);
            pathPoints.Add(newPathPoint /*enemyPath.GetPosition(i)*/);

        }
        Logger.DebugLogFormat("Vertex count in portal {0}: {1}", gameObject.name, pathPoints.Count);

        //timer setup. If we're in the tutorial stage, no timer is shown.
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.inTutorial)
        {
            timer.timerUI.transform.position = Camera.main.WorldToScreenPoint(transform.position);
            timer.timeFormat = Timer.TimeFormat.Seconds;
            timer.StartTimer();
        }
        else
        {
            timer.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        //hide timer if time is up
        if (!timer.TimeUp())
            return;

        //icon is disabled as it won't be needed
        if (unitIcon.gameObject.activeSelf)
        {
            unitIcon.gameObject.SetActive(false);
            enemyCountText.gameObject.SetActive(false);
        }

        if (nextEnemy < enemyCount && Time.time > currentDeployTime + deployCooldown)
        {
            //timer.gameObject.SetActive(false);

            //deploy unit
            currentDeployTime = Time.time;
            enemyUnits[nextEnemy].gameObject.SetActive(true);
            enemyUnits[nextEnemy].SetPath(pathPoints);
            nextEnemy++;
            Logger.DebugLog("nextEnemy index: " + nextEnemy);
        }
    }

    //when an enemy dies, we must adjust the index so it points to the correct enemy in case there are more
    //in the list to deploy.
    public void ReduceEnemyCount()
    {
        nextEnemy--;
        enemyCount--;
        Logger.DebugLog("nextEnemy index: " + nextEnemy);
    }

    public void SetEnemyPath()
    {
        pathPoints = new List<Vector3>();
        for (int i = 0; i < enemyPath.positionCount; i++)
        {
            pathPoints.Add(enemyPath.GetPosition(i));
        }
    }


}
