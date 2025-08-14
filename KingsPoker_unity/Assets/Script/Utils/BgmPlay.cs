using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmPlay : MonoBehaviour
{
    public Sound_TableEnum bgmEnumType;
    void Start()
    {
        SoundManager.Instance.PlayBGM(bgmEnumType);
    }

    private void OnDestroy() {
        SoundManager.Instance.StopBGM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
