using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.IO;

public class CSVEnumGenerator : EditorWindow
{
    private static CSVEnumGenerator generatorWindow = null;
    [MenuItem("Assets/GenerateCSVEnum")]
    public static void OpenWindow()
    {
        if (generatorWindow == null)
        {
            generatorWindow = EditorWindow.CreateInstance<CSVEnumGenerator>();
        }
        generatorWindow.Show();
    }

    TextAsset csv = null;
    string key = "";
    private void OnGUI()
    {
        EditorGUILayout.LabelField("CSV");
        csv = EditorGUILayout.ObjectField(csv,typeof(TextAsset)) as TextAsset;
        EditorGUILayout.LabelField("Key");
        key = EditorGUILayout.TextField(key);
        if(GUILayout.Button("Generate Enum"))
        {
            if(csv)
            {
                string path = EditorUtility.SaveFilePanel("Save Enum", "Assets/Scripts/Enums", csv.name + "Enum","cs");
                string[] pathArr = path.Split('/');
                string enumName = pathArr[pathArr.Length - 1].Split('.')[0];
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write( CsvToEnum(AssetDatabase.GetAssetPath(csv), enumName));
                sw.Close();
                fs.Close();
            }
            else
            {
                Debug.LogError("Please select a csv file first.");
            }
        }
    }

    private string CsvToEnum(string path, string name)
    {
        string[] lines = System.IO.File.ReadAllLines(path);
        List<JObject> test = CSVParser.Parse(lines);
        string result = "public enum " + name + " { \n";

        for(int i = 0; i < test.Count; i++)
        {
            result += "\t" + test[i][key] + ",\n";
        }
        result += "}";
        return result;
    }
}
