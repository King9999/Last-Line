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
                Logger.DebugLog("Bullet hit barrier");
            }
        }
        else if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            //destroy both unit and barrier.
            GameManager gm = Singleton.instance.GameManager;
            gm.RemovePlayerUnit(unit);
            EnemyUnit_Dozer dozer = gameObject.GetComponentInParent<EnemyUnit_Dozer>();
            dozer.SetInvul(true);
            gameObject.SetActive(false);
            
            Logger.DebugLog("Barrier hit unit");
        }
    }
}
