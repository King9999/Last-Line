using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//This class contains all data that will be written to file.
namespace MMurray.LastLine
{
    [Serializable]
    public class GameData
    {
        /********level data********/
        public int level;
        public int max_levels;      //includes tutorial levels

        /***********player data. Includes positions of all player units, as well as money *******/
        public List<Vector2> unit_positions;
        public int money;

        /**********Enemy data**********/

        

        /*********universal data*********/
        public bool music_enabled = true;
        public bool tts_enabled = true;
    }
}
