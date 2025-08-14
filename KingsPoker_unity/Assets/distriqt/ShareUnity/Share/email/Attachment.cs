using System;
using UnityEngine;

namespace distriqt.plugins.share.email
{
    [Serializable]
    public class Attachment
    {
        /// <summary>
        /// The native path on the device to the file to use as the attachment. 
        /// </summary>
        public string nativePath;

        /// <summary>
        /// A mimetype to use for the attachment. Not required but makes the email attachments
        /// appear better in some mail applications.
        /// </summary>
        public string mimeType = "";

        /// <summary>
        /// On iOS this can be used to rename the name of the file attached to the email
        /// </summary>
        public string filename = "";

        /// <summary>
        /// Unused
        /// </summary>
        public string location = "";



        public Attachment()
        {
        }

        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }
        
    }

}