using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using Assets.SimpleAndroidNotifications;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class LocalNotificationManager : MonoBehaviour
{
    private static string IosNotificationKey = "ios_noti_";
    
    public static int SendNotification(string title, string content, DateTime time)
    {
        var TimeInterval = time - DateTime.UtcNow;
        if (TimeInterval.TotalSeconds < 0)
            return 0;
#if UNITY_ANDROID
        TimeInterval = time - DateTime.UtcNow;
        return NotificationManager.SendWithAppIcon(TimeInterval, title, content, Color.gray, NotificationIcon.Bell);
#elif UNITY_IOS
        int id = PlayerPrefs.GetInt("IosNotificationLastId") + 1;
        PlayerPrefs.SetInt("IosNotificationLastId", id);
        PlayerPrefs.Save();
        
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = time - DateTime.UtcNow,
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = IosNotificationKey + id,
            Title = title,
            Body = content,
            //Subtitle = content,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        
        return id;
#endif
        return 0;
    }
    
    public static void CancelNotification(int id)
    {
#if UNITY_ANDROID
        NotificationManager.Cancel(id);
#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification(IosNotificationKey + id);
        iOSNotificationCenter.RemoveDeliveredNotification(IosNotificationKey + id);
#endif
    }

    public static void CancelAllNotifications()
    {
#if UNITY_ANDROID
        NotificationManager.CancelAll();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
    }
}
