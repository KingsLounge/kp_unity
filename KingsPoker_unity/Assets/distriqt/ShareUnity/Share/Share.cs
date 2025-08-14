using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using distriqt.plugins.share.events;
using distriqt.plugins.share.applications;
using distriqt.plugins.share.sms;
using distriqt.plugins.share.email;

namespace distriqt.plugins.share
{

    /// <summary>
    /// The main class for the Share plugin
    /// </summary>
    public class Share : MonoBehaviour
    {
        private const string version = "1.0.1";
        private const string MISSING_IMPLEMENTATION_ERROR_MESSAGE = "Check you have correctly included the library for this platform";

#if UNITY_IOS
        const string dll = "__Internal";

        [DllImport(dll)]
        private static extern string Share_version();
        [DllImport(dll)]
        private static extern string Share_implementation();
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_isSupported();

        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_isShareSupported();
        [DllImport(dll)]
        private static extern void Share_share(string text, string url, byte[] imageData, int imageWidth, int imageHeight, string optionsJSON);
        [DllImport(dll)]
        private static extern void Share_shareFile(string path, string name, string mimeType, string optionsJSON, bool useActivityViewController);
        [DllImport(dll)]
        private static extern void Share_showOpenIn(string path, string name, string mimeType, string optionsJSON);

        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_social_isNetworkSupported(string network);
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_social_sharePost(string postJSON, string network);

        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_applications_isInstalled(string applicationUrl);
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_applications_launch(string applicationUrl, string optionsJSON);
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_applications_startActivity(string intentJSON);

        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_email_isMailSupported();
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_email_sendMailWithOptions(string subject, string body, string toRecipients, string ccRecipients, string bccRecipients, string attachmentsJSON, bool isHTML, bool useChooser );
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_email_sendMail(string subject, string body, string toRecipients);

        [DllImport(dll)]
        private static extern string Share_sms_authorisationStatus();
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_sms_hasAuthorisation();
        [DllImport(dll)]
        private static extern void Share_sms_requestAuthorisation();
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_sms_isSMSSupported();
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_sms_sendSMS(string smsJSON, int subscriptionId);
        [DllImport(dll)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Share_sms_sendSMSWithUI(string smsJSON, bool useChooser);


#elif UNITY_ANDROID
        private static AndroidJavaClass pluginClass;
        private static AndroidJavaObject extContext;
#endif


        //
        //  VARIABLES
        //


        private static bool _create;
        private static Share _instance;


        



        //
        //  FUNCTIONALITY
        //

        static Share()
        {
#if UNITY_ANDROID
            try
            {
                pluginClass = new AndroidJavaClass("com.distriqt.extension.share.ShareUnityPlugin");
                extContext = pluginClass.CallStatic<AndroidJavaObject>("instance");
            }
            catch
            {

            }
#endif
        }


        private Share()
        {
        }


        /// <summary>
        /// Access to the singleton instance for all this plugins functionality
        /// </summary>
        public static Share Instance
        {
            get
            {
                if (_instance == null)
                {
                    _create = true;

                    GameObject go = new GameObject();
                    _instance = go.AddComponent<Share>();
                    _instance.name = "Share";
                }
                return _instance;
            }
        }


        private static bool platformSupported()
        {
#if UNITY_IOS || UNITY_ANDROID
            return
                (UnityEngine.Application.platform != RuntimePlatform.OSXEditor)
                && (UnityEngine.Application.platform != RuntimePlatform.WindowsEditor)
                && (UnityEngine.Application.platform != RuntimePlatform.LinuxEditor)
            ;

//#elif UNITY_STANDALONE_OSX
//            return
//                //(Application.platform != RuntimePlatform.OSXEditor) &&
//                (Application.platform != RuntimePlatform.WindowsEditor) &&
//                (Application.platform != RuntimePlatform.LinuxEditor)
//            ;

#else
            return false;
#endif

        }


        /// <summary>
        /// Whether the current device supports the extensions functionality
        /// </summary>
        public static bool isSupported
        {
            get
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_isSupported();
#elif UNITY_ANDROID
                        return pluginClass.CallStatic<bool>("isSupported");
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }
        }

