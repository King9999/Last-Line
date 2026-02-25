using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains the stats for every unit in the game. */
[CreateAssetMenu(fileName = "UnitData_", menuName = "Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    public string unitName;             //internal name that won't be used in game
    public int unitCost;                //only applies to player units
    public int unitMoneyReward;         //applies to enemy units. Money earned for defeating them.
    public int hitPoints;
    public float moveSpeed;             //this only applies to enemy units
    [Header("---Weapon Data---")]
    public float weaponCooldown;
    public float bulletSpeed;
    public float bulletDistance;
    public int weaponDamage;
    
}
