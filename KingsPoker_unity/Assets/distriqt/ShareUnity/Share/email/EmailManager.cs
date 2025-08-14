using System;

using distriqt.plugins.share.events;

namespace distriqt.plugins.share.email
{
    public delegate void EmailEventHandler(EmailEvent e);

	/// <summary>
	/// Defines the email sharing functionality
	/// </summary>
	public interface EmailManager
    {


		/// <summary>
        /// 
        /// </summary>
        event EmailEventHandler OnAttachmentError;

		/// <summary>
        /// 
        /// </summary>
        event EmailEventHandler OnCompose;


		/// <summary>
		/// This checks that the device you are using has the ability to send an email.
		/// Generally when this returns<code>false</code> it is because the device doesn't have a mail
		/// client or an email account setup on the device.
		/// <br/>
		///
		/// Use this to check if you should expect<code> sendMail</code> to complete successfully.
		/// <br/>
		/// 
		/// <strong>iOS</strong>: Double check in Settings / "Mail, Contacts ..." that there's a valid email account.
		/// <br/>
		///
		/// Please note that you should incorporate this check into your application and
		/// inform users if they don't have the ability to send email from their device.
		/// <br/>
		/// 
		/// true if email messaging is supported
		/// 
		/// </summary>
		bool IsMailSupported
		{
			get;
		}


		/// <summary>
		/// Sends an email using the native "compose mail" user interface. The parameters
		/// of the UI are prepopulated with the information passed into this function.
		/// <br/>
		///
		/// Recipients are specified as a comma separated string of email addresses, and
		/// can be an empty string if not required.
		/// <br/>
		///
		/// <strong>Supported OS:</strong> default, iOS, Android
		/// </summary>
		/// <param name="subject">The subject of the email to prepopulate</param>
		/// <param name="body">The body content of the email</param>
		/// <param name="toRecipients">The list of <strong>to</strong> addresses</param>
		bool SendMail(string subject, string body, string toRecipients);



		/// <summary>
		/// Sends an email using the native "compose mail" user interface. The parameters
		///	of the UI are prepopulated with the information passed into this function.
		/// <br/>
		///
		/// Recipients are specified as a comma separated string of email addresses, and
		///	can be an empty string if not required.
		///	<br/>
		///	
		/// <strong>Note:</strong> Attachments are not yet supported in the default implementation.
		///	<br/>
		/// 
		/// <strong>HTML Content</strong>
		/// <br/>
		/// The way the HTML content is rendered and the tags supported is greatly dependant
		/// on the email application used to send the email. The following list are the
		/// generallly accepted tags, there are a few more on iOS (including table support)
		/// but you are better off sticking to only using the following. If you require
		/// additional tags, we suggest you save your output as html and attach it as a
		/// file.
		/// <ul>
		///	<li>&lt;a href="..."&gt;</li>
		///	<li>&lt;b&gt;</li>
		///	<li>&lt;big&gt;</li>
		///	<li>&lt;blockquote&gt;</li>
		///	<li>&lt;br&gt;</li>
		///	<li>&lt;cite&gt;</li>
		///	<li>&lt;dfn&gt;</li>
		///	<li>&lt;div align="..."&gt;</li>
		///	<li>&lt;em&gt;</li>
		///	<li>&lt;font size="..." color="..." face="..."&gt;</li>
		///	<li>&lt;h1&gt;</li>
		///	<li>&lt;h2&gt;</li>
		///	<li>&lt;h3&gt;</li>
		///	<li>&lt;h4&gt;</li>
		///	<li>&lt;h5&gt;</li>
		///	<li>&lt;h6&gt;</li>
		///	<li>&lt;i&gt;</li>
		///	<li>&lt;img src="..."&gt;</li>
		///	<li>&lt;p&gt;</li>
		///	<li>&lt;small&gt;</li>
		///	<li>&lt;strike&gt;</li>
		///	<li>&lt;strong&gt;</li>
		///	<li>&lt;sub&gt;</li>
		///	<li>&lt;sup&gt;</li>
		///	<li>&lt;tt&gt;</li>
		///	<li>&lt;u&gt;</li>
		/// </ul>
		/// 
		/// </summary>
		/// 
		/// <param name="subject">The subject of the email to prepopulate</param>
		/// <param name="body">The body content of the email</param>
		/// <param name="toRecipients">The list of <strong>to</strong> addresses</param>
		/// <param name="ccRecipients">The list of <strong>cc</strong> addresses (carbon copy)</param>
		/// <param name="bccRecipients">The list of <strong>bcc</strong> addresses (blind carbon copy)</param>
		/// <param name="attachments">The attachments array. This should be an array of Attachment objects. Default: null i.e. no attachments</param>
		/// <param name="isHTML">If true the <strong>body</strong> string is rendered as HTML in the email content, if false it's treated as plain text.</param>
		/// <param name="useChooser">If <code>true</code> on Android a dialog will be displayed to allow the user to choose the mail application to use.</param>
		bool SendMailWithOptions(
			string subject,
			string body,
			string toRecipients,
			string ccRecipients = "",
			string bccRecipients = "",
			Attachment[] attachments = null,
			bool isHTML = true,
			bool useChooser = true);


	}

}