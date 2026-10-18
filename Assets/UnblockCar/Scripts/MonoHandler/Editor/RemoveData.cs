using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RemoveData  
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
    }
}
