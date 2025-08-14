using System;

namespace distriqt.plugins.share.events
{
    [Serializable]
    public class ShareEvent
    {
        public const string COMPLETE = "share:complete";
        public const string CANCELLED = "share:cancelled";
        public const string FAILED = "share:failed";
        public const string CLOSED = "share:closed";


        public string activityType = "";
        public string error = "";

        public ShareEvent()
        {
        }
    }
}
