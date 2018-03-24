﻿using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;


using CMS.CMSHelper;
using CMS.Ecommerce;
using CMS.EcommerceProvider;
using CMS.GlobalHelper;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.EventLog;
using CMS.DataEngine;
using System.Data.SqlClient;
using System.Configuration;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Globalization;
using CMS.Protection;

public partial class CMSModules_Ecommerce_Controls_ShoppingCart_ShoppingCartOrderAddresses : ShoppingCartStep
{
    class Bundle
    {
        public int BundleId
        {
            get;
            set;
        }

        public int Quantity
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public int Total
        {
            get;
            set;
        }


    }

    class BundledItem
    {
        public int BundleID
        {
            get;
            set;
        }

        public int ProductID
        {
            get;
            set;
        }
    }

	private readonly EventLogProvider p = new EventLogProvider();
    private List<Bundle> availableBundles = new List<Bundle>();
    private List<BundledItem> bundledItems = new List<BundledItem>();
    private DataSet dsProd;

    #region "ViewState Constants"

    // Constants for billing address
    private const string BILLING_ADDRESS_ID = "BillingAddressID";
    private const string BILLING_ADDRESS_NAME = "BillingAddressName";
    private const string BILLING_ADDRESS_LINE1 = "BillingAddressLine1";
    private const string BILLING_ADDRESS_LINE2 = "BillingAddressLine2";
    private const string BILLING_ADDRESS_CITY = "BillingAddressCity";
    private const string BILLING_ADDRESS_ZIP = "BillingAddressZIP";
    private const string BILLING_ADDRESS_COUNTRY_ID = "BillingAddressCountryID";
    private const string BILLING_ADDRESS_STATE_ID = "BillingAddressStateID";
    private const string BILLING_ADDRESS_PHONE = "BillingAddressPhone";

    // Constants for shipping address
    private const string SHIPPING_ADDRESS_CHECKED = "ShippingAddressChecked";
    private const string SHIPPING_ADDRESS_ID = "ShippingAddressID";
    private const string SHIPPING_ADDRESS_NAME = "ShippingAddressName";
    private const string SHIPPING_ADDRESS_LINE1 = "ShippingAddressLine1";
    private const string SHIPPING_ADDRESS_LINE2 = "ShippingAddressLine2";
    private const string SHIPPING_ADDRESS_CITY = "ShippingAddressCity";
    private const string SHIPPING_ADDRESS_ZIP = "ShippingAddressZIP";
    private const string SHIPPING_ADDRESS_COUNTRY_ID = "ShippingAddressCountryID";
    private const string SHIPPING_ADDRESS_STATE_ID = "ShippingAddressStateID";
    private const string SHIPPING_ADDRESS_PHONE = "ShippingAddressPhone";

    // Constants for company address
    private const string COMPANY_ADDRESS_CHECKED = "CompanyAddressChecked";
    private const string COMPANY_ADDRESS_ID = "CompanyAddressID";
    private const string COMPANY_ADDRESS_NAME = "CompanyAddressName";
    private const string COMPANY_ADDRESS_LINE1 = "CompanyAddressLine1";
    private const string COMPANY_ADDRESS_LINE2 = "CompanyAddressLine2";
    private const string COMPANY_ADDRESS_CITY = "CompanyAddressCity";
    private const string COMPANY_ADDRESS_ZIP = "CompanyAddressZIP";
    private const string COMPANY_ADDRESS_COUNTRY_ID = "CompanyAddressCountryID";
    private const string COMPANY_ADDRESS_STATE_ID = "CompanyAddressStateID";
    private const string COMPANY_ADDRESS_PHONE = "CompanyAddressPhone";
	
	private bool? mIsShippingNeeded = null;

    #endregion

	#region "Properties"
	protected bool IsShippingNeeded
    {
        get
        {
            if (mIsShippingNeeded.HasValue)
            {
                return mIsShippingNeeded.Value;
            }
            else
            {
                if (ShoppingCart != null)
                {
                    // Figure out from shopping cart
                    mIsShippingNeeded = ShippingOptionInfoProvider.IsShippingNeeded(ShoppingCart);
                    return mIsShippingNeeded.Value;
                }
                else
                {
                    return true;
                }
            }
        }
    }
	#endregion

    #region "Temporary values operations"

