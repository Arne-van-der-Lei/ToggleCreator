using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class language
{
    public static string currentLang = "EN_US";

    public static Dictionary<string, string> Lines;

    public static void loadFile()
    {
        TextAsset asset = (TextAsset)AssetDatabase.LoadAssetAtPath("Packages/com.talox.togglecreator/Editor/Lang/" + currentLang + ".txt",typeof(TextAsset));
        string[] lines = asset.text.Split('\n');

        Lines = new Dictionary<string, string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            string[] line = lines[i].Split('=');
            if(line.Length > 1)
            Lines.Add(line[0],line[1]);
        }
    }

    public static string getString(string name) {
        if(Lines == null)
        {
            loadFile();
        }

        if (Lines.ContainsKey(name))
        {
            return Lines[name];
        }
        return name;
    }
}
