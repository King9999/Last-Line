using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using Unity.VisualScripting.Antlr3.Runtime;

/* Bombs do nothing until either an enemy touches it or the player detonates it. Bombs can be detonated by re-entering the coordinates
of where they were placed. Bombs have a high build time to prevent players from just using bombs to win. 
Enhanced: Increased range, and does not hurt player units. */
public class PlayerUnit_Bomb : PlayerUnit
{

    [SerializeField]private CircleCollider2D bombRange;
    [SerializeField]private float startupTimer;         //the delay before bomb blows up.
    private float timer;
    [SerializeField]private GameObject outline;         //flash effect when bomb's going to detonate.
    [SerializeField]private List<Unit> targets;         //includes friendly units. Base takes 3 damage.
    Coroutine flashCoroutine = null;
    // Start is called before the first frame update
    void Start()
    {
        //bomb range starts deactivated.
        bombRange.gameObject.SetActive(false);
        outline.gameObject.SetActive(false);
        startupTimer = 1;
        timer = -1;
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

        if (bombRange.gameObject.activeSelf && Time.time > timer + startupTimer)
        {
            //bomb blows up
            Explode();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        //NOTE: This method should only be called when bombRange is active.
        if (/*bombRange.IsTouching(collision) &&*/ collision.TryGetComponent<Unit>(out Unit unit))
        {
            //Logger.DebugLogFormat("Target is {0}", unit.name);
            //add target to list
            if (!targets.Contains(unit))
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
        if (bombRange.gameObject.activeSelf)
            return;

        bombRange.gameObject.SetActive(true);
        timer = Time.time;
        flashCoroutine = StartCoroutine(Flash());
    }

    void Explode()
    {
        //destroy all units in the target list. if the target is the base, damage it.
        GameManager gm = Singleton.instance.GameManager;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] is PlayerUnit_Base)
            {
                if (!isEnhanced)
                    targets[i].ReduceHitPoints(weaponDamage);
            }
            else if (targets[i] is PlayerUnit)
            {
                if (!isEnhanced)
                    gm.RemovePlayerUnit(targets[i] as PlayerUnit);
            }
            else
            {
                EnemyUnit unit = targets[i] as EnemyUnit;
                gm.AddMoney(unit.moneyAmount, unit.transform.position);
                gm.RemoveEnemyUnit(unit);
            }
        }

        gm.RemovePlayerUnit(this);
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
        while(bombRange.gameObject.activeSelf)
        {
            outline.gameObject.SetActive(toggle);
            yield return new WaitForSeconds(0.1f);
            toggle = !toggle;
        }
    }
    
}
