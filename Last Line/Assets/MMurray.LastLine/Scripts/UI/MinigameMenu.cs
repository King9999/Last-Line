using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MMurray.LastLine;
using System;

/* modified version of the menu script. */
public class MinigameMenu : MonoBehaviour
{
    Singleton s = Singleton.instance;
    public SystemTooltip systemTooltip;
    [SerializeField]private TextMeshProUGUI crystalCoordinatesLabel;
    public List<CheckBox> crystalLocationCheckboxes;
    [SerializeField]List<Vector2> crystalPoints;
    [SerializeField]Sprite checkmarkSpriteOn, checkmarkSpriteOff;
    [SerializeField]private TextMeshProUGUI currentLevel;

    [Header("---Money---")]
    [SerializeField]private GameObject moneyContainer;
    [SerializeField]private TextMeshProUGUI moneyLabel;
    [SerializeField]private TextMeshProUGUI moneyValue;

    [Header("---System Buttons---")]
    [SerializeField]private Button ttsButton;
    [SerializeField]private Button musicButton;
    [SerializeField]private Button exitGameButton;
    [Header("---System Button Images---")]
    [SerializeField]Image ttsButtonImage;
    [SerializeField]Image musicButtonImage;
    [SerializeField]Sprite ttsSpriteOn; 
    [SerializeField]Sprite ttsSpriteOff;
    [SerializeField]Sprite musicSpriteOn;
    [SerializeField]Sprite musicSpriteOff;
    //public int level;               //there are 2 levels.

    [Serializable]
    public struct CheckBox
    {
        public Vector2 crystalLocation;     //used to find the correct checkbox to mark.
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
        moneyLabel.text = s.GetText("moneyLabel");
        crystalCoordinatesLabel.text = s.GetText("crystalCoordinates");
        foreach(CheckBox toggle in crystalLocationCheckboxes)
            toggle.checkBoxImage.sprite = checkmarkSpriteOff;
        
        UpdateSystemButtonSprites();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this must be called in MinigameManager's Start()
    public void SetupCheckboxes()
    {
        MinigameManager mm = Singleton.instance.MinigameManager;
        UpdateLevel(mm.level);
        if (mm.level == 1)
        {
            //set the points
            //crystalPoints.Add(new Point());
            //crystalPoints[0].pointAxes = new Vector2(1, 4);
            //crystalPoints[1].pointAxes = new Vector2(5, 0);
            //crystalPoints[2].pointAxes = new Vector2(2, 2);
        

            //update text
            crystalLocationCheckboxes[0].checkBoxText.text = s.GetText("miningLevel1_coordinates1");
            crystalLocationCheckboxes[1].checkBoxText.text = s.GetText("miningLevel1_coordinates2");
            crystalLocationCheckboxes[2].checkBoxText.text = s.GetText("miningLevel1_coordinates3");
            crystalLocationCheckboxes[3].checkBoxText.text = s.GetText("miningLevel1_coordinates4");
            crystalLocationCheckboxes[4].checkBoxText.text = s.GetText("miningLevel1_coordinates5");
            
        }
        else
        {
            crystalLocationCheckboxes[0].checkBoxText.text = s.GetText("miningLevel2_coordinates1");
            crystalLocationCheckboxes[1].checkBoxText.text = s.GetText("miningLevel2_coordinates2");
            crystalLocationCheckboxes[2].checkBoxText.text = s.GetText("miningLevel2_coordinates3");
            crystalLocationCheckboxes[3].checkBoxText.text = s.GetText("miningLevel2_coordinates4");
            crystalLocationCheckboxes[4].checkBoxText.text = s.GetText("miningLevel2_coordinates5");
        }

        //get vectors. This is used to search for correct box to tick.
        for(int i = 0; i < crystalLocationCheckboxes.Count; i++)
        {
            CheckBox box = crystalLocationCheckboxes[i];
            box.crystalLocation = mm.minigameLevelData[mm.level - 1].crystals[i].crystalLocation;
            crystalLocationCheckboxes[i] = box;
        }
    }

    public void ToggleCheckBox(CheckBox box, bool isOn)
    {
        box.isOn = isOn;
        if (box.isOn)
            box.checkBoxImage.sprite = checkmarkSpriteOn;
        else
            box.checkBoxImage.sprite = checkmarkSpriteOff;
    }

    public void UpdateLevel(int level)
    {
        GameManager gm = Singleton.instance.GameManager;
        if (level < 1 || level > 2)
            return;
        
        currentLevel.text = string.Format("{0} {1}", s.GetText("miningLevel"), level);
    }

    public void UpdateMoneyText()
    {
        MinigameManager mm = Singleton.instance.MinigameManager;
        moneyValue.text = string.Format("{0:000}", mm.money);
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void UpdateSystemButtonSprites()
    {
        Singleton s = Singleton.instance;
        //ttsButtonImage.sprite = s.ttsEnabled ? ttsSpriteOn : ttsSpriteOff;
        musicButtonImage.sprite = s.musicEnabled ? musicSpriteOn : musicSpriteOff;
    }
}
