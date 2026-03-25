using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/* this is used to point to different elements on the screen. */
public class ArrowGuide : MonoBehaviour
{
    public GameObject targetedObject; //the object the arrow will point to.
    public Vector2 direction;
    private Sprite arrowSprite;
    private Coroutine animateCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        arrowSprite = GetComponent<Image>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Places an arrow near the desired game object.
    /// </summary>
    /// <param name="obj">The game object the arrow will be pointing to.</param>
    /// <param name="direction">The direction the arrow is facing. Alters the sprite. </param>
    public void PointArrow(GameObject obj, Vector2 direction)
    {
        /*the offset value will change depending on whether the arrow is pointing to a UI object or a game object.
        also, depending on the arrow's direction, its position will always be adjusted to be a short distance away 
        from the pointed object.*/
        //if (obj.layer == 5)     //UI layer
        float offset = (obj.layer == 5) ? 50 : 1;       //5 is the UI layer in the inspector.
        transform.position = new Vector2(obj.transform.position.x + (direction.x * -offset), obj.transform.position.y + (direction.y * -offset));
        animateCoroutine = StartCoroutine(AnimateArrow(direction));
    }

    IEnumerator AnimateArrow(Vector2 direction)
    {
        Vector2 oldPos = transform.position;
            
        float targetXPos = 0; 
        float targetYPos = 0;

        if (direction.x < 0)
        {
            targetXPos = (targetedObject.layer == 5) ? -10 : -1;
        }
        else if (direction.x > 0)
        {
            targetXPos = (targetedObject.layer == 5) ? 10 : 1;
        }

        if (direction.y < 0)
        {
            targetYPos = (targetedObject.layer == 5) ? -10 : -1;
        }
        else if (direction.y > 0)
        {
            targetYPos = (targetedObject.layer == 5) ? 10 : 1;
        }

        while (gameObject.activeSelf)
        {
            while(transform.position.x < oldPos.x + targetXPos)
            {
                Vector2 newPos = transform.position;
                transform.position = new Vector2(newPos.x + 20 * Time.deltaTime, newPos.y);
                yield return null;
            }
            transform.position = oldPos;
        }
    }
}
