﻿
using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.Ecommerce;
using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.SettingsProvider;
using CMS.DataEngine;
using CMS.ExtendedControls.ActionsConfig;
using CMS.Helpers;

[Title("Objects/Ecommerce_ShippingOption/object.png", "Shipping extension country pricing list", "newgeneral_tab2")]
//[Security(Resource = "CMS.Ecommerce", UIElements = "Configuration.ShippingOptions.General")]
public partial class CMSModules_Ecommerce_Pages_Tools_Configuration_ShippingExtension_ShippingExtension_Edit_Pricing : CMSShippingOptionsPage
{
    protected int mShippingExtensionPricingID = QueryHelper.GetInteger("ItemID", 0);
    protected int mShippingOptionID = -1, mShippingOptionProcessingMode = 0;
    protected string ShippingOptionDisplayName, ShippingCountryDisplayName;
    protected decimal ShippingCountryBaseCost = 0, ShippingCountryUnitPrice = 0;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        // Init Unigrid
        UniGrid.OnAction += new OnActionEventHandler(uniGrid_OnAction);
        UniGrid.OnExternalDataBound += new OnExternalDataBoundEventHandler(UniGrid_OnExternalDataBound);
        UniGrid.ZeroRowsText = GetString("general.nodatafound");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        
        hdrActions.ActionsList.Add(new HeaderAction()
        {
            //Text = GetString("COM_ShippingOption_List.NewItemCaption"),
            Text = "Add price range",
            RedirectUrl = ResolveUrl("ShippingExtension_AddPrice.aspx?ItemID=" + mShippingExtensionPricingID),
            ImageUrl = GetImageUrl("Objects/Ecommerce_ShippingOption/add.png"),
            ControlType = HeaderActionTypeEnum.LinkButton,
            Visible =false
            
        });

