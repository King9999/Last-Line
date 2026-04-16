using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Simple effect that's played whenever a unit is destroyed. */
public class Explosion : MonoBehaviour
{
    [SerializeField]private ParticleSystem flash;
    [SerializeField]private ParticleSystem sparks;
    

    public void Play(Vector2 location)
    {
        /*randomize the colours. Steps:
        -one of the RGB values is set to 255
        -the second value is set between 128 and 255
        -the final value is set between 0 and 255 */
        float[] colorValues = new float[3];     //0 is red, 1 is green, 2 is blue

        //pick a random colour to be maxed out
        int randColor = Random.Range(0, colorValues.Length);
        colorValues[randColor] = 1;

        //next colour is between 0.5 and 1
        int usedColor = randColor;
        do
        {
            randColor = Random.Range(0, colorValues.Length);
        }
        while(randColor == usedColor);
        float randValue = Random.Range(0.5f, 1);
        colorValues[randColor] = randValue;

        //final colour value's range is between 0 and 1
        int otherUsedColor = randColor;
        do
        {
            randColor = Random.Range(0, colorValues.Length);
        }
        while(randColor == usedColor || randColor == otherUsedColor);
        randValue = Random.value;
        colorValues[randColor] = randValue;

        //set new color.
        Color newColor = new Color(colorValues[0], colorValues[1], colorValues[2]);

        var flashMain = flash.main;
        var sparksMain = sparks.main;
        flashMain.startColor = newColor;
        sparksMain.startColor = newColor;

        transform.position = location;
        flash.Play();
        sparks.Play();
    }

    public bool IsPlaying()
    {
        return flash.isPlaying || sparks.isPlaying;
    }
}
