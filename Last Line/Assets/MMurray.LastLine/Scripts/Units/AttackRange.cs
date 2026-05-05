using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public List<PlayerUnit> targets;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            if (!targets.Contains(unit) && unit is not PlayerUnit_Base)
            {
                targets.Add(unit);
            }
        }
    }
}
