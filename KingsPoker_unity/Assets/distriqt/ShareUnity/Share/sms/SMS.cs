using System;
using UnityEngine;

namespace distriqt.plugins.share.sms
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SMS
    {
        /// <summary>
        /// 
        /// </summary>
        public string id;

        /// <summary>
        /// The phone number or address of the destination eg 0444444444
        /// </summary>
        public string address;

        /// <summary>
        /// The contents of the message
        /// </summary>
        public string message;

        /// <summary>
        /// 
        /// </summary>
        public int subscriptionId;


        public SMS()
        {
        }


        public string ToJSONString()
        {
            return JsonUtility.ToJson(this);
        }


    }
}
