﻿// --------------------------------------------------------------------
// <copyright file="Update.aspx.cs" company="Smart Aquaponic">
// Copyright (c) Smart Aquaponic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------

namespace SmartAquaponic.Web.Fish
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;
    using SmartAquaponic.Business;
    using SmartAquaponic.Common.Enum;
    using SmartAquaponic.Common.Extensions;
    using SmartAquaponic.Common.Helpers;
    using SmartAquaponic.Domain;
    using SmartAquaponic.Domain.Enum;
    using SmartAquaponic.Web.Base;
    using Constants = SmartAquaponic.Common.Constants.WebConstant;

    /// <summary>
    /// Update.
    /// </summary>
    public partial class Update : CustomPage
    {
        /// <inheritdoc/>
        internal override void SetControls()
        {
            this.SetResources(this.Controls);
        }

        /// <summary>
        /// Page_Load.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">e.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    this.Build();
                }
            }
            catch (Exception ex)
            {
                this.ShowError(this.GetResource(Constants.Errors.UnexpectedError), hideMainPanel: true);
                this.LogMessage(ex);
            }
        }

        /// <summary>
        /// BtnSave_Click.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">e.</param>
        protected void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.IsEntityValid())
                {
                    var result = new FishBll(this.GetUser(), this.GetIp())
                        .Update(
                        new Fish()
                        {
                            Id = Convert.ToInt32(this.Request.QueryString[Constants.QueryStrings.Id]),
                            ModifiedDate = Convert.ToDateTime(this.FldModifiedDate.Value),
                            Name = this.TxtName.Text,
                            IsLonely = this.CbxIsLonely.Checked,
                            Ph = (PhType)Convert.ToInt32(this.DdlPh.SelectedValue),
                            Predators = this.GetPredatores(),
                            Size = Convert.ToDecimal(this.TxtSize.Text),
                            Temp = Convert.ToDecimal(this.TxtTemp.Text),
                            WaterRequired = Convert.ToInt32(this.TxtWaterRequired.Text),
                        });

                    if (result > 0)
                    {
                        this.Response.Redirect(Constants.Pages.Fish, false);
                    }
                    else
                    {
                        this.ShowError(this.GetResource(result.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowError(this.GetResource(Constants.Errors.UnexpectedError), hideMainPanel: true);
                this.LogMessage(ex);
            }
        }

        private void Build()
        {
            if (this.IsUserLogged())
            {
                this.IsUserAuthorized(Constants.Permission.FishManagement);
                this.SetControls();
                this.LoadData();
            }
        }

        private void LoadData()
        {
            var id = Convert.ToInt32(this.Request.QueryString[Constants.QueryStrings.Id]);

            if (id == 0)
            {
                this.ShowError(this.GetResource(Constants.Errors.NoResultsFound), hideMainPanel: true);
            }
            else
            {
                var fish = new FishBll().Read(id);

                if (fish == null)
                {
                    this.ShowError(this.GetResource(Constants.Errors.NoResultsFound), hideMainPanel: true);
                }
                else
                {
                    this.TxtName.Text = fish.Name;
                    this.TxtSize.Text = fish.Size.ToString();
                    this.TxtTemp.Text = fish.Temp.ToString();
                    this.TxtWaterRequired.Text = fish.WaterRequired.ToString();
                    this.CbxIsLonely.Checked = fish.IsLonely;
                    this.FldModifiedDate.Value = fish.ModifiedDate.ToDateTime2();

                    var result = new FishBll().Read();

                    foreach (var item in result)
                    {
                        if (item.Id != fish.Id)
                        {
                            DdlPredators.Items.Add(new ListItem(item.Name, item.Id.ToString())
                            {
                                Selected = fish.Predators.Any(x => x.Id == item.Id)
                            });
                        }
                    }

                    foreach (PhType type in Enum.GetValues(typeof(PhType)))
                    {
                        DdlPh.Items.Add(new ListItem($"{((int)type).ToString()} - {this.GetResource(type.ToString())}", ((int)type).ToString()));
                    }

                    DdlPh.SelectedValue = ((int)fish.Ph).ToString();
                }
            }
        }

        private void ShowError(string message, bool hideMainPanel = false, BootstrapContextualType alertType = BootstrapContextualType.Danger, bool isDismissible = true)
        {
            this.PnlMain.Visible = !hideMainPanel;

            this.AlertControl.Visible = true;
            this.AlertControl.Message = message;
            this.AlertControl.AlertType = alertType;
            this.AlertControl.IsAlertDismissible = isDismissible;
            this.AlertControl.PropertyBind();
        }

        private bool IsEntityValid()
        {
            var result = true;

            if (string.IsNullOrEmpty(this.TxtName.Text) || this.TxtName.Text.Length > 50)
            {
                this.TxtName.CssClass = BootstrapHerlper.GetInvalidFormClass();
                result = false;
            }
            else
            {
                this.TxtName.CssClass = BootstrapHerlper.GetValidFormClass();
            }

            if (string.IsNullOrEmpty(this.TxtSize.Text))
            {
                this.TxtSize.CssClass = BootstrapHerlper.GetInvalidFormClass();
                result = false;
            }
            else
            {
                this.TxtSize.CssClass = BootstrapHerlper.GetValidFormClass();
            }

            if (string.IsNullOrEmpty(this.TxtTemp.Text))
            {
                this.TxtTemp.CssClass = BootstrapHerlper.GetInvalidFormClass();
                result = false;
            }
            else
            {
                this.TxtTemp.CssClass = BootstrapHerlper.GetValidFormClass();
            }

            if (string.IsNullOrEmpty(this.TxtWaterRequired.Text))
            {
                this.TxtWaterRequired.CssClass = BootstrapHerlper.GetInvalidFormClass();
                result = false;
            }
            else
            {
                this.TxtWaterRequired.CssClass = BootstrapHerlper.GetValidFormClass();
            }

            return result;
        }

        private IList<Fish> GetPredatores()
        {
            var result = new List<Fish>();

            foreach (ListItem item in DdlPredators.Items)
            {
                if (item.Selected)
                {
                    result.Add(new Fish()
                    {
                        Id = Convert.ToInt32(item.Value),
                        Name = item.Text,
                    });
                }
            }

            return result;
        }
    }
}