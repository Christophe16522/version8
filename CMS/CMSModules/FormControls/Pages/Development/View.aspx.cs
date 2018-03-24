﻿using System;

using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;
using CMS.FormEngine;

[UIElement(ModuleName.BIZFORM, "View")]
public partial class CMSModules_FormControls_Pages_Development_View : GlobalAdminPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
        // Load form control
        int controlId = QueryHelper.GetInteger("controlId", 0);
        FormUserControlInfo fuci = FormUserControlInfoProvider.GetFormUserControlInfo(controlId);
        EditedObject = fuci;
        if (fuci != null)
        {
            ctrlView.FormControlName = fuci.UserControlCodeName;
        }

        btnSubmit.Click += btnSubmit_Click;
    }


    void btnSubmit_Click(object sender, EventArgs e)
    {
        string value = ValidationHelper.GetString(ctrlView.Value, null);
        ShowInformation(string.Format("Returned control value : '{0}'.", value));
    }
}