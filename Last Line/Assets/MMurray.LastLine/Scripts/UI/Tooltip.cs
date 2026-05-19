using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using MMurray.LastLine;

public class Tooltip : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI unitDetails;    //describes what unit does.
    [SerializeField]private TextMeshProUGUI unitCost, unitBuildTime, unitDamage, unitHealth;
    private const float tooltipYOffset = -120;
    private const float tooltipXOffset = -50;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }



    public void DisplayUnitTooltip(bool toggle, string details = "", string cost = "", string buildTime = "", 
        string damage = "", string health = "")
    {
        if (toggle == false)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);

            //place the tooltip around the mouse's position
            GameManager gm = Singleton.instance.GameManager;
            gameObject.transform.position = new Vector2(Input.mousePosition.x + tooltipXOffset * gm.screenWidthScale, Input.mousePosition.y + tooltipYOffset * gm.screenHeightScale);
            unitDetails.text = details;
            unitCost.text = cost;
            unitBuildTime.text = string.Format("{0} sec.", buildTime);
            unitDamage.text = damage;
            unitHealth.text = health;
        }
    }
}
