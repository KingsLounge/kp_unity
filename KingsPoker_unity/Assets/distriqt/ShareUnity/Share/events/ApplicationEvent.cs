using System;

namespace distriqt.plugins.share.events
{
    [Serializable]
    public class ApplicationEvent
    {
        public const string ACTIVITY_RESULT = "activity:result";


        public int resultCode;
        public object data;

        public ApplicationEvent()
        {
        }
    }
}
