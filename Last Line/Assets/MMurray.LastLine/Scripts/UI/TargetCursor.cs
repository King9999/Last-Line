using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Appears when the player chooses a point without selecting a unit first. */
public class TargetCursor : MonoBehaviour
{
    [SerializeField]float defaultScale = 1;     //both X and Y
    [SerializeField]float targetScale = 0.5f;   //both X and Y
    Coroutine animateCursor = null; 
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ShowCursor(Vector3 location)
    {
        if (animateCursor != null)
        {
            transform.localScale = new Vector3(defaultScale, defaultScale, 1);
            StopCoroutine(animateCursor);
        }
        gameObject.SetActive(true);
        transform.position = location;
        animateCursor = StartCoroutine(AnimateCursor());
    }
    
    /* Scales the cursor down to 0.5 from 1, then disappears. */
    IEnumerator AnimateCursor()
    {
        float scaleSpeed = 10;
        while(transform.localScale.x > targetScale && transform.localScale.y > targetScale)
        {
            float newScaleX = transform.localScale.x - scaleSpeed * Time.deltaTime;
            float newScaleY = transform.localScale.y - scaleSpeed * Time.deltaTime;
            transform.localScale = new Vector3(newScaleX, newScaleY, 1);
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);
        transform.localScale = new Vector3(defaultScale, defaultScale, 1);
        gameObject.SetActive(false);   
    }
}
