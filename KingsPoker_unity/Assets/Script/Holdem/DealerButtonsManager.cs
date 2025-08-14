using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerButtonsManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> dealerButtons;

    private int mySeat = 0;

    public void SetMySeat(int mySeat)
    {
        this.mySeat = mySeat;
    }

    public void SetBoss(int bossSeat)
    {
        bossSeat = bossSeat - mySeat;
        if (bossSeat < 0)
        {
            bossSeat += dealerButtons.Count;
        }
        for (int i = 0; i < dealerButtons.Count; ++i)
        {
            var button = dealerButtons[i];
            if (button)
            {
                button.SetActive(i == bossSeat);
            }
        }
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
