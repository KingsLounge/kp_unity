using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIActiveToggle : CustomUIToggle
{
    private string true_group = "";
    private string false_group = "";

    public override void SetUI(JObject data)
    {
        base.SetUI(data);

        if (data.ContainsKey("true_group"))
        {
            true_group = data["true_group"].ToString();
        }
        else
        {
            true_group = null;
        }
        if (data.ContainsKey("false_group"))
        {
            false_group = data["false_group"].ToString();
        }
        else
        {
            false_group = null;
        }
    }

    protected override void OnChangedToggle(bool check)
    {
        base.OnChangedToggle(check);
        SetGroupActive();
    }

    public override void ChangeActiveWithOtherCustomUI(bool active)
    {
        base.ChangeActiveWithOtherCustomUI(active);
        SetGroupActive();
    }

    private void SetGroupActive()
    {
        List<CustomUI> true_groups = null;
        List<CustomUI> false_groups = null;
        if(true_group != "")
            true_groups = root.GetGroup(true_group);
        if (false_group != "")
            false_groups = root.GetGroup(false_group);
        if(toggle.isOn)
        {
            if(true_groups != null)
            {
                for(int i = 0; i < true_groups.Count; i++)
                {
                    true_groups[i].gameObject.SetActive(true);
                }
            }
            if (false_groups != null)
            {
                for (int i = 0; i < false_groups.Count; i++)
                {
                    false_groups[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (true_groups != null)
            {
                for (int i = 0; i < true_groups.Count; i++)
                {
                    true_groups[i].gameObject.SetActive(false);
                }
            }
            if (false_groups != null)
            {
                for (int i = 0; i < false_groups.Count; i++)
                {
                    false_groups[i].gameObject.SetActive(true);
                }
            }
            if (true_groups != null)
            {
                for (int i = 0; i < true_groups.Count; i++)
                {
                    true_groups[i].ChangeActiveWithOtherCustomUI(false);
                }
            }
            if (false_groups != null)
            {
                for (int i = 0; i < false_groups.Count; i++)
                {
                    false_groups[i].ChangeActiveWithOtherCustomUI(true);
                }
            }
        }
    }

    public override void CompleteSetting()
    {
        base.CompleteSetting();
        SetGroupActive();
    }
}
