using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartingDisableTiming
{
    Awake,
    Start
}
public class StartingDisabler : MonoBehaviour
{
    public StartingDisableTiming timing = StartingDisableTiming.Awake;
    // Start is called before the first frame update

    private void Awake()
    {
        if (timing == StartingDisableTiming.Awake)
        {
            gameObject.SetActive(false);
        }
    }
    void Start()
    {
        if(timing == StartingDisableTiming.Start)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
