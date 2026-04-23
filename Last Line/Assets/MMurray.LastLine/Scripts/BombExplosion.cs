using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BombExplosion : MonoBehaviour
{
    [SerializeField]private ParticleSystem bombExplosion;


    public void Play(Vector2 location, float blastRange = 8)
    {
        transform.position = location;
        var blastMain = bombExplosion.main;
        blastMain.startSize = blastRange;
        bombExplosion.Play();
    }
}
