using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Controls.Configuration;
using CMS.DataEngine;
using CMS.ExtendedControls;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Reporting;
using CMS.Synchronization;
using CMS.UIControls;

public partial class CMSModules_Reporting_Tools_ReportGraph_Edit : CMSReportingModalPage
{
    #region "Constants"

    /// <summary>
    /// Short link to help page.
    /// </summary>
    private const string HELP_TOPIC_LINK = "zoBG";

    #endregion


    #region "Variables"

    protected ReportInfo mReportInfo = null;
    protected ReportGraphInfo mReportGraphInfo = null;
    protected int mGraphId;
    private bool mNewReport;

    #endregion


    #region "Properties"

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


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        ucSelectString.Scope = ReportInfo.OBJECT_TYPE;
        ConnectionStringRow.Visible = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.reporting", "SetConnectionString");
        txtQueryQuery.FullScreenParentElementID = pnlWebPartForm_Properties.ClientID;

        // Test permission for query
        txtQueryQuery.Enabled = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Reporting", "EditSQLQueries");

        versionList.OnAfterRollback += versionList_onAfterRollback;

        tabControlElem.TabItems.Add(new UITabItem
        {
            Text = GetString("general.general")
        });

        tabControlElem.TabItems.Add(new UITabItem
        {
            Text = GetString("general.preview")
        });

        tabControlElem.UsePostback = true;

        Title = "ReportGraph Edit";
        PageTitle.HelpTopicName = HELP_TOPIC_LINK;

        RegisterClientScript();

        rfvCodeName.ErrorMessage = GetString("general.requirescodename");
        rfvDisplayName.ErrorMessage = GetString("general.requiresdisplayname");


        Save += (s, ea) =>
        {
            if (SetData(true))
            {
                ltlScript.Text += ScriptHelper.GetScript("window.RefreshContent();CloseDialog();");
            }
        };

        int reportId = QueryHelper.GetInteger("reportid", 0);
        bool isPreview = QueryHelper.GetBoolean("preview", false);

        // If preview by URL -> select preview tab
        if (isPreview && !RequestHelper.IsPostBack())
        {
            tabControlElem.SelectedTab = 1;
        }

        if (reportId > 0)
        {
            // Get report info
            mReportInfo = ReportInfoProvider.GetReportInfo(reportId);
        }

        // Must be valid reportid parameter
        if (PersistentEditedObject == null)
        {
            if (mReportInfo != null)
            {
                int id = QueryHelper.GetInteger("objectid", 0);

                // Try to load graph name from hidden field (adding new graph & preview)
                if (id == 0)
                {
                    id = ValidationHelper.GetInteger(txtNewGraphHidden.Value, 0);
                }

                if (id > 0)
                {
                    PersistentEditedObject = ReportGraphInfoProvider.GetReportGraphInfo(id);
                    mReportGraphInfo = PersistentEditedObject as ReportGraphInfo;
                }
            }
        }
        else
        {
            mReportGraphInfo = PersistentEditedObject as ReportGraphInfo;
        }

        if (mReportInfo != null)
        {
            ucSelectString.DefaultConnectionString = mReportInfo.ReportConnectionString;
        }

        if (mReportGraphInfo != null)
        {
            // Set title text and image
            PageTitle.TitleText = GetString("Reporting_ReportGraph_Edit.TitleText");
            mGraphId = mReportGraphInfo.GraphID;

            // Show versions tab
            if (ObjectVersionManager.DisplayVersionsTab(mReportGraphInfo))
            {
                tabControlElem.TabItems.Add(new UITabItem
                {
                    Text = GetString("objectversioning.tabtitle")
                });

                versionList.Object = mReportGraphInfo;
                versionList.IsLiveSite = false;
            }
        }
        else
        {
            // Current report graph is new object
            PageTitle.TitleText = GetString("Reporting_ReportGraph_Edit.NewItemTitleText");
            mNewReport = true;
        }

        if (!RequestHelper.IsPostBack())
        {
            // Fill border styles
            FillBorderStyle(drpLegendBorderStyle);
            FillBorderStyle(drpChartAreaBorderStyle);
            FillBorderStyle(drpPlotAreaBorderStyle);
            FillBorderStyle(drpSeriesBorderStyle);
            FillBorderStyle(drpSeriesLineBorderStyle);

            // Fill gradient styles
            FillGradientStyle(drpChartAreaGradient);
            FillGradientStyle(drpPlotAreaGradient);
            FillGradientStyle(drpSeriesGradient);

            // Fill axis's position
            FillPosition(drpYAxisTitlePosition);
            FillPosition(drpXAxisTitlePosition);
            FillTitlePosition(drpTitlePosition);

            // Fill legend's position
            FillLegendPosition(drpLegendPosition);

            // Fill font type
            FillChartType(drpChartType);

            // Fill Chart type controls
            FillBarType(drpBarDrawingStyle);
            FillStackedBarType(drpStackedBarDrawingStyle);
            FillPieType(drpPieDrawingStyle);
            FillDrawingDesign(drpPieDrawingDesign);
            FillLabelStyle(drpPieLabelStyle);
            FillPieRadius(drpPieDoughnutRadius);
            FillLineDrawingStyle(drpLineDrawingStyle);
            FillOrientation(drpBarOrientation);
            FillOrientation(drpBarStackedOrientation);
            FillBorderSkinStyle();

            // FillSymbos
            FillSymbols(drpSeriesSymbols);
            if (!mNewReport)
            {
                LoadData();
            }
            // Load default data
            else
            {
                chkExportEnable.Checked = ReportGraphDefaults.ExportEnable;
                chkSubscription.Checked = ReportGraphDefaults.Subscription;

                ucSelectString.Value = ReportGraphDefaults.SelectConnectionString;

                // Set default values for some controls
                txtQueryNoRecordText.Text = ResHelper.GetString("attachmentswebpart.nodatafound");
                drpPieLabelStyle.SelectedValue = ReportGraphDefaults.PieLabelStyle;
                drpPieDoughnutRadius.SelectedValue = ReportGraphDefaults.PieDoughnutRadius;
                drpSeriesBorderStyle.SelectedValue = ReportGraphDefaults.SeriesBorderStyle;
                drpSeriesLineBorderStyle.SelectedValue = ReportGraphDefaults.SeriesLineBorderStyle;
                drpLegendBorderStyle.SelectedValue = ReportGraphDefaults.LegendBorderStyle;
                drpChartAreaBorderStyle.SelectedValue = ReportGraphDefaults.ChartAreaBorderStyle;
                drpPlotAreaBorderStyle.SelectedValue = ReportGraphDefaults.PlotAreaBorderStyle;

                // Border size
                txtSeriesLineBorderSize.Text = ReportGraphDefaults.SeriesLineBorderSize.ToString();
                txtSeriesBorderSize.Text = ReportGraphDefaults.SeriesBorderSize.ToString();
                txtChartAreaBorderSize.Text = ReportGraphDefaults.ChartAreaBorderSize.ToString();
                txtPlotAreaBorderSize.Text = ReportGraphDefaults.PlotAreaBorderSize.ToString();
                txtLegendBorderSize.Text = ReportGraphDefaults.LegendBorderSize.ToString();

                // Border color
                ucPlotAreaBorderColor.SelectedColor = ReportGraphDefaults.PlotAreaBorderColor;
                ucChartAreaBorderColor.SelectedColor = ReportGraphDefaults.ChartAreaBorderColor;
                ucLegendBorderColor.SelectedColor = ReportGraphDefaults.LegendBorderColor;
                ucSeriesBorderColor.SelectedColor = ReportGraphDefaults.SeriesBorderColor;

                // Other values
                ucChartAreaPrBgColor.SelectedColor = ReportGraphDefaults.ChartAreaPrBgColor;
                ucChartAreaSecBgColor.SelectedColor = ReportGraphDefaults.ChartAreaSecBgColor;
                drpChartAreaGradient.SelectedValue = ReportGraphDefaults.ChartAreaGradient;
                ucTitleColor.SelectedColor = ReportGraphDefaults.TitleColor;
                drpBorderSkinStyle.SelectedValue = ReportGraphDefaults.BorderSkinStyle;
                txtChartWidth.Text = ReportGraphDefaults.ChartWidth.ToString();
                txtChartHeight.Text = ReportGraphDefaults.ChartHeight.ToString();
                chkShowGrid.Checked = ReportGraphDefaults.ShowGrid;
                chkYAxisUseXSettings.Checked = ReportGraphDefaults.YAxisUseXSettings;
                ucXAxisTitleColor.SelectedColor = ReportGraphDefaults.XAxisTitleColor;
                ucYAxisTitleColor.SelectedColor = ReportGraphDefaults.YAxisTitleColor;
                drpSeriesSymbols.SelectedValue = ReportGraphDefaults.SeriesSymbols;

                drpBarDrawingStyle.SelectedValue = ReportGraphDefaults.BarDrawingStyle;
                chkSeriesDisplayItemValue.Checked = ReportGraphDefaults.SeriesDisplayItemValue;
                txtXAxisInterval.Text = ReportGraphDefaults.XAxisInterval.ToString();
                chkXAxisSort.Checked = ReportGraphDefaults.XAxisSort;
                drpPieDrawingStyle.SelectedValue = ReportGraphDefaults.PieDrawingStyle;
                drpPieDrawingDesign.SelectedValue = ReportGraphDefaults.PieDrawingDesign;
                drpStackedBarDrawingStyle.SelectedValue = ReportGraphDefaults.StackedBarDrawingStyle;
            }
        }

