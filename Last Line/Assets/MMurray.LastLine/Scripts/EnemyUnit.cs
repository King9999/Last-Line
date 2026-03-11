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
        public Portal portal;               //reference to the portal the enemy belongs to.
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

            //health bar setup. Only visible when damaged.
            healthBar.maxValue = maxHitPoints;
            //SetHealth(maxHitPoints);
        }

        protected void Update()
        {
            GameManager gm = Singleton.instance.GameManager;
            if (!gm.GameRunning())
                return;
                
            //Units cannot immediately be damaged when deployed.
            if (isInvul)
            {
                if (Time.time > currentInvulTime + INVUL_TIME)
                {
                    isInvul = false;
                    Logger.DebugLog("Invul removed on " + unitName);
                }
            }
            //is enemy dead?
            /*else if (hitPoints <= 0)
            {
                gm.RemoveEnemyUnit(this);
            }*/

            //update movement
            if (currentPoint != nextPoint)
            {
                transform.Translate(direction * 0.1f * moveSpeed * Time.deltaTime);

                //update healthbar position if necessary
                if (healthBar.gameObject.activeSelf)
                {
                    UpdateHealthBar(transform.position);
                    //healthBar.transform.position = Camera.main.WorldToScreenPoint(new Vector2(transform.position.x + 0.2f, 
                        //transform.position.y + 0.4f));
                }

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

        //Checks for collision against player units.
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (isInvul)
                return;

            GameManager gm = Singleton.instance.GameManager;
            if (collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
            {
                //base takes damage.
                unitBase.hitPoints -= collisionDamage;
                unitBase.UpdateHealthBar(unitBase.transform.position);
                Logger.DebugLog("Hit Base " + unitBase.name);
            }
            else if(collision.TryGetComponent<PlayerUnit_Bomb>(out PlayerUnit_Bomb bomb))
            {
                //enemy unit dies, but bomb is activated.
                gm.RemoveEnemyUnit(this);
                bomb.ActivateBomb();
            }
            else if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                Logger.DebugLog("Hit Unit " + unit.name);

                //both player unit and enemy unit die.
                
                gm.RemovePlayerUnit(unit);
                //gm.deadPlayerUnits.Add(unit);
                gm.RemoveEnemyUnit(this);
            }
        }

        public bool IsInvulnerable()
        {
            return isInvul;
        }

        public void SetInvul(bool toggle)
        {
            isInvul = toggle;
        }
    }
}
