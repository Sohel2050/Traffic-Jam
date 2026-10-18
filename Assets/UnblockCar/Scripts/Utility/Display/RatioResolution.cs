using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatioResolution 
{

    public static float GetResolution()
    {
#if UNITY_EDITOR

            int max = Mathf.Max((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
            int min = Mathf.Min((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
          return (float)max / min;
     
#else
        int max = Mathf.Max(Screen.width, Screen.height);
        int min = Mathf.Min(Screen.width, Screen.height);
        return (float)max / min;
#endif
    }


    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }
}
