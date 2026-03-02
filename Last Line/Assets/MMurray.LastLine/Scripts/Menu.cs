using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* The menu for the main gameplay. Contains buttons for selecting units and game options. */
public class Menu : MonoBehaviour
{
    [Header("---Unit Buttons---")]
    [SerializeField]private Button unitButton_straightShooter;

    [Header("---System Buttons---")]
    [SerializeField]private Button ttsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button exitGameButton;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Opens a tooltip window displaying info about a button when mouse hovers over it.
    /// </summary>
    public void OnMouseHoverEnter()
    {
        Logger.DebugLog("Hovering over button");
    }

    /// <summary>
    /// Hides tooltip when mouse leaves button.
    /// </summary>
    public void OnMouseHoverExit()
    {
        Logger.DebugLog("Leaving button");
    }
    
}
