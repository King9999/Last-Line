using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using LoLSDK;
using MMurray.GenericCode;

//we want to disable debug logs when not in unity editor to prevent potential performance issues.
#if !(UNITY_EDITOR || DEBUG)
    Debug.unityLogger.logEnabled = false;
#endif

/* This is the master singleton that's used to access various managers, i.e. other singletons. */
namespace MMurray.LastLine
{
    public class Singleton : MonoBehaviour
    {

        public static Singleton instance {get; private set;}
        public GameManager GameManager {get; set;}
        public DialogueManager DialogueManager { get; set; }
        //public HintDialogueManager HintDialogueManager { get; set; }
        public ScreenFade ScreenFade {get; private set;}
        //public UI UI { get; set; }
        public AudioManager AudioManager { get; private set; }
        public TitleManager TitleManager {get; private set;}
        public Menu Menu {get; set;}
        public ArrowGuideManager ArrowGuideManager {get; set;}

        //save state data
        public GameData gameData;
        public SaveState saveState;
        public bool saveStateFound;

        //TTS and music
        public bool ttsEnabled, musicEnabled;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            //if (instance != null)
                //Debug.Log("Singleton found");
            ScreenFade = GetComponentInChildren<ScreenFade>();
            //if (ScreenFade != null)
                //Debug.Log("ScreenFade not null");
            AudioManager = GetComponentInChildren<AudioManager>();
            gameData = new GameData();
            saveState = new SaveState();
            Application.runInBackground = false;

            //PlayerPrefs.DeleteAll();
            ttsEnabled = true;      //tts is on by default
            musicEnabled = true;


            DontDestroyOnLoad(instance);
        }

        public string GetText(string key)
        {
            string value = SharedState.LanguageDefs?[key];
            return value ?? "--missing--";
        }
    }
}
