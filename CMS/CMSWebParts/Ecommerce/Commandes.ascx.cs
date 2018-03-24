﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Ecommerce;
using CMS.EcommerceProvider;
using CMS.SiteProvider;
using CMS.GlobalHelper;
using CMS.CMSHelper;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using CMS.PortalControls;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Helpers;

public partial class CMSWebParts_Ecommerce_Commandes : CMSAbstractWebPart
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (MembershipContext.AuthenticatedUser.IsAuthenticated())
        {

            BindCommande();

        }
    }

    private void BindCommande()
    {
        var customer = CustomerInfoProvider.GetCustomerInfoByUserID(MembershipContext.AuthenticatedUser.UserID);
        if (customer == null) return;

        var customerID = customer.CustomerID;
        var filtre = String.Format("OrderCustomerID = {0}", customerID);

        try
        {
            var ds = OrderInfoProvider.GetOrderList(filtre, "OrderDate DESC");
            rptCommande.DataSource = ds.Tables[0];
            rptCommande.DataBind();
        }
        catch (Exception ex)
        {
            lbInfo.Visible = true;
            lbInfo.Text = ex.Message;
        }
    }


    protected void RptCommandeItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            var itemRepeater = e.Item.FindControl("rptItem") as Repeater;
            if (itemRepeater != null)
            {
                var commande = (DataRowView)e.Item.DataItem;
                if (commande != null)
                {
                    var commandeId = (int)commande["OrderID"];
                    var filtre = String.Format("OrderItemOrderID = {0}", commandeId);
                    var dsItem = OrderItemInfoProvider.GetOrderItems(filtre, "", 100, "");
                    itemRepeater.DataSource = dsItem.Tables[0];
                    itemRepeater.DataBind();
                    // OrderItemInfo oii = new OrderItemInfo();

                    var orderInfo = OrderInfoProvider.GetOrderInfo(commandeId);
                    var status = orderInfo.GetValue("OrderStatus") != null ? (string)orderInfo.GetValue("OrderStatus") :
                       String.Empty;
                    if (status == "0")
                    {
                        var button = e.Item.FindControl("lnkPayer") as LinkButton;
                        if (button != null)
                            button.Visible = true;
                    }

                }

            }
        }
    }

    protected string GetProductName(object skuname)
    {
        string skunom = skuname.ToString();
        if (skunom != null)
            return skunom.ToUpper();
        return String.Empty;
    }

    protected double GetProductPrice(object skuprice)
    {
        double skuprix = Convert.ToDouble(skuprice);
        if (skuprix != null)
            return System.Math.Round(skuprix, 2);
        return 0;
    }

    protected double GetProductPricettc(object oid, object id)
    {
        int orderId = Convert.ToInt32(oid);
        double result = 0;
        ShoppingCartInfo cart = ShoppingCartInfoProvider.GetShoppingCartInfoFromOrder(orderId);
        if (cart != null)
        {
            string where = "SKUID = " + id;

            ShoppingCartItemInfo item = null;

            DataSet items = ShoppingCartItemInfoProvider.GetShoppingCartItems(where, null);
            if (!DataHelper.DataSourceIsEmpty(items))
            {
                item = new ShoppingCartItemInfo(items.Tables[0].Rows[0]);
                result = item.UnitTotalPrice;
            }
        }

        return System.Math.Round(result, 2);
    }

    protected double GetPriceWithDiscount(object skuid, object price)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);

        if (sku != null)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("select VolumeDiscountValue,VolumeDiscountIsFlatValue from View_CONTENT_Product_Joined left join COM_VolumeDiscount on View_CONTENT_Product_Joined.SKUID=COM_VolumeDiscount.VolumeDiscountSKUID where NodeSKUID = " + sku.SKUID);
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            string Promo = Convert.ToString(ds.Tables[0].Rows[0]["VolumeDiscountValue"]);
            string Type = Convert.ToString(ds.Tables[0].Rows[0]["VolumeDiscountIsFlatValue"]);
            double prix = double.Parse(price.ToString());
            double montant = 0;
            if (!string.IsNullOrEmpty(Promo))
            {
                double promotion = double.Parse(Promo);
                if (Type == "False")
                {
                    montant = prix - ((prix * promotion) / 100);
                    return montant;
                }
                else
                {
                    montant = prix - promotion;
                    return montant;
                }
            }
            else
                return prix;
        }
        return 0;
    }

    protected double GetFormatedPrice(object skuid, object price)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);
        string res = string.Empty;
        double result = 0;
        if (sku != null)
        {
            res = EcommerceFunctions.GetFormatedPrice(GetPriceWithDiscount(skuid, price), sku.SKUDepartmentID, sku.SKUID);
            res = res.Substring(0, res.Length - 1);     
            result = double.Parse(res);
        }
        return System.Math.Round(result, 2);
    }

    protected double GetSousTotal(object qte, object prix)
    {
        if (qte != null && prix != null)
            return System.Math.Round((int)qte * (double)prix, 2);
        return 0;        
    }

    protected string GetProductImage(object skuid)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);
        if (sku != null)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("select DISTINCT DispositionImage as Disp from View_CONTENT_Product_Joined where NodeSKUID = " + sku.SKUID);
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            string Disp = Convert.ToString(ds.Tables[0].Rows[0]["Disp"]);
            string Divclass = string.Empty;
            string Image = string.Empty;
            if (Disp == "1") return "<div class=\"produit_vertical\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
            else if (Disp == "2") return "<div class=\"produit_horizonal\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
            else return "<div class=\"produit_horizonal\">" + EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName) + "</div>";
        }
        return String.Empty;
    }

    protected string GetTitre(object idcmd, object datecomplete)
    {
        datecomplete = Convert.ToDateTime(datecomplete);
        System.Globalization.CultureInfo currentUI = System.Globalization.CultureInfo.CurrentUICulture;

        if ((datecomplete != null) && (idcmd != null))
        {
            if (Convert.ToString(currentUI) == "fr-FR")
                return "Commande num�ro " + idcmd + " du " + String.Format("{0:d MMMM yyyy} � {1:HH:mm:ss}", datecomplete, datecomplete);
            else
                return "Order number " + idcmd + " of " + String.Format("{0:MMMM d, yyyy} at {1:HH:mm:ss}", datecomplete, datecomplete);
        }
        return String.Empty;
    }

    protected string GetFormatDate(object datecomplete)
    {
        datecomplete = Convert.ToDateTime(datecomplete);
        if (datecomplete != null)
            return String.Format("{0:d MMMM yyyy} � {1:HH:mm:ss}", datecomplete, datecomplete);
        return String.Empty;
    }


    public string CustomTrimText(object txtValue, int leftChars)
    {
        // Checks that text is not null.
        if (txtValue == null | txtValue == DBNull.Value)
        {
            return "";
        }
        else
        {
            string txt = (string)txtValue;

            // Returns a substring if the text is longer than specified.
            if (txt.Length <= leftChars)
            {
                return txt;
            }
            else
            {
                return txt.Substring(0, leftChars) + " ...";
            }
        }
    }

    protected string GetProductReference(object skuid)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);

        if (sku != null)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("select Distinct Reference as Ref from View_CONTENT_Product_Joined where NodeSKUID = " + sku.SKUID);
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            if (ds.Tables[0].Rows.Count != 0)
            {
                string Ref = Convert.ToString(ds.Tables[0].Rows[0]["Ref"]);
                return CustomTrimText(Ref, 5);
            }
            else return String.Empty;

        }
        return String.Empty;
    }

    protected string GetProductNodeAliasPath(object skuid)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);

        if (sku != null)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("select NodeAliasPath from View_CONTENT_Product_Joined where NodeSKUID = " + sku.SKUID);
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            if (ds.Tables[0].Rows.Count != 0)
            {
                string NodeAliasPath = Convert.ToString(ds.Tables[0].Rows[0]["NodeAliasPath"]);
                return "~" + NodeAliasPath + ".aspx";
            }
            else
            {
                return "#";
            }
        }
        return String.Empty;
    }

    protected int GetProductQuantity(object skuid, object quantity)
    {
        SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);
        var type = sku.GetValue("SKUType");
        var qte = Convert.ToInt32(quantity);

        if (type == null)
        {
            return qte;
        }

        if (Convert.ToInt32(type) == 1)
        {
            return qte;
        }

        return qte;
    }

}



