using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/* Bombs do nothing until either an enemy touches it or the player detonates it. Bombs can be detonated by re-entering the coordinates
of where they were placed. Bombs have a high build time to prevent players from just using bombs to win. 
Enhanced: Increased range, and does not hurt player units. */
public class PlayerUnit_Bomb : PlayerUnit
{

    [SerializeField]private CircleCollider2D bombRange;
    [SerializeField]private BoxCollider2D bombHitbox;
    [SerializeField]private float startupTimer;         //the delay before bomb blows up.
    private float timer;
    [SerializeField]private GameObject outline;         //flash effect when bomb's going to detonate.
    [SerializeField]private List<Unit> targets;         //includes friendly units. Base takes 3 damage.
    Coroutine flashCoroutine = null;
    Coroutine explode;
    Coroutine vacuum;
    [SerializeField]private bool isDetonating;                          //used to prevent Explode from being called more than once.
    [SerializeField]private bool bombDetonated;
    // Start is called before the first frame update
    /*void Start()
    {
        //bomb range starts deactivated.
        bombRange.gameObject.SetActive(false);
        outline.gameObject.SetActive(false);
        startupTimer = 1;
        timer = -1;
    }*/

    protected override void OnEnable()
    {
        base.OnEnable();
        bombRange.gameObject.SetActive(false);
        outline.gameObject.SetActive(false);
        startupTimer = 1;
        timer = -1;
        bombRange.radius = 4;
        bombDetonated = false;
        bombHitbox = GetComponent<BoxCollider2D>();
        bombHitbox.enabled = true;

        if (targets.Count > 0)
            targets.Clear();

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        
        if (explode != null)
            StopCoroutine(explode);
    }

    // Update is called once per frame
    protected override void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        base.Update();    
        /*if (bombRange.gameObject.activeSelf && timer <= 0)
        {
            timer = Time.time;
            //TODO: Start coroutine to have bomb flash
            flashCoroutine = StartCoroutine(Flash());
        }*/

        if (isDetonating /*&& bombRange.gameObject.activeSelf*/ && Time.time > timer + startupTimer)
        {
            //bomb blows up
            isDetonating = false;
            //Explode();
            explode = StartCoroutine(ExplodeBomb());
        }

        if (bombDetonated)
        {
            bombDetonated = false;
            vacuum = StartCoroutine(VacuumUnits());
            /*float moveSpeed = 5 * Time.deltaTime;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] is PlayerUnit_Base)
                   continue;

                //direction of each target is towards the bomb's position
                targets[i].transform.position = Vector2.MoveTowards(targets[i].transform.position, transform.position, moveSpeed);

            }*/
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        //NOTE: This method should only be called when bombRange is active.
        if (/*bombRange.IsTouching(collision) &&*/ collision.TryGetComponent<Unit>(out Unit unit))
        {
            //Logger.DebugLogFormat("Target is {0}", unit.name);
            //add target to list
            if (unit != null && unit.gameObject.activeSelf && !targets.Contains(unit))
                targets.Add(unit);
        }
    }

    //this collision is for the bomb itself.
    /*void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
        {
            //detonate
            ActivateBomb();
        }
    }*/

    /* increased range, and no longer destroys friendly units. */
    public override void EnhanceUnit()
    {
        if (isEnhanced)
            return;

        isEnhanced = true;
        bombRange.radius = 6;
    }

    public override void ActivateAbility()
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayAlertWithDuration(GetText("bombActivated"));
        ActivateBomb();
    }

    public void ActivateBomb()
    {
        if (isDetonating /*bombRange.gameObject.activeSelf*/)
            return;

        //bombRange.gameObject.SetActive(true);
        isDetonating = true;
        timer = Time.time;
        flashCoroutine = StartCoroutine(Flash());
    }


    void Explode()
    {
        //destroy all units in the target list. if the target is the base, damage it.
        /* Updating bomb to instead suck everything in range into a void. The units are being captured. This is a change made to appease LoL
        because of their concerns of the game being "too violent" */
        GameManager gm = Singleton.instance.GameManager;
        for (int i = 0; i < targets.Count; i++)
        {
                
            if (targets[i] is PlayerUnit_Base)
            {
                continue;
                //if (!isEnhanced)
                    //targets[i].ReduceHitPoints(weaponDamage);
            }
            else if (targets[i] is PlayerUnit)
            {
                if (!isEnhanced)
                    //gm.RemovePlayerUnit(targets[i] as PlayerUnit);
                    gm.VacuumPlayerUnit(targets[i] as PlayerUnit);
            }
            else
            {
                EnemyUnit unit = targets[i] as EnemyUnit;
                if (unit != null)
                {
                    gm.AddMoney(unit.moneyAmount, unit.transform.position);
                    gm.RemoveEnemyUnit(unit);
                }
            }
        }
        bombDetonated = true;
        //bomb's hitbox is disabled
        bombHitbox.enabled = false;
        //gm.RemovePlayerUnit(this);
        AudioManager am = Singleton.instance.AudioManager;
        am.bombExplode.Play();
        gm.bombExplosion.Play(transform.position, BombRange() * 2);
    }

    public float BombRange()
    {
        return bombRange.radius;
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    IEnumerator Flash()
    {
        bool toggle = true;
        while(isDetonating /*bombRange.gameObject.activeSelf*/)
        {
            outline.gameObject.SetActive(toggle);
            yield return new WaitForSeconds(0.1f);
            toggle = !toggle;
        }
    }

    IEnumerator ExplodeBomb()
    {
        bombRange.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);  //we wait to give the game time to collect targets.
        Explode();
    }

    IEnumerator VacuumUnits(float duration = 0.5f)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float moveSpeed = 5 * Time.deltaTime;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] is PlayerUnit_Base)
                    continue;

                //direction of each target is towards the bomb's position
                targets[i].unitHitbox.enabled = false;
                targets[i].transform.position = Vector2.MoveTowards(targets[i].transform.position, transform.position, moveSpeed);
            }
            yield return null;
        }

        GameManager gm = Singleton.instance.GameManager;
        gm.RemovePlayerUnit(this);
    }
    
}
