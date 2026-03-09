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
        [SerializeField]protected bool isEnhanced;        //becomes true when unit is placed in a power up space.

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
            
        }

        /*void Update()
        {
            if (healthBar.gameObject.activeSelf)
                UpdateHealthBar(transform.position);
        }*/

    }
}
