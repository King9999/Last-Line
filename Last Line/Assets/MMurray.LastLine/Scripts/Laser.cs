using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using MMurray.GenericCode;

/*Laser is similar to bullet except it inherently pierces. It's also larger than a standard bullet. */
public class Laser : MonoBehaviour
{
    public enum Owner {Player, Enemy}       //for collision checking.
    public enum BulletType {Normal, Chain}  //piercing is normal for a laser
    public Owner owner;
    public BulletType bulletType;
    public float moveSpeed;
    public float distance;
    public Vector2 direction, forwardDirection, angle;   //forwardDirection is for adjusting the laser's orientation.     
    public int damage;
    public bool bulletStruckUnit;
    private Collider2D laserCollider;
    public float hitCooldown = 0.5f;      //prevents laser from hitting every frame.
    private float hitCooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        laserCollider = GetComponent<Collider2D>();
        hitCooldownTime = 0;
        forwardDirection = transform.rotation * Vector2.up;     //meaning the laser travels up on the Y axis by default.
    }

    // Update is called once per frame
    void Update()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (!gm.GameRunning())
            return;
            
        //this should only occur when the bullet is active and onscreen.
        transform.Translate(direction * forwardDirection * moveSpeed * Time.deltaTime);

        /*if (Time.time > hitCooldownTime + hitCooldown)
        {
            hitCooldownTime = Time.time;
            laserCollider.gameObject.SetActive(false);
        }
        else
        {
            laserCollider.gameObject.SetActive(true);
        }*/
    }

    /// <summary>
    /// Returns the angle between the initial vector (a) and the destination vector (b). The result will be applied
    /// to the Z rotation.
    /// </summary>
    /// <param name="a">The starting vector, which must match the orientation of the laser's sprite.</param>
    /// <param name="b">The destination vector.</param>
    /// <returns>The Z angle of rotation in degrees.</returns>
    public float GetAngle(Vector2 a, Vector2 b)
    {
        /*first we get the dot product of a and b. Refresher:
        Dot product of a . b = (a1 * b1) + (a2 * b2) */
        //float dotProduct = (a.x * b.x) + (a.y * b.y);
        float dotProduct = Vector2.Dot(a, b);

        //next we get the magnitude of both vectors
        float magnitudeA = a.magnitude; //Vector2.SqrMagnitude(a);
        float magnitudeB = b.magnitude; //Vector2.SqrMagnitude(b);

        //next we calculate the inverse cosine to get the angle. The value returned from Mathf.Acos is in radians so we
        //must convert to degrees.
        float z = Mathf.Rad2Deg * Mathf.Acos(dotProduct / (magnitudeA * magnitudeB));
        //float z = Mathf.Rad2Deg * Mathf.Acos(0.9733f);
        Logger.DebugLogFormat("Inv. Cos: {0}", z);
        return z;
    }

    public void FireWeapon(Vector2 direction, float angle)
    {
        gameObject.SetActive(true);
        this.direction = direction;     //TODO: Direction must factor in the angle somehow so it moves correctly after rotation.
        transform.Rotate(0, 0, angle);
    }

    //Check if laser collides with an enemy
    void OnTriggerStay2D(Collider2D collision)
    {
        Logger.DebugLogFormat("Time: {0} Hit Cooldown: {1}", Time.time, hitCooldownTime + hitCooldown);
        if (Time.time < hitCooldownTime + hitCooldown)
            return;
        //if (owner == Owner.Player)
        //{
            if (collision.TryGetComponent<EnemyUnit>(out EnemyUnit unit))
            {
                //destroy bullet
                //if (bulletType != BulletType.Pierce)
                    //bulletStruckUnit = true;

                //check for invul
                if (unit.IsInvulnerable() || unit.isDefeated)
                    return;
                    
                //unit.hitPoints -= damage;
                unit.ReduceHitPoints(damage);
                //unit.SetHealth(unit.hitPoints);
                //unit.UpdateHealthBar(unit.transform.position);
                hitCooldownTime = Time.time;
                
                Logger.DebugLogFormat("Laser struck enemy {0}", unit.name);
                //gameObject.SetActive(false);
            }
        //}
        /*else  //enemy bullet has hit a player's unit.
        {
            if (collision.TryGetComponent<PlayerUnit_Base>(out PlayerUnit_Base unitBase))
            {
                //nothing happens. Base is unaffected by bullets.
                //TODO: Maybe show some kind of effect?
                bulletStruckUnit = true;
                Logger.DebugLog("enemy's Bullet struck player's base! No damage");
            }
            else if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
            {
                unit.hitPoints -= damage;
                //unit.SetHealth(unit.hitPoints);
                unit.UpdateHealthBar(unit.transform.position);

                //destroy bullet
                bulletStruckUnit = true;
                Logger.DebugLogFormat("enemy's Bullet struck player unit {0}", unit.name);
                //gameObject.SetActive(false);
            }
        }*/
    }
}