        /// <summary>
        /// The version of this extension.
        /// This should be of the format, MAJOR.MINOR.BUILD
        /// </summary>
        /// <returns>The version of this extension</returns>
        public string Version()
        {
            return version;
        }


        /// <summary>
        /// The native version string of the native extension
        /// </summary>
        /// <returns>The native version string of the native extension</returns>
        public string NativeVersion()
        {
            try
            {
                if (platformSupported())
                {
#if UNITY_IOS
                    return Share_version();
#elif UNITY_ANDROID
                    return extContext.Call<string>("version");
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
            return "0";
        }


        /// <summary>
        /// The implementation currently in use.
        /// This should be one of the following depending on the platform in use and the functionality supported by this extension:
        /// <ul>
        /// <li><code>Android</code></li>
        /// <li><code>iOS</code></li>
        /// <li><code>default</code></li>
        /// <li><code>unknown</code></li>
        /// </ul>
        /// </summary>
        /// <returns>The implementation currently in use</returns>
        public string Implementation()
        {
            try
            {
                if (platformSupported())
                {
#if UNITY_IOS
                    return Share_implementation();
#elif UNITY_ANDROID
                    return extContext.Call<string>("implementation");
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
            return "default";
        }






        public bool isShareSupported()
        {
            try
            {
                if (platformSupported())
                {
#if UNITY_IOS
                    return Share_isShareSupported();
#elif UNITY_ANDROID
                    return extContext.Call<bool>("isShareSupported");
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
            return false;
        }


        /// <summary>
        /// Displays a share action that allows the user to choose another application to share the specified data with.
        /// <br/>
        /// You should always check <code>isShareSupported</code> before calling this function.
        /// <br/>
        /// On iOS this uses the <code>UIActivityViewController</code> to display a list of activities that support the supplied data.
        /// <br/>
        /// It also provides actions such as:
        /// <ul>
        /// <li>Save image</li>
        /// <li>Copy</li>
        /// <li>Print</li>
        /// <li>Add to Reading List</li>
        /// </ul>
        /// <br/>
        /// On iPads the dialog displayed allowing the user to select the destination application should be shown in a popover.
        /// The position and size of this popover is controlled with the <code>options</code> parameter. If null it is displayed modally.
        /// <br/>
        /// <strong>Android</strong>
        /// <br/>
        /// Unfortunately different applications on Android handle the url and text different when they are both supplied to the intent.
        /// We suggest that if you are wanting a url and text to appear in the content of a message that you simply combine the two into the text parameter:
        /// <br/>
        /// <listing>
        /// Share.service.share( "something to share " + "http://airnativeextensions.com" );
        /// </listing>
        /// <br/>
        /// This is the most consistent method if you are not concerned about the url being separated.
        /// </summary>
        /// <param name="text">The text content to share</param>
        /// <param name="image">Image data to share, if null no image is shared</param>
        /// <param name="url">A url to share, if empty ("") no url is shared  </param>
        /// <param name="options">Additional sharing options including the position of the dialog when presented on iPads</param>
        public void share(string text, Texture2D image = null, string url = "", ShareOptions options = null)
        {
            try
            {
                if (platformSupported())
                {
                    if (text == null) text = "";
                    if (url == null) url = "";
                    if (options == null) options = new ShareOptions();


                    byte[] imageData = new byte[0];
                    int imageWidth = 0;
                    int imageHeight = 0;
                    if (image != null)
                    {
                        if (image.format == TextureFormat.RGBA32)
                        {
                            imageData = image.GetRawTextureData();
                        }
                        else
                        {
                            Texture2D t = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
                            t.SetPixels(image.GetPixels());
                            t.Apply();

                            imageData = t.GetRawTextureData();
                        }

                        imageWidth = image.width;
                        imageHeight = image.height;
                    }

#if UNITY_IOS
                    Share_share(text, url, imageData, imageWidth, imageHeight, options.toJSONString());
#elif UNITY_ANDROID
                    extContext.Call("share", text, url, imageData, imageWidth, imageHeight, options.toJSONString() );
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
        }


        /// <summary>
        /// Displays a share action dialog to that allows the user to choose another application services that accept the specified file.
        /// <br/>
        /// You should always check <code>isShareSupported</code> before calling this function.
        /// <br/>
        /// On iOS this uses the <code>UIActivityViewController</code> to display a list of services that support the supplied data.
        /// <br/>
        /// On iPads the dialog displayed allowing the user to select the destination application should be shown in a popover.
        /// The position and size of this popover is controlled with the <code>options</code> parameter. If null it is displayed modally.
        /// <br/>
        /// If <code>options.showOpenIn</code> is <code>true</code> then the dialog will display an "Open in ..." action which will trigger display of the<code> showOpenIn</code> dialog.
        /// </summary>
        /// <param name="path">The path of a file to share</param>
        /// <param name="name">The name of the file to use when sharing</param>
        /// <param name="mimeType">The mime type of the file (is used to limit the displayed applications)</param>
        /// <param name="options">Additional sharing options including the position of the dialog when presented on iPads</param>
        /// <param name="useActivityViewController">This controls whether this function on iOS uses the activity view controller or the document interaction controller</param>
        public void shareFile(string path, string name, string mimeType, ShareOptions options = null, bool useActivityViewController = false)
        {
            try
            {
                if (platformSupported())
                {
                    if (path == null) return;
                    if (name == null) name = "";
                    if (mimeType == null) mimeType = "";
                    if (options == null) options = new ShareOptions();

#if UNITY_IOS
                    Share_shareFile(path, name, mimeType, options.toJSONString(), useActivityViewController);
#elif UNITY_ANDROID
                    extContext.Call("shareFile", path, name, mimeType, options.toJSONString(), useActivityViewController );
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
        }



        /// <summary>
        /// Displays an open in dialog that allows the user to choose another application to <b>open</b> the specified file with.
        /// <br/>
        /// You should always check <code>isShareSupported</code> before calling this function.
        /// <br/>
        /// On iOS this uses the <code>UIDocumentInteractionController</code> to display a list of applications that can open the specified file.
        /// <br/>
        /// On iPads the dialog displayed allowing the user to select the destination application should be shown in a popover.
        /// The position and size of this popover is controlled with the <code>options</code> parameter. If null it is displayed modally.
        /// 
        /// </summary>
        /// <param name="path">The path of a file to share</param>
        /// <param name="name">The name of the file to use when sharing</param>
        /// <param name="mimeType">The mime type of the file (is used to limit the displayed applications)</param>
        /// <param name="options">Additional sharing options including the position of the dialog when presented on iPads</param>
        public void showOpenIn(string path, string name = "", string mimeType = "", ShareOptions options = null)
        {
            try
            {
                if (platformSupported())
                {
                    if (path == null) return;
                    if (name == null) name = "";
                    if (mimeType == null) mimeType = "";
                    if (options == null) options = new ShareOptions();

#if UNITY_IOS
                    Share_showOpenIn(path, name, mimeType, options.toJSONString());
#elif UNITY_ANDROID
                    extContext.Call("showOpenIn", path, name, mimeType, options.toJSONString() );
#endif
                }
            }
            catch (EntryPointNotFoundException e)
            {
                throw new Exception(MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
            }
        }



        //
        //  APPLICATIONS
        //

        private _ApplicationsImpl _applications;
        /// <summary>
        /// Access to the applications launching functionality
        /// </summary>
        public Applications Applications
        {
            get
            {
                if (_applications == null)
                {
                    _applications = new _ApplicationsImpl();
                }
                return _applications;
            }
        }


        private class _ApplicationsImpl : Applications
        {

            public _ApplicationsImpl()
            {
            }

            public event ApplicationEventHandler OnActivityResult;

            public bool IsInstalled(applications.Application app)
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_applications_isInstalled(app.toJSONString());
#elif UNITY_ANDROID
					    return extContext.Call<bool>("applications_isInstalled", app.toJSONString() );
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }


            public bool Launch(applications.Application app, ApplicationOptions options = null)
            {
                try
                {
                    if (platformSupported())
                    {
                        if (options == null) options = new ApplicationOptions();
#if UNITY_IOS
                        return Share_applications_launch(app.toJSONString(), options.toJSONString());
#elif UNITY_ANDROID
    					return extContext.Call<bool>("applications_launch", app.toJSONString(), options.toJSONString() );
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }


            public bool StartActivity(Intent intent)
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_applications_startActivity(intent.toJSONString());
#elif UNITY_ANDROID
					    return extContext.Call<bool>("applications_startActivity", intent.toJSONString() );
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }



            public void HandleEvent(EventData eventData)
            {
                try
                {
                    switch (eventData.code)
                    {
                        case ApplicationEvent.ACTIVITY_RESULT:
                            {
                                ApplicationEvent e = JsonUtility.FromJson<ApplicationEvent>(eventData.data);
                                OnActivityResult?.Invoke(e);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

        }



        //
        //  EMAIL
        //

        private _EmailManagerImpl _emailManager;
        /// <summary>
        /// Access to the EmailManager functionality
        /// </summary>
        public EmailManager Email
        {
            get
            {
                if (_emailManager == null)
                {
                    _emailManager = new _EmailManagerImpl();
                }
                return _emailManager;
            }
        }


        private class _EmailManagerImpl : EmailManager
        {

            public _EmailManagerImpl()
            {
            }

            public bool IsMailSupported
            {
                get
                {
                    try
                    {
                        if (platformSupported())
                        {
#if UNITY_IOS
                            return Share_email_isMailSupported();
#elif UNITY_ANDROID
					        return extContext.Call<bool>("email_isMailSupported");
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                    }
                    return false;
                }
            }


            public event EmailEventHandler OnAttachmentError;
            public event EmailEventHandler OnCompose;


            public bool SendMail(string subject, string body, string toRecipients)
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_email_sendMail(subject, body, toRecipients);
#elif UNITY_ANDROID
					    return extContext.Call<bool>("email_sendMail", subject, body, toRecipients);
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }


            public bool SendMailWithOptions(
                string subject,
                string body,
                string toRecipients,
                string ccRecipients = "",
                string bccRecipients = "",
                Attachment[] attachments = null,
                bool isHTML = true,
                bool useChooser = true)
            {
                try
                {
                    if (platformSupported())
                    {
                        if (attachments == null) attachments = new Attachment[0];
                        string attachmentsJSONArray = "[";
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            Attachment a = attachments[i];
                            string attachmentString = JsonUtility.ToJson(a);
                            if (i > 0) attachmentsJSONArray += ",";
                            attachmentsJSONArray += attachmentString;
                        }
                        attachmentsJSONArray += "]";
#if UNITY_IOS
                        return Share_email_sendMailWithOptions(subject, body, toRecipients, ccRecipients, bccRecipients, attachmentsJSONArray, isHTML, useChooser );
#elif UNITY_ANDROID
					    return extContext.Call<bool>("email_sendMailWithOptions", subject, body, toRecipients, ccRecipients, bccRecipients, attachmentsJSONArray, isHTML, useChooser );
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }



            public void HandleEvent(EventData eventData)
            {
                try
                {
                    EmailEvent e = new EmailEvent();
                    e.details = eventData.data;

                    switch (eventData.code)
                    {
                        case EmailEvent.MESSAGE_MAIL_COMPOSE:
                            OnCompose?.Invoke(e);
                            break;
                        case EmailEvent.MESSAGE_MAIL_ATTACHMENT_ERROR:
                            OnAttachmentError?.Invoke(e);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

        }




        //
        //  SMS
        //

        private _SMSManagerImpl _smsManager;
        /// <summary>
        /// Access to the SMS Manager functionality
        /// </summary>
        public SMSManager SMS
        {
            get
            {
                if (_smsManager == null)
                {
                    _smsManager = new _SMSManagerImpl();
                }
                return _smsManager;
            }
        }


        private class _SMSManagerImpl : SMSManager
        {

            public _SMSManagerImpl()
            {
            }

            public bool IsSMSSupported
            {
                get
                {
                    try
                    {
                        if (platformSupported())
                        {
#if UNITY_IOS
                            return Share_sms_isSMSSupported();
#elif UNITY_ANDROID
					        return extContext.Call<bool>("sms_isSMSSupported");
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                    }
                    return false;
                }
            }

            public event SMSEventHandler OnSMSReceived;
            public event SMSEventHandler OnSMSSent;
            public event SMSEventHandler OnSMSCancelled;
            public event SMSEventHandler OnSMSSentError;
            public event SMSEventHandler OnSMSDelivered;
            public event SMSEventHandler OnSMSNotDelivered;


            public SubscriptionInfo[] GetSubscriptions()
            {
                try
                {
                    if (platformSupported())
                    {
                        string jsonString = "{ 'subscriptions': [] }";
#if UNITY_IOS
                        // Not supported
#elif UNITY_ANDROID
					    jsonString = extContext.Call<string>("sms_getSubscriptions");
#endif

                        SubscriptionInfo[] subscriptions = SubscriptionsJsonHelper.FromJson<SubscriptionInfo>(jsonString);

                        return subscriptions;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return new SubscriptionInfo[0];
            }

            // https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
            public static class SubscriptionsJsonHelper
            {
                public static T[] FromJson<T>(string json)
                {
                    Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                    return wrapper.subscriptions;
                }

                //public static string ToJson<T>(T[] array)
                //{
                //    Wrapper<T> wrapper = new Wrapper<T>();
                //    wrapper.subscriptions = array;
                //    return JsonUtility.ToJson(wrapper);
                //}

                //public static string ToJson<T>(T[] array, bool prettyPrint)
                //{
                //    Wrapper<T> wrapper = new Wrapper<T>();
                //    wrapper.subscriptions = array;
                //    return JsonUtility.ToJson(wrapper, prettyPrint);
                //}

                [Serializable]
                private class Wrapper<T>
                {
                    public T[] subscriptions;
                }
            }


            public bool SendSMS(SMS sms, int subscriptionId = -1)
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_sms_sendSMS(sms.ToJSONString(), subscriptionId);
#elif UNITY_ANDROID
					    return extContext.Call<bool>("sms_sendSMS", sms.ToJSONString(), subscriptionId);
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }


            public bool SendSMSWithUI(SMS sms, bool useChooser = false)
            {
                try
                {
                    if (platformSupported())
                    {
#if UNITY_IOS
                        return Share_sms_sendSMSWithUI(sms.ToJSONString(), useChooser);
#elif UNITY_ANDROID
					    return extContext.Call<bool>("sms_sendSMSWithUI", sms.ToJSONString(), useChooser);
#endif
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(Share.MISSING_IMPLEMENTATION_ERROR_MESSAGE, e);
                }
                return false;
            }


            public void HandleEvent(EventData eventData)
            {
                try
                {
                    SMSEvent e = JsonUtility.FromJson<SMSEvent>(eventData.data);

                    switch (eventData.code)
                    {
                        case SMSEvent.MESSAGE_SMS_SENT:
                            OnSMSSent?.Invoke(e);
                            break;
                        case SMSEvent.MESSAGE_SMS_RECEIVED:
                            OnSMSReceived?.Invoke(e);
                            break;
                        case SMSEvent.MESSAGE_SMS_SENT_ERROR:
                            OnSMSSentError?.Invoke(e);
                            break;
                        case SMSEvent.MESSAGE_SMS_CANCELLED:
                            OnSMSCancelled?.Invoke(e);
                            break;
                        case SMSEvent.MESSAGE_SMS_DELIVERED:
                            OnSMSDelivered?.Invoke(e);
                            break;
                        case SMSEvent.MESSAGE_SMS_NOT_DELIVERED:
                            OnSMSNotDelivered?.Invoke(e);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

        }






        //
        //  EVENT HANDLER
        //

        [System.Serializable]
        private class EventData
        {
            public string code = "";
            public string data = "";
        }


        public void Dispatch(string message)
        {
            try
            {
                EventData eventData = JsonUtility.FromJson<EventData>(message);

                Debug.Log(eventData.code);
                Debug.Log(eventData.data);

                switch (eventData.code)
                {
                    case ShareEvent.COMPLETE:
                        {
                            ShareEvent e = JsonUtility.FromJson<ShareEvent>(eventData.data);
                            OnComplete?.Invoke(e);
                            break;
                        }
                    case ShareEvent.CANCELLED:
                        {
                            ShareEvent e = JsonUtility.FromJson<ShareEvent>(eventData.data);
                            OnCancelled?.Invoke(e);
                            break;
                        }
                    case ShareEvent.FAILED:
                        {
                            ShareEvent e = JsonUtility.FromJson<ShareEvent>(eventData.data);
                            OnFailed?.Invoke(e);
                            break;
                        }
                    case ShareEvent.CLOSED:
                        {
                            ShareEvent e = JsonUtility.FromJson<ShareEvent>(eventData.data);
                            OnClosed?.Invoke(e);
                            break;
                        }


                    case ApplicationEvent.ACTIVITY_RESULT:
                        {
                            _applications.HandleEvent(eventData);
                            break;
                        }


                    case EmailEvent.MESSAGE_MAIL_COMPOSE:
                    case EmailEvent.MESSAGE_MAIL_ATTACHMENT_ERROR:
                        {
                            _emailManager.HandleEvent(eventData);
                            break;
                        }


                    case SMSEvent.MESSAGE_SMS_SENT:
                    case SMSEvent.MESSAGE_SMS_RECEIVED:
                    case SMSEvent.MESSAGE_SMS_SENT_ERROR:
                    case SMSEvent.MESSAGE_SMS_CANCELLED:
                    case SMSEvent.MESSAGE_SMS_DELIVERED:
                    case SMSEvent.MESSAGE_SMS_NOT_DELIVERED:
                        {
                            _smsManager.HandleEvent(eventData);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }



        //
        //  EVENTS
        //


        public delegate void ShareEventHandler(ShareEvent e);

        /// <summary>
        /// Dispatched when the share / open is complete.
        /// <br/>
        /// On iOS you'll be able to determine the activity used to share the content
        /// via the activityType variable on this event.
        /// <br/>
        /// On Android this event is dispatched when the activity finishes, either as a success or a failure.
        /// Android doesn't return information about the success of the activity.
        /// </summary>
        public event ShareEventHandler OnComplete;

        /// <summary>
        /// Dispatched when the share / open  was cancelled by the user
        /// <br/>
        /// This will not occur on Android as we cannot determine the success or failure of the activity.
        /// </summary>
        public event ShareEventHandler OnCancelled;

        /// <summary>
        /// Dispatched when the share / open failed and an error message should be available.
        /// <br/>
        /// This will not occur on Android as we cannot determine the success or failure of the activity.
        /// </summary>
        public event ShareEventHandler OnFailed;

        /// <summary>
        /// Dispatched when the share / open dialog is closed
        /// </summary>
        public event ShareEventHandler OnClosed;




        //
        //  MonoBehaviour
        //


        public void Awake()
        {
            if (_create)
            {
                _create = false;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Enforce singleton
                Destroy(gameObject);
            }
        }


        public void OnDestroy()
        {
        }


    }


}