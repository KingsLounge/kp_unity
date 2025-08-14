using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class Kakao : MonoBehaviour {
	private static Kakao instance = null;
	public static Kakao Instance {
		get {
			return instance;
		}
	}
	private bool _canKakaoLogin = false;
	public bool cankakaoLogin {
		get {
			return _canKakaoLogin;
		} 
	}
	public delegate void OnLoginSuccessDelegate();
	public delegate void OnLoginFailDelegate(string except);
	public delegate void OnLogoutDelegate();
	public delegate void UnLinkSuccessDelegate();
	public delegate void UnLinkFailDelegate(string except);
	private OnLoginSuccessDelegate loginSuccessDelegate;
	private OnLoginFailDelegate loginFailDelegate;
	private OnLogoutDelegate logoutDelegate;
	private UnLinkSuccessDelegate unLinkSuccessDelegate;
	private UnLinkFailDelegate unLinkFailDelegate;
#if UNITY_ANDROID
	AndroidJavaObject kotlin;
#elif UNITY_IOS
	[DllImport("__Internal")]
	private static extern void KakaoLogin_IOS();
	
	[DllImport("__Internal")]
	private static extern void KakaoLogout_IOS();
	[DllImport("__Internal")]
	private static extern void KakaoUnlink_IOS();
	[DllImport("__Internal")]
	private static extern string KakaoGetToken_IOS();
#endif

	private void Awake() {
		Kakao.instance = this;
		GameObject.DontDestroyOnLoad(gameObject);
		_canKakaoLogin = true;
#if UNITY_ANDROID
		//kotlin = new AndroidJavaObject("com.fromthered.kakao.KakaoPlugin");
		//kotlin.Call("Init");
#endif
	}

	public void Login(OnLoginSuccessDelegate successCallback, OnLoginFailDelegate failCallback) {
		if(cankakaoLogin) {
			loginSuccessDelegate += successCallback;
			loginFailDelegate += failCallback;
#if UNITY_ANDROID
			kotlin.Call("Login");
#elif UNITY_IOS
			KakaoLogin_IOS();	
#endif
		}
	}

	public void Logout(OnLogoutDelegate callback) {

		if(cankakaoLogin) {
			logoutDelegate += callback;
#if UNITY_ANDROID
			kotlin.Call("Logout");
#elif UNITY_IOS
			KakaoLogout_IOS();
#endif
		}
	}

	public void UnLink(UnLinkSuccessDelegate successCallback, UnLinkFailDelegate failCallback) {
		if(cankakaoLogin) {
#if UNITY_ANDROID
			kotlin.Call("UnLink");
#elif UNITY_IOS
			KakaoUnlink_IOS();
#endif
		}
	}

	public string GetToken() {

		if(cankakaoLogin) {
#if UNITY_ANDROID
			return kotlin.Call<string>("GetToken");
#elif UNITY_IOS
			return KakaoGetToken_IOS();
#endif
		}
		return null;
	}

	private void OnUnLinkSuccess() {
		if(unLinkSuccessDelegate != null)
			unLinkSuccessDelegate();
		unLinkSuccessDelegate = null;
	}

	private void OnUnLinkFail(string except) {
		if(unLinkFailDelegate != null)
			unLinkFailDelegate(except);
		unLinkFailDelegate = null;
	}

	private void OnLoginSuccess() {
		if(loginSuccessDelegate != null)
			loginSuccessDelegate();
		loginSuccessDelegate = null;
	}

	private void OnLoginFail(string except) {
		if(loginFailDelegate != null)
			loginFailDelegate(except);
		loginFailDelegate = null;
	}

	private void OnLogout() {
		if(logoutDelegate != null)
			logoutDelegate();
		logoutDelegate = null;
	}
}