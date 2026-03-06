using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*Fires an instantaneous beam at the nearest target. High damage, but long cooldown between shots. 
Enhanced: beam will chain, targeting 2 additional enemy units. */
public class PlayerUnit_TrackingBeam : PlayerUnit
{
    public LineRenderer beam;
    [SerializeField]private List<EnemyUnit> targets;
    [SerializeField]private EnemyUnit target;
    [SerializeField]private float cooldownTimer;            //tracks when weapon fires.
    [SerializeField]private Collider2D attackRange, strikerCollider;
    [SerializeField]private List<Vector2> linePositions;    //the points that will be used to draw the beam lines.
    private Coroutine animateBeam = null;

    // Start is called before the first frame update
    void Start()
    {
        //there will always be at least 1 point that won't change.
        linePositions.Add(transform.position);
        beam.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        AttackTarget();
    }

    //The only thing the attack range does is collect all player units in range for targeting. The actual
    //firing of the weapon happens in AttackTarget using the data acquired here.
    void OnTriggerStay2D(Collider2D collision)
    {
        if (attackRange.IsTouching(collision) && collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
        {
            Logger.DebugLogFormat("Target is {0}", unit.name);
            //add target to list
            if (!targets.Contains(unit))
                targets.Add(unit);
        }
    }

    /*void OnTriggerEnter2D(Collider2D collision)
    {
        //Attack range is ignored. Only want to check when the unit hits a player unit.
        if (attackRange.IsTouching(collision) && !strikerCollider.IsTouching(collision))
            return;

    }*/

    void AttackTarget()
    {
        if (targets.Count <= 0)
            return;

        //get closest target and fire
        float smallestDistance = 1000;
        foreach (EnemyUnit target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                this.target = target;
            }
        }

        //fire weapon at target if able
        if (target != null && Time.time > cooldownTimer + weaponCooldown)
        {
            //bullet.direction = new Vector2(target.transform.position.x - transform.position.x, 
                //target.transform.position.y - transform.position.y);
            //FireWeapon(bullet, bullet.direction);
            FireWeapon(target);
        }

        //clear targets. Targets may not always be valid on the next frame.
        target = null;
        targets.Clear();
    }

    //fire beam by placing two points. One point is at the tracking unit, and the other point is at the target.
    //a line is drawn between the 2 points. Also handles the chain effect if applicable.
    void FireWeapon(EnemyUnit target)
    {
        if (target == null)
            return;

        if (!isEnhanced)
        {
            Vector3[] linePositions = new Vector3[2];       
            //paint target
            linePositions[0] = transform.position;
            linePositions[1] = target.transform.position;

            //draw line at enemy's location.
            beam.positionCount = linePositions.Length;
            beam.SetPositions(linePositions);
            beam.gameObject.SetActive(true);

            //coroutine to animate laser
            animateBeam = StartCoroutine(AnimateBeam(1));
        }
        else
        {
            //if enhanced, target remaining units.
            Vector3[] linePositions = new Vector3[targets.Count + 1];//1 is added because we have to include the player unit's position.
        }
    }

    //beam shrinks over time. When it's finished, cooldown starts.
    IEnumerator AnimateBeam(float duration)
    {
        /*float i = 0;
        while (i < duration)
        {
            i += Time.deltaTime;
            yield return null;
        }*/
        yield return new WaitForSeconds(duration);
        cooldownTimer = Time.time;
        beam.gameObject.SetActive(false);
    }
}
