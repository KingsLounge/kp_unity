using System;

public static class DateTimeParser
{
    private static string[] formats = new string[] { "MM/dd/yyyy HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", };
    public static DateTime Parse(string timeString)
    {
        DateTime time;

        if (DateTime.TryParseExact(timeString, formats, null, System.Globalization.DateTimeStyles.None, out time))
        {
            return time;
        }

        if (DateTime.TryParse(timeString,out time))
        {
            return time;       
        }
        
        time = DateTime.Now;
        return time;
    }
}
