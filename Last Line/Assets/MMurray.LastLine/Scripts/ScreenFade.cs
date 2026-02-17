using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//used to hide the screen before loading the next scene.
namespace MMurray.GenericCode
{
    public class ScreenFade : MonoBehaviour
    {
        public Image fadeImage;         //this must cover the entire screen.
        public float fadeSpeed;
        public bool coroutineOn;        //can be used to prevent any user input while the screen is fading.
        public static ScreenFade instance;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(instance);
        }

        // Start is called before the first frame update
        void Start()
        {
            //uncomment this line if you want the game to start with no fade in
            //fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
        }

        //fade to black
        public void ChangeSceneFadeOut(string sceneName)
        {
            StartCoroutine(FadeScreenToScene(sceneName));
        }

        //fade back to normal
        public void FadeIn()
        {
            StartCoroutine(FadeFromBlack());
        }
        ///<summary>
        ///Fade to black, then change scene. Once scene is changed, screen fades back to normal.
        ///</summary>
        ///<param name="sceneName">The name of the scene.</param>
        IEnumerator FadeScreenToScene(string sceneName)
        {
            coroutineOn = true;
            float alpha = 0;
            //float fadeSpeed = 2;

            while(fadeImage.color.a < 1)
            {
                alpha += fadeSpeed * Time.deltaTime;
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
                yield return null;
            }

            coroutineOn = false;
            SceneManager.LoadScene(sceneName);
            yield return FadeFromBlack();
        }

        
        /// <summary>
        /// Screen fades back to normal.
        /// </summary>
        /// <param name="delayDuration">The time in seconds that must pass before fade in. Can be used for when loading scene is slow.</param>
        IEnumerator FadeFromBlack(float delayDuration = 0)
        {
            coroutineOn = true;
            float alpha = 1;
            //float fadeSpeed = 2;
            yield return new WaitForSeconds(delayDuration);
            while(fadeImage.color.a > 0)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
                yield return null;
            }

            coroutineOn = false;
        }
    }
}
