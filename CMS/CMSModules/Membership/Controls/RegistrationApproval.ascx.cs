using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Configuration;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.WebAnalytics;
using CMS.MacroEngine;
using CMS.DataEngine;
using CMS.ExtendedControls;
using CMS.DocumentEngine;

public partial class CMSModules_Membership_Controls_RegistrationApproval : CMSUserControl
{
    #region "Variables"

    private bool mNotifyAdministrator = false;

    #endregion


    #region "Private properties"

    /// <summary>
    /// Gets or sets registered user.
    /// </summary>
    private UserInfo RegisteredUser
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets default url.
    /// </summary>
    private string DefaultUrl
    {
        get;
        set;
    }

    #endregion


    #region "Public properties"

    /// <summary>
    /// Gets or sets the value that indicates whether administrator should be informed about new user.
    /// </summary>
    public bool NotifyAdministrator
    {
        get
        {
            return mNotifyAdministrator || QueryHelper.GetBoolean("notifyadmin", false);
        }
        set
        {
            mNotifyAdministrator = value;
        }
    }


    /// <summary>
    /// Gets or sets the administrator e-mail address.
    /// </summary>
    public string AdministratorEmail
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets waiting for approval text.
    /// </summary>
    public string WaitingForApprovalText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets email address of sender.
    /// </summary>
    public string FromAddress
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets Successful Approval Text.
    /// </summary>
    public string SuccessfulApprovalText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets Unsuccessful Approval text.
    /// </summary>
    public string UnsuccessfulApprovalText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets ui deleted text.
    /// </summary>
    public string UserDeletedText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets the confirmation button text.
    /// </summary>
    public string ConfirmationButtonText
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets the confirmation button CSS class.
    /// </summary>
    public string ConfirmationButtonCssClass
    {
        get;
        set;
    }


    /// <summary>
    /// Messages placeholder
    /// </summary>
    public override MessagesPlaceHolder MessagesPlaceHolder
    {
        get
        {
            return plcMess;
        }
    }

    #endregion


    #region "Control events"

    /// <summary>
    /// Page Load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        Guid userGuid = QueryHelper.GetGuid("userguid", Guid.Empty);

        // If StopProcessing flag is set or userguid is empty, do nothing
        if (StopProcessing || (userGuid == Guid.Empty))
        {
            Visible = false;
            return;
        }

        // Validate hash
        if (!QueryHelper.ValidateHash("hash", "aliaspath", false))
        {
            URLHelper.Redirect(ResolveUrl("~/CMSMessages/Error.aspx?title=" + ResHelper.GetString("dialogs.badhashtitle") + "&text=" + ResHelper.GetString("dialogs.badhashtext")));
        }

        // Get registered user
        RegisteredUser = UserInfoProvider.GetUserInfoByGUID(userGuid);

        // Get default alias path where user will be redirected to
        string defaultAliasPath = SettingsKeyInfoProvider.GetStringValue(SiteContext.CurrentSiteName + ".CMSDefaultAliasPath");
        string url = DocumentURLProvider.GetUrl(defaultAliasPath);

        // Set default url
        DefaultUrl = ResolveUrl(DataHelper.GetNotEmpty(url, "~/"));

        bool controlPb = false;

        if (RequestHelper.IsPostBack())
        {
            Control pbCtrl = ControlsHelper.GetPostBackControl(Page);
            if (pbCtrl == btnConfirm)
            {
                controlPb = true;
            }
        }

        SetupControls(!controlPb);

