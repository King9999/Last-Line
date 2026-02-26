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
        public Vector2 currentPoint, nextPoint;

        /*void Start()
        {
            GetUnitData(unitData);
            pathIndex = 0;

            isInvul = true;
            currentInvulTime = Time.time;
        }*/
        
        public override void GetUnitData(UnitData data)
        {
            base.GetUnitData(data);
            moveSpeed = data.moveSpeed;
            moneyAmount = data.unitMoneyReward;
            collisionDamage = data.collisionDamage;
        }

        void OnEnable()
        {
            GetUnitData(unitData);
            pathIndex = 0;
            isInvul = true;
            currentInvulTime = Time.time;
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
            if (currentPoint != nextPoint)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);

                if (Vector2.Distance(transform.position, nextPoint) < 0.01f)
                {
                    GetNextPoint();
                }
            }
        }

        public void SetPath(List<Vector2> newPath)
        {
            path = newPath;

            //get the first point
            //direction = new Vector2(path[pathIndex].x - transform.position.x, path[pathIndex].y - transform.position.y);
            //currentPoint = path[pathIndex];

            //get the destination
            //nextPoint = pathIndex + 1 < path.Count ? path[++pathIndex] : currentPoint;
            GetNextPoint();
        }

        /// <summary>
        /// Returns the next point in the path and sets the direction to that point.
        /// </summary>
        public void GetNextPoint()
        {
            currentPoint = path[pathIndex];
            nextPoint = pathIndex + 1 < path.Count ? path[++pathIndex] : currentPoint;
            transform.position = currentPoint;
            direction = new Vector2(nextPoint.x - transform.position.x, nextPoint.y - transform.position.y);
        }
    }
}
