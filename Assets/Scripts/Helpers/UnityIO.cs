using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class UnityIO
{

#if UNITY_EDITOR

    [MenuItem("TestProject/OpenProjectFolder")]
    public static void OpenProjectFolder() 
    {
        EditorUtility.RevealInFinder(Application.dataPath);    
    }
#endif
}
