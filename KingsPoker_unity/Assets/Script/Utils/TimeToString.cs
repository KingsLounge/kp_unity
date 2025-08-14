using System;
public static class TimeToString
{
    public static string SpanToString(TimeSpan span)
    {
        string str = "";
        if ((long)span.TotalDays > 0)
        {
            str = string.Format(LocalizeManager.GetLocalString("mail_timer_info_day_over"), Math.Floor(span.TotalDays), span.Hours, span.Minutes);
        }
        else if ((long)span.TotalHours > 0)
        {
            str = string.Format(LocalizeManager.GetLocalString("mail_timer_info_hours_over"), span.Hours, span.Minutes);
        }
        else if ((long)span.TotalMinutes > 0)
        {
            str = string.Format(LocalizeManager.GetLocalString("mail_timer_info_minutes_over"), span.Minutes, span.Seconds);
        }
        else
        {
            str = string.Format(LocalizeManager.GetLocalString("mail_timer_info_less_minute"), span.Seconds);
        }
        return str;
    }
}
