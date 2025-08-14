using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIActiveRadio : CustomUIRadio
{
    private JArray activeGroup;

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(data.ContainsKey("active_groups"))
        {
            activeGroup = data["active_groups"] as JArray;
        }
    }

    public override void ChangeActiveWithOtherCustomUI(bool active)
    {
        base.ChangeActiveWithOtherCustomUI(active);
        GroupActiveSetting();
    }

    private void GroupActiveSetting()
    {
        if (activeGroup == null) return;
        List<int> idx = new List<int>();
        for (int i = 0; i < activeTemplates.Count; i++)
        {
            idx.Add(i);
        }
        idx.Sort((a, b) =>
        {
            int ap = activeTemplates[a].toggle.isOn ? 1 : 0;
            int bp = activeTemplates[b].toggle.isOn ? 1 : 0;
            return ap - bp;
        });
        for (int i = 0; i < idx.Count; i++)
        {
            if (activeGroup.Count > idx[i])
            {
                string group = activeGroup[idx[i]].ToString();
                List<CustomUI> uis = root.GetGroup(group);
                for (int j = 0; j < uis.Count; j++)
                {
                    uis[j].gameObject.SetActive(activeTemplates[idx[i]].toggle.isOn);
                }
            }
        }
        for (int i = 0; i < idx.Count; i++)
        {
            if (activeGroup.Count > idx[i])
            {
                string group = activeGroup[idx[i]].ToString();
                List<CustomUI> uis = root.GetGroup(group);
                for (int j = 0; j < uis.Count; j++)
                {
                    uis[j].ChangeActiveWithOtherCustomUI(activeTemplates[idx[i]].toggle.isOn);
                }
            }
        }
    }

    protected override void OnChangeToggle()
    {
        GroupActiveSetting();
        base.OnChangeToggle();
    }
}
