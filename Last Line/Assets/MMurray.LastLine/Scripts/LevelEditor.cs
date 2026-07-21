using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MMurray.GenericCode;

/* This script is used to add portals and enemy paths to the scene, and record their positions to a JSON file. 
This all works by copying vector2 data from Level into LevelData. the LevelData gets written to JSON. */
namespace MMurray.LastLine
{
    [ExecuteInEditMode]
    public class LevelEditor : MonoBehaviour
    {
        [Serializable]
        public class Level
        {
            public int level, startingMoney, crystalThreshold, crystalValue;
            public string levelName;
            public List<Portal> portals;
            public PlayerUnit_Base playerBase;
            public PowerUpSpace powerUpSpace;

            //bools
            public bool[] toggleUnit = new bool[4];
            public bool powerUpSpaceActive;

            //planets. They're only cosmetic
            public bool showSmallPlanet, showMediumPlanet, showLargePlanet;
        }

        /*[Serializable]
        public class LevelData          //to be written to JSON
        {
            public int level, starting_money, crystal_threshold, crystal_value;
            public List<PortalData> portals;
            //public List<EnemyPath> enemy_path;  //for each portal, one of these must be added.
            public Vector2 player_base_position, base_point_axes;

            //bools for selectable units

        }*/

        [Serializable]
        public class LevelDataArray
        {
            public LevelData[] levels;
        }

        /*[Serializable]
        public class EnemyPath
        {
            public List<Vector2> path_points;
        }

        [Serializable]
        public class PortalData
        {
            public Vector2 point_axes;        //the location on the Point where the portal will be placed
            public Vector2 point_position;    //the point's actual transform position.
            public Portal.EnemyType enemy_type;
            public float deploy_cooldown;
            public float deploy_time;
            public int enemy_count;
            public Vector3[] enemy_path_points;
        }*/
        
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
            //Logger.DebugLog("Updating");
            //foreach(Level level in levels)
                //WriteToFile(levels);
            //ReadFile();
            
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Save"))
            {
                Logger.DebugLog("Save Button pressed");
                WriteToFile(levels);
            }
        }

        void WriteToFile(List<Level> levelList)
        {
            /*var path = Resources.Load<TextAsset>("levels");
            if (path == null)
            {
                Logger.DebugLogWarning("Path not found!");
                return;
            }*/
            
            StreamWriter writer = new StreamWriter(levelsPath);
                //for each level, copy vector2 from portal list and player base, and create a LevelData.
                levelDataList = new List<LevelData>();
                foreach(Level newLevel in levelList)
                {
                    LevelData data = new LevelData();
                    data.level = newLevel.level;
                    data.level_name = newLevel.levelName;
                    data.starting_money = newLevel.startingMoney;
                    data.crystal_threshold = newLevel.crystalThreshold;
                    data.crystal_value = newLevel.crystalValue;
                    data.toggle_unit = new bool[newLevel.toggleUnit.Length];
                    for (int i = 0; i < data.toggle_unit.Length; i++)
                    {
                        data.toggle_unit[i] = newLevel.toggleUnit[i];
                    }

                    //planet data
                    data.show_small_planet = newLevel.showSmallPlanet;
                    data.show_medium_planet = newLevel.showMediumPlanet;
                    data.show_large_planet = newLevel.showLargePlanet;

                    /*data.toggle_straight_shooter = newLevel.toggleStraightShooter;
                    data.toggle_vertical_shooter = newLevel.toggleVerticalShooter;
                    data.toggle_tracking_beam = newLevel.toggleTrackingBeam;
                    data.toggle_bomb = newLevel.toggleBomb;*/

                    //player base data
                    if (newLevel.playerBase != null)
                    {
                        data.player_base_position = newLevel.playerBase.point.transform.position;
                        data.base_point_axes = newLevel.playerBase.point.pointAxes;
                    }

                    //power up space data
                    if (newLevel.powerUpSpace != null)
                    {
                        data.power_up_space_active = newLevel.powerUpSpaceActive;
                        data.power_up_space_position = newLevel.powerUpSpace.point.transform.position;
                        data.power_up_space_point_axes = newLevel.powerUpSpace.point.pointAxes;
                    }

                    //get portal data.
                    data.portals = new List<PortalData>();
                    foreach (Portal portal in newLevel.portals)
                    {
                        PortalData newPortal = new PortalData()
                        {
                            //save the point's actual position
                            point_axes = portal.point.pointAxes,
                            point_position = portal.point.transform.position,
                            enemy_type = portal.enemyType,
                            deploy_cooldown = portal.deployCooldown,
                            enemy_count = portal.enemyCount,
                            deploy_time = portal.timer.initTime,
                        };

                        //get enemy path points
                        newPortal.enemy_path_points = new Vector3[portal.enemyPath.positionCount];
                        for (int i = 0; i < portal.enemyPath.positionCount; i++)
                        {
                            //portal.pathPoints.Add(portal.enemyPath.GetPosition(i));
                            //newPortal.enemy_path_points.Add(portal.enemyPath.GetPosition(i));
                            newPortal.enemy_path_points[i] = portal.enemyPath.GetPosition(i);
                        }
                        data.portals.Add(newPortal);
                    }

                    //add to data list.
                    levelDataList.Add(data);
                }

                //write the level value and the starting money
                LevelDataArray levelArray = new LevelDataArray();
                levelArray.levels = new LevelData[levelDataList.Count];
                //LevelData[] levels = new LevelData[levelDataList.Count];
                levelDataList.CopyTo(levelArray.levels);
                string levelString = JsonUtility.ToJson(levelArray, true);
                //string level = JsonUtility.ToJson(levelDataList[i], true);
                //string startingMoney = JsonUtility.ToJson(levels[i].starting_money);
                //Logger.DebugLogFormat("Level: {0}", level);


                writer.Write(levelString);
            
            
            writer.Close();

        }

        void ReadFile()
        {
            StreamReader reader = new StreamReader(levelsPath);
            Logger.DebugLog(reader.ReadToEnd());
            reader.Close();
        }
    }
}