        HideChartTypeControls(drpChartType.SelectedIndex);
        ucXAxisTitleFont.SetOnChangeAttribute("xAxixTitleFontChanged ()");

        // Font settings
        ucTitleFont.DefaultSize = 14;
        ucTitleFont.DefaultStyle = "bold";
        ucXAxisTitleFont.DefaultSize = 11;
        ucXAxisTitleFont.DefaultStyle = "bold";
        ucYAxisTitleFont.DefaultSize = 11;
        ucYAxisTitleFont.DefaultStyle = "bold";

        txtRotateX.Enabled = chkShowAs3D.Checked;
        txtRotateY.Enabled = chkShowAs3D.Checked;
        chkBarOverlay.Enabled = chkShowAs3D.Checked;

        // Doughnut settings
        drpPieDoughnutRadius.Enabled = drpPieDrawingStyle.SelectedValue == "Doughnut";

        CurrentMaster.PanelContent.RemoveCssClass("dialog-content");
        CurrentMaster.PanelContent.RemoveCssClass("ModalDialogContent");     
    }


    /// <summary>
    /// PreRender event handler
    /// </summary>
    protected override void OnPreRender(EventArgs e)
    {
        divScrolable.CssClass = "";

        // State
        var tabIndex = tabControlElem.SelectedTab < 0 ? 0 : tabControlElem.SelectedTab;
        switch (tabIndex)
        {
            // Edit
            case 0:
                divScrolable.CssClass = "ScrollableContent";
                pnlPreview.Visible = false;
                pnlVersions.Visible = false;
                DisplayEditControls(true);
                
                break;

            // Preview
            case 1:
                if (SetData())
                {
                    ShowPreview();
                }
                else
                {
                    tabControlElem.SelectedTab = 0;
                    divScrolable.CssClass = "ScrollableContent";
                }
                break;

            // Version
            case 2:
                // Color picker preview issue
                DisplayEditControls(false);
               
                pnlPreview.Visible = false;

                if (mReportGraphInfo != null)
                {
                    pnlVersions.Visible = true;
                }
                break;
        }

        // Register the dialog script
        ScriptHelper.RegisterDialogScript(this);
        AddNoCacheTag();
        RegisterModalPageScripts();

        base.OnPreRender(e);
    }


    /// <summary>
    /// Delete all chart controls except the one set
    /// </summary>
    /// <param name="index">Chart type indicator</param>
    private void HideChartTypeControls(int index)
    {
        // Use x axis
        if (chkYAxisUseXSettings.Checked)
        {
            YAxisTitleFontRow.AddCssClass("hidden");
            YAxisTitlePositionRow.AddCssClass("hidden");
            YAxisLabelFont.AddCssClass("hidden");
        }

        // Bar
        if (index != 0)
        {
            BarDrawingStyleRow.AddCssClass("hidden");
            BarOverlayRow.AddCssClass("hidden");
            BarOrientationRow.AddCssClass("hidden");
        }

        // Stacked bar
        if (index != 1)
        {
            StackedBarDrawingStyleRow.AddCssClass("hidden");
            StackedBar100ProcStacked.AddCssClass("hidden");
            BarStackedOrientationRow.AddCssClass("hidden");
        }

        // Pie bar
        if (index != 2)
        {
            PieDrawingStyleRow.AddCssClass("hidden");
            PieDrawingDesign.AddCssClass("hidden");
            PieLabelStyleRow.AddCssClass("hidden");
            PieDoughnutRadiusRow.AddCssClass("hidden");
            PieOtherValue.AddCssClass("hidden");
        }
        else
        {
            SeriesValuesAsPercent.AddCssClass("hidden");
        }

        // Line bar
        if (index != 3)
        {
            LineDrawingStyleRow.AddCssClass("hidden");
            SeriesLineBorderSizeRow.AddCssClass("hidden");
            SeriesLineBorderStyleRow.AddCssClass("hidden");
            SeriesLineColorRow.AddCssClass("hidden");
            SeriesSymbols.AddCssClass("hidden");
        }
        else
        {
            SeriesBorderColorRow.AddCssClass("hidden");
            SeriesBorderSizeRow.AddCssClass("hidden");
            SeriesGradientRow.AddCssClass("hidden");
            SeriesPrBgColorRow.AddCssClass("hidden");
            SeriesSecBgColorRow.AddCssClass("hidden");
            SeriesBorderStyleRow.AddCssClass("hidden");
        }

        if (index == 2)
        {
            ChartShowGridRow.AddCssClass("hidden");
        }
    }


    /// <summary>
    /// Convert old graph data to new one (if any)
    /// </summary>
    /// <param name="settings">Old graph data</param>
    private void Convert(ReportCustomData settings)
    {
        // Smooth curves
        bool smoothCurves = ValidationHelper.GetBoolean(settings["SmoothCurves"], false);
        if (smoothCurves)
        {
            drpLineDrawingStyle.SelectedValue = "SpLine";
        }

        // Fill curves
        bool fillCurves = ValidationHelper.GetBoolean(settings["FillCurves"], false);
        if (fillCurves)
        {
            drpStackedBarDrawingStyle.SelectedValue = "Area";
        }

        // Vertical bars
        if (settings.ContainsColumn("VerticalBars"))
        {
            bool verticalBars = ValidationHelper.GetBoolean(settings["VerticalBars"], false);
            drpBarOrientation.SelectedValue = verticalBars ? "Vertical" : "Horizontal";
        }

        // Some types of graph type
        string graphType = mReportGraphInfo.GraphType;
        if (graphType == "baroverlay")
        {
            chkBarOverlay.Checked = true;
            drpChartType.SelectedValue = "bar";
        }

        if (graphType == "barpercentage")
        {
            chkStacked.Checked = true;
            drpChartType.SelectedValue = "barstacked";
        }

        if (graphType == "barstacked")
        {
            chkStacked.Checked = false;
            drpChartType.SelectedValue = "barstacked";
        }

        // Legend
        int value = ValidationHelper.GetInteger(mReportGraphInfo.GraphLegendPosition, ReportGraphDefaults.GraphLegendPosition);
        if (value != ReportGraphDefaults.GraphLegendPosition)
        {
            string position;
            switch (value)
            {
                case 0:
                    position = "TopLeft";
                    break;

                case 1:
                    position = "TopLeft";
                    break;

                case 2:
                    position = "TopRight";
                    break;

                case 3:
                    position = "BottomLeft";
                    break;

                case 4:
                    position = "TopLeft";
                    chkLegendInside.Checked = true;
                    break;

                case 5:
                    position = "TopRight";
                    chkLegendInside.Checked = true;
                    break;

                case 6:
                    position = "BottomLeft";
                    chkLegendInside.Checked = true;
                    break;

                case 7:
                    position = "BottomRight";
                    chkLegendInside.Checked = true;
                    break;

                case 8:
                    position = "TopLeft";
                    break;

                case 9:
                    position = "Top";
                    break;

                case 10:
                    position = "Bottom";
                    break;

                case 11:
                    position = "TopLeft";
                    break;

                case 12:
                    position = "BottomLeft";
                    break;

                case -1:
                    position = "None";
                    break;

                default:
                    position = "None";
                    break;
            }

            drpLegendPosition.SelectedValue = position;
        }

        // If old x axis font defined set same y axis
        if (settings.ContainsColumn("axisFont"))
        {
            chkYAxisUseXSettings.Checked = true;
            ucYAxisTitleFont.Value = ucXAxisTitleFont.Value;
            drpYAxisTitlePosition.SelectedValue = drpYAxisTitlePosition.SelectedValue;
        }
    }


    /// <summary>
    /// Loads data from graph storage
    /// </summary>
    private void LoadData()
    {
        if (mReportGraphInfo != null)
        {
            // Set graph settings to controls
            txtDefaultName.Text = mReportGraphInfo.GraphDisplayName;
            txtDefaultCodeName.Text = mReportGraphInfo.GraphName;
            txtQueryQuery.Text = mReportGraphInfo.GraphQuery;
            chkIsStoredProcedure.Checked = mReportGraphInfo.GraphQueryIsStoredProcedure;
            drpChartType.SelectedValue = mReportGraphInfo.GraphType;
            ucSelectString.Value = mReportGraphInfo.GraphConnectionString;

            // Graph height
            if (ValidationHelper.GetInteger(mReportGraphInfo.GraphHeight, 0) == 0)
            {
                txtChartHeight.Text = String.Empty;
            }
            else
            {
                txtChartHeight.Text = ValidationHelper.GetString(mReportGraphInfo.GraphHeight.ToString(), String.Empty);
            }

            // Graph width
            if (ValidationHelper.GetInteger(mReportGraphInfo.GraphWidth, 0) == 0)
            {
                txtChartWidth.Text = String.Empty;
            }
            else
            {
                txtChartWidth.Text = ValidationHelper.GetString(mReportGraphInfo.GraphWidth.ToString(), String.Empty);
            }
            // Title text
            txtGraphTitle.Text = mReportGraphInfo.GraphTitle;
            txtXAxisTitle.Text = mReportGraphInfo.GraphXAxisTitle;
            txtYAxisTitle.Text = mReportGraphInfo.GraphYAxisTitle;

            // Load Custom settings
            ReportCustomData settings = mReportGraphInfo.GraphSettings;

            // Export
            chkExportEnable.Checked = ValidationHelper.GetBoolean(settings["ExportEnabled"], ReportGraphDefaults.ExportEnable);

            // Subscription
            chkSubscription.Checked = ValidationHelper.GetBoolean(settings["SubscriptionEnabled"], ReportGraphDefaults.Subscription);

            // Chart Type
            chkShowGrid.Checked = ValidationHelper.GetBoolean(settings["ShowMajorGrid"], ReportGraphDefaults.ShowGrid);

            // Bar settings
            drpBarDrawingStyle.SelectedValue = ValidationHelper.GetString(settings["BarDrawingStyle"], ReportGraphDefaults.BarDrawingStyle);

            chkBarOverlay.Checked = ValidationHelper.GetBoolean(settings["BarOverlay"], false);
            drpBarOrientation.SelectedValue = settings["BarOrientation"];
            drpBarStackedOrientation.SelectedValue = settings["BarOrientation"];

            drpStackedBarDrawingStyle.SelectedValue = ValidationHelper.GetString(settings["StackedBarDrawingStyle"], ReportGraphDefaults.StackedBarDrawingStyle);
            chkStacked.Checked = ValidationHelper.GetBoolean(settings["StackedBarMaxStacked"], false);

            // Pie settings
            drpPieDrawingStyle.SelectedValue = ValidationHelper.GetString(settings["PieDrawingStyle"], ReportGraphDefaults.PieDrawingStyle);
            drpPieDrawingDesign.SelectedValue = ValidationHelper.GetString(settings["PieDrawingDesign"], ReportGraphDefaults.PieDrawingDesign);
            drpPieLabelStyle.SelectedValue = ValidationHelper.GetString(settings["PieLabelStyle"], ReportGraphDefaults.PieLabelStyle);
            drpPieDoughnutRadius.SelectedValue = ValidationHelper.GetString(settings["PieDoughnutRadius"], ReportGraphDefaults.PieDoughnutRadius);
            txtPieOtherValue.Text = ValidationHelper.GetString(settings["PieOtherValue"], string.Empty);

            drpLineDrawingStyle.SelectedValue = settings["LineDrawinStyle"];

            chkShowAs3D.Checked = ValidationHelper.GetBoolean(settings["ShowAs3D"], false);

            txtRotateX.Text = settings["RotateX"];
            txtRotateY.Text = settings["RotateY"];

            // Title
            ucTitleFont.Value = settings["TitleFontNew"];
            drpTitlePosition.SelectedValue = settings["TitlePosition"];
            ucTitleColor.SelectedColor = settings["TitleColor"];

            // Legend
            ucLegendBgColor.SelectedColor = settings["LegendBgColor"];
            ucLegendBorderColor.SelectedColor = ValidationHelper.GetString(settings["LegendBorderColor"], ReportGraphDefaults.LegendBorderColor);
            txtLegendBorderSize.Text = ValidationHelper.GetString(settings["LegendBorderSize"], ReportGraphDefaults.LegendBorderSize.ToString());
            drpLegendBorderStyle.SelectedValue = ValidationHelper.GetString(settings["LegendBorderStyle"], ReportGraphDefaults.LegendBorderStyle);
            drpLegendPosition.SelectedValue = settings["LegendPosition"];
            chkLegendInside.Checked = ValidationHelper.GetBoolean(settings["LegendInside"], false);
            txtLegendTitle.Text = settings["LegendTitle"];

            // XAxis
            ucXAxisTitleFont.Value = settings["XAxisFont"];
            drpXAxisTitlePosition.SelectedValue = settings["XAxisTitlePosition"];
            txtXAxisAngle.Text = settings["XAxisAngle"];
            txtXAxisInterval.Text = settings["XAxisInterval"];
            chkXAxisSort.Checked = ValidationHelper.GetBoolean(settings["xaxissort"], ReportGraphDefaults.XAxisSort);
            ucXAxisLabelFont.Value = settings["XAxisLabelFont"];
            ucXAxisTitleColor.SelectedColor = settings["XAxisTitleColor"];
            txtXAxisFormat.Text = settings["XAxisFormat"];

            // YAxis
            chkYAxisUseXSettings.Checked = ValidationHelper.GetBoolean(settings["YAxisUseXAxisSettings"], ReportGraphDefaults.YAxisUseXSettings);
            txtYAxisAngle.Text = settings["YAxisAngle"];
            ucYAxisTitleColor.SelectedColor = settings["YAxisTitleColor"];
            txtYAxisFormat.Text = settings["YAxisFormat"];

            if (chkYAxisUseXSettings.Checked)
            {
                ucYAxisTitleFont.Value = ucXAxisTitleFont.Value;
                ucYAxisLabelFont.Value = ucXAxisLabelFont.Value;
                drpYAxisTitlePosition.SelectedValue = drpXAxisTitlePosition.SelectedValue;
            }
            else
            {
                ucYAxisTitleFont.Value = mReportGraphInfo.GraphSettings["YAxisFont"];
                ucYAxisLabelFont.Value = mReportGraphInfo.GraphSettings["YAxisLabelFont"];
                drpYAxisTitlePosition.SelectedValue = mReportGraphInfo.GraphSettings["YAxisTitlePosition"];
            }

            // Chart Area
            ucChartAreaPrBgColor.SelectedColor = ValidationHelper.GetString(settings["ChartAreaPrBgColor"], ReportGraphDefaults.ChartAreaPrBgColor);
            ucChartAreaSecBgColor.SelectedColor = ValidationHelper.GetString(settings["ChartAreaSecBgColor"], ReportGraphDefaults.ChartAreaSecBgColor);
            drpChartAreaGradient.SelectedValue = ValidationHelper.GetString(settings["ChartAreaGradient"], ReportGraphDefaults.ChartAreaGradient);
            ucChartAreaBorderColor.SelectedColor = ValidationHelper.GetString(settings["ChartAreaBorderColor"], ReportGraphDefaults.ChartAreaBorderColor);
            txtChartAreaBorderSize.Text = ValidationHelper.GetString(settings["ChartAreaBorderSize"], ReportGraphDefaults.ChartAreaBorderSize.ToString());
            drpChartAreaBorderStyle.SelectedValue = ValidationHelper.GetString(settings["ChartAreaBorderStyle"], ReportGraphDefaults.ChartAreaBorderStyle);
            txtChartAreaBorderSize.Text = settings["ChartAreaBorderSize"];
            txtScaleMin.Text = settings["ScaleMin"];
            txtScaleMax.Text = settings["ScaleMax"];
            chkTenPowers.Checked = ValidationHelper.GetBoolean(settings["TenPowers"], false);
            chkReverseY.Checked = ValidationHelper.GetBoolean(settings["ReverseYAxis"], false);
            drpBorderSkinStyle.SelectedValue = ValidationHelper.GetString(settings["BorderSkinStyle"], ReportGraphDefaults.BorderSkinStyle);
            txtQueryNoRecordText.Text = settings["QueryNoRecordText"];

            // Plot area
            drpPlotAreaGradient.SelectedValue = settings["PlotAreaGradient"];
            ucPlotAreaPrBgColor.SelectedColor = settings["PlotAreaPrBgColor"];
            ucPlotAreSecBgColor.SelectedColor = settings["PlotAreaSecBgColor"];
            ucPlotAreaBorderColor.SelectedColor = ValidationHelper.GetString(settings["PlotAreaBorderColor"], ReportGraphDefaults.PlotAreaBorderColor);
            txtPlotAreaBorderSize.Text = ValidationHelper.GetString(settings["PlotAreaBorderSize"], ReportGraphDefaults.PlotAreaBorderSize.ToString());
            drpPlotAreaBorderStyle.SelectedValue = ValidationHelper.GetString(settings["PlotAreaBorderStyle"], ReportGraphDefaults.PlotAreaBorderStyle);

            // Series 
            ucSeriesPrBgColor.SelectedColor = settings["SeriesPrBgColor"];
            ucSeriesSecBgColor.SelectedColor = settings["SeriesSecBgColor"];
            drpSeriesGradient.SelectedValue = settings["SeriesGradient"];
            drpSeriesSymbols.SelectedValue = ValidationHelper.GetString(settings["SeriesSymbols"], ReportGraphDefaults.SeriesSymbols);
            ucSeriesBorderColor.SelectedColor = ValidationHelper.GetString(settings["SeriesBorderColor"], ReportGraphDefaults.SeriesBorderColor);
            txtSeriesBorderSize.Text = ValidationHelper.GetString(settings["SeriesBorderSize"], ReportGraphDefaults.SeriesBorderSize.ToString());
            drpSeriesBorderStyle.SelectedValue = ValidationHelper.GetString(settings["SeriesBorderStyle"], ReportGraphDefaults.SeriesBorderStyle);
            chkSeriesDisplayItemValue.Checked = ValidationHelper.GetBoolean(settings["DisplayItemValue"], ReportGraphDefaults.SeriesDisplayItemValue);
            txtItemValueFormat.Text = settings["ItemValueFormat"];
            txtSeriesItemTooltip.Text = settings["SeriesItemToolTip"];
            txtSeriesItemLink.Text = settings["SeriesItemLink"];
            chkValuesAsPercent.Checked = ValidationHelper.GetBoolean(settings["ValuesAsPercent"], false);

            ucSeriesLineColor.SelectedColor = settings["SeriesPrBgColor"];
            txtSeriesLineBorderSize.Text = ValidationHelper.GetString(settings["SeriesBorderSize"], ReportGraphDefaults.SeriesLineBorderSize.ToString());
            drpSeriesLineBorderStyle.SelectedValue = ValidationHelper.GetString(settings["SeriesBorderStyle"], ReportGraphDefaults.SeriesLineBorderStyle);

            // Try to convert old data
            Convert(settings);
        }
    }


    /// <summary>
    /// Fills drop down list with border style enum
    /// </summary>
    /// <param name="drp">Data drop down list</param>
    private void FillBorderStyle(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.notset"), "NotSet"));
        drp.Items.Add(new ListItem(GetString("rep.graph.solid"), "Solid"));
        drp.Items.Add(new ListItem(GetString("rep.graph.dash"), "Dash"));
        drp.Items.Add(new ListItem(GetString("rep.graph.dashdot"), "DashDot"));
        drp.Items.Add(new ListItem(GetString("rep.graph.dashdotdot"), "DashDotDot"));
        drp.Items.Add(new ListItem(GetString("rep.graph.dot"), "Dot"));
    }


    /// <summary>
    /// Fills drop down list with usable gradient style
    /// </summary>
    /// <param name="drp">Drop down list</param>
    private void FillGradientStyle(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.none"), "None"));
        drp.Items.Add(new ListItem(GetString("rep.graph.leftright"), "LeftRight"));
        drp.Items.Add(new ListItem(GetString("rep.graph.diagonalleft"), "DiagonalLeft"));
        drp.Items.Add(new ListItem(GetString("rep.graph.topbottom"), "TopBottom"));
        drp.Items.Add(new ListItem(GetString("rep.graph.diagonalright"), "DiagonalRight"));
        drp.Items.Add(new ListItem(GetString("rep.graph.rightleft"), "RightLeft"));
        drp.Items.Add(new ListItem(GetString("rep.graph.leftdiagonal"), "LeftDiagonal"));
        drp.Items.Add(new ListItem(GetString("rep.graph.bottomtop"), "BottomTop"));
        drp.Items.Add(new ListItem(GetString("rep.graph.rightdiagonal"), "RightDiagonal"));
    }


    /// <summary>
    /// Fills drop down list with usable position for axe titles
    /// </summary>
    /// <param name="drp">Drop down list</param>
    private void FillPosition(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.center"), "Center"));
        drp.Items.Add(new ListItem(GetString("rep.graph.near"), "Near"));
        drp.Items.Add(new ListItem(GetString("rep.graph.far"), "Far"));
    }


    /// <summary>
    /// Fills title position
    /// </summary>
    /// <param name="drp">Title position drop down</param>
    private void FillTitlePosition(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.center"), "Center"));
        drp.Items.Add(new ListItem(GetString("rep.graph.right"), "Right"));
        drp.Items.Add(new ListItem(GetString("rep.graph.left"), "Left"));
    }


    /// <summary>
    /// Fills legend position 
    /// </summary>
    private void FillLegendPosition(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.none"), "None"));
        drp.Items.Add(new ListItem(GetString("rep.graph.top"), "Top"));
        drp.Items.Add(new ListItem(GetString("rep.graph.left"), "Left"));
        drp.Items.Add(new ListItem(GetString("rep.graph.right"), "Right"));
        drp.Items.Add(new ListItem(GetString("rep.graph.bottom"), "Bottom"));
        drp.Items.Add(new ListItem(GetString("rep.graph.topleft"), "TopLeft"));
        drp.Items.Add(new ListItem(GetString("rep.graph.topright"), "TopRight"));
        drp.Items.Add(new ListItem(GetString("rep.graph.bottomleft"), "BottomLeft"));
        drp.Items.Add(new ListItem(GetString("rep.graph.bottomright"), "BottomRight"));
    }


    /// <summary>
    /// Fills chart type drop down 
    /// </summary>
    /// <param name="drp">Data drop down list</param>
    public void FillChartType(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.barchart"), "bar"));
        drp.Items.Add(new ListItem(GetString("rep.graph.barstackedchart"), "barstacked"));
        drp.Items.Add(new ListItem(GetString("rep.graph.piechart"), "pie"));
        drp.Items.Add(new ListItem(GetString("rep.graph.linechart"), "line"));
    }


    /// <summary>
    /// Fills drawing style for bar charts
    /// </summary>
    /// <param name="drp">Drop-down list control</param>
    private void FillBarType(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.bar"), "Bar"));
        drp.Items.Add(new ListItem(GetString("rep.graph.cylinder"), "Cylinder"));
    }


    /// <summary>
    /// Fills drawing style for stacked bar charts
    /// </summary>
    /// <param name="drp">Stacked bar data drop down list</param>
    private void FillStackedBarType(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.bar"), "Bar"));
        drp.Items.Add(new ListItem(GetString("rep.graph.cylinder"), "Cylinder"));
        drp.Items.Add(new ListItem(GetString("rep.graph.area"), "Area"));
    }


    /// <summary>
    /// Fills drawing style for pie charts
    /// </summary>
    /// <param name="drp">Pie type data drop down list</param>
    private void FillPieType(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.pie"), "Pie"));
        drp.Items.Add(new ListItem(GetString("rep.graph.doughnut"), "Doughnut"));
    }


    /// <summary>
    /// Fills drawing design for pie charts
    /// </summary>
    /// <param name="drp">Drawing design drop down list</param>
    private void FillDrawingDesign(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.none"), "Default"));
        drp.Items.Add(new ListItem(GetString("rep.graph.softedge"), "SoftEdge"));
        drp.Items.Add(new ListItem(GetString("rep.graph.concave"), "Concave"));
    }


    /// <summary>
    /// Fills label style for pie charts
    /// </summary>
    /// <param name="drp">Label style drop down list</param>
    private void FillLabelStyle(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.inside"), "Inside"));
        drp.Items.Add(new ListItem(GetString("rep.graph.outside"), "Outside"));
        drp.Items.Add(new ListItem(GetString("rep.graph.none"), "Disabled"));
    }


    /// <summary>
    /// Fills drawing style for line charts
    /// </summary>
    /// <param name="drp">Line drawing style drop down list</param>
    private void FillLineDrawingStyle(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.line"), "Line"));
        drp.Items.Add(new ListItem(GetString("rep.graph.spline"), "SpLine"));
    }


    /// <summary>
    /// Fills drawing style for pie charts
    /// </summary>
    /// <param name="drp">Pie radius drop down list</param>
    private void FillPieRadius(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem("20", "20"));
        drp.Items.Add(new ListItem("30", "30"));
        drp.Items.Add(new ListItem("40", "40"));
        drp.Items.Add(new ListItem("50", "50"));
        drp.Items.Add(new ListItem("60", "60"));
        drp.Items.Add(new ListItem("70", "70"));
    }


    /// <summary>
    /// Fill bar chart orientation
    /// </summary>
    /// <param name="drp">Orientation drop down list</param>
    private void FillOrientation(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.vertical"), "Vertical"));
        drp.Items.Add(new ListItem(GetString("rep.graph.horizontal"), "Horizontal"));
    }


    /// <summary>
    /// Fills border skin style
    /// </summary>
    private void FillBorderSkinStyle()
    {
        drpBorderSkinStyle.Items.Add(new ListItem(GetString("general.none"), "None"));
        drpBorderSkinStyle.Items.Add(new ListItem(GetString("rep.graph.emboss"), "Emboss"));
        drpBorderSkinStyle.Items.Add(new ListItem(GetString("rep.graph.raised"), "Raised"));
        drpBorderSkinStyle.Items.Add(new ListItem(GetString("rep.graph.sunken"), "Sunken"));
    }


    /// <summary>
    /// Fills symbols for charts
    /// </summary>
    /// <param name="drp">Symbols drop down list</param>
    private void FillSymbols(CMSDropDownList drp)
    {
        drp.Items.Add(new ListItem(GetString("rep.graph.none"), "None"));
        drp.Items.Add(new ListItem(GetString("rep.graph.square"), "Square"));
        drp.Items.Add(new ListItem(GetString("rep.graph.diamond"), "Diamond"));
        drp.Items.Add(new ListItem(GetString("rep.graph.triangle"), "Triangle"));
        drp.Items.Add(new ListItem(GetString("rep.graph.circle"), "Circle"));
        drp.Items.Add(new ListItem(GetString("rep.graph.cross"), "Cross"));
        drp.Items.Add(new ListItem(GetString("rep.graph.star4"), "Star4"));
    }


    /// <summary>
    /// Register JavaScript
    /// </summary>
    private void RegisterClientScript()
    {
        RegisterResizeAndRollbackScript("divFooter", divScrolable.ClientID);
        
        string script = @"
function checkXAxisSettings() {  
if (jQuery('#" + chkYAxisUseXSettings.ClientID + @"').is(':checked')) { 

    var YAxisTitleFontRow = jQuery('#" + YAxisTitleFontRow.ClientID + @"');
    if (YAxisTitleFontRow) {
        YAxisTitleFontRow.addClass('hidden');
    }
 
    var YAxisTitlePositionRow = jQuery('#" + YAxisTitlePositionRow.ClientID + @"');
    if (YAxisTitlePositionRow) {
        YAxisTitlePositionRow.addClass('hidden'); 
    }

    var YAxisLabelFont = jQuery('#" + YAxisLabelFont.ClientID + @"');
    if (YAxisLabelFont) {
        YAxisLabelFont.addClass('hidden');
    }
                
    var drpYAxisTitlePosition = jQuery('#" + drpYAxisTitlePosition.ClientID + @"');
    var drpXAxisTitlePosition = jQuery('#" + drpXAxisTitlePosition.ClientID + @"');
    var ucYAxisLabelFont = jQuery('#" + ucYAxisLabelFont.FontTypeTextBoxClientId + @"');
    var ucYAxisTitleFont = jQuery('#" + ucYAxisTitleFont.FontTypeTextBoxClientId + @"');
    var ucXAxisLabelFont = jQuery('#" + ucXAxisLabelFont.FontTypeTextBoxClientId + @"');
    var ucXAxisTitleFont = jQuery('#" + ucXAxisTitleFont.FontTypeTextBoxClientId + @"');

    if (drpYAxisTitlePosition && drpXAxisTitlePosition) {
        drpYAxisTitlePosition.val(drpXAxisTitlePosition.val());
    }

    if (ucYAxisLabelFont && ucXAxisLabelFont) {
        ucYAxisLabelFont.val(ucXAxisLabelFont.val());
    }

    if (ucYAxisTitleFont && ucXAxisTitleFont) {
        ucYAxisTitleFont.val(ucXAxisTitleFont.val());
    }
}
else  {   
    var YAxisTitleFontRow = jQuery('#" + YAxisTitleFontRow.ClientID + @"');
    if (YAxisTitleFontRow) {
        YAxisTitleFontRow.removeClass('hidden');
    }
 
    var YAxisTitlePositionRow = jQuery('#" + YAxisTitlePositionRow.ClientID + @"');
    if (YAxisTitlePositionRow) {
        YAxisTitlePositionRow.removeClass('hidden');
    }

    var YAxisLabelFont = jQuery('#" + YAxisLabelFont.ClientID + @"');
    if (YAxisLabelFont) {
        YAxisLabelFont.removeClass('hidden');
    }            
} 
}
";

script += @"function hideAllChartTypeControls() {
        jQuery('.Bar').addClass('hidden');
        jQuery('.StackedBar').addClass('hidden');
        jQuery('.Pie').addClass('hidden');
        jQuery('.Line').addClass('hidden');
        jQuery('.Common').addClass('hidden');
        jQuery('.Grid').addClass('hidden');
    }

    function showAs3DClicked() {
        if (document.getElementById ('" + chkShowAs3D.ClientID + @"').checked == false) {
            document.getElementById ('" + txtRotateX.ClientID + @"').disabled = true;
            document.getElementById ('" + txtRotateY.ClientID + @"').disabled = true;
            document.getElementById ('" + chkBarOverlay.ClientID + @"').disabled = true;
        }
        else {
            document.getElementById ('" + txtRotateX.ClientID + @"').disabled = false;
            document.getElementById ('" + txtRotateY.ClientID + @"').disabled = false;
            document.getElementById ('" + chkBarOverlay.ClientID + @"').disabled = false;
}
    }

