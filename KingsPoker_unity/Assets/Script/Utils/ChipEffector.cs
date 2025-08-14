using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipEffector : MonoBehaviour
{
    public GameObject chipContainerPrefab;
    private ObjectPool2<GameObject> pool = new ObjectPool2<GameObject>();
    public delegate void Callback();
    private static ChipEffector instance = null;
    public static ChipEffector Instance
    {
        get
        {
            return instance;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        pool.activator = (go) =>
        {
            go.SetActive(true);
        };
        pool.deactivator = (go) =>
        {
            go.transform.SetParent(transform);
            go.SetActive(false);
        };
        pool.generator = () =>
        {
            GameObject go = Instantiate(chipContainerPrefab);
            go.AddComponent<MagneticMover>();
            return go;
        };
    }

    public void Betting(long chip,Transform starting, Transform target, Callback callback = null)
    {
        GameObject go = pool.GetObject();
        go.transform.SetParent(starting);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        // ChipContainer3D container = go.GetComponent<ChipContainer3D>();
        go.GetComponent<MagneticMover>().MoveStart(target, 0.5f, () =>
        {
            // container.SetChip(chip);
            // container.ResetChip();
            if(callback != null)
            {
                callback();
            }
            pool.ReturnObject(go);
        });
    }

    public void BettingWithSpeed(long chip, Transform starting, Transform target, float secondPerUnit, Callback callback = null)
    {
        GameObject go = pool.GetObject();
        go.transform.SetParent(starting);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        // ChipContainer3D container = go.GetComponent<ChipContainer3D>();
        // container.SetChip(chip);
        go.GetComponent<MagneticMover>().MoveStartWithSpeed(target, secondPerUnit, () =>
        {
            // container.ResetChip();
            if (callback != null)
            {
                callback();
            }
            pool.ReturnObject(go);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
