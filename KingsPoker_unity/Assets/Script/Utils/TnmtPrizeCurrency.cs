using Newtonsoft.Json.Linq;

/// <summary>
/// 토너먼트 상금 지급 통화. 서버가 t_rewards.prize_currency 를 "kp" 로 주면 비율/고정 칩 상금이 KP 로 나간다.
/// 아직 서버에 없는 설정이라 지금은 항상 칩이지만, 앱이 먼저 읽도록 해 두면 서버가 붙는 날 그대로 KP 로 보인다.
/// </summary>
public static class TnmtPrizeCurrency
{
    public const string Chip = "chip";
    public const string Kp = "kp";

    public static string Of(JObject t_rewards)
    {
        if (t_rewards == null)
        {
            return Chip;
        }
        var v = t_rewards.ValueOrDefault("prize_currency", Chip, true);
        return string.IsNullOrEmpty(v) ? Chip : v.ToLower();
    }

    public static bool IsKp(JObject t_rewards)
    {
        return Of(t_rewards) == Kp;
    }

    /// <summary>토너먼트 info(JObject) 에서 바로 판단.</summary>
    public static bool IsKpInfo(JObject info)
    {
        return info != null && IsKp(info.CastOrEmpty<JObject>("t_rewards", true));
    }

    /// <summary>상금풀 금액 표시용: KP 지급이면 "1,000 KP", 아니면 "1,000".</summary>
    public static string PoolText(JObject info, long amount)
    {
        var s = MoneyToString.Converting(amount);
        return IsKpInfo(info) ? s + " KP" : s;
    }
}
