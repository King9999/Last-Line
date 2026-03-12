using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* The Juggernaut is the strongest enemy unit. It's slow, but has a lot of HP and its weapon pierces. It destroys player units
that it collides with without being destroyed itself. If it collides with the base, it's instant game over. */
public class EnemyUnit_Juggernaut : EnemyUnit
{
    [Header("---Bullet---")]
    public Bullet bullet;
    //public Vector2 bulletRange;
    [SerializeField]private float cooldownTimer;            //tracks when weapon fires.
    [SerializeField]private Collider2D attackRange, juggCollider;
    [SerializeField]private PlayerUnit target;          //bullets are fired at this unit
    [SerializeField]private List<PlayerUnit> targets;

    // Start is called before the first frame update
    void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;
        //GetUnitData(unitData);
        
        bullet = Instantiate(gm.bulletPrefab, parent: this.transform);
        SetupBullet(bullet);
        bullet.gameObject.SetActive(false);

        //range of bullet is the unit's position + 2 units
        //bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);

        cooldownTimer = 0;
        juggCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        //move unit
        //transform.Translate(direction * moveSpeed * Time.deltaTime);

        //fire weapon
        if (bullet == null)     //this is to prevent game from updating before bullet exists.
            return;

        AttackTarget();
        /*if (!bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            //cooldownTimer = Time.time;
            //FireWeapon(bullet, new Vector2())
            bullet.gameObject.SetActive(true);
        }*/
        if (bullet.gameObject.activeSelf)
        {
            bulletRange = SetBulletRange(bullet);
            float distance = Vector2.Distance(bulletRange, bullet.transform.position);
            Logger.DebugLog("Juggernaut Bullet distance to target: " + distance);
            //if (bullet.transform.position.x > bulletRange.x)
            if (distance < 0.5f || bullet.bulletStruckUnit)
            {
                cooldownTimer = Time.time;
                bullet.bulletStruckUnit = false;
                bullet.transform.position = transform.position;
                bullet.gameObject.SetActive(false);
            }
        }
    }

    public override void SetupBullet(Bullet bullet)
    {
        base.SetupBullet(bullet);
        bullet.direction = Vector2.right;
        bullet.owner = Bullet.Owner.Enemy;
        bullet.bulletType = Bullet.BulletType.Pierce;
    }

    public override void FireWeapon(Bullet bullet, Vector2 direction)
    {
        bullet.gameObject.SetActive(true);
        bullet.direction = direction;
    }

    //Checks for collision against player units.
    void OnTriggerEnter2D(Collider2D collision)
    {
        //Attack range is ignored. Only want to check when the unit hits a player unit.
        if (attackRange.IsTouching(collision) && !juggCollider.IsTouching(collision))
            return;

        if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            Logger.DebugLog("Hit Unit " + unit.name);

            //player unit dies, but not the juggernaut.
            GameManager gm = Singleton.instance.GameManager;
            gm.RemovePlayerUnit(unit);
        }
    }

    //The only thing the attack range does is collect all player units in range for targeting. The actual
    //firing of the weapon happens in AttackTarget using the data acquired here.
    void OnTriggerStay2D(Collider2D collision)
    {
        //TODO: Need to get all units in range and fire at the closest one.
        if (attackRange.IsTouching(collision) && collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            //add target to list
            if (!targets.Contains(unit) && unit is not PlayerUnit_Base)//!collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
                targets.Add(unit);
            
        }
    }

    void AttackTarget()
    {
        if (targets.Count <= 0)
            return;

        //get closest target and fire
        float smallestDistance = 1000;
        foreach (PlayerUnit target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                this.target = target;
            }
        }

        //fire weapon at target if able
        if (target != null && !bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, 
                target.transform.position.y - transform.position.y);
            //bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);
            //SetBulletRange(bullet);
            FireWeapon(bullet, direction);
        }

        //clear targets. Targets may not always be valid on the next frame.
        target = null;
        targets.Clear();
        
    }

}
