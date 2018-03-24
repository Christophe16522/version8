using System;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CMS.ExtendedControls;
using CMS.FormControls;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Base;
using CMS.FormEngine;
using CMS.ExtendedControls.DragAndDrop;
using CMS.MacroEngine;
using CMS.DataEngine;

public partial class CMSAdminControls_UI_Macros_MacroRuleDesigner : FormEngineUserControl
{
    #region "Variables"

    private DragAndDropExtender extDragDrop = null;
    private int counter = 0;
    private static StringSafeDictionary<string> mRulesTooltips = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Returns table of rules tooltips (indexed by rule id).
    /// This needs to be preserved for correct view after postback.
    /// </summary>
    public static StringSafeDictionary<string> RulesTooltips
    {
        get
        {
            if (mRulesTooltips == null)
            {
                mRulesTooltips = new StringSafeDictionary<string>();
            }
            return mRulesTooltips;
        }
    }


    /// <summary>
    /// Returns the representation of the designer tree. It returns the root of the whole tree.
    /// </summary>
    public MacroRuleTree RuleTree
    {
        get
        {
            if (ViewState["RuleTree"] == null)
            {
                ViewState["RuleTree"] = new MacroRuleTree();
            }
            return (MacroRuleTree)ViewState["RuleTree"];
        }
        private set
        {
            ViewState["RuleTree"] = value;
        }
    }


    /// <summary>
    /// Gets or sets name(s) of the Macro rule category(ies) which should be displayed in Rule designer. Items should be separated by semicolon.
    /// </summary>
    public string RuleCategoryNames
    {
        get;
        set;
    }


    /// <summary>
    /// Determines which rules to display. 0 means all rules, 1 means only rules which does not require context, 2 only rules which require context.
    /// </summary>
    public int DisplayRuleType
    {
        get;
        set;
    }


    /// <summary>
    /// Determines whether the global rules are shown among with the specific rules defined in the RuleCategoryNames property.
    /// </summary>
    public bool ShowGlobalRules
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets the text which is displayed by default when there is no rule defined.
    /// </summary>
    public string DefaultConditionText
    {
        get;
        set;
    }


    /// <summary>
    /// Returns the resulting condition
    /// </summary>
    public override object Value
    {
        get
        {
            string error = this.RuleTree.ValidateParameters();
            if (!string.IsNullOrEmpty(error))
            {
                pnlMessagePlaceholder.ShowError(GetString("macros.macrorule.requiredparamsmissing"), null, null);
                throw new Exception(error);
            }

            string condition = GetCondition();
            return "Rule(\"" + MacroElement.EscapeSpecialChars(condition) + "\", \"" + MacroElement.EscapeSpecialChars(GetXML()) + "\")";
        }
        set
        {
            ParseFromExpression(ValidationHelper.GetString(value, ""));
        }
    }


    /// <summary>
    /// Returns whether the parameter should be shown.
    /// </summary>
    private bool ShowParameterEdit
    {
        get
        {
            return ValidationHelper.GetBoolean(hdnParamEditShown.Value, false);
        }
        set
        {
            hdnParamEditShown.Value = (value ? "1" : "0");
        }
    }

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Attach events
        btnAutoIndent.Click += btnAutoIndent_Click;
        btnDelete.Click += (btnDelete_Click);
        btnIndent.Click += btnIndent_Click;
        btnUnindent.Click += btnUnindent_Click;
        btnChangeOperator.Click += new EventHandler(btnChangeOperator_Click);
        btnChangeParameter.Click += new EventHandler(btnChangeParameter_Click);
        btnMove.Click += new EventHandler(btnMove_Click);
        btnCancel.Click += new EventHandler(btnCancel_Click);
        btnSetParameter.Click += new EventHandler(btnSetParameter_Click);
        btnViewCode.Click += btnViewCode_Click;
        btnAddClause.Click += new EventHandler(btnAddClause_Click);
        btnClearAll.Click += btnClearAll_Click;
        txtFilter.TextChanged += new EventHandler(btnFilter_Click);

