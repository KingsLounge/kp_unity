using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

public class CSVParser
{

    public static List<JObject> Parse(string[] csvLines)
    {
        return CsvToJson(csvLines).ToList();
    }
    private static IEnumerable<JObject> CsvToJson(IEnumerable<string> csvLines)
    {
        var csvLinesList = csvLines.ToList();

        var header = Split(csvLinesList[0],',');
        for (int i = 1; i < csvLinesList.Count; i++)
        {
            var thisLineSplit = Split(csvLinesList[i],',');
            var pairedWithHeader = header.Zip(thisLineSplit, (h, v) => new KeyValuePair<string, string>(h, v));

            yield return new JObject(pairedWithHeader.Select(j => new JProperty(j.Key, j.Value)));
        }
    }

    private static string[] Split(string str, char t)
    {
        int marking_stack = 0;
        bool marking_open = false;
        string cur_str = "";
        List<string> result = new List<string>();
        for(int i = 0; i < str.Length; i++)
        {
            if (str[i] == '"')
            {
                marking_stack++;
            }
            else
            {
                if(marking_stack == 1)
                {
                    marking_open = !marking_open;
                }
                else if(marking_stack == 3)
                {
                    cur_str += '"';
                }
                marking_stack = 0;
                if(str[i] == t && !marking_open)
                {
                    result.Add(cur_str);
                    cur_str = "";
                }
                else
                {
                    cur_str += str[i];
                }
            }
        }
        return result.ToArray();
    }
}
