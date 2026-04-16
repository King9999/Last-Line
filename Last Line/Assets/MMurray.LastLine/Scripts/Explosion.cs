using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Simple effect that's played whenever a unit is destroyed. */
public class Explosion : MonoBehaviour
{
    [SerializeField]private ParticleSystem flash;
    [SerializeField]private ParticleSystem sparks;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(Vector2 location)
    {
        /*randomize the colours. Steps:
        -one of the RGB values is set to 255
        -the second value is set between 128 and 255
        -the final value is set between 0 and 255 */
        transform.position = location;
        flash.Play();
        sparks.Play();
    }
}
