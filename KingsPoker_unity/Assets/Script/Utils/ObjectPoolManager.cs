using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager instance;
    public static ObjectPoolManager Instance{get{return instance;}}
    public ObjectPool spritfontPool;
    private void Awake()
    {
        instance = this;

        spritfontPool.Init(0);
    }
}
