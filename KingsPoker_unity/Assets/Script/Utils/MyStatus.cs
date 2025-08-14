using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MyStatus
{
    public static string gid = "";
    public static string uid = null;
    public static string nick = "";
    public static int icon_no = 0;
    public static string photoURL = "";
    public static bool usePhotoURL = false;
    public static long zc = 0; //유료
    public static long dc = 0; //무료칩
    public static long cc = 0; //카페칩
    public static long safeRu = 0;
    public static long safeSc = 0;
    public static long safeGc = 0;

    public static int status = 0;
    public static JObject push = null;

    //public static int seat = -1;
    public static string token = "";
    public static int type = -1; //0은 일반유저 1은 딜러(BJ)
    public static string password = "";
    public static bool loginSuccess = false;
    public static List<TnmtData> tournament = new List<TnmtData>();
    public static int w_rank = 0;
    public static int d_rank = 0;
    public static int w_exp = 0;
    public static int d_exp = 0;
    public static int loginContinued = 0;
    public static bool isLoginRewarded = false;
    public static List<Eliminate> eliminates = new List<Eliminate>();

    public static int limittype;
    public static JObject limitData = new JObject();
    public static long lossLimit;
    public static long[] lossLimits;
    public static JObject request = new JObject();
    public static JObject loungeData = null;

    public static int vipRate = 0;
    public static int VipRate
    {
        get { return vipRate; }
        set
        {
            if (vipRate != value)
            {
                vipRate = value;
                vipChange?.Invoke();
            }
        }
    }
    public delegate void ChangeValue();
    public static ChangeValue vipChange;

    public static void Init()
    {
        gid = "";
        uid = "";
        nick = "";
        photoURL = "";
        zc = 0;
        dc = 0;
        safeRu = 0;
        safeGc = 0;
        safeSc = 0;
        status = 0;
        //seat = -1;
        token = "";
        type = -1;
        password = "";
        tournament.Clear();
        w_rank = 0;
        d_rank = 0;
        w_exp = 0;
        d_exp = 0;
        loginContinued = 0;
        isLoginRewarded = false;
    }

    public static void ClearTnmt()
    {
        tournament.Clear();
    }

    public static TnmtData AddTnmt(int tn)
    {
        TnmtData data = GetTnmt(tn);
        if (data == null)
        {
            data = new TnmtData();
            data.tn = tn;
            tournament.Add(data);
        }
        return data;
    }

    public static TnmtData GetTnmt(int tn)
    {
        for (int i = 0; i < tournament.Count; i++)
        {
            if (tournament[i].tn == tn)
                return tournament[i];
        }
        return null;
    }

    public static void RemoveTnmt(int tn)
    {
        for (int i = tournament.Count - 1; i >= 0; i--)
        {
            if (tournament[i].tn == tn)
                tournament.RemoveAt(i);
        }
    }

    public static void RemoveAllTnmt()
    {
        tournament.Clear();
    }

    public static bool TnmtApplyInspection(int tn)
    {
        return tournament.FindIndex(
                delegate(TnmtData data)
                {
                    return data.tn == tn;
                }
            ) != -1;
    }

    public static List<TnmtData> GetNotEnterTnmt()
    {
        var result = new List<TnmtData>();

        for (int i = 0; i < tournament.Count; ++i)
        {
            var data = tournament[i];

            //var idx = GamesManager.instance.GetTableIdx(data.gtn);
            var enterd = InfoManager.Instance.IsEnteredRoom(data.gtn);
            //var rd = InfoManager.Instance.GetRoom(data.gtn);
            if (!enterd && data.state < TNMT_FLOW.end)
            {
                result.Add(data);
            }
        }
        result.Sort(
            (a, b) =>
            {
                return DateTime.Compare(a.startTime, b.startTime);
            }
        );

        return result;
    }

    public static TnmtData GetFirstTounament()
    {
        TnmtData data = null;
        RoomStatus rd = null;
        for (int i = 0; i < tournament.Count; ++i) { }
        if (tournament.Count > 0)
        {
            data = tournament[0];
            rd = InfoManager.Instance.GetRoom(data.gtn);
        }
        for (int i = 2; i < tournament.Count; ++i)
        {
            if (data.startTime < tournament[i].startTime)
            {
                data = tournament[i];
            }
        }
        return data;
    }

    public static void GetUserInfo()
    {
        var packet = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);
        WebSocketManager.defaultCli.Send(packet.ToJson());
        InfoManager.Instance.GetMyItems();
    }
}

public class Eliminate
{
    public int tn;
    public int rank;
}

public class TnmtData
{
    public int tn;
    public int gtn;
    public int cafeIdx;
    public TNMT_FLOW state = TNMT_FLOW.none;
    public DateTime startTime;
    public DateTime enterTime;
}
