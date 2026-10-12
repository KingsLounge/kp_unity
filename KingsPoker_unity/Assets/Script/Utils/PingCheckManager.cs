using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using BestHTTP.SocketIO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public static class PingCheckManager
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool isNetworkAvailable();
#endif

    public static UnityEvent<int> success = new UnityEvent<int>();
    public static UnityEvent failure = new UnityEvent();
    private static bool pinging = false;

    public static bool CheckNetworkConnection(
        System.Action success = null,
        System.Action failure = null,
        int retryCount = 5
    )
    {
        if (IsNetworkAvailable())
        {
            success?.Invoke();
            return true;
        }
        else
        {
            if (retryCount > 0)
            {
                return CheckNetworkConnection(success, failure, retryCount - 1);
            }
            else
            {
                failure?.Invoke();
                return false;
            }
        }
    }

    public static async UniTask<bool> CheckNetworkPing(
        System.Action success = null,
        System.Action failure = null,
        int retryCount = 5
    )
    {
        string host = "google.com";
#if UNITY_EDITOR
        //try
        //{
        //    IPHostEntry ipEntry = Dns.GetHostEntry(host);
        //    if (ipEntry.AddressList[0] != null)
        //    {
        //        UnityEngine.Ping ping = new UnityEngine.Ping(ipEntry.AddressList[0].ToString());
        //        var cts = new System.Threading.CancellationTokenSource();
        //        cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
        //        await UniTask.WaitForSeconds(2f);

        //        if(ping.isDone)
        //        {
        //            success?.Invoke();
        //        }
        //        else
        //        {
        //            failure?.Invoke();
        //        }
        //    }

        //}
        //catch (Exception e)
        //{
        //    failure?.Invoke();
        //}


        try
        {
            var pingSender = new System.Net.NetworkInformation.Ping();
            var pingReply = await pingSender.SendPingAsync(host);

            if (pingReply.Status == IPStatus.Success)
            {
                success?.Invoke();
                return true;
            }
            else
            {
                if (retryCount > 0)
                {
                    return await CheckNetworkPing(success, failure, retryCount - 1);
                }
                else
                {
                    failure?.Invoke();
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Console.Log(e);
            return false;
        }

#elif UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
        using (UnityWebRequest www = UnityWebRequest.Get(host))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                success?.Invoke();
                return true;
            }
            else if (
                www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError
            )
            {
                if (retryCount > 0)
                {
                    return await CheckNetworkPing(success, failure, retryCount - 1);
                }
                else
                {
                    failure?.Invoke();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
#endif
    }

    public static async UniTask<UnityWebRequest> CheckApiPing(
        System.Action success = null,
        System.Action failure = null,
        int retryCount = 5
    )
    {
        return await PublisherApiManager.Instance.PingCheckAsync();
    }

    public static async UniTask<bool> CheckWebsocketLogin(double timeout = 10)
    {
        try
        {
            Console.Log("--------Wait Ws--------");
            await UniTask
                .WaitUntil(
                    () =>
                        WebSocketManager.defaultCli != null && WebSocketManager.defaultCli.isOpened
                )
                .Timeout(TimeSpan.FromSeconds(timeout));
            Console.Log("--------Connected Ws--------");
            var wait = new WaitForPCProtocol(PCProtocol.PC_PING);
            await UniTask.WaitUntil(() => !wait.keepWaiting).Timeout(TimeSpan.FromSeconds(timeout));
            Console.Log("--------LOGIN_SUCCESS Ws--------");

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    // 마지막 CP_PING 을 보낸 시각(Stopwatch 틱). 수신 시각은 WebSocketClient.PingReceivedTicks.
    private static long pingSentTicks = 0;

    /// <summary>
    /// 마지막 핑의 왕복 시간(ms). 송신 직전 ~ 소켓 콜백 도착 기준이라 프레임 대기가 섞이지 않는다.
    /// 수신 기록이 없거나 송신보다 앞서면(이전 핑의 값) -1.
    /// </summary>
    private static double LastPingRttMs()
    {
        long recv = WebSocketClient.PingReceivedTicks;
        if (recv <= pingSentTicks) return -1;
        return (recv - pingSentTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
    }

    public static async UniTask<bool> CheckWebSocketPing(double timeout = 2)
    {
        try
        {
            if (!WebSocketManager.defaultCli.isOpened)
            {
                return false;
            }
            Packet p = new Packet(CPProtocol.CP_PING);
            pingSentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            WebSocketManager.defaultCli.Send(p);
            var wait = new WaitForPCProtocol(PCProtocol.PC_PING);

            await UniTask.WaitUntil(() => !wait.keepWaiting).Timeout(TimeSpan.FromSeconds(timeout));

            if (wait.Result.c.ValueOrDefault("ecode", 0) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static async void RepeatWebSocketPing(int checkCount = 5, float interval = 1f)
    {
        Queue<int> queue = new Queue<int>();

        int cnt = 0;

        Console.Log("--------PING REPEAT START--------");

        if (pinging)
            return;
        pinging = true;
        while (true)
        {
            try
            {
                System.Diagnostics.Stopwatch fallback = System.Diagnostics.Stopwatch.StartNew();

                var succ = await CheckWebSocketPing();

                if (succ)
                {
                    // 소켓 도착 시각 기준. 못 구하면 예전처럼 완료 시점 기준(프레임 대기 포함)으로 대체.
                    // 예전 코드의 .Milliseconds 는 밀리초 자릿수(0~999)만 돌려줘 1초 넘는 지연이 작게 보였다.
                    double rtt = LastPingRttMs();
                    if (rtt < 0) rtt = fallback.Elapsed.TotalMilliseconds;
                    queue.Enqueue((int)Math.Round(rtt));
                    while (queue.Count > checkCount)
                    {
                        queue.Dequeue();
                    }

                    int sum = queue.Sum();
                    int ev = sum / queue.Count;
                    success?.Invoke(ev);
                }
                else
                {
                    failure.Invoke();
                    queue.Clear();
                }
                await UniTask.WaitForSeconds(interval);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                break;
            }
        }
        pinging = false;
        Console.Log("--------PING REPEAT FAIL--------");
    }

    public static bool IsNetworkAvailable()
    {
#if UNITY_EDITOR
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            return false;
        }

        foreach (NetworkInterface net in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (
                (net.OperationalStatus != OperationalStatus.Up)
                || (net.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                || (net.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            )
            {
                continue;
            }

            if (
                (net.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0)
                || (net.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0)
            )
            {
                continue;
            }

            if (
                net.Description.Equals(
                    "Microsoft Loopback Adapter",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            return true;
        }

        return false;
#elif UNITY_ANDROID || UNITY_IOS
        return Application.internetReachability != NetworkReachability.NotReachable;
#elif UNITY_WEBGL
        return isNetworkAvailable();
#endif
    }
}
