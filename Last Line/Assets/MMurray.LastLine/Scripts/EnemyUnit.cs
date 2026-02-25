using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MMurray.LastLine;

namespace MMurray.LastLine
{
    public class EnemyUnit : Unit
    {
        public Vector2 direction;
        public float moveSpeed;
        public int moneyAmount;
        public int collisionDamage;
        protected const float INVUL_TIME = 0.8f;    //time that unit can't be damaged after being delpoyed.
        protected float currentInvulTime;
        protected bool isInvul;

        void Start()
        {
            GetUnitData(unitData);
        }
        
        public override void GetUnitData(UnitData data)
        {
            base.GetUnitData(data);
            moveSpeed = data.moveSpeed;
            moneyAmount = data.unitMoneyReward;
            collisionDamage = data.collisionDamage;
        }

        void OnEnable()
        {
            isInvul = true;
            currentInvulTime = Time.time;

            //get direction of the player's base. TODO: Enemy will move along a path until it reaches player base.
            direction = Vector2.left;
        }

        void Update()
        {
            //Units cannot immediately be damaged when deployed.
            if (isInvul)
            {
                if (Time.time > currentInvulTime + INVUL_TIME)
                {
                    isInvul = false;
                    Logger.DebugLog("Invul removed");
                }
            }

            //update movement
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }
}
