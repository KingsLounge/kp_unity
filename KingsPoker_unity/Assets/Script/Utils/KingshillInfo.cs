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
        if (ticketData == null) // 킹스라운지 응답 실패 시 빈 목록
        {
            return keys;
        }
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
        return ticketData.ValueOrDefault(ticketString, FallbackTicketName(t_type));
    }

    public static async UniTask<string> GetTicketString(string ticket)
    {
        await GetTicketData();
        return ticketData.ValueOrDefault(ticket, FallbackTicketName(GetTicketGbn(ticket)));
    }

    // 킹스라운지 장애로 이름을 못 받아온 경우 표시용 대체 이름
    private static string FallbackTicketName(int t_type)
    {
        return t_type <= 1 ? "티켓" : $"티켓{t_type}";
    }

    // 비동기 호출이 불가능한 곳(패킷 핸들러 등)용 — 캐시만 조회, 없으면 대체 이름
    public static string GetTicketStringCached(int t_type)
    {
        var ticketString = "ticket";
        if (t_type != 1)
        {
            ticketString += t_type.ToString();
        }
        return ticketData.ValueOrDefault(ticketString, FallbackTicketName(t_type));
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

    // 킹스라운지 장애 시 아이템마다 재요청/타임아웃 대기하는 것을 막기 위한 상태
    private static bool fetching;
    private static float lastFailTime = -9999f;
    private const float RETRY_COOLDOWN = 10f; // 실패 후 재시도 간격(초)
    private const int REQUEST_TIMEOUT = 5; // 초

    public static async UniTask GetKingshillInfo()
    {
        // 이미 요청 중이면 그 결과만 기다린다 (토너 목록이 아이템 수만큼 동시 호출)
        if (fetching)
        {
            await UniTask.WaitWhile(() => fetching);
            return;
        }
        // 직전 실패 후 쿨다운 동안은 재요청하지 않고 폴백 표시로 넘어간다
        if (Time.realtimeSinceStartup - lastFailTime < RETRY_COOLDOWN)
        {
            return;
        }

        fetching = true;
        try
        {
            WWWForm form = new WWWForm();
            string url = $"{Constant.GameConfig.KingshillUrl}/api/getAgentList";
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.timeout = REQUEST_TIMEOUT;
                // UniTask 는 ConnectionError/ProtocolError 를 예외로 던지므로 전체를 try 로 감싼다
                await www.SendWebRequest();
                JObject json = JObject.Parse(www.downloadHandler.text);
                SetKingshillInfo(json);
                Debug.Log("Web Success : " + www.downloadHandler.text);
            }
        }
        catch (Exception e)
        {
            lastFailTime = Time.realtimeSinceStartup;
            Debug.LogError("KingshillInfo fetch failed : " + e.Message);
        }
        finally
        {
            fetching = false;
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