    /// <summary>
    /// Removes billing address values from ShoppingCart ViewState.
    /// </summary>
    private void RemoveBillingTempValues()
    {
        // Billing address values
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_ID, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_CITY, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_COUNTRY_ID, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_LINE1, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_LINE2, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_NAME, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_PHONE, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_STATE_ID, null);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_ZIP, null);
    }


    /// <summary>
    /// Removes shipping address values from ShoppingCart ViewState.
    /// </summary>
    private void RemoveShippingTempValues()
    {
        // Shipping address values
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_CHECKED, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_ID, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_CITY, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_COUNTRY_ID, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_LINE1, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_LINE2, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_NAME, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_PHONE, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_STATE_ID, null);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_ZIP, null);
    }


    /// <summary>
    /// Removes company address values from ShoppingCart ViewState.
    /// </summary>
    private void RemoveCompanyTempValues()
    {
        // Company address values
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_CHECKED, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_ID, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_CITY, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_COUNTRY_ID, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_LINE1, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_LINE2, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_NAME, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_PHONE, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_STATE_ID, null);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_ZIP, null);
    }

    /// <summary>
    /// Loads shipping address temp values.
    /// </summary>
    private void LoadShippingFromViewState()
    {
        /*
        txtShippingName.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_NAME));
        txtShippingAddr1.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_LINE1));
        txtShippingAddr2.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_LINE2));
        txtShippingCity.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_CITY));
        txtShippingZip.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_ZIP));
        txtShippingPhone.Text = Convert.ToString(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_PHONE));
        CountrySelector2.CountryID = ValidationHelper.GetInteger(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_COUNTRY_ID), 0);
        CountrySelector2.StateID = ValidationHelper.GetInteger(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_STATE_ID), 0);*/
    }

    /// <summary>
    /// Back button actions.
    /// </summary>
    public override void ButtonBackClickAction()
    {
        /*
        // Billing address values
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_ID, drpBillingAddr.SelectedValue);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_CITY, txtBillingCity.Text);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_COUNTRY_ID, CountrySelector1.CountryID);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_LINE1, txtBillingAddr1.Text);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_LINE2, txtBillingAddr2.Text);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_NAME, txtBillingName.Text);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_PHONE, txtBillingPhone.Text);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_STATE_ID, CountrySelector1.StateID);
        ShoppingCartControl.SetTempValue(BILLING_ADDRESS_ZIP, txtBillingZip.Text);

        // Shipping address values
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_CHECKED, chkShippingAddr.Checked);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_ID, drpShippingAddr.SelectedValue);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_CITY, txtShippingCity.Text);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_COUNTRY_ID, CountrySelector2.CountryID);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_LINE1, txtShippingAddr1.Text);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_LINE2, txtShippingAddr2.Text);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_NAME, txtShippingName.Text);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_PHONE, txtShippingPhone.Text);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_STATE_ID, CountrySelector2.StateID);
        ShoppingCartControl.SetTempValue(SHIPPING_ADDRESS_ZIP, txtShippingZip.Text);

        // Company address values
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_CHECKED, chkCompanyAddress.Checked);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_ID, drpCompanyAddress.SelectedValue);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_CITY, txtCompanyCity.Text);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_COUNTRY_ID, CountrySelector3.CountryID);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_LINE1, txtCompanyLine1.Text);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_LINE2, txtCompanyLine2.Text);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_NAME, txtCompanyName.Text);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_PHONE, txtCompanyPhone.Text);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_STATE_ID, CountrySelector3.StateID);
        ShoppingCartControl.SetTempValue(COMPANY_ADDRESS_ZIP, txtCompanyZip.Text);
        */
        base.ButtonBackClickAction();
    }

    #endregion


    /// <summary>
    /// Private properties.
    /// </summary>
    private int mCustomerId = 0, ShippingUnit = 0;

    private SiteInfo mCurrentSite = null;

    private void ShowAdresses(bool billing, bool shipping)
    {
       // EventLogProvider ev = new EventLogProvider();
        string where = string.Empty, orderby = string.Empty;
        AddressInfo ai;
        DataSet ds, dsoi = null;
        #region billing
        if (billing)
        {
            ai = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartBillingAddressID);
            /*if (!ai.AddressIsBilling)
            {
                ai = null;
            }*/
               
                if (ai == null)
                {
                    where = string.Format("OrderCustomerID={0}", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    orderby = "OrderID DESC";
                    dsoi = OrderInfoProvider.GetOrderList(where, orderby);
                    if (!DataHelper.DataSourceIsEmpty(dsoi))
                    {
                        foreach (DataRow drow in dsoi.Tables[0].Rows)
                        {
                            OrderInfo oi = new OrderInfo(drow);
                            AddressInfo bai = AddressInfoProvider.GetAddressInfo(oi.OrderBillingAddressID);
                            if (bai.AddressEnabled && bai.AddressIsBilling)
                            {
                                ai = bai;
                                ShoppingCart.ShoppingCartBillingAddressID = bai.AddressID;
                                break;
                            }
                        }
                    }
                }
                if (ai == null)
                {
                    where = string.Format("AddressCustomerID={0} AND AddressIsBilling=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    orderby = "AddressID";
                    ds = AddressInfoProvider.GetAddresses(where, orderby);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        ai = new AddressInfo(ds.Tables[0].Rows[0]);
                        ShoppingCart.ShoppingCartBillingAddressID = ai.AddressID;
                    }
                }
                if (ai != null)
                {
                    if (ai.AddressEnabled)
                    {
                        //lblBillingAddressFullName.Text = ECommerceContext.CurrentCustomer.CustomerLastName + " " + ECommerceContext.CurrentCustomer.CustomerFirstName;//ai.AddressPersonalName;
                        //lblBillingAddressFullName.Text = ECommerceContext.CurrentCustomer.CustomerFirstName + " " + ECommerceContext.CurrentCustomer.CustomerLastName;//ai.AddressPersonalName;
                        lblBillingAddressFullName.Text = string.IsNullOrEmpty(ai.AddressPersonalName) ? ECommerceContext.CurrentCustomer.CustomerFirstName + " " + ECommerceContext.CurrentCustomer.CustomerLastName : ai.AddressPersonalName;
                        lblBillingAddressStreet.Text = string.IsNullOrEmpty(ai.AddressLine2) ? ai.AddressLine1 : string.Format("{0}, {1}", ai.AddressLine1, ai.AddressLine2);
                        lblBillingAddressStreet.Text = string.Format("{0} {1}", ai.GetStringValue("AddressNumber", string.Empty), lblBillingAddressStreet.Text).Trim();
                        lblBillingAddressZipCode.Text = ai.AddressZip + "," + ai.AddressCity + "<br/>";
                        lblBillingAddressCityCountry.Text = string.Format("{0}", MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));
                    }          //  lblBillingAddressCityCountry.Text = string.Format("{0}, {1}", ai.AddressCity, CMSContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));

                    else
                    {
                        // recup�ration de l'ID enabled and is billing 
                        int idAdr = 0;
                        int idCustomer = ECommerceContext.CurrentCustomer.CustomerID;
                        SqlConnection con4 = new SqlConnection(ConfigurationManager.ConnectionStrings["CMSConnectionString"].ConnectionString);
                        var query = "SELECT TOP 1 AddressID FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY AddressID desc) as row FROM COM_Address) a WHERE AddressIsBilling = 1 AND AddressEnabled = 1 AND AddressCustomerID =" + idCustomer;
                        var query2 = "SELECT TOP 1 AddressID FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY AddressID desc) as row FROM COM_Address) a WHERE AddressEnabled = 1 AND AddressCustomerID =" + idCustomer;
                        SqlCommand cmd2 = new SqlCommand(query, con4);
                        SqlCommand cmd3 = new SqlCommand(query2, con4);
                        con4.Open();
                        try
                        {
                            idAdr = (int)cmd2.ExecuteScalar();
                          //  ev.LogEvent("I", DateTime.Now, "dans try = " + idAdr, "code");
                        }
                        catch (Exception ex)
                        {
                            idAdr = (int)cmd3.ExecuteScalar();
                            //ev.LogEvent("I", DateTime.Now, "dans catch r�cup�ration adress billing ", "code");
                        }
                        con4.Close();
                        ai = AddressInfoProvider.GetAddressInfo(idAdr);

                        lblBillingAddressFullName.Text = ECommerceContext.CurrentCustomer.CustomerFirstName + " " + ECommerceContext.CurrentCustomer.CustomerLastName;//ai.AddressPersonalName;
                        lblBillingAddressStreet.Text = string.IsNullOrEmpty(ai.AddressLine2) ? ai.AddressLine1 : string.Format("{0}, {1}", ai.AddressLine1, ai.AddressLine2);
                        lblBillingAddressStreet.Text = string.Format("{0} {1}", ai.GetStringValue("AddressNumber", string.Empty), lblBillingAddressStreet.Text).Trim();
                        lblBillingAddressZipCode.Text = ai.AddressZip + "," + ai.AddressCity + "<br/>";
                        lblBillingAddressCityCountry.Text = string.Format("{0}", MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));

                    }
                }//fin if ai!=null

        }// fin billing
        #endregion

        #region shipping
        if (IsShippingNeeded && shipping)
        {
            ai = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID);
            /*if (!ai.AddressIsShipping)
            {
                ai = null;
            }*/
          //  ev.LogEvent("I", DateTime.Now, "dans if shipping", "code");
            if (ai == null)
            {
                if (DataHelper.DataSourceIsEmpty(dsoi))
                {
                    where = string.Format("OrderCustomerID={0}", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    orderby = "OrderID DESC";
                    dsoi = OrderInfoProvider.GetOrderList(where, orderby);
                }
                if (!DataHelper.DataSourceIsEmpty(dsoi))
                {
                    foreach (DataRow drow in dsoi.Tables[0].Rows)
                    {
                        OrderInfo oi = new OrderInfo(drow);
                        AddressInfo sai = AddressInfoProvider.GetAddressInfo(oi.OrderShippingAddressID);
                        if (sai !=null && sai.AddressEnabled && sai.AddressIsShipping)
                        {
                            ai = sai;
                            ShoppingCart.ShoppingCartShippingAddressID = sai.AddressID;
                            break;
                        }
                    }
                }
            }
            if (ai == null)
            {
                where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                orderby = "AddressID";
                ds = AddressInfoProvider.GetAddresses(where, orderby);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    ai = new AddressInfo(ds.Tables[0].Rows[0]);

                    ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
                }
                else
                {
                    // NO SHIPPING ADDRESS DEFINED- PICK FIRST BILLING ADDRESS    
                    AddressInfo ai_shipping = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartBillingAddressID);
                    ai_shipping.AddressIsShipping = true;
                    AddressInfoProvider.SetAddressInfo(ai_shipping);
                    where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    ds = AddressInfoProvider.GetAddresses(where, orderby);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        ai = new AddressInfo(ds.Tables[0].Rows[0]);
                        ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
                    }

                }
            }
            
            //lblShippingAddressFullName.Text = ECommerceContext.CurrentCustomer.CustomerLastName + " " + ECommerceContext.CurrentCustomer.CustomerFirstName; //ai.AddressPersonalName;
            lblShippingAddressFullName.Text = ECommerceContext.CurrentCustomer.CustomerFirstName + " " + ECommerceContext.CurrentCustomer.CustomerLastName; //ai.AddressPersonalName;
            lblShippingAddressStreet.Text = string.IsNullOrEmpty(ai.AddressLine2) ? ai.AddressLine1 : string.Format("{0}, {1}", ai.AddressLine1, ai.AddressLine2);
            lblShippingAddressStreet.Text = string.Format("{0} {1}", ai.GetStringValue("AddressNumber", string.Empty), lblShippingAddressStreet.Text).Trim();
            lblShippingAddressZipCode.Text = ai.AddressZip + "," + ai.AddressCity+"<br/>";
            lblShippingAddressCityCountry.Text = string.Format("{0}", MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));

            // reloader le champ shipping price
          //  EventLogProvider ppp = new EventLogProvider();
         //   ppp.LogEvent("W", DateTime.Now,"ai = "+ai.AddressID,"code front");

            
            ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
            DisplayTotalPrice();

            ReloadData();
            // SetSessionParams(ai);
        }// fin shipping

        #endregion




        double vat = ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart);
        if (vat > 0)
        {
            vat = 1.06;
        }
        else
        {
            vat = 1;
        }
		
		var addrezz = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID);
        if(addrezz!=null){
			var newCountryId = addrezz.AddressCountryID;
			QueryDataParameters parameters = new QueryDataParameters();
			parameters.Add("@ShippingUnits", ShippingUnit);
			parameters.Add("@CountryID", newCountryId);
			parameters.Add("@VATRate", vat);
			//parameters.Add("@VATRate", 1 + ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart) / 100);
			GeneralConnection cn = ConnectionHelper.GetConnection();
			ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters);
			cn.Close();
			if (!DataHelper.DataSourceIsEmpty(ds))
			{
				DataTable dt = ds.Tables[0];
				foreach (DataRow drow in dt.Rows)
				{
					double price = Convert.ToDouble(drow["ShippingFinalCost"]);
					string prices = CurrencyInfoProvider.GetFormattedPrice(price, ShoppingCart.Currency);
					drow["DisplayString"] = string.Format("{0}- {1}", drow["ShippingOptionDisplayName"].ToString(), prices);
				}

				ddlShippingOption.DataSource = ds;
				ddlShippingOption.SelectedIndex = -1;
				ddlShippingOption.SelectedValue = null;
				ddlShippingOption.ClearSelection();
				ddlShippingOption.DataTextField = "DisplayString";
				ddlShippingOption.DataValueField = "ItemID";
				ddlShippingOption.DataBind();
                ListItem listItem = new ListItem("Votre choix", "-1");
                listItem.Selected = true;
                ddlShippingOption.Items.Add(listItem);
				ddlShippingOption.AutoPostBack = (ddlShippingOption.Items.Count > 1);
				// string value = ValidationHelper.GetString(SessionHelper.GetValue("CarriedOnPriceID"), string.Empty);
				string value = ValidationHelper.GetString(ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartCarriedOnPriceID"), string.Empty);

				if (!string.IsNullOrEmpty(value) && ddlShippingOption.Items.Count > 1)
				{
					if (int.Parse(value) > 0)
					{
						// SessionHelper.SetValue("CarriedOnPriceID", string.Empty);
						ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCarriedOnPriceID", -1);
						try
						{
							ddlShippingOption.SelectedValue = value;
						}
						catch
						{
						}
					}
				}
				//int PriceID = ValidationHelper.GetInteger(ddlShippingOption.SelectedValue, -1);
				//SessionHelper.SetValue("PriceID", PriceID);

				// SessionHelper.SetValue("CountryID", ai.AddressCountryID);
				ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCountryID", newCountryId );

				ddlShippingOption_SelectedIndexChanged(null, null);
				//btnUpdate_Click1(null, null);
			}

			else
			{
				// NO SHIPPING AVAILABLE
				ddlShippingOption.Items.Clear();
				ddlShippingOption.DataSource = null;
				ListItem listItem = new ListItem("Votre choix", "-1");
				ddlShippingOption.Items.Add(listItem);
			}
		}
        DisplayTotalPrice();
    }

    private void ShowAdressesList(bool bill, bool ship)
    {
        List<AddressInfo> ais = ShippingExtendedInfoProvider.GetAdresses(bill, ship, ShoppingCart);
        if (ais.Count == 2)
        {
            AddressInfo ai = ais[1];
            lblShippingAddressFullName.Text = ai.AddressPersonalName;
            lblShippingAddressStreet.Text = string.IsNullOrEmpty(ai.AddressLine2) ? ai.AddressLine1 : string.Format("{0}, {1}", ai.AddressLine1, ai.AddressLine2);
            lblShippingAddressZipCode.Text = ai.AddressZip;
            lblShippingAddressCityCountry.Text = string.Format("{0}, {1}", ai.AddressCity, CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName);

           // EventLogProvider elp1 = new EventLogProvider();
            double vat = ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart);
       //     elp1.LogEvent("I", DateTime.Now, "valeur1 vat in orderadress = " + vat, "code");
            if (vat > 0)
            {
                vat = 1.06;
             //   elp1.LogEvent("I", DateTime.Now, "vat1 in order address : " + vat, "code");
            }
            else
            {
                vat = 1;
            }
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ShippingUnits", ShippingUnit);
            parameters.Add("@CountryID", ai.AddressCountryID);
            parameters.Add("@VATRate", vat);
            //parameters.Add("@VATRate", 1 + ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart) / 100);
            GeneralConnection cn = ConnectionHelper.GetConnection();
            DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters);
            cn.Close();
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataTable dt = ds.Tables[0];
                foreach (DataRow drow in dt.Rows)
                {
                    double price = Convert.ToDouble(drow["ShippingFinalCost"]);
                    string prices = CurrencyInfoProvider.GetFormattedPrice(price, ShoppingCart.Currency);
                    drow["DisplayString"] = string.Format("{0}- {1}", drow["ShippingOptionDisplayName"].ToString(), prices);
                }
                ddlShippingOption.DataSource = ds;
                ddlShippingOption.DataTextField = "DisplayString";
                ddlShippingOption.DataValueField = "ItemID";
                ddlShippingOption.DataBind();
                int PriceID = ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID");
                if (PriceID > 0)
                {
                    ddlShippingOption.SelectedValue = PriceID.ToString();
                }
                // int PriceID = ValidationHelper.GetInteger(ddlShippingOption.SelectedValue, -1);
                // SessionHelper.SetValue("PriceID", PriceID);
                // SessionHelper.SetValue("CountryID", ai.AddressCountryID);
                ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCountryID", ai.AddressCountryID);

                ddlShippingOption_SelectedIndexChanged(null, null);
                //btnUpdate_Click1(null, null);
            }

            else
            {
                // NO SHIPPING AVAILABLE
                ddlShippingOption.Items.Clear();
                ddlShippingOption.DataSource = null;
                ListItem listItem = new ListItem("Votre choix", "-1");
                ddlShippingOption.Items.Add(listItem);
            }
            ai = ais[0];
            lblBillingAddressFullName.Text = ai.AddressPersonalName;
            lblBillingAddressStreet.Text = string.IsNullOrEmpty(ai.AddressLine2) ? ai.AddressLine1 : string.Format("{0}, {1}", ai.AddressLine1, ai.AddressLine2);
            lblBillingAddressZipCode.Text = ai.AddressZip;
            lblBillingAddressCityCountry.Text = string.Format("{0}, {1}", ai.AddressCity, CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName);
        }
    }


    /// <summary>
    /// Reloads the shipping option data after selecting a shipping adress.
    /// </summary>
    /// <summary>
    private void ReloadShippingOptions()
    {
        return;
        int CountryID = (AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID)).AddressCountryID;
        // SessionHelper.SetValue("CountryID", CountryID);
        // SessionHelper.SetValue("PriceID", -1);
        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID", CountryID);
        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCountryID", -1);

        int ShippingCartUnits = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);

        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", ShippingCartUnits);
        parameters.Add("@CountryID", CountryID);
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters);
        cn.Close();
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            DataRow drow = ds.Tables[0].Rows[0];
            int PriceID = (int)drow["ItemID"];
            // SessionHelper.SetValue("PriceID", PriceID);
            DataTable dt = ds.Tables[0];
            foreach (DataRow drowid in dt.Rows)
            {
                double price = Convert.ToDouble(drowid["ShippingFinalCost"]);
                string prices = CurrencyInfoProvider.GetFormattedPrice(price, ShoppingCart.Currency);
                drowid["DisplayString"] = string.Format("{0}- {1}", drowid["ShippingOptionDisplayName"].ToString(), prices);
            }
            ddlShippingOption.DataSource = ds;
            ddlShippingOption.DataTextField = "DisplayString";
            ddlShippingOption.DataValueField = "ItemID";
            ddlShippingOption.DataBind();
            //string value = ValidationHelper.GetString(SessionHelper.GetValue("PriceID"), string.Empty);
            string value = ValidationHelper.GetString(ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID"), string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                if (int.Parse(value) > 0)
                {
                    ddlShippingOption.SelectedValue = value;
                }
            }

        }
        DisplayTotalPrice();
    }

    /// Page load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
         //bt.CssClass = ResHelper.LocalizeString("btn.adressefacture");
        InitializeLabel();
        ShippingUnit = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
        EventLogProvider.LogInformation(ECommerceContext.CurrentCustomer.CustomerID.ToString() + " " + CurrentUser.UserID, "I");
        // *** SERVRANX START***
        bundledItems.Clear();
        ShowPaymentList();
        if (!rdbVisa.Checked && !rdbMaestro.Checked && !rdbMastercard.Checked)
        {
            rdbVisa.Checked = true;
            rdoBtn_CheckedChanged(null, null);
        }

        //ddlPaymentOption.SelectedValue = SessionHelper.GetValue("PaymentID").ToString();
        //A demander � tovo pourquoi une duplication
        int PaymentID = ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPaymentID");
       // EventLogProvider ev = new EventLogProvider();
       // ev.LogEvent("E", DateTime.Now, "nb1", PaymentID.ToString());
        if (PaymentID > 0)
        {
            ddlPaymentOption_SelectedIndexChanged(null, null);
        }

        ReloadData();
        // *** SERVRANX END***
        mCurrentSite = SiteContext.CurrentSite;


        //lblBillingTitle.Text = GetString("ShoppingCart.BillingAddress");
        //lblShippingTitle.Text = GetString("ShoppingCart.ShippingAddress");
        //lblCompanyAddressTitle.Text = GetString("ShoppingCart.CompanyAddress");

        // Initialize labels.
        // LabelInitialize();
        //this.TitleText = GetString("Order_new.ShoppingCartOrderAddresses.Title");

        // Get customer ID from ShoppingCartInfoObj
        mCustomerId = ShoppingCart.ShoppingCartCustomerID;
        // verify coupon
        double couponvalue = ShoppingCart.OrderDiscount;
        if (couponvalue != 0.0)
        {
            sanscoupon2.Visible = true;
            totalcoupon2.Visible = true;
        }
        else
        {
            sanscoupon2.Visible = false;
            sanscoupon2.Visible = false;
        }


        // Get customer info.
        CustomerInfo ci = CustomerInfoProvider.GetCustomerInfo(mCustomerId);

        if (ci != null)
        {
            // Display customer addresses if customer is not anonymous
            if (ci.CustomerID > 0)
            {
                if (!ShoppingCartControl.IsCurrentStepPostBack)
                {
                    // Initialize customer billing and shipping addresses
                    InitializeAddresses();
                }
            }
        }

        // If shopping cart does not need shipping
        if (!ShippingOptionInfoProvider.IsShippingNeeded(ShoppingCart))
        {
            // Hide title
            lblBillingTitle.Visible = false;

            // Change current checkout process step caption
            ShoppingCartControl.CheckoutProcessSteps[ShoppingCartControl.CurrentStepIndex].Caption = GetString("order_new.shoppingcartorderaddresses.titlenoshipping");
        }
        ShowAdresses(true, true);
        ReloadShippingOptions();
        
    }

    private string GetBundleBody()
    {
        string firstMarker = "<b>", secondMarker = "</b>"; 
        string result = GetString("FreeBundleList");
        if (!string.IsNullOrEmpty(result) && result.IndexOf(firstMarker) >= 0 && result.IndexOf(secondMarker) >=0)
        {
            string part1 = result.Substring(0, result.IndexOf(firstMarker)).Trim();
            result = result.Substring(result.IndexOf(firstMarker) + firstMarker.Length);
            string part2 = result.Substring(0, result.IndexOf(secondMarker)).Trim();
            part2 = string.Format("<a href='#popup_bundle' class='fancybox_list_adress'>{0}</a>", part2);
            result = result.Substring(result.IndexOf(secondMarker) + secondMarker.Length);
            result = string.Format("{0} {1} {2}", part1, part2, result);
        }
        return result;
    }

    protected void InitializeLabel()
    {
        ltlPopupBundleHeader.Text = GetString("FreeBundlePopupHeader");
        lblBundleBody.Text = GetBundleBody(); 
        LabelArticle.Text = GetString("article");
        LiteralAdresseDeFacturation.Text = GetString("adressefacturation");
        LiteralModifier.Text = GetString("modify");
        ltlAdresseLivraison.Text = GetString("adresselivraison");
        ltlModifier.Text = GetString("modify");
        LiteralOptionEnvoi.Text = GetString("optionenvoi");
        LiteralMoyenDePaiement.Text = GetString("moyenpaiement");
        LiteralCartePaiement.Text = GetString("cartepaiement ");
        LabelQuantite.Text = GetString("qte");
        LabelPrixTotal.Text = GetString("prixtotal");
        LabelSousTotal.Text = GetString("soustotal");
        LabelFraisEnvoi.Text = GetString("Fraisdenvoi");
        LabelMontantTotal.Text = GetString("prixtotalttc");
        wmBillingNumero.WatermarkText = GetString("numerorue");
        wmShippingNumero.WatermarkText = GetString("numerorue");
        wmBillingadresse1.WatermarkText = GetString("adresse1");
        wmShippingadresse1.WatermarkText = GetString("adresse1");
        wmBillingadresse2.WatermarkText = GetString("adresse2");
        wmShippingadresse2.WatermarkText = GetString("adresse2");
        wmBillingcp.WatermarkText = GetString("cp");
        wmShippingcp.WatermarkText = GetString("cp");
        wmBillingville.WatermarkText = GetString("ville");
        wmShippingville.WatermarkText = GetString("ville");
        LabelMontantCoupon.Text = GetString("texte.montantcoupon");
        LabelMontantDesAchatsSanscoupon.Text = GetString("texte.achatsanscoupon");
        // redirection vers mon compte si adresse active = 0 Response.Redirect("~/Special-Page/Mon-compte.aspx");

        int idCustomer = ECommerceContext.CurrentCustomer.CustomerID;
        SqlConnection con3 = new SqlConnection(ConfigurationManager.ConnectionStrings["CMSConnectionString"].ConnectionString);
        con3.Open();
        var stringQuery = "select count(AddressID) as NbAdress from COM_Address WHERE COM_Address.AddressEnabled = 'true'  AND COM_Address.AddressCustomerID  = " + idCustomer;
        SqlCommand cmd3 = new SqlCommand(stringQuery, con3);
        int nb = (int)cmd3.ExecuteScalar();
        con3.Close();
        if (nb == 0)
        {
            Response.Redirect("~/Special-Page/Mon-compte.aspx");
        }

    }

    /// <summary>
    /// Initialize customer's addresses in billing and shipping dropdown lists.
    /// </summary>
    protected void InitializeAddresses()
    {
        // add new item <(new), 0>
        ListItem li = new ListItem(GetString("ShoppingCartOrderAddresses.NewAddress"), "0");
        li = new ListItem(GetString("ShoppingCartOrderAddresses.NewAddress"), "0");

        LoadBillingSelectedValue();

        LoadShippingSelectedValue();
        LoadCompanySelectedValue();

        LoadBillingAddressInfo();
        LoadShippingAddressInfo();
    }


    protected void LoadBillingSelectedValue()
    {
        try
        {
            int lastBillingAddressId = 0;

            // Get last used shipping and billing addresses in the order
            DataSet ds = OrderInfoProvider.GetOrders("OrderCustomerID=" + mCustomerId, "OrderDate DESC");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                OrderInfo oi = new OrderInfo(ds.Tables[0].Rows[0]);
                lastBillingAddressId = oi.OrderBillingAddressID;
            }
        }
        catch
        {
        }
    }


    protected void LoadShippingSelectedValue()
    {
        try
        {
            int lastShippingAddressId = 0;

            // Get last used shipping and billing addresses in the order
            DataSet ds = OrderInfoProvider.GetOrders("OrderCustomerID=" + mCustomerId, "OrderDate DESC");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                OrderInfo oi = new OrderInfo(ds.Tables[0].Rows[0]);
                lastShippingAddressId = oi.OrderShippingAddressID;
            }

            // Try to select shipping address from ViewState first
            object viewStateValue = ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_ID);
            bool viewStateChecked = ValidationHelper.GetBoolean(ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_CHECKED), false);
        }
        catch
        {
        }
    }


    protected void LoadCompanySelectedValue()
    {
        try
        {
            int lastCompanyAddressId = 0;

            // Get last used shipping and billing addresses in the order
            DataSet ds = OrderInfoProvider.GetOrders("OrderCustomerID=" + mCustomerId, "OrderDate DESC");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                OrderInfo oi = new OrderInfo(ds.Tables[0].Rows[0]);
                lastCompanyAddressId = oi.OrderCompanyAddressID;
            }

            // Try to select company address from ViewState first
            object viewStateValue = ShoppingCartControl.GetTempValue(COMPANY_ADDRESS_ID);
            bool viewStateChecked = ValidationHelper.GetBoolean(ShoppingCartControl.GetTempValue(COMPANY_ADDRESS_CHECKED), false);
        }
        catch
        {
        }
    }

    /// <summary>
    /// On drpBillingAddr selected index changed.
    /// </summary>
    private void drpBillingAddr_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadBillingAddressInfo();
    }

    /// <summary>
    /// Clean specified part of the form.
    /// </summary>    
    private void CleanForm(bool billing, bool shipping, bool company)
    {
        int defaultCountryId = 0;
        int defaultStateId = 0;

        // Prefill country from customer if any
        if ((ShoppingCart != null) && (ShoppingCart.Customer != null))
        {
            defaultCountryId = ShoppingCart.Customer.CustomerCountryID;
            defaultStateId = ShoppingCart.Customer.CustomerStateID;
        }

        // Prefill default store country if customers country not found
        if ((defaultCountryId <= 0) && (SiteContext.CurrentSite != null))
        {
            string countryName = ECommerceSettings.DefaultCountryName(SiteContext.CurrentSite.SiteName);
            CountryInfo ci = CountryInfoProvider.GetCountryInfo(countryName);
            defaultCountryId = (ci != null) ? ci.CountryID : 0;
            defaultStateId = 0;
        }
    }

    /// <summary>
    /// Loads selected billing  address info.
    /// </summary>
    protected void LoadBillingAddressInfo()
    {
        /*
        // Try to select company address from ViewState first
        if (!ShoppingCartControl.IsCurrentStepPostBack && ShoppingCartControl.GetTempValue(BILLING_ADDRESS_ID) != null)
        {
            // LoadBillingFromViewState();
        }
        else
        {
            int addressId = 0;

            if (drpBillingAddr.SelectedValue != "0")
            {
                addressId = Convert.ToInt32(drpBillingAddr.SelectedValue);
            }
            else
            {
                // Clean billing part of the form
                CleanForm(true, false, false);
            }
        }*/
    }

    /// <summary>
    /// Loads selected shipping  address info.
    /// </summary>
    protected void LoadShippingAddressInfo()
    {
        /*
        int addressId = 0;

        // Load shipping info only if shipping part is visible
        if (plhShipping.Visible)
        {
            // Try to select company address from ViewState first
            if (!ShoppingCartControl.IsCurrentStepPostBack && ShoppingCartControl.GetTempValue(SHIPPING_ADDRESS_ID) != null)
            {
                LoadShippingFromViewState();
            }
            else
            {
                if (drpShippingAddr.SelectedValue != "0")
                {
                    addressId = Convert.ToInt32(drpShippingAddr.SelectedValue);
                }
                else
                {
                    // clean shipping part of the form
                    CleanForm(false, true, false);
                }
            }
        }*/
    }

    /// <summary>
    /// Check if the form is well filled.
    /// </summary>
    /// <returns>True or false.</returns>
    public override bool IsValid()
    {
        Validator val = new Validator();
        bool isEnvoiValid = IsEnvoiValid();
        if(!isEnvoiValid)
        {
            return isEnvoiValid;
        }

        return true;
    }

    private bool IsEnvoiValid()
    {
        if(!IsShippingNeeded)
            return true;
        if(Convert.ToInt32(ddlShippingOption.SelectedValue) == -1)
        {

            lblError.Text = "L'option d'envoi est obligatoire.";
            return false;
        }
        return true;
    }


    /// <summary>
    /// Process valid values of this step.
    /// </summary>
    public override bool ProcessStep()
    {
        // AddressInfo ai = null;
        if (mCustomerId > 0)
        {
            // Clean the viewstate
            RemoveBillingTempValues();
            RemoveShippingTempValues();
            RemoveCompanyTempValues();

            // Process billing address
            /*if (ai == null)
            {
                ai = new AddressInfo();
                newAddress = true;
            }

            if (newAddress)
            {
                ai.AddressIsBilling = true;
                ai.AddressEnabled = true;
            }
            ai.AddressCustomerID = mCustomerId;
            ai.AddressName = AddressInfoProvider.GetAddressName(ai);
            
            // Save address and set it's ID to ShoppingCartInfoObj
            AddressInfoProvider.SetAddressInfo(ai);*/

            // Update current contact's address
            ModuleCommands.OnlineMarketingMapAddress(AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartBillingAddressID), ContactID);

			p.LogEvent("I", DateTime.Now, "IsShippingNeeded :" +IsShippingNeeded.ToString() , "");
            // If shopping cart does not need shipping
            if (!ShippingOptionInfoProvider.IsShippingNeeded(ShoppingCart))
            {
                ShoppingCart.ShoppingCartShippingAddressID = 0;
            }
            // If shipping address is different from billing address
            /*
            else if (chkShippingAddr.Checked)
            {

                newAddress = false;
                // Process shipping address
                //-------------------------
                if (ai == null)
                {
                    ai = new AddressInfo();
                    newAddress = true;
                }

                if (newAddress)
                {
                    ai.AddressIsShipping = true;
                    ai.AddressEnabled = true;
                    ai.AddressIsBilling = false;
                    ai.AddressIsCompany = false;
                    ai.AddressEnabled = true;
                }
                ai.AddressCustomerID = mCustomerId;
                ai.AddressName = AddressInfoProvider.GetAddressName(ai);

                // Save address and set it's ID to ShoppingCartInfoObj
                AddressInfoProvider.SetAddressInfo(ai);
                ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
            }
            // Shipping address is the same as billing address
            else
            {
                ShoppingCart.ShoppingCartShippingAddressID = ShoppingCart.ShoppingCartBillingAddressID;
            }*/

            try
            {
                // Update changes in database only when on the live site
                if (!ShoppingCartControl.IsInternalOrder)
                {
                    ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);
                }
				p.LogEvent("I", DateTime.Now, "process stetp orderAdress return true" , "");
                return true;
            }
            catch (Exception ex)
            {
                // Show error message
                lblError.Visible = true;
                lblError.Text = ex.Message;
                return false;
            }
        }
        else
        {
            lblError.Visible = true;
            lblError.Text = GetString("Ecommerce.NoCustomerSelected");
            return false;
        }
    }

    protected override void Render(HtmlTextWriter writer)
    {
        if (!ShoppingCartControl.IsCurrentStepPostBack)
        {
            // Load values
            if(IsShippingNeeded) LoadShippingSelectedValue();
            LoadBillingSelectedValue();
            LoadCompanySelectedValue();
        }
        base.Render(writer);
    }

    protected void RptPickBillingAddressItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (System.Data.DataRowView)e.Item.DataItem;
        if (drv != null)
        {
            int addressID = ValidationHelper.GetInteger(drv["AddressID"], 0);
            if (addressID > 0)
            {
                AddressInfo ai = AddressInfoProvider.GetAddressInfo(addressID);
                var ltlBillingAddress = e.Item.FindControl("ltlBillingAddress") as Literal;
                if (ltlBillingAddress != null)
                {
                    ltlBillingAddress.Text = string.Format("{0}, {1}", ai.AddressName, MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));
                }

                // txtNumeroBillingAdresse
                if (ai.GetValue("AddressNumber") != null)
                {
                    var txtNumeroBillingAdresse = e.Item.FindControl("txtNumeroBillingAdresse") as TextBox;
                    if (txtNumeroBillingAdresse != null)
                    {
                        txtNumeroBillingAdresse.Text = ai.GetStringValue("AddressNumber", string.Empty).Trim();
                    }

                }

                // wmNumeroBilling
                var wmNumeroBilling = e.Item.FindControl("wmNumeroBilling") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmNumeroBilling != null)
                {
                    wmNumeroBilling.WatermarkText = GetString("numerorue");
                }


                // txtBillingadresse1
                var txtBillingadresse1 = e.Item.FindControl("txtBillingadresse1") as TextBox;
                if (txtBillingadresse1 != null)
                {
                    txtBillingadresse1.Text = ai.AddressLine1.Trim();
                }

                // wmBillingadresse1
                var wmBillingadresse1 = e.Item.FindControl("wmBillingadresse1") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmBillingadresse1 != null)
                {
                    wmBillingadresse1.WatermarkText = GetString("adresse1");
                }


                // txtBillingadresse2
                var txtBillingadresse2 = e.Item.FindControl("txtBillingadresse2") as TextBox;
                if (txtBillingadresse2 != null)
                {
                    txtBillingadresse2.Text = ai.AddressLine2.Trim();
                }

                // wmBillingadresse2
                var wmBillingadresse2 = e.Item.FindControl("wmBillingadresse2") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmBillingadresse2 != null)
                {
                    wmBillingadresse2.WatermarkText = GetString("adresse2");
                }


                // txtBillingcp
                var txtBillingcp = e.Item.FindControl("txtBillingcp") as TextBox;
                if (txtBillingcp != null)
                {
                    txtBillingcp.Text = ai.AddressZip.Trim();
                }

                // wmBillingcp
                var wmBillingcp = e.Item.FindControl("wmBillingcp") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmBillingcp != null)
                {
                    wmBillingcp.WatermarkText = GetString("cp");
                }

                // txtBillingville
                var txtBillingville = e.Item.FindControl("txtBillingville") as TextBox;
                if (txtBillingville != null)
                {
                    txtBillingville.Text = ai.AddressCity.Trim();
                }

                // wmBillingville
                var wmBillingville = e.Item.FindControl("wmBillingville") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmBillingville != null)
                {
                    wmBillingville.WatermarkText = GetString("ville");
                }

                //txtBillingcountry
                var txtBillingcountry = e.Item.FindControl("txtBillingcountry") as TextBox;
                if (txtBillingcountry != null)
                {
                    txtBillingcountry.Text = MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName);
                    txtBillingcountry.ReadOnly = true;
                }
            }
        }
    }

    protected void RptPickShippingAddressItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (System.Data.DataRowView)e.Item.DataItem;
        if (drv != null)
        {
            int addressID = ValidationHelper.GetInteger(drv["AddressID"], 0);
            if (addressID > 0)
            {
                AddressInfo ai = AddressInfoProvider.GetAddressInfo(addressID);

                var ltlShippingAddress = e.Item.FindControl("ltlShippingAddress") as Literal;
                if (ltlShippingAddress != null)
                {
                    ltlShippingAddress.Text = string.Format("{0}, {1}", ai.AddressName, MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName));
                }

                // txtShippingNumero
                if (ai.GetValue("AddressNumber") != null)
                {
                    var txtShippingNumero = e.Item.FindControl("txtShippingNumero") as TextBox;
                    if (txtShippingNumero != null)
                    {
                        txtShippingNumero.Text = ai.GetStringValue("AddressNumber", string.Empty).Trim();
                    }

                }

                // wmNumero
                var wmNumero = e.Item.FindControl("wmNumero") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmNumero != null)
                {
                    wmNumero.WatermarkText = GetString("numerorue");
                }

                // txtShippingadresse1
                var txtShippingadresse1 = e.Item.FindControl("txtShippingadresse1") as TextBox;
                if (txtShippingadresse1 != null)
                {
                    txtShippingadresse1.Text = ai.AddressLine1.Trim();
                }

                // wmShipadresse1
                var wmShipadresse1 = e.Item.FindControl("wmShipadresse1") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmShipadresse1 != null)
                {
                    wmShipadresse1.WatermarkText = GetString("adresse1");
                }

                // txtShippingadresse2
                var txtShippingadresse2 = e.Item.FindControl("txtShippingadresse2") as TextBox;
                if (txtShippingadresse2 != null)
                {
                    txtShippingadresse2.Text = ai.AddressLine2.Trim();
                }

                // wmShipadresse2
                var wmShipadresse2 = e.Item.FindControl("wmShipadresse2") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmShipadresse2 != null)
                {
                    wmShipadresse2.WatermarkText = GetString("adresse2");
                }

                // txtcp
                var txtShippingcp = e.Item.FindControl("txtShippingcp") as TextBox;
                if (txtShippingcp != null)
                {
                    txtShippingcp.Text = ai.AddressZip.Trim();
                }

                // wmShipcp
                var wmShipcp = e.Item.FindControl("wmShipcp") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmShipcp != null)
                {
                    wmShipcp.WatermarkText = GetString("cp");
                }

                // txtville
                var txtShippingville = e.Item.FindControl("txtShippingville") as TextBox;
                if (txtShippingville != null)
                {
                    txtShippingville.Text = ai.AddressCity.Trim();
                }

                // wmShipville
                var wmShipville = e.Item.FindControl("wmShipville") as AjaxControlToolkit.TextBoxWatermarkExtender;
                if (wmShipville != null)
                {
                    wmShipville.WatermarkText = GetString("ville");
                }

                //txtShippingcountry
                var txtShippingcountry = e.Item.FindControl("txtShippingcountry") as TextBox;
                if (txtShippingcountry != null)
                {
                    txtShippingcountry.Text = MacroContext.CurrentResolver.ResolveMacros(CountryInfoProvider.GetCountryInfo(ai.AddressCountryID).CountryDisplayName);
                    txtShippingcountry.ReadOnly = true;
                }

                // chkShippingAddr
                var chkShippingAddr = e.Item.FindControl("chkShippingAddr") as CheckBox;
                if (chkShippingAddr != null)
                {
                    chkShippingAddr.Checked = ai.AddressIsShipping;
                }

                // chkShippingAddr
                var chk_ShippingAddr = e.Item.FindControl("chk_ShippingAddr") as CheckBox;
                if (chk_ShippingAddr != null)
                {
                    chk_ShippingAddr.Checked = ai.AddressIsShipping;
                }
            }
        }
    }

    protected string GetProductImage(object skuid)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);
        if (sku != null)
        {
            string Disp = string.Empty;
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("select DISTINCT DispositionImage as Disp from View_CONTENT_Product_Joined where NodeSKUID = " + sku.SKUID);
            try
            {
                DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
                Disp = Convert.ToString(ds.Tables[0].Rows[0]["Disp"]);
            }
            catch
            {
                Disp = "1";
            }
            string Divclass = string.Empty;
            string Image = string.Empty;
            if (Disp == "1")
            {
                return "<div class=\"produit_vertical\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
            }
            else
            {
                return "<div class=\"produit_horizonal\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
            }
            //else if (Disp == "2") return "<div class=\"produit_horizonal\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
            cn.Close();
        }
        return String.Empty;
    }


    protected void RptCartItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (System.Data.DataRowView)e.Item.DataItem;
        if (drv != null)
        {
            int currentSKUID = ValidationHelper.GetInteger(drv["SKUID"], 0);
            if (currentSKUID > 0)
            {
                SKUInfo sku = SKUInfoProvider.GetSKUInfo(currentSKUID);
                if (sku != null)
                {
                    int subTotal = 0;
                    double remise = 0;
                    //Display product image
                    var ltlProductImage = e.Item.FindControl("ltlProductImage") as Literal;
                    if (ltlProductImage != null)
                        //<%--# EcommerceFunctions.GetProductImage(Eval("SKUImagePath"), Eval("SKUName"))--%>
                        //ltlProductImage.Text = EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName);
                        ltlProductImage.Text = GetProductImage(sku.SKUID);

                    var ltlProductName = e.Item.FindControl("ltlProductName") as Literal;
                    if (ltlProductName != null)
                        ltlProductName.Text = sku.SKUName;

                    var txtProductCount = e.Item.FindControl("txtProductCount") as TextBox;
                    if (txtProductCount != null)
                    {
                        foreach (ShoppingCartItemInfo shoppingCartItem in ShoppingCart.CartItems)
                        {
                            if (shoppingCartItem.SKUID == sku.SKUID)
                            {
                                remise = shoppingCartItem.UnitTotalDiscount;
                                txtProductCount.Text = shoppingCartItem.CartItemUnits.ToString();
                                subTotal = shoppingCartItem.CartItemUnits;
                                break;
                            }
                        }
                    }

                    var ltlProductPrice = e.Item.FindControl("ltlProductPrice") as Literal;
                    if (ltlProductPrice != null)
                    {
                        //ltlProductPrice.Text = (sku.SKUPrice * subTotal).ToString();
                        ltlProductPrice.Text = EcommerceFunctions.GetFormatedPrice((sku.SKUPrice - remise) * subTotal, sku.SKUDepartmentID, sku.SKUID);

                        //ltlProductPrice.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedValue(sku.SKUPrice * subTotal, ShoppingCart.Currency).ToString());
                        ltlProductPrice.Text = string.Format("{0}<em>{1}</em>", ltlProductPrice.Text.Substring(0, ltlProductPrice.Text.Length - 1).Trim(), ltlProductPrice.Text.Substring(ltlProductPrice.Text.Length - 1, 1).Trim());
                    }
                }
            }
        }
    }

    protected void RptBundleSelectorItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (System.Data.DataRowView)e.Item.DataItem;
        if (drv != null)
        {
            var litProduct = e.Item.FindControl("litProduct") as Literal;
            if (litProduct != null)
            {
                try
                {
                    litProduct.Text = ValidationHelper.GetString(drv["ProductName"], string.Empty);
                }
                catch
                {
                    litProduct.Text = ValidationHelper.GetString(drv["Name"], string.Empty);
                }
            }
        }
    }

    protected void RptBundleProductsItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (BundledItem)e.Item.DataItem;
        if (drv != null)
        {

            var litProduct = e.Item.FindControl("litProduct") as Label;
            if (litProduct != null)
            {
                var obj = dsProd.Tables[0].Rows.Find(drv.ProductID.ToString());
                if (obj != null)
                {
                    try
                    {
                        litProduct.Text = obj["ProductName"].ToString();
                    }
                    catch
                    {
                        litProduct.Text = obj["Name"].ToString();
                    }
                }
            }

        }
    }

    protected void RptPickBillingAddressItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Select"))
        {
            int AddressID = Convert.ToInt32(e.CommandArgument);
            AddressInfo ai = AddressInfoProvider.GetAddressInfo(AddressID);
            ShoppingCart.ShoppingCartBillingAddressID = ai.AddressID;
            ShowAdresses(true, false);
            ReloadShippingOptions();
            ReloadData();
        }
        if (e.CommandName.Equals("Update"))
        {
            int AddressID = Convert.ToInt32(e.CommandArgument);
            AddressInfo ai = AddressInfoProvider.GetAddressInfo(AddressID);
            string s = ai.AddressZip;

            // txtNumeroBillingAdresse
            var txtNumeroBillingAdresse = e.Item.FindControl("txtNumeroBillingAdresse") as TextBox;
            if (!string.IsNullOrEmpty(txtNumeroBillingAdresse.Text))
            {
                ai.SetValue("AddressNumber", txtNumeroBillingAdresse.Text);
            }

            // txtBillingadresse1
            var txtBillingadresse1 = e.Item.FindControl("txtBillingadresse1") as TextBox;
            if (!string.IsNullOrEmpty(txtBillingadresse1.Text))
            {
                ai.AddressLine1 = txtBillingadresse1.Text;
            }

            // txtBillingadresse2
            var txtBillingadresse2 = e.Item.FindControl("txtBillingadresse2") as TextBox;
            if (!string.IsNullOrEmpty(txtBillingadresse2.Text))
            {
                ai.AddressLine2 = txtBillingadresse2.Text;
            }

            // txtBillingcp
            TextBox txtBillingcp = e.Item.FindControl("txtBillingcp") as TextBox;
            if (!string.IsNullOrEmpty(txtBillingcp.Text))
            {
                ai.AddressZip = txtBillingcp.Text;
                // Response.Write("<script>alert('This is Alert " + txtcp.Text + " " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "');</script>");
            }
            // txtBillingville
            var txtBillingville = e.Item.FindControl("txtBillingville") as TextBox;
            if (!string.IsNullOrEmpty(txtBillingville.Text))
            {
                ai.AddressCity = txtBillingville.Text;
            }


            CustomerInfo uc = ECommerceContext.CurrentCustomer;
            string mCustomerName = string.Format("{0} {1}", uc.CustomerFirstName, uc.CustomerLastName);
            // Set the properties
            ai.AddressName = string.Format("{0}, {4} {1} - {2} {3}", mCustomerName, ai.AddressLine1, ai.AddressZip, ai.AddressCity, ai.GetStringValue("AddressNumber", string.Empty));
            /*
            // chkShippingAddr
            var chk_ShippingAddr = e.Item.FindControl("chk_ShippingAddr") as CheckBox;
            if (chk_ShippingAddr != null)
            {
                ai.AddressIsShipping = chk_ShippingAddr.Checked;
            }

            // chkShippingAddr
            var chk_ShippingAddr = e.Item.FindControl("chk_ShippingAddr") as CheckBox;
            if (chk_ShippingAddr != null)
            {
                ai.AddressIsShipping = chk_ShippingAddr.Checked;
            }*/
            AddressInfoProvider.SetAddressInfo(ai);

            ShoppingCart.ShoppingCartBillingAddressID = ai.AddressID;
            ShowAdresses(true, true);
            ReloadData();
        }
    }

    protected void RptPickShippingAddressItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Select"))
        {
            int AddressID = Convert.ToInt32(e.CommandArgument);
            AddressInfo ai = AddressInfoProvider.GetAddressInfo(AddressID);
            ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
            ShowAdresses(false, true);
            ReloadShippingOptions();
            ReloadData();
        }
        if (e.CommandName.Equals("Update"))
        {
            int AddressID = Convert.ToInt32(e.CommandArgument);
            AddressInfo ai = AddressInfoProvider.GetAddressInfo(AddressID);
            string s = ai.AddressZip;

            // txtShippingNumero
            var txtShippingNumero = e.Item.FindControl("txtShippingNumero") as TextBox;
            if (txtShippingNumero != null)
            {
                ai.SetValue("AddressNumber", txtShippingNumero.Text);
            }

            // txtShippingadresse1
            var txtShippingadresse1 = e.Item.FindControl("txtShippingadresse1") as TextBox;
            if (txtShippingadresse1 != null)
            {
                ai.AddressLine1 = txtShippingadresse1.Text;
            }

            // txtShippingadresse2
            var txtShippingadresse2 = e.Item.FindControl("txtShippingadresse2") as TextBox;
            if (txtShippingadresse2 != null)
            {
                ai.AddressLine2 = txtShippingadresse2.Text;
            }

            // txtShippingcp
            TextBox txtShippingcp = e.Item.FindControl("txtShippingcp") as TextBox;
            if (txtShippingcp != null)
            {
                ai.AddressZip = txtShippingcp.Text;
                // Response.Write("<script>alert('This is Alert " + txtcp.Text + " " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "');</script>");
            }
            // txtShippingville
            var txtShippingville = e.Item.FindControl("txtShippingville") as TextBox;
            if (txtShippingville != null)
            {
                ai.AddressCity = txtShippingville.Text;
            }


            CustomerInfo uc = ECommerceContext.CurrentCustomer;
            string mCustomerName = string.Format("{0} {1}", uc.CustomerFirstName, uc.CustomerLastName);
            // Set the properties
            ai.AddressName = string.Format("{0}, {4} {1} - {2} {3}", mCustomerName, ai.AddressLine1, ai.AddressZip, ai.AddressCity, ai.GetStringValue("AddressNumber", string.Empty));
            AddressInfoProvider.SetAddressInfo(ai);
            ShoppingCart.ShoppingCartShippingAddressID = ai.AddressID;
            ShowAdresses(true, true);
            ReloadData();
        }

    }

    protected void RptBundleSelectorItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Select"))
        {
            int productID = Convert.ToInt32(e.CommandArgument);
            bundledItems.Add(new BundledItem { BundleID = 1, ProductID = productID });
            SaveBundleData();
            ReloadBundledItems();
        }
    }

    protected void RptBundleProductsItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Delete"))
        {
            int productID = Convert.ToInt32(e.CommandArgument);
            bundledItems.Remove(bundledItems.Where(i => i.ProductID == productID).First());
            SaveBundleData();
            ReloadBundledItems();
        }
    }

    private void SetSessionParams(AddressInfo ai)
    {
        int CountryID = ai.AddressCountryID;

        // SessionHelper.SetValue("CountryID", CountryID);
        // SessionHelper.SetValue("PriceID", -1);

        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID", -1);
        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCountryID", CountryID);

        int ShippingCartUnits = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
      //  EventLogProvider elp2 = new EventLogProvider();
        double vat = ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart);
      //  elp2.LogEvent("I", DateTime.Now, "valeur vat in orderadress = " + vat, "code");
        if (vat > 0)
        {
            vat = 1.06;
           // elp2.LogEvent("I", DateTime.Now, "vatin order address : " + vat, "code");
        }
        else
        {
            vat = 1;
        }
        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", ShippingCartUnits);
        parameters.Add("@CountryID", CountryID);
        parameters.Add("@VATRate", vat);
       // parameters.Add("@VATRate", 1 + ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart) / 100);
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters);
        cn.Close();
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            DataRow drow = ds.Tables[0].Rows[0];
            int PriceID = (int)drow["ItemID"];
            // SessionHelper.SetValue("PriceID", PriceID);
            ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID", PriceID);
        }

        ShippingExtendedInfoProvider.CalculateShipping(ShoppingCart);
        ShippingExtendedInfoProvider.CalculateShippingTax(ShoppingCart);
        ShippingExtendedInfoProvider.CalculateTotalShipping(ShoppingCart);
    }

    protected void RptCartItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Remove"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            // Remove product and its product option from list
            this.ShoppingCart.RemoveShoppingCartItem(cartItemGuid);

            if (!this.ShoppingCartControl.IsInternalOrder)
            {
                // Delete product from database
                ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(cartItemGuid);
            }
            ShippingUnit = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
            btnUpdate_Click1(null, null);
            ReloadData();
        }
        if (e.CommandName.Equals("Decrease"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            ShoppingCartItemInfo cartItem = ShoppingCart.GetShoppingCartItem(cartItemGuid);
            if (cartItem != null)
            {
                if (cartItem.CartItemUnits - 1 > 0)
                {
                    cartItem.CartItemUnits--;
                    // Update units of child bundle items
                    foreach (ShoppingCartItemInfo bundleItem in cartItem.BundleItems)
                    {
                        bundleItem.CartItemUnits--;
                    }

                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        try
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);
                        }
                        catch
                        {
                         //   EventLogProvider ev = new EventLogProvider();
                         //   ev.LogEvent("I", DateTime.Now, "erreur cartitem Decrease OrderAre", "code");
                        }

                        // Update product options in database
                        foreach (ShoppingCartItemInfo option in cartItem.ProductOptions)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(option);
                        }
                    }
                    ShippingUnit = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
                    btnUpdate_Click1(null, null);
                    ReloadData();
                }
            }
        }
        if (e.CommandName.Equals("Increase"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            ShoppingCartItemInfo cartItem = ShoppingCart.GetShoppingCartItem(cartItemGuid);
            if (cartItem != null)
            {
                if (cartItem.CartItemUnits + 1 > 0)
                {
                    cartItem.CartItemUnits++;
                    // Update units of child bundle items
                    foreach (ShoppingCartItemInfo bundleItem in cartItem.BundleItems)
                    {
                        bundleItem.CartItemUnits++;
                    }

                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        try
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);
                        }
                        catch
                        {
                          //  EventLogProvider ev = new EventLogProvider();
                         //   ev.LogEvent("I", DateTime.Now, "erreur cartitem Increase orderAdress", "code");
                        }
                        // Update product options in database
                        foreach (ShoppingCartItemInfo option in cartItem.ProductOptions)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(option);
                        }
                    }
                    ShippingUnit = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
                    btnUpdate_Click1(null, null);
                    ReloadData();
                }
            }
        }
    }

    private void ReloadBillingAdresses()
    {
        //RptPickBillingAddress
        string where = string.Format("AddressCustomerID={0} AND AddressIsBilling=1 AND AddressEnabled = 1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
        string orderby = "AddressID";
        DataSet ds = AddressInfoProvider.GetAddresses(where, orderby);
        RptPickBillingAddress.DataSource = ds;
        //Response.Write("<script>alert('This is databinding " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + "');</script>");
        RptPickBillingAddress.DataBind();
    }

    private void ReloadShippingAdresses()
    {
		if(IsShippingNeeded){
			//RptPickShippingAddress
			string where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1 AND AddressEnabled = 1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
			string orderby = "AddressID";
			DataSet ds = AddressInfoProvider.GetAddresses(where, orderby);
			RptPickShippingAddress.DataSource = ds;
			RptPickShippingAddress.DataBind();
		}
    }

    private void ReloadBundledItems()
    {
        string bundledString = ShoppingCart.GetStringValue("ShoppingCartBundleData", string.Empty);
        if (!string.IsNullOrEmpty(bundledString))
        {
            bundledItems.Clear();
            while (!string.IsNullOrEmpty(bundledString))
            {
                int productId = 0, bundleID = 0, mark = bundledString.IndexOf("-");

                string bundlestring = bundledString.Substring(0, mark);
                string productIdString = bundlestring.Substring(0, bundlestring.IndexOf(","));
                string bundleIdString = bundlestring.Substring(bundlestring.IndexOf(",") + 1);
                bundleID = int.Parse(bundleIdString);
                productId = int.Parse(productIdString);
                bundledItems.Add(new BundledItem { BundleID = bundleID, ProductID = productId });
                bundledString = bundledString.Substring(mark + 1);
            }
            initDSProd();
            rptBundledProducts.DataSource = bundledItems;
            rptBundledProducts.DataBind();

            lblBundleHeader.Text = string.Format(GetString("BundleHeader"), bundledItems.Count()> 1 ? "s": string.Empty);
            pnlCartBundleContent.Visible = true;
        }
        else
        {
            pnlCartBundleContent.Visible = false;
        }
        EvaluateBundle();
    }

    /// <summary>
    /// Reloads the form data.
    /// </summary>
    protected void ReloadData()
    {
        rptCart.DataSource = ShoppingCart.ContentTable;
        rptCart.DataBind();

        ReloadBillingAdresses();
        ReloadShippingAdresses();
        ReloadBundledItems();

        ListItem listitem = new ListItem(GetString("choixpays"), "0");
        ddlBillingCountry.DataSource = GetCountryList();
        ddlBillingCountry.DataTextField = "CountryDisplayName";
        ddlBillingCountry.DataValueField = "ShippingCountryID";
        ddlBillingCountry.DataBind();
        ddlBillingCountry.Items.Insert(0, listitem);

        ddlShippingCountry.DataSource = GetCountryList();
        ddlShippingCountry.DataTextField = "CountryDisplayName";
        ddlShippingCountry.DataValueField = "ShippingCountryID";
        ddlShippingCountry.DataBind();
        ddlShippingCountry.Items.Insert(0, listitem);
        DisplayTotalPrice();
    }

    private void SaveBundleData()
    {
        string serialized = string.Empty;
        foreach (BundledItem bundledItem in bundledItems)
        {
            serialized = string.Format("{0}{1},{2}-", serialized, bundledItem.ProductID.ToString(), bundledItem.BundleID.ToString());
        }
        ShoppingCart.SetValue("ShoppingCartBundleData", serialized);
    }

    private DataSet GetCountryList()
    {
        DataSet ds;
        GeneralConnection cn = ConnectionHelper.GetConnection();
        ds = cn.ExecuteQuery(@"select  ShippingCountryId, CountryDisplayName, CountryName, CountryTwoLetterCode, CountryThreeLetterCode from customtable_shippingextensioncountry Join CMS_Country on customtable_shippingextensioncountry.ShippingCountryId= CMS_Country.CountryID
                                GROUP BY ShippingCountryId, CountryDisplayName, CountryName, CountryTwoLetterCode, CountryThreeLetterCode
                                ORDER BY dbo.CMS_Country.CountryDisplayName", null, QueryTypeEnum.SQLQuery, false);
        cn.Close();
        return LocalizedCountry.LocalizeCountry(ds);
    }

    // Displays PaymentList
    private void ShowPaymentList()
    {
        string where = "PaymentOptionEnabled=1";
        string orderby = "PaymentOptionName";
        //DataSet ds = PaymentOptionInfoProvider.GetPaymentOptions(CurrentSite.SiteID, true);
        DataSet ds = PaymentOptionInfoProvider.GetPaymentOptions(where, orderby);

        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            ddlPaymentOption.DataSource = ds;
            ddlPaymentOption.DataTextField = "PaymentOptionDisplayName";
            ddlPaymentOption.DataValueField = "PaymentOptionId";
            ddlPaymentOption.DataBind();
            try
            {
                int PaymentID = ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPaymentID");
              //  EventLogProvider ev = new EventLogProvider();
              //  ev.LogEvent("E", DateTime.Now, "nb2", PaymentID.ToString());
                if (PaymentID > 0)
                {
                    ddlPaymentOption.SelectedValue = PaymentID.ToString();
                }
            }
            catch
            {
            }
            ddlPaymentOption_SelectedIndexChanged(null, null);
        }
    }

    // Displays total price
    protected void DisplayTotalPrice()
    {


        double prix = ShoppingCart.TotalItemsPriceInMainCurrency;
        lblMontantAchatSanscoupon.Text = string.Format("{0}", CurrencyInfoProvider.GetFormattedPrice(prix));
        lblMontantAchatSanscoupon.Text = string.Format("{0} <em>{1}</em>", lblMontantAchatSanscoupon.Text.Substring(0, lblMontantAchatSanscoupon.Text.Length - 1), lblMontantAchatSanscoupon.Text.Substring(lblMontantAchatSanscoupon.Text.Length - 1));
      //   // end total sans coupon

      //  // total coupon 
      ////  double prixcoupon = prix - ShoppingCart.TotalItemsPriceInMainCurrency;
      //  double prixcoupon = 20;
      //  lblMontantCoupon.Text = prixcoupon.ToString();
       lblMontantCoupon.Text = string.Format("{0}", CurrencyInfoProvider.GetFormattedPrice(ShoppingCart.OrderDiscount, ShoppingCart.CurrencyInfoObj));
       lblMontantCoupon.Text = string.Format("{0} <em>{1}</em>", lblMontantCoupon.Text.Substring(0, lblMontantCoupon.Text.Length - 1), lblMontantCoupon.Text.Substring(lblMontantCoupon.Text.Length - 1));


       double bulkPrice = ShoppingCart.RoundedTotalPrice - ShoppingCart.TotalShipping;
      //  //SetSessionParams(AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID));
       lblTotalPriceValue.Text = string.Format("{0} <em>�</em>", IsShippingNeeded ? CurrencyInfoProvider.GetFormattedValue(ShoppingCart.RoundedTotalPrice, ShoppingCart.Currency).ToString() : CurrencyInfoProvider.GetFormattedValue(bulkPrice, ShoppingCart.Currency).ToString());
       if(IsShippingNeeded) lblShippingPriceValue.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedValue(ShoppingCart.TotalShipping, ShoppingCart.Currency).ToString());

        lblMontantAchat.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedValue(bulkPrice, ShoppingCart.Currency).ToString());
        if (IsShippingNeeded)
        {
            lblError.Visible = false;
            if (ShoppingCart.TotalShipping > 0)
            {
                btn_valid_order.Visible = true;
            }
            else
            {
                btn_valid_order.Visible = false;
                lblError.Text = "L'option d'envoi est obligatoire";
                lblError.Visible = true;
            }
        }
        else
        {
            btn_valid_order.Visible = true;
        }
        EvaluateBundle();
    }

    private void EvaluateBundle()
    {
        int BundleID = 0;
        availableBundles.Clear();
        foreach (ShoppingCartItemInfo item in ShoppingCart.CartItems)
        {
            BundleID = GetBundle(item.SKUID);// *item.CartItemUnits;
            var obj = availableBundles.Find(i => i.BundleId == BundleID);
            if (obj != null)
            {
                obj.Total += item.CartItemUnits;
            }
            else
            {
                // ShoppingCart.SetValue("ShoppingCartBundleData", string.Empty);
            }
        }

        var validBundles = availableBundles.Where(i => i.Total >= i.Quantity);
        if (validBundles.Count() > 0)
        {
            Bundle validBundle = validBundles.First();
            int availableBundle = validBundle.Total / validBundle.Quantity;
            //lblBundle.Text = string.Format(GetString("FreeBundleAlert"), availableBundle.ToString(), validBundle.Description);
            lblBundle.Text = GetString("FreeBundleAlert");

            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("SELECT * FROM CONTENT_Product WHERE BundleID={0}", validBundle.BundleId.ToString());
            DataSet dsProduct = cn.ExecuteQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);
            cn.Close();
            if (!DataHelper.DataSourceIsEmpty(dsProduct))
            {
                rptBundleSelector.DataSource = dsProduct;
                rptBundleSelector.DataBind();
            }

            int bundledCount = bundledItems.Where(i => i.BundleID == validBundle.BundleId).Count();
            lnkBundle.Visible = (bundledCount < availableBundle);
            if (bundledCount > availableBundle)
            {
                lblErrorBundle.Text = GetString("ErrorBundleTooMuch");
                lblErrorBundle.Visible = true;
            }
            else
            {
                lblErrorBundle.Visible = false;
            }
            btn_valid_order.Visible = !lblErrorBundle.Visible;
        }
        else
        {
            lnkBundle.Visible = false;
        }
    }

    private int GetBundle(int SKUID)
    {
        int result = 0;
        GeneralConnection cn = ConnectionHelper.GetConnection();
        string stringQuery = string.Format(@"SELECT     dbo.CONTENT_Product.BundleID, dbo.customtable_customBundle.Quantity, dbo.customtable_customBundle.Enabled, 
                      dbo.customtable_customBundle.Description FROM dbo.CONTENT_Product INNER JOIN
                      dbo.customtable_customBundle ON dbo.CONTENT_Product.BundleID = dbo.customtable_customBundle.ItemID WHERE ProductID=
                                            (SELECT Top 1 DocumentForeignKeyValue FROM view_cms_tree_joined WHERE ClassName='CMS.Product' AND SKUID={0})", SKUID.ToString());
        /*string stringQuery = string.Format(@"SELECT BundleID FROM CONTENT_Product WHERE ProductID=
                                            (SELECT Top 1 DocumentForeignKeyValue FROM view_cms_tree_joined WHERE ClassName='CMS.Product' AND SKUID={0})", SKUID.ToString());*/
        DataSet ds = cn.ExecuteQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);
        cn.Close();
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            DataRow dr = ds.Tables[0].Rows[0];
            var obj = availableBundles.Find(i => i.BundleId == ValidationHelper.GetInteger(dr["BundleID"], 0));
            if (obj == null)
            {
                availableBundles.Add(new Bundle { BundleId = ValidationHelper.GetInteger(dr["BundleID"], 0), Quantity = ValidationHelper.GetInteger(dr["Quantity"], 0), Description = ValidationHelper.GetString(dr["Description"], string.Empty) });
            }
            result = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["BundleID"], 0);
        }

        return result;
    }

    protected void ddlShippingOption_SelectedIndexChanged(object sender, EventArgs e)
    {
        int PriceID = ValidationHelper.GetInteger(ddlShippingOption.SelectedValue, -1);
        int CountryID = (AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID)).AddressCountryID;

        // SessionHelper.SetValue("PriceID", PriceID);
        // SessionHelper.SetValue("CountryID", CountryID);

        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID", PriceID);
        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartCountryID", CountryID);

        btnUpdate_Click1(null, null);
        DisplayTotalPrice();
    }

    protected void ddlPaymentOption_SelectedIndexChanged(object sender, EventArgs e)
    {
        int PaymentID = ValidationHelper.GetInteger(ddlPaymentOption.SelectedValue, -1);
        // SessionHelper.SetValue("PaymentID", PaymentID);
        ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPaymentID", PaymentID);
        //pnlPaymentOption.Visible = CheckPaymentIsGateway(PaymentID);
        lblPayment.Text = CMSContext.ResolveMacros(ddlPaymentOption.SelectedItem.Text);
      
    }

    protected void rdoBtn_CheckedChanged(object sender, EventArgs e)
    {
        if (rdbVisa.Checked) this.ShoppingCart.PaymentGatewayCustomData["BRAND"] = "VISA";
        if (rdbMastercard.Checked) this.ShoppingCart.PaymentGatewayCustomData["BRAND"] = "MasterCard";
        if (rdbMaestro.Checked) this.ShoppingCart.PaymentGatewayCustomData["BRAND"] = "Maestro";
        lblCardName.Text = this.ShoppingCart.PaymentGatewayCustomData["BRAND"].ToString();
    }

    protected void btn_Valid_Order_Click(object sender, EventArgs e)
    {
		p.LogEvent("I", DateTime.Now, "Valider order  :" , "");
        if(IsShippingNeeded)
        {
            if(ShoppingCart.ShoppingCartShippingAddressID > 0)
            {
                p.LogEvent("I", DateTime.Now, "Valider true ", "");
                ddlShippingOption_SelectedIndexChanged(null, null);
                // EventLogProvider msg = new EventLogProvider();
                Session["newAddress"] = null;
                //  msg.LogEvent("I", DateTime.Now, "session effacer", "code");
                ButtonNextClickAction();
                //  msg.LogEvent("I", DateTime.Now, " Avant ID : " +ShoppingCart.GetValue("ShoppingCartPriceID"), "code");
                ButtonNextClickAction();
                // msg.LogEvent("I", DateTime.Now, " Apr�s ID : " + ShoppingCart.GetValue("ShoppingCartPriceID"), "code");
            }
            else
            {
                p.LogEvent("I", DateTime.Now, "Valider false ", "");
                lblError.Visible = true;
                lblError.Text = "Error ShoppingCartShippingAddressID";
            } 
        }
        else
        {
            ShippingExtendedInfoProvider.SetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID", -1);
            ButtonNextClickAction();
            ButtonNextClickAction();
        }
    }

    private bool CheckPaymentIsGateway(string paymentID)
    {
        bool result = false;
        int paymentid = int.Parse(paymentID);
        PaymentOptionInfo payment = PaymentOptionInfoProvider.GetPaymentOptionInfo(paymentid);
        result = string.Equals(payment.PaymentOptionDescription, "PAYMENTGATEWAY");
        return result;
    }

    protected void buttonNewBillingAddress_Click(object sender, EventArgs e)
    {
        String siteName = SiteContext.CurrentSiteName;
        #region "Banned IPs"

        // Ban IP addresses which are blocked for registration
        if (!BannedIPInfoProvider.IsAllowed(siteName, BanControlEnum.Registration))
        {
            lblError.Visible = true;
            lblError.Text = GetString("banip.ipisbannedregistration");
            return;
        }
        #endregion


        //Update Customer
        CustomerInfo updateCustomer = ECommerceContext.CurrentCustomer;
        updateCustomer.CustomerEnabled = true;
        updateCustomer.CustomerLastModified = DateTime.Now;
        updateCustomer.CustomerSiteID = CMSContext.CurrentSiteID;
        updateCustomer.CustomerCompany = "";
        updateCustomer.CustomerOrganizationID = "";
        updateCustomer.CustomerTaxRegistrationID = "";
        CustomerInfoProvider.SetCustomerInfo(updateCustomer);

        #region "Insert new adress / Update selected adress"

        #region "Adresse"

        if (txtBillingadresse1.Text == "")
        {
            lblErrorBillingAdress.Visible = true;
            lblErrorBillingAdress.Text = "Veuillez saisir l'Adresse";
            return;
        }

        #endregion

        #region "CP"

        if (txtBillingcp.Text == "")
        {
            lblErrorBillingAdress.Visible = true;
            lblErrorBillingAdress.Text = "Veuillez saisir le Code Postal";
            return;
        }

        #endregion

        #region "Ville"

        if (txtBillingville.Text == "")
        {
            lblErrorBillingAdress.Visible = true;
            lblErrorBillingAdress.Text = "Veuillez saisir la Ville";
            return;
        }

        #endregion

        #region "Adresse"

        if ((chkBillingBillingAddr.Checked == false) && (chkBillingShippingAddr.Checked == false))
        {
            lblErrorBillingAdress.Visible = true;
            lblErrorBillingAdress.Text = "Veuillez mentionner le type d'Adresse";
            return;
        }

        #endregion

        #region "Pays"
        if (ddlBillingCountry.SelectedValue == "0")
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez s�l�ctionner le pays";
            return;
        }
        #endregion



        #region "New adress"

        // Create new address object
        AddressInfo newAddress = new AddressInfo();

        int CountryID = ValidationHelper.GetInteger(ddlBillingCountry.SelectedValue, 0);
        CustomerInfo uc = ECommerceContext.CurrentCustomer;
        mCustomerId = uc.CustomerID;
        string mCustomerName = string.Format("{0} {1}", uc.CustomerFirstName, uc.CustomerLastName);
        // Set the properties
        newAddress.AddressName = string.Format("{0}, {4} {1} - {2} {3}", mCustomerName, txtBillingadresse1.Text, txtBillingcp.Text, txtBillingville.Text, txtBillingnumero.Text);
        newAddress.AddressLine1 = txtBillingadresse1.Text;
        newAddress.AddressLine2 = txtBillingadresse2.Text;
        newAddress.AddressCity = txtBillingville.Text;
        newAddress.AddressZip = txtBillingcp.Text;
        newAddress.AddressIsBilling = chkBillingBillingAddr.Checked;
        newAddress.AddressIsShipping = chkBillingShippingAddr.Checked;
        newAddress.AddressEnabled = true;
        newAddress.AddressPersonalName = mCustomerName;
        newAddress.AddressCustomerID = mCustomerId;
        newAddress.AddressCountryID = CountryID;
        newAddress.SetValue("AddressNumber", txtBillingnumero.Text);
        // Create the address
        AddressInfoProvider.SetAddressInfo(newAddress);

        ShoppingCart.ShoppingCartBillingAddressID = newAddress.AddressID;
        if (chkBillingShippingAddr.Checked)
        {
            ShoppingCart.ShoppingCartShippingAddressID = newAddress.AddressID;
        }
        ShoppingCart.SetValue("ShoppingCartCarriedOnPriceID", string.Empty);
        ShowAdresses(true, true);
        ReloadData();


        #endregion


        #endregion
    }

    protected void buttonNewShippingAddress_Click(object sender, EventArgs e)
    {
        String siteName = SiteContext.CurrentSiteName;
        #region "Banned IPs"

        // Ban IP addresses which are blocked for registration
        if (!BannedIPInfoProvider.IsAllowed(siteName, BanControlEnum.Registration))
        {
            lblError.Visible = true;
            lblError.Text = GetString("banip.ipisbannedregistration");
            return;
        }
        #endregion


        //Update Customer
        CustomerInfo updateCustomer = ECommerceContext.CurrentCustomer;
        updateCustomer.CustomerEnabled = true;
        updateCustomer.CustomerLastModified = DateTime.Now;
        updateCustomer.CustomerSiteID = CMSContext.CurrentSiteID;
        updateCustomer.CustomerCompany = "";
        updateCustomer.CustomerOrganizationID = "";
        updateCustomer.CustomerTaxRegistrationID = "";
        CustomerInfoProvider.SetCustomerInfo(updateCustomer);

        #region "Insert new adress / Update selected adress"

        #region "Adresse"

        if (txtShippingadresse1.Text == "")
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez saisir l'Adresse";
            return;
        }

        #endregion

        #region "CP"

        if (txtShippingcp.Text == "")
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez saisir le Code Postal";
            return;
        }

        #endregion

        #region "Ville"

        if (txtShippingville.Text == "")
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez saisir la Ville";
            return;
        }

        #endregion

        #region "Adresse"

        if ((chkShippingBillingAddr.Checked == false) && (chkShippingShippingAddr.Checked == false))
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez mentionner le type d'Adresse";
            return;
        }

        #endregion

        #region "Pays"
        if (ddlShippingCountry.SelectedValue == "0")
        {
            lblErrorShippingAdress.Visible = true;
            lblErrorShippingAdress.Text = "Veuillez s�l�ctionner le pays";
            return;
        }
        #endregion

        #region "New adress"

        // Create new address object
        AddressInfo newAddress = new AddressInfo();

        int CountryID = ValidationHelper.GetInteger(ddlShippingCountry.SelectedValue, 0);
        CustomerInfo uc = ECommerceContext.CurrentCustomer;
        mCustomerId = uc.CustomerID;
        string mCustomerName = string.Format("{0} {1}", uc.CustomerFirstName, uc.CustomerLastName);
        // Set the properties
        newAddress.AddressName = string.Format("{0}, {4} {1} - {2} {3}", mCustomerName, txtShippingadresse1.Text, txtShippingcp.Text, txtShippingville.Text, txtShippingnumero.Text);
        newAddress.AddressLine1 = txtShippingadresse1.Text;
        newAddress.AddressLine2 = string.Empty;
        newAddress.AddressCity = txtShippingville.Text;
        newAddress.AddressZip = txtShippingcp.Text;
        newAddress.AddressIsBilling = chkShippingBillingAddr.Checked;
        newAddress.AddressIsShipping = chkShippingShippingAddr.Checked;
        newAddress.AddressEnabled = true;
        newAddress.AddressPersonalName = mCustomerName;
        newAddress.AddressCustomerID = mCustomerId;
        newAddress.AddressCountryID = CountryID;
        newAddress.SetValue("AddressNumber", txtShippingnumero.Text);

        // Create the address
        AddressInfoProvider.SetAddressInfo(newAddress);

        ShoppingCart.ShoppingCartShippingAddressID = newAddress.AddressID;
        if (chkShippingBillingAddr.Checked)
        {
            ShoppingCart.ShoppingCartBillingAddressID = newAddress.AddressID;
        }
        
        ShowAdresses(true, true);
        ReloadData();
        #endregion


        #endregion
    }

    protected void btnUpdate_Click1(object sender, EventArgs e)
    {
        if (ShoppingCart != null)
        {
            ShoppingCart.ShoppingCartCurrencyID = ValidationHelper.GetInteger(selectCurrency.CurrencyID, 0);
            //CheckDiscountCoupon();

            //if ((ShoppingCart.ShoppingCartDiscountCouponID > 0) && (!ShoppingCart.IsDiscountCouponApplied))
            //{
            //    // Discount coupon code is valid but not applied to any product of the shopping cart
            //    lblError.Text = GetString("shoppingcart.discountcouponnotapplied");
            //}

            // Inventory shloud be checked
            ReloadData();
        }
    }

    private void initDSProd()
    {
        GeneralConnection cn = ConnectionHelper.GetConnection();
        string stringQuery = "SELECT * FROM CONTENT_Product";
        dsProd = cn.ExecuteQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);
        if (!DataHelper.DataSourceIsEmpty(dsProd))
        {
            DataColumn[] key = new DataColumn[1];
            DataTable dt = dsProd.Tables[0];
            key[0] = dt.Columns[0];
            dt.PrimaryKey = key;
            
            
        }
        cn.Close();
    }
}