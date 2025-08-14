using UnityEditor;
using UnityEngine;



public class EditorUtils
{

    [MenuItem("Utils/Clear PlayerPrefs")]

    static public void ClearPlayerPrefs()
    {

        PlayerPrefs.DeleteAll();

        PlayerPrefs.Save();

    }

}

