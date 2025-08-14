using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public Queue<SpriteFontPrefab> pool = new Queue<SpriteFontPrefab>();
    public GameObject prefab;
    public Transform poolContainer;
    private int count;
    
    public void Init(int num)
    {
        for(int i = 0; i < num; i++)
        {   
            count++;
            Push(Instantiate(prefab).GetComponent<SpriteFontPrefab>());
        }
    }
    public SpriteFontPrefab Pop()
    {
        SpriteFontPrefab go = null;
        if(pool.Count >0)
        {
            go = pool.Dequeue();
        }
        if(go == null)
        {
            count++;
            go = Instantiate(prefab).GetComponent<SpriteFontPrefab>();
        }
        go.gameObj.SetActive(true);
        return go;
    }

    public void Push(SpriteFontPrefab obj)
    {
        obj.gameObj.SetActive(false);
        pool.Enqueue(obj);
        obj.rect.SetParent(poolContainer);
    }
}