        if (!controlPb)
        {
            CheckUserStatus();
        }
    }


    /// <summary>
    /// Click event of btnConfirm.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        CheckUserStatus();
        ConfirmUserRegistration();
    }

    #endregion


    #region "Private methods"

    /// <summary>
    /// Initialize controls properties.
    /// <param name="forceReload">Force reload</param>
    /// </summary>
    private void SetupControls(bool forceReload)
    {
        btnConfirm.CssClass = ConfirmationButtonCssClass;

        if (forceReload)
        {
            btnConfirm.Text = DataHelper.GetNotEmpty(ConfirmationButtonText, GetString("general.registration_confirmbutton"));
            lblInfo.Text = GetString("mem.reg.registration_confirmtext");
        }
    }


    /// <summary>
    /// Checks if user exists or if user has already been activated.
    /// </summary>
    private void CheckUserStatus()
    {
        // User was not found, probably late activation try
        if (RegisteredUser == null)
        {
            DisplayErrorMessage(UserDeletedText, GetString("mem.reg.UserDeletedText"));

            return;
        }

        // User has already been activated or she is been waiting for administrator approval
        if (RegisteredUser.UserEnabled || RegisteredUser.UserSettings.UserWaitingForApproval)
        {
            DisplayErrorMessage(UnsuccessfulApprovalText, GetString("mem.reg.UnsuccessfulApprovalText"));
        }
    }


    /// <summary>
    /// Confirms user registration.
    /// </summary>
    private void ConfirmUserRegistration()
    {
        // Approve user registration
        ApproveRegistration();

        // Log user activity
        LogActivity();
    }


    /// <summary>
    /// Approve user registration.
    /// </summary>
    private void ApproveRegistration()
    {
        string currentSiteName = SiteContext.CurrentSiteName;
        bool administrationApproval = SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSRegistrationAdministratorApproval");

        // Administrator approve is not required, enable user
        if (!administrationApproval)
        {
            ShowInformation(DataHelper.GetNotEmpty(SuccessfulApprovalText, GetString("mem.reg.succesfullapprovaltext")));

            // Get logon link if confirmation was successful
            string logonlink = SettingsKeyInfoProvider.GetStringValue(currentSiteName + ".CMSSecuredAreasLogonPage");
            lblInfo.Text = String.Format(GetString("memberhsip.logonlink"), ResolveUrl(DataHelper.GetNotEmpty(logonlink, "~/")));
            btnConfirm.Visible = false;

            // Enable user
            RegisteredUser.UserSettings.UserActivationDate = DateTime.Now;
            RegisteredUser.Enabled = true;

            // User is confirmed and enabled, could be logged into statistics
            AnalyticsHelper.LogRegisteredUser(currentSiteName, RegisteredUser);
        }
        // User must wait for administration approval
        else
        {
            ShowInformation(DataHelper.GetNotEmpty(WaitingForApprovalText, GetString("mem.reg.SuccessfulApprovalWaitingForAdministratorApproval")));

            // Mark for admin approval
            RegisteredUser.UserSettings.UserWaitingForApproval = true;

            // Display link to home page
            lblInfo.Text = String.Format(GetString("general.gotohomepage"), DefaultUrl);
            btnConfirm.Visible = false;
        }

        // Save changes
        UserInfoProvider.SetUserInfo(RegisteredUser);

        // Notify administrator if enabled and email confirmation is not required
        if ((!String.IsNullOrEmpty(AdministratorEmail)) && (administrationApproval || NotifyAdministrator))
        {
            SendEmailToAdministrator(administrationApproval);
        }
    }


    /// <summary>
    /// Log user activity.
    /// </summary>
    private void LogActivity()
    {
        // Create new activity for registered user
        Activity activity = new ActivityRegistration(RegisteredUser, DocumentContext.CurrentDocument, AnalyticsContext.ActivityEnvironmentVariables);
        if (activity.Data != null)
        {
            int contactId = QueryHelper.GetInteger("contactid", 0);
            if (contactId <= 0)
            {
                // Get contact for registered user
                contactId = ModuleCommands.OnlineMarketingGetUserLoginContactID(RegisteredUser);
            }

            // Log contact data
            activity.Data.ContactID = contactId;
            activity.CheckViewMode = false;
            activity.Log();
        }
    }


    /// <summary>
    /// Send e-mail to administrator about new registration.
    /// </summary>
    /// <param name="administrationApproval">Indicates if administration approval is required</param>
    private void SendEmailToAdministrator(bool administrationApproval)
    {
        EmailTemplateInfo template = null;

        if (administrationApproval)
        {
            template = EmailTemplateProvider.GetEmailTemplate("Registration.Approve", SiteContext.CurrentSiteName);
        }
        else
        {
            template = EmailTemplateProvider.GetEmailTemplate("Registration.New", SiteContext.CurrentSiteName);
        }

        if (template == null)
        {
            EventLogProvider.LogEvent(EventType.ERROR, "RegistrationForm", "GetEmailTemplate", eventUrl: RequestContext.RawURL);
        }
        else
        {
            // E-mail template ok
            string from = EmailHelper.GetSender(template, (!String.IsNullOrEmpty(FromAddress)) ? FromAddress : SettingsKeyInfoProvider.GetStringValue(SiteContext.CurrentSiteName + ".CMSNoreplyEmailAddress"));
            if (!String.IsNullOrEmpty(from))
            {
                // Prepare macro replacements
                string[,] replacements = new string[4, 2];
                replacements[0, 0] = "firstname";
                replacements[0, 1] = RegisteredUser.FirstName;
                replacements[1, 0] = "lastname";
                replacements[1, 1] = RegisteredUser.LastName;
                replacements[2, 0] = "email";
                replacements[2, 1] = RegisteredUser.Email;
                replacements[3, 0] = "username";
                replacements[3, 1] = RegisteredUser.UserName;

                // Set resolver
                MacroResolver resolver = MacroContext.CurrentResolver;
                resolver.SetNamedSourceData(replacements);
                resolver.Settings.EncodeResolvedValues = true;

                // Add user info data
                resolver.SetAnonymousSourceData(new object[1] { RegisteredUser });

                // Email message
                EmailMessage email = new EmailMessage();
                email.EmailFormat = EmailFormatEnum.Default;
                email.Recipients = AdministratorEmail;

                // Get e-mail sender and subject from template, if used
                email.From = from;
                email.Body = resolver.ResolveMacros(template.TemplateText);

                resolver.Settings.EncodeResolvedValues = false;
                email.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);

                string emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.EmailSubject"));
                email.Subject = resolver.ResolveMacros(emailSubject);
                email.CcRecipients = template.TemplateCc;
                email.BccRecipients = template.TemplateBcc;

                try
                {
                    EmailHelper.ResolveMetaFileImages(email, template.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                    // Send the e-mail immediately
                    EmailSender.SendEmail(SiteContext.CurrentSiteName, email, true);
                }
                catch
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "Membership", "RegistrationApprovalEmail");
                }
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, "RegistrationApproval", "EmailSenderNotSpecified");
            }
        }
    }


    /// <summary>
    /// Display error message, home page link and hide confirmation button.
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="defaultErrorMessage">Default error message, it is displayed if error message is empty</param>
    private void DisplayErrorMessage(object errorMessage, string defaultErrorMessage)
    {
        ShowError(DataHelper.GetNotEmpty(errorMessage, defaultErrorMessage));
        lblInfo.Text = String.Format(GetString("general.gotohomepage"), DefaultUrl);
        btnConfirm.Visible = false;
    }

    #endregion
}