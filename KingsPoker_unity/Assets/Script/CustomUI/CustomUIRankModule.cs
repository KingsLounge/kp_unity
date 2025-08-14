using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUIRankModule : CustomUI
{
    public ItemControllerServerCommunication itemController;
    private JArray rankData = new JArray();
    private List<RankListItem> templates = new List<RankListItem>();

    private void Awake()
    {
        itemController.updateItemCallback = (go, data) =>
        {
            RankListItem t = go.GetComponent<RankListItem>();
            if (!templates.Contains(t))
                templates.Add(t);
            t.SetRankItem(data as JObject);
        };
    }

    public override void SetValue(JToken data)
    {
        var d = data as JObject;
        var cafeIdx = d.ValueOrDefault("cafeIdx", 0);
        rankData = (data as JObject).CastOrEmpty<JArray>("list");
        foreach (var item in rankData)
        {
            if (item is JObject obj)
            {
                obj["cafeIdx"] = cafeIdx;
            }
        }
        SetList(rankData);
        if (rankData != null)
            SetList(rankData);
    }

    private void SetList(JArray arr)
    {
        itemController.dataArray = rankData;
        itemController.Refresh();
    }

    private void OnEnable()
    {
        StartCoroutine(ReSize());
    }

    private IEnumerator ReSize()
    {
        yield return new WaitForEndOfFrame();

        var rect = (transform as RectTransform);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, root.viewportSize.y);
    }
}
