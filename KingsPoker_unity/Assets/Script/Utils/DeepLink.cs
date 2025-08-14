using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeepLink : MonoBehaviour
{
    private static string _cafeCode = "";
    private static string _autoJoinCode = "";
    public static string CafeCode {
        get => _cafeCode;
        set
        {
            string before = _cafeCode;
            _cafeCode = value;
            if(before != _cafeCode)
                updated?.Invoke();
        }
    }
    public static string AutoJoinCode
    {
        get => _autoJoinCode;
        set
        {
            string before = _autoJoinCode;
            _autoJoinCode = value;
            if(before != _autoJoinCode)
                updated?.Invoke();
        }
    }

    public static System.Action updated;

    void Awake()
    {
        ImaginationOverflow.UniversalDeepLinking.DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
    }

    private void OnDestroy()
    {
        ImaginationOverflow.UniversalDeepLinking.DeepLinkManager.Instance.LinkActivated -= Instance_LinkActivated;
    }

    private void Instance_LinkActivated(ImaginationOverflow.UniversalDeepLinking.LinkActivation linkActivation)
    {
        string url = linkActivation.Uri;
        string querystring = linkActivation.RawQueryString;

        string cafeCodeParameter = "";
        if(linkActivation.QueryString.ContainsKey("cafeCode"))
            cafeCodeParameter = linkActivation.QueryString["cafeCode"];
        string autoJoinCodeParameter = "";
        if (linkActivation.QueryString.ContainsKey("autoJoinCode"))
            autoJoinCodeParameter = linkActivation.QueryString["autoJoinCode"];

        if (string.IsNullOrEmpty(cafeCodeParameter) == false && string.IsNullOrWhiteSpace(cafeCodeParameter) == false)
        {
            Debug.Log($"{url}\n{querystring}\n{cafeCodeParameter}");
            Debug.Log($"DeepLink CafeCode: {cafeCodeParameter}");
            CafeCode = cafeCodeParameter;
        }
        
        if (string.IsNullOrEmpty(autoJoinCodeParameter) == false && string.IsNullOrWhiteSpace(autoJoinCodeParameter) == false)
        {
            Debug.Log($"{url}\n{querystring}\n{autoJoinCodeParameter}");
            Debug.Log($"DeepLink AutoJoinCode: {autoJoinCodeParameter}");
            AutoJoinCode = autoJoinCodeParameter;
        }
    }
}

