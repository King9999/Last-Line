using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* Used by different units. Bullets deal damage to other units. When instantiated, they begin travelling
towards their target. */
public class Bullet : MonoBehaviour
{
    public enum Owner {Player, Enemy}       //for collision checking.
    public enum BulletType {Normal, Pierce, Chain}
    public Owner owner;
    public BulletType bulletType;
    public float moveSpeed;
    public float distance;
    public Vector2 direction;
    public int damage;
    public bool bulletStruckUnit;

    // Update is called once per frame
    void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;
            
        //this should only occur when the bullet is active and onscreen.
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        //TODO: check for collision or out of bounds.
    }

    //Check if bullet collides with an enemy
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == Owner.Player)
        {
            if (collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
            {
                //destroy bullet
                if (bulletType != BulletType.Pierce)
                    bulletStruckUnit = true;

                //check for invul
                if (unit.IsInvulnerable())
                    return;
                    
                //unit.hitPoints -= damage;
                unit.ReduceHitPoints(damage);
                //unit.SetHealth(unit.hitPoints);
                //unit.UpdateHealthBar(unit.transform.position);

                
                Logger.DebugLogFormat("Player bullet struck enemy {0}", unit.name);
                //gameObject.SetActive(false);
            }
        }
        else  //enemy bullet has hit a player's unit.
        {
            //ignore any enemy attack range colliders
            if (collision.TryGetComponent<CircleCollider2D>(out CircleCollider2D range))
                return; 

            PlayerUnit unit = collision.GetComponent<PlayerUnit>();
            if (unit is PlayerUnit_Base || unit is PlayerUnit_Bomb)//collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
            {
                //nothing happens. Base is unaffected by bullets.
                //TODO: Maybe show some kind of effect?
                bulletStruckUnit = true;
                Logger.DebugLog("enemy's Bullet struck player's base/bomb! No damage");
            }
            else //if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                //unit.hitPoints -= damage;
                unit.ReduceHitPoints(damage);
                //unit.SetHealth(unit.hitPoints);
                //unit.UpdateHealthBar(unit.transform.position);

                //destroy bullet
                if (bulletType != BulletType.Pierce)
                    bulletStruckUnit = true;
                Logger.DebugLogFormat("enemy's Bullet struck player unit {0}", unit.name);
                //gameObject.SetActive(false);
            }
        }
    }
}
