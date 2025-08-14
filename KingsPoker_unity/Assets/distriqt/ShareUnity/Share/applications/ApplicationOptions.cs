using System;
using UnityEngine;

namespace distriqt.plugins.share.applications
{
    [Serializable]
    public class ApplicationOptions
    {

        public const string ACTION_MAIN = "MAIN";
        public const string ACTION_VIEW = "VIEW";
        public const string ACTION_SENDTO = "SENDTO";
        public const string ACTION_SEND = "SEND";


        public string data = "";
        public string parameters = "";
        public string action = ACTION_MAIN;
        public object extras;
        public string type = "";

        public ApplicationOptions()
        {
            extras = new object();
        }

        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }

    }

}
