using System.Collections;
using System.Collections.Generic;
using MMurray.LastLine;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]private List<Collider2D> unitColliders;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerUnit>(out PlayerUnit unit))
        {
            if (!unitColliders.Contains(collision))
            {
                unitColliders.Add(collision);
            }
        }
    }
}
