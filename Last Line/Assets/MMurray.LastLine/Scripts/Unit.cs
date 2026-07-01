using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using UnityEngine.UI;

/* This is the base class for all units in the game. */
namespace MMurray.LastLine
{
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField]public UnitData unitData;
        [SerializeField]protected string unitName;
        public UnitData.UnitType unitType;
        public int hitPoints, maxHitPoints;    
        public float weaponCooldown;   //time in seconds that must pass before unit can fire weapon, if applicable
        //public float moveSpeed; 
        public float bulletSpeed;
        public float bulletDistance;   
        public Vector2 bulletRange;         //if a bullet reaches this vector, it's destroyed.
        public float bulletTime;        //the lifetime of the bullet in seconds. Once this is exceeded, the bullet disappears.
        public int weaponDamage;

        public Slider healthBar;        //only visible when unit is damaged to reduce clutter.
        public SpriteRenderer unitSpriteRenderer;
        public Sprite uiSprite;         //used for UI and portal unit icons.
        public BoxCollider2D unitHitbox;    //unit's collision box. Does not include any other colliders, such as attack ranges.

        //coroutine
        Coroutine animateUnit = null;
        bool coroutineRunning;

        /*void Update()
        {
            unitSpriteRenderer.transform.position = new Vector3(0, -3, 1);
            /*if (!coroutineRunning)
            {
                coroutineRunning = true;
                animateUnit = StartCoroutine(AnimateUnit());
            }
        }*/
        void OnDisable()
        {
            coroutineRunning = false;
            if (animateUnit != null)
                StopCoroutine(animateUnit);
            
        }

        public void AnimateUnit()
        {
            if (!coroutineRunning)
            {
                coroutineRunning = true;
                animateUnit = StartCoroutine(Animate());
            }
        }
        //gets data from scriptable object
        public virtual void GetUnitData(UnitData data)
        {
            unitName = data.unitName;
            unitType = data.unitType;
            weaponCooldown = data.weaponCooldown;
            //MoveSpeed = data.moveSpeed;
            maxHitPoints = data.hitPoints;
            hitPoints = maxHitPoints;
            bulletSpeed = data.bulletSpeed;
            weaponDamage = data.weaponDamage;
            bulletDistance = data.bulletDistance;
            bulletTime = data.bulletTime;
        }

        /// <summary>
        /// Place a unit on the map at the given position.
        /// </summary>
        /// <param name="position">The unit's initial position when placed onto the map.</param>
        public void SetUnitPosition(Vector2 position)
        {
            
        }

        //Used by specific units to active a special ability, such as changing firing direction.
        public virtual void ActivateAbility()
        {
            GameManager gm = Singleton.instance.GameManager;
            gm.DisplayAlertWithDuration(GetText("noUnitAbility"));
        }

        //fire unit's unique weapon if they have one.
        public virtual void FireWeapon(Bullet bullet, Vector2 direction)
        {
            
        }

        public virtual void SetupBullet(Bullet bullet)
        {
            bullet.unit = this;
            bullet.transform.position = transform.position;
            bullet.moveSpeed = bulletSpeed;
            bullet.direction = Vector2.right;
            bullet.damage = weaponDamage;
            bullet.distance = bulletDistance;
        }

        public Vector2 SetBulletRange(Bullet bullet)
        {
            Vector2 range = new Vector2(transform.position.x + (bullet.distance * bullet.direction.x), transform.position.y + 
                (bullet.distance * bullet.direction.y));
            
            Logger.DebugLogFormat("Bullet range: {0}", range);
            return range;
            //Debug.DrawRay(transform.position + new Vector3(bullet.direction.x, bullet.direction.y, 0), bulletRange, Color.red);
        }

        public virtual void SetupLaser(Laser laser)
        {
            laser.moveSpeed = bulletSpeed;
            laser.direction = Vector2.right;
            laser.damage = weaponDamage;
            laser.distance = bulletDistance;
            laser.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        string GetText(string key)
        {
            string value = SharedState.LanguageDefs?[key];
            return value ?? "--missing--";
        }

        public void ReduceHitPoints(int amount)
        {
            if (amount <= 0 || hitPoints <= 0)
                return;

            hitPoints -= amount;
            UpdateHealthBar(transform.position);

            //check if unit is dead
            if (hitPoints <= 0)
            {
                GameManager gm = Singleton.instance.GameManager;
                if (this is PlayerUnit)
                {
                    if (this is PlayerUnit_Base)
                    {
                         Logger.DebugLog("Base destroyed");
                         //gm.playerBase.gameObject.SetActive(false);
                         //TODO: Freeze game and show base exploding
                         gm.SetGameState(gm.gameState = GameManager.GameState.BaseDestroyed);
                    }
                    else
                    {
                       gm.RemovePlayerUnit(this as PlayerUnit);
                    }

                }
                else
                {
                    //remove enemy unit. player gets money for this kill
                    EnemyUnit enemy = this as EnemyUnit;
                    gm.AddMoney(enemy.moneyAmount, enemy.transform.position);
                    //gm.money += enemy.moneyAmount;
                    gm.RemoveEnemyUnit(enemy, false);
                }
            }
            else
            {
                PlayerUnit unit = this as PlayerUnit;
                if (unit != null)
                {
                    if (unit is not PlayerUnit_Base)
                    {
                        //unit becomes temporarily invul
                        unit.SetInvul();
                    }
                    else
                    {
                        //display base's particles to indicate it got damaged.
                        PlayerUnit_Base unitBase = this as PlayerUnit_Base;
                        unitBase.PlayParticles();
                    }
                }
            }
        }

        //updates health bar's slider value, position, and visibility.
        public void UpdateHealthBar(Vector2 unitPos)
        {
            if (hitPoints <= 0)
                return;

            healthBar.value = hitPoints;

            healthBar.transform.position = Camera.main.WorldToScreenPoint(new Vector2(unitPos.x + 0.2f, 
                        unitPos.y + 0.4f));

            //health bar is only visible when unit is damaged.
            if (healthBar.value >= healthBar.maxValue)
                healthBar.gameObject.SetActive(false);
            else
                healthBar.gameObject.SetActive(true);
        } 

        //float unit up and down. Only the sprite itself moves to prevent hitboxes from being affected.
        IEnumerator Animate()
        {
            float yDestination = 0.1f;
            float moveSpeed = 0.1f;
            Vector2 origPos = unitSpriteRenderer.transform.position;

            //float up
            while (unitSpriteRenderer.transform.position.y < origPos.y + yDestination)
            {
                Vector2 unitPos = unitSpriteRenderer.transform.position;
                unitSpriteRenderer.transform.position = new Vector2(unitPos.x, unitPos.y + moveSpeed * Time.deltaTime);
                yield return null;
            }

            //float down
            while (unitSpriteRenderer.transform.position.y > origPos.y)
            {
                Vector2 unitPos = unitSpriteRenderer.transform.position;
                unitSpriteRenderer.transform.position = new Vector2(unitPos.x, unitPos.y - moveSpeed * Time.deltaTime);
                yield return null;
            }
            unitSpriteRenderer.transform.position = origPos;
            coroutineRunning = false;
        }
    }
}
