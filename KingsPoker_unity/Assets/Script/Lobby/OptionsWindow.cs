using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsWindow : MonoBehaviour
{
    public OptionToggle efSound, voice, openprofile, jockbo, bgm, vibe, activeopen, push;
    public Text idText, nickText, emailText, versionText;
    private void Awake() {
        Init();
    }
    private void Init()
    {
        efSound.t = EffectSound;
        efSound.toggle.isOn = PlayerPrefs.GetInt("efsound", 1) == 1;
        
        voice.t = Voice;
        voice.toggle.isOn = PlayerPrefs.GetInt("voice", 1) == 1;
        
        openprofile.t = OpenProfile;
        openprofile.toggle.isOn = PlayerPrefs.GetInt("profile", 1) == 1;
        
        jockbo.t = Jockbo;
        jockbo.toggle.isOn = PlayerPrefs.GetInt("jockbo", 1) == 1;
        
        bgm.t = BGMSound;
        bgm.toggle.isOn = PlayerPrefs.GetInt("bgm", 1) == 1;
        
        vibe.t = Vibe;
        vibe.toggle.isOn = PlayerPrefs.GetInt("vibe", 1) == 1;
        
        activeopen.t = ActiveOpen;
        activeopen.toggle.isOn = PlayerPrefs.GetInt("activeopen", 1) == 1;
        
        push.t = Push;
        push.toggle.isOn = PlayerPrefs.GetInt("push", 1) == 1;
#if UNITY_IOS || UNITY_ANDROID
        try
        {
            emailText.text = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email;
        }
        catch
        {
            emailText.text = "테스트계정이라 이메일 없음.";
        }
#endif

        nickText.text = MyStatus.nick;
        idText.text = MyStatus.uid;
        versionText.text = Application.version;
        
    }
    public void EffectSound(bool sound)
    {
        PlayerPrefs.SetInt("efsound",sound ? 1 :0);
        SoundManager.Instance.SetEffectMute(!sound);
    }
    public void Voice(bool v)
    {
        PlayerPrefs.SetInt("voice", v ? 1: 0);
        SoundManager.Instance.SetVoiceMute(!v);
    }

    public void OpenProfile(bool p)
    {
        PlayerPrefs.SetInt("profile", p ? 1 : 0);
    }

    public void Jockbo(bool j)
    {
        PlayerPrefs.SetInt("jockbo", j ? 1: 0);
    }

    public void BGMSound(bool b)
    {
        PlayerPrefs.SetInt("bgm", b ? 1 : 0);
        SoundManager.Instance.SetBGMMute(!b);
    }

    public void Vibe(bool v)
    {
        PlayerPrefs.SetInt("vibe", v? 1: 0);
    }

    public void ActiveOpen(bool a)
    {
        PlayerPrefs.SetInt("activeopen", a ? 1 : 0);
    }

    public void Push(bool p)
    {
        PlayerPrefs.SetInt("push", p ? 1:0);
#if UNITY_IOS || UNITY_ANDROID
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = p;
#endif
    }
}