function pieStyleChanged() { 
        if (document.getElementById ('" + drpPieDrawingStyle.ClientID + @"').value == 'Doughnut') {
            document.getElementById ('" + drpPieDoughnutRadius.ClientID + @"').disabled = false;    
        }
        else {
            document.getElementById ('" + drpPieDoughnutRadius.ClientID + @"').disabled = true;    
        }
}
        function typeChanged() {
                var value=jQuery('#" + drpChartType.ClientID + @"').prop('selectedIndex');
                hideAllChartTypeControls();
                switch (value) {
                    case 0: jQuery('.Bar').removeClass('hidden');
                            jQuery('.Common').removeClass('hidden');
                            jQuery('.Grid').removeClass('hidden');
                    break;
                    case 1: jQuery('.StackedBar').removeClass('hidden');
                            jQuery('.Common').removeClass('hidden');
                            jQuery('.Grid').removeClass('hidden');
                        break;
                    case 2: jQuery('.Pie').removeClass('hidden');
                            jQuery('.Common').removeClass('hidden');                                      
                        break;
                    case 3: jQuery('.Line').removeClass('hidden');
                            jQuery('.Grid').removeClass('hidden');
                        break;
                }                                          
    }
                
";

        drpChartType.Attributes["onChange"] = "typeChanged()";
        ScriptHelper.RegisterClientScriptBlock(Page, typeof(Page), "YAxisScript", ScriptHelper.GetScript(script));
    }


    /// <summary>
    /// Check if control contains valid non negative integer
    /// </summary>
    /// <param name="txtControl">Text control</param>
    /// <param name="errorMessage">Error message text from previous controls</param>
    /// <param name="row">Row with textbox control</param>    
    private string ValidateNonNegativeIntegerValue(TextBox txtControl, string errorMessage, object row)
    {
        if ((txtControl.Text.Trim() != String.Empty) && ((!ValidationHelper.IsInteger(txtControl.Text.Trim())) || (Int32.Parse(txtControl.Text.Trim()) < 0)))
        {
            // Show error
            ShowError("rep.invalidnonnegativeinteger");
            return GetString("rep.invaliddata");
        }
        return errorMessage;
    }


    /// <summary>
    /// Check if control contains valid integer number
    /// </summary>
    /// <param name="txtControl">Text control</param>
    /// <param name="errorMessage">Error message text from previous controls</param>
    /// <param name="row">Row with textbox control</param> 
    private string ValidateIntegerValue(TextBox txtControl, string errorMessage, object row)
    {
        if ((txtControl.Text.Trim() != String.Empty) && (!ValidationHelper.IsInteger(txtControl.Text.Trim())))
        {
            // Show error
            ShowError("rep.invalidinteger");
            return GetString("rep.invaliddata");
        }
        return errorMessage;
    }


    /// <summary>
    /// Save the changes to DB
    /// </summary>
    /// <param name="saveToDatabase">If false, data is not stored to DB, only info object is filled</param>
    private bool SetData(bool saveToDatabase = false)
    {
        string errorMessage = String.Empty;
        if (saveToDatabase)
        {
            // Check permissions
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.reporting", "Modify"))
            {
                RedirectToAccessDenied("cms.reporting", "Modify");
            }

            // Validate inputs
            errorMessage = new Validator()
                .NotEmpty(txtDefaultName.Text, rfvDisplayName.ErrorMessage)
                .NotEmpty(txtDefaultCodeName.Text, rfvCodeName.ErrorMessage)
                .NotEmpty(txtQueryQuery.Text, GetString("Reporting_ReportGraph_Edit.ErrorQuery")).Result;

            if ((errorMessage == "") && (!ValidationHelper.IsIdentifier(txtDefaultCodeName.Text.Trim())))
            {
                errorMessage = GetString("general.erroridentifierformat");
            }

            string fullName = mReportInfo.ReportName + "." + txtDefaultCodeName.Text.Trim();

            // Get graph info
            ReportGraphInfo codeNameCheck = ReportGraphInfoProvider.GetReportGraphInfo(fullName);
            if ((errorMessage == "") && (codeNameCheck != null) && (codeNameCheck.GraphID != mGraphId))
            {
                errorMessage = GetString("Reporting_ReportGraph_Edit.ErrorCodeNameExist");
            }
        }

        if (errorMessage == String.Empty)
        {
            // Test query in all cases        
            errorMessage = new Validator().NotEmpty(txtQueryQuery.Text.Trim(), GetString("Reporting_ReportGraph_Edit.ErrorQuery")).Result;

            // Test valid integers 
            errorMessage = ValidateNonNegativeIntegerValue(txtChartWidth, errorMessage, ChartWidthRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtChartHeight, errorMessage, ChartHeightRow);

            errorMessage = ValidateIntegerValue(txtRotateX, errorMessage, RotateXRow);
            errorMessage = ValidateIntegerValue(txtRotateY, errorMessage, RotateYRow);

            errorMessage = ValidateIntegerValue(txtXAxisAngle, errorMessage, XAxisAngleRow);
            errorMessage = ValidateIntegerValue(txtYAxisAngle, errorMessage, YAxisAngleRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtXAxisInterval, errorMessage, XAxisInterval);

            errorMessage = ValidateIntegerValue(txtScaleMin, errorMessage, ChartAreaScaleMin);
            errorMessage = ValidateIntegerValue(txtScaleMax, errorMessage, ChartAreaScaleMax);

            errorMessage = ValidateNonNegativeIntegerValue(txtChartAreaBorderSize, errorMessage, ChartAreaBorderSizeRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtSeriesBorderSize, errorMessage, SeriesBorderSizeRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtLegendBorderSize, errorMessage, LegendBorderSizeRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtPlotAreaBorderSize, errorMessage, PlotAreaBorderSizeRow);
            errorMessage = ValidateNonNegativeIntegerValue(txtSeriesLineBorderSize, errorMessage, SeriesLineBorderSizeRow);
        }

        if (errorMessage == String.Empty)
        {
            // New graph
            if (mReportGraphInfo == null)
            {
                mReportGraphInfo = new ReportGraphInfo();
            }

            mReportGraphInfo.GraphDisplayName = txtDefaultName.Text.Trim();
            mReportGraphInfo.GraphName = txtDefaultCodeName.Text.Trim();

            // Check authorization
            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Reporting", "EditSQLQueries"))
            {
                mReportGraphInfo.GraphQuery = txtQueryQuery.Text;
            }

            mReportGraphInfo.GraphQueryIsStoredProcedure = chkIsStoredProcedure.Checked;

            mReportGraphInfo.GraphType = drpChartType.SelectedValue;
            mReportGraphInfo.GraphReportID = mReportInfo.ReportID;
            mReportGraphInfo.GraphWidth = ValidationHelper.GetInteger(txtChartWidth.Text, 0);
            mReportGraphInfo.GraphHeight = ValidationHelper.GetInteger(txtChartHeight.Text, 0);
            mReportGraphInfo.GraphTitle = txtGraphTitle.Text;

            mReportGraphInfo.GraphXAxisTitle = txtXAxisTitle.Text;
            mReportGraphInfo.GraphYAxisTitle = txtYAxisTitle.Text;
            mReportGraphInfo.GraphConnectionString = ValidationHelper.GetString(ucSelectString.Value, String.Empty);

            ReportCustomData settings = mReportGraphInfo.GraphSettings;

            // Export
            settings["ExportEnabled"] = chkExportEnable.Checked.ToString();

            // Subscription
            settings["SubscriptionEnabled"] = chkSubscription.Checked.ToString();

            // ChartType                        
            settings["BarDrawingStyle"] = drpBarDrawingStyle.SelectedValue;
            settings["BarOverlay"] = chkBarOverlay.Checked.ToString();
            settings["BarOrientation"] = drpChartType.SelectedValue.ToLowerCSafe() == "bar" ? drpBarOrientation.SelectedValue : drpBarStackedOrientation.SelectedValue;

            settings["StackedBarDrawingStyle"] = drpStackedBarDrawingStyle.SelectedValue;
            settings["StackedBarMaxStacked"] = chkStacked.Checked.ToString();

            // Pie settings
            settings["PieDrawingStyle"] = drpPieDrawingStyle.SelectedValue;
            settings["PieDrawingDesign"] = drpPieDrawingDesign.SelectedValue;
            settings["PieLabelStyle"] = drpPieLabelStyle.SelectedValue;
            settings["PieDoughnutRadius"] = drpPieDoughnutRadius.SelectedValue;
            settings["PieOtherValue"] = txtPieOtherValue.Text;

            settings["LineDrawinStyle"] = drpLineDrawingStyle.SelectedValue;

            settings["ShowAs3D"] = chkShowAs3D.Checked.ToString();
            settings["RotateX"] = txtRotateX.Text;
            settings["RotateY"] = txtRotateY.Text;

            // Title
            settings["ShowMajorGrid"] = chkShowGrid.Checked.ToString();
            settings["TitleFontNew"] = (string)ucTitleFont.Value;
            settings["TitlePosition"] = drpTitlePosition.SelectedValue;
            settings["TitleColor"] = ucTitleColor.SelectedColor;

            // Legend
            settings["LegendBgColor"] = ucLegendBgColor.SelectedColor;
            settings["LegendBorderColor"] = ucLegendBorderColor.SelectedColor;
            settings["LegendBorderSize"] = txtLegendBorderSize.Text;
            settings["LegendBorderStyle"] = drpLegendBorderStyle.SelectedValue;
            settings["LegendPosition"] = drpLegendPosition.SelectedValue;
            settings["LegendInside"] = chkLegendInside.Checked.ToString();
            settings["LegendTitle"] = txtLegendTitle.Text;

            // XAxis
            settings["XAxisFont"] = (string)ucXAxisTitleFont.Value;
            settings["XAxisTitlePosition"] = drpXAxisTitlePosition.SelectedValue;
            settings["XAxisAngle"] = txtXAxisAngle.Text;
            settings["XAxisInterval"] = txtXAxisInterval.Text;
            settings["XAxisSort"] = chkXAxisSort.Checked.ToString();
            settings["XAxisLabelFont"] = (string)ucXAxisLabelFont.Value;
            settings["XAxisTitleColor"] = ucXAxisTitleColor.SelectedColor;
            settings["XAxisFormat"] = txtXAxisFormat.Text;

            // YAxis             
            settings["YAxisUseXAxisSettings"] = chkYAxisUseXSettings.Checked.ToString();
            settings["YAxisAngle"] = txtYAxisAngle.Text;
            settings["YAxisTitleColor"] = ucYAxisTitleColor.SelectedColor;
            settings["YAxisFormat"] = txtYAxisFormat.Text;

            if (chkYAxisUseXSettings.Checked)
            {
                settings["YAxisFont"] = (string)ucXAxisTitleFont.Value;
                settings["YAxisTitlePosition"] = drpXAxisTitlePosition.SelectedValue;
                settings["YAxisLabelFont"] = (string)ucXAxisLabelFont.Value;
            }
            else
            {
                settings["YAxisFont"] = (string)ucYAxisTitleFont.Value;
                settings["YAxisLabelFont"] = (string)ucYAxisLabelFont.Value;
                settings["YAxisTitlePosition"] = drpYAxisTitlePosition.SelectedValue;
            }

            // Chart Area
            settings["ChartAreaPrBgColor"] = ucChartAreaPrBgColor.SelectedColor;
            settings["ChartAreaSecBgColor"] = ucChartAreaSecBgColor.SelectedColor;
            settings["ChartAreaBorderColor"] = ucChartAreaBorderColor.SelectedColor;
            settings["ChartAreaGradient"] = drpChartAreaGradient.SelectedValue;
            settings["ChartAreaBorderSize"] = txtChartAreaBorderSize.Text;
            settings["ChartAreaBorderStyle"] = drpChartAreaBorderStyle.SelectedValue;
            settings["ScaleMin"] = txtScaleMin.Text;
            settings["ScaleMax"] = txtScaleMax.Text;
            settings["TenPowers"] = chkTenPowers.Checked.ToString();
            settings["ReverseYAxis"] = chkReverseY.Checked.ToString();
            settings["BorderSkinStyle"] = drpBorderSkinStyle.SelectedValue;
            settings["QueryNoRecordText"] = txtQueryNoRecordText.Text;

            // Plot Area
            settings["PlotAreaPrBgColor"] = ucPlotAreaPrBgColor.SelectedColor;
            settings["PlotAreaSecBgColor"] = ucPlotAreSecBgColor.SelectedColor;
            settings["PlotAreaBorderColor"] = ucPlotAreaBorderColor.SelectedColor;
            settings["PlotAreaGradient"] = drpPlotAreaGradient.SelectedValue;
            settings["PlotAreaBorderSize"] = txtPlotAreaBorderSize.Text;
            settings["PlotAreaBorderStyle"] = drpPlotAreaBorderStyle.SelectedValue;

            // Series 
            settings["SeriesPrBgColor"] = ucSeriesPrBgColor.SelectedColor;
            settings["SeriesSecBgColor"] = ucSeriesSecBgColor.SelectedColor;
            settings["SeriesBorderColor"] = ucSeriesBorderColor.SelectedColor;
            settings["SeriesGradient"] = drpSeriesGradient.SelectedValue;
            settings["SeriesBorderSize"] = txtSeriesBorderSize.Text;
            settings["SeriesBorderStyle"] = drpSeriesBorderStyle.SelectedValue;
            settings["SeriesSymbols"] = drpSeriesSymbols.SelectedValue;
            settings["DisplayItemValue"] = chkSeriesDisplayItemValue.Checked.ToString();
            settings["SeriesItemToolTip"] = txtSeriesItemTooltip.Text;
            settings["SeriesItemLink"] = txtSeriesItemLink.Text;
            settings["ItemValueFormat"] = txtItemValueFormat.Text;
            settings["ValuesAsPercent"] = chkValuesAsPercent.Checked.ToString();

            if (drpChartType.SelectedValue.ToLowerCSafe() == "line")
            {
                settings["SeriesBorderSize"] = txtSeriesLineBorderSize.Text;
                settings["SeriesBorderStyle"] = drpSeriesLineBorderStyle.SelectedValue;
                settings["SeriesPrBgColor"] = ucSeriesLineColor.SelectedColor;
            }

            // Delete old settings
            settings["axisFont"] = null;
            settings["colors"] = null;
            settings["graphGradient"] = null;
            settings["chartGradient"] = null;
            settings["itemGradient"] = null;
            settings["symbols"] = null;
            settings["titleFont"] = null;
            settings["VerticalBars"] = null;
            settings["FillCurves"] = null;
            settings["SmoothCurves"] = null;
            mReportGraphInfo.GraphLegendPosition = ReportGraphDefaults.GraphLegendPosition;
            if (saveToDatabase)
            {
                ReportGraphInfoProvider.SetReportGraphInfo(mReportGraphInfo);
            }
            else
            {
                PersistentEditedObject = mReportGraphInfo;
            }

            return true;
        }

        ShowError(errorMessage);
        return false;
    }


    /// <summary>
    /// Show graph
    /// </summary>
    private void ShowPreview()
    {
        // Color picker preview issue
        DisplayEditControls(false);

        
        pnlVersions.Visible = false;
        if (mReportInfo != null)
        {
            pnlPreview.Visible = true;

            FormInfo fi = new FormInfo(mReportInfo.ReportParameters);
            // Get DataRow with required columns
            DataRow dr = fi.GetDataRow(false);

            fi.LoadDefaultValues(dr, true);

            ctrlReportGraph.ReportParameters = dr;

            ctrlReportGraph.Visible = true;

            // Prepare fully qualified graph name = with ReportName
            string fullReportGraphName = mReportInfo.ReportName + "." + mReportGraphInfo.GraphName;
            ctrlReportGraph.GraphInfo = mReportGraphInfo;
            ctrlReportGraph.Parameter = fullReportGraphName;

            ctrlReportGraph.ReloadData(true);
        }
    }


    /// <summary>
    /// Show/hide edit forms (tab 0)
    /// </summary>
    private void DisplayEditControls(bool show)
    {
        // Color picker preview issue  (need to hide validators when saving in preview mode (JavaScript ruins validation))
        if (show)
        {
            FormPanelHolder.Attributes.Remove("style");
        }
        else
        {
            FormPanelHolder.Attributes.Add("style", "display:none");
        }

        rfvDisplayName.Visible = show;
        rfvCodeName.Visible = show;
    }


    /// <summary>
    /// Get info from PersistentEditedObject and reload data
    /// </summary>
    private void ReloadDataAfterRollback()
    {
        // Load roll backed info
        GeneralizedInfo gi = PersistentEditedObject as GeneralizedInfo;
        if (gi != null)
        {
            mReportGraphInfo = gi.MainObject as ReportGraphInfo;
        }
        LoadData();
    }


    protected void versionList_onAfterRollback(object sender, EventArgs e)
    {
        ReloadDataAfterRollback();
    }

    #endregion
}