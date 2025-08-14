using System;
using UnityEngine;

namespace distriqt.plugins.share.applications
{
    [Serializable]
    public class Intent
    {

        public const string ACTION_VIEW = "android.intent.action.VIEW";


        public string action = ACTION_VIEW;
        public string data = "";
        public string type = "";
        public string packageName = "";
        public string extrasJSON = "{}";

        public Intent( string action, string data = null)
        {
            this.action = action;
            this.data = data;
        }


        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }

    }

}
