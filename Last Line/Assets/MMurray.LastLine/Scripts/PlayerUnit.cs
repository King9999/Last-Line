using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MMurray.LastLine;

public class PlayerUnit : Unit
{
    // Start is called before the first frame update
    void Start()
    {
        GetUnitData(unitData);
        Debug.Log(HitPoints);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
