using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* This is a script used by the Dozer enemy unit. Barrier is unaffected by all weapons, and can only be destroyed by collision with a
player unit. */
public class Barrier : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bullet>(out Bullet bullet))
        {
            if (bullet.owner == Bullet.Owner.Player)
            {
                bullet.bulletStruckUnit = true; //this should be true even if the bullet pierces

                //Only the straight shooter has bullets, so we get a reference to it to set the cooldown.
                //if unit is enhanced, cooldown is reset temporarily to original value.
                if (bullet.bulletType == Bullet.BulletType.Pierce)
                {
                    PlayerUnit_StraightShooter unit = bullet.unit as PlayerUnit_StraightShooter;
                    unit.weaponCooldown = unit.unitData.weaponCooldown;
                }
                Logger.DebugLog("Bullet hit barrier");
            }
        }
        else if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            //destroy both unit and barrier.           
            GameManager gm = Singleton.instance.GameManager;

            if (unit is PlayerUnit_Base)
                //just remove barrier. Base doesn't take damage from barrier.
                gameObject.SetActive(false);
            else
            {
                gm.RemovePlayerUnit(unit);
                EnemyUnit_Dozer dozer = gameObject.GetComponentInParent<EnemyUnit_Dozer>();
                dozer.SetInvul(true);
                gameObject.SetActive(false);
            }
            
            Logger.DebugLog("Barrier hit unit");
        }
    }
}
