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
    public Transform arrowLocation;     //the arrow's position.
    public Vector2 direction;
    private Image arrow;
    private Coroutine animateCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        arrow = GetComponent<Image>();
        //gameObject.SetActive(false);
    }

    void OnEnable()
    {
        arrow = GetComponent<Image>();
    }


    /// <summary>
    /// Places an arrow near the desired game object.
    /// </summary>
    /// <param name="obj">The game object the arrow will be pointing to.</param>
    /// <param name="direction">The direction the arrow is facing. Alters the sprite. </param>
    public void PointArrow(GameObject obj, Vector2 direction)
    {
        gameObject.SetActive(true);
        /*the offset value will change depending on whether the arrow is pointing to a UI object or a game object.
        also, depending on the arrow's direction, its position will always be adjusted to be a short distance away 
        from the pointed object.*/
        //if (obj.layer == 5)     //UI layer
        float offset;
        if (obj.layer == 5)  //5 is the UI layer in the inspector.
        {
            offset = 50;
            transform.position = new Vector2(obj.transform.position.x + (direction.x * -offset), obj.transform.position.y + (direction.y * -offset));
        }
        else
        {
            //adjust arrow's transform to world pos to match game object
            offset = 1;
            transform.position = Camera.main.ScreenToWorldPoint(new Vector2(obj.transform.position.x + (direction.x * -offset), obj.transform.position.y + (direction.y * -offset)));
        }
        //float offset = (obj.layer == 5) ? 50 : 1;       //5 is the UI layer in the inspector.
        //transform.position = new Vector2(obj.transform.position.x + (direction.x * -offset), obj.transform.position.y + (direction.y * -offset));

        //change sprite orientation if needed
        if (direction.x < 0)
            arrow.transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x > 0)
            arrow.transform.localScale = new Vector3(1, 1, 1);

        animateCoroutine = StartCoroutine(AnimateArrow(direction));
    }

    public void PointArrow(Transform location, Vector2 direction)
    {
        //gameObject.SetActive(true);
        /*the offset value will change depending on whether the arrow is pointing to a UI object or a game object.
        also, depending on the arrow's direction, its position will always be adjusted to be a short distance away 
        from the pointed object.*/
        //if (obj.layer == 5)     //UI layer
        transform.position = location.position;
        //float offset = (obj.layer == 5) ? 50 : 1;       //5 is the UI layer in the inspector.
        //transform.position = new Vector2(obj.transform.position.x + (direction.x * -offset), obj.transform.position.y + (direction.y * -offset));

        //change sprite orientation if needed
        if (direction.x < 0)
            arrow.transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x > 0)
            arrow.transform.localScale = new Vector3(1, 1, 1);

        animateCoroutine = StartCoroutine(AnimateArrow(direction));
    }

    IEnumerator AnimateArrow(Vector2 direction)
    {
        Vector2 oldPos = transform.position;
            
        float targetXPos = 0; 
        float targetYPos = 0;

        if (direction.x < 0)
        {
            targetXPos = -10; //(targetedObject.layer == 5) ? -10 : -1;
        }
        else if (direction.x > 0)
        {
            targetXPos = 10; //(targetedObject.layer == 5) ? 10 : 1;
        }

        if (direction.y < 0)
        {
            targetYPos = -10; //(targetedObject.layer == 5) ? -10 : -1;
        }
        else if (direction.y > 0)
        {
            targetYPos = 10; //(targetedObject.layer == 5) ? 10 : 1;
        }

        Vector2 targetPos = new Vector2(transform.position.x + targetXPos, transform.position.y + targetYPos);

        while (gameObject.activeSelf)
        {
            float moveSpeed = 1.5f;
            while(Vector2.Distance(transform.position, targetPos) > 0.1f /*, transform.position.x < oldPos.x + targetXPos*/)
            {
                Vector2 newPos = transform.position;
                transform.position = new Vector2(newPos.x + (targetXPos * moveSpeed * Time.deltaTime), newPos.y + (targetYPos * moveSpeed * Time.deltaTime));
                yield return null;
            }
            transform.position = oldPos;
        }
    }
}
