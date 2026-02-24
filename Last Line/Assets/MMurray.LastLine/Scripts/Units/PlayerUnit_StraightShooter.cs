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
    private float cooldownTimer;            //tracks when weapon fires.
    // Start is called before the first frame update
    void Start()
    {
        //bullet setup
        GameManager gm = Singleton.instance.GameManager;
        bullet = Instantiate(gm.bulletPrefab, parent: this.transform);
        SetupBullet(bullet);
        bullet.gameObject.SetActive(false);

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
            cooldownTimer = Time.time;
            bullet.gameObject.SetActive(true);
        }
    }

    void SetupBullet(Bullet bullet)
    {
        bullet.moveSpeed = 3f;
        bullet.direction = Vector2.right;
        bullet.damage = 1;
        bullet.distance = 2;
    }
}
