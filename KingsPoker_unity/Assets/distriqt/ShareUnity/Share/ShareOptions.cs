using System;
using UnityEngine;

namespace distriqt.plugins.share
{

    [Serializable]
    public class ShareOptions
    {
        public const int ARROWDIRECTION_NONE     = 0;
		public const int ARROWDIRECTION_UP       = 1 << 0;
		public const int ARROWDIRECTION_DOWN     = 1 << 1;
		public const int ARROWDIRECTION_LEFT     = 1 << 2;
		public const int ARROWDIRECTION_RIGHT    = 1 << 3;

        public const int ARROWDIRECTION_ANY = ARROWDIRECTION_UP | ARROWDIRECTION_DOWN | ARROWDIRECTION_LEFT | ARROWDIRECTION_RIGHT;



        public Rect position;
        public int arrowDirection;

        public bool autoScale = false;
        public bool useChooser = true;

        public string title = "Share ...";
        public bool showOpenIn = false;
        public string packageName = "";
        public string UTI = "";
        public bool cacheBitmapInternally = true;
        public string[] excludedActivityTypes;


        public ShareOptions()
        {
            position = new Rect(0, 0, 100, 100);
            excludedActivityTypes = new string[] { };
        }


        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }

    }

}