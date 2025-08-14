using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModelInfo
{
    public Character_TableEnum charactor = Character_TableEnum.char0;
    public GameObject prefab;
    public float throwSpeed = 2f;
    public float throwDelay = 0f;
}

[CreateAssetMenu(fileName ="NewModelData", menuName = "ModelData")]
public class ModelData : ScriptableObject
{
    public Character_TableEnum defaultCharactor = Character_TableEnum.char0;
    [Space]
    public List<ModelInfo> modelInfos = new List<ModelInfo>();

    public GameObject GetCharactorPrefab(Character_TableEnum charactor)
    {
        ModelInfo info = modelInfos.Find((value) => value.charactor == charactor);
        if(info != null)
        {
            return info.prefab;
        }
        info = modelInfos.Find((value) => value.charactor == defaultCharactor);
        if (info != null)
        {
            return info.prefab;
        }
        return null;
    }

    public float GetCharactorThrowSpeed(Character_TableEnum charactor)
    {
        ModelInfo info = modelInfos.Find((value) => value.charactor == charactor);
        if (info != null)
        {
            return info.throwSpeed;
        }
        info = modelInfos.Find((value) => value.charactor == defaultCharactor);
        if (info != null)
        {
            return info.throwSpeed;
        }
        return 0f;
    }

    public float GetCharactorThrowDelay(Character_TableEnum charactor)
    {
        ModelInfo info = modelInfos.Find((value) => value.charactor == charactor);
        if (info != null)
        {
            return info.throwDelay;
        }
        info = modelInfos.Find((value) => value.charactor == defaultCharactor);
        if (info != null)
        {
            return info.throwDelay;
        }
        return 0f;
    }
}
