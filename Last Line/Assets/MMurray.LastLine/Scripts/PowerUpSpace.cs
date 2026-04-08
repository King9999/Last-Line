using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

/* If a player unit enters this space, it becomes enhanced by gaining more offensive power and 1 extra hit point. */
public class PowerUpSpace : MonoBehaviour
{
    public Point point;                             //reference to the point the unit is placed on.
    public Vector2 pointAxes;
    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerUnit unit = collision.GetComponent<PlayerUnit>();

        //just in case the space picks up a non-player unit.
        if (unit == null)
            return;

        unit.EnhanceUnit();
        Logger.DebugLogFormat("{0} is now enhanced", unit.name);
    }
}
