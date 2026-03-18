using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* This script is used to add portals and enemy paths to the scene, and record their positions to a JSON file. */
[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour
{
    [Serializable]
    public struct Level
    {
        public int level, startingMoney;
        public List<Portal> portalList;
        public PlayerUnit_Base playerBase;
    }
   
    public List<Level> levels;
    public TextAsset levelFile;
    string levelsPath = Application.dataPath + "/MMurray.LastLine/Resources/levels";

    // Start is called before the first frame update
    void Awake()
    {
        Logger.DebugLog("Test");
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        Logger.DebugLog("Updating");
        //WriteToFile("test 123");
        //ReadFile();
    }

    void WriteToFile(string text)
    {
        var path = Resources.Load<TextAsset>("levels");
        if (path == null)
        {
            Logger.DebugLogWarning("Path not found!");
            return;
        }
        
        //string test = JsonUtility.ToJson(levels[0].portalList[0].enemyPath, true);
        //string newPath = Application.dataPath + "/MMurray.LastLine/Resources/levels";
        Logger.DebugLog("Path: " + levelsPath);
        StreamWriter writer = new StreamWriter(levelsPath, true);
        writer.Write(text);
        writer.Close();

    }

    void ReadFile()
    {
        StreamReader reader = new StreamReader(levelsPath);
        Logger.DebugLog(reader.ReadToEnd());
        reader.Close();
    }
}
