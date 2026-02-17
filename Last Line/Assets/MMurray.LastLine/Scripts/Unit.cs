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
        public int HitPoints {get; set;}    
        public float Cooldown {get; set;}   //time in seconds that must pass before unit can fire weapon, if applicable
        public float MoveSpeed {get; set;}
        //public float 
        

        //gets data from scriptable object
        public void GetUnitData(UnitData data)
        {
            Cooldown = data.weaponCooldown;
            MoveSpeed = data.moveSpeed;
            HitPoints = data.hitPoints;
        }

        /// <summary>
        /// Place a unit on the map at the given position.
        /// </summary>
        /// <param name="position">The unit's initial position when placed onto the map.</param>
        public void SetUnitPosition(Vector2 position)
        {
            
        } 
    }
}
