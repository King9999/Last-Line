using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* A point contains a vector2 that's used to point to a space in the game where a player unit is located. Points can also contain 
the location of a crystal. */
namespace MMurray.LastLine
{
    public class Point : MonoBehaviour
    {
        public Vector2 pointAxes;
        public Unit unit;           //reference to a unit that's at this point.
        public Crystal crystal;
        public Portal portal;
        public PowerUpSpace powerUpSpace;
        public Image pointImage;    //used to visually display points when placing unit or selecting a point manually.

        void Start()
        {
            pointImage = GetComponent<Image>();
        }

        public void ClearPoint()
        {
            unit = null;
            crystal = null;
            portal = null;
            powerUpSpace = null;
        }

        public bool IsEmpty()
        {
            //note that power up space is excluded
            return unit == null && crystal == null && portal == null /*&& powerUpSpace == null*/;
        }


        //returns the world coordinates of the point. Used to place objects.
        public Vector2 WorldPoint()
        {
            return Camera.main.ScreenToWorldPoint(transform.position);
        }

        public void ShowPoint(bool toggle)
        {
            pointImage.enabled = toggle;
        }

    }
}
