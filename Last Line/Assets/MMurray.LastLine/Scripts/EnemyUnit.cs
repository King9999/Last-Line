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
        public List<Vector3> path;          //the path the enemy follows towards the player's base.
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
            healthBar.gameObject.SetActive(false);
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
                return;
            }*/

            //update movement. TODO: the translation has to factor in the length of the path, otherwise the unit speed will
            //be inconsistent.
            if (currentPoint != nextPoint)
            {
                //transform.Translate(direction * 0.1f * moveSpeed * Time.deltaTime);
                float step = 0.1f * moveSpeed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, nextPoint, step);

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

        public void SetPath(List<Vector3> newPath)
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
            PlayerUnit unit = collision.GetComponent<PlayerUnit>();
            if (unit == null)
                return;
                
            if (unit is PlayerUnit_Base)//collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
            {
                //base takes damage.
                //unitBase.hitPoints -= collisionDamage;
                //unitBase.UpdateHealthBar(unitBase.transform.position);
                //Logger.DebugLog("Hit Base " + unitBase.name);

                //unit.hitPoints -= collisionDamage;
                unit.ReduceHitPoints(collisionDamage);
                Logger.DebugLog("Hit " + unit.name);
                gm.RemoveEnemyUnit(this);
            }
            else if(unit is PlayerUnit_Bomb)//collision.TryGetComponent<PlayerUnit_Bomb>(out PlayerUnit_Bomb bomb))
            {
                //enemy unit dies, but bomb is activated.
                PlayerUnit_Bomb bomb = unit as PlayerUnit_Bomb;
                bomb.ActivateBomb();
                gm.RemoveEnemyUnit(this);
            }
            else //if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                Logger.DebugLog("Destroyed Unit " + unit.name);

                //both player unit and enemy unit die.
                gm.RemovePlayerUnit(unit);
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
