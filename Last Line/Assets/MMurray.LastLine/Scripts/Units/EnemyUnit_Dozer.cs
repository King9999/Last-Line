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
    [SerializeField]ParticleSystem barrierParticles;
    Vector3 particleAngle = new Vector3(0, 90, 0);
    bool speedUp = false;

    void Update()
    {
        base.Update();

        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;

        //barrier changes position depending on dozer's direction. The particle's angle has to change for the particles to 
        //animate towards the unit.
        if (barrierCollider.gameObject.activeSelf)
        {
            if (direction.x <= 0.05f)   //for cases where the unit is moving only slightly left.
            {
                barrierCollider.transform.position = leftBarrierPos.position;
                barrierParticles.transform.localEulerAngles = particleAngle;
            }
            else
            {
                barrierCollider.transform.position = rightBarrierPos.position;
                barrierParticles.transform.localEulerAngles = -particleAngle;
            }
            //barrierCollider.transform.position = direction.x <= 0 ? leftBarrierPos.position : rightBarrierPos.position;
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
