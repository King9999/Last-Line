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
    [SerializeField]ParticleSystem chargeParticles;
    bool isCharging = false;
    private Coroutine animateBeam = null;
    private bool beamFired;

    // Start is called before the first frame update
    /*void Start()
    {
        //there will always be at least 1 point that won't change.
        linePositions.Add(transform.position);
        beam.widthMultiplier = 0.3f;
        beam.gameObject.SetActive(false);
        //EnhanceUnit();
    }*/

    protected override void OnEnable()
    {
        base.OnEnable();
        if (animateBeam != null)
            StopCoroutine(animateBeam);
        
        //reset line position
        if (linePositions.Count > 0)
            linePositions.Clear();

        linePositions.Add(transform.position);
        beam.widthMultiplier = 0.3f;
        targets.Clear();
        target = null;
        isCharging = false;
        cooldownTimer = Time.time;          //unit doesn't fire immediately.
        beam.gameObject.SetActive(false);
    }


    // Update is called once per frame
    protected override void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        base.Update();

        //play particle effect if there's 1 second remaining in the charge up
        if (OneSecondRemaining() && !isCharging)
        {
            isCharging = true;
            AudioManager audio = Singleton.instance.AudioManager;
            audio.soundSource.PlayOneShot(audio.trackingBeamCharging, audio.soundVolume);
            chargeParticles.Play();
            Logger.DebugLog("Charging laser!");
        }    
        AttackTarget();
    }

    //The only thing the attack range does is collect all player units in range for targeting. The actual
    //firing of the weapon happens in AttackTarget using the data acquired here.
    void OnTriggerStay2D(Collider2D collision)
    {
        if (/*attackRange.IsTouching(collision) &&*/ collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
        {
            //Logger.DebugLogFormat("Target is {0}", unit.name);
            //add target to list
            if (!targets.Contains(unit))
                targets.Add(unit);
        }
    }

    bool OneSecondRemaining()
    {
        float timeRemaining = cooldownTimer + weaponCooldown - Time.time;
        return timeRemaining > 0 && timeRemaining <= 1;
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
        
        if (beamFired)
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

            //deal damage. Bool is used to prevent beam hitting every frame.
            if (!beamFired)
            {
                beamFired = true;
                //target.hitPoints -= weaponDamage;
                target.ReduceHitPoints(weaponDamage);
                //target.UpdateHealthBar(target.transform.position);
                //Logger.DebugLog("enemy hit");
            }

            //coroutine to animate laser
            animateBeam = StartCoroutine(AnimateBeam(1));
        }
        else
        {
            //if enhanced, target remaining units.
            //Vector3[] linePositions = new Vector3[targets.Count + 1];//1 is added because we have to include the player unit's position.
            //List<Vector3> pos = new List<Vector3>();
            List<Unit> units = new List<Unit>();
            //pos.Add(transform.position);
            units.Add(this);
            //pos.Add(target.transform.position);
            units.Add(target);

            int targetCount = 2;
            int i = 0; int j = 0;
            while(i < targetCount && j < targets.Count)
            {
                if (targets[j] != target)
                {
                    //pos.Add(targets[j].transform.position);
                    units.Add(targets[j]);
                    i++;
                }
                
                j++;
                
            }

            Vector3[] linePositions = new Vector3[units.Count];
            //linePositions[0] = transform.position;
            //linePositions[1] = target.transform.position;
           
            for (i = 0; i < linePositions.Length; i++)
            {
                linePositions[i] = units[i].transform.position;
            }
        
            //only 2 more targets are added.
            beam.positionCount = linePositions.Length;             //player unit + 3 targets' positions.
            beam.SetPositions(linePositions);
            beam.gameObject.SetActive(true);

            //damage each enemy. Start at index 1 because the first unit is the player
            if (!beamFired)
            {
                beamFired = true;
                for (i = 1; i < units.Count; i++)
                {
                    //units[i].hitPoints -= weaponDamage;
                    units[i].ReduceHitPoints(weaponDamage);
                    //units[i].UpdateHealthBar(units[i].transform.position);
                    Logger.DebugLogFormat("hit enemy {0} with beam", units[i].name);
                }
            }

            animateBeam = StartCoroutine(AnimateBeam(1));
        }

        AudioManager audio = Singleton.instance.AudioManager;
        audio.trackingBeamShot.Play();
    }

    public override void EnhanceUnit()
    {
        if (isEnhanced)
            return;
            
        base.EnhanceUnit();
        isEnhanced = true;
    }

    //beam shrinks over time. When it's finished, cooldown starts.
    IEnumerator AnimateBeam(float duration)
    {   
        Logger.DebugLog("Animate Beam called");
        float i = 0;
        //AnimationCurve origCurve = beam.widthCurve;

        while (i < duration)
        {
            i += Time.deltaTime;
            /*for (int j = 0; j < beam.widthCurve.keys.Length; j++)
            {
                beam.widthCurve.keys[j].value -= 0.05f * Time.deltaTime;
                Logger.DebugLogFormat("beam width {0}", beam.widthCurve.keys[j].value);
                yield return null;
            }*/
            beam.widthMultiplier -= 0.3f * Time.deltaTime;
            yield return null;
        }
        //yield return new WaitForSeconds(duration);
        //beam.widthCurve = origCurve;
        cooldownTimer = Time.time;
        isCharging = false;
        beam.widthMultiplier = 0.3f;
        beamFired = false;
        beam.gameObject.SetActive(false);
    }
}
