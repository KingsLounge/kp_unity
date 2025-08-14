using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Esf;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PokerChipText : MonoBehaviour
{
    private long chip = 0;
    private HoldemTableManager tableManager = null;
    private RoomStatus roomData = null;
    private Text chipText = null;
    

    private void Start()
    {
        SetChip(chip);
    }
    private void OnEnable()
    {
        MoneyToString.RemoveBBOptionChangeEvent(SetChipText);
        MoneyToString.RemoveBBChangeEvent(BBChange);

        MoneyToString.AddBBOptionChangeEvent(SetChipText);
        MoneyToString.AddBBChangeEvent(BBChange);
    }

    private void OnDisable()
    {
        MoneyToString.RemoveBBOptionChangeEvent(SetChipText);
        MoneyToString.RemoveBBChangeEvent(BBChange);
    }


    public Color color
    {
        set { chipText.color = value; }
    }

    private void GetRoomData()
    {
        var rn = tableManager.GetRoomNumber();
        var rd = InfoManager.Instance.GetRoom(rn);
        roomData = rd;
    }

    public void Init()
    {
        if(tableManager == null)
        {
            tableManager = transform.GetComponentInParent<HoldemTableManager>();
        }
        if(tableManager != null && roomData == null)
        {
            GetRoomData();
        }
        if(chipText == null)
        {
            chipText = gameObject.GetComponent<Text>();
        }
    }

    public long GetChip()
    {
        return chip;
    }

   
    public void SetChip(long chip)
    {
        
        this.chip = chip;
        SetChipText();
    }

    private void SetChipText()
    {
        Init();
        long bb = 0;
        

        if (roomData != null)
        {
            bb = roomData.bg;   
        }

        
        
        if (chipText != null)
        {
            chipText.text = MoneyToString.ConvertingBB(chip, bb);
        }
    }

    public void BBChange(long rn)
    {
        Init();
        if (roomData != null && rn != roomData.gtn)
            return;
        SetChipText();
    }
}
