using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Jeckl.InfinityScroll;


public class InfinityScrollTest : MonoBehaviour
{
    public int testCount = 8;
    private InfinityScroll scroll;
    private void Start()
    {

        scroll = GetComponent<InfinityScroll>();
        scroll.updateItem = (rectTransform, data) =>
        {
            JObject obj = data as JObject;
            var newSize = rectTransform.sizeDelta;
            newSize.y = (float)obj["height"];
            rectTransform.sizeDelta = newSize;
            string[] colorStr = obj["color"].ToString().Split(',');
            rectTransform.GetComponent<Image>().color = new Color(float.Parse(colorStr[0]), float.Parse(colorStr[1]), float.Parse(colorStr[2]));
        };
    }

    private void UpdateDataArray()
    {
        JArray arr = new JArray();
        for (int i = 0; i < testCount; i++)
        {
            JObject obj = new JObject();
            obj["height"] = Random.Range(50, 150);
            obj["color"] = string.Format("{0},{1},{2}", Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            arr.Add(obj);
        }
        scroll.dataArray = arr;
        //Debug.Log(scroll.dataArray.ToString());
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "test"))
        {
            UpdateDataArray();
        }
    }
}