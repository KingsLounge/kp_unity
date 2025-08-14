using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

public class AddZuicePannel : MonoBehaviour
{
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    public ItemControllerServerCommunication itemController;
    public TextAsset csvTable;

    public void Awake()
    {
        itemController.updateItemCallback = (self, data) =>
        {
            self.GetComponent<AddZuiceItemSlot>().SetData(data as JObject);
        };
    }

    private void OnEnable()
    {
        Init();
    }
    private void Init()
    {
        string[] lines = Regex.Split(csvTable.text, LINE_SPLIT_RE);
        JArray dataArray = new JArray(CSVParser.Parse(lines).ToArray());
        for(int i = dataArray.Count - 1; i >= 0; i--)
        {
            JObject data = dataArray[i] as JObject;
            string tierStr = data.ValueOrDefault("tier", "");
            if (string.IsNullOrEmpty(tierStr)) continue;

            string[] tier = tierStr.Split(',');
            bool remove = true;

            for (int j = 0; j < tier.Length; j++)
            {
                if (tier[j].ToString() == Cafe.instance.tier.ToString())
                    remove = false;
            }

            if (remove)
            {
                dataArray.RemoveAt(i);
            }
        }
        itemController.dataArray = dataArray;
        itemController.Refresh();
        itemController.SetScrollOnTop();
    }
}
