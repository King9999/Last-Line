using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* Used by different units. Bullets deal damage to other units. When instantiated, they begin travelling
towards their target. */
public class Bullet : MonoBehaviour
{
    public enum Owner {Player, Enemy}       //for collision checking.
    public Owner owner;
    public float moveSpeed;
    public float distance;
    public Vector2 direction;
    public int damage;
    public bool bulletStruckUnit;

    // Update is called once per frame
    void Update()
    {
        //this should only occur when the bullet is active and onscreen.
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        //TODO: check for collision or out of bounds.
    }

    //Check if bullet collides with an enemy
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == Owner.Player)
        {
            if (collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
            {
                unit.hitPoints -= damage;
                //unit.SetHealth(unit.hitPoints);
                unit.UpdateHealthBar(unit.transform.position);

                //destroy bullet
                bulletStruckUnit = true;
                Logger.DebugLog("Bullet struck enemy");
                //gameObject.SetActive(false);
            }
        }
        else  //enemy bullet has hit a player's unit.
        {
            
        }
    }
}
