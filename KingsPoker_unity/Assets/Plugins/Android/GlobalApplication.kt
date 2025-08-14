package com.fromthered.kakao

import android.app.Application
import android.content.Context
import android.content.pm.PackageInfo
import android.content.pm.PackageManager
import android.util.Base64
import android.util.Log
import com.kakao.auth.*
import com.kakao.util.helper.Utility.getPackageInfo
import java.security.MessageDigest
import java.security.NoSuchAlgorithmException


class GlobalApplication : Application() {
    class KakaoSdkAdapter : KakaoAdapter() {
        override fun getApplicationConfig(): IApplicationConfig {
            return IApplicationConfig { globalApplicationContext }
        }

        override fun getSessionConfig(): ISessionConfig {
            return object : ISessionConfig {
                override fun getAuthTypes(): Array<AuthType> {
                    return arrayOf(AuthType.KAKAO_LOGIN_ALL)
                }

                override fun isUsingWebviewTimer(): Boolean {
                    return false
                }

                override fun isSecureMode(): Boolean {
                    return false
                }

                override fun getApprovalType(): ApprovalType? {
                    return ApprovalType.INDIVIDUAL
                }

                override fun isSaveFormData(): Boolean {
                    return true
                }
            }
        }
    }

    override fun onCreate() {
        super.onCreate()
        instance = this;
        KakaoSDK.init(KakaoSdkAdapter())
        Log.w("com.fromthered.kakao","KeyHash : " + getKeyHash(globalApplicationContext))
    }

    fun getKeyHash(context: Context?): String? {
        val packageInfo: PackageInfo = getPackageInfo(context, PackageManager.GET_SIGNATURES)
                ?: return null
        for (signature in packageInfo.signatures) {
            try {
                val md = MessageDigest.getInstance("SHA")
                md.update(signature.toByteArray())
                return Base64.encodeToString(md.digest(), Base64.NO_WRAP)
            } catch (e: NoSuchAlgorithmException) {
                Log.w("com.fromthered.kakao", "Unable to get MessageDigest. signature=$signature", e)
            }
        }
        return null
    }

    companion object{
        private var instance : GlobalApplication? = null
        val globalApplicationContext:GlobalApplication
            get(){
                if(instance == null)
                    throw IllegalStateException("wow")

                return instance!!
            }
    }
}