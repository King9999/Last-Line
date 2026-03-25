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
    
    void Awake()
    {
        Singleton.instance.ArrowGuideManager = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
