using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMurray.LastLine;

/* Progress bar is displayed when a unit is being built. They show which unit is being built and where. */
public class ProgressBar : MonoBehaviour
{
    public Image radialBar;               //radial image that fills up over time.
    public Image unitIcon;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        radialBar.fillAmount = 0;
    }

    /// <summary>
    /// Display progress bar at specified point on the grid.
    /// </summary>
    /// <param name="point">The point on the grid to display the bar. This is a screen position.</param>
    public void DisplayProgressBar(bool toggle, Point point = null)
    {
        gameObject.SetActive(toggle);

        if (toggle == true)
        {
            transform.position = point.transform.position;
            //TODO: display unit icon
        }
    }

}
