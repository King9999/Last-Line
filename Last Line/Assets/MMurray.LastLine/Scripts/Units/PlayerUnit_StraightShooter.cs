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
    public Vector2 bulletRange;
    private float cooldownTimer;            //tracks when weapon fires.
    [SerializeField]private bool reverseDirection;  //if true, unit fires left.
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
        bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);

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
            float distance = Vector2.Distance(bulletRange,  bullet.transform.position);
            Logger.DebugLog("Bullet distance to target: " + distance);
            //if (bullet.transform.position.x > bulletRange.x)
            if (distance < 0.5f)
            {
                cooldownTimer = Time.time;
                //check if unit's firing direction needs to change.
                if (reverseDirection == true)
                {
                    reverseDirection = false;
                    bullet.direction *= -1;
                    bulletRange = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y);
                }
                Logger.DebugLog("Bullet range: " + bulletRange);
                bullet.transform.position = transform.position;
                bullet.gameObject.SetActive(false);

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

    //change firing direction.
    public override void ActivateAbility()
    {
        Logger.DebugLog("Reversing Unit firing direction!");
        reverseDirection = true;
    }
}
