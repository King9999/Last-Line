//#define ENABLE_LOGS

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Logger
{
   [Conditional("UNITY_EDITOR")]
   public static void DebugLog(string logStr)
   {
        UnityEngine.Debug.Log(logStr);
   }
}
