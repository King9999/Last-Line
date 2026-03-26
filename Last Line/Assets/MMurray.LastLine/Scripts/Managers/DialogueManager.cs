using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.LastLine;
using System;

public class DialogueManager : MonoBehaviour
{
    public List<Dialogue> dialogueList;
    public List<Dialogue> commonDialogueList;     //dialogue for common scenarios, such as resetting a level or only one block remaining.
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

    /*****constants for various tutorial dialogue ******
    Used to find dialogue in the dialogue list*/
    public const int SELECTING_X_VALUE = 1;
    public const int SELECTING_Y_VALUE = 2;

    // Start is called before the first frame update
    void Awake()
    {
        Singleton.instance.DialogueManager = this;

        //disable all dialogues and triggers until ready for them
        foreach(Dialogue dialogue in dialogueList)
        {
            dialogue.gameObject.SetActive(false);
        }

        foreach (Dialogue dialogue in commonDialogueList)
        {
            dialogue.gameObject.SetActive(false);
        }

        /*foreach(DialogueTrigger trigger in dialogueTrigger)
        {
            trigger.gameObject.SetActive(false);
        }*/
    }
}
