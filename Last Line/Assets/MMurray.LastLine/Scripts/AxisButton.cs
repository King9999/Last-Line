using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using UnityEngine.Animations;

/* All this script does is hold a value for each button on the X and Y axis. When the button is clicked, the value
it holds is passed on elsewhere to create an ordered pair. */
public class AxisButton : MonoBehaviour
{
    [SerializeField]private int value;
    public enum AxisValue {X_AXIS, Y_AXIS, ORIGIN}
    /// <summary>
    /// Label for determining if a value is on the X or Y axis.
    /// </summary>
    public AxisValue axisValue;

    public void GetValue()
    {
        GameManager gm = Singleton.instance.GameManager;

        //check which axis we're on and record the value. A special case is made for the origin.
        if (axisValue == AxisValue.ORIGIN)
        {
            if (gm.enteringValueX)
                gm.playerCoordinates.x = value;
            else
                gm.playerCoordinates.y = value;
            Debug.Log("We're in the origin");
            return;
        }

        if (axisValue == AxisValue.X_AXIS)
            gm.playerCoordinates.x = value;
        else
            gm.playerCoordinates.y = value;
        
        Debug.LogFormat("player Coordinates {0}", gm.playerCoordinates);
    }


}
