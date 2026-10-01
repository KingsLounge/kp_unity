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

    // 서버가 준 옵션이면 true → 신청 때 option id 를 보낸다.
    // false 면 앱이 기존 필드로 합성한 것(구버전 서버) → legacyScaleKey 로 예전 방식 신청.
    public bool fromServer = true;
    public string legacyScaleKey = "scale"; // "scale" | "scale_chip" | "scale_ticket"

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

    /// <summary>
    /// 이 토너의 옵션 목록. 서버가 buyin_options / reentry_options 를 줬으면 그것,
    /// 없으면(구버전 서버) 기존 필드로 서버와 같은 규칙으로 합성한다.
    /// </summary>
    public static List<BuyinOption> Resolve(JObject info, bool reentry)
    {
        var list = Parse(info, reentry ? "reentry_options" : "buyin_options");
        if (reentry && list.Count == 0)
        {
            list = Parse(info, "buyin_options");
        }
        if (list.Count > 0)
        {
            return list;
        }
        return FromLegacy(info, reentry);
    }

    /// <summary>
    /// 기존 필드(t_buyin, t_buyin_fee, t_reentry_cost, t_ticket, t_o.buyin_kp/reentry_kp)로 합성.
    ///  - KP 전용          → [kp]
    ///  - 티켓 조건 'or'   → [chip] , [ticket]   (신청 키 scale_chip / scale_ticket)
    ///  - 그 외            → [chip (+ticket)]    (신청 키 scale)
    /// </summary>
    public static List<BuyinOption> FromLegacy(JObject info, bool reentry)
    {
        var list = new List<BuyinOption>();
        if (info == null)
        {
            return list;
        }
        var t_o = info.CastOrEmpty<JObject>("t_o", true);
        var kpBuyin = t_o.ValueOrDefault<long>("buyin_kp", 0, true);
        var kpReentry = t_o.ValueOrDefault<long>("reentry_kp", 0, true);
        if (kpReentry <= 0)
        {
            kpReentry = kpBuyin;
        }
        if (kpBuyin > 0)
        {
            list.Add(new BuyinOption { id = "kp", kp = reentry ? kpReentry : kpBuyin, fromServer = false, legacyScaleKey = "scale" });
            return list;
        }

        long chip = reentry
            ? info.ValueOrDefault<long>("t_reentry_cost", 0, true)
            : info.ValueOrDefault<long>("t_buyin", 0, true) + info.ValueOrDefault<long>("t_buyin_fee", 0, true);

        var t_ticket = info.CastOrEmpty<JObject>("t_ticket", true);
        var ticketType = t_ticket.ValueOrDefault("ticket_type", 0, true);
        var ticketCount = t_ticket.ValueOrDefault("ticket_count", 0, true);
        var condition = t_ticket.ValueOrDefault("condition", "and", true);
        var hasTicket = ticketType > 0 && ticketCount > 0;

        if (hasTicket && condition.Equals("or"))
        {
            list.Add(new BuyinOption { id = "chip", chip = chip, fromServer = false, legacyScaleKey = "scale_chip" });
            var ticketOnly = new BuyinOption { id = "ticket", fromServer = false, legacyScaleKey = "scale_ticket" };
            ticketOnly.tickets.Add(new Ticket { type = ticketType, count = ticketCount });
            list.Add(ticketOnly);
            return list;
        }

        var single = new BuyinOption { id = hasTicket ? "chip_ticket" : "chip", chip = chip, fromServer = false, legacyScaleKey = "scale" };
        if (hasTicket)
        {
            single.tickets.Add(new Ticket { type = ticketType, count = ticketCount });
        }
        list.Add(single);
        return list;
    }

    /// <summary>
    /// 이 옵션에 관련된 재화의 내 보유량 한 줄: "보유 15칩 · 340KP · 데일리 시드권 0장".
    /// scale 배만큼 사기에 모자란 재화는 shortColor 로 감싼다 (Text 의 Rich Text 필요).
    /// </summary>
    public async UniTask<string> HoldLabel(long myChip, long myKp, IList<long> myTickets, int scale, string shortColor = "#E5484D")
    {
        var parts = new List<string>();
        System.Func<string, bool, string> mark = (text, isShort) => isShort ? $"<color={shortColor}>{text}</color>" : text;
        if (chip > 0)
        {
            parts.Add(mark(MoneyToString.Converting(myChip) + "칩", myChip < chip * scale));
        }
        if (kp > 0)
        {
            parts.Add(mark(MoneyToString.Converting(myKp) + "KP", myKp < kp * scale));
        }
        foreach (var t in tickets)
        {
            var have = t.type >= 1 && t.type <= myTickets.Count ? myTickets[t.type - 1] : 0;
            var name = await KingshillInfo.GetTicketString(t.type);
            parts.Add(mark($"{name} {have}장", have < (long)t.count * scale));
        }
        if (parts.Count == 0)
        {
            return string.Empty;
        }
        return "보유 " + string.Join(" · ", parts);
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
