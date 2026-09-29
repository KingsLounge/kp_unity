using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// 토너먼트 바이인 옵션 — 서버(FW)가 목록/정보 패킷에 buyin_options / reentry_options 로 붙여 준다.
/// 한 옵션 = 유저가 고르는 결제 묶음. 묶음 안 칩·KP·티켓은 전부 내고, 옵션끼리는 택1.
/// 옵션 배열이 없는(구버전) 토너도 서버가 기존 필드로 합성해 보내므로 항상 있다고 봐도 되지만,
/// 없으면 호출 쪽에서 기존 필드 경로로 처리한다.
/// </summary>
public class BuyinOption
{
    public struct Ticket
    {
        public int type;
        public int count;
    }

    public string id;
    public long chip;
    public long kp;
    public List<Ticket> tickets = new List<Ticket>();
    public long pool;

    public bool IsFree => chip <= 0 && kp <= 0 && tickets.Count == 0;
    public bool HasChip => chip > 0;
    public bool HasKp => kp > 0;
    public bool HasTicket => tickets.Count > 0;

    public static List<BuyinOption> Parse(JObject info, string key)
    {
        var list = new List<BuyinOption>();
        if (info == null || !info.ContainsKey(key) || info[key].Type != JTokenType.Array)
        {
            return list;
        }
        foreach (var token in (JArray)info[key])
        {
            var o = token as JObject;
            if (o == null)
            {
                continue;
            }
            var opt = new BuyinOption
            {
                id = o.ValueOrDefault("id", $"opt{list.Count + 1}"),
                chip = o.ValueOrDefault<long>("chip", 0),
                kp = o.ValueOrDefault<long>("kp", 0),
                pool = o.ValueOrDefault<long>("pool", 0),
            };
            if (o.ContainsKey("tickets") && o["tickets"].Type == JTokenType.Array)
            {
                foreach (var t in (JArray)o["tickets"])
                {
                    var tj = t as JObject;
                    if (tj == null)
                    {
                        continue;
                    }
                    var type = tj.ValueOrDefault("type", 0);
                    var count = tj.ValueOrDefault("count", 0);
                    if (type > 0 && count > 0)
                    {
                        opt.tickets.Add(new Ticket { type = type, count = count });
                    }
                }
            }
            list.Add(opt);
        }
        return list;
    }

    /// <summary>"2칩 + 5번티켓 1장" 처럼 한 줄 요약. 티켓 이름은 킹스라운지에서 받아오므로 비동기.</summary>
    public async UniTask<string> Label(int scale = 1)
    {
        var sb = new StringBuilder();
        if (chip > 0)
        {
            sb.Append(MoneyToString.Converting(chip * scale)).Append("칩");
        }
        if (kp > 0)
        {
            if (sb.Length > 0) sb.Append(" + ");
            sb.Append(MoneyToString.Converting(kp * scale)).Append("KP");
        }
        foreach (var t in tickets)
        {
            if (sb.Length > 0) sb.Append(" + ");
            var name = await KingshillInfo.GetTicketString(t.type);
            sb.Append(name).Append(' ').Append(t.count * scale).Append("장");
        }
        return sb.Length > 0 ? sb.ToString() : LocalizeManager.GetLocalString("free");
    }

    /// <summary>비동기 불가한 곳용 — 티켓 이름은 캐시만 조회.</summary>
    public string LabelCached(int scale = 1)
    {
        var sb = new StringBuilder();
        if (chip > 0)
        {
            sb.Append(MoneyToString.Converting(chip * scale)).Append("칩");
        }
        if (kp > 0)
        {
            if (sb.Length > 0) sb.Append(" + ");
            sb.Append(MoneyToString.Converting(kp * scale)).Append("KP");
        }
        foreach (var t in tickets)
        {
            if (sb.Length > 0) sb.Append(" + ");
            sb.Append(KingshillInfo.GetTicketStringCached(t.type)).Append(' ').Append(t.count * scale).Append("장");
        }
        return sb.Length > 0 ? sb.ToString() : LocalizeManager.GetLocalString("free");
    }

    /// <summary>내 보유량으로 이 옵션을 몇 배까지 살 수 있는지 (배율 상한 계산용).</summary>
    public int MaxAffordableScale(long myChip, long myKp, IList<long> myTickets, int fallback)
    {
        var max = int.MaxValue;
        if (chip > 0) max = (int)System.Math.Min(max, myChip / chip);
        if (kp > 0) max = (int)System.Math.Min(max, myKp / kp);
        foreach (var t in tickets)
        {
            var have = t.type >= 1 && t.type <= myTickets.Count ? myTickets[t.type - 1] : 0;
            max = (int)System.Math.Min(max, have / t.count);
        }
        return max == int.MaxValue ? fallback : max;
    }
}
