using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.LastLine;

public class Portal : MonoBehaviour
{
    public Timer timer;                 //time in seconds before portal starts sending units.
    public List<EnemyUnit> enemyUnits;      //the unit that's deployed.
    public float deployCooldown;        //the time that must pass before another unit is generated.

    // Start is called before the first frame update
    void Start()
    {
        //timer setup
        timer.timerUI.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        timer.timeFormat = Timer.TimeFormat.Seconds;
        timer.SetTimer(10);
        timer.StartTimer();
    }

    // Update is called once per frame
    void Update()
    {
        //hide timer if time is up
        if (timer.TimeUp())
        {
            timer.gameObject.SetActive(false);
        }
    }
}
