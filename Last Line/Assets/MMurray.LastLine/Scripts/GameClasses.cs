using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MMurray.LastLine
{
    [Serializable]
    public class LevelData          //to be written to JSON
    {
        public int level, starting_money;
        public List<PortalData> portals;
        public Vector2 player_base_location;
    }

    [Serializable]
    public class Levels
    {
        public LevelData[] levels;
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
}
