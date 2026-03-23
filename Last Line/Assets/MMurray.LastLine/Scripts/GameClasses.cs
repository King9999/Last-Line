using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MMurray.LastLine
{
    [Serializable]
    public class LevelData          //to be written to JSON
    {
        public int level, starting_money, crystal_threshold, crystal_value;
        public List<PortalData> portals;
        public Vector2 player_base_position, base_point_axes;
    }

    [Serializable]
    public class Levels
    {
        public LevelData[] levels;
    }

    [Serializable]
    public class PortalData
    {
        public Vector2 point_axes;
        public Vector2 point_position;
        public Portal.EnemyType enemy_type;
        public float deploy_cooldown;
        public float deploy_time;
        public int enemy_count;
        public Vector3[] enemy_path_points;
    }
}
