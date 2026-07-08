using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MMurray.LastLine;
using System;

/* modified version of the menu script. */
public class MinigameMenu : Menu
{
    [SerializeField]private TextMeshProUGUI crystalCoordinatesLabel;
    [SerializeField]private List<CheckBox> crystalLocationCheckboxes;
    [SerializeField]List<Vector2> crystalPoints;
    [SerializeField]Sprite checkmarkSpriteOn, checkmarkSpriteOff;
    //public int level;               //there are 2 levels.

    [Serializable]
    public struct CheckBox
    {
        public bool isOn;
        public TextMeshProUGUI checkBoxText;
        public Image checkBoxImage;
    }

    void Awake()
    {
        Singleton.instance.MinigameMenu = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        crystalCoordinatesLabel.text = GetText("crystalCoordinates");
        foreach(CheckBox toggle in crystalLocationCheckboxes)
            toggle.checkBoxImage.sprite = checkmarkSpriteOff;
        
        UpdateSystemButtonSprites();

        //update text based on level
        MinigameManager mm = Singleton.instance.MinigameManager;
        if (mm.level == 1)
        {
            //set the points
            //crystalPoints.Add(new Point());
            //crystalPoints[0].pointAxes = new Vector2(1, 4);
            //crystalPoints[1].pointAxes = new Vector2(5, 0);
            //crystalPoints[2].pointAxes = new Vector2(2, 2);
        

            //update text
            crystalLocationCheckboxes[0].checkBoxText.text = GetText("miningLevel1_coordinates1");
            crystalLocationCheckboxes[1].checkBoxText.text = GetText("miningLevel1_coordinates2");
            crystalLocationCheckboxes[2].checkBoxText.text = GetText("miningLevel1_coordinates3");
            crystalLocationCheckboxes[3].checkBoxText.text = GetText("miningLevel1_coordinates4");
            crystalLocationCheckboxes[4].checkBoxText.text = GetText("miningLevel1_coordinates5");
        }
        else
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void UpdateLevel(int level)
    {
        GameManager gm = Singleton.instance.GameManager;
        if (level < 1 || level > 2)
            return;
        
        currentLevel.text = string.Format("{0} {1}", GetText("miningLevel"), level);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }
}