        grdAction.ActionsList.Add(new HeaderAction()
        {
            //Text = GetString("COM_ShippingOption_List.NewItemCaption"),
            Text = "Add a price",
            RedirectUrl = ResolveUrl("ShippingExtension_AddPrice.aspx?ItemID=" + mShippingExtensionPricingID),
            ImageUrl = GetImageUrl("Objects/Ecommerce_ShippingOption/add.png"),
            ControlType = HeaderActionTypeEnum.LinkButton ,
            //UseImageButton = true,
            Visible = true
        });
        GetAndUpdateCustomTableQueryItem();
        // Initializes page title and breadcrumbs
        GetShippingOptionName(mShippingExtensionPricingID.ToString());
        string[,] breadcrumbs = new string[3, 3];
        breadcrumbs[0, 0] = "Shipping Extension";
        breadcrumbs[0, 1] = "~/CMSModules/Ecommerce/Pages/Tools/Configuration/ShippingExtension/ShippingExtension_List.aspx";
        breadcrumbs[0, 2] = "configEdit";
        breadcrumbs[1, 0] = ShippingOptionDisplayName;
        breadcrumbs[1, 1] = string.Format("~/CMSModules/Ecommerce/Pages/Tools/Configuration/ShippingExtension/ShippingExtension_Edit_Country.aspx?shippingExtensionID={0}", mShippingOptionID.ToString());
        breadcrumbs[1, 2] = "configEdit";
        breadcrumbs[2, 0] = ShippingCountryDisplayName;
        breadcrumbs[2, 1] = "";
        breadcrumbs[2, 2] = "";
        if (!IsPostBack)
        {
            txtBasePrice.Text = ShippingCountryBaseCost.ToString();
            txtUnitPrice.Text = ShippingCountryUnitPrice.ToString();
            pnlPriceParams.GroupingText = string.Format("Parameters for {1}/{0}", ShippingCountryDisplayName, ShippingOptionDisplayName);
            rdoByRange.Checked = (mShippingOptionProcessingMode == 0);
            rdoByUnit.Checked = (mShippingOptionProcessingMode == 1);
        }
        CMSMasterPage master = (CMSMasterPage)CurrentMaster;
        master.Title.Breadcrumbs = breadcrumbs;

    }

    protected void uniGrid_OnAction(string actionName, object actionArgument)
    {
        if (actionName == "delete")
        {
            if (GetAndDeleteCustomTableQueryItem(actionArgument.ToString()))
            {
                Response.Redirect(Request.Url.ToString());
            }
            else
            {
                ShowError("Only last line of the grid can be deleted");
            }
        }
        if (actionName == "edit")
        {
            //URLHelper.Redirect(ResolveUrl("ShippingExtension_EditPrice.aspx?ItemID=" + mShippingExtensionPricingID.ToString()));
            URLHelper.Redirect(string.Format("ShippingExtension_EditPrice.aspx?ItemID={0}&PriceID={1}", mShippingExtensionPricingID.ToString(), actionArgument.ToString()));
        }

    }

    private bool GetAndDeleteCustomTableQueryItem(string ItemID)
    {
        bool result = false;
        GeneralConnection cn = ConnectionHelper.GetConnection();
        object max = cn.ExecuteScalar(string.Format("select max(itemid) from customtable_shippingextensionpricing where shippingextensioncountryid={0}", mShippingExtensionPricingID.ToString()), null, QueryTypeEnum.SQLQuery, false);
        if (max.ToString() == ItemID)
        {
            cn.ExecuteNonQuery(string.Format("DELETE FROM customtable_shippingextensionpricing WHERE ItemID={0}", ItemID), null, QueryTypeEnum.SQLQuery, false);
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Sets data to database.
    /// </summary>
    protected void btnOK_Click(object sender, EventArgs e)
    {
        System.Globalization.CultureInfo before = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture =
        new System.Globalization.CultureInfo("en-US");

        decimal ShippingBaseCost = 0, ShippingUnitPrice =0 ;
        if (!string.IsNullOrEmpty(txtBasePrice.Text))
        {
            // Ensures decimal value entered in txtPrice if txtShippingUnitTo is filled
            var basedCost = txtBasePrice.Text;
            try
            {
                basedCost = basedCost.Replace(',', '.');
                    
                var culture = new System.Globalization.CultureInfo("en-US");
                ShippingBaseCost = Convert.ToDecimal(basedCost, culture);
                SaveShippingCost(ShippingBaseCost,"ShippingBase"); 
            }
            catch
            {
                
                ShowError(string.Format("Base cost <u><b>{0}</b></u> {0} is not valid", txtBasePrice.Text));
                return;
            }
        }
        if (!string.IsNullOrEmpty(txtUnitPrice.Text))
        {
            // Ensures decimal value entered in txtPrice if txtShippingUnitTo is filled
            var unitPrice = txtUnitPrice.Text;
            try
            {
                unitPrice = unitPrice.Replace(',', '.');
                var culture = new System.Globalization.CultureInfo("en-US");
                ShippingUnitPrice = Convert.ToDecimal(unitPrice, culture);
                SaveShippingCost(ShippingUnitPrice,"UnitPrice"); 
            }
            catch
            {
                ShowError(string.Format("Unit price <u><b>{0}</b></u> {0} is not valid", txtUnitPrice.Text));
                return;
            }
        }
        System.Threading.Thread.CurrentThread.CurrentUICulture = before;
        ShowChangesSaved();
        
    }

    private void GetAndUpdateCustomTableQueryItem()
    {
        GeneralConnection cn = ConnectionHelper.GetConnection();

        DataSet ds = cn.ExecuteQuery(GetQuery(), null, QueryTypeEnum.SQLQuery, false);
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            UniGrid.DataSource = ds;
        }
    }

    private string GetQuery()
    {
        string result = string.Format("select itemid, ShippingUnitFrom, case WHEN ShippingUnitTo=-1 THEN '' ELSE CAST(ShippingUnitTo as Varchar(50)) END AS ShippingUnitTo, case WHEN ShippingUnitPrice=0 THEN '' ELSE CAST(ShippingUnitPrice as Varchar(50)) END AS ShippingUnitPrice  from customtable_shippingextensionpricing where shippingextensioncountryid={0} order by shippingunitfrom", mShippingExtensionPricingID.ToString());
        return result;
    }

    #region "Methods"

    private void GetShippingOptionName(string ShippingOptionID)
    {
        GeneralConnection cn = ConnectionHelper.GetConnection();
        string stringQuery = string.Format("SELECT S.ItemID, C.ShippingOptionID, S.ShippingCountryId, s.UnitPrice,s.ProcessingMode, C.ShippingOptionDisplayName, dbo.CMS_Country.CountryDisplayName, S.ShippingBase FROM dbo.customtable_shippingextensioncountry AS S INNER JOIN dbo.COM_ShippingOption AS C ON C.ShippingOptionID = S.ShippingOptionId INNER JOIN dbo.CMS_Country ON S.ShippingCountryId = dbo.CMS_Country.CountryID WHERE S.ItemID = {0}", mShippingExtensionPricingID.ToString());
        DataSet ds = cn.ExecuteQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);

        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            ShippingOptionDisplayName = ValidationHelper.GetString(ds.Tables[0].Rows[0]["ShippingOptionDisplayName"], string.Empty);
            ShippingCountryDisplayName = ValidationHelper.GetString(ds.Tables[0].Rows[0]["CountryDisplayName"], string.Empty);
            ShippingCountryBaseCost = (decimal)ValidationHelper.GetDouble(ds.Tables[0].Rows[0]["ShippingBase"], 0);
            ShippingCountryUnitPrice = (decimal)ValidationHelper.GetDouble(ds.Tables[0].Rows[0]["UnitPrice"], 0);
            mShippingOptionID = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["ShippingOptionID"], -1);
            mShippingOptionProcessingMode = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["ProcessingMode"], 0);
        }
    }

    private void SaveShippingCost(decimal cost, string field)
    {
        string processing = "0";
        if (rdoByUnit.Checked)
        {
            processing = "1";
        }
        
        GeneralConnection cn = ConnectionHelper.GetConnection();
        string stringQuery = string.Format("UPDATE dbo.customtable_shippingextensioncountry SET {2}={0}, ProcessingMode={3} WHERE ItemID={1}", cost.ToString(), mShippingExtensionPricingID.ToString(), field, processing);
        cn.ExecuteNonQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);   
    }



    private object UniGrid_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerCSafe())
        {
            case "shippoptenabled":
                return UniGridFunctions.ColoredSpanYesNo(parameter);
            case "shippingoptionsiteid":
                return UniGridFunctions.ColoredSpanYesNo(parameter == DBNull.Value);
            case "shippoptcharge":
                DataRowView row = (DataRowView)parameter;
                double value = ValidationHelper.GetDouble(row["ShippingOptionCharge"], 0);
                int siteId = ValidationHelper.GetInteger(row["ShippingOptionSiteID"], 0);

                return CurrencyInfoProvider.GetFormattedPrice(value, siteId);
        }

        return parameter;
    }

    #endregion

}