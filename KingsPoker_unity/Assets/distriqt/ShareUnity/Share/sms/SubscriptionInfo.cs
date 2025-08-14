using System;
using UnityEngine;

namespace distriqt.plugins.share.sms
{
    /// <summary>
    /// Subscription Information
    /// </summary>
    [Serializable]
    public class SubscriptionInfo
    {

        /// <summary>
        /// the subscription ID
        /// </summary>
        public int id;

        /// <summary>
        /// the name displayed to the user that identifies this subscription
        /// </summary>
        public string displayName;

        /// <summary>
        /// the name displayed to the user that identifies Subscription provider name
        /// </summary>
        public string carrierName;

        /// <summary>
        /// the ISO country code
        /// </summary>
        public string country;

        /// <summary>
        /// the number of this subscription
        /// </summary>
        public string number;

        /// <summary>
        /// the ICC ID
        /// </summary>
        public string ICCID;

        /// <summary>
        /// the slot index of this Subscription's SIM card
        /// </summary>
        public int simSlotIndex;



        public SubscriptionInfo()
        {
        }


        public string ToJSONString()
        {
            return JsonUtility.ToJson(this);
        }


    }

}