using System;
using distriqt.plugins.share.applications;
using distriqt.plugins.share.events;

namespace distriqt.plugins.share.applications
{


	public delegate void ApplicationEventHandler(ApplicationEvent e);


	/// <summary>
	/// The Applications implementation allows you to launch other
    /// applications installed on the user's device.
    /// </summary>
	public interface Applications
    {
		
		/**
		 * <p>
		 * Check whether an application is installed on a device
		 * </p>
		 * 
		 * 
		 * <strong>iOS</strong>
		 * <p>
		 * On iOS you must add the application schemes you wish to query to your info additions:
		 * </p>
		 * 
		 * <listing>
		 *	&lt;iPhone&gt;
		 *		&lt;InfoAdditions&gt;&lt;![CDATA[
		 * 			&lt;key&gt;LSApplicationQueriesSchemes&lt;/key&gt;
		 * 			&lt;array&gt;
		 * 				&lt;string&gt;instagram&lt;/string&gt;
		 * 				&lt;string&gt;whatsapp&lt;/string&gt;
		 * 			&lt;/array&gt;
		 * 		]]&gt;&lt;/InfoAdditions&gt;
		 * 	&lt;/iPhone&gt;
		 * </listing>
		 * 
		 * @param app	Details of the application of interest (needs a package name on Android and a url scheme on iOS)
		 * 
		 * @return 		true if the application is installed, and false otherwise. Note this will return false if you haven't set the queries schemes on iOS correctly.
		 * 
		 */
		bool IsInstalled(Application app);


		/// <summary>
        /// 
		/// Launch or start an application
        /// <br/>
        /// This function allows your application to start another application on the device.
        /// It uses different methods on iOS and Android however both will allow you to specify
        /// start up parameters to an application:
        /// <ul>
        /// <li>On Android it uses an explicit Intent with a package, type, action and extras</li>
        /// <li>On iOS it uses a custom url scheme with query parameters</li>
        /// </ul>
        /// Example
        /// <br/>
        /// The simplest example of launching another application if installed
        /// <listing>
        /// var app:Application = new Application( "com.instagram.android", "instagram://" );
        /// if (Share.service.applications.isInstalled( app ))
        /// {
        ///  	Share.service.applications.launch( app );
        /// }
        /// </listing>
        /// 
		/// </summary>
		/// <param name="app">Details of the application of interest (needs a package name on Android and a url scheme on iOS)</param>
		/// <param name="options">Start options, including extras and query parameters</param>
		/// <returns>true if the application is installed and launched successfully, false if the application isn't installed or the launch failed</returns>
		bool Launch(Application app, ApplicationOptions options = null);



		/// <summary>
        /// <strong>Android only</strong>
        /// <br/>
        /// This functionality allows you to use the Android Intent system to launch an Intent directly.
        /// <br/>
        ///
        /// This is sometimes required when you need particular control over how the data is passed to an Intent.
        /// <br/>
        /// Use the `Intent` class to create an intent and then pass it to this function.
        /// <br/>
        /// This function will return <code>false</code> if no activity is available to handle the Intent
        /// or `true` if an activity was started.
        /// 		 
		/// </summary>
		/// <param name="intent"></param>
		/// <returns>`true` if the intent was handled and an activity started,	`false` if there was an error or no activity could handle the intent.</returns>
		bool StartActivity(Intent intent);



		/// <summary>
		/// Event for start activity result handling
		/// </summary>
		event ApplicationEventHandler OnActivityResult;

	}




}