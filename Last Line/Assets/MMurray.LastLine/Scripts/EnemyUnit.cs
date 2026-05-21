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
        public bool isDefeated;         //if true, unit is running away and cannot hurt player units or base.
        protected const float INVUL_TIME = 0.6f;    //time that unit can't be damaged after being delpoyed.
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

        protected virtual void OnEnable()
        {
            GetUnitData(unitData);
            isInvul = true;
            isDefeated = false;
            currentInvulTime = Time.time;

            //health bar setup. Only visible when damaged.
            healthBar.maxValue = maxHitPoints;
            healthBar.gameObject.SetActive(false);
            unitHitbox = GetComponent<BoxCollider2D>();
            unitHitbox.enabled = true;
            //SetHealth(maxHitPoints);

            //reset sprite alpha
            unitSpriteRenderer.color = new Color(unitSpriteRenderer.color.r, unitSpriteRenderer.color.g, unitSpriteRenderer.color.b, 1);
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
            //path.Clear();
            pathIndex = 0;
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
            if (isInvul || isDefeated)
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
                gm.RemoveEnemyUnit(this, true);
            }
            else if(unit is PlayerUnit_Bomb)//collision.TryGetComponent<PlayerUnit_Bomb>(out PlayerUnit_Bomb bomb))
            {
                //enemy unit dies, but bomb is activated.
                PlayerUnit_Bomb bomb = unit as PlayerUnit_Bomb;
                bomb.ActivateBomb();
                gm.RemoveEnemyUnit(this, true);
            }
            else //if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                Logger.DebugLog("Destroyed Unit " + unit.name);

                //both player unit and enemy unit die.
                gm.RemovePlayerUnit(unit);
                gm.RemoveEnemyUnit(this, true);
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
