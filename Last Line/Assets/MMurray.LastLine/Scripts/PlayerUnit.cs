using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using MMurray.LastLine;

/* Unit that's controlled by hte player. Player units don't move, and cost money to deploy. 
The player's base counts as a unit. */
namespace MMurray.LastLine
{
    public class PlayerUnit : Unit
    {
        public int unitCost;
        public float buildTime;                         //the time it takes to deploy unit.
        public Sprite unitSprite;                       //used by the menu's build queue.
        public Point point;                             //reference to the point the unit is placed on.
        public Vector2 pointAxes;
        [SerializeField]protected bool isEnhanced;        //becomes true when unit is placed in a power up space.
        [SerializeField]protected bool isInvul;         //when damaged by bullet, unit becomes invul for a duration. Still not protected against collisions.
        [SerializeField]protected float invulTime = 2;  //time in seconds
        protected float invulCounter;

        public override void GetUnitData(UnitData data)
        {
            base.GetUnitData(data);
            unitCost = data.unitCost;
            buildTime = data.unitBuildTime;
        }

        void OnEnable()
        {
            GetUnitData(unitData);
            //healthBar.gameObject.SetActive(true);
            //health bar setup. Only visible when damaged.
            //healthBar.transform.position = Camera.main.WorldToScreenPoint(new Vector2(transform.position.x + 0.2f, 
                        //transform.position.y + 0.4f));
            healthBar.maxValue = maxHitPoints;
            healthBar.gameObject.SetActive(false);
            //UpdateHealthBar(transform.position);
            //TODO: For some reason the player unit's health bar is not positioning correctly.
        }

        public void SetEnhancedState(bool toggle)
        {
            isEnhanced = toggle;
        }

        /* Power up unit. The enhancements depend on the unit. */
        public virtual void EnhanceUnit()
        {
            //standard buff for all units.
            maxHitPoints += 1;
            hitPoints = maxHitPoints;
        }

        public void SetInvul()
        {
            isInvul = true;
            invulCounter = Time.time;
            Logger.DebugLogFormat("{0} is invul", unitName);
        }

        public bool IsInvul()
        {
            return isInvul;
        }

        protected virtual void Update()
        {
            GameManager gm = Singleton.instance.GameManager;
            if (!gm.GameRunning())
                return;

            if (isInvul && Time.time > invulCounter + invulTime)
            {
                isInvul = false;
                Logger.DebugLogFormat("Invul removed on {0}", unitName);
            }
            /*if (hitPoints <= 0)
            {
                GameManager gm = Singleton.instance.GameManager;
                gm.RemovePlayerUnit(this);
            }*/
        }

    }
}