        btnFilter.Text = GetString("general.filter");
        btnSetParameter.Text = GetString("general.ok");
        btnCodeOK.Text = GetString("general.ok");
        btnCancel.Text = GetString("general.cancel");
        btnIndent.ScreenReaderDescription = btnIndent.ToolTip = GetString("macros.macrorule.indent");
        btnUnindent.ScreenReaderDescription = btnUnindent.ToolTip = GetString("macros.macrorule.unindent");
        btnAutoIndent.ScreenReaderDescription = btnAutoIndent.ToolTip = GetString("macros.macrorule.autoindent");
        btnDelete.ScreenReaderDescription = btnDelete.ToolTip = GetString("general.delete");
        btnClearAll.ScreenReaderDescription = btnClearAll.ToolTip = GetString("macro.macrorule.clearall");
        btnViewCode.ScreenReaderDescription = btnViewCode.ToolTip = GetString("macros.macrorule.viewcode");

        btnIndent.OnClientClick = "if (isNothingSelected()) { alert(" + ScriptHelper.GetString(GetString("macros.macrorule.nothingselected")) + "); return false; }";
        btnUnindent.OnClientClick = "if (isNothingSelected()) { alert(" + ScriptHelper.GetString(GetString("macros.macrorule.nothingselected")) + "); return false; }";
        btnDelete.OnClientClick = "if (isNothingSelected()) { alert(" + ScriptHelper.GetString(GetString("macros.macrorule.nothingselected")) + "); return false; } else { if (!confirm('" + GetString("macros.macrorule.deleteconfirmation") + "')) { return false; }}";
        btnAutoIndent.OnClientClick = "if (!confirm(" + ScriptHelper.GetString(GetString("macros.macrorule.deleteautoindent")) + ")) { return false; }";
        btnClearAll.OnClientClick = "if (!confirm(" + ScriptHelper.GetString(GetString("macros.macrorule.clearall.confirmation")) + ")) { return false; }";

        lstRules.Attributes.Add("ondblclick", ControlsHelper.GetPostBackEventReference(btnAddClause, null));

        pnlViewCode.Visible = false;

        // Basic form
        formElem.SubmitButton.Visible = false;
        formElem.SiteName = SiteContext.CurrentSiteName;

        titleElem.TitleText = GetString("macros.macrorule.changeparameter");
        btnAddaClause.ToolTip = GetString("macros.macrorule.addclause");
        btnAddaClause.Click += btnAddClause_Click;

        // Drop cue
        Panel pnlCue = new Panel();
        pnlCue.ID = "pnlCue";
        pnlCue.CssClass = "MacroRuleCue";
        pnlCondtion.Controls.Add(pnlCue);

        pnlCue.Controls.Add(new LiteralControl("&nbsp;"));
        pnlCue.Style.Add("display", "none");

        // Create drag and drop extender
        extDragDrop = new DragAndDropExtender();
        extDragDrop.ID = "extDragDrop";
        extDragDrop.TargetControlID = pnlCondtion.ID;
        extDragDrop.DragItemClass = "MacroRule";
        extDragDrop.DragItemHandleClass = "MacroRuleHandle";
        extDragDrop.DropCueID = pnlCue.ID;
        extDragDrop.OnClientDrop = "OnDropRule";
        pnlCondtion.Controls.Add(extDragDrop);

