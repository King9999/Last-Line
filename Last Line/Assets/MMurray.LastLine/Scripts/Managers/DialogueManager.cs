using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using System;

public class DialogueManager : MonoBehaviour
{
    public List<Dialogue> dialogueList;
    //public List<Dialogue> commonDialogueList;     //dialogue for common scenarios
    //public List<DialogueTrigger> dialogueTrigger;
    public bool dialogueOpen;                       //prevents player actions while dialogue window is open.


    //conditional triggers activate after the start or the end of a level. the player does not collide with these.
    [Serializable]
    public struct ConditionalTrigger
    {
        public int level;
        public int dialogueId;
        //public Dialogue dialogue;
        public enum Condition { None, LevelStart, LevelEnd }
        public Condition condition;
    }

    public List<ConditionalTrigger> condTriggerList;

    /* Common dialogue appears after a game over to provide tips. */
    [Serializable]
    public struct GameOverDialogue
    {
        public int level;
        public int dialogueId;          //dialogue id should start at 200 so it doesn't get mixed up with conditional triggers.
    }

    public List<GameOverDialogue> gameOverDialogueList;

    /*****constants for various tutorial dialogue ******
    Used to find dialogue in the dialogue list*/
    public const int SELECTING_X_VALUE = 1;
    public const int SELECTING_Y_VALUE = 2;
    public const int SELECTING_SECOND_UNIT = 3;
    public const int ACTIVATING_UNIT_ABILITY = 4;
    public const int ENEMY_INCOMING = 5;
    public const int CRYSTAL_INTRO = 8;
    public const int ORIGIN_TUTORIAL = 9;

    // Start is called before the first frame update
    void Awake()
    {
        Singleton.instance.DialogueManager = this;

        //disable all dialogues and triggers until ready for them
        foreach(Dialogue dialogue in dialogueList)
        {
            dialogue.gameObject.SetActive(false);
        }

        /*foreach (CommonDialogue dialogue in commonDialogueList)
        {
            dialogue.gameObject.SetActive(false);
        }*/

        /*foreach(DialogueTrigger trigger in dialogueTrigger)
        {
            trigger.gameObject.SetActive(false);
        }*/
    }
}
