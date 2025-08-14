using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebViewManager
{
    private static UniWebView webView;
    
    public  static UniWebView CreateWebView()
    {
        if(webView == null)
        {
            webView = new GameObject("Uniwebview").AddComponent<UniWebView>();
        }
        return webView;
    }
}
