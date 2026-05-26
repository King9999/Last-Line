using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*Vertical Shooter fires their weapon up and down simultaneously.
Enhanced Unit: fires a continuous beam, acting as a wall. */
public class PlayerUnit_VerticalShooter : PlayerUnit
{
    public List<Bullet> bullets;        //fires 2 bullets.
    public List<Laser> lasers;
    private int BulletCount {get;} = 2;
    public List<Vector2> bulletRanges, laserRanges;
    private float cooldownTimer;            //tracks when weapon fires.
    public List<Transform> maxRanges; //0 is for top laser, 1 is for bottom laser

    // Start is called before the first frame update
    /*void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;

        //laser setup
        for (int i = 0; i < BulletCount; i++)
        {
            Laser laser = Instantiate(gm.laserPrefab, parent: this.transform);
            SetupLaser(laser);

            //range and direction setup. The laser is going to be rotated, which will alter its direction. So X is modified
            //instead of the Y value. (It makes sense if you rotate the laser object in the Inspector and see how the movement changes.) 
            if (i == 0)
            {
                laser.direction = Vector2.up; //new Vector2(1, 0); //Vector2.up;
            }
            else
            {
                laser.direction = Vector2.down; //new Vector2(-1, 0); //Vector2.down;
            }

            //Lasers aren't round so must adjust angle.
            //Vector3 angle = laser.transform.eulerAngles;
            //laser.transform.eulerAngles = new Vector3(angle.x, angle.y, -90);

            //laser's range is altered on the X pos due to rotation..
            //Vector2 laserRange = new Vector2(transform.position.x + (laser.distance * laser.direction.x), transform.position.y);
            Vector2 laserRange = new Vector2(transform.position.x, transform.position.y + (laser.distance * laser.direction.y));
            laserRanges.Add(laserRange);
            laser.gameObject.SetActive(false);
            lasers.Add(laser);
        }       
        
        cooldownTimer = 0;
        //EnhanceUnit();
    }*/

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager gm = Singleton.instance.GameManager;
        //laser setup
        if (lasers.Count <= 0)
        {
            for (int i = 0; i < BulletCount; i++)
            {
                Laser laser = Instantiate(gm.laserPrefab, parent: this.transform);
                SetupLaser(laser);

                //range and direction setup. The laser is going to be rotated, which will alter its direction. So X is modified
                //instead of the Y value. (It makes sense if you rotate the laser object in the Inspector and see how the movement changes.) 
                if (i == 0)
                {
                    laser.direction = Vector2.up; //new Vector2(1, 0); //Vector2.up;
                }
                else
                {
                    laser.direction = Vector2.down; //new Vector2(-1, 0); //Vector2.down;
                }

                //Lasers aren't round so must adjust angle.
                //Vector3 angle = laser.transform.eulerAngles;
                //laser.transform.eulerAngles = new Vector3(angle.x, angle.y, -90);

                //laser's range is altered on the X pos due to rotation..
                //Vector2 laserRange = new Vector2(transform.position.x + (laser.distance * laser.direction.x), transform.position.y);
                Vector2 laserRange = new Vector2(transform.position.x, transform.position.y + (laser.distance * laser.direction.y));
                laserRanges.Add(laserRange);
                laser.gameObject.SetActive(false);
                lasers.Add(laser);
            }
        }
        else
        {
            //reuse existing lasers.
            for(int i = 0; i < lasers.Count; i++)
            {
                //SetupLaser(lasers[i]);
                Vector2 laserRange = new Vector2(transform.position.x, transform.position.y + (lasers[i].distance * lasers[i].direction.y));
                laserRanges[i] = laserRange;
                lasers[i].gameObject.SetActive(false);
            }
        }
        cooldownTimer = 0;       
    }

    // Update is called once per frame
    protected override void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        base.Update();    
        //fire weapon
        if (bullets == null)     //this is to prevent game from updating before bullet exists.
            return;

        /*if (!isEnhanced)
        {
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
        else 
        {*/
            //laser update
            bool lasersReady = LasersReadyToFire();
            if (lasersReady && Time.time > cooldownTimer + weaponCooldown)
            {
                foreach(Laser laser in lasers)
                    laser.gameObject.SetActive(true);
            }
            else
            {
                for (int i = 0; i < lasers.Count; i++)
                {
                    Vector2 laserRange = laserRanges[i];
                    //Transform laserRange = maxRanges[i];
                    Laser laser = lasers[i];
                    if (!laser.gameObject.activeSelf)  //nothing is done with this bullet.
                        continue;

                    float distance = Vector2.Distance(laserRange, laser.transform.position);
                    Logger.DebugLogFormat("Laser {0} distance to target: {1}", i, distance);

                    if (distance < 0.5f /*|| laser.bulletStruckUnit*/)
                    {
                        //cooldownTimer = Time.time;
                        //laser.bulletStruckUnit = false;

                        Logger.DebugLogFormat("Laser {0} range: {1}", i, laserRange);
                        laser.transform.position = transform.position;
                        laser.gameObject.SetActive(false);
                    }
                }

                //both bullets must be inactive for the cooldown to reset.
                lasersReady = LasersReadyToFire();
                if (lasersReady && Time.time > cooldownTimer + weaponCooldown)
                {
                    AudioManager audio = Singleton.instance.AudioManager;
                    audio.soundSource.PlayOneShot(audio.laserSfx, audio.soundVolume);
                    cooldownTimer = Time.time;
                    Logger.DebugLog("Lasers ready");
                }
            }
        //}
    }

    bool BulletsReadyToFire()
    {
        return !bullets[0].gameObject.activeSelf && !bullets[1].gameObject.activeSelf;
    }

    bool LasersReadyToFire()
    {
        return !lasers[0].gameObject.activeSelf && !lasers[1].gameObject.activeSelf;
    }

    public override void SetupBullet(Bullet bullet)
    {
        base.SetupBullet(bullet);
        //bullet.direction = Vector2.up;
        bullet.owner = Bullet.Owner.Player;
    }

    /* fires a continuous laser, acting as a wall. Enemy units take continuous damage. */
    public override void EnhanceUnit()
    {
        if (isEnhanced)
            return;

        base.EnhanceUnit();
        isEnhanced = true;
        weaponCooldown = 0;
        
        //reduce damage to compensate for the increased attack speed.
        foreach(Laser laser in lasers)
        {
            laser.damage = 1;
            laser.hitCooldown = 0.2f;
        }
        //replace bullets with laser
        /*foreach (Bullet bullet in bullets)
        {
            bullet.gameObject.SetActive(false);
        }*/
    }

}
