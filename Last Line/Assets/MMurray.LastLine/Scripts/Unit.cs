using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/* This is the base class for all units in the game. */
namespace MMurray.LastLine
{
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField]protected UnitData unitData;
        [SerializeField]protected string unitName;
        public int hitPoints;    
        public float weaponCooldown;   //time in seconds that must pass before unit can fire weapon, if applicable
        //public float moveSpeed; 
        

        //gets data from scriptable object
        public virtual void GetUnitData(UnitData data)
        {
            unitName = data.unitName;
            weaponCooldown = data.weaponCooldown;
            //MoveSpeed = data.moveSpeed;
            hitPoints = data.hitPoints;
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
    }
}
