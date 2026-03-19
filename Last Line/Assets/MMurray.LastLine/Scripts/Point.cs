using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A point contains a vector2 that's used to point to a space in the game where a player unit is located. Points can also contain 
the location of a crystal. */
namespace MMurray.LastLine
{
    public class Point : MonoBehaviour
    {
        public Vector2 point;
        public Unit unit;           //reference to a unit that's at this point.
        public Crystal crystal;
        public Portal portal;

        public void ClearPoint()
        {
            unit = null;
            crystal = null;
            portal = null;
        }

        public bool IsEmpty()
        {
            return unit == null && crystal == null && portal == null;
        }


        //returns the world coordinates of the point. Used to place objects.
        public Vector2 WorldPoint()
        {
            return Camera.main.ScreenToWorldPoint(transform.position);
        }

    }
}
