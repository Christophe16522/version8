using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.ExtendedControls;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;
using CMS.UIControls;

public partial class CMSAdminControls_UI_UniGrid_Controls_AdvancedExport : CMSUserControl, IPostBackEventHandler
{
    #region "Constants"

    // JavaScript function prefixes
    private const string VALIDATE_EXPORT_PREFIX = "ValidateExport_";

    private const string DEFAULT_SELECTION_PREFIX = "DefaultSelection_";

    private const string FIX_DIALOG_HEIGHT_PREFIX = "FixDialogHeight_";

    private const string SET_CHECKED_PREFIX = "SetChecked";

	private const string CLOSE_DIALOG = "closedialog";


    /// <summary>
    /// Short link to help page.
    /// </summary>
    private const string HELP_TOPIC_LINK = "xopP";

    #endregion


    #region "Variables"

	private UniGridExportHelper mUniGridExportHelper;

	private string mCurrentDelimiter;

	private bool mAlertAdded;

	private bool mControlLoaded;

    #endregion


    #region "Enumerations"

    /// <summary>
    /// CSV delimiter.
    /// </summary>
    protected enum Delimiter
    {
        Comma,
        Semicolon
    }

    #endregion


    #region "Properties"

    /// <summary>
    /// Holds an instance of the UniGrid control.
    /// </summary>
    public UniGrid UniGrid
    {
        get;
        set;
    }


    /// <summary>
    /// Currently selected format (in dropdown list).
    /// </summary>
    protected DataExportFormatEnum CurrentFormat
    {
        get
        {
            return (DataExportFormatEnum)Enum.Parse(typeof(DataExportFormatEnum), drpExportTo.SelectedItem.Value);
        }
    }


    /// <summary>
    /// Currently selected delimiter (in dropdown list).
    /// </summary>
    protected string CurrentDelimiter
    {
        get
        {
            if (string.IsNullOrEmpty(mCurrentDelimiter))
            {
                ListItem delimiterItem = drpDelimiter.SelectedItem;
                if ((delimiterItem != null) && !string.IsNullOrEmpty(delimiterItem.Value))
                {
                    // Parse delimiter from drop down list
                    Delimiter delimiter = (Delimiter)Enum.Parse(typeof(Delimiter), delimiterItem.Value);
                    switch (delimiter)
                    {
                        case Delimiter.Comma:
                            mCurrentDelimiter = ",";
                            break;

                        case Delimiter.Semicolon:
                            mCurrentDelimiter = ";";
                            break;
                    }
                }
                if (string.IsNullOrEmpty(mCurrentDelimiter))
                {
                    mCurrentDelimiter = CultureHelper.PreferredUICultureInfo.TextInfo.ListSeparator;
                }
            }
            return mCurrentDelimiter;
        }
    }


    /// <summary>
    /// Gets export helper for UniGrid.
    /// </summary>
    private UniGridExportHelper UniGridExportHelper
    {
        get
        {
            if (mUniGridExportHelper == null)
            {
                mUniGridExportHelper = new UniGridExportHelper(UniGrid);
                mUniGridExportHelper.Error += mUniGridExportHelper_Error;
                mUniGridExportHelper.MacroResolver = MacroContext.CurrentResolver;
            }
            return mUniGridExportHelper;
        }
    }


    /// <summary>
    /// Gets or sets identifier of current dialog (in case of more dialogs).
    /// </summary>
    private string CurrentDialogID
    {
        get
        {
            return ValidationHelper.GetString(ViewState["CurrentDialogID"], string.Empty);
        }
        set
        {
            ViewState["CurrentDialogID"] = value;
        }
    }


    /// <summary>
    /// Gets or sets current modal dialog ID (in case of more popups).
    /// </summary>
    private string CurrentModalID
    {
        get
        {
            return ValidationHelper.GetString(ViewState["CurrentModalID"], string.Empty);
        }
        set
        {
            ViewState["CurrentModalID"] = value;
        }
    }


    /// <summary>
    /// Gets current dialog control (in case of more dialogs).
    /// </summary>
    private Panel CurrentDialog
    {
        get
        {
            return (Panel)FindControl(CurrentDialogID);
        }
    }


