using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* This is a generic countdown timer */

namespace MMurray.GenericCode
{
    public class Timer : MonoBehaviour
    {
        private float time;           //time in seconds. Will use this value to get the minutes, seconds and milliseconds
        public float initTime;      //the initial start time. Used to get time elapsed.
        public bool timerRunning;
        public string timeText;     //the format to display the time.
        private float minutes, seconds, milliseconds; 

        public enum TimeFormat 
        {
            Default,                ///Displays time in minutes, seconds, and milliseconds
            MinutesAndSeconds,      ///Displays time in minutes and seconds.
            Seconds                 ///Displays only seconds.
        }

        public TimeFormat timeFormat;

        [Header("---Timer UI---")]
        public TextMeshProUGUI timerUI;

        // Start is called before the first frame update
        void Start()
        {
            //timerRunning = false;
        // time = 120;             //default time is 2 minutes.
            time = initTime;
        }

        // Update is called once per frame
        void Update()
        {
            //timer is displayed as a string so that extra zeroes can be added when necessary to display correct time
            if (timerRunning)
            {
                time -= Time.deltaTime;

                if (time < 0)
                {
                    time = 0;
                    timerRunning = false;
                }
            
            }

            //update timer display. Colour changes if less than 30% time left. I use rounding to prevent decimal 
            //values from delaying the colour change.
            UpdateTime();
            timerUI.color = (time <= Mathf.Round(initTime * 0.3f)) ? Color.red : Color.white;
            //timerUI.text = DisplayTimer();
            timerUI.text = timeText;
        }

        public void SetTimer(float seconds)
        {
            time = seconds;
            initTime = time;
        }

        public string CurrentTimeFormat(TimeFormat format)
        {
            string timeText = "";
            switch (format)
            {
                case TimeFormat.Default:
                    timeText = string.Format("{0:0}:{1:00}:{2:00}", minutes, seconds, milliseconds);
                    break;
                
                case TimeFormat.MinutesAndSeconds:
                    timeText = string.Format("{0:0}:{1:00}", minutes, seconds);
                    break;
                
                case TimeFormat.Seconds:
                    timeText = seconds.ToString();
                    break;
            }

            return timeText;
        }

        public string DisplayTimer()
        {
            UpdateTime();

            //change format as needed
            //string timeText = string.Format("{0:0}:{1:00}:{2:00}", minutes, seconds, milliseconds);
            timeText = CurrentTimeFormat(timeFormat);

            return timeText;
        }

        void UpdateTime()
        {
            minutes = Mathf.FloorToInt(time / 60);
            seconds = Mathf.FloorToInt(time % 60);
            milliseconds = time % 1 * 99;    //I only want the first two digits displayed so I'm not using 1000. Format wouldn't remove the extra digits.
            timeText = CurrentTimeFormat(timeFormat);
        }

        public string DisplayElapsedTime()
        {
            float elapsedTime = initTime - time;
            float minutes = Mathf.FloorToInt(elapsedTime / 60);
            float seconds = Mathf.FloorToInt(elapsedTime % 60);

            string timeText = string.Format("{0:0}:{1:00}", minutes, seconds);

            return timeText;
        }

        public bool TimeUp()
        {
            return time <= 0;
        }

        public void StopTimer()
        {
            timerRunning = false;
        }

        public void StartTimer()
        {
            timerRunning = true;
        }

    }
}
