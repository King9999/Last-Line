using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Used by different units. Bullets deal damage to other units. When instantiated, they begin travelling
towards their target. */
public class Bullet : MonoBehaviour
{
    public float moveSpeed;
    public float distance;
    public Vector2 direction;
    public int damage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this should only occur when the bullet is active and onscreen.
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        //TODO: check for collision or out of bounds.
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
