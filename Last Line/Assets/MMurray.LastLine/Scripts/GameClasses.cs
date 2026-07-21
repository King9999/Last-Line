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
        public string level_name;       //Wave 1, Wave 2, etc. This is a key to be read by language json.
        public List<PortalData> portals;
        public Vector2 player_base_position, base_point_axes;
        public Vector2 power_up_space_position, power_up_space_point_axes;

        //bools for selectable units
        public bool[] toggle_unit;
        public const int STRAIGHT_SHOOTER = 0;
        public const int VERTICAL_SHOOTER = 1;
        public const int TRACKING_BEAM = 2;
        public const int BOMB = 3;
        public bool power_up_space_active;

        //planets. They're only cosmetic
        public bool show_small_planet, show_medium_planet, show_large_planet;
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
