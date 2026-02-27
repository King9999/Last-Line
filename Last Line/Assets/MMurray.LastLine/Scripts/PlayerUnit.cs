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

        public override void GetUnitData(UnitData data)
        {
            base.GetUnitData(data);
            unitCost = data.unitCost;
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

        /*void Update()
        {
            if (healthBar.gameObject.activeSelf)
                UpdateHealthBar(transform.position);
        }*/

    }
}
