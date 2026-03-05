using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*Laser is similar to bullet except it inherently pierces. It's also larger than a standard bullet. */
public class Laser : MonoBehaviour
{
    public enum Owner {Player, Enemy}       //for collision checking.
    public enum BulletType {Normal, Chain}  //piercing is normal for a laser
    public Owner owner;
    public BulletType bulletType;
    public float moveSpeed;
    public float distance;
    public Vector2 direction, angle;        
    public int damage;
    public bool bulletStruckUnit;
    private Collider2D laserCollider;
    private float hitCooldown {get; } = 0.2f;      //prevents laser from hitting every frame.
    private float hitCooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        laserCollider = GetComponent<Collider2D>();
        hitCooldownTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //this should only occur when the bullet is active and onscreen.
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        /*if (Time.time > hitCooldownTime + hitCooldown)
        {
            hitCooldownTime = Time.time;
            laserCollider.gameObject.SetActive(false);
        }
        else
        {
            laserCollider.gameObject.SetActive(true);
        }*/
    }

    //Check if laser collides with an enemy
    void OnTriggerStay2D(Collider2D collision)
    {
        if (Time.time < hitCooldownTime + hitCooldown)
            return;
        //if (owner == Owner.Player)
        //{
            if (collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
            {
                //destroy bullet
                //if (bulletType != BulletType.Pierce)
                    //bulletStruckUnit = true;

                //check for invul
                if (unit.IsInvulnerable())
                    return;
                    
                unit.hitPoints -= damage;
                //unit.SetHealth(unit.hitPoints);
                unit.UpdateHealthBar(unit.transform.position);
                hitCooldownTime = Time.time;
                
                Logger.DebugLogFormat("Laser struck enemy {0}", unit.name);
                //gameObject.SetActive(false);
            }
        //}
        /*else  //enemy bullet has hit a player's unit.
        {
            if (collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
            {
                //nothing happens. Base is unaffected by bullets.
                //TODO: Maybe show some kind of effect?
                bulletStruckUnit = true;
                Logger.DebugLog("enemy's Bullet struck player's base! No damage");
            }
            else if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                unit.hitPoints -= damage;
                //unit.SetHealth(unit.hitPoints);
                unit.UpdateHealthBar(unit.transform.position);

                //destroy bullet
                bulletStruckUnit = true;
                Logger.DebugLogFormat("enemy's Bullet struck player unit {0}", unit.name);
                //gameObject.SetActive(false);
            }
        }*/
    }
}
