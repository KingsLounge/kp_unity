using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomUiBlindTableModule : CustomUI
{
    public ItemControllerServerCommunication itemController;
    private JArray blindData = new JArray();
    private List<BlindListItem> templates = new List<BlindListItem>();

    private int currentBlindLevel = 0;

    private void Awake()
    {
        itemController.updateItemCallback = (go, data) =>
        {
            BlindListItem t = go.GetComponent<BlindListItem>();

            if (!templates.Contains(t))
            {
                templates.Add(t);
            }

            t.SetBlind(data as JObject, currentBlindLevel);
        };
    }

    public override void SetValue(JToken data)
    {
        blindData = (data as JObject).CastOrEmpty<JArray>("blind_up");
        if (blindData != null)
            SetList(blindData);
    }

    public void SetBlindLevel(int level)
    {
        currentBlindLevel = level + 1;
    }

    private void SetList(JArray arr)
    {
        itemController.dataArray = blindData;
        itemController.Refresh();
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
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
