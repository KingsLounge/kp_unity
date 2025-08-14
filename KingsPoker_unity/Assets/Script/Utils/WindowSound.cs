using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowSound : MonoBehaviour
{
    private void OnEnable() {
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_WINDOW_OPEN);
    }
    private void OnDisable() {
        SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_WINDOW_CLOSE);
    }
}
