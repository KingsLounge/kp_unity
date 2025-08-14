using System;

namespace distriqt.plugins.share.events
{
    [Serializable]
    public class EmailEvent
    {
        public const string MESSAGE_MAIL_ATTACHMENT_ERROR = "message:mail:attachment:error";
        public const string MESSAGE_MAIL_COMPOSE = "message:mail:compose";

        public const string MESSAGE_MAIL_COMPOSE_COMPLETE = "complete";
        public const string MESSAGE_MAIL_COMPOSE_CANCELLED = "cancelled";
        public const string MESSAGE_MAIL_COMPOSE_SAVED = "saved";
        public const string MESSAGE_MAIL_COMPOSE_SENT = "sent";
        public const string MESSAGE_MAIL_COMPOSE_FAILED = "failed";
        public const string MESSAGE_MAIL_COMPOSE_UNKNOWN = "unknown";



        public string details = "";


        public EmailEvent()
        {
        }

    }
}
