using System.Collections;
using System.Collections.Generic;
using System;
using LoLSDK;
using UnityEngine;

/*read and write game data for saving game states. Required by LoL */

namespace MMurray.LastLine
{
    [Serializable]
    public class SaveState
    {
       public void WriteState(GameData data)
        {
            GameManager gm = Singleton.instance.GameManager;
            //data.test_unit_pos = gm.testUnit.transform.position;
            data.level = gm.currentLevel;

            //submit progress
            LOLSDK.Instance.SubmitProgress(0, data.level + 1, gm.maxLevels); //add 1 to level because level starts at 0.

            LOLSDK.Instance.SaveState(data);
        }

        public void ReadState(GameData data)
        {
            GameManager gm = Singleton.instance.GameManager;
            //gm.testUnit.transform.position = data.test_unit_pos;
            gm.currentLevel = data.level;
        }

        //This LoL code is used to check for an existing save state when the game runs.
        public void LoadState(Action<GameData> callback)
        {
            LOLSDK.Instance.LoadState<GameData>(state =>
            {
                if (state != null)
                {
                    callback(state.data);
                }

            });
        }
    }
}
