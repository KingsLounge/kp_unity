using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum POKER_RESERVE_TYPE
{
    NONE, FOLD, CALL, CHECK, BET, RAISE, FOLDCHECK, CHECKCALL, BETRAISE, BBWAIT
}

[System.Serializable]
public class ReserveBettingToggleData
{
    public POKER_RESERVE_TYPE bettype;
    public Toggle toggle;
}

public class ReserveBettingToggles : MonoBehaviour
{
    public GameObject toggles;
    public List<ReserveBettingToggleData> reserveToggles;
    public Dictionary<POKER_RESERVE_TYPE, Toggle> reserveToggleDic = new Dictionary<POKER_RESERVE_TYPE, Toggle>();
    [SerializeField]
    private ReserveBettingToggleData[] currentToggleData = new ReserveBettingToggleData[3];

    public POKER_RESERVE_TYPE currentReserveType = POKER_RESERVE_TYPE.NONE;
    public bool alwaysFold = false;

    public bool foldCheck = false, fold = false, check = false, call = false, bet = false, raise = false;
    private void Awake()
    {
        for (int i = 0; i < reserveToggles.Count; i++)
        {
            reserveToggleDic.Add(reserveToggles[i].bettype, reserveToggles[i].toggle);
        }
    }

    /// <summary>
    /// 예약 베팅 토글들을 설정합니다.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="type"></param>
    private void SetCurrentToggleData(int index, POKER_RESERVE_TYPE type)
    {
        if(type == POKER_RESERVE_TYPE.NONE)
        {
            if (currentToggleData[index].toggle != null)
            {
                currentToggleData[index].toggle.isOn = false;
                currentToggleData[index].toggle.gameObject.SetActive(false);
            }
            return;
        }

        currentToggleData[index].bettype = type;
        if(currentToggleData[index].bettype != type)
        {
            currentToggleData[index].toggle.isOn = false;
        }
        reserveToggleDic.TryGetValue(type, out currentToggleData[index].toggle);
        currentToggleData[index].toggle.gameObject.SetActive(true);
    }

    /// <summary>
    /// 예약 베팅 토글들을 초기화합니다.
    /// </summary>
    public void Init()
    {
        for (int i = 0; i < reserveToggles.Count; i++)
        {
            reserveToggles[i].toggle.isOn = false;
            reserveToggles[i].toggle.gameObject.SetActive(false);
        }

        for (int i = 0; i < currentToggleData.Length; i++)
        {
            currentToggleData[i].bettype = POKER_RESERVE_TYPE.NONE;
            currentToggleData[i].toggle = null;
        }

        foldCheck = false;
        fold = false;
        check = false;
        call = false;
        bet = false;
        raise = false;

        currentReserveType = POKER_RESERVE_TYPE.NONE;
    }

    public void ReserveToggleChange(POKER_RESERVE_TYPE type, bool isOn)
    {
        switch(type)
        {
            case POKER_RESERVE_TYPE.FOLDCHECK:
                foldCheck = isOn;
                break;
            case POKER_RESERVE_TYPE.FOLD:
                fold = isOn;
                break;
            case POKER_RESERVE_TYPE.CHECK:
                check = isOn;
                break;
            case POKER_RESERVE_TYPE.CALL:
                call = isOn;
                break;
            case POKER_RESERVE_TYPE.RAISE:
                raise = isOn;
                break;
            case POKER_RESERVE_TYPE.BET:
                bet = isOn;
                break;
        }
    }

    public void SetReserveToggles(POKER_RESERVE_TYPE type1 = POKER_RESERVE_TYPE.NONE, POKER_RESERVE_TYPE type2 = POKER_RESERVE_TYPE.NONE, POKER_RESERVE_TYPE type3 = POKER_RESERVE_TYPE.NONE)
    {
        for(int i =0; i < currentToggleData.Length; i++)
        {
            if(currentToggleData[i].toggle != null)
            {
                currentToggleData[i].toggle.gameObject.SetActive(false);
            }
        }
        CheckBetType(type1);
        CheckBetType(type2);
        CheckBetType(type3);
        SetCurrentToggleData(0, type1);
        SetCurrentToggleData(1, type2);
        SetCurrentToggleData(2, type3);
        
    }
    public void SetToggleOn(ReserveBettingToggleData data)
    {
        switch (data.bettype)
        {
            case POKER_RESERVE_TYPE.FOLDCHECK:
                data.toggle.isOn = foldCheck;
                break;
            case POKER_RESERVE_TYPE.FOLD:
                data.toggle.isOn = fold;
                break;
            case POKER_RESERVE_TYPE.CHECK:
                data.toggle.isOn = check;
                break;
            case POKER_RESERVE_TYPE.CALL:
                data.toggle.isOn = call;
                break;
            case POKER_RESERVE_TYPE.RAISE:
                data.toggle.isOn = raise;
                break;
            case POKER_RESERVE_TYPE.BET:
                data.toggle.isOn = bet;
                break;
        }
    }

    public void CheckBetType(POKER_RESERVE_TYPE type)
    {
        switch (type)
        {
            case POKER_RESERVE_TYPE.FOLDCHECK:
                break;
            case POKER_RESERVE_TYPE.FOLD:
                if(foldCheck)
                {
                    fold = true;
                    foldCheck = false;
                }
                break;
            case POKER_RESERVE_TYPE.CHECK:
                break;
            case POKER_RESERVE_TYPE.CALL:
                break;
            case POKER_RESERVE_TYPE.RAISE:
                if(bet)
                {
                    bet = false;
                    raise = true;
                }
                break;
            case POKER_RESERVE_TYPE.BET:
                break;
        }
    }
    
}
