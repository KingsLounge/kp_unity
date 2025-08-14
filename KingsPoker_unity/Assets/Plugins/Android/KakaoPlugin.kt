package com.fromthered.kakao

import android.content.Intent
import android.os.Bundle
import android.util.Log
import com.kakao.auth.*
import com.kakao.auth.network.response.AccessTokenInfoResponse
import com.kakao.network.ErrorResult
import com.kakao.usermgmt.UserManagement
import com.kakao.usermgmt.callback.LogoutResponseCallback
import com.kakao.usermgmt.callback.UnLinkResponseCallback
import com.kakao.util.exception.KakaoException
import com.unity3d.player.UnityPlayer
import com.unity3d.player.UnityPlayerActivity
import com.google.firebase.MessagingUnityPlayerActivity


class KakaoPlugin : MessagingUnityPlayerActivity() {

    var callback : SessionCallback? = null

    override fun onCreate(p0: Bundle?) {
        super.onCreate(p0)
    }

    fun Init() {

        callback = SessionCallback()
        Session.getCurrentSession().addCallback(callback)
    }

    fun AutoLogin(): Boolean {
        Session.getCurrentSession().checkAndImplicitOpen()
        return Session.getCurrentSession().isOpened;
    }

    fun Login() {
        Session.getCurrentSession().open(AuthType.KAKAO_LOGIN_ALL,UnityPlayer.currentActivity)
    }

    fun Logout() {
        UserManagement.getInstance()
                .requestLogout(object : LogoutResponseCallback() {
                    override fun onCompleteLogout() {
                        SendToUnity("OnLogout","")
                    }
                })
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {

        // 카카오톡|스토리 간편로그인 실행 결과를 받아서 SDK로 전달
        Log.w("com.fromthered.kakao",requestCode.toString() + "," + resultCode.toString() + "," + data.toString());
        if (Session.getCurrentSession().handleActivityResult(requestCode, resultCode, data)) {
            return
        }
        super.onActivityResult(requestCode, resultCode, data)
    }

    fun UnLink() {
        UserManagement.getInstance().requestUnlink(object : UnLinkResponseCallback() {
            override fun onSuccess(result: Long?) {
                Log.d("com.fromthered.kakao","탈퇴처리")
                SendToUnity("OnUnLinkSuccess","");
            }

            override fun onSessionClosed(errorResult: ErrorResult?) {
                Log.d("com.fromthered.kakao","탈퇴실패")
                SendToUnity("OnUnLinkFail",errorResult.toString());
                Log.d("com.fromthered.kakao",errorResult.toString())
            }
        })
    }

    override fun onDestroy() {
        super.onDestroy()
        // 세션 콜백 삭제
        Session.getCurrentSession().removeCallback(callback)
    }

    inner class SessionCallback : ISessionCallback {

        override fun onSessionOpened() {
            redirectSignupActivity()
        }

        override fun onSessionOpenFailed(exception: KakaoException?) {
            Log.w("com.fromthered.kakao",exception);
            SendToUnity("OnLoginFail", exception.toString())
        }
    }

    protected fun redirectSignupActivity() {
        SendToUnity("OnLoginSuccess", "")
    }

    private fun SendToUnity(func:String,msg:String) {
        UnityPlayer.UnitySendMessage("KakaoManager",func, msg)
    }

    public fun GetToken():String {
        val accessToken = Session.getCurrentSession().tokenInfo.accessToken;
//        AuthService.getInstance()
//                .requestAccessTokenInfo(object : ApiResponseCallback<AccessTokenInfoResponse?>() {
//                    override fun onSessionClosed(errorResult: ErrorResult) {
//                        SendToUnity("GetTokenFail",errorResult.toString())
//                    }
//
//                    override fun onFailure(errorResult: ErrorResult) {
//                        SendToUnity("GetTokenFail",errorResult.toString())
//                    }
//
//                    override fun onSuccess(result: AccessTokenInfoResponse?) {
//                        SendToUnity("GetTokenSuccess",result.toString())
//                    }
//                })
        Log.w("com.fromthered.kakao",accessToken)
        SendToUnity("GetTokenSuccess",accessToken)
        return accessToken
    }
}