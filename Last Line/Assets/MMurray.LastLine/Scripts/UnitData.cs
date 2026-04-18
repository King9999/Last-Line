using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* Contains the stats for every unit in the game. */
[CreateAssetMenu(fileName = "UnitData_", menuName = "Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    public enum UnitType { Base, StraightShooter, VerticalShooter, TrackingBeam, Bomb, Scout, Striker, Dozer, Juggernaut}
    public UnitType unitType;
    public string unitName;             //internal name that won't be used in game
    public int unitCost;                //only applies to player units
    public float unitBuildTime;         //applies to player units. How long it takes to build unit.
    public int unitMoneyReward;         //applies to enemy units. Money earned for defeating them.
    public int hitPoints;
    public float moveSpeed;             //this only applies to enemy units
    public int collisionDamage;         //only applies to enemies. How much damage they deal to the base.
    [Header("---Weapon Data---")]
    public float weaponCooldown;
    public float bulletSpeed;
    public float bulletDistance;
    public int weaponDamage;
    
}