        // Load the rule set
        if (!RequestHelper.IsPostBack())
        {
            if (ShowGlobalRules || !string.IsNullOrEmpty(RuleCategoryNames))
            {
                string where = (ShowGlobalRules ? "MacroRuleResourceName IS NULL OR MacroRuleResourceName = ''" : "");

                // Append rules module name condition
                if (!string.IsNullOrEmpty(RuleCategoryNames))
                {
                    bool appendComma = false;
                    StringBuilder sb = new StringBuilder();
                    string[] names = RuleCategoryNames.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string n in names)
                    {
                        string name = "'" + SqlHelper.GetSafeQueryString(n.Trim(), false) + "'";
                        if (appendComma)
                        {
                            sb.Append(",");
                        }
                        sb.Append(name);
                        appendComma = true;
                    }
                    where = SqlHelper.AddWhereCondition(where, "MacroRuleResourceName IN (" + sb.ToString() + ")", "OR");
                }

                // Append require context condition
                switch (DisplayRuleType)
                {
                    case 1:
                        where = SqlHelper.AddWhereCondition(where, "MacroRuleRequiresContext = 0", "AND");
                        break;

                    case 2:
                        where = SqlHelper.AddWhereCondition(where, "MacroRuleRequiresContext = 1", "AND");
                        break;
                }

                // Select only enabled rules
                where = SqlHelper.AddWhereCondition(where, "MacroRuleEnabled = 1");

                DataSet ds = MacroRuleInfoProvider.GetMacroRules(where, "MacroRuleDisplayName", 0, "MacroRuleID, MacroRuleDisplayName, MacroRuleDescription, MacroRuleRequiredData");
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    MacroResolver resolver = MacroResolverStorage.GetRegisteredResolver(ResolverName);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        bool add = true;
                        if (resolver != null)
                        {
                            // Check the required data, all specified data have to be present in the resolver
                            string requiredData = ValidationHelper.GetString(dr["MacroRuleRequiredData"], "");
                            if (!string.IsNullOrEmpty(requiredData))
                            {
                                string[] required = requiredData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var req in required)
                                {
                                    if (!resolver.IsDataItemAvailable(req))
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (add)
                        {
                            var ruleId = dr["MacroRuleID"].ToString();
                            ListItem item = new ListItem(dr["MacroRuleDisplayName"].ToString(), ruleId);
                            lstRules.Items.Add(item);

                            // Save the tooltip
                            RulesTooltips[ruleId] = ResHelper.LocalizeString(ValidationHelper.GetString(dr["MacroRuleDescription"], ""));
                        }
                    }
                }
                if (lstRules.Items.Count > 0)
                {
                    lstRules.SelectedIndex = 0;
                }
            }
        }

        // Make sure that one user click somewhere else than to any rule, selection will disappear
        pnlCondtion.Attributes["onclick"] = "if (!doNotDeselect && !isCTRL) { jQuery('.RuleSelected').removeClass('RuleSelected'); document.getElementById('" + hdnSelected.ClientID + "').value = ';'; }; doNotDeselect = false;";

        LoadFormDefinition(false);

        // Set the default button for parameter edit dialog so that ENTER key works to submit the parameter value
        pnlParameterPopup.DefaultButton = btnSetParameter.ID;

        // Ensure correct edit dialog show/hide (because of form controls which cause postback)
        btnSetParameter.OnClientClick = "HideParamEdit();";
        btnCancel.OnClientClick = "HideParamEdit();";
        if (ShowParameterEdit)
        {
            mdlDialog.Show();
        }

        if (!string.IsNullOrEmpty(hdnScroll.Value))
        {
            // Preserve scroll position
            ScriptHelper.RegisterStartupScript(this.Page, typeof(string), "MacroRulesScroll", "setTimeout('setScrollPosition()', 100);", true);
        }

        // Add tooltips to the rules in the list
        foreach (ListItem item in lstRules.Items)
        {
            if (RulesTooltips.ContainsKey(item.Value))
            {
                item.Attributes.Add("title", RulesTooltips[item.Value]);
            }
        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        RegisterScriptMethods();

        if (this.RuleTree.Children.Count > 0)
        {
            this.ltlText.Text = GetResultHTML(this.RuleTree);
        }
        else
        {
            if (string.IsNullOrEmpty(DefaultConditionText))
            {
                this.ltlText.Text = "<span class=\"MacroRuleInfo\">" + GetString("macros.macrorule.emptycondition") + "</span>";
            }
            else
            {
                this.ltlText.Text = DefaultConditionText;
            }
        }
    }


    /// <summary>
    /// Registers needed JS methods for operating the designer.
    /// </summary>
    private void RegisterScriptMethods()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(@"
var doNotDeselect = false;
function SelectRule(path, currentElem) {

    doNotDeselect = true;

    if (currentElem == null) {
        return;
    }

    var hidden = document.getElementById('", hdnSelected.ClientID, @"');
    if (hidden != null) {
        if (!isCTRL) {
            // Deselect all rules when CTRL is not pressed
            jQuery('.RuleSelected').removeClass('RuleSelected');
            hidden.value = '';
        }

        var orig = hidden.value;
        var newText = hidden.value.replace(';' + path + ';', ';');
        if (orig.length != newText.length) {
            // If the rule was present it means it was selected, so deselect the item
            currentElem.removeClass('RuleSelected');
        } else {
            // If the rule was not selected before, select it and add to the list of selected
            currentElem.addClass('RuleSelected');
            if (newText == '') {
                newText = ';' + path + ';';
            } else {            
                newText += path + ';';
            }
        }
        hidden.value = newText;
    }
}

function setScrollPosition() {
    var hdnScroll = document.getElementById('", hdnScroll.ClientID, @"');
    var scrollDiv = document.getElementById('scrollDiv');
    if ((hdnScroll != null) && (scrollDiv != null)) {
        if (hdnScroll.value != '') {
            scrollDiv.scrollTop = hdnScroll.value;
        }
    }
}

function isNothingSelected() {
    // If nothing is selected, do not allow to use buttons such as delete, indent, unindent
    var newText = document.getElementById('", hdnSelected.ClientID, @"').value;
    return (newText == '') || (newText == ';') || (newText == ';;');
}

");

        sb.Append(
@"
var isCTRL = false;
jQuery(document).keyup(function(event) {
    if (event.which == 17) {
        isCTRL = false;
    }  
}).keydown(function(event) {
    if (event.which == 17) {
       isCTRL = true;
    }  
});
");

        sb.Append(string.Format(
@"var targetPosition = new Array();
function OnDropRule(source, target) {{
    var item = target.get_droppedItem();
    var targetPos = target.get_position(); 

    var hidden = document.getElementById('{0}')
    if (hidden != null) {{
        hidden.value = item.id + ';' + targetPosition[targetPos];
        {1}; 
    }}
}}", hdnParam.ClientID, ControlsHelper.GetPostBackEventReference(btnMove, null)));

        sb.Append(
@"
if (window.recursiveDragAndDrop) {
    window.recursiveDragAndDrop = true;
}
if (window.lastDragAndDropBehavior) {
    lastDragAndDropBehavior._initializeDraggableItems();
}");

        sb.Append(
            @"
function ActivateBorder(elementId, className) {
  var e = document.getElementById(elementId);
  if (e != null) {
    e.className = e.className.replace(className, className + 'Active');
  }
}

function DeactivateBorder(elementId, className) {
  var e = document.getElementById(elementId);
  if (e != null) {
    e.className = e.className.replace(className + 'Active', className);
  }
}
");

        sb.Append(
@"function ChangeOperator(path, operator) {
    document.getElementById('", hdnOpSelected.ClientID, @"').value = path;
    document.getElementById('", hdnParam.ClientID, @"').value = operator;
    ", ControlsHelper.GetPostBackEventReference(btnChangeOperator, null), @"
}");

        sb.Append(
@"function ChangeParamValue(path, parameter) {
    document.getElementById('", hdnParamSelected.ClientID, @"').value = path;
    document.getElementById('", hdnParam.ClientID, @"').value = parameter;
    ", ControlsHelper.GetPostBackEventReference(btnChangeParameter, null), @"
}");

        sb.Append(
@"function InitDesignerAreaSize() {
    jQuery('#", pnlCondtion.ClientID, @"').height(document.body.clientHeight - 295);
    jQuery('#", lstRules.ClientID, @"').height(document.body.clientHeight - 287);
    jQuery('.add-clause button').css('margin-top', (document.body.clientHeight - 164) / 2);
}

jQuery(window).resize(InitDesignerAreaSize);
jQuery(document).ready(InitDesignerAreaSize);
");

        sb.Append(
@"function HideParamEdit() {
    document.getElementById('" + hdnParamEditShown.ClientID + @"').value = '0';
}
");

        sb.Append(
@"jQuery('#scrollDiv').scroll(function() {
  document.getElementById('" + hdnScroll.ClientID + @"').value = document.getElementById('scrollDiv').scrollTop;
});");
        ScriptHelper.RegisterJQuery(this.Page);
        ScriptHelper.RegisterClientScriptBlock(this.Page, typeof(string), "MacroRuleDesigner", ScriptHelper.GetScript(sb.ToString()));
    }

    #endregion


    #region "Sentences building"

    /// <summary>
    /// Renders complete rule.
    /// </summary>
    /// <param name="rule">Rule to render</param>
    private string GetResultHTML(MacroRuleTree rule)
    {
        StringBuilder sb = new StringBuilder();

        // Append operator
        if (rule.Position > 0)
        {
            bool isAnd = (rule.Operator == "&&");
            sb.Append("<div class=\"MacroRuleOperator\" style=\"padding-left: ", 15 * (rule.Level - 1), "px\" onclick=\"ChangeOperator('", rule.IDPath, "', '", (isAnd ? "||" : "&&"), "');\">", (isAnd ? "and" : "or"), "</div>");
        }

        if (rule.IsLeaf)
        {
            sb.Append("<div id=\"", rule.IDPath, "\" class=\"MacroRule\" style=\"padding-left: ", 15 * (rule.Level - 1), "px\">");

            // Register position to a JS hashtable (for drag and drop purposes)
            ScriptHelper.RegisterStartupScript(Page, typeof(string), "targetPosition" + counter, "targetPosition[" + counter++ + "] = '" + rule.Parent.IDPath + ";" + rule.Position + "';", true);

            sb.Append("<span id=\"ruleHandle" + rule.IDPath + "\"  class=\"MacroRuleHandle\">");
            string handleParams = "<span" + (rule.IsLeaf ? " onclick=\"SelectRule('" + rule.IDPath + "', jQuery(this).parent()); return false;\"" : "") + "onmousedown=\"return false;\" onmouseover=\"ActivateBorder('ruleText" + rule.IDPath + "', 'MacroRuleText');\" onmouseout=\"DeactivateBorder('ruleText" + rule.IDPath + "', 'MacroRuleText');\">";
            string text = handleParams.Replace("##ID##", "0") + HTMLHelper.HTMLEncode(rule.RuleText) + "</span>";
            if (rule.Parameters != null)
            {
                int i = 1;
                foreach (string key in rule.Parameters.Keys)
                {
                    MacroRuleParameter p = rule.Parameters[key];

                    string paramText = (string.IsNullOrEmpty(p.Text) ? p.DefaultText : p.Text.TrimStart('#'));
                    paramText = MacroRuleTree.GetParameterText(paramText, true, null, p.ApplyValueTypeConversion ? p.ValueType : "text");

                    text = Regex.Replace(text, "\\{" + key + "\\}", "</span><span class=\"MacroRuleParameter\" onclick=\"ChangeParamValue('" + rule.IDPath + "', " + ScriptHelper.GetString(key) + ");\">" + paramText + "</span>" + handleParams, CMSRegex.IgnoreCase);
                    i++;
                }
            }
            bool isSelected = hdnSelected.Value.Contains(";" + rule.IDPath + ";");
            sb.Append("<div id=\"ruleText", rule.IDPath, "\" class=\"MacroRuleText", (isSelected ? " RuleSelected" : ""), "\">", text, "</div>");
            sb.Append("</span>");
            sb.Append("</div>");
        }
        else
        {
            foreach (MacroRuleTree child in rule.Children)
            {
                sb.Append(GetResultHTML(child));
            }
        }

        return sb.ToString();
    }

    #endregion


    #region "Button operations

    protected void btnFilter_Click(object sender, EventArgs e)
    {
        string textToFind = txtFilter.Text.ToLowerCSafe();
        foreach (ListItem item in lstRules.Items)
        {
            item.Enabled = item.Text.ToLowerCSafe().Contains(textToFind);
        }
    }


    protected void btnClearAll_Click(object sender, EventArgs e)
    {
        this.RuleTree = new MacroRuleTree();
        hdnSelected.Value = "";
    }


    protected void btnViewCode_Click(object sender, EventArgs e)
    {
        this.viewCodeElem.Text = this.RuleTree.GetCondition();
        this.titleElem.TitleText = GetString("macros.macrorule.viewcodeheader");
        this.pnlViewCode.Visible = true;
        this.mdlDialog.Visible = true;
        this.mdlDialog.Show();
    }


    protected void btnMove_Click(object sender, EventArgs e)
    {
        string[] parts = hdnParam.Value.Split(';');
        if (parts.Length == 3)
        {
            int plusOne = parts[0].CompareToCSafe((string.IsNullOrEmpty(parts[1]) ? "" : parts[1] + ".") + parts[2]);
            plusOne = (plusOne < 0 ? 1 : 0);

            if (parts[1] == pnlCondtion.ClientID)
            {
                this.RuleTree.MoveNode(parts[0], "", ValidationHelper.GetInteger(parts[2], 0) + plusOne);
            }
            else
            {
                this.RuleTree.MoveNode(parts[0], parts[1], ValidationHelper.GetInteger(parts[2], 0) + plusOne);
            }

            // Clear selection
            hdnSelected.Value = ";";
        }
    }


    protected void btnChangeParameter_Click(object sender, EventArgs e)
    {
        LoadFormDefinition(true);

        hdnLastSelected.Value = hdnParamSelected.Value;
        hdnLastParam.Value = hdnParam.Value;
        hdnParamEditShown.Value = "1";

        this.titleElem.TitleText = GetString("macros.macrorule.changeparameter");
        this.pnlModalProperty.Visible = true;
        this.pnlFooter.Visible = true;
        this.mdlDialog.Visible = true;
        this.mdlDialog.Show();
    }


    protected void btnSetParameter_Click(object sender, EventArgs e)
    {
        var selected = GetSelected(hdnParamSelected.Value);
        if (selected != null)
        {
            string paramName = hdnParam.Value.ToLowerCSafe();

            var param = selected.Parameters[paramName];
            if (param != null)
            {
                if (formElem.ValidateData())
                {
                    // Load value from the form control
                    var ctrl = formElem.FieldControls[paramName];
                    if (ctrl != null)
                    {
                        // Convert values to EN culture
                        var dataType = ctrl.FieldInfo.DataType;

                        var convertedValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, dataType, ctrl.Value);

                        string value = ValidationHelper.GetString(convertedValue, "", CultureHelper.EnglishCulture);
                        string displayName = ctrl.ValueDisplayName;

                        if (String.IsNullOrEmpty(displayName))
                        {
                            displayName = value;
                            param.ApplyValueTypeConversion = true;
                        }

                        param.Value = value;
                        param.Text = displayName;
                        param.ValueType = dataType;
                    }

                    pnlModalProperty.Visible = false;
                    pnlFooter.Visible = false;
                }
                else
                {
                    pnlModalProperty.Visible = true;
                    pnlFooter.Visible = true;
                    mdlDialog.Visible = true;
                    mdlDialog.Show();
                }
            }
        }
    }


    protected void btnCancel_Click(object sender, EventArgs e)
    {
        pnlModalProperty.Visible = false;
        pnlFooter.Visible = false;
    }


    protected void btnChangeOperator_Click(object sender, EventArgs e)
    {
        MacroRuleTree selected = GetSelected(hdnOpSelected.Value);
        if (selected != null)
        {
            selected.Operator = hdnParam.Value;
            if ((selected.Position == 1) && (selected.Parent != null))
            {
                // Change operator to previous sibling if we are changing the first operator in the group
                // It's because if we switch those two it should have same opearators
                selected.Parent.Children[0].Operator = selected.Operator;
            }
        }
    }


    protected void btnUnindent_Click(object sender, EventArgs e)
    {
        List<MacroRuleTree> selected = GetSelected();
        hdnSelected.Value = ";";
        foreach (MacroRuleTree item in selected)
        {
            item.Unindent();
            hdnSelected.Value += item.IDPath + ";";
        }
    }


    protected void btnIndent_Click(object sender, EventArgs e)
    {
        List<MacroRuleTree> selected = GetSelected();
        hdnSelected.Value = ";";
        foreach (MacroRuleTree item in selected)
        {
            item.Indent();
            hdnSelected.Value += item.IDPath + ";";
        }
    }


    protected void btnDelete_Click(object sender, EventArgs e)
    {
        List<MacroRuleTree> selected = GetSelected();
        foreach (MacroRuleTree item in selected)
        {
            if (item.Parent != null)
            {
                item.Parent.RemoveNode(item.Position);
            }
        }
        hdnSelected.Value = "";
    }


    protected void btnAutoIndent_Click(object sender, EventArgs e)
    {
        MacroRuleTree.RemoveBrackets(this.RuleTree);
        this.RuleTree.AutoIndent();
    }


    protected void btnAddClause_Click(object sender, EventArgs e)
    {
        AddClause();
    }


    /// <summary>
    /// Adds a clause according to selected item.
    /// </summary>
    private void AddClause()
    {
        MacroRuleInfo rule = MacroRuleInfoProvider.GetMacroRuleInfo(ValidationHelper.GetInteger(lstRules.SelectedValue, 0));
        if (rule != null)
        {
            List<MacroRuleTree> selected = GetSelected();
            if (selected.Count == 1)
            {
                MacroRuleTree item = selected[0];
                if ((item != null) && (item.Parent != null))
                {
                    item.Parent.AddRule(rule, item.Position + 1);
                    return;
                }
            }

            // Add the rule at the root level, when no selected item
            this.RuleTree.AddRule(rule, this.RuleTree.Children.Count);
        }
    }

    #endregion


    #region "General methods"

    /// <summary>
    /// Returns true if at least one rule is selected.
    /// </summary>
    private bool IsAnyRuleSelected()
    {
        return this.hdnSelected.Value.Trim(';') != "";
    }


    /// <summary>
    /// Gets the object from its IDPath.
    /// </summary>
    /// <param name="idPath">IDPath of the rule</param>
    private MacroRuleTree GetSelected(string idPath)
    {
        if (!string.IsNullOrEmpty(idPath))
        {
            string[] parts = idPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            MacroRuleTree srcGroup = this.RuleTree;
            foreach (string posStr in parts)
            {
                int pos = ValidationHelper.GetInteger(posStr, 0);
                if (srcGroup.Children.Count > pos)
                {
                    srcGroup = srcGroup.Children[pos];
                }
            }

            return srcGroup;
        }
        return null;
    }


    /// <summary>
    /// Returns list of selected objects (gets IDPaths from hidden field).
    /// </summary>
    private List<MacroRuleTree> GetSelected()
    {
        List<MacroRuleTree> selected = new List<MacroRuleTree>();
        if (!string.IsNullOrEmpty(hdnSelected.Value))
        {
            string[] ids = hdnSelected.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // We need to sort the items, so as the items upper go sooner than items more down
            Array.Sort(ids);

            foreach (string id in ids)
            {
                selected.Add(GetSelected(id));
            }
        }
        return selected;
    }


    /// <summary>
    /// Loads the from definition from selected parameter into a BasicForm control.
    /// </summary>
    /// <param name="actual">If true, data from actual hiddens are loaded</param>
    private void LoadFormDefinition(bool actual)
    {
        MacroRuleTree selected = GetSelected((actual ? hdnParamSelected.Value : hdnLastSelected.Value));
        if (selected != null)
        {
            string paramName = (actual ? hdnParam.Value.ToLowerCSafe() : hdnLastParam.Value.ToLowerCSafe());
            MacroRuleParameter param = selected.Parameters[paramName];
            if (param != null)
            {
                FormInfo fi = new FormInfo(selected.RuleParameters);
                FormFieldInfo ffi = fi.GetFormField(paramName);
                if (ffi != null)
                {
                    fi = new FormInfo();
                    fi.AddFormItem(ffi);

                    // Add fake DisplayName field
                    FormFieldInfo displayName = new FormFieldInfo();
                    displayName.Visible = false;
                    displayName.Name = "DisplayName";
                    displayName.DataType = FieldDataType.Text;
                    fi.AddFormItem(displayName);

                    DataRow row = fi.GetDataRow().Table.NewRow();

                    if (ffi.AllowEmpty && String.IsNullOrEmpty(param.Value))
                    {
                        if (!DataTypeManager.IsString(TypeEnum.Field, ffi.DataType))
                        {
                            row[paramName] = DBNull.Value;
                        }
                    }
                    else
                    {
                        // Convert to a proper type
                        var val = DataTypeManager.ConvertToSystemType(TypeEnum.Field, ffi.DataType, param.Value, CultureHelper.EnglishCulture);
                        if (val != null)
                        {
                            row[paramName] = val;
                        }
                    }

                    formElem.DataRow = row;
                    formElem.FormInformation = fi;
                    formElem.ReloadData();
                }
            }
        }
    }

    #endregion


    #region "Data methods"

    /// <summary>
    /// Returns the condition of the whole rule.
    /// </summary>
    public string GetCondition()
    {
        return this.RuleTree.GetCondition();
    }


    /// <summary>
    /// Returns the XML of the designer.
    /// </summary>
    public string GetXML()
    {
        return this.RuleTree.GetXML();
    }


    /// <summary>
    /// Loads the designer from xml.
    /// </summary>
    public void LoadFromXML(string xml)
    {
        try
        {
            MacroRuleTree ruleTree = new MacroRuleTree();

            ruleTree.LoadFromXml(xml);
            ViewState["RuleTree"] = ruleTree;
        }
        catch { }
    }


    /// <summary>
    /// Extracs the condition from Rule method.
    /// </summary>
    public string ConditionFromExpression(string expression)
    {
        MacroExpression xml = null;
        try
        {
            xml = MacroExpression.ExtractParameter(expression, "rule", 1);
        }
        catch { }


        string user = null;
        if (xml == null)
        {
            return MacroSecurityProcessor.RemoveMacroSecurityParams(expression, out user);
        }
        else
        {
            // Returns first parameter of the expression
            return MacroSecurityProcessor.RemoveMacroSecurityParams(ValidationHelper.GetString(xml.Value, ""), out user);
        }
    }


    /// <summary>
    /// Parses the rule tree from Rule expression.
    /// </summary>
    public void ParseFromExpression(string expression)
    {
        MacroExpression xml = MacroExpression.ExtractParameter(expression, "rule", 1);
        if (xml != null)
        {
            // Load from the XML
            if (xml.Type == ExpressionType.Value)
            {
                LoadFromXML(xml.Value.ToString());
                return;
            }
        }

        // If something went wrong, assign null to the state variable
        ViewState["RuleTree"] = null;
    }

    #endregion
}