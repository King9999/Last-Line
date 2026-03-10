using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* Dozers are slow moving units that have a barrier that protects them from frontal attacks. They are vulnerable
from above, below, and behind. If a player unit collides with the barrier, both the unit and barrier are destroyed,
but the dozer keeps moving at a faster speed. */
public class EnemyUnit_Dozer : EnemyUnit
{
    [SerializeField]private BoxCollider2D barrierCollider;

    /*void Update()
    {
        
    }*/

    /*void OnTriggerEnter2D(Collider2D collision)
    {
        if (barrierCollider.IsTouching(collision))
        {
            Logger.DebugLogFormat("Barrier hit by {0}", collision.name);
            return;
        }
            
    }*/

}
