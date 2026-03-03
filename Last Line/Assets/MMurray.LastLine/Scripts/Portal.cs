using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.LastLine;
using TMPro;

public class Portal : MonoBehaviour
{
    public Timer timer;                 //time in seconds before portal starts sending units.
    public List<EnemyUnit> enemyUnits;      //the unit that's deployed.
    public float deployCooldown;        //the time that must pass before another unit is generated.
    private float currentDeployTime;
    public int enemyCount;              //the number of enemies in the portal.
    private int nextEnemy;              //iterator for the enemy unit list.
    [SerializeField]private GameObject unitIcon;    //appears above portal to show player which enemy is deployed.
    [Header("---Path---")]
    public LineRenderer enemyPath;
    public List<Vector2>pathPoints;
    [SerializeField]private TextMeshProUGUI enemyCountText;

    // Start is called before the first frame update
    void Start()
    {
        nextEnemy = 0;
        currentDeployTime = 0;

        //icon setup
        SpriteRenderer renderer = unitIcon.GetComponent<SpriteRenderer>();
        renderer.sprite = enemyUnits[0].GetComponent<SpriteRenderer>().sprite;
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
        timer.StartTimer();
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
            unitIcon.gameObject.SetActive(false);

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
}
