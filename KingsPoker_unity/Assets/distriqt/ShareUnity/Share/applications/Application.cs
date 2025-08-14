using System;
using UnityEngine;

namespace distriqt.plugins.share.applications
{
    [Serializable]
    public class Application
    {

        public string packageName;
        public string applicationUrl;

        public Application(string packageName, string applicationUrl)
        {
            this.packageName = packageName;
            this.applicationUrl = applicationUrl;
        }

        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }
    }

}
