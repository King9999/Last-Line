using System.Collections;
using System.Collections.Generic;
using MMurray.GenericCode;
using MMurray.LastLine;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* Manager for the title screen. */
public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Must call the screen fade since the game begins with a black background on init.
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
