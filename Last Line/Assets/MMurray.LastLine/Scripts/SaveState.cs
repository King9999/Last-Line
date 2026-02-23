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
            data.test_unit_pos = gm.testUnit.transform.position;

            LOLSDK.Instance.SaveState(data);
        }

        public void ReadState(GameData data)
        {
            GameManager gm = Singleton.instance.GameManager;
            gm.testUnit.transform.position = data.test_unit_pos;
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
