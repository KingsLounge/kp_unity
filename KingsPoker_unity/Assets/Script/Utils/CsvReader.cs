using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
public class CsvReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };
    public static string[] ReadCSV(string filename)
    {
        TextAsset csv = Resources.Load(string.Format("table/{0}", filename)) as TextAsset;
        string[] lines = Regex.Split(csv.text, LINE_SPLIT_RE);

        for (var i = 1; i < lines.Length; i++)
        {
            lines[i]= lines[i].Replace("<br>", "\n");
        }
        return lines;
    }
}
