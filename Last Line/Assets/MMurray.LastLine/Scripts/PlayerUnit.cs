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
        public int UnitCost {get; set;}
        // Start is called before the first frame update
        void Start()
        {
            GetUnitData(unitData);
            Debug.Log(UnitCost);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public override void GetUnitData(UnitData data)
        {
            base.GetUnitData(data);
            UnitCost = data.unitCost;
        }
    }
}
