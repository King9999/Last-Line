//#define ENABLE_LOGS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/* This class is only used to test things while in the Unity Editor. This code should not be included in the release build. */
public static class Logger
{
     public static int playerUnitId = 0; 
     private static int enemyUnitId = 0;     //used to identify units more easily.
   [Conditional("UNITY_EDITOR")]
   public static void DebugLog(string logStr)
   {
        UnityEngine.Debug.Log(logStr);
   }

   [Conditional("UNITY_EDITOR")]
   public static void DebugLogFormat(string logStr, params object[] args)
   {
        UnityEngine.Debug.LogFormat(logStr, args);
   }

   [Conditional("UNITY_EDITOR")]
   public static void AddPlayerId(String unitName)
     {
          unitName += playerUnitId.ToString();
          playerUnitId++;
     }

   [Conditional("UNITY_EDITOR")]
   public static void AddEnemyId(String unitName) 
   { 
          unitName += enemyUnitId.ToString();
          enemyUnitId++; 
   }
}
