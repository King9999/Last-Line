using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;

/*displays hints at the start of a level, instructing where a unit should be built, but not which unit.
Also used to show a crystal's coordinates. */
public class HintManager : MonoBehaviour
{
    [Serializable]
    public struct Hint
    {
        public int level;           //the level to display the hint.
        public string hintKey;
    }

    public List<Hint> hints;

    void Awake()
    {
        Singleton.instance.HintManager = this;
    }
    
    public string ShowHint(int level)
    {
        bool levelFound = false;
        int i = 0;
        while(!levelFound && i < hints.Count)
        {
            if (hints[i].level == level)
            {
                levelFound = true;
            }
            else
            {
                i++;
            }
        }

        return Singleton.instance.GetText(hints[i].hintKey);
       
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/
}
