using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS || UNITY_ANDROID
// Firebase SDK 참조
using Firebase;
#endif
public class FCM : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    FirebaseApp app;
#endif
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {

#if UNITY_IOS || UNITY_ANDROID
        // Firebase 클라우드 메시징 초기화
        // https://firebase.google.com/docs/cloud-messaging/unity/client?hl=ko#initialize
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

        Debug.Log("구글 체크");

        // GooglePlay 서비스 버전 요구사항 확인
        // https://firebase.google.com/docs/cloud-messaging/unity/client?hl=ko#confirm_google_play_version

        Firebase.DependencyStatus ds = Firebase.FirebaseApp.CheckAndFixDependenciesAsync().Result;
    

        if(ds == Firebase.DependencyStatus.Available)
        {
            app = Firebase.FirebaseApp.DefaultInstance;
            Debug.Log("파이어 베이스 앱 초기화 완료");
        }
        else
        {
            UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", ds));
        }

        Debug.Log("구글 체크 끝");

        Firebase.Messaging.FirebaseMessaging.SubscribeAsync("all");

        Debug.Log("all 구독 완료");
#endif
    }

    // Update is called once per frame
    void Update()
    {
    }
#if UNITY_IOS || UNITY_ANDROID
    // 토큰을 수신하며 차후 토큰을 사용하도록 캐시한다.
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    // 메시지를 수신한다.
    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        var notification = e.Message.Notification;
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
#endif
}
