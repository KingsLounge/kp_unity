using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource effectSource;

    [SerializeField]
    private AudioSource voiceSource;

    private static SoundManager instance;
    public static SoundManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        instance = this;

        //SetBGMMute(PlayerPrefs.GetInt("bgm", 1) == 0);
        //SetEffectMute(PlayerPrefs.GetInt("efsound", 1) == 0);
        //SetVoiceMute(PlayerPrefs.GetInt("voice", 1) == 0);

        //SetBGMMute(true);
        //SetEffectMute(true);
        //SetVoiceMute(true);
        SetAllMute(!bool.Parse(PlayerPrefs.GetString("sound", true.ToString())));
    }

    public void ButtonSound(Sound_TableEnum type = Sound_TableEnum.sfx_button)
    {
        var name = TableDataManager.soundTable[type]["default"].ToObject<string>();
        //        Debug.LogError(string.Format("Sound/{0}",name));
        var clip = Resources.Load(string.Format("Sound/{0}", name)) as AudioClip;

        if (clip != null)
        {
            SoundManager.Instance.PlayEffct(clip);
        }
        else
        {
            Console.Log(string.Format("{0} is notFound", name));
        }
    }

    public void ToggleSound(
        bool isOn,
        Sound_TableEnum onSound = Sound_TableEnum.sfx_toggle,
        Sound_TableEnum offSound = Sound_TableEnum.sfx_toggle
    )
    {
        Sound_TableEnum type = isOn ? onSound : offSound;
        var name = TableDataManager.soundTable[type]["default"].ToObject<string>();
        //        Debug.LogError(string.Format("Sound/{0}",name));
        var clip = Resources.Load(string.Format("Sound/{0}", name)) as AudioClip;

        if (clip != null)
        {
            SoundManager.Instance.PlayEffct(clip);
        }
        else
        {
            Console.Log(string.Format("{0} is notFound", name));
        }
    }

    public void PlayEffectSound(Sound_TableEnum type, bool loop = false)
    {
        var name = TableDataManager.soundTable[type]["default"].ToObject<string>();
        var clip = Resources.Load(string.Format("Sound/{0}", name)) as AudioClip;

        if (clip != null)
        {
            SoundManager.Instance.PlayEffct(clip, loop);
        }
        else
        {
            Console.Log(string.Format("{0} is notFound", name));
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetEffectVolume(float volume)
    {
        effectSource.volume = volume;
    }

    public void SetVoiceVolume(float volume)
    {
        effectSource.volume = volume;
    }

    public void SetBGMMute(bool mute)
    {
        bgmSource.mute = mute;
    }

    public void SetEffectMute(bool mute)
    {
        effectSource.mute = mute;
    }

    public void SetVoiceMute(bool mute)
    {
        voiceSource.mute = mute;
    }

    public void SetAllMute(bool mute, bool save = false)
    {
        SetBGMMute(mute);
        SetEffectMute(mute);
        SetVoiceMute(mute);
        if (save)
        {
            PlayerPrefs.SetString("sound", (!mute).ToString());
        }
    }
    public void ResetAllMute()
    {
        SetAllMute(!bool.Parse(PlayerPrefs.GetString("sound", true.ToString())));
    }

    public void RecoverSoundeOption()
    {
        bool ef = PlayerPrefs.GetInt("efsound", 1) == 1;
        bool vo = PlayerPrefs.GetInt("voice", 1) == 1;
        bool bg = PlayerPrefs.GetInt("bgm", 1) == 1;
        SetBGMMute(!ef);
        SetEffectMute(!vo);
        SetVoiceMute(!bg);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlayBGM(Sound_TableEnum type)
    {
        var name = TableDataManager.soundTable[type]["default"].ToObject<string>();
        if (string.IsNullOrEmpty(name))
        {
            //TODO : 사운드로컬라이징?
            return;
        }

        var filepath = string.Format("Sound/{0}", name).Replace(".mp3", "");
        var clip = Resources.Load(filepath) as AudioClip;
        if (clip != null)
        {
            SoundManager.Instance.PlayBGM(clip);
        }
        else
        {
            Console.Log(string.Format("{0} is notFound", filepath));
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource?.Stop();
        }
    }

    private void PlayEffct(AudioClip clip, bool loop = false)
    {
        if (clip != null)
        {
            if (loop)
            {
                effectSource.clip = clip;
                effectSource.Play();
            }
            else
            {
                effectSource.PlayOneShot(clip);
            }
        }
    }

    public void PlayVoice(AudioClip clip)
    {
        voiceSource.PlayOneShot(clip);
    }

    public void EffctSoundEnd()
    {
        effectSource.Stop();
    }

    public void PlayCharacterVoice(Sound_TableEnum soundType, int charNo)
    {
        charNo = charNo >= 6 ? 5 : charNo; //TODO : 캐릭터 선택 적용 후 제거
        PlayVoiceWithTable(charNo, soundType);
    }

    public void PlayVoiceWithTable(int charNo, Sound_TableEnum soundType)
    {
        charNo = charNo < TableDataManager.characterTable.Count ? charNo : 0;
        if (TableDataManager.characterTable.Count == 0)
        {
            return;
        }
        Console.Log(
            string.Format("Contains {0}: ", charNo)
                + TableDataManager.characterTable.ContainsKey(charNo)
        );
        var pre = TableDataManager.characterTable[charNo]["sound_prefix"].ToObject<string>();
        var name = TableDataManager.soundTable[soundType]["default"].ToObject<string>();
        if (string.IsNullOrEmpty(pre))
        {
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            //TODO : 사운드 로컬라이징
        }
        var filepath = string.Format("Sound/{0}{1}", pre, name).Replace(".mp3", "");
        var clip = Resources.Load(filepath) as AudioClip;
        SoundManager.Instance.PlayVoice(clip);
    }
}
