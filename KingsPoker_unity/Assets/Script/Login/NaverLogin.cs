using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NaverLogin : MonoBehaviour
{
    // Start is called before the first frame update  package com.nhn.android.naverlogin;
    private static NaverLogin instance;
    public static NaverLogin Instance {
        get
        {
            return instance;
        }
    }

#if UNITY_ANDROID
    private AndroidJavaClass naver = null;
    private AndroidJavaObject activity = null;
    private AndroidJavaClass javaClass = null;
    private AndroidJavaObject naverInstance = null;
    private AndroidJavaObject context;
#elif UNITY_IOS
    [DllImport("__Internal")]
    private static extern void NaverInit();
    [DllImport("__Internal")]
    private static extern void RequestThirdPartyLogin();
    [DllImport("__Internal")]
    private static extern void ReQuestDeleteToken();
    [DllImport("__Internal")]
    private static extern void RequestThirdPartyLoginWithDatas(string  key  , string secret, string url, string name);
#endif

    private string scheme = "naveroauthlogin";
    public delegate void loginDelegate(string message);
    private loginDelegate loginSuccess;
    private loginDelegate loginFaild;
    void Start()
    {
        instance = this;
    }

    public void Login(loginDelegate success, loginDelegate faild)
    {
        loginSuccess = success;
        loginFaild = faild;
#if UNITY_ANDROID
       using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
       {
           activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
           context = activity.Call<AndroidJavaObject>("getApplicationContext");
       }
        Console.Log("Login??");
       using(naver = new AndroidJavaClass("com.ftred.naveridlogin.Login"))
       {
            if(naver != null)
            {   
                try
                {
                   
                    naverInstance = naver.CallStatic<AndroidJavaObject>("GetInstance");
                    naverInstance.Call("SetContext", activity);
                    naverInstance.Call("SetClient", LinkOptionConstant.naverClientId, LinkOptionConstant.naverClientSecret, LinkOptionConstant.naverClientName);
                    naverInstance.Call("InitLogin");
               
                }
                catch(System.Exception e)
                {
                    Console.Log(string.Format("Naver Login Fail : {0}", e.Message));
                }
                
            }
       }
#elif UNITY_IOS
        NaverInit();
         RequestThirdPartyLoginWithDatas(LinkOptionConstant.naverClientId, LinkOptionConstant.naverClientSecret, LinkOptionConstant.naverClientName, scheme);
#endif
    }
    public void Log(string log)
    {
        Console.Log(log);
    }

    public void OnLoginSuccess(string token)
    {
        Console.Error(token);
        loginSuccess(token);
    }

    public void OnLoginFaild(string error)
    {
        Console.Error(error);
        loginFaild(error);
    }

    public void Logout()
    {
#if UNITY_ANDROID
        naverInstance.Call("LogOut");
#elif UNITY_IOS
#else
#endif
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
