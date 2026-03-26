using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

public class ArrowGuideManager : MonoBehaviour
{
    [Serializable]
    public struct ArrowGuides
    {
        //public int level;
        public string key;              //matches the key of any active dialogue. if found, the arrows are shown.
        public List<ArrowGuide> arrowList;
    }

    public List<ArrowGuides> arrowGuides;
    public List<ArrowGuide> activeArrows;       //tracks which arrows are active so they can be deactivated later.
    
    void Awake()
    {
        Singleton.instance.ArrowGuideManager = this;
    }

    public void HideGuideArrows()
    {
        //hide any arrow guides that were shown
        if (activeArrows.Count <= 0)
            return;
        
        for (int i = 0; i < activeArrows.Count; i++)
        {
            activeArrows[i].gameObject.SetActive(false);
            activeArrows.Remove(activeArrows[i]);
            i--;
        }
        
    }
   
}
