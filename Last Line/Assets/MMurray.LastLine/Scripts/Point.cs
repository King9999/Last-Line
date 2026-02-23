using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A point contains a vector2 that's used to point to a space in the game where a player unit is located. */
namespace MMurray.LastLine
{
    public class Point : MonoBehaviour
    {
        public Vector2 point;
        public Unit unit;           //reference to a unit that's at this point.
    }
}