    /// <summary>
    /// Gets current modal control (in case of more popups).
    /// </summary>
    private ModalPopupDialog CurrentModal
    {
        get
        {
            return (ModalPopupDialog)FindControl(CurrentModalID);
        }
    }


    /// <summary>
    /// If true, control does not process the data.
    /// </summary>
    public override bool StopProcessing
    {
        get
        {
            return base.StopProcessing;
        }
        set
        {
            base.StopProcessing = value;
            orderByElem.StopProcessing = value;
            advancedExportTitle.StopProcessing = value;
            mdlAdvancedExport.StopProcessing = value;
        }
    }

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        StopProcessing = !UniGrid.ShowActionsMenu;
        SetupControl();
    }


    protected void Page_PreRender(object sender, EventArgs e)
    {
        SetupControl();

        if (!StopProcessing)
        {
            ScriptHelper.RegisterJQueryDialog(Page);

            // Register control scripts
            StringBuilder exportScript = new StringBuilder();
            exportScript.Append(@"
function UG_Export_", UniGrid.ClientID, @"(format, param)
{
    document.getElementById('", hdnParameter.ClientID, @"').value = format;
    if(format == 'advancedexport')
    {",
                                ControlsHelper.GetPostBackEventReference(this, "##PARAM##").Replace("'##PARAM##'", "format"), @";
    }
    else
    {",
                                ControlsHelper.GetPostBackEventReference(btnFullPostback, "", false, false), @";
    }
}");

            ScriptHelper.RegisterStartupScript(pnlUpdate, typeof(string), "exportScript_" + ClientID, ScriptHelper.GetScript(exportScript.ToString()));

			if (Visible && pnlUpdate.Visible && !ShouldCloseDialog())
            {
                if (RequestHelper.IsPostBack() && (CurrentModal != null))
                {
                    // Register scripts shared across multiple advanced export controls
                    StringBuilder sharedExportScript = new StringBuilder();
                    sharedExportScript.Append(@"
function ", SET_CHECKED_PREFIX, @"(id, checked)
{
    $j('#' + id + ' :checkbox').attr('checked', checked);
    return false;
}
");
                    StringBuilder modalSupportScripts = new StringBuilder();
                    // Register script for default column selection
                    modalSupportScripts.Append(@"
function ", DEFAULT_SELECTION_PREFIX, chlColumns.ClientID, @"()
{
    var checkBoxes = $j(""input[type='checkbox']"",'#", chlColumns.ClientID, @"');
    var defSelStr = document.getElementById('", hdnDefaultSelection.ClientID, @"').value;
    var defaultSelection = defSelStr.split(',');
    for (var i = 0; i < checkBoxes.length; i++)
    {
        var indexOfChk = $j.inArray(i.toString(), defaultSelection);        
        $j(checkBoxes[i]).attr('checked', (indexOfChk > -1));
    }
    return false;  
}");
                    // Register script for keeping correct dialog dimensions
                    modalSupportScripts.Append(@"
var numVal = 'none';
var colVal = 'none';
function ", FIX_DIALOG_HEIGHT_PREFIX, ClientID, @"()
{
    var numberValidator = document.getElementById('", revRecords.ClientID, @"');
    var columnValidator = document.getElementById('", cvColumns.ClientID, @"');
    if((numberValidator != 'undefined') && (columnValidator != 'undefined') && (numberValidator != null) && (columnValidator != null))
    {
        var process = false;
        if(numVal != numberValidator.style.display)
        {
            process = true;
        }
        numVal = numberValidator.style.display;
        if(colVal != columnValidator.style.display)
        {
            process = true;
        }
        colVal = columnValidator.style.display;
        if(process)
        {
            resizableDialog = true;
            keepDialogAccesible('", mdlAdvancedExport.ClientID, @"');
        }
    }
    setTimeout('", FIX_DIALOG_HEIGHT_PREFIX, ClientID, @"()',500);
}");
                    // Register column selection validation script
                    modalSupportScripts.Append(@"
function ", VALIDATE_EXPORT_PREFIX, ClientID, @"(source, arguments)
{
    var checked = $j(""input[type='checkbox']:checked"",'#", chlColumns.ClientID, @"');
    arguments.IsValid = (checked.length > 0);
}
");
                    ScriptHelper.RegisterStartupScript(pnlUpdate, typeof(string), "sharedExportScript", ScriptHelper.GetScript(sharedExportScript.ToString()));
                    ScriptHelper.RegisterStartupScript(pnlUpdate, typeof(string), "modalSupportScripts_" + ClientID, ScriptHelper.GetScript(modalSupportScripts.ToString()));
                    ScriptHelper.RegisterStartupScript(this, typeof(string), "fixDialogHeight" + ClientID, ScriptHelper.GetScript("setTimeout('" + FIX_DIALOG_HEIGHT_PREFIX + ClientID + "()',500);"));

                    // Show popup after postback
                    CurrentModal.Show();
                }
            }
            pnlAdvancedExport.EnableViewState = (CurrentDialog == pnlAdvancedExport);
        }
    }

    #endregion


    #region "Button handling"

    /// <summary>
    /// When dialog's export button is clicked.
    /// </summary>
    protected void btnExport_Click(object sender, EventArgs e)
    {
        try
        {
            DataExportFormatEnum format = (DataExportFormatEnum)Enum.Parse(typeof(DataExportFormatEnum), drpExportTo.SelectedItem.Value);
            ExportData(format);
        }
        catch (Exception ex)
        {
            AddAlert(GetString("general.erroroccurred") + " " + ex.Message);
        }
    }


    /// <summary>
    /// When dialog's preview export button is clicked.
    /// </summary>
    protected void btnPreview_Click(object sender, EventArgs e)
    {
        try
        {
            DataExportFormatEnum format = (DataExportFormatEnum)Enum.Parse(typeof(DataExportFormatEnum), drpExportTo.SelectedItem.Value);
            ExportData(format, 100);
        }
        catch (Exception ex)
        {
            AddAlert(GetString("general.erroroccurred") + " " + ex.Message);
        }
    }


    /// <summary>
    /// When postback is invoked to perform direct export.
    /// </summary>
    protected void btnFullPostback_Click(object sender, EventArgs e)
    {
        // Parse format to export
        string parameter = ValidationHelper.GetString(hdnParameter.Value, string.Empty);
        try
        {
            DataExportFormatEnum format = (DataExportFormatEnum)Enum.Parse(typeof(DataExportFormatEnum), parameter);
            ExportData(format);
        }
        catch (Exception ex)
        {
            AddAlert(GetString("general.erroroccurred") + " " + ex.Message);
        }
    }


    /// <summary>
    /// When export format is changed (in dropdown list).
    /// </summary>
    protected void drpExportTo_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitializeDelimiter();
        InitializeExportHeader();
    }


    /// <summary>
    /// When raw data checkbox changes its checked state.
    /// </summary>
    protected void chkExportRawData_CheckedChanged(object sender, EventArgs e)
    {
        InitializeColumns(true);
        InitializeOrderBy(true);
    }

    #endregion


    #region "IPostBackEventHandler Members"

    /// <summary>
    /// Handles postbacks invoked upon this control.
    /// </summary>
    /// <param name="eventArgument">Argument that goes with postback</param>
    public void RaisePostBackEvent(string eventArgument)
    {
        if (!string.IsNullOrEmpty(eventArgument))
        {
            try
            {
                // Parse event argument
                switch (eventArgument)
                {
	                case "advancedexport":
		                pnlAdvancedExport.Visible = true;
		                ShowPopup(pnlAdvancedExport, mdlAdvancedExport);
		                InitializeAdvancedExport();
		                break;
					case CLOSE_DIALOG:
		                HideCurrentPopup();
		                break;
                }
            }
            catch (Exception ex)
            {
                AddAlert(GetString("general.erroroccurred") + " " + ex.Message);
            }
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Sets the control up
    /// </summary>
    private void SetupControl()
    {
	    if (mControlLoaded || StopProcessing)
	    {
		    return;
	    }

	    // Register full postback buttons for direct and advanced export
	    ControlsHelper.RegisterPostbackControl(btnExport);
	    ControlsHelper.RegisterPostbackControl(btnPreview);

	    // Initialize page title
	    advancedExportTitle.TitleText = GetString("export.advancedexport");
	    advancedExportTitle.ShowFullScreenButton = false;
		advancedExportTitle.SetCloseJavaScript(ControlsHelper.GetPostBackEventReference(this, CLOSE_DIALOG) + "; return false;");

	    // Initialize help icon
	    advancedExportTitle.IsDialog = true;
	    advancedExportTitle.HelpTopicName = HELP_TOPIC_LINK;

	    // Initialize column-selecting buttons
	    btnSelectAll.OnClientClick = "return " + SET_CHECKED_PREFIX + "('" + chlColumns.ClientID + "' , true);";
	    btnDeselectAll.OnClientClick = "return " + SET_CHECKED_PREFIX + "('" + chlColumns.ClientID + "' , false);";
	    btnDefaultSelection.OnClientClick = "return " + DEFAULT_SELECTION_PREFIX + chlColumns.ClientID + "();";

	    lblCurrentPageOnly.ToolTip = GetString("export.currentpagetooltip");
	    chkCurrentPageOnly.ToolTip = GetString("export.currentpagetooltip");

	    // Set up validator
	    string validationGroup = "advancedExport_" + ClientID;

	    revRecords.ValidationGroup = validationGroup;
	    revRecords.ErrorMessage = GetString("export.validinteger");
	    revRecords.ValidationExpression = "^\\d{1,9}$";

	    cvColumns.ValidationGroup = validationGroup;
	    cvColumns.ClientValidationFunction = VALIDATE_EXPORT_PREFIX + ClientID;
	    cvColumns.ErrorMessage = GetString("export.selectcolumns");

	    btnExport.ValidationGroup = validationGroup;
	    btnExport.OnClientClick = ScriptHelper.GetDisableProgressScript();

	    btnPreview.ValidationGroup = validationGroup;
	    btnPreview.OnClientClick = ScriptHelper.GetDisableProgressScript();

	    // Initialize
	    if (!MembershipContext.AuthenticatedUser.IsGlobalAdministrator)
	    {
            // Make the dialog wider when OrderBy controls are displayed
            pnlAdvancedExport.Width = new Unit("1100px");
		    InitializeOrderBy(false);
	    }
	    mControlLoaded = true;
    }


	private bool ShouldCloseDialog()
	{
		return Request.Params.Get("__EVENTARGUMENT") == CLOSE_DIALOG;
	}


	/// <summary>
    /// Handles possible errors during export
    /// </summary>
    /// <param name="customMessage">Message set when error occurs</param>
    /// <param name="exception">Original exception</param>
    protected void mUniGridExportHelper_Error(string customMessage, Exception exception)
    {
        AddAlert(customMessage);
    }


    /// <summary>
    /// Exports data based on given format.
    /// </summary>
    /// <param name="format">Format to export data in</param>
    /// <param name="forceMaxItems">Max items count (for preview export)</param>
    private void ExportData(DataExportFormatEnum format, int? forceMaxItems = null)
    {
        UniGridExportHelper.ExportRawData = chkExportRawData.Checked;
        UniGridExportHelper.CSVDelimiter = CurrentDelimiter;
        UniGridExportHelper.GenerateHeader = chkExportHeader.Checked;
        UniGridExportHelper.CurrentPageOnly = chkCurrentPageOnly.Checked;
        UniGridExportHelper.Records = ValidationHelper.GetInteger(txtRecords.Text, -1);

        // Preview export
        if (forceMaxItems != null)
        {
            int limit = 0;
            if (UniGridExportHelper.CurrentPageOnly)
            {
                limit = UniGrid.Pager.DisplayPager ? UniGrid.Pager.CurrentPageSize : 0;
            }
            else
            {
                limit = UniGridExportHelper.Records;
            }
            if ((limit >= forceMaxItems) || (limit <= 0))
            {
                UniGridExportHelper.Records = forceMaxItems.Value;
                UniGridExportHelper.CurrentPageOnly = false;
            }
        }

        UniGridExportHelper.UseGridFilter = chkUseGridFilter.Checked;
        // Get order by clause from correct control
        if (MembershipContext.AuthenticatedUser.IsGlobalAdministrator)
        {
            UniGridExportHelper.OrderBy = TrimExtendedTextAreaValue(txtOrderBy.Text);
        }
        else
        {
            UniGridExportHelper.OrderBy = ValidationHelper.GetString(orderByElem.Value, null);
        }
        UniGridExportHelper.WhereCondition = TrimExtendedTextAreaValue(txtWhereCondition.Text);
        UniGridExportHelper.Columns = GetSelectedColumns();
        if (IsCMSDesk)
        {
            UniGridExportHelper.SiteName = SiteContext.CurrentSiteName;
        }
        UniGridExportHelper.ExportData(format, Page.Response);
    }


    /// <summary>
    /// Extracts list of columns from checkbox list.
    /// </summary>
    /// <returns>List of columns selected to be exported</returns>
    private List<string> GetSelectedColumns()
    {
        List<string> exportedColumns = new List<string>();
        for (int i = 0; i < chlColumns.Items.Count; i++)
        {
            ListItem column = chlColumns.Items[i];
            if (column.Selected)
            {
	            // Get correct set of selected columns
	            string col = chkExportRawData.Checked ? UniGridExportHelper.AvailableColumns[i] : i.ToString();
	            if (!string.IsNullOrEmpty(col))
                {
                    exportedColumns.Add(col);
                }
            }
        }
        return exportedColumns;
    }


    /// <summary>
    /// Trims values returned by extended text area.
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <returns>Text without line breaks at the end</returns>
    private string TrimExtendedTextAreaValue(string text)
    {
		if (!string.IsNullOrEmpty(text))
		{
			while (text.EndsWithCSafe("\n") || text.EndsWithCSafe("\r"))
			{
				text = text.TrimEnd('\n').TrimEnd('\r');
			}
		}
		return text;
    }


    /// <summary>
    /// Initializes advanced export dialog.
    /// </summary>
    private void InitializeAdvancedExport()
    {
        // Initialize dropdown lists
        drpExportTo.Items.Clear();
        ControlsHelper.FillListControlWithEnum<DataExportFormatEnum>(drpExportTo, "export");
        drpDelimiter.Items.Clear();
        ControlsHelper.FillListControlWithEnum<Delimiter>(drpDelimiter, "export");
        // Initialize rest of dialog
        InitializeDelimiter();
        InitializeExportHeader();
        InitializeColumns(false);
        plcWhere.Visible = MembershipContext.AuthenticatedUser.IsGlobalAdministrator;
        plcExportRawData.Visible = MembershipContext.AuthenticatedUser.IsGlobalAdministrator;
        orderByElem.Visible = !MembershipContext.AuthenticatedUser.IsGlobalAdministrator;
        txtOrderBy.Visible = MembershipContext.AuthenticatedUser.IsGlobalAdministrator;
        btnDefaultSelection.Visible = MembershipContext.AuthenticatedUser.IsGlobalAdministrator;
    }


    /// <summary>
    /// Sets visibility of delimiter dropdown list.
    /// </summary>
    private void InitializeDelimiter()
    {
        plcDelimiter.Visible = (CurrentFormat == DataExportFormatEnum.CSV);
    }


    /// <summary>
    /// Sets visibility of export header placeholder.
    /// </summary>
    private void InitializeExportHeader()
    {
        plcExportHeader.Visible = (CurrentFormat != DataExportFormatEnum.XML);
    }


    /// <summary>
    /// Initializes columns in order by selector.
    /// </summary>
    /// <param name="force">Whether to force loading</param>
    private void InitializeOrderBy(bool force)
    {
        if (force)
        {
            orderByElem.Columns.Clear();
        }
        if (orderByElem.Columns.Count == 0)
        {
            if (chkExportRawData.Checked)
            {
                orderByElem.Columns.AddRange(from c in UniGridExportHelper.AvailableColumns select new ListItem(c, c));
            }
            else
            {
                orderByElem.Columns.AddRange(from f in UniGridExportHelper.BoundFields where (f.DataField != UniGrid.ALL && !string.IsNullOrEmpty(f.DataField) && IsColumnAvailable(f.DataField)) select new ListItem(f.HeaderText, f.DataField));
            }
            orderByElem.ReloadData();
        }
    }


    /// <summary>
    /// Determines whether UniGrid's info object contains given column.
    /// </summary>
    /// <param name="field">Column to check</param>
    /// <returns>TRUE if column is available in info object (or object type is not used)</returns>
    private bool IsColumnAvailable(string field)
    {
        if (!string.IsNullOrEmpty(UniGrid.ObjectType))
        {
            // When loading using object type
            if (UniGrid.InfoObject != null)
            {
                return UniGrid.InfoObject.ColumnNames.Contains(field);
            }
        }
        else
        {
            // For other types of loading (cannot determine whether column is available)
            return true;
        }
        return false;
    }


    /// <summary>
    /// Initializes columns checkboxlist (and default selection JS array).
    /// </summary>
    /// <param name="force">Whether to force loading</param>
    private void InitializeColumns(bool force)
    {
        string defaultSelection = string.Empty;
        if (force)
        {
            chlColumns.Items.Clear();
        }
        if (chlColumns.Items.Count == 0)
        {
            // Initialize column selector
            if (chkExportRawData.Checked)
            {
                // Using raw db columns
                for (int i = 0; i < UniGridExportHelper.AvailableColumns.Count; i++)
                {
                    string col = UniGridExportHelper.AvailableColumns[i];
                    if (AddColumn(i, col, (UniGridExportHelper.BoundFields.FirstOrDefault(bf => bf.DataField == col) != null)))
                    {
                        defaultSelection += i + ",";
                    }
                }
            }
            else
            {
                // Using UI columns
                for (int i = 0; i < UniGridExportHelper.BoundFields.Count; i++)
                {
                    BoundField field = UniGridExportHelper.BoundFields[i];
                    if (AddColumn(i, field.HeaderText, true))
                    {
                        defaultSelection += i + ",";
                    }
                }
            }
        }

        hdnDefaultSelection.Value = defaultSelection.TrimEnd(',');
    }


    /// <summary>
    /// Adds column to checkbox list.
    /// </summary>
    /// <param name="i">Index of a column (used as value)</param>
    /// <param name="text">Text of a column (used as caption) - value is required</param>
    /// <param name="selected">Whether the column is selected (checked)</param>
    /// <returns>Whether the column should be listed as selected by default</returns>
    private bool AddColumn(int i, string text, bool selected)
    {
        if (!string.IsNullOrEmpty(text))
        {
            ListItem chkCol = new ListItem();
            chkCol.Text = text;
            chkCol.Value = i.ToString();
            chkCol.Selected = selected;
            chlColumns.Items.Add(chkCol);
            return selected;
        }
        return false;
    }


    /// <summary>
    /// Performs actions necessary to show the popup dialog.
    /// </summary>
    /// <param name="dialogControl">New dialog control</param>
    /// <param name="modalPopup">Modal control</param>
    private void ShowPopup(Control dialogControl, ModalPopupDialog modalPopup)
    {
        // Set new identifiers
        CurrentModalID = modalPopup.ID;
        CurrentDialogID = dialogControl.ID;

        if ((CurrentModal != null) && (CurrentDialog != null))
        {
            // Enable dialog control's viewstate and visibility
            CurrentDialog.EnableViewState = true;
            CurrentDialog.Visible = true;

            // Show modal popup
            CurrentModal.Show();
        }
    }


    /// <summary>
    /// Performs actions necessary to hide popup dialog.
    /// </summary>
    private void HideCurrentPopup()
    {
        if ((CurrentModal != null) && (CurrentDialog != null))
        {
            // Hide modal dialog
            CurrentModal.Hide();

            // Reset dialog control's viewstate and visibility
            CurrentDialog.EnableViewState = false;
            CurrentDialog.Visible = false;
        }

        // Reset identifiers
        CurrentModalID = null;
        CurrentDialogID = null;
    }


    /// <summary>
    /// Adds alert script to the page.
    /// </summary>
    /// <param name="message">Message to show</param>
    private void AddAlert(string message)
    {
        if (!mAlertAdded)
        {
            AddScript(ScriptHelper.GetScript("setTimeout(function() {" + ScriptHelper.GetAlertScript(message, false) + "}, 50);"));
            mAlertAdded = true;
        }
    }


    /// <summary>
    /// Adds script to the page.
    /// </summary>
    /// <param name="script">Script to add</param>
    private void AddScript(string script)
    {
        ScriptHelper.RegisterStartupScript(this, typeof(string), script.GetHashCode().ToString(), script);
    }

    #endregion
}