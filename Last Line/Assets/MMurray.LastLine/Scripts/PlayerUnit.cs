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

        
    }
}
