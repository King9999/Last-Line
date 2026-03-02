using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI unitDetails;    //describes what unit does.
    [SerializeField]private TextMeshProUGUI unitCost, unitDamage, unitHealth;
    private const float tooltipYOffset = -120;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayUnitTooltip(bool toggle, string details = "")
    {
        if (toggle == false)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);

            //place the tooltip around the mouse's position
            gameObject.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y + tooltipYOffset);
            unitDetails.text = details;
        }
    }
}
