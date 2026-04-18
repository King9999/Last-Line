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
    [SerializeField]Transform leftBarrierPos, rightBarrierPos;
    bool speedUp = false;

    void Update()
    {
        base.Update();

        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        //barrier changes position depending on dozer's direction.
        if (barrierCollider.gameObject.activeSelf)
        {
            barrierCollider.transform.position = direction.x <= 0 ? leftBarrierPos.position : rightBarrierPos.position;
        }

        //speed increases whenever barrier is gone.
        if (!speedUp && !barrierCollider.gameObject.activeSelf)
        {
            currentInvulTime = Time.time;
            speedUp = true;
            moveSpeed *= 2;
        }

        //Dozer goes invul for a moment when barrier is gone to prevent dying immediately.
        if (isInvul && Time.time > currentInvulTime + 0.1f)
        {
            isInvul = false;
        }
    }

    /*void OnTriggerEnter2D(Collider2D collision)
    {
        if (barrierCollider.IsTouching(collision))
        {
            Logger.DebugLogFormat("Barrier hit by {0}", collision.name);
            return;
        }
            
    }*/

}
