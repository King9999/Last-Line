using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MMurray.GenericCode;
using UnityEditor.Experimental.GraphView;

/* This script is used to add portals and enemy paths to the scene, and record their positions to a JSON file. 
This all works by copying vector2 data from Level into LevelData. the LevelData gets written to JSON. */
[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour
{
    [Serializable]
    public class Level
    {
        public int level, startingMoney;
        public List<Portal> portals;
        public PlayerUnit_Base playerBase;
    }

    [Serializable]
    public class LevelData          //to be written to JSON
    {
        public int level, starting_money;
        public List<PortalData> portals;
        //public List<EnemyPath> enemy_path;  //for each portal, one of these must be added.
        public Vector2 player_base_location;
    }

    [Serializable]
    public class EnemyPath
    {
        public List<Vector2> path_points;
    }

    [Serializable]
    public class PortalData
    {
        public Vector2 location;
        public Portal.EnemyType enemy_type;
        public float deploy_cooldown;
        public float deploy_time;
        public float enemy_count;
        public List<Vector2> enemy_path_points;
    }
    
    public List<Level> levels;
    public List<LevelData> levelDataList;
    public TextAsset levelFile;
    string levelsPath = Application.dataPath + "/MMurray.LastLine/Resources/levels.json";

    // Start is called before the first frame update
    void Awake()
    {
        Logger.DebugLog("Test");
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        Logger.DebugLog("Updating");
        foreach(Level level in levels)
            WriteToFile(level);
        //ReadFile();
    }

    void WriteToFile(Level newLevel)
    {
        /*var path = Resources.Load<TextAsset>("levels");
        if (path == null)
        {
            Logger.DebugLogWarning("Path not found!");
            return;
        }*/
        
        StreamWriter writer = new StreamWriter(levelsPath);
            //for each level, copy vector2 from portal list and player base, and create a LevelData.
            LevelData data = new LevelData();
            data.level = newLevel.level;
            data.starting_money = newLevel.startingMoney;

            //player base data
            if (newLevel.playerBase != null)
                data.player_base_location = newLevel.playerBase.transform.position;

            //get portal data.
            data.portals = new List<PortalData>();
            foreach (Portal portal in newLevel.portals)
            {
                PortalData newPortal = new PortalData()
                {
                    location = portal.transform.position,
                    enemy_type = portal.enemyType,
                    deploy_cooldown = portal.deployCooldown,
                    enemy_count = portal.enemyCount,
                    deploy_time = portal.timer.initTime,
                };

                //get enemy path points
                newPortal.enemy_path_points = new List<Vector2>();
                for (int i = 0; i < portal.enemyPath.positionCount; i++)
                {
                    //portal.pathPoints.Add(portal.enemyPath.GetPosition(i));
                    newPortal.enemy_path_points.Add(portal.enemyPath.GetPosition(i));
                }
                data.portals.Add(newPortal);
            }

            //write the level value and the starting money
            string level = JsonUtility.ToJson(data, true);
            //string level = JsonUtility.ToJson(levelDataList[i], true);
            //string startingMoney = JsonUtility.ToJson(levels[i].starting_money);
            //Logger.DebugLogFormat("Level: {0}", level);

            writer.Write(level);
            //writer.Write(startingMoney);

            //record portal data
            /*for (int j = 0; j < levels[i].portalList.Count; j++)
            {
                string test = JsonUtility.ToJson(levels[i].portalList[j].enemyPath, true);
            }*/
        
        //string newPath = Application.dataPath + "/MMurray.LastLine/Resources/levels";
        //Logger.DebugLog("Path: " + levelsPath);
        
        
        writer.Close();

    }

    void ReadFile()
    {
        StreamReader reader = new StreamReader(levelsPath);
        Logger.DebugLog(reader.ReadToEnd());
        reader.Close();
    }
}
