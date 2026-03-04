using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* Strikers are fast and can fire their weapon at nearby player units. Portals will deploy strikers at a slower rate than scouts. */
public class EnemyUnit_Striker : EnemyUnit
{
    [Header("---Bullet---")]
    public Bullet bullet;
    public Vector2 bulletRange;
    [SerializeField]private float cooldownTimer;            //tracks when weapon fires.
    [SerializeField]private CircleCollider2D attackRange;

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
        bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);

        cooldownTimer = 0;
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

        if (!bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            //cooldownTimer = Time.time;
            //FireWeapon(bullet, new Vector2())
            bullet.gameObject.SetActive(true);
        }
        else
        {
            float distance = Vector2.Distance(bulletRange, bullet.transform.position);
            Logger.DebugLog("Bullet distance to target: " + distance);
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
    }

    public override void FireWeapon(Bullet bullet, Vector2 direction)
    {
        bullet.gameObject.SetActive(true);
        
    }


}
