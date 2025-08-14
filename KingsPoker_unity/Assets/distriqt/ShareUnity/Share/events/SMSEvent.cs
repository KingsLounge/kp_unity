using System;
using distriqt.plugins.share.sms;

namespace distriqt.plugins.share.events
{
    [Serializable]
    public class SMSEvent
    {

        public const string MESSAGE_SMS_RECEIVED = "sms:received";
        public const string MESSAGE_SMS_SENT = "sms:sent";
        public const string MESSAGE_SMS_CANCELLED = "sms:cancelled";
        public const string MESSAGE_SMS_SENT_ERROR = "sms:sent:error";
        public const string MESSAGE_SMS_DELIVERED = "sms:delivered";
        public const string MESSAGE_SMS_NOT_DELIVERED = "sms:not:delivered";


        /// <summary>
        /// An error message or additional information about this event
        /// </summary>
        public string error = "";

        /// <summary>
        /// The sms concerned. Generally only populated on a sms received event
        /// </summary>
        public SMS sms = null;


        public SMSEvent()
        {
        }

    }
}
