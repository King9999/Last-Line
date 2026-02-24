using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*Straight Shooter fires their weapon in a horizontal direction. The direction can be changed by re-entering the
coordinates of where the unit was placed. The weapon's range is 2 spaces.
Enhanced Unit: unlimited weapon range, and weapon pierces targets. */
public class PlayerUnit_StraightShooter : PlayerUnit
{
    public Bullet bullet;
    public float bulletRange;
    private float cooldownTimer;            //tracks when weapon fires.
    // Start is called before the first frame update
    void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;
        GetUnitData(unitData);
        
        bullet = Instantiate(gm.bulletPrefab, parent: this.transform);
        SetupBullet(bullet);
        bullet.gameObject.SetActive(false);

        //range of bullet is the unit's position + 2 units
        bulletRange = transform.position.x + (bullet.distance * bullet.direction.x);

        cooldownTimer = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        //fire weapon
        if (bullet == null)     //this is to prevent game from updating before bullet exists.
            return;

        if (!bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            //cooldownTimer = Time.time;
            bullet.gameObject.SetActive(true);
        }
        else
        {
            if (bullet.transform.position.x > bulletRange)
            {
                cooldownTimer = Time.time;
                bullet.gameObject.SetActive(false);
                bullet.transform.position = this.transform.position;
            }
        }
    }

    public override void SetupBullet(Bullet bullet)
    {
        base.SetupBullet(bullet);
        //bullet.moveSpeed = 6;
        bullet.direction = Vector2.right;
        //bullet.damage = 1;
        //bullet.distance = 4;
    }
}
