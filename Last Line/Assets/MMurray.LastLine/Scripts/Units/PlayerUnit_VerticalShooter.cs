using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*Vertical Shooter fires their weapon up and down simultaneously.
Enhanced Unit: fires a continuous beam, acting as a wall. */
public class PlayerUnit_VerticalShooter : PlayerUnit
{
    public List<Bullet> bullets;        //fires 2 bullets.
    private int BulletCount {get;} = 2;
    public List<Vector2> bulletRanges;
    private float cooldownTimer;            //tracks when weapon fires.

    // Start is called before the first frame update
    void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;
        //GetUnitData(unitData);
        
        for (int i = 0; i < BulletCount; i++)
        {
            Bullet bullet = Instantiate(gm.bulletPrefab, parent: this.transform);
            SetupBullet(bullet);

            //range and direction setup
            if (i == 0)
                bullet.direction = Vector2.up;
            else
                bullet.direction = Vector2.down;

            Vector2 bulletRange = new Vector2(transform.position.x, transform.position.y + (bullet.distance * bullet.direction.y));
            bulletRanges.Add(bulletRange);
            bullet.gameObject.SetActive(false);
            bullets.Add(bullet);
        }       
        
        cooldownTimer = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        //fire weapon
        if (bullets == null)     //this is to prevent game from updating before bullet exists.
            return;

        //both bullets fire at the same time, so both bullets must be inactive and ready to fire.
        bool bulletsReady = BulletsReadyToFire();
        //Bullet bullet = bullets[0];
        if (bulletsReady && Time.time > cooldownTimer + weaponCooldown)
        //if (!bullet.gameObject.activeSelf && Time.time > cooldownTimer + weaponCooldown)
        {
            //cooldownTimer = Time.time;
            foreach(Bullet bullet in bullets)
                bullet.gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                Vector2 bulletRange = bulletRanges[i];
                Bullet bullet = bullets[i];
                if (!bullet.gameObject.activeSelf)  //nothing is done with this bullet.
                    continue;

                float distance = Vector2.Distance(bulletRange,  bullet.transform.position);
                Logger.DebugLogFormat("Bullet {0} distance to target: {1}", i, distance);
                //if (bullet.transform.position.x > bulletRange.x)
                if (distance < 0.5f || bullet.bulletStruckUnit)
                {
                    //cooldownTimer = Time.time;
                    bullet.bulletStruckUnit = false;

                    Logger.DebugLogFormat("Bullet {0} range: {1}", i, bulletRange);
                    bullet.transform.position = transform.position;
                    bullet.gameObject.SetActive(false);
                }
            }

            //both bullets must be inactive for the cooldown to reset.
            bulletsReady = BulletsReadyToFire();
            if (bulletsReady && Time.time > cooldownTimer + weaponCooldown)
            {
                cooldownTimer = Time.time;
                Logger.DebugLog("Bullets ready");
            }
        }
    }

    bool BulletsReadyToFire()
    {
        return !bullets[0].gameObject.activeSelf && !bullets[1].gameObject.activeSelf;
    }

    public override void SetupBullet(Bullet bullet)
    {
        base.SetupBullet(bullet);
        //bullet.direction = Vector2.up;
        bullet.owner = Bullet.Owner.Player;
    }

}
