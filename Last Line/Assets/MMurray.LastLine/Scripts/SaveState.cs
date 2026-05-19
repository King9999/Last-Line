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
            Singleton s = Singleton.instance;
            //data.test_unit_pos = gm.testUnit.transform.position;
            data.level = gm.currentLevel;
            data.max_levels = gm.maxLevels;
            data.tts_enabled = s.ttsEnabled;
            data.music_enabled = s.musicEnabled;

            //dialogue state setup
            DialogueManager dm = s.DialogueManager;
            data.dialogue_read = new bool[dm.dialogueList.Count];
            for (int i = 0; i < dm.dialogueList.Count; i++)
            {
                data.dialogue_read[i] = dm.dialogueList[i].dialogueRead;
                Logger.DebugLogFormat("Dialogue {0} read state: {1}", i, data.dialogue_read[i]);
            }

            //submit progress
            LOLSDK.Instance.SubmitProgress(0, data.level, gm.maxLevels);    //level 0 is the tutorial.
            LOLSDK.Instance.SaveState(data);
        }

        public void ReadState(GameData data)
        {
            GameManager gm = Singleton.instance.GameManager;
            Singleton s = Singleton.instance;
            //gm.testUnit.transform.position = data.test_unit_pos;
            gm.currentLevel = data.level;
            s.musicEnabled = data.music_enabled;
            s.ttsEnabled = data.tts_enabled;
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
