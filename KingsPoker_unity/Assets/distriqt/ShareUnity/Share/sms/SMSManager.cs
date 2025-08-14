using System;

using distriqt.plugins.share.events;

namespace distriqt.plugins.share.sms
{
	public delegate void SMSEventHandler(SMSEvent e);

	/// <summary>
	/// Access to the SMS functionality
	/// </summary>	
	public interface SMSManager
    {
		/// <summary>
		/// An SMS has been received
		/// </summary>
		event SMSEventHandler OnSMSReceived;

		/// <summary>
		/// The SMS has been sent
		/// </summary>
		event SMSEventHandler OnSMSSent;

		/// <summary>
		/// The user cancelled the SMS send operation
		/// </summary>
		event SMSEventHandler OnSMSCancelled;

		/// <summary>
        /// There was an error sending the SMS. error may contain additional information
		/// </summary>
		event SMSEventHandler OnSMSSentError;

		/// <summary>
        /// Dispatched when the SMS delivery report was received successfully indicating the SMS was delivered.
        /// <br/>
        /// Android only
		/// </summary>
		event SMSEventHandler OnSMSDelivered;

		/// <summary>
        /// Dispatched when the SMS delivery report was not received successfully
        /// indicating the SMS was not delivered or if delivery could not be determined.
        /// <br/>
        /// Android only
		/// </summary>
		event SMSEventHandler OnSMSNotDelivered;


		//
		//	SIM CARDS
		//

		/// <summary>
		/// Retrieve a list of the available subscriptions (sim cards).
		/// <strong>Android</strong>
		/// Supported on Android API v22 and higher.
		/// <br/>
		/// Requires the <code>READ_PHONE_STATE</code> permission to be added to your application manifest additions:
		/// <br/>
		/// <pre>
		/// &lt;uses-permission android:name="android.permission.READ_PHONE_STATE" /&gt;
		/// </pre>
		/// </summary>
		/// <returns>Array of <code>SubscriptionInfo</code> objects, empty if not supported.</returns>
		SubscriptionInfo[] GetSubscriptions();




		//
		//  SMS FUNCTIONALITY
		//


		/// <summary>
		/// Returns true if the current device supports SMS functionality
		/// </summary>
		bool IsSMSSupported
		{
			get;
		}


		/// <summary>
        /// Sends an sms directly without any user interface. This is only available on Android
        /// devices.iOS must use the UI based functionality.
        /// <br/>
        /// <strong>Android</strong>
        /// <br/>
        /// Requires the SEND_SMS permission to be added to your application manifest additions:
        /// <pre>
        /// &lt;uses-permission android:name="android.permission.SEND_SMS" /&gt;
        /// </pre>
        /// 
		/// </summary>
		/// <param name="sms">The SMS message to send</param>
		/// <param name="subscriptionId">The identifier of a subscription to use to send the SMS, uses the default if not specified</param>
		/// <returns>true if successfull and false otherwise</returns>
		bool SendSMS(SMS sms, int subscriptionId = -1);


		/// <summary>
		/// Send an SMS by opening up the user interface and allowing the user
		/// to edit the message before sending.
		/// <br/>
		/// Firstly construct an SMS to send:
		/// <br/>
		/// <listing>
		/// SMS sms = new SMS();
		/// sms.address = "0444444444"; // The destination phone number
		/// sms.message = "SMS Message from actionscript"; // The message to send
		/// </listing>
		/// <br/>
		///
		/// Then pass this SMS to the parameter of this function.
		/// The UI will be populated with the above information
		/// 
		/// </summary>
		/// <param name="sms">The SMS message to send</param>
		/// <param name="useChooser">Android only: If true the user will be presented a choice on the application to use otherwise the default SMS application will be used</param>
		/// <returns>true if successfull and false otherwise</returns>
		bool SendSMSWithUI(SMS sms, bool useChooser = false);


	}

}