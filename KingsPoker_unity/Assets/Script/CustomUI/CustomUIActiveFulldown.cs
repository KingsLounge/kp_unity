using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIActiveFulldown : CustomUIFulldown
{
    private JArray activeGroups;

    public override void SetUI(JObject data)
    {
        base.SetUI(data);
        if(data.ContainsKey("active_groups"))
        {
            activeGroups = data["active_groups"] as JArray;
        }
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        GroupActiveSetting();
    }

    protected override void OnChangeValue()
    {
        base.OnChangeValue();
        GroupActiveSetting();
    }

    public void ChangeOptions(JArray optionArr, JArray activeGroups)
    {
        base.ChangeOptions(optionArr);
        if(this.activeGroups != null)
        {
            for (int i = 0; i < this.activeGroups.Count; i++)
            {
                List<CustomUI> group = root.GetGroup(this.activeGroups[i].ToString());
                for (int j = 0; j < group.Count; j++)
                {
                    group[j].gameObject.SetActive(false);
                }
            }
        }
        this.activeGroups = activeGroups;
        GroupActiveSetting();
    }

    public override void ChangeActiveWithOtherCustomUI(bool active)
    {
        base.ChangeActiveWithOtherCustomUI(active);
        GroupActiveSetting();
    }

    private void GroupActiveSetting()
    {
        if (activeGroups != null)
        {
            for (int i = 0; i < activeGroups.Count; i++)
            {
                List<CustomUI> group = root.GetGroup(activeGroups[i].ToString());
                for (int j = 0; j < group.Count; j++)
                {
                    group[j].gameObject.SetActive(i == dropdown.value);
                }
            }
            for (int i = 0; i < activeGroups.Count; i++)
            {
                List<CustomUI> group = root.GetGroup(activeGroups[i].ToString());
                for (int j = 0; j < group.Count; j++)
                {
                    group[j].ChangeActiveWithOtherCustomUI(i == dropdown.value);
                }
            }
        }
    }
}
