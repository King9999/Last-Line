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
        [SerializeField]protected UnitData unitData;
        [SerializeField]protected string unitName;
        public int hitPoints, maxHitPoints;    
        public float weaponCooldown;   //time in seconds that must pass before unit can fire weapon, if applicable
        //public float moveSpeed; 
        public float bulletSpeed;
        public float bulletDistance;
        public int weaponDamage;

        public Slider healthBar;        //only visible when unit is damaged to reduce clutter.

        //gets data from scriptable object
        public virtual void GetUnitData(UnitData data)
        {
            unitName = data.unitName;
            weaponCooldown = data.weaponCooldown;
            //MoveSpeed = data.moveSpeed;
            maxHitPoints = data.hitPoints;
            hitPoints = maxHitPoints;
            bulletSpeed = data.bulletSpeed;
            weaponDamage = data.weaponDamage;
            bulletDistance = data.bulletDistance;
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
            
        }

        //fire unit's unique weapon if they have one.
        public virtual void FireWeapon()
        {
            
        }

        public virtual void SetupBullet(Bullet bullet)
        {
            bullet.moveSpeed = bulletSpeed;
            bullet.direction = Vector2.right;
            bullet.damage = weaponDamage;
            bullet.distance = bulletDistance;
        }

        public void SetHealth(int hitPoints)
        {
            if (hitPoints <= 0)
                return;

            healthBar.value = hitPoints;

            //health bar is only visible when unit is damaged.
            /*if (healthBar.value >= healthBar.maxValue)
                healthBar.gameObject.SetActive(false);
            else
                healthBar.gameObject.SetActive(true);*/
        }

        //updates health bar's slider value, position, and visibility.
        public void UpdateHealthBar(Vector2 unitPos)
        {

            healthBar.value = hitPoints;

            healthBar.transform.position = Camera.main.WorldToScreenPoint(new Vector2(unitPos.x + 0.2f, 
                        unitPos.y + 0.4f));

            //health bar is only visible when unit is damaged.
            if (healthBar.value >= healthBar.maxValue)
                healthBar.gameObject.SetActive(false);
            else
                healthBar.gameObject.SetActive(true);
        } 
    }
}
