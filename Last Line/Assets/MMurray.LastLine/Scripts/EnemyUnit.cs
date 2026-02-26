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
        [Header("---Pathing---")]
        public List<Vector2> path;          //the path the enemy follows towards the player's base.
        [SerializeField]private int pathIndex;

        void Start()
        {
            GetUnitData(unitData);
            pathIndex = 0;
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
            //direction = Vector2.left;
        }

        void Update()
        {
            //Units cannot immediately be damaged when deployed.
            if (isInvul)
            {
                if (Time.time > currentInvulTime + INVUL_TIME)
                {
                    isInvul = false;
                    Logger.DebugLog("Invul removed on " + unitName);
                }
            }

            //update movement
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }

        public void SetPath(List<Vector2> newPath)
        {
            path = newPath;

            //get the first point
            direction = new Vector2(path[pathIndex].x - transform.position.x, path[pathIndex].y - transform.position.y);
        }
    }
}
