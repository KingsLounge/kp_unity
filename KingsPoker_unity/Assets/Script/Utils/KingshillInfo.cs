using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;

public static class KingshillInfo
{
    private static JObject agentData;

    private static JObject ticketData;

    private static JArray ticketPriceData;

    public static async UniTask<JObject> GetAgentData()
    {
        if (agentData == null)
        {
            await GetKingshillInfo();
        }
        return agentData;
    }

    public static async UniTask<JObject> GetTicketData()
    {
        if (ticketData == null)
        {
            await GetKingshillInfo();
        }
        return ticketData;
    }

    public static async UniTask<JArray> GetTicketPriceData()
    {
        if (ticketPriceData == null)
        {
            await GetKingshillInfo();
        }
        return ticketPriceData;
    }

    public static async UniTask<List<string>> GetTicketList()
    {
        var ticketData = await GetTicketData();
        var keys = new List<string>();
        foreach(var property in ticketData.Properties())
        {
            keys.Add(property.Name);
        }
        return keys;    
    }
    public static async UniTask<string> GetTicketString(int t_type)
    {
        await GetTicketData();
        var ticketString = "ticket";
        if (t_type != 1)
        {
            ticketString += t_type.ToString();
        }
        return ticketData.ValueOrDefault(ticketString, "ticket");
    }

    public static async UniTask<string> GetTicketString(string ticket)
    {
        await GetTicketData();
        return ticketData.ValueOrDefault(ticket, "ticket");
    }

    public static int GetTicketGbn(string ticket)
    {
        if(string.IsNullOrEmpty(ticket))
        {
            return 0;
        }
        ticket = ticket.Replace("ticket", "");
        if (ticket == "")
        {
            return 1;
        }
        if(int.TryParse(ticket, out int result))
        {
            return result;
        }
        return 0;
    }

    public static async UniTask GetKingshillInfo()
    {
        WWWForm form = new WWWForm();
        string url = $"{Constant.GameConfig.KingshillUrl}/api/getAgentList";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Web Error : " + www.error);
            }
            else
            {
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    SetKingshillInfo(json);
                    Debug.Log("Web Success : " + www.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogError("Web Error : " + e.Message);
                }
            }
        }
    }

    public static void SetKingshillInfo(JObject data)
    {
        var agentList = data.ValueOrDefault<JObject[]>("agentList", null);
        if (agentList != null)
        {
            agentData = new JObject();
            foreach (JObject agent in agentList)
            {
                agentData[agent["agentCode"].ToString()] = agent["agentName"];
            }
        }

        var ticketList = data.ValueOrDefault<JObject[]>("ticketList", null);
        if (ticketList != null)
        {
            ticketData = new JObject();
            foreach (JObject ticket in ticketList)
            {
                ticketData[ticket["ticketCode"].ToString()] = ticket["ticketName"];
            }
        }
        var ticketPriceList = data.CastOrEmpty<JArray>("ticketPriceList", true);
        if (ticketPriceList != null)
        {
            ticketPriceData = ticketPriceList;
        }
    }
}
