using Asi.Data;
using Asi.iBO.ContentManagement;
using Asi.Soa.ClientServices;
using Asi.Soa.Commerce.DataContracts;
using Asi.Soa.Core.DataContracts;
using Asi.Soa.Membership.DataContracts;
using Asi.Web.UI.WebControls;
using enSYNC.Custom.Soa.AutoDraft.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace enSYNC.Web.iParts.Custom.OAD
{
    public partial class OADPaymentDetailsDisplay : iPartDisplayBase
    {
        //Added a comment for Git test
        #region Event Handlers
        private EntityManager entityManager;

        private new EntityManager EntityManager
        {
            get
            {
                String username = Asi.AppContext.CurrentIdentity.UserId;

                try
                {
                    EntityManager em = new EntityManager();
                    QueryData usernameQuery = new QueryData("Name_Security");
                    usernameQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));

                    FindResultsData usernameResults = em.Find(usernameQuery);
                    if (usernameResults != null && usernameResults.Result != null && usernameResults.Result.Count > 0)
                    {
                        GenericEntityData usernameEntity = usernameResults.Result[0] as GenericEntityData;
                        if (usernameEntity["WEB_LOGIN"] != null)
                        {
                            username = usernameEntity["WEB_LOGIN"].ToString();
                        }
                    }
                }
                catch (Exception error)
                {
                    ErrorLabel.Text = error.Message + error.StackTrace;
                }

                if (entityManager == null || entityManager.UserName != username)
                    entityManager = new EntityManager(username);
                return entityManager;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!HideContent)
            {
                var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

                if (EntityManager.UserName == String.Empty || EntityManager.UserName == "GUEST" || (!OnlyUpdateAutoDraftAccountConfig && cartManager.CartIsEmpty) || (!OnlyUpdateAutoDraftAccountConfig && !CheckComplimentary()))
                {
                    HideContent = true;
                    //HideContent = false;
                    //Response.Write("HideContent On load " + HideContent);

                }
                else
                {
                    HideContent = false;
                    //Response.Write("ID " + Asi.Security.Utility.SecurityHelper.GetSelectedImisId() + "HideContent2 On load " + HideContent);
                }
            }
            Page.MaintainScrollPositionOnPostBack = true;

            if (!IsPostBack)
                OneTimeInitializations();
            bool validatePaymentMethod = false;
            //Response.Write("<br/>OnlyUpdateAutoDraftAccountConfig " + OnlyUpdateAutoDraftAccountConfig);

            if (OnlyUpdateAutoDraftAccountConfig)
            {
                if (NoUpdateLabel.Visible)
                {
                    CardsAcceptedPanel.Visible = false;
                    CardInformationFieldsPanel.Visible = false;
                    AccountInformationFieldsPanel.Visible = false;
                    AccountNamePanel.Visible = false;
                    BillingAddressPanel.Visible = false;

                }
                else
                {
                    //MP 06/20/2016
                    //Response.Write("1");//AccountNamePanel.Visible = true;
                    //MP 08/19/2016 On load show Autohrization checkbox
                    AuthorizationCheckBox.Visible = true;

                    validatePaymentMethod = true;
                }
                //NewAccount.Visible = false;
                BillingAddressPanel.Visible = false;

                if (!IsPostBack)
                {

                    if (AuthorizationTextConfig.Length > 0)
                        AuthorizationCheckBox.Text = HttpUtility.HtmlDecode(AuthorizationTextConfig) + " ";

                    if (MembershipInstallmentTextConfig.Length > 0)
                        MembershipInstallmentCheckBox.Text = HttpUtility.HtmlDecode(MembershipInstallmentTextConfig);

                    if (MembershipAutoPayTextConfig.Length > 0)
                        MembershipAutoPayCheckBox.Text = HttpUtility.HtmlDecode(MembershipAutoPayTextConfig);

                    if (DonationTextConfig.Length > 0)
                        DonationCheckBox.Text = HttpUtility.HtmlDecode(DonationTextConfig);

                    if (AuthorizationCheckBoxConfig)
                        SaveButton.Visible = false;
                    else
                        SaveButton.Visible = true;
                }
            }
            else
            {
                SaveButtonPanel.Visible = false;
                BillingAddressPanel.Visible = true;
                //Response.Write("<br/> useautodraftconfig " + UseAutoDraftConfig);
                if (UseAutoDraftConfig)
                {
                    //    Response.Write("<br/> checkbasket " + CheckBasketForAutoDraftProducts());

                    //MP 06/20/2016
                    /**
                    if (CheckBasketForAutoDraftProducts())
                        Response.Write("2");//AccountNamePanel.Visible = true; 
                    else
                        AccountNamePanel.Visible = false;
                     * **/
                    //MP 06/20/2016

                    if (ExistingAccountConfig)
                        ExistingAccountFirstPanel.Visible = true;
                    else
                        ExistingAccountFirstPanel.Visible = false;
                }
                else
                {
                    ExistingAccountFirstPanel.Visible = false;
                    AccountNamePanel.Visible = false;
                }
                validatePaymentMethod = true;
            }
            //Response.Write("<br/>Validate payment method " + validatePaymentMethod);
            if (validatePaymentMethod)
            {
                if (PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString())
                {
                    PaymentMethodPanel.Visible = true;
                    if (!IsPostBack && !OnlyUpdateAutoDraftAccountConfig)
                    {
                        if (!String.IsNullOrEmpty(ACHAccountID))
                        {
                            PaymentMethodList.SelectedValue = PaymentMethodOptions.BankAccount.ToString();
                            //MP 11/08/2016 Non immediate ACH setting session variable for Non immediate ACh
                            Session["PaymentMethod"] = PaymentMethodOptions.BankAccount.ToString();
                            //Response.Write("he-ah 1");
                        }
                    }

                    if (PaymentMethodList.SelectedValue == PaymentMethodOptions.BankAccount.ToString())
                    {
                        CardInformationFieldsPanel.Visible = false;
                        AccountInformationFieldsPanel.Visible = true;
                        if (HideSavingsAccount)
                            SavingRadioButton.Visible = false;
                        //MP 11/14/2016 If config is checked, show check panel
                        if (ShowCheckImage)
                            CheckImagePanel.Visible = true;
                        CardsAcceptedPanel.Visible = false;
                        BillingAddressPanel.Visible = false;
                        if (!CheckingRadioButton.Checked && !SavingRadioButton.Checked)
                            CheckingRadioButton.Checked = true;
                        if (!IsPostBack)
                        {
                            if (CanadianBankConfig)
                                CanadianBankPanel.Visible = true;
                            else
                                CanadianBankPanel.Visible = false;
                        }
                        //MP 11/08/2016  Non immediate ACH setting session variable for Non immediate ACh
                        Session["PaymentMethod"] = PaymentMethodOptions.BankAccount.ToString();
                        //Response.Write("he-ah 2");
                    }
                    else if (PaymentMethodList.SelectedValue == PaymentMethodOptions.CreditCard.ToString())
                    {
                        CardInformationFieldsPanel.Visible = true;
                        AccountInformationFieldsPanel.Visible = false;
                        CardsAcceptedPanel.Visible = true;
                        CanadianBankPanel.Visible = false;
                        if (!OnlyUpdateAutoDraftAccountConfig)
                            BillingAddressPanel.Visible = true;
                        else
                            BillingAddressPanel.Visible = false;
                        //MP 11/08/2016  Non immediate ACH setting session variable for Non immediate ACh
                        Session["PaymentMethod"] = PaymentMethodOptions.CreditCard.ToString();
                        //Response.Write("he-ah 3");
                    }
                }
                else
                {
                    PaymentMethodPanel.Visible = false;
                    if (PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString())
                    {
                        CardInformationFieldsPanel.Visible = true;
                        AccountInformationFieldsPanel.Visible = false;
                        CardsAcceptedPanel.Visible = true;
                        CanadianBankPanel.Visible = false;
                        if (!OnlyUpdateAutoDraftAccountConfig)
                            BillingAddressPanel.Visible = true;
                        else
                            BillingAddressPanel.Visible = false;
                        //MP 11/08/2016  Non immediate ACH setting session variable for Non immediate ACh							
                        Session["PaymentMethod"] = PaymentMethodOptions.CreditCard.ToString();

                        //Response.Write("he-ah 4");
                    }
                    else if (PaymentMethodAllowedConfig == PaymentMethodOptions.BankAccount.ToString())
                    {
                        CardInformationFieldsPanel.Visible = false;
                        AccountInformationFieldsPanel.Visible = true;
                        if (HideSavingsAccount)
                            SavingRadioButton.Visible = false;
                        //MP 11/14/2016 If config is checked, show check panel
                        if (ShowCheckImage)
                            CheckImagePanel.Visible = true;
                        CardsAcceptedPanel.Visible = false;
                        BillingAddressPanel.Visible = false;
                        if (!CheckingRadioButton.Checked && !SavingRadioButton.Checked)
                            CheckingRadioButton.Checked = true;
                        if (!IsPostBack)
                        {
                            if (CanadianBankConfig)
                                CanadianBankPanel.Visible = true;
                            else
                                CanadianBankPanel.Visible = false;
                        }
                        //MP 11/08/2016  Non immediate ACH setting session variable for Non immediate ACh
                        Session["PaymentMethod"] = PaymentMethodOptions.BankAccount.ToString();

                        //Response.Write("he-ah 5");
                    }
                }
            }

            try
            {
                ErrorLabel.Text = "";
                if (!IsPostBack)
                {
                    if (NameOnCardLabelConfig.Length > 0)
                        CreditCardNameLabel.Text = HttpUtility.HtmlDecode(NameOnCardLabelConfig);
                    if (CardTypeLabelConfig.Length > 0)
                        CreditCardTypeLabel.Text = HttpUtility.HtmlDecode(CardTypeLabelConfig);
                    if (CardNumberLabelConfig.Length > 0)
                        CreditCardNumLabel.Text = HttpUtility.HtmlDecode(CardNumberLabelConfig);
                    if (ExpirationDateLabelConfig.Length > 0)
                        CreditCardExpirationLabel.Text = HttpUtility.HtmlDecode(ExpirationDateLabelConfig);
                    if (CardSecurityCodeLabelConfig.Length > 0)
                        CreditCardSecurityCodeLabel.Text = HttpUtility.HtmlDecode(CardSecurityCodeLabelConfig);
                    if (AccountNameLabelConfig.Length > 0)
                        NicknameCardLabel.Text = HttpUtility.HtmlDecode(AccountNameLabelConfig);
                    if (AccountNameExampleTextConfig.Length > 0)
                        NicknameExampleTextLabel.Text = HttpUtility.HtmlDecode(AccountNameExampleTextConfig);
                    if (AddressPurposeLabelConfig.Length > 0)
                        AddressPurposeLabel.Text = HttpUtility.HtmlDecode(AddressPurposeLabelConfig);
                    if (SavingConfig.Length > 0)
                        SavingRadioButton.Text = SavingConfig;
                    if (CheckingConfig.Length > 0)
                        CheckingRadioButton.Text = CheckingConfig;
                    if (NameOnAccountConfig.Length > 0)
                        AccountNameLabel.Text = HttpUtility.HtmlDecode(NameOnAccountConfig);
                    if (CanadianBankNumberConfig.Length > 0)
                        CanadianBankNumberLabel.Text = HttpUtility.HtmlDecode(CanadianBankNumberConfig);
                    if (AccountNumberConfig.Length > 0)
                        AccountNumberLabel.Text = HttpUtility.HtmlDecode(AccountNumberConfig);
                    if (RoutingNumberConfig.Length > 0)
                        RoutingNumberLabel.Text = HttpUtility.HtmlDecode(RoutingNumberConfig);
                    if (BankNameConfig.Length > 0)
                        BankNameLabel.Text = HttpUtility.HtmlDecode(BankNameConfig);

                    for (int i = 0; i <= 10; i++)
                    {
                        ListItem item = new ListItem();
                        item.Text = (DateTime.Today.Year + i).ToString();
                        item.Value = (DateTime.Today.Year + i).ToString();
                        CreditCardExpirationYearList.Items.Add(item);
                    }

                    if (CountryConfig)
                    {
                        CountryPanel.Visible = true;
                        StateRequiredLabel.Visible = false;
                        StateValidator.Visible = false;
                        StateExtraSpaceLabel.Visible = true;
                    }
                    else
                    {
                        CountryPanel.Visible = false;
                        StateRequiredLabel.Visible = true;
                        StateValidator.Visible = true;
                        StateExtraSpaceLabel.Visible = false;
                    }

                    Session.Remove("OADPaymentDetails.UseMemberAccountID");

                    QueryData addressQuery = new QueryData("AddressPurpose");
                    FindResultsData addressResults = EntityManager.Find(addressQuery);
                    if (addressResults != null && addressResults.Result.Count > 0)
                    {
                        foreach (AddressPurposeData address in addressResults.Result)
                        {
                            ListItem item = new ListItem();
                            item.Text = address.Name;
                            item.Value = address.AddressPurposeId;

                            if (!String.IsNullOrEmpty(DefaultAddressPurposeConfig) && DefaultAddressPurposeConfig == address.AddressPurposeId)
                                item.Selected = true;

                            AddressPurposeList.Items.Add(item);
                        }
                    }

                    var countryEntityQuery = new QueryData("CountryRef") { Pager = new PagerData { PageSize = 100, PageCount = 1, PageNumber = 0 } };
                    while (countryEntityQuery.Pager.PageNumber < countryEntityQuery.Pager.PageCount)
                    {
                        var countryEntityResults = EntityManager.Find(countryEntityQuery);
                        countryEntityResults.Query.Pager.PageNumber++; //Response.Write("result.Result.Count " + countryEntityResults.Result.Count + "<br/>");
                        if (countryEntityResults.Result != null && countryEntityResults.Result.Count > 0)
                        {
                            foreach (GenericEntityData country in countryEntityResults.Result)
                            {
                                ListItem item = new ListItem();
                                item.Text = country["CountryName"].ToString();
                                item.Value = country["CountryCode"].ToString();
                                CountryList.Items.Add(item);
                            }
                        }
                    }

                    if (CountryList.Items.Count > 0)
                    {
                        var list = CountryList.Items.Cast<ListItem>().OrderBy(x => x.Text).ToList();
                        CountryList.Items.Clear();
                        CountryList.DataSource = list;
                        CountryList.DataTextField = "Text";
                        CountryList.DataValueField = "Value";
                        CountryList.DataBind();
                    }

                    QueryData countrySubEntityQuery = new QueryData("StateProvinceRef");
                    if (CountryPanel.Visible || !string.IsNullOrEmpty(CountryList.SelectedValue))
                        countrySubEntityQuery.AddCriteria(new CriteriaData("COUNTRYCODE", OperationData.Equal, CountryList.SelectedValue));
                    else
                        countrySubEntityQuery.AddCriteria(new CriteriaData("COUNTRYCODE", OperationData.Equal, "US"));

                    FindResultsData countrySubEntityResults = EntityManager.Find(countrySubEntityQuery);
                    if (countrySubEntityResults != null && countrySubEntityResults.Result.Count > 0)
                    {
                        foreach (GenericEntityData countrySubEntity in countrySubEntityResults.Result)
                        {
                            ListItem item = new ListItem();
                            item.Text = countrySubEntity["Description"].ToString();
                            item.Value = countrySubEntity["Code"].ToString();
                            StateList.Items.Add(item);
                        }
                        StateList.Visible = true;
                        StateTextBox.Visible = false;
                    }
                    else
                    {
                        StateList.Visible = false;
                        StateTextBox.Visible = true;
                    }

                    if (BillingAddressPanel.Visible)
                        AddressPurpose_Changed(AddressPurposeList, e);
                }

                if (AcceptedCardsConfig.Length > 0)
                {
                    String[] acceptedCards = AcceptedCardsConfig.Split(',');

                    foreach (string card in acceptedCards)
                    {
                        if (card.Contains("AMEX"))
                        {
                            Image image = new Image();
                            //image.ID = card;
                            image.Width = 50;
                            image.Height = 35;
                            image.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/AcceptedCards/AMEX.png";

                            CardsAcceptedPanel.Controls.Add(image);
                        }
                        if (card.Contains("DISC"))
                        {
                            Image image = new Image();
                            //image.ID = card;
                            image.Width = 50;
                            image.Height = 35;
                            image.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/AcceptedCards/DISC.png";

                            CardsAcceptedPanel.Controls.Add(image);
                        }
                        if (card.Contains("MC"))
                        {
                            Image image = new Image();
                            //image.ID = card;
                            image.Width = 50;
                            image.Height = 35;
                            image.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/AcceptedCards/MC.png";

                            CardsAcceptedPanel.Controls.Add(image);
                        }
                        if (card.Contains("VISA"))
                        {
                            Image image = new Image();
                            //image.ID = card;
                            image.Width = 50;
                            image.Height = 35;
                            image.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/AcceptedCards/VISA.png";

                            CardsAcceptedPanel.Controls.Add(image);
                        }
                    }

                    if (!IsPostBack)
                    {
                        QueryData cashAccountsQuery = new QueryData("Cash_Accounts");
                        cashAccountsQuery.AddCriteria(new CriteriaData("CASH_ACCOUNT_CODE", OperationData.In, acceptedCards));
                        FindResultsData cashAccountsResults = EntityManager.Find(cashAccountsQuery);
                        if (cashAccountsResults != null && cashAccountsResults.Result.Count > 0)
                        {
                            foreach (GenericEntityData cashAccount in cashAccountsResults.Result)
                            {
                                ListItem item = new ListItem();
                                item.Text = cashAccount["DESCRIPTION"].ToString();
                                item.Value = cashAccount["CASH_ACCOUNT_CODE"].ToString();
                                CreditCardTypeList.Items.Add(item);
                            }
                        }
                    }
                }

                if (CanadianBankConfig)
                {
                    Image canadianImage = new Image();
                    canadianImage.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/CheckSample/CANADIANCHECK.jpg";
                    CheckImagePanel.Controls.Add(canadianImage);
                }
                else
                {
                    Image image = new Image();
                    image.ImageUrl = "../../../images/enSYNC/OnlineAutoDraft/CheckSample/USCHECK.png";
                    CheckImagePanel.Controls.Add(image);
                }

                //string imisId = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                //if(CompanyCardsConfig)
                //    imisId = Request.QueryString["ID"];

                //Response.Write("<br/>ID " + imisId);
                //Response.Write("ID " + ImisId);
                if (ExistingAccountConfig)
                {
                    DisplayExistingCards(ImisId);
                }
                RemoveTentativeAccount();
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //Response.Write("HideContent " + HideContent);
            if (!HideContent)
            {
                if (DoNotRenderInDesignMode && IsContentDesignMode)
                    HideContent = true;
                else
                    HideContent = false;

                if (Utility.CheckLicense())
                {
                    //var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                    //if (EntityManager.UserName == String.Empty || EntityManager.UserName == "GUEST" || (!OnlyUpdateAutoDraftAccountConfig && !CheckComplimentary())) // || (!OnlyUpdateAutoDraftAccountConfig && cartManager.CartIsEmpty)
                    //{
                    //    //HideContent = true;
                    //    HideContent = false;
                    //    Response.Write("HideContent1 " + HideContent);

                    //}
                    //else
                    //{
                    //    HideContent = false;
                    //    Response.Write("HideContent2 " + HideContent);
                    //}
                }
                else
                {
                    ErrorLabel.Text = "Not licensed for Online AutoDraft.";
                    HideContent = true;
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // This is sample code that shows how to add a command button to the header area, and how to add
            // an optional command to the 'Options' dropdown.
            // Initialize command buttons and dropdown commands
            if (!IsContentDesignMode)
            {
                // Create an 'Edit' command button                
                // Note that the button images are specified in a .skin file, with a SkinID that matches the SkinID below                


            }
        }

        #endregion

        #region Properties

        // TODO: Remove this example property, and add your configuration properties here.

        /// <summary>
        /// Enables/disables the display of the current user.
        /// </summary>
        /// 
        public bool AuthorizationCheckBoxConfig
        {
            get
            {
                if (ViewState["AuthorizationCheckBoxConfig"] == null)
                    return true;

                return (bool)ViewState["AuthorizationCheckBoxConfig"];
            }
            set
            {
                ViewState["AuthorizationCheckBoxConfig"] = value;
            }
        }

        public bool SaveToDatabaseConfig
        {
            get
            {
                if (ViewState["SaveToDatabaseConfig"] == null)
                    return true;

                return (bool)ViewState["SaveToDatabaseConfig"];
            }
            set
            {
                ViewState["SaveToDatabaseConfig"] = value;
            }
        }

        public string AuthorizationType
        {
            get
            {
                if (ViewState["AuthorizationType"] == null)
                    return "";

                return ViewState["AuthorizationType"].ToString();
            }
            set
            {
                ViewState["AuthorizationType"] = value;
            }
        }

        public string AuthorizationTextConfig
        {
            get
            {
                if (ViewState["AuthorizationTextConfig"] == null)
                    return "";

                return ViewState["AuthorizationTextConfig"].ToString();
            }
            set
            {
                ViewState["AuthorizationTextConfig"] = value;
            }
        }

        public string MembershipInstallmentTextConfig
        {
            get
            {
                if (ViewState["MembershipInstallmentTextConfig"] == null)
                    return "";

                return ViewState["MembershipInstallmentTextConfig"].ToString();
            }
            set
            {
                ViewState["MembershipInstallmentTextConfig"] = value;
            }
        }

        public string MembershipAutoPayTextConfig
        {
            get
            {
                if (ViewState["MembershipAutoPayTextConfig"] == null)
                    return "";

                return ViewState["MembershipAutoPayTextConfig"].ToString();
            }
            set
            {
                ViewState["MembershipAutoPayTextConfig"] = value;
            }
        }

        public string DonationTextConfig
        {
            get
            {
                if (ViewState["DonationTextConfig"] == null)
                    return "";

                return ViewState["DonationTextConfig"].ToString();
            }
            set
            {
                ViewState["DonationTextConfig"] = value;
            }
        }

        public bool OnlyUpdateAutoDraftAccountConfig
        {
            get
            {
                if (ViewState["OnlyUpdateAutoDraftAccountConfig"] == null)
                    return true;

                return (bool)ViewState["OnlyUpdateAutoDraftAccountConfig"];
            }
            set
            {
                ViewState["OnlyUpdateAutoDraftAccountConfig"] = value;
            }
        }

        public bool ExistingAccountConfig
        {
            get
            {
                if (ViewState["ExistingAccountConfig"] == null)
                    return true;

                return (bool)ViewState["ExistingAccountConfig"];
            }
            set
            {
                ViewState["ExistingAccountConfig"] = value;
            }
        }

        public bool UseAutoDraftConfig
        {
            get
            {
                if (ViewState["UseAutoDraftConfig"] == null)
                    return true;

                return (bool)ViewState["UseAutoDraftConfig"];
            }
            set
            {
                ViewState["UseAutoDraftConfig"] = value;
            }
        }

        public bool CountryConfig
        {
            get
            {
                if (ViewState["CountryConfig"] == null)
                    return true;

                return (bool)ViewState["CountryConfig"];
            }
            set
            {
                ViewState["CountryConfig"] = value;
            }
        }

        public bool CanadianBankConfig
        {
            get
            {
                if (ViewState["CanadianBankConfig"] == null)
                    return true;

                return (bool)ViewState["CanadianBankConfig"];
            }
            set
            {
                ViewState["CanadianBankConfig"] = value;
            }
        }

        public string AcceptedCardsConfig
        {
            get
            {
                if (ViewState["AcceptedCardsConfig"] == null)
                    return "";

                return ViewState["AcceptedCardsConfig"].ToString();
            }
            set
            {
                ViewState["AcceptedCardsConfig"] = value;
            }
        }

        public string NameOnCardLabelConfig
        {
            get
            {
                if (ViewState["NameOnCardLabelConfig"] == null)
                    return "";

                return ViewState["NameOnCardLabelConfig"].ToString();
            }
            set
            {
                ViewState["NameOnCardLabelConfig"] = value;
            }
        }

        public string CardTypeLabelConfig
        {
            get
            {
                if (ViewState["CardTypeLabelConfig"] == null)
                    return "";

                return ViewState["CardTypeLabelConfig"].ToString();
            }
            set
            {
                ViewState["CardTypeLabelConfig"] = value;
            }
        }

        public string CardNumberLabelConfig
        {
            get
            {
                if (ViewState["CardNumberLabelConfig"] == null)
                    return "";

                return ViewState["CardNumberLabelConfig"].ToString();
            }
            set
            {
                ViewState["CardNumberLabelConfig"] = value;
            }
        }

        public string ExpirationDateLabelConfig
        {
            get
            {
                if (ViewState["ExpirationDateLabelConfig"] == null)
                    return "";

                return ViewState["ExpirationDateLabelConfig"].ToString();
            }
            set
            {
                ViewState["ExpirationDateLabelConfig"] = value;
            }
        }

        public string CardSecurityCodeLabelConfig
        {
            get
            {
                if (ViewState["CardSecurityCodeLabelConfig"] == null)
                    return "";

                return ViewState["CardSecurityCodeLabelConfig"].ToString();
            }
            set
            {
                ViewState["CardSecurityCodeLabelConfig"] = value;
            }
        }

        public string AccountNameLabelConfig
        {
            get
            {
                if (ViewState["AccountNameLabelConfig"] == null)
                    return "";

                return ViewState["AccountNameLabelConfig"].ToString();
            }
            set
            {
                ViewState["AccountNameLabelConfig"] = value;
            }
        }

        public string AddressPurposeLabelConfig
        {
            get
            {
                if (ViewState["AddressPurposeLabelConfig"] == null)
                    return "";

                return ViewState["AddressPurposeLabelConfig"].ToString();
            }
            set
            {
                ViewState["AddressPurposeLabelConfig"] = value;
            }
        }

        public string DefaultAddressPurposeConfig
        {
            get
            {
                if (ViewState["DefaultAddressPurposeConfig"] == null)
                    return "";

                return ViewState["DefaultAddressPurposeConfig"].ToString();
            }
            set
            {
                ViewState["DefaultAddressPurposeConfig"] = value;
            }
        }

        public string PaymentMethodAllowedConfig
        {
            get
            {
                if (ViewState["PaymentMethodAllowedConfig"] == null)
                    return "";

                return ViewState["PaymentMethodAllowedConfig"].ToString();
            }
            set
            {
                this.ViewState["PaymentMethodAllowedConfig"] = value;
            }
        }

        public string SavingConfig
        {
            get
            {
                if (ViewState["SavingConfig"] == null)
                    return "";

                return ViewState["SavingConfig"].ToString();
            }
            set
            {
                this.ViewState["SavingConfig"] = value;
            }
        }

        public string CheckingConfig
        {
            get
            {
                if (ViewState["CheckingConfig"] == null)
                    return "";

                return ViewState["CheckingConfig"].ToString();
            }
            set
            {
                this.ViewState["CheckingConfig"] = value;
            }
        }

        public string NameOnAccountConfig
        {
            get
            {
                if (ViewState["NameOnAccountConfig"] == null)
                    return "";

                return ViewState["NameOnAccountConfig"].ToString();
            }
            set
            {
                this.ViewState["NameOnAccountConfig"] = value;
            }
        }

        public string AccountNumberConfig
        {
            get
            {
                if (ViewState["AccountNumberConfig"] == null)
                    return "";

                return ViewState["AccountNumberConfig"].ToString();
            }
            set
            {
                this.ViewState["AccountNumberConfig"] = value;
            }
        }

        public string CanadianBankNumberConfig
        {
            get
            {
                if (ViewState["CanadianBankNumberConfig"] == null)
                    return "";

                return ViewState["CanadianBankNumberConfig"].ToString();
            }
            set
            {
                this.ViewState["CanadianBankNumberConfig"] = value;
            }
        }

        public string AccountNameExampleTextConfig
        {
            get
            {
                if (ViewState["AccountNameExampleTextConfig"] == null)
                    return "";

                return ViewState["AccountNameExampleTextConfig"].ToString();
            }
            set
            {
                this.ViewState["AccountNameExampleTextConfig"] = value;
            }
        }

        public string RoutingNumberConfig
        {
            get
            {
                if (ViewState["RoutingNumberConfig"] == null)
                    return "";

                return ViewState["RoutingNumberConfig"].ToString();
            }
            set
            {
                this.ViewState["RoutingNumberConfig"] = value;
            }
        }

        public string BankNameConfig
        {
            get
            {
                if (ViewState["BankNameConfig"] == null)
                    return "";

                return ViewState["BankNameConfig"].ToString();
            }
            set
            {
                this.ViewState["BankNameConfig"] = value;
            }
        }

        public bool ShowCheckImage
        {
            get
            {
                if (ViewState["ShowCheckImage"] == null)
                    return true;

                return (bool)ViewState["ShowCheckImage"];
            }
            set
            {
                ViewState["ShowCheckImage"] = value;
            }
        }

        public bool HideSavingsAccount
        {
            get
            {
                if (ViewState["HideSavingsAccount"] == null)
                    return true;

                return (bool)ViewState["HideSavingsAccount"];
            }
            set
            {
                ViewState["HideSavingsAccount"] = value;
            }
        }

        public string ACHAccountID
        {
            get
            {
                string achAccountId = String.Empty;

                try
                {
                    QueryData giftQuery = new QueryData("OAD_Enrollment_Tracker");
                    giftQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                    FindResultsData giftResults = EntityManager.Find(giftQuery);

                    if (giftResults != null && giftResults.Result != null && giftResults.Result.Count > 0)
                    {
                        foreach (GenericEntityData existingEnrollmentEntity in giftResults.Result)
                        {
                            if (existingEnrollmentEntity["ACH_ACCOUNT_ID"] != null)
                                achAccountId = existingEnrollmentEntity["ACH_ACCOUNT_ID"].ToString();
                        }
                    }
                }
                catch (Exception error)
                {
                    ErrorLabel.Text += error.Message + error.StackTrace;
                }
                return achAccountId;
            }
        }

        public bool AutoEnrollment
        {
            get
            {
                if (ViewState["AutoEnrollment"] == null)
                    return true;

                return (bool)ViewState["AutoEnrollment"];
            }
            set
            {
                ViewState["AutoEnrollment"] = value;
            }
        }

        public bool CompanyCardsConfig
        {
            get
            {
                if (ViewState["CompanyCardsConfig"] == null)
                    return true;

                return (bool)ViewState["CompanyCardsConfig"];
            }
            set
            {
                ViewState["CompanyCardsConfig"] = value;
            }
        }

        public bool IsCompanyAdmin
        {
            get
            {
                //char separator = ',';
                CWebUser loggedInUser = CWebUser.LoginByPrincipal(HttpContext.Current.User);
                Asi.iBO.DataServer companyAdminDS = new Asi.iBO.DataServer(loggedInUser);

                string companyAdminQuery = "SELECT ID, RELATION_TYPE, TARGET_ID from Relationship where TARGET_ID = '" + Asi.Security.Utility.SecurityHelper.GetSelectedImisId() + "' ";
                SqlDataReader companyAdminReader = companyAdminDS.ExecuteReader(System.Data.CommandType.Text, companyAdminQuery);
                try
                {
                    if (companyAdminReader.HasRows)
                    {
                        //Response.Write("<br/>HasRows " + companyAdminReader.RecordsAffected);
                        while (companyAdminReader.Read())
                        {
                            //Response.Write("<br/> companyAdminReader[RELATION_TYPE].ToString()" + companyAdminReader["RELATION_TYPE"].ToString());
                            if (companyAdminReader["RELATION_TYPE"].ToString().Equals("_ORG-ADMIN"))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    ErrorLabel.Text = error.Message + error.StackTrace;
                }
                finally
                {
                    companyAdminReader.Close();
                    companyAdminDS.CloseConnection();
                }
                return false;
            }
        }

        public string ImisId
        {
            get
            {
                string imisId = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                bool isCompanyAdmin = IsCompanyAdmin;
                try
                {
                    //Response.Write("CompanyCardsConfig " + CompanyCardsConfig + "IsCompanyAdmin " + IsCompanyAdmin);
                    if (CompanyCardsConfig && isCompanyAdmin && OnlyUpdateAutoDraftAccountConfig)
                    {
                        //PartyData partyData = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartyData;
                        //if (partyData.AdditionalAttributes.GetPropertyValue("ParentPartyId") != null)
                        //{
                        //    imisId = partyData.AdditionalAttributes["ParentPartyId"].Value.ToString();
                        //}
                        imisId = Request.QueryString["ID"];
                    }
                    else if (CompanyCardsConfig && isCompanyAdmin && !OnlyUpdateAutoDraftAccountConfig)
                    {
                        var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

                        if (cartManager.Cart.ComboOrder.Invoices.Count > 0)
                        {
                            foreach (InvoiceSummaryData invoiceSummaryData in cartManager.Cart.ComboOrder.Invoices)
                            {
                                //Response.Write("SoldToParty.Id " + invoiceSummaryData.SoldToParty.Id + "loggedId " + Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                                if (invoiceSummaryData.SoldToParty.Id != Asi.Security.Utility.SecurityHelper.GetSelectedImisId())
                                {
                                    imisId = invoiceSummaryData.SoldToParty.Id;
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    ErrorLabel.Text = error.Message + error.StackTrace;
                }
                //Response.Write("imisId " + imisId);
                return imisId;
            }
        }

        public bool HideDeleteButton
        {
            get
            {
                if (ViewState["HideDeleteButton"] == null)
                    return true;

                return (bool)ViewState["HideDeleteButton"];
            }
            set
            {
                ViewState["HideDeleteButton"] = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the appropriate object
        /// </summary>
        /// <returns></returns>
        public override Asi.Business.ContentManagement.ContentItem CreateContentItem()
        {
            var item = new OADPaymentDetailsCommon { ContentItemKey = ContentItemKey };
            return item;
        }

        /// <summary>
        /// Logic that will execute on initial page load
        /// </summary>
        private void OneTimeInitializations()
        {
        }

        /// <summary>
        /// Resests fields
        /// </summary>
        /// <returns></returns>
        protected void ResetFields()
        {
            foreach (Control control in ExistingAccountsPanel.Controls)
            {
                if (control is StyledButton)
                {
                    StyledButton button = (StyledButton)control;
                    button.Enabled = false;
                }
            }

            CreditCardNameTextBox.Text = String.Empty;
            CreditCardTypeList.SelectedValue = "default";
            CreditCardNumTextBox.Text = String.Empty;
            CreditCardExpirationList.SelectedValue = "01";
            CreditCardExpirationYearList.SelectedValue = DateTime.Now.Year.ToString();
            CreditCardSecurityCodeTextBox.Text = String.Empty;
            NicknameCardTextBox.Text = String.Empty;
            BillingAddressTextBox.Text = String.Empty;
            BillingAddressTextBox1.Text = String.Empty;
            BillingAddressTextBox2.Text = String.Empty;
            CityTextBox.Text = String.Empty;
            //StateList.SelectedValue = address.CountrySubEntityCode;
            //CountryList.SelectedValue = address.CountyCode;
            ZipTextBox.Text = String.Empty;

            CheckingRadioButton.Checked = true;
            SavingRadioButton.Checked = false;
            AccountNameTextBox.Text = String.Empty;
            CanadianBankNumberTextBox.Text = String.Empty;
            AccountNumberTextBox.Text = String.Empty;
            RoutingNumberTextBox.Text = String.Empty;
            BankNameTextBox.Text = String.Empty;

            CreditCardNameHiddenField.Value = String.Empty;
            CreditCardTypeHiddenField.Value = String.Empty;
            CreditCardNumHiddenField.Value = String.Empty;
            CreditCardExpirationHiddenField.Value = String.Empty;
            CreditCardExpirationYearHiddenField.Value = String.Empty;
            NicknameCardHiddenField.Value = String.Empty;
            AddressPurposeHiddenField.Value = String.Empty;

            IsCheckingHiddenField.Value = String.Empty;
            CanadianBankNumberHiddenField.Value = String.Empty;
            AccountNumberHiddenField.Value = String.Empty;
            AccountNameHiddenField.Value = String.Empty;
            RoutingNumberHiddenField.Value = String.Empty;
            BankNameHiddenField.Value = String.Empty;

            //if (OnlyUpdateAutoDraftAccountConfig)
            //{
            //    CardInformationFieldsPanel.Visible = false;
            //    AccountInformationFieldsPanel.Visible = false;
            //    AccountNamePanel.Visible = false;
            //}
        }

        /// <summary>
        /// Displays exisiting credit cards
        /// </summary>
        /// <returns></returns>
        protected void DisplayExistingCards(string imisId)
        {
            try
            {
                //Response.Write("<br/> Display existing cards " + imisId);
                bool exists = false;
                var query = new QueryData("MemberAccount");
                if (imisId != null)
                    query.AddCriteria(CriteriaData.Equal("PartyId", imisId));
                //MP - 09/26/2016 commented below - added imisID instead of GetSelectedImisId
                //query.AddCriteria(CriteriaData.Equal("PartyId", Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));

                FindResultsData memberAccountResults = EntityManager.Find(query);
                //NewAccount.Visible = true;
                //Response.Write("<br/>memberAccountResults.Result.Count " + memberAccountResults.Result.Count);
                if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                {
                    //ExistingAccountFirstPanel.Visible = true;
                    NoUpdateLabel.Visible = false;
                    ExistingAccountFirstPanel.Visible = true;
                    //MP 06/27/2016 New account visible on update ipart - removed not in the below condition
                    //if(!OnlyUpdateAutoDraftAccountConfig)
                    //if(OnlyUpdateAutoDraftAccountConfig)
                    //Response.Write("<br/> Display newacc ");


                    foreach (MemberAccountData memberAccountData in memberAccountResults.Result)
                    {
                        bool add = false;
                        if (memberAccountData.DraftType != DraftTypeData.ECheck)
                        {
                            if (PaymentMethodAllowedConfig == PaymentMethodOptions.BankAccount.ToString() && ((memberAccountData.DraftType == DraftTypeData.USBankDraft && !CanadianBankConfig && (memberAccountData.ACHInformation.IsCanadianBank == null || (memberAccountData.ACHInformation.IsCanadianBank != null && !(bool)memberAccountData.ACHInformation.IsCanadianBank))) || (memberAccountData.DraftType == DraftTypeData.CanadianBank && CanadianBankConfig && (bool)memberAccountData.ACHInformation.IsCanadianBank)))
                                add = true;
                            else if (PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString() && memberAccountData.DraftType == DraftTypeData.CreditorDebitCard && CreditCardTypeList.Items.FindByValue(memberAccountData.CreditOrDebitCardInformation.CardType) != null)
                                add = true;
                            else if (PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString())
                            {
                                if (PaymentMethodList.Visible)
                                {
                                    //Response.Write("PaymentMethodList.SelectedValue " + PaymentMethodList.SelectedValue);
                                    //Response.Write("<br/>1memberAccountData.DraftType " + memberAccountData.DraftType);
                                    //Response.Write("<br/>2CanadianBankConfig " + CanadianBankConfig);
                                    //Response.Write("<br/>3memberAccountData.ACHInformation.IsCanadianBank " + memberAccountData.ACHInformation.IsCanadianBank);

                                    if (PaymentMethodList.SelectedValue == PaymentMethodOptions.BankAccount.ToString() && ((memberAccountData.DraftType == DraftTypeData.USBankDraft && !CanadianBankConfig && (memberAccountData.ACHInformation.IsCanadianBank == null || (memberAccountData.ACHInformation.IsCanadianBank != null && !(bool)memberAccountData.ACHInformation.IsCanadianBank))) || (memberAccountData.DraftType == DraftTypeData.CanadianBank && CanadianBankConfig && (bool)memberAccountData.ACHInformation.IsCanadianBank)))
                                        add = true;
                                    else if (PaymentMethodList.SelectedValue == PaymentMethodOptions.CreditCard.ToString() && memberAccountData.DraftType == DraftTypeData.CreditorDebitCard && CreditCardTypeList.Items.FindByValue(memberAccountData.CreditOrDebitCardInformation.CardType) != null)
                                        add = true;
                                }
                            }
                            //Response.Write("<br/>add "+add);
                            if (add)
                            {
                                exists = true;
                                RadioButton account = new RadioButton();
                                //Response.Write("<br/>memberAccountData.MemberAccountId " + memberAccountData.MemberAccountId);
                                account.ID = memberAccountData.MemberAccountId.ToString();
                                //Response.Write("<br/>memberAccountData.DraftType " + memberAccountData.DraftType);

                                if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                {
                                    //Retreiving account description for the particular ount code
                                    string cardTypeDescription = "";
                                    QueryData cashAccountsQuery = new QueryData("Cash_Accounts");
                                    cashAccountsQuery.AddCriteria(new CriteriaData("CASH_ACCOUNT_CODE", OperationData.Equal, memberAccountData.CreditOrDebitCardInformation.CardType));
                                    FindResultsData cashAccountsResults = EntityManager.Find(cashAccountsQuery);
                                    if (cashAccountsResults != null && cashAccountsResults.Result.Count > 0)
                                    {
                                        foreach (GenericEntityData cashAccount in cashAccountsResults.Result)
                                        {
                                            cardTypeDescription = cashAccount["DESCRIPTION"].ToString();
                                        }
                                    }
                                    if (memberAccountData.AccountName.Contains("..."))
                                        account.Text = memberAccountData.AccountName;
                                    else
                                        account.Text = cardTypeDescription + " (..." + memberAccountData.CreditOrDebitCardInformation.LastFour + ")";
                                }
                                else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                    if (memberAccountData.AccountName.Contains("..."))
                                        account.Text = memberAccountData.AccountName;
                                    else
                                        account.Text = memberAccountData.ACHInformation.BankName + " (..." + memberAccountData.ACHInformation.LastFour + ")";

                                account.Width = 250;
                                account.GroupName = "ExistingOrNewAccount";
                                account.AutoPostBack = true;
                                account.CheckedChanged += new EventHandler(Checked_Changed);

                                if (!OnlyUpdateAutoDraftAccountConfig && !IsPostBack)
                                {
                                    string aCHAccountID = ACHAccountID;
                                    if (!String.IsNullOrEmpty(aCHAccountID))
                                    {
                                        if (aCHAccountID == memberAccountData.MemberAccountId.ToString())
                                        {
                                            account.Checked = true;
                                            NewAccount.Checked = false;
                                            Checked_Changed(account, new EventArgs());
                                        }
                                    }
                                }

                                ExistingAccountsPanel.Controls.Add(account);

                                if (!HideDeleteButton)
                                {
                                    Button removeButton = new Button();
                                    removeButton.ID = "Delete-" + memberAccountData.MemberAccountId.ToString();
                                    removeButton.Text = "Delete";
                                    removeButton.Enabled = false;
                                    removeButton.ValidationGroup = "DeleteGroup";
                                    removeButton.CssClass = "TextButton";
                                    if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                        removeButton.OnClientClick = "return confirm('This Credit/Debit Card will be deleted!');";
                                    else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                        removeButton.OnClientClick = "return confirm('This account will be deleted!');";
                                    removeButton.Click += new EventHandler(RemoveAccount);

                                    ExistingAccountsPanel.Controls.Add(removeButton);
                                }
                                ExistingAccountsPanel.Controls.Add(new LiteralControl("<br/>"));
                            }
                        }
                    }
                }
                else
                {
                    //Response.Write("<br/>no accounts ");
                    ExistingAccountFirstPanel.Visible = false;
                    //MP 11/15/2016 Hiding no update label
                    //if (OnlyUpdateAutoDraftAccountConfig)
                    //{
                    //    //Response.Write("<br/>no accounts ");
                    //    NoUpdateLabel.Visible = true;
                    //}
                    //MP commented 09/22/2016
                    //else
                    //    NewAccount.Visible = false;
                }


                //MP 09/22/2016 Below code hides the PaymentContentPanel if OnlyUpdateAutoDraftAccountConfig is true
                //Unhiding it to make the New account option display even on the Profile page
                //if (OnlyUpdateAutoDraftAccountConfig)
                //{
                //    Response.Write("<br/>exists " + exists);
                //    if (!exists)
                //    {
                //        Response.Write("<br/> PaymentContentPanel.Visible = false");
                //        PaymentContentPanel.Visible = false;
                //    }
                //    else
                //    {
                //        Response.Write("<br/> PaymentContentPanel.Visible = true");

                //        if (PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString())
                //        {
                //            Response.Write("<br/>both");
                //            PaymentMethodPanel.Visible = true;
                //        }
                //        else
                //        {
                //            PaymentMethodPanel.Visible = false;
                //        }
                //        PaymentContentPanel.Visible = true;
                //    }
                //}
                //MP 09/22/2016
            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
        }


        /// <summary>
        /// Credit card or Bank Account panels are loaded based upon the selection
        /// </summary>
        /// <returns></returns>
        protected void PaymentMethodSelected_Change(object sender, EventArgs e)
        {
            try
            {
                ResetFields();

                if (AuthorizationCheckBoxConfig)
                {
                    AuthorizationCheckBox.Checked = false;
                    MembershipInstallmentCheckBox.Checked = false;
                    MembershipAutoPayCheckBox.Checked = false;
                    DonationCheckBox.Checked = false;

                    AuthorizationCheckBox.Visible = false;
                    MembershipInstallmentCheckBox.Visible = false;
                    MembershipAutoPayCheckBox.Visible = false;
                    DonationCheckBox.Visible = false;

                    SaveButton.Visible = false;
                }

                if (PaymentMethodList.SelectedValue == PaymentMethodOptions.BankAccount.ToString())
                {
                    AccountInformationFieldsPanel.Visible = true;
                    if (HideSavingsAccount)
                        SavingRadioButton.Visible = false;
                    //MP 11/14/2016 If config is checked, show check panel
                    if (ShowCheckImage)
                        CheckImagePanel.Visible = true;
                    CardInformationFieldsPanel.Visible = false;
                    CardsAcceptedPanel.Visible = false;
                    BillingAddressPanel.Visible = false;
                    if (CanadianBankConfig)
                        CanadianBankPanel.Visible = true;
                    else
                        CanadianBankPanel.Visible = false;
                    //MP - 08/19/2016 Show Authorization checkbox when payment method is changed
                    AuthorizationCheckBox.Visible = true;

                    Session["PaymentMethod"] = PaymentMethodOptions.BankAccount.ToString();
                    //MP 11/08/2016 setting session variable for Non immediate ACh

                }
                else if (PaymentMethodList.SelectedValue == PaymentMethodOptions.CreditCard.ToString())
                {
                    CardInformationFieldsPanel.Visible = true;
                    AccountInformationFieldsPanel.Visible = false;
                    CardsAcceptedPanel.Visible = true;
                    CanadianBankPanel.Visible = false;
                    if (!OnlyUpdateAutoDraftAccountConfig)
                        BillingAddressPanel.Visible = true;
                    else
                        BillingAddressPanel.Visible = false;
                    if (BillingAddressPanel.Visible)
                        AddressPurpose_Changed(AddressPurposeList, e);
                    //MP - 08/19/2016 Show Authorization checkbox when payment method is changed
                    AuthorizationCheckBox.Visible = true;
                    //MP 11/08/2016 setting session variable for Non immediate ACh
                    Session["PaymentMethod"] = PaymentMethodOptions.CreditCard.ToString();

                }

                foreach (Control control in ExistingAccountsPanel.Controls)
                {
                    if (control is RadioButton)
                    {
                        RadioButton button = (RadioButton)control;

                        button.Checked = false;

                        var query = new QueryData("MemberAccount");
                        query.AddCriteria(CriteriaData.Equal("PartyId", Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                        query.AddCriteria(CriteriaData.Equal("MemberAccountId", button.ID));

                        FindResultsData memberAccountResults = EntityManager.Find(query);

                        if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                        {
                            MemberAccountData memberAccountData = (MemberAccountData)memberAccountResults.Result[0];
                            //MP 08/16/2016
                            if (memberAccountData.AccountName.Contains("..."))
                                button.Text = memberAccountData.AccountName;
                            else
                                if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                    button.Text = memberAccountData.AccountName + " (..." + memberAccountData.CreditOrDebitCardInformation.LastFour + ")";
                                else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                    button.Text = memberAccountData.AccountName + " (..." + memberAccountData.ACHInformation.LastFour + ")";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Credit/debit card information is loaded if existing card is selected else resets all the fields
        /// </summary>
        /// <returns></returns>
        protected void Checked_Changed(object sender, EventArgs e)
        {
            //Response.Write("inside checked changed");
            try
            {
                if (AuthorizationCheckBoxConfig)
                {
                    AuthorizationCheckBox.Checked = false;
                    MembershipInstallmentCheckBox.Checked = false;
                    MembershipAutoPayCheckBox.Checked = false;
                    DonationCheckBox.Checked = false;

                    AuthorizationCheckBox.Visible = false;
                    MembershipInstallmentCheckBox.Visible = false;
                    MembershipAutoPayCheckBox.Visible = false;
                    DonationCheckBox.Visible = false;

                    SaveButton.Visible = false;
                }

                if (NewAccount.Checked && NewAccount.Visible)
                {
                    ResetFields();
                    AddressPurpose_Changed(AddressPurposeList, e);
                    //Response.Write("<br/>inside checked changed basket " + CheckBasketForAutoDraftProducts());
                    if (CheckBasketForAutoDraftProducts())
                        Response.Write("");//AccountNamePanel.Visible = true; MP - 06/20/2016
                    else
                        AccountNamePanel.Visible = false;

                    if (PaymentMethodAllowedConfig == PaymentMethodOptions.BankAccount.ToString() || (PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString() && PaymentMethodList.SelectedValue == PaymentMethodOptions.BankAccount.ToString()))
                    {
                        AccountInformationFieldsPanel.Visible = true;
                        if (HideSavingsAccount)
                            SavingRadioButton.Visible = false;
                        //MP 11/14/2016 If config is checked, show check panel
                        if (ShowCheckImage)
                            CheckImagePanel.Visible = true;
                        CardInformationFieldsPanel.Visible = false;
                        CardsAcceptedPanel.Visible = false;
                        //MP 06/27/2016 No billing address for adding a new account - added ac check
                        if (OnlyUpdateAutoDraftAccountConfig)
                            BillingAddressPanel.Visible = false;
                        if (CanadianBankConfig)
                            CanadianBankPanel.Visible = true;
                        else
                            CanadianBankPanel.Visible = false;
                    }
                    else if (PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString() || (PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString() && PaymentMethodList.SelectedValue == PaymentMethodOptions.CreditCard.ToString()))
                    {
                        CardInformationFieldsPanel.Visible = true;
                        AccountInformationFieldsPanel.Visible = false;
                        CardsAcceptedPanel.Visible = true;
                        //MP 06/27/2016 No billing address for adding a new account - addition a check
                        if (!OnlyUpdateAutoDraftAccountConfig)
                            BillingAddressPanel.Visible = true;
                        CanadianBankPanel.Visible = false;
                    }
                    //MP 06/27/2016 - added to show checkbox when new account is selected
                    if (AuthorizationCheckBoxConfig && AuthorizationType == AuthorizationOptions.Multiple.ToString())
                    {
                        //Response.Write("<br/> 111");

                        //if (membershipAutoPayProduct)
                        //    MembershipAutoPayCheckBox.Visible = true;
                        //if (membershipProduct)
                        //    MembershipInstallmentCheckBox.Visible = true;
                        //if (donationProduct)
                        //    DonationCheckBox.Visible = true;

                    }

                    else if (AuthorizationCheckBoxConfig && AuthorizationType == AuthorizationOptions.Single.ToString())
                    {
                        //Response.Write("<br/> 222 ");
                        AuthorizationCheckBox.Visible = true;
                        MembershipInstallmentCheckBox.Visible = false;
                        MembershipAutoPayCheckBox.Visible = false;
                        DonationCheckBox.Visible = false;
                    }
                    else
                    {
                        //Response.Write("<br/> 333 ");

                        AuthorizationCheckBox.Visible = false;
                        MembershipInstallmentCheckBox.Visible = false;
                        MembershipAutoPayCheckBox.Visible = false;
                        DonationCheckBox.Visible = false;
                    }
                    //MP
                }
                else
                {
                    //Response.Write("<br/> load existing accounts");
                    //Response.Write(" 4"); //AccountNamePanel.Visible = true; MP 06/20/2016
                    string existingAccountId = ((RadioButton)sender).ID;
                    var query = new QueryData("MemberAccount");
                    //MP 09/26/2016 Replaced GetSelectedImisId with ImisId
                    query.AddCriteria(CriteriaData.Equal("PartyId", ImisId));
                    //query.AddCriteria(CriteriaData.Equal("PartyId", Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                    query.AddCriteria(CriteriaData.Equal("MemberAccountId", existingAccountId));

                    FindResultsData memberAccountResults = EntityManager.Find(query);
                    //Response.Write("<br/> memberAccountResults.Result.Count " + memberAccountResults.Result.Count);
                    if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                    {
                        MemberAccountData memberAccountData = memberAccountResults.Result[0] as MemberAccountData;
                        //Response.Write("<br/> memberAccountData.DraftType " + memberAccountData.DraftType);
                        if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                        {
                            AccountInformationFieldsPanel.Visible = false;
                            CardInformationFieldsPanel.Visible = true;
                            CardsAcceptedPanel.Visible = true;
                            CanadianBankPanel.Visible = false;
                            if (!OnlyUpdateAutoDraftAccountConfig)
                                BillingAddressPanel.Visible = true;
                            //Response.Write("<br/>HolderName " + memberAccountData.CreditOrDebitCardInformation.HolderName);
                            CreditCardNameTextBox.Text = memberAccountData.CreditOrDebitCardInformation.HolderName;
                            CreditCardTypeList.SelectedValue = memberAccountData.CreditOrDebitCardInformation.CardType;
                            CreditCardCashAccount_Changed(sender, e);
                            CreditCardNumTextBox.Text = "************" + memberAccountData.CreditOrDebitCardInformation.LastFour;
                            if (((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).ToString("MM/yy").Contains("/"))
                            {
                                CreditCardExpirationList.SelectedValue = ((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).ToString("MM/yy").Split('/')[0];

                                if (CreditCardExpirationYearList.Items.FindByValue(((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).Year.ToString()) != null)
                                    CreditCardExpirationYearList.SelectedValue = ((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).Year.ToString();
                            }
                            CreditCardSecurityCodeTextBox.Text = String.Empty;
                            if (!String.IsNullOrEmpty(memberAccountData.CreditOrDebitCardInformation.BillingAddressPurpose))
                            {
                                AddressPurposeList.SelectedValue = memberAccountData.CreditOrDebitCardInformation.BillingAddressPurpose;
                                AddressPurpose_Changed(AddressPurposeList, e);
                                AddressPurposeHiddenField.Value = memberAccountData.CreditOrDebitCardInformation.BillingAddressPurpose;
                            }

                            CreditCardNameHiddenField.Value = memberAccountData.CreditOrDebitCardInformation.HolderName;
                            CreditCardTypeHiddenField.Value = memberAccountData.CreditOrDebitCardInformation.CardType;
                            CreditCardNumHiddenField.Value = memberAccountData.CreditOrDebitCardInformation.CardNumber;

                            if (((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).ToString("MM/yy").Contains("/"))
                            {
                                CreditCardExpirationHiddenField.Value = ((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).ToString("MM/yy").Split('/')[0];

                                if (CreditCardExpirationYearList.Items.FindByValue(((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).Year.ToString()) != null)
                                    CreditCardExpirationYearHiddenField.Value = ((YearMonthDateData)memberAccountData.CreditOrDebitCardInformation.Expiration).Year.ToString();
                            }
                        }

                        if (memberAccountData.DraftType == DraftTypeData.USBankDraft || memberAccountData.DraftType == DraftTypeData.CanadianBank)
                        {
                            AccountInformationFieldsPanel.Visible = true;
                            if (HideSavingsAccount)
                                SavingRadioButton.Visible = false;
                            CardInformationFieldsPanel.Visible = false;
                            CardsAcceptedPanel.Visible = false;
                            BillingAddressPanel.Visible = false;
                            if (CanadianBankConfig && (bool)memberAccountData.ACHInformation.IsCanadianBank)
                            {
                                CanadianBankPanel.Visible = true;
                                CanadianBankNumberTextBox.Text = memberAccountData.ACHInformation.CanadianBankNumber;
                                CanadianBankNumberHiddenField.Value = memberAccountData.ACHInformation.CanadianBankNumber;
                            }
                            else
                                CanadianBankPanel.Visible = false;

                            AccountNameTextBox.Text = memberAccountData.ACHInformation.AccountOwnerName;
                            AccountNumberTextBox.Text = "************" + memberAccountData.ACHInformation.LastFour;
                            RoutingNumberTextBox.Text = memberAccountData.ACHInformation.RoutingNumber;
                            BankNameTextBox.Text = memberAccountData.ACHInformation.BankName;
                            if (memberAccountData.ACHInformation.IsChecking != null)
                            {
                                if ((bool)memberAccountData.ACHInformation.IsChecking)
                                    CheckingRadioButton.Checked = true;
                                else
                                    SavingRadioButton.Checked = true;
                            }

                            AccountNameHiddenField.Value = memberAccountData.ACHInformation.AccountOwnerName;
                            AccountNumberHiddenField.Value = memberAccountData.ACHInformation.AccountNumber;
                            RoutingNumberHiddenField.Value = memberAccountData.ACHInformation.RoutingNumber;
                            BankNameHiddenField.Value = memberAccountData.ACHInformation.BankName;
                            IsCheckingHiddenField.Value = memberAccountData.ACHInformation.IsChecking.ToString();
                        }
                        //Mp 06/20/2016 hiding nickname field
                        //NicknameCardTextBox.Text = memberAccountData.AccountName;              
                        NicknameCardHiddenField.Value = memberAccountData.AccountName;

                        QueryData draftItemQuery = new QueryData("DraftItem");
                        draftItemQuery.AddCriteria(CriteriaData.Equal("MemberAccountId", memberAccountData.MemberAccountId.ToString()));

                        FindResultsData draftItemResults = EntityManager.Find(draftItemQuery);
                        bool memberAccountInUse = false;
                        //Response.Write("<br/>draftItemResults.Result.Count " + draftItemResults.Result.Count);
                        if (draftItemResults.Result != null && draftItemResults.Result.Count > 0)
                        {
                            memberAccountInUse = true;
                            bool donationProduct = false;
                            bool membershipProduct = false;
                            bool membershipAutoPayProduct = false;

                            foreach (DraftItemData draftItemData in draftItemResults.Result)
                            {
                                if (draftItemData.ProductType != "GIFT" && draftItemData.ProductType != "REQUEST" && draftItemData.ProductType != "PLEDGE")
                                {
                                    if (draftItemData.DraftItemBillingInformation.Frequency == 12)
                                        membershipAutoPayProduct = true;
                                    else
                                        membershipProduct = true;
                                }
                                else
                                {
                                    donationProduct = true;
                                }
                            }
                            //Response.Write("authorizationcheckbox " + AuthorizationCheckBoxConfig);
                            if (AuthorizationCheckBoxConfig && AuthorizationType == AuthorizationOptions.Multiple.ToString())
                            {
                                if (membershipAutoPayProduct)
                                    MembershipAutoPayCheckBox.Visible = true;
                                if (membershipProduct)
                                    MembershipInstallmentCheckBox.Visible = true;
                                if (donationProduct)
                                    DonationCheckBox.Visible = true;

                            }

                            else if (AuthorizationCheckBoxConfig && AuthorizationType == AuthorizationOptions.Single.ToString())
                            {
                                //Response.Write("<br/> AuthorizationCheckBox.Visible = true ");
                                AuthorizationCheckBox.Visible = true;
                                MembershipInstallmentCheckBox.Visible = false;
                                MembershipAutoPayCheckBox.Visible = false;
                                DonationCheckBox.Visible = false;
                            }
                            else
                            {
                                AuthorizationCheckBox.Visible = false;
                                MembershipInstallmentCheckBox.Visible = false;
                                MembershipAutoPayCheckBox.Visible = false;
                                DonationCheckBox.Visible = false;
                            }
                        }
                        else
                        {
                            //Response.Write("<br/> else checkbox visible");
                            AuthorizationCheckBox.Visible = true;
                        }

                        foreach (Control control in ExistingAccountsPanel.Controls)
                        {
                            if (control is Button)
                            {
                                Button button = (Button)control;
                                if (button.ID == "Delete-" + existingAccountId && !memberAccountInUse)
                                    button.Enabled = true;
                                else
                                    button.Enabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        protected void AuthorizationCheck_Changed(Object sender, EventArgs e)
        {
            try
            {
                bool allChecked = true;
                foreach (Control c in CheckBoxPanel.Controls)
                {
                    if (c is CheckBox && !((CheckBox)c).Checked && c.Visible)
                    {
                        allChecked = false;
                    }
                }
                if (AuthorizationCheckBoxConfig && allChecked)
                {
                    //Response.Write("<br/>show save " + SaveButton.Text);
                    SaveButton.Visible = true;
                }
                else if (AuthorizationCheckBoxConfig && !allChecked)
                {
                    //Response.Write("<br/>hide save " + SaveButton.Text);

                    SaveButton.Visible = false;
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        protected bool CheckComplimentary()
        {
            bool notComplimentary = false;
            try
            {
                EntityManager entityManager = new EntityManager(Asi.AppContext.CurrentIdentity.UserId);
                var cartManager = new CartManager(entityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

                foreach (OrderLineData line in cartManager.Cart.ComboOrder.Order.Lines)
                {
                    if (line.UnitPrice.Value.Amount > 0)
                        notComplimentary = true;
                }

                if (cartManager.Cart.ComboOrder.Invoices.Count > 0)
                    notComplimentary = true;

            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
            return notComplimentary;
        }

        /// <summary>
        /// Deletes the account from AutoDraft
        /// </summary>
        /// <returns></returns>
        protected void RemoveAccount(object sender, EventArgs e)
        {
            try
            {
                Button deleteButton = (Button)sender;

                string memberAccountId = deleteButton.ID.Split('-')[1];

                MemberAccountData memberAccountData = new MemberAccountData();
                memberAccountData.MemberAccountId = Convert.ToInt32(memberAccountId);
                EntityManager.Delete(memberAccountData);

                if (!OnlyUpdateAutoDraftAccountConfig)
                {
                    Session.Remove("OADPaymentDetails.UseMemberAccountID");

                    if (memberAccountData.DraftType == DraftTypeData.USBankDraft || memberAccountData.DraftType == DraftTypeData.CanadianBank)
                        UpdateACHAccountToEnrollmentTable(String.Empty);
                }
                else
                    Session["UpdatePaymentMethodList"] = true;



                ExistingAccountsPanel.FindControl(memberAccountId).Visible = false;
                ExistingAccountsPanel.FindControl("Delete-" + memberAccountId).Visible = false;
                ResetFields();
                ExistingAccountsPanel.Controls.Clear();
                if (NewAccount.Visible)
                    NewAccount.Checked = true;
                DisplayExistingCards(ImisId);
                if (SaveToDatabaseConfig)
                {
                    DataParameter[] dataParameterArray = new DataParameter[5];
                    DataParameter dataParameter = new DataParameter("@id", SqlDbType.VarChar);
                    dataParameter.Value = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                    dataParameterArray[0] = dataParameter;

                    DataParameter dataParameter1 = new DataParameter("@logType", SqlDbType.VarChar);
                    dataParameter1.Value = "CHANGE";
                    dataParameterArray[1] = dataParameter1;

                    DataParameter dataParameter2 = new DataParameter("@subType", SqlDbType.VarChar);
                    dataParameter2.Value = "CHANGE";
                    dataParameterArray[2] = dataParameter2;

                    DataParameter dataParameter3 = new DataParameter("@userId", SqlDbType.VarChar);
                    dataParameter3.Value = EntityManager.UserName;
                    dataParameterArray[3] = dataParameter3;

                    DataParameter dataParameter4 = new DataParameter("@logText", SqlDbType.VarChar);
                    dataParameter4.Value = "AutoDraft: Payment Method record " + memberAccountId + "(BDR_Member_Account.Member_Account_ID): Deleted";
                    dataParameterArray[4] = dataParameter4;

                    Utility.RunStoredProcedure("sp_es_LogAutoDraftChange", dataParameterArray);
                }
            }
            catch (FaultException error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Deletes the TentativeT accounts from AutoDraft
        /// </summary>
        /// <returns></returns>
        protected void RemoveTentativeAccount()
        {
            try
            {
                if (!OnlyUpdateAutoDraftAccountConfig && UseAutoDraftConfig)
                {
                    string accountId = String.Empty;
                    foreach (Control control in ExistingAccountsPanel.Controls)
                    {
                        if (control is RadioButton)
                        {
                            RadioButton button = (RadioButton)control;
                            if (button.Checked && button.Visible)
                            {
                                accountId = button.ID;
                            }
                        }
                    }
                    if (Session["OADPaymentDetails.UseMemberAccountID"] != null && Session["OADPaymentDetails.UseMemberAccountID"].ToString() == accountId)
                    {
                        MemberAccountData memberAccountData = new MemberAccountData();
                        memberAccountData.AccountName = NicknameCardHiddenField.Value;

                        if (CardInformationFieldsPanel.Visible)
                        {
                            memberAccountData.DraftType = DraftTypeData.CreditorDebitCard;
                            memberAccountData.CreditOrDebitCardInformation = new CreditOrDebitCardInformationData();
                            memberAccountData.CreditOrDebitCardInformation.BillingAddressPurpose = AddressPurposeHiddenField.Value;
                            memberAccountData.CreditOrDebitCardInformation.CardNumber = CreditCardNumHiddenField.Value;
                            memberAccountData.CreditOrDebitCardInformation.CardType = CreditCardTypeHiddenField.Value;
                            int expirationYear = Thread.CurrentThread.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(CreditCardExpirationYearHiddenField.Value));
                            int expirationMonth = Convert.ToInt32(CreditCardExpirationHiddenField.Value);
                            memberAccountData.CreditOrDebitCardInformation.Expiration = new YearMonthDateData(expirationYear, expirationMonth);
                            memberAccountData.CreditOrDebitCardInformation.HolderName = CreditCardNameHiddenField.Value;
                        }
                        else if (AccountInformationFieldsPanel.Visible)
                        {
                            memberAccountData.DraftType = DraftTypeData.USBankDraft;
                            memberAccountData.ACHInformation = new ACHInformationData();
                            memberAccountData.ACHInformation.AccountNumber = AccountNumberHiddenField.Value;
                            //Response.Write("accc nm " + AccountNameHiddenField.Value);
                            memberAccountData.ACHInformation.AccountOwnerName = AccountNameHiddenField.Value;
                            memberAccountData.ACHInformation.BankName = BankNameHiddenField.Value;
                            memberAccountData.ACHInformation.RoutingNumber = RoutingNumberHiddenField.Value;
                            memberAccountData.ACHInformation.IsChecking = (IsCheckingHiddenField.Value == "true" ? true : (IsCheckingHiddenField.Value == "false" ? false : true));
                            if (CanadianBankPanel.Visible)
                            {
                                memberAccountData.ACHInformation.IsCanadianBank = true;
                                memberAccountData.ACHInformation.CanadianBankNumber = CanadianBankNumberHiddenField.Value;
                            }
                        }

                        memberAccountData.MemberAccountId = Convert.ToInt32(Session["OADPaymentDetails.UseMemberAccountID"]);
                        memberAccountData.AccountStatus = "ACT";

                        ValidateResultsData<MemberAccountData> results;
                        results = entityManager.Update(memberAccountData);

                        foreach (ValidationResultData vrd in results.ValidationResults.Errors)
                        {
                            ErrorLabel.Text += "<br/>" + vrd.Message;
                        }

                        if (results != null && results.IsValid)
                        {
                            RadioButton accountText = (RadioButton)ExistingAccountsPanel.FindControl(memberAccountData.MemberAccountId.ToString());
                            if (memberAccountData.AccountName.Contains("..."))
                                accountText.Text = memberAccountData.AccountName;
                            else
                                if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                    accountText.Text = memberAccountData.AccountName + " (..." + memberAccountData.CreditOrDebitCardInformation.CardNumber.Substring((memberAccountData.CreditOrDebitCardInformation.CardNumber.Length) - 4, 4) + ")";
                                else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                    accountText.Text = memberAccountData.AccountName + " (..." + memberAccountData.ACHInformation.AccountNumber.Substring((memberAccountData.ACHInformation.AccountNumber.Length) - 4, 4) + ")";

                            if (IsPostBack)
                                Session.Remove("OADPaymentDetails.UseMemberAccountID");
                        }
                    }
                    if ((Session["OADPaymentDetails.UseMemberAccountID"] != null && Session["OADPaymentDetails.UseMemberAccountID"].ToString() != accountId) || !IsPostBack)
                    {
                        var query = new QueryData("MemberAccount");
                        query.AddCriteria(CriteriaData.Equal("PartyId", Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                        query.AddCriteria(CriteriaData.Equal("AccountStatus", "TNT"));

                        FindResultsData memberAccountResults = EntityManager.Find(query);

                        if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                        {
                            foreach (MemberAccountData memberAccountData in memberAccountResults.Result)
                            {
                                if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                {
                                    entityManager.Delete(memberAccountData);
                                    if (ExistingAccountsPanel.FindControl(memberAccountData.MemberAccountId.ToString()) != null)
                                        ExistingAccountsPanel.FindControl(memberAccountData.MemberAccountId.ToString()).Visible = false;
                                    if (ExistingAccountsPanel.FindControl("Delete-" + memberAccountData.MemberAccountId.ToString()) != null)
                                        ExistingAccountsPanel.FindControl("Delete-" + memberAccountData.MemberAccountId.ToString()).Visible = false;
                                }
                            }
                        }
                        if (IsPostBack)
                            Session.Remove("OADPaymentDetails.UseMemberAccountID");
                    }
                }
            }
            catch (FaultException error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Makes the CSC/CVV2 required or not when card type is changed
        /// </summary>
        /// <returns></returns>
        protected void CreditCardCashAccount_Changed(object sender, EventArgs e)
        {
            try
            {
                QueryData cashAccountsQuery = new QueryData("Cash_Accounts");
                cashAccountsQuery.AddCriteria(new CriteriaData("CASH_ACCOUNT_CODE", OperationData.Equal, CreditCardTypeList.SelectedValue));
                FindResultsData cashAccountsResults = EntityManager.Find(cashAccountsQuery);
                if (cashAccountsResults != null && cashAccountsResults.Result.Count > 0)
                {
                    GenericEntityData cashAccount = cashAccountsResults.Result[0] as GenericEntityData;
                    if (cashAccount != null && (bool)cashAccount["CSC_REQUIRED_WEB"])
                    {
                        CreditCardSecurityRequiredFieldValidator.Visible = true;
                        CreditCardSecurityCodeRequiredLabel.Visible = true;
                        CreditCardSecurityCodeExtraSpace.Visible = false;
                    }
                    else
                    {
                        CreditCardSecurityRequiredFieldValidator.Visible = false;
                        CreditCardSecurityCodeRequiredLabel.Visible = false;
                        CreditCardSecurityCodeExtraSpace.Visible = true;
                    }
                }
                CreditCardNumTextBox.Focus();
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Loads the address based on the address purpose selected
        /// </summary>
        /// <returns></returns>
        protected void AddressPurpose_Changed(object sender, EventArgs e)
        {
            try
            {
                AddressData addressData = null;
                PartyData partData = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartyData;
                foreach (FullAddressData fullAddressData in partData.Addresses)
                {
                    if (fullAddressData.AddressPurpose == AddressPurposeList.SelectedValue)
                    {
                        addressData = fullAddressData.Address;
                    }
                }

                if (addressData != null && addressData.AddressLines != null)
                {
                    if (addressData.AddressLines != null)
                    {
                        for (int i = 0; i < addressData.AddressLines.Count; i++)
                        {
                            if (i == 0)
                                BillingAddressTextBox.Text = addressData.AddressLines[i];
                            if (i == 1)
                                BillingAddressTextBox1.Text = addressData.AddressLines[i];
                            if (i == 2)
                                BillingAddressTextBox2.Text = addressData.AddressLines[i];
                        }
                    }

                    CityTextBox.Text = addressData.CityName;
                    if (CountryPanel.Visible)
                    {
                        if (CountryList.Items.FindByValue(addressData.CountryCode) != null)
                            CountryList.SelectedValue = addressData.CountryCode;
                        Country_Changed(sender, e);
                    }
                    if (StateList.Items.FindByValue(addressData.CountrySubEntityCode) != null)
                    {
                        StateList.Visible = true;
                        StateTextBox.Visible = false;
                        StateList.SelectedValue = addressData.CountrySubEntityCode;
                    }
                    else
                    {
                        StateList.Visible = false;
                        StateTextBox.Visible = true;
                        StateTextBox.Text = addressData.CountrySubEntityName;
                    }

                    ZipTextBox.Text = addressData.PostalCode;
                }
                else
                {
                    BillingAddressTextBox.Text = String.Empty;
                    BillingAddressTextBox1.Text = String.Empty;
                    BillingAddressTextBox2.Text = String.Empty;
                    CityTextBox.Text = String.Empty;
                    StateList.SelectedValue = "default";
                    ZipTextBox.Text = String.Empty;
                }

                //QueryData addressQuery = new QueryData("Name_Address");
                //addressQuery.AddCriteria(new CriteriaData("PURPOSE", OperationData.Equal, AddressPurposeList.SelectedValue));
                //addressQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                //FindResultsData addressResults = EntityManager.Find(addressQuery);
                //if (addressResults != null && addressResults.Result != null && addressResults.Result.Count > 0)
                //{
                //    GenericEntityData address = addressResults.Result[0] as GenericEntityData;
                //    BillingAddressTextBox.Text = address["ADDRESS_1"].ToString();
                //    if(address["ADDRESS_2"] != null)
                //        BillingAddressTextBox1.Text = address["ADDRESS_2"].ToString();
                //    CityTextBox.Text = address["CITY"].ToString();
                //    if (StateList.Items.FindByValue(address["STATE_PROVINCE"].ToString()) != null)
                //        StateList.SelectedValue = address["STATE_PROVINCE"].ToString();
                //    ZipTextBox.Text = address["ZIP"].ToString();
                //}
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Loads state list when country is changed
        /// </summary>
        /// <returns></returns>
        protected void Country_Changed(object sender, EventArgs e)
        {
            try
            {
                StateList.Items.Clear();
                ListItem defaultItem = new ListItem();
                defaultItem.Text = "(NONE)";
                defaultItem.Value = "default";
                defaultItem.Selected = true;
                StateList.Items.Add(defaultItem);

                QueryData countrySubEntityQuery = new QueryData("StateProvinceRef");
                countrySubEntityQuery.AddCriteria(new CriteriaData("COUNTRYCODE", OperationData.Equal, CountryList.SelectedValue));

                FindResultsData countrySubEntityResults = EntityManager.Find(countrySubEntityQuery);
                if (countrySubEntityResults != null && countrySubEntityResults.Result.Count > 0)
                {
                    foreach (GenericEntityData countrySubEntity in countrySubEntityResults.Result)
                    {
                        ListItem item = new ListItem();
                        item.Text = countrySubEntity["Description"].ToString();
                        item.Value = countrySubEntity["Code"].ToString();
                        StateList.Items.Add(item);
                    }

                    StateTextBox.Visible = false;
                    StateList.Visible = true;
                    ZipRequiredLabel.Visible = true;
                    ZipValidator.Enabled = true;
                }
                else
                {
                    StateTextBox.Visible = true;
                    StateList.Visible = false;
                    ZipRequiredLabel.Visible = false;
                    ZipValidator.Enabled = false;
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Saves details to AutoDraft and passes the information to ASI's payment object
        /// </summary>f
        /// <returns></returns>
        protected void SaveDetails()
        {
            try
            {
                string imisId = ImisId;
                bool differentPayor = false;
                HashSet<string> invoiceIds = new HashSet<string>();
                //string shipToId = "";

                var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                var comboOrderManager = new ComboOrderManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

                if (cartManager.Cart.ComboOrder.Invoices.Count > 0)
                {
                    foreach (InvoiceSummaryData invoiceSummaryData in cartManager.Cart.ComboOrder.Invoices)
                    {
                        //Response.Write("SoldToParty.Id " + invoiceSummaryData.SoldToParty.Id + "loggedId " + Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                        if (invoiceSummaryData.SoldToParty.Id != Asi.Security.Utility.SecurityHelper.GetSelectedImisId())
                        {
                            differentPayor = true;
                            invoiceIds.Add(invoiceSummaryData.SoldToParty.Id);
                            //shipToId = invoiceSummaryData.SoldToParty.Id;

                        }
                    }
                }
                //Response.Write("shiptoid " + shipToId);
                //shipToId = "";
                //Response.Write("<br/>CardInformationFieldsPanel "+CardInformationFieldsPanel.Visible);
                //MP - 09/27/2016 setting session variable to be used on the OrderConf page
                //if (CompanyCardsConfig && IsCompanyAdmin && shipToId != null)
                if (CompanyCardsConfig && IsCompanyAdmin && !OnlyUpdateAutoDraftAccountConfig)
                {
                    //Session["OADPaymentDetails.CompanyCardsConfig"] = CompanyCardsConfig;
                    Session["OADPaymentDetails.CompanyCardsConfig"] = imisId;
                    //Response.Write("<br/><script>alert('a " + Session["OADPaymentDetails.CompanyCardsConfig"] + "');</script>");
                }
                //MP - 09/27/2016 
                if (CardInformationFieldsPanel.Visible)
                {
                    bool validExpirationDate = false;
                    //bool validADExpirationDate = false;

                    int expirationYear = Thread.CurrentThread.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(CreditCardExpirationYearList.SelectedValue));
                    int expirationMonth = Convert.ToInt32(CreditCardExpirationList.SelectedValue);
                    //DateTime expirationDate = new DateTime(expirationYear, expirationMonth, DateTime.Today.Day);
                    if (expirationYear > DateTime.Today.Year)
                        validExpirationDate = true;
                    else if (expirationYear == DateTime.Today.Year && expirationMonth >= DateTime.Today.Month)
                        validExpirationDate = true;

                    //DateTime lastPaymentDate = DateTime.Today;
                    //lastPaymentDate = new DateTime(lastPaymentDate.Year, lastPaymentDate.Month, 1);
                    //if (AutoDraftPeriod - AutoDraftFrequency != 0)
                    //    lastPaymentDate = lastPaymentDate.AddMonths(AutoDraftPeriod - AutoDraftFrequency);
                    //else
                    //    lastPaymentDate = lastPaymentDate.AddMonths(AutoDraftPeriod);

                    //if (expirationYear > lastPaymentDate.Year)
                    //    validADExpirationDate = true;
                    //else if (expirationYear == lastPaymentDate.Year && expirationMonth >= lastPaymentDate.Month)
                    //    validADExpirationDate = true;

                    //if (expirationDate > DateTime.Today)

                    //Response.Write("<br/>valid expiration date " + validExpirationDate);
                    if (validExpirationDate)
                    {
                        //if (!OnlyUpdateAutoDraftAccountConfig)
                        //{
                        //    FullAddressData newFullAddressData = new FullAddressData();
                        //    FullAddressData oldFullAddressData = new FullAddressData();
                        //    bool addressPurposeExists = false;
                        //    //MP 09/27/2016 Adding company cards ability
                        //    PartyData partyData = EntityManager.FindByIdentity(new IdentityData("Party", ImisId)) as PartyData;
                        //    //PartyData partyData = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartyData;
                        //    if (partyData != null)
                        //    {
                        //        if (partyData.Addresses != null)
                        //        {
                        //            foreach (FullAddressData fullAddressData1 in partyData.Addresses)
                        //            {
                        //                if (!addressPurposeExists)
                        //                {
                        //                    oldFullAddressData = fullAddressData1;

                        //                }    
                        //                if (fullAddressData1.AddressPurpose == AddressPurposeList.SelectedValue)
                        //                {
                        //                    newFullAddressData = fullAddressData1;
                        //                    addressPurposeExists = true;
                        //                }
                        //            }
                        //        }
                        //    }

                        //    if (!addressPurposeExists)
                        //    {
                        //        newFullAddressData.DisplayName = oldFullAddressData.DisplayName;
                        //        newFullAddressData.DisplayOrganizationName = oldFullAddressData.DisplayOrganizationName;
                        //        newFullAddressData.DisplayOrganizationTitle = oldFullAddressData.DisplayOrganizationTitle;
                        //        newFullAddressData.Email = oldFullAddressData.Email;
                        //        newFullAddressData.ExtensionData = oldFullAddressData.ExtensionData;
                        //        newFullAddressData.Fax = oldFullAddressData.Fax;
                        //        newFullAddressData.Phone = oldFullAddressData.Phone;
                        //        newFullAddressData.Salutation = oldFullAddressData.Salutation;
                        //    }

                        //    AddressData addressData = new AddressData();
                        //    AddressLineDataCollection billingAddressLines = new AddressLineDataCollection();
                        //    billingAddressLines.Add(BillingAddressTextBox.Text);
                        //    if (BillingAddressTextBox1.Text.Length > 0)
                        //        billingAddressLines.Add(BillingAddressTextBox1.Text);
                        //    if (BillingAddressTextBox2.Text.Length > 0)   
                        //        billingAddressLines.Add(BillingAddressTextBox2.Text);

                        //    addressData.AddressLines = billingAddressLines;
                        //    addressData.CityName = CityTextBox.Text;
                        //    if (StateList.Visible)
                        //    {
                        //        addressData.CountrySubEntityCode = (StateList.SelectedValue == "default" ? "" : StateList.SelectedValue);
                        //        addressData.CountrySubEntityName = (StateList.SelectedValue == "default" ? "" : StateList.SelectedItem.Text);
                        //    }
                        //    else if (StateTextBox.Visible)
                        //    {
                        //        addressData.CountrySubEntityName = StateTextBox.Text;
                        //    }
                        //    if (CountryPanel.Visible)
                        //    {
                        //        addressData.CountryCode = CountryList.SelectedValue;
                        //        addressData.CountryName = CountryList.SelectedItem.Text;
                        //    }
                        //    addressData.PostalCode = ZipTextBox.Text;

                        //    newFullAddressData.Address = addressData;
                        //    newFullAddressData.AddressPurpose = AddressPurposeList.SelectedValue;
                        //    partyData.Addresses.Add(newFullAddressData);
                        //    ValidateResultsData<PartyData> partyResults = EntityManager.Update(partyData);
                        //    foreach (ValidationResultData vrd in partyResults.ValidationResults.Errors)
                        //    {
                        //        ErrorLabel.Text += "<br/>" + vrd.Message;
                        //    }
                        //}

                        bool autoDraftProductsExists = false;
                        autoDraftProductsExists = CheckBasketForAutoDraftProducts();
                        //Response.Write("<br/>autoDraftProductsExists " + autoDraftProductsExists);
                        if (OnlyUpdateAutoDraftAccountConfig || (autoDraftProductsExists && UseAutoDraftConfig))
                        {
                            bool existingCardSelected = false;
                            DateTime createdTime = DateTime.Now;
                            ErrorLabel.Text = String.Empty;
                            //Response.Write("<br/>hell yeah");
                            MemberAccountData memberAccountData = new MemberAccountData();

                            if (differentPayor)
                                memberAccountData.IsParent = true;

                            memberAccountData.DraftType = DraftTypeData.CreditorDebitCard;
                            memberAccountData.CreditOrDebitCardInformation = new CreditOrDebitCardInformationData();
                            memberAccountData.CreditOrDebitCardInformation.BillingAddressPurpose = AddressPurposeList.SelectedValue;

                            if (CreditCardNumHiddenField.Value != String.Empty && CreditCardNumTextBox.Text.Contains("*") && (CreditCardNumHiddenField.Value.EndsWith(CreditCardNumTextBox.Text.Substring((CreditCardNumTextBox.Text.Length) - 4, 4)) || CreditCardExpirationHiddenField.Value.EndsWith("*")))
                            {
                                memberAccountData.CreditOrDebitCardInformation.CardNumber = CreditCardNumHiddenField.Value;
                            }
                            else
                            {
                                double n;
                                if (double.TryParse(CreditCardNumTextBox.Text, out n))
                                {
                                    memberAccountData.CreditOrDebitCardInformation.CardNumber = CreditCardNumTextBox.Text;
                                }
                            }

                            //commented below 06/20/2016 Setting account name to a default - card type + last 4 digits in CC 
                            //memberAccountData.AccountName = NicknameCardTextBox.Text;
                            //Response.Write("hii " + CreditCardTypeList.SelectedItem.Text + "-" + " sub-" + CreditCardNumTextBox.Text.Substring((CreditCardNumTextBox.Text.Length - 4), 4));

                            memberAccountData.AccountName = CreditCardTypeList.SelectedItem.Text + "(..." + CreditCardNumTextBox.Text.Substring((CreditCardNumTextBox.Text.Length - 4), 4) + ")";

                            memberAccountData.CreditOrDebitCardInformation.CardType = CreditCardTypeList.SelectedValue;
                            memberAccountData.CreditOrDebitCardInformation.Expiration = new YearMonthDateData(expirationYear, expirationMonth);
                            memberAccountData.CreditOrDebitCardInformation.HolderName = CreditCardNameTextBox.Text;

                            ValidateResultsData<MemberAccountData> results;
                            foreach (Control control in ExistingAccountsPanel.Controls)
                            {
                                if (control is RadioButton)
                                {
                                    RadioButton button = (RadioButton)control;
                                    if (button.Checked && button.Visible)
                                    {
                                        existingCardSelected = true;
                                        memberAccountData.MemberAccountId = Convert.ToInt32(button.ID);
                                        //Response.Write("<br/>memberAccountData.MemberAccountId " + memberAccountData.MemberAccountId);
                                        if (!OnlyUpdateAutoDraftAccountConfig)
                                            Session["OADPaymentDetails.UseMemberAccountID"] = Convert.ToInt32(button.ID);
                                    }
                                }
                            }

                            if (existingCardSelected && memberAccountData.MemberAccountId > 0)
                            {
                                //Response.Write("<br/> inside");
                                memberAccountData.AccountStatus = "ACT";
                                memberAccountData.AccountHoldInformation = new HoldInformationData();
                                memberAccountData.AccountHoldInformation.DeclinedCount = 0;
                                memberAccountData.AccountHoldInformation.HoldReasonCode = String.Empty;
                                memberAccountData.AccountHoldInformation.HoldReason = String.Empty;
                                memberAccountData.AccountHoldInformation.HoldStartDate = null;
                                memberAccountData.AccountHoldInformation.HoldEndDate = null;
                                memberAccountData.AccountHoldInformation.HoldNotified = false;
                                //Response.Write("<br/>memberAccountData " + memberAccountData);
                                results = entityManager.Update(memberAccountData);

                                QueryData draftItemQuery = new QueryData("DraftItem");
                                draftItemQuery.AddCriteria(CriteriaData.Equal("MemberAccountId", memberAccountData.MemberAccountId.ToString()));

                                FindResultsData draftItemResults = EntityManager.Find(draftItemQuery);

                                if (draftItemResults.Result != null && draftItemResults.Result.Count > 0)
                                {
                                    foreach (DraftItemData draftItemData in draftItemResults.Result)
                                    {
                                        if (draftItemData.LineItemStatus == "HOLD")
                                        {

                                            //Response.Write("<br/> Updating draft item");
                                            draftItemData.LineItemStatus = "ACT";
                                            draftItemData.ApplyHoldToAccount = false;
                                            draftItemData.LineItemHoldInformation = new HoldInformationData();
                                            draftItemData.LineItemHoldInformation.DeclinedCount = 0;
                                            draftItemData.LineItemHoldInformation.HoldReason = String.Empty;
                                            draftItemData.LineItemHoldInformation.HoldReasonCode = String.Empty;
                                            draftItemData.LineItemHoldInformation.HoldStartDate = null;
                                            draftItemData.LineItemHoldInformation.HoldEndDate = null;
                                            draftItemData.LineItemHoldInformation.HoldNotified = false;

                                            ValidateResultsData<DraftItemData> results1 = entityManager.Update(draftItemData);
                                            foreach (ValidationResultData vrd in results1.ValidationResults.Errors)
                                            {
                                                ErrorLabel.Text += "<br/>" + vrd.Message + vrd.Location;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!existingCardSelected && !OnlyUpdateAutoDraftAccountConfig)
                            {
                                //Response.Write("<br/>1elseif ");

                                memberAccountData.AccountStatus = "TNT";
                                memberAccountData.CreatedOn = createdTime;
                                memberAccountData.CreatedByParty = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;

                                memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", imisId)) as PartySummaryData;

                                results = entityManager.Add(memberAccountData);
                            }
                            //MP 06/28/2016 added - new account creation on update ipart
                            else if (NewAccount.Visible && NewAccount.Checked && OnlyUpdateAutoDraftAccountConfig)
                            {
                                //Response.Write("<br/>elseif " );

                                memberAccountData.AccountStatus = "ACT";
                                memberAccountData.CreatedOn = createdTime;
                                memberAccountData.CreatedByParty = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;

                                memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", imisId)) as PartySummaryData;


                                results = entityManager.Add(memberAccountData);

                                //setting UpdatePaymentMethodList session variable so that the payment method list dropdown in the existing drafts ipart updates
                                Session["UpdatePaymentMethodList"] = true;
                                //Response.Write("Session['UpdatePaymentMethodList']" + Session["UpdatePaymentMethodList"]);
                            }
                            //MP
                            else
                            {
                                ErrorLabel.Text += "<br/>Please Check your selection";
                                results = null;
                            }

                            foreach (ValidationResultData vrd in results.ValidationResults.Errors)
                            {
                                ErrorLabel.Text += "<br/>" + vrd.Message;
                            }

                            if (results != null && results.IsValid)
                            {
                                if (existingCardSelected && memberAccountData.MemberAccountId > 0)
                                {
                                    RadioButton cardText = (RadioButton)ExistingAccountsPanel.FindControl(memberAccountData.MemberAccountId.ToString());
                                    if (memberAccountData.AccountName.Contains("..."))
                                        cardText.Text = memberAccountData.AccountName;// +" (..." + memberAccountData.CreditOrDebitCardInformation.CardNumber.Substring((memberAccountData.CreditOrDebitCardInformation.CardNumber.Length) - 4, 4) + ")";
                                    else
                                        cardText.Text = memberAccountData.CreditOrDebitCardInformation.CardType + " (..." + memberAccountData.CreditOrDebitCardInformation.CardNumber.Substring((memberAccountData.CreditOrDebitCardInformation.CardNumber.Length) - 4, 4) + ")";
                                    if (OnlyUpdateAutoDraftAccountConfig)
                                    {
                                        AccountModifiedMessage.Visible = true;
                                        AccountModifiedMessage.Text = "Account update successful";
                                    }
                                }
                                if (!existingCardSelected && !OnlyUpdateAutoDraftAccountConfig)
                                {
                                    var query = new QueryData("MemberAccount");
                                    query.AddCriteria(CriteriaData.Equal("PartyId", imisId));
                                    query.AddCriteria(CriteriaData.GreaterThanOrEqual("CreatedOn", createdTime.ToString(CultureInfo.InvariantCulture)));

                                    FindResultsData memberAccountResults = EntityManager.Find(query);

                                    if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                                    {
                                        MemberAccountData newMemberAccountData = memberAccountResults.Result[0] as MemberAccountData;
                                        Session["OADPaymentDetails.UseMemberAccountID"] = newMemberAccountData.MemberAccountId;
                                    }
                                }
                                if (autoDraftProductsExists && !OnlyUpdateAutoDraftAccountConfig)
                                {
                                    //Response.Write("autoDraftProductsExists !OnlyUpdateAutoDraftAccountConfig");
                                    if (Session["OADPaymentDetails.UseMemberAccountID"] != null)
                                    {
                                        //Response.Write(" Session['OADPaymentDetails.UseMemberAccountID']");
                                        SavePaymentInfoToiMisObject();
                                    }
                                    else
                                        throw new Exception("Please provide credit/debit card details");
                                }
                                //MP - 08/19/2016 Issue #26 WHen new account is added add a new radio button and delete account button
                                if (NewAccount.Visible && NewAccount.Checked && OnlyUpdateAutoDraftAccountConfig && ExistingAccountConfig)
                                {
                                    RadioButton account = new RadioButton();
                                    var query = new QueryData("MemberAccount");
                                    query.AddCriteria(CriteriaData.Equal("PartyId", ImisId));
                                    query.AddCriteria(CriteriaData.GreaterThanOrEqual("CreatedOn", createdTime.ToString(CultureInfo.InvariantCulture)));

                                    FindResultsData memberAccountResults = EntityManager.Find(query);
                                    MemberAccountData newMemberAccountData = null;
                                    if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                                    {
                                        newMemberAccountData = memberAccountResults.Result[0] as MemberAccountData;
                                        account.ID = newMemberAccountData.MemberAccountId.ToString();

                                        //Response.Write("<br/>memberAccountData.DraftType " + memberAccountData.DraftType);

                                        if (newMemberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                            if (newMemberAccountData.AccountName.Contains("..."))
                                                account.Text = newMemberAccountData.AccountName;
                                            else
                                                account.Text = newMemberAccountData.CreditOrDebitCardInformation.CardType + " (..." + newMemberAccountData.CreditOrDebitCardInformation.LastFour + ")";
                                        else if (newMemberAccountData.DraftType != DraftTypeData.ECheck)
                                            if (newMemberAccountData.AccountName.Contains("..."))
                                                account.Text = newMemberAccountData.AccountName;
                                            else
                                                account.Text = newMemberAccountData.AccountName + " (..." + newMemberAccountData.ACHInformation.LastFour + ")";

                                        account.Width = 250;
                                        account.GroupName = "ExistingOrNewAccount";
                                        account.AutoPostBack = true;
                                        account.CheckedChanged += new EventHandler(Checked_Changed);
                                        ExistingAccountFirstPanel.Visible = true;

                                        ExistingAccountsPanel.Controls.Add(account);

                                        Button removeButton = new Button();
                                        removeButton.ID = "Delete-" + newMemberAccountData.MemberAccountId.ToString();
                                        removeButton.Text = "Delete";
                                        removeButton.Enabled = false;
                                        removeButton.ValidationGroup = "DeleteGroup";
                                        removeButton.CssClass = "TextButton";
                                        if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                            removeButton.OnClientClick = "return confirm('This Credit/Debit Card will be deleted!');";
                                        else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                            removeButton.OnClientClick = "return confirm('This account will be deleted!');";
                                        removeButton.Click += new EventHandler(RemoveAccount);

                                        ExistingAccountsPanel.Controls.Add(removeButton);
                                        ExistingAccountsPanel.Controls.Add(new LiteralControl("<br/>"));

                                        account.Checked = true;
                                        NewAccount.Checked = false;

                                        Checked_Changed(account, new EventArgs());
                                        //Response.Write("<script>alert('New Account Added!');</script>");
                                        AccountModifiedMessage.Visible = true;
                                        AccountModifiedMessage.Text = "New Account Added.";

                                    }
                                }
                                //MP - 08/19/2016 

                                if (OnlyUpdateAutoDraftAccountConfig)
                                {
                                    if (SaveToDatabaseConfig)
                                    {
                                        DataParameter[] dataParameterArray = new DataParameter[5];
                                        DataParameter dataParameter = new DataParameter("@id", SqlDbType.VarChar);
                                        dataParameter.Value = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                                        dataParameterArray[0] = dataParameter;

                                        DataParameter dataParameter1 = new DataParameter("@logType", SqlDbType.VarChar);
                                        dataParameter1.Value = "CHANGE";
                                        dataParameterArray[1] = dataParameter1;

                                        DataParameter dataParameter2 = new DataParameter("@subType", SqlDbType.VarChar);
                                        dataParameter2.Value = "CHANGE";
                                        dataParameterArray[2] = dataParameter2;

                                        DataParameter dataParameter3 = new DataParameter("@userId", SqlDbType.VarChar);
                                        dataParameter3.Value = EntityManager.UserName;
                                        dataParameterArray[3] = dataParameter3;

                                        DataParameter dataParameter4 = new DataParameter("@logText", SqlDbType.VarChar);
                                        dataParameter4.Value = "AutoDraft: Payment Method record (BDR_Member_Account.Member_Account_ID): " + memberAccountData.MemberAccountId + " - " + memberAccountData.AccountName + " is modified.";
                                        dataParameterArray[4] = dataParameter4;

                                        Utility.RunStoredProcedure("sp_es_LogAutoDraftChange", dataParameterArray);
                                    }
                                }
                            }
                        }
                        //else if (autoDraftProductsExists && UseAutoDraftConfig && !validADExpirationDate)
                        //{
                        //    throw new Exception("Please check the expiration date");
                        //}
                        else
                            SavePaymentInfoToiMisObject();
                    }
                    else
                    {
                        ErrorLabel.Text = "Please check the expiration date";
                        //throw new Exception("Please check the expiration date");
                    }
                }
                else if (AccountInformationFieldsPanel.Visible)// If it is ACH
                {
                    bool autoDraftProductsExists = false;

                    autoDraftProductsExists = CheckBasketForAutoDraftProducts();
                    if (OnlyUpdateAutoDraftAccountConfig || (autoDraftProductsExists && UseAutoDraftConfig))
                    {
                        bool existingAccountSelected = false;
                        DateTime createdTime = DateTime.Now;
                        ErrorLabel.Text = String.Empty;

                        //MP - 09/27/2016 setting session variable to be used on the OrderConf page
                        //if (CompanyCardsConfig && IsCompanyAdmin)
                        //{
                        //    //Session["OADPaymentDetails.CompanyCardsConfig"] = CompanyCardsConfig;
                        //    Session["OADPaymentDetails.CompanyCardsConfig"] = shipToId;
                        //    //Response.Write("<br/><script>alert('a " + Session["OADPaymentDetails.CompanyCardsConfig"] + "');</script>");
                        //}
                        //MP - 09/27/2016 

                        MemberAccountData memberAccountData = new MemberAccountData();
                        if (differentPayor)
                            memberAccountData.IsParent = true;

                        //MP 08/19/2016 Account name is set as bank Name(...last 4 digits of account number)
                        //memberAccountData.AccountName = NicknameCardTextBox.Text;
                        memberAccountData.AccountName = BankNameTextBox.Text + "(..." + AccountNumberTextBox.Text.Substring((AccountNumberTextBox.Text.Length - 4), 4) + ")";

                        memberAccountData.ACHInformation = new ACHInformationData();

                        if (CanadianBankConfig && CanadianBankPanel.Visible)
                        {
                            memberAccountData.DraftType = DraftTypeData.CanadianBank;
                            memberAccountData.ACHInformation.IsCanadianBank = true;
                            memberAccountData.ACHInformation.CanadianBankNumber = CanadianBankNumberTextBox.Text;
                        }
                        else
                        {
                            memberAccountData.DraftType = DraftTypeData.USBankDraft;
                            memberAccountData.ACHInformation.IsCanadianBank = false;
                        }

                        memberAccountData.ACHInformation.IsChecking = (CheckingRadioButton.Checked ? true : false);

                        if (AccountNumberHiddenField.Value != String.Empty && AccountNumberTextBox.Text.Contains("*") && AccountNumberHiddenField.Value.EndsWith(AccountNumberTextBox.Text.Substring((AccountNumberTextBox.Text.Length) - 4, 4)))
                        {
                            memberAccountData.ACHInformation.AccountNumber = AccountNumberHiddenField.Value;
                        }
                        else
                        {
                            double n;
                            if (double.TryParse(AccountNumberTextBox.Text, out n))
                            {
                                memberAccountData.ACHInformation.AccountNumber = AccountNumberTextBox.Text;
                            }
                        }
                        memberAccountData.ACHInformation.RoutingNumber = RoutingNumberTextBox.Text;
                        memberAccountData.ACHInformation.BankName = BankNameTextBox.Text;
                        //Response.Write("acc name" + AccountNameTextBox.Text);
                        memberAccountData.ACHInformation.AccountOwnerName = AccountNameTextBox.Text;

                        ValidateResultsData<MemberAccountData> results;
                        foreach (Control control in ExistingAccountsPanel.Controls)
                        {
                            if (control is RadioButton)
                            {
                                RadioButton button = (RadioButton)control;
                                if (button.Checked && button.Visible)
                                {
                                    existingAccountSelected = true;
                                    memberAccountData.MemberAccountId = Convert.ToInt32(button.ID);
                                    if (!OnlyUpdateAutoDraftAccountConfig)
                                    {
                                        Session["OADPaymentDetails.UseMemberAccountID"] = Convert.ToInt32(button.ID);
                                        if (FirstPaymentDateHiddenField.Value == "Today")
                                            UpdateACHAccountToEnrollmentTable(button.ID);
                                    }
                                }
                            }
                        }

                        if (existingAccountSelected && memberAccountData.MemberAccountId > 0)
                        {
                            memberAccountData.AccountStatus = "ACT";

                            memberAccountData.AccountHoldInformation = new HoldInformationData();
                            memberAccountData.AccountHoldInformation.DeclinedCount = 0;
                            memberAccountData.AccountHoldInformation.HoldReasonCode = String.Empty;
                            memberAccountData.AccountHoldInformation.HoldReason = String.Empty;
                            memberAccountData.AccountHoldInformation.HoldStartDate = null;
                            memberAccountData.AccountHoldInformation.HoldEndDate = null;
                            memberAccountData.AccountHoldInformation.HoldNotified = false;

                            results = entityManager.Update(memberAccountData);

                            QueryData draftItemQuery = new QueryData("DraftItem");
                            draftItemQuery.AddCriteria(CriteriaData.Equal("MemberAccountId", memberAccountData.MemberAccountId.ToString()));

                            FindResultsData draftItemResults = EntityManager.Find(draftItemQuery);

                            if (draftItemResults.Result != null && draftItemResults.Result.Count > 0)
                            {
                                foreach (DraftItemData draftItemData in draftItemResults.Result)
                                {
                                    if (draftItemData.LineItemStatus == "HOLD")
                                    {
                                        draftItemData.LineItemStatus = "ACT";
                                        draftItemData.ApplyHoldToAccount = false;

                                        draftItemData.LineItemHoldInformation = new HoldInformationData();
                                        draftItemData.LineItemHoldInformation.DeclinedCount = 0;
                                        draftItemData.LineItemHoldInformation.HoldReason = String.Empty;
                                        draftItemData.LineItemHoldInformation.HoldReasonCode = String.Empty;
                                        draftItemData.LineItemHoldInformation.HoldStartDate = null;
                                        draftItemData.LineItemHoldInformation.HoldEndDate = null;
                                        draftItemData.LineItemHoldInformation.HoldNotified = false;

                                        ValidateResultsData<DraftItemData> results1 = entityManager.Update(draftItemData);
                                        foreach (ValidationResultData vrd in results1.ValidationResults.Errors)
                                        {
                                            ErrorLabel.Text += "<br/>" + vrd.Message;
                                        }
                                    }
                                }
                            }
                        }
                        else if (!existingAccountSelected && !OnlyUpdateAutoDraftAccountConfig)
                        {
                            memberAccountData.AccountStatus = "ACT";
                            memberAccountData.CreatedOn = createdTime;
                            memberAccountData.CreatedByParty = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;
                            //memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;

                            //MP 09/27/2016
                            //if (CompanyCardsConfig && IsCompanyAdmin)
                            //{
                            memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", imisId)) as PartySummaryData;
                            //memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", shipToId)) as PartySummaryData;
                            //  Session["OADPaymentDetails.CompanyCardsConfig"] = shipToId;
                            //}
                            //else
                            //    memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;
                            //MP 09/27/2016

                            results = entityManager.Add(memberAccountData);
                        }
                        //MP 08/19/2016 added - new account creation on update ipart
                        else if (NewAccount.Visible && NewAccount.Checked && OnlyUpdateAutoDraftAccountConfig)
                        {
                            //Response.Write("<br/>elseif ");

                            memberAccountData.AccountStatus = "ACT";
                            memberAccountData.CreatedOn = createdTime;
                            memberAccountData.CreatedByParty = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;
                            //memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;
                            //MP 09/27/2016
                            //if (CompanyCardsConfig && IsCompanyAdmin)
                            //{
                            memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", imisId)) as PartySummaryData;
                            //Session["OADPaymentDetails.CompanyCardsConfig"] = CompanyCardsConfig;
                            //Session["OADPaymentDetails.CompanyCardsConfig"] = shipToId;
                            //Response.Write("<br/>assign " + Session["OADPaymentDetails.CompanyCardsConfig"]);
                            //}
                            //else
                            //{
                            //Response.Write("else");                             
                            //memberAccountData.Party = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartySummaryData;
                            //}
                            //MP 09/27/2016
                            results = entityManager.Add(memberAccountData);
                        }
                        //MP 08/19/2016   
                        else
                        {
                            ErrorLabel.Text += "<br/>Please Check your selection";
                            results = null;
                        }

                        foreach (ValidationResultData vrd in results.ValidationResults.Errors)
                        {
                            ErrorLabel.Text += "<br/>" + vrd.Message;
                        }

                        if (results != null && results.IsValid)
                        {
                            if (existingAccountSelected && memberAccountData.MemberAccountId > 0)
                            {
                                RadioButton accountText = (RadioButton)ExistingAccountsPanel.FindControl(memberAccountData.MemberAccountId.ToString());
                                //Response.Write("<br/> memberAccountData.AccountName " + memberAccountData.AccountName);
                                if (memberAccountData.AccountName.Contains("..."))
                                    accountText.Text = memberAccountData.AccountName.ToString();
                                else
                                    accountText.Text = memberAccountData.ACHInformation.BankName + " (..." + memberAccountData.ACHInformation.AccountNumber.Substring((memberAccountData.ACHInformation.AccountNumber.Length) - 4, 4) + ")";
                                //Response.Write("<script>alert('Details updated!');</script>");
                                if(OnlyUpdateAutoDraftAccountConfig)
                                {
                                    AccountModifiedMessage.Visible = true;
                                    AccountModifiedMessage.Text = "Account update successful.";
                                }
                            }

                            if (!existingAccountSelected && !OnlyUpdateAutoDraftAccountConfig)
                            {
                                var query = new QueryData("MemberAccount");
                                query.AddCriteria(CriteriaData.Equal("PartyId", imisId));
                                query.AddCriteria(CriteriaData.GreaterThanOrEqual("CreatedOn", createdTime.ToString(CultureInfo.InvariantCulture)));
                                
                                FindResultsData memberAccountResults = EntityManager.Find(query);

                                if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                                {
                                    MemberAccountData newMemberAccountData = memberAccountResults.Result[0] as MemberAccountData;
                                    Session["OADPaymentDetails.UseMemberAccountID"] = newMemberAccountData.MemberAccountId;
                                    if (FirstPaymentDateHiddenField.Value == "Today")
                                        UpdateACHAccountToEnrollmentTable(newMemberAccountData.MemberAccountId.ToString());
                                }
                            }

                            if (autoDraftProductsExists && !OnlyUpdateAutoDraftAccountConfig)
                            {
                                if (Session["OADPaymentDetails.UseMemberAccountID"] == null)
                                    throw new Exception("Please provide account details");
                            }

                            //MP - 08/19/2016 Issue #26 WHen new account is added add a new radio button and delete account button
                            if (NewAccount.Visible && NewAccount.Checked && OnlyUpdateAutoDraftAccountConfig && ExistingAccountConfig)
                            {
                                ExistingAccountFirstPanel.Visible = true;
                                RadioButton account = new RadioButton();
                                var query = new QueryData("MemberAccount");
                                query.AddCriteria(CriteriaData.Equal("PartyId", ImisId));
                                query.AddCriteria(CriteriaData.GreaterThanOrEqual("CreatedOn", createdTime.ToString(CultureInfo.InvariantCulture)));

                                FindResultsData memberAccountResults = EntityManager.Find(query);
                                MemberAccountData newMemberAccountData = null;
                                if (memberAccountResults != null && memberAccountResults.Result != null && memberAccountResults.Result.Count > 0)
                                {
                                    newMemberAccountData = memberAccountResults.Result[0] as MemberAccountData;
                                    account.ID = newMemberAccountData.MemberAccountId.ToString();

                                    //Response.Write("<br/>memberAccountData.DraftType " + memberAccountData.DraftType);

                                    if (newMemberAccountData.DraftType == DraftTypeData.USBankDraft || newMemberAccountData.DraftType == DraftTypeData.CanadianBank)
                                        if (newMemberAccountData.AccountName.Contains("..."))
                                            account.Text = newMemberAccountData.AccountName;
                                        else
                                            account.Text = newMemberAccountData.ACHInformation.BankName + " (..." + newMemberAccountData.ACHInformation.LastFour + ")";

                                    account.Width = 250;
                                    account.GroupName = "ExistingOrNewAccount";
                                    account.AutoPostBack = true;
                                    account.CheckedChanged += new EventHandler(Checked_Changed);
                                    ExistingAccountFirstPanel.Visible = true;
                                    ExistingAccountsPanel.Controls.Add(account);

                                    Button removeButton = new Button();
                                    removeButton.ID = "Delete-" + newMemberAccountData.MemberAccountId.ToString();
                                    removeButton.Text = "Delete";
                                    removeButton.Enabled = false;
                                    removeButton.ValidationGroup = "DeleteGroup";
                                    removeButton.CssClass = "TextButton";
                                    if (memberAccountData.DraftType == DraftTypeData.CreditorDebitCard)
                                        removeButton.OnClientClick = "return confirm('This Credit/Debit Card will be deleted!');";
                                    else if (memberAccountData.DraftType != DraftTypeData.ECheck)
                                        removeButton.OnClientClick = "return confirm('This account will be deleted!');";
                                    removeButton.Click += new EventHandler(RemoveAccount);

                                    ExistingAccountsPanel.Controls.Add(removeButton);
                                    ExistingAccountsPanel.Controls.Add(new LiteralControl("<br/>"));
                                    account.Checked = true;
                                    NewAccount.Checked = false;
                                    Checked_Changed(account, new EventArgs());
                                    AccountModifiedMessage.Visible = true;
                                    AccountModifiedMessage.Text = "New Account Added.";
                                }
                            }
                            //MP - 08/19/2016 

                            if (OnlyUpdateAutoDraftAccountConfig)
                            {
                                if (SaveToDatabaseConfig)
                                {
                                    DataParameter[] dataParameterArray = new DataParameter[5];
                                    DataParameter dataParameter = new DataParameter("@id", SqlDbType.VarChar);
                                    dataParameter.Value = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                                    dataParameterArray[0] = dataParameter;

                                    DataParameter dataParameter1 = new DataParameter("@logType", SqlDbType.VarChar);
                                    dataParameter1.Value = "CHANGE";
                                    dataParameterArray[1] = dataParameter1;

                                    DataParameter dataParameter2 = new DataParameter("@subType", SqlDbType.VarChar);
                                    dataParameter2.Value = "CHANGE";
                                    dataParameterArray[2] = dataParameter2;

                                    DataParameter dataParameter3 = new DataParameter("@userId", SqlDbType.VarChar);
                                    dataParameter3.Value = EntityManager.UserName;
                                    dataParameterArray[3] = dataParameter3;

                                    DataParameter dataParameter4 = new DataParameter("@logText", SqlDbType.VarChar);
                                    dataParameter4.Value = "AutoDraft: Payment Method record (BDR_Member_Account.Member_Account_ID): " + memberAccountData.MemberAccountId + " - " + memberAccountData.AccountName + " is modified.";
                                    dataParameterArray[4] = dataParameter4;

                                    Utility.RunStoredProcedure("sp_es_LogAutoDraftChange", dataParameterArray);
                                }
                            }
                        }
                    }
                }
                else
                {
                    UpdateACHAccountToEnrollmentTable(String.Empty);
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text = error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Check ASI cart for AutoDRaft products
        /// </summary>
        /// <returns>boolean value</returns>
        protected bool CheckBasketForAutoDraftProducts()
        {
            bool autoDraftProduct = false;
            bool invoicesExist = false;

            Dictionary<string, decimal> allProductsCartList = new Dictionary<string, decimal>();
            HashSet<string> invoiceProducts = new HashSet<string>();
            HashSet<string> products = new HashSet<string>();
            Dictionary<string, decimal> enrollmentProducts = new Dictionary<string, decimal>();
            List<String> billingCycleProducts = new List<String>();
            string billingProduct = String.Empty;
            try
            {
                if (!String.IsNullOrEmpty(ACHAccountID) && PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString() && !OnlyUpdateAutoDraftAccountConfig)
                {

                }
                else
                {
                    var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                    var comboOrderManager = new ComboOrderManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                    HashSet<string> invoiceIds = new HashSet<string>();
                    if (!cartManager.CartIsEmpty)
                    {
                        QueryData emrollmentQuery = new QueryData("OAD_Enrollment_Tracker");
                        emrollmentQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                        FindResultsData enrollmentResults = EntityManager.Find(emrollmentQuery);

                        if (enrollmentResults != null && enrollmentResults.Result != null && enrollmentResults.Result.Count > 0)
                        {
                            foreach (GenericEntityData existingEnrollmentEntity in enrollmentResults.Result)
                            {
                                //  Response.Write("<br/> oad_enroll_prod " + existingEnrollmentEntity["PRODUCT_CODE"].ToString());
                                if (Convert.ToBoolean(existingEnrollmentEntity["OAD_ENROLLED"].ToString()))
                                {
                                    //Response.Write("<br/>autodraft prods " + existingEnrollmentEntity["PRODUCT_CODE"].ToString());
                                    enrollmentProducts.Add(existingEnrollmentEntity["PRODUCT_CODE"].ToString(), Convert.ToDecimal(existingEnrollmentEntity["PRICE"]));
                                }
                            }
                        }

                        foreach (OrderLineData line in cartManager.Cart.ComboOrder.Order.Lines)
                        {
                            if (!allProductsCartList.ContainsKey(line.Item.ItemCode))
                            {
                                allProductsCartList.Add(line.Item.ItemCode, Convert.ToDecimal(line.UnitPrice));
                                //Response.Write("line.Item.ItemCode " + line.Item.ItemCode);
                            }
                        }


                        if (cartManager.Cart.ComboOrder.Invoices.Count > 0)
                        {
                            invoicesExist = true;
                            foreach (InvoiceSummaryData invoiceSummaryData in cartManager.Cart.ComboOrder.Invoices)
                            {
                                invoiceIds.Add(invoiceSummaryData.SoldToParty.Id);
                            }
                        }

                        //QueryData query = new QueryData("BDR_Org_Account_Products");
                        //query.AddCriteria(new CriteriaData("MERCHANT_ACCOUNT_ID", OperationData.Equal, GetDefaultMerchantAccountId().ToString()));
                        //FindResultsData results = EntityManager.Find(query);

                        //if (results != null && results.Result != null && results.Result.Count > 0)
                        //{
                        //    foreach (GenericEntityData productEntity in results.Result)
                        //    {
                        //        invoiceProducts.Add(productEntity["PRODUCT_CODE"].ToString());
                        //    }
                        //}

                        FindResultsData subscriptionResults = null;
                        //FindResultsData arResults = null;
                        if (invoicesExist)
                        {
                            QueryData datumPre = new QueryData("es_vSoaInvoiceLineCash");

                            datumPre.AddCriteria(new CriteriaData("ShipToPartyId", OperationData.In, invoiceIds));

                            FindResultsData subscriptionResultsPre = EntityManager.Find(datumPre);

                            if (subscriptionResultsPre.Result != null && subscriptionResultsPre.Result.Count > 0)
                            {
                                foreach (GenericEntityData subscription in subscriptionResultsPre.Result)
                                {
                                    //Response.Write("<br/>vsoa " + subscription["ItemId"].ToString());
                                    products.Add(subscription["ItemId"].ToString());
                                }
                            }

                            //MP on 04/08/2016 for accrual Dues
                            //QueryData datumAr = new QueryData("es_vSoaInvoiceLineAR");

                            //datumAr.AddCriteria(new CriteriaData("ShipToPartyId", OperationData.In, invoiceIds));

                            //FindResultsData arResultsPre = EntityManager.Find(datumAr);

                            //if (arResultsPre.Result != null && arResultsPre.Result.Count > 0)
                            //{
                            //    foreach (GenericEntityData subscription in arResultsPre.Result)
                            //    {
                            //        Response.Write("<br/>vsoa " + subscription["ItemId"].ToString());
                            //        products.Add(subscription["ItemId"].ToString());
                            //    }
                            //}

                            if (products.Count > 0)
                            {
                                QueryData queryCheck = new QueryData("BDR_Org_Account_Products");
                                queryCheck.AddCriteria(new CriteriaData("MERCHANT_ACCOUNT_ID", OperationData.Equal, Utility.GetDefaultMerchantAccountId().ToString()));
                                queryCheck.AddCriteria(new CriteriaData("PRODUCT_CODE", OperationData.In, products));
                                FindResultsData results = EntityManager.Find(queryCheck);

                                if (results != null && results.Result != null && results.Result.Count > 0)
                                {
                                    foreach (GenericEntityData productEntity in results.Result)
                                        invoiceProducts.Add(productEntity["PRODUCT_CODE"].ToString());
                                }

                                foreach (string invoiceId in invoiceIds)
                                {
                                    QueryData queryDatum = new QueryData("es_vSoaInvoiceLineCash");

                                    queryDatum.AddCriteria(new CriteriaData("ItemId", OperationData.In, invoiceProducts));
                                    queryDatum.AddCriteria(new CriteriaData("ShipToPartyId", OperationData.In, invoiceId));

                                   // Response.Write("Processing..");
                                    //Response.Write("<br/>InvoiceProductCount - "+invoiceProducts.Count.ToString()+" -Invoice ID- "+invoiceId);

                                    //SD debugging for APHA

                                    if (invoiceProducts.Count > 0)
                                    {
                                        subscriptionResults = EntityManager.Find(queryDatum);

                                        if (subscriptionResults.Result != null && subscriptionResults.Result.Count > 0)
                                        {
                                            foreach (GenericEntityData subscription in subscriptionResults.Result)
                                            {
                                                if (!allProductsCartList.ContainsKey(subscription["ItemId"].ToString()))
                                                {
                                                    if (invoicesExist)
                                                    {
                                                        //Response.Write("<br/> allprodcartlist : " + subscription["ItemId"].ToString());
                                                        allProductsCartList.Add(subscription["ItemId"].ToString(), Math.Round(Convert.ToDecimal(subscription["Balance"]), 2));
                                                    }
                                                }
                                                // Response.Write("<br/>autoEnroll-" + AutoEnrollment);
                                                if (AutoEnrollment && !enrollmentProducts.ContainsKey(subscription["ItemId"].ToString()) && invoicesExist) //AutoEnrollment && please add back - HS
                                                {
                                                    //Response.Write("<br/> im here 1 "+ subscription["ItemId"].ToString());
                                                    enrollmentProducts.Add(subscription["ItemId"].ToString(), Math.Round(Convert.ToDecimal(subscription["Balance"]), 2));
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            //MP on 04/08/2016 for accrual dues
                            //foreach (string invoiceId in invoiceIds)
                            //{
                            //    QueryData queryDatum = new QueryData("es_vSoaInvoiceLineAR");

                            //    queryDatum.AddCriteria(new CriteriaData("ItemId", OperationData.In, invoiceProducts));
                            //    queryDatum.AddCriteria(new CriteriaData("ShipToPartyId", OperationData.In, invoiceId));

                            //    arResults = EntityManager.Find(queryDatum);

                            //    if (arResults.Result != null && arResults.Result.Count > 0)
                            //    {
                            //        foreach (GenericEntityData subscription in arResults.Result)
                            //        {
                            //            if (!allProductsCartList.ContainsKey(subscription["ItemId"].ToString()))
                            //            {
                            //                if (invoicesExist)
                            //                {
                            //                    Response.Write("<br/> allprodcartlist : " + subscription["ItemId"].ToString());
                            //                    allProductsCartList.Add(subscription["ItemId"].ToString(), Math.Round(Convert.ToDecimal(subscription["Balance"]), 2));
                            //                }
                            //            }
                            //            // Response.Write("<br/>autoEnroll-" + AutoEnrollment);
                            //            if (!enrollmentProducts.ContainsKey(subscription["ItemId"].ToString()) && invoicesExist) //AutoEnrollment && please add back - HS
                            //            {
                            //                Response.Write("<br/> im here 1 " + subscription["ItemId"].ToString());
                            //                enrollmentProducts.Add(subscription["ItemId"].ToString(), Math.Round(Convert.ToDecimal(subscription["Balance"]), 2));
                            //            }
                            //        }
                            //    }
                            //}

                            billingCycleProducts.Clear();
                            foreach (string product in products)
                            {
                                if (product.Contains("CHAPT/"))
                                {
                                    billingProduct = CheckForChapterProducts(product);
                                    billingCycleProducts.Add(billingProduct);
                                }
                                else
                                {
                                    // Response.Write("<br/> billcycprod: " + product);
                                    billingCycleProducts.Add(product);
                                }
                            }

                            if (billingCycleProducts.Count > 0)
                            {
                                QueryData itemSetQuery1 = new QueryData("PartyItemPrice");
                                itemSetQuery1.AddCriteria(new CriteriaData("ItemCode", OperationData.In, billingCycleProducts));
                                itemSetQuery1.AddCriteria(new CriteriaData("PartyId", OperationData.In, invoiceIds));
                                FindResultsData itemSetResults1 = EntityManager.Find(itemSetQuery1);
                                if (itemSetResults1 != null && itemSetResults1.Result.Count > 0)
                                {
                                    foreach (PartyItemPriceData itemEntity in itemSetResults1.Result)
                                    {
                                        //baseProductPrices.Add(itemEntity.Item.ItemId, Convert.ToDecimal(itemEntity.StandardPrice));

                                        decimal cartPrice = 0;
                                        decimal fullPrice = 0;

                                        if (itemEntity.Item.ItemId.Contains("CHAPT/"))
                                        {
                                            itemEntity.Item.ItemId = itemEntity.Item.ItemId.Substring(itemEntity.Item.ItemId.IndexOf("CHAPT/"));
                                        }

                                        allProductsCartList.TryGetValue(itemEntity.Item.ItemId, out cartPrice);
                                        if (itemEntity.Item.ItemId.Contains("DISCOUNTAD"))
                                        {
                                            foreach (KeyValuePair<string, decimal> promoCodeAmount in Utility.RunStoredProcedure("enSYNC_SP_OAD_Discounts"))
                                            {
                                                fullPrice = promoCodeAmount.Value;
                                            }
                                        }
                                        else
                                        {
                                            fullPrice = Convert.ToDecimal(itemEntity.StandardPrice);

                                            if (!enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId) && fullPrice != cartPrice && cartPrice != 0)
                                            {
                                                fullPrice = cartPrice;
                                            }
                                            else if (enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId))
                                            {
                                                enrollmentProducts.TryGetValue(itemEntity.Item.ItemId, out fullPrice);
                                            }
                                        }

                                        // Response.Write("<br/> enrollprodcontains1"+enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId));
                                        if (enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId))
                                        {
                                            //  Response.Write("<br/> fullprice"+fullPrice);
                                            if (fullPrice != 0)
                                                autoDraftProduct = true;
                                        }
                                    }
                                }
                            }
                        }

                        foreach (string duesItem in products)
                        {
                            allProductsCartList.Remove(duesItem);
                        }

                        billingCycleProducts.Clear();
                        foreach (string product in allProductsCartList.Keys)
                        {
                            if (product.Contains("CHAPT/"))
                            {
                                billingProduct = CheckForChapterProducts(product);
                                billingCycleProducts.Add(billingProduct);
                            }
                            else
                                billingCycleProducts.Add(product);
                        }
                        if (billingCycleProducts.Count > 0)
                        {
                            QueryData itemSetQuery = new QueryData("PartyItemPrice");
                            itemSetQuery.AddCriteria(new CriteriaData("ItemCode", OperationData.In, billingCycleProducts));
                            itemSetQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                            FindResultsData itemSetResults = EntityManager.Find(itemSetQuery);
                            if (itemSetResults != null && itemSetResults.Result.Count > 0)
                            {
                                foreach (PartyItemPriceData itemEntity in itemSetResults.Result)
                                {
                                    decimal cartPrice = 0;
                                    decimal fullPrice = 0;

                                    allProductsCartList.TryGetValue(itemEntity.Item.ItemId, out cartPrice);

                                    if (itemEntity.Item.ItemId.Contains("DISCOUNTAD"))
                                    {
                                        foreach (KeyValuePair<string, decimal> promoCodeAmount in Utility.RunStoredProcedure("enSYNC_SP_OAD_Discounts"))
                                        {
                                            fullPrice = promoCodeAmount.Value;
                                        }
                                    }
                                    else
                                    {
                                        bool priceFound = false;

                                        if (Convert.ToDecimal(itemEntity.StandardPrice) == 0)
                                        {
                                            QueryData giftQuery = new QueryData("OAD_Gift_Tracker");
                                            giftQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                                            giftQuery.AddCriteria(new CriteriaData("PRODUCT_CODE", OperationData.Equal, itemEntity.Item.ItemId));
                                            FindResultsData giftResults = EntityManager.Find(giftQuery);

                                            if (giftResults != null && giftResults.Result != null && giftResults.Result.Count > 0)
                                            {
                                                GenericEntityData productEntity = giftResults.Result[0] as GenericEntityData;
                                                fullPrice = Convert.ToDecimal(productEntity["PRICE"]);
                                                priceFound = true;
                                            }
                                        }
                                        if (!priceFound)
                                            fullPrice = Convert.ToDecimal(itemEntity.StandardPrice);


                                        if (!enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId) && fullPrice != cartPrice && cartPrice != 0)
                                        {
                                            fullPrice = cartPrice;
                                        }
                                        else if (enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId))
                                        {
                                            enrollmentProducts.TryGetValue(itemEntity.Item.ItemId, out fullPrice);
                                        }
                                    }

                                    //if (Math.Round((fullPrice / NumberOfPayments), 2) == Math.Round(cartPrice, 2))
                                    if (enrollmentProducts.Keys.Contains(itemEntity.Item.ItemId))
                                    {
                                        //  Response.Write("<br/> fullprice"+fullPrice);
                                        if (fullPrice != 0)
                                            autoDraftProduct = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
            //Response.Write("<br/>autodraft prod " + autoDraftProduct);
            return autoDraftProduct;
        }

        protected String CheckForChapterProducts(String product)
        {
            string chapterBillingProducts = String.Empty;
            string memberType = String.Empty;
            string category = String.Empty;
            string billingCycleName = String.Empty;
            try
            {
                QueryData nameQuery = new QueryData("Name");
                nameQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                FindResultsData nameResults = EntityManager.Find(nameQuery);

                if (nameResults != null && nameResults.Result != null && nameResults.Result.Count > 0)
                {
                    foreach (GenericEntityData nameEntity in nameResults.Result)
                    {
                        if (nameEntity["MEMBER_TYPE"] != null && !String.IsNullOrEmpty(nameEntity["MEMBER_TYPE"].ToString()))
                            memberType = nameEntity["MEMBER_TYPE"].ToString();
                        if (nameEntity["CATEGORY"] != null && !String.IsNullOrEmpty(nameEntity["CATEGORY"].ToString()))
                            category = nameEntity["CATEGORY"].ToString();
                    }
                }

                DataParameter[] dataParameterArray = new DataParameter[4];
                DataParameter dataParameter = new DataParameter("@id", SqlDbType.VarChar);
                dataParameter.Value = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                dataParameterArray[0] = dataParameter;

                DataParameter dataParameter1 = new DataParameter("@memberType", SqlDbType.VarChar);
                dataParameter1.Value = memberType;
                dataParameterArray[1] = dataParameter1;

                DataParameter dataParameter2 = new DataParameter("@category", SqlDbType.VarChar);
                dataParameter2.Value = category;
                dataParameterArray[2] = dataParameter2;

                DataParameter dataParameter3 = new DataParameter("@products", SqlDbType.VarChar);
                dataParameter3.Value = product;
                dataParameterArray[3] = dataParameter3;

                foreach (KeyValuePair<string, object> billingCycle in Utility.RunStoredProcedure("sp_es_GetChapterBillingName", dataParameterArray))
                {
                    if (billingCycle.Key == "DuesCycleName")
                    {
                        billingCycleName = billingCycle.Value.ToString();
                        billingCycleName = billingCycleName.Replace(" ", "_");
                    }
                }

                if (product.Contains("CHAPT/") && !String.IsNullOrEmpty(billingCycleName))
                    chapterBillingProducts = billingCycleName + "/" + product;
                else
                    chapterBillingProducts = product;
            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
            return chapterBillingProducts;
        }

        /// <summary>
        /// Check ASI cart for enrollment eligible products
        /// </summary>
        /// <returns>default merchant account Id value</returns>
        //protected int GetDefaultMerchantAccountId()
        //{
        //    int merchantAccountId = 0;
        //    try
        //    {
        //        MemberAccountLinqDefinition memberAccountLinqDefinition = new MemberAccountLinqDefinition();

        //        QueryData query = new QueryData("OrganizationAccount");
        //        query.AddCriteria(new CriteriaData("AccountType", OperationData.Equal, memberAccountLinqDefinition.GetDraftTypeValue(DraftTypeData.CreditorDebitCard).ToString()));
        //        query.AddCriteria(new CriteriaData("DefaultAccount", OperationData.Equal, true.ToString()));
        //        FindResultsData results = EntityManager.Find(query);

        //        if (results != null && results.Result != null && results.Result.Count > 0)
        //        {
        //            OrganizationAccountData organizationAccountData = results.Result[0] as OrganizationAccountData;
        //            return organizationAccountData.AccountId;
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        ErrorLabel.Text += error.Message;
        //    }
        //    return merchantAccountId;
        //}

        /// <summary>
        /// Saving ach account id
        /// </summary>
        private void UpdateACHAccountToEnrollmentTable(string memberAccountId)
        {
            try
            {
                ValidateResultsData<GenericEntityData> enrollmentResults = null;

                QueryData giftQuery = new QueryData("OAD_Enrollment_Tracker");
                giftQuery.AddCriteria(new CriteriaData("PartyId", OperationData.Equal, Asi.Security.Utility.SecurityHelper.GetSelectedImisId()));
                FindResultsData giftResults = EntityManager.Find(giftQuery);

                if (giftResults != null && giftResults.Result != null && giftResults.Result.Count > 0)
                {
                    foreach (GenericEntityData existingEnrollmentEntity in giftResults.Result)
                    {
                        GenericEntityData enrollmentEntity = existingEnrollmentEntity;
                        enrollmentEntity.EntityTypeName = "OAD_Enrollment_Tracker";
                        enrollmentEntity["PartyId"] = Asi.Security.Utility.SecurityHelper.GetSelectedImisId();
                        enrollmentEntity["ACH_ACCOUNT_ID"] = memberAccountId;

                        enrollmentEntity["SEQN"] = existingEnrollmentEntity["SEQN"].ToString();
                        enrollmentResults = EntityManager.Update(enrollmentEntity);
                        if (enrollmentResults != null)
                        {
                            foreach (ValidationResultData vrd in enrollmentResults.ValidationResults.Errors)
                            {
                                ErrorLabel.Text += "<br/>" + vrd.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
        }

        /// <summary>
        /// Saves details to ASI's payment object
        /// </summary>
        /// <returns></returns>
        protected void SavePaymentInfoToiMisObject()
        {
            try
            {
                //bool differentPayor = false;
                var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
                var comboOrderManager = new ComboOrderManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

                //if (cartManager.Cart.ComboOrder.Invoices.Count > 0)
                //{
                //    foreach (InvoiceSummaryData invoiceSummaryData in cartManager.Cart.ComboOrder.Invoices)
                //    {
                //        if (invoiceSummaryData.BillToParty.Id != Asi.Security.Utility.SecurityHelper.GetSelectedImisId())
                //            differentPayor = true;
                //    }
                //}

                AddressLineDataCollection billingAddressLines = new AddressLineDataCollection();
                string securityCode = String.Empty;
                string cardType = String.Empty;
                string cardTypeName = String.Empty;
                string holdersName = String.Empty;
                string cardNumber = String.Empty;
                string city = String.Empty;
                string state = String.Empty;
                string country = String.Empty;
                string zip = String.Empty;
                int expirationYear = Convert.ToInt32(CreditCardExpirationYearList.SelectedValue);
                int expirationMonth = Convert.ToInt32(CreditCardExpirationList.SelectedValue);

                if (CreditCardNumHiddenField.Value != String.Empty && CreditCardNumTextBox.Text.Contains("*") && CreditCardNumHiddenField.Value.EndsWith(CreditCardNumTextBox.Text.Substring((CreditCardNumTextBox.Text.Length) - 4, 4)))
                {
                    cardNumber = CreditCardNumHiddenField.Value;
                }
                else
                {
                    double n;
                    if (double.TryParse(CreditCardNumTextBox.Text, out n))
                    {
                        cardNumber = CreditCardNumTextBox.Text;
                    }
                }

                cardType = CreditCardTypeList.SelectedValue;
                cardTypeName = CreditCardTypeList.SelectedItem.Text;
                holdersName = CreditCardNameTextBox.Text;
                securityCode = CreditCardSecurityCodeTextBox.Text;

                billingAddressLines.Add(BillingAddressTextBox.Text);
                if (BillingAddressTextBox1.Text.Length > 0)
                    billingAddressLines.Add(BillingAddressTextBox1.Text);
                if (BillingAddressTextBox2.Text.Length > 0)
                    billingAddressLines.Add(BillingAddressTextBox2.Text);

                city = CityTextBox.Text;
                if (StateList.Visible)
                    state = StateList.SelectedValue;
                else if (StateTextBox.Visible)
                    state = StateTextBox.Text;

                country = CountryList.SelectedValue;
                zip = ZipTextBox.Text;

                cartManager.Cart.ComboOrder.Payments.Clear();   // Shouldn't be necessary for a new Cart object, this is just for demo purposes.

                //PaymentData paymentData = new PaymentData();
                //paymentData.Amount = new MonetaryAmountData(comboOrderManager.TransactionGrandTotal);
                //paymentData.CreditCardInformation = new CreditCardInformationData();
                //paymentData.CreditCardInformation.CardNumber = cardNumber;
                //paymentData.CreditCardInformation.Expiration = new YearMonthDateData(expirationYear, expirationMonth);
                //paymentData.CreditCardInformation.HoldersName = holdersName;
                //paymentData.CreditCardInformation.SecurityCode = securityCode;

                //paymentData.CreditCardInformation.Address = new AddressData();
                //paymentData.CreditCardInformation.Address.AddressLines = billingAddressLines;
                //paymentData.CreditCardInformation.Address.CityName = city;
                //if (state != "default")
                //    paymentData.CreditCardInformation.Address.CountrySubEntityCode = state;
                //if(CountryList.Visible)
                //    paymentData.CreditCardInformation.Address.CountryCode = country;
                //paymentData.CreditCardInformation.Address.PostalCode = zip;
                //paymentData.PaymentMethod = new PaymentMethodData();
                //paymentData.PaymentMethod.PaymentMethodId = cardType;
                //paymentData.PaymentMethod.Name = cardTypeName;
                //cartManager.Cart.ComboOrder.Payments.Add(paymentData);
                //12/1/2015 ASTA
                //PartyData partyData = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as PartyData;
                //String companyID = "";
                //12/1/2015 ASTA
                RemittanceData paymentData = new RemittanceData();
                CurrencyData currency = new CurrencyData();
                currency.CurrencyCode = "USD";
                //MonetaryAmountData amount = new MonetaryAmountData();
                //amount.Amount = comboOrderManager.TransactionGrandTotal;
                paymentData.Amount = new MonetaryAmountData(comboOrderManager.TransactionGrandTotal, currency);
                paymentData.CreditCardInformation = new CreditCardInformationData();
                paymentData.CreditCardInformation.CardNumber = cardNumber;
                paymentData.CreditCardInformation.Expiration = new YearMonthDateData(expirationYear, expirationMonth);
                paymentData.CreditCardInformation.HoldersName = holdersName;
                paymentData.CreditCardInformation.SecurityCode = securityCode;
                //12/1/2015 ASTA
                //if (partyData.AdditionalAttributes.GetPropertyValue("ParentPartyId") != null)
                //    companyID = partyData.AdditionalAttributes["ParentPartyId"].Value.ToString();
                //paymentData.PayorParty = EntityManager.FindByIdentity(new IdentityData("Party", companyID)) as CustomerPartyData;
                //12/1/2015 ASTA
                paymentData.CreditCardInformation.Address = new AddressData();
                paymentData.CreditCardInformation.Address.AddressLines = billingAddressLines;
                paymentData.CreditCardInformation.Address.CityName = city;
                if (state != "default")
                    paymentData.CreditCardInformation.Address.CountrySubEntityCode = state;
                if (CountryList.Visible)
                    paymentData.CreditCardInformation.Address.CountryCode = country;
                paymentData.CreditCardInformation.Address.PostalCode = zip;
                paymentData.PaymentMethod = new PaymentMethodData();
                paymentData.PaymentMethod.PaymentMethodId = cardType;
                paymentData.PaymentMethod.Name = cardTypeName;
                //if (differentPayor)
                //{
                //    paymentData.PayorParty = EntityManager.FindByIdentity(new IdentityData("Party", Asi.Security.Utility.SecurityHelper.GetSelectedImisId())) as CustomerPartyData;
                //}

                cartManager.Cart.ComboOrder.Payments.Add(paymentData);

                //cartManager.Cart.ComboOrder.Order.BillToCustomerParty = new Asi.Soa.Commerce.DataContracts.CustomerPartyData { PartyId = "1" };
                //cartManager.Cart.ComboOrder.Order.SoldToCustomerParty = new Asi.Soa.Commerce.DataContracts.CustomerPartyData { PartyId = "1" };
                //PaymentData paymentData1 = new PaymentData();
                //paymentData1.CreditCardInformation = null;
                //paymentData1.Amount = new MonetaryAmountData(comboOrderManager.TransactionGrandTotal);
                //paymentData1.ReferenceNumber = "PO123456";
                //paymentData1.PayorParty = new Asi.Soa.Commerce.DataContracts.CustomerPartyData { PartyId = "1" };
                //paymentData1.PaymentMethod = new PaymentMethodData();
                //paymentData1.PaymentMethod.PaymentMethodId = "BillMe";
                //paymentData1.PaymentMethod.Name = "Bill Me";
                //paymentData1.PaymentMethod.PaymentType = "BillMe";
                //cartManager.Cart.ComboOrder.Payments.Add(paymentData1);
            }
            catch (Exception error)
            {
                ErrorLabel.Text += error.Message + error.StackTrace;
            }
        }

        protected void SaveButton_Clicked(object ssender, EventArgs e)
        {
            SaveDetails();

        }



        #endregion

        #region Method Overrides

        /// <summary>
        /// Called on the connection consumer. This method will act on the object passed in from
        /// the connection provider.
        /// </summary>
        /// <param name="providerObject">Object passed in from the connection provider.</param>
        public override void SetObjectProviderData(Object providerObject)
        {
            // TODO: If this iPart is to be a connection consumer, add code here to act on the
            // object passed in from the connection provider. Note that other connection types 
            // are available, see SetAtomObjectProviderData, SetUniformKeyProviderData, 
            // SetStringKeyProviderData.
            if (!AutoEnrollment)
            {
                string autoDraftAmount = String.Empty;
                string firstPaymentDate = String.Empty;

                if (providerObject.GetType() == typeof(Dictionary<string, string>))
                {
                    Dictionary<string, string> parameter = (Dictionary<string, string>)providerObject;

                    parameter.TryGetValue("AutoDraftAmount", out autoDraftAmount);

                    parameter.TryGetValue("FirstPaymentDate", out firstPaymentDate);
                    FirstPaymentDateHiddenField.Value = firstPaymentDate;
                    if (Convert.ToDecimal(autoDraftAmount) <= 0 || (firstPaymentDate == "Today" && PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString()))
                    {
                        //Response.Write("here");
                        HideContent = true;
                    }
                    else
                    {

                        HideContent = false;
                        //Response.Write(" 5");//AccountNamePanel.Visible = true; MP - 06/20/2016

                        if (firstPaymentDate == "Today" && PaymentMethodAllowedConfig == PaymentMethodOptions.Both.ToString())
                        {
                            if (PaymentMethodList.SelectedValue == PaymentMethodOptions.CreditCard.ToString())
                                PaymentContentPanel.Visible = false;
                            else
                                PaymentContentPanel.Visible = true;
                        }

                        if (PaymentMethodAllowedConfig == PaymentMethodOptions.CreditCard.ToString() && Session["AddressLoaded"] == null)
                        {
                            AddressPurpose_Changed(AddressPurposeList, new EventArgs());
                            Session["AddressLoaded"] = true;
                        }
                    }
                }
            }
            else
            {
                if (!IsPostBack)
                {
                    // Response.Write("<br/> setobjprov postback checkbasket " + CheckBasketForAutoDraftProducts());
                    if (CheckBasketForAutoDraftProducts())
                        Response.Write("");//AccountNamePanel.Visible = true; MP 06/20/2016
                    else
                        AccountNamePanel.Visible = false;
                }
                else
                {
                    // Response.Write("<br/> setobjprov if not postback checkbasket " + CheckBasketForAutoDraftProducts());
                    if (Session["AuthorizationCheckedForAutoEnrollment"] != null && Convert.ToBoolean(Session["AuthorizationCheckedForAutoEnrollment"]))
                        Response.Write("");//AccountNamePanel.Visible = true; MP 06/20/2016
                    else
                        AccountNamePanel.Visible = false;
                }
            }
        }

        /// <summary>
        /// Called on the connection provider. 
        /// </summary>
        /// <returns>An object that will be acted on by the connection consumer.</returns>
        public override Object GetObjectProviderData()
        {
            // TODO: If this iPart is to be a connection provider, add code here to return
            // an object that will be acted on by the connection consumer. Note that other connection 
            // types are available, see GetAtomObjectProviderData, GetUniformKeyProviderData, 
            // GetStringKeyProviderData.

            Dictionary<string, bool> parameters = new Dictionary<string, bool>();
            //Response.Write("Session['UpdatePaymentMethodList']get " + Session["UpdatePaymentMethodList"]);
            if (Session["UpdatePaymentMethodList"] != null && (bool)Session["UpdatePaymentMethodList"])
            {
                parameters.Add("UpdatePaymentMethodList", true);
                //parameters.Add(PaymentMethodList.SelectedValue.ToString(), true);
                Session.Remove("UpdatePaymentMethodList");
            }

            return parameters;
        }

        //public override void CommandButtonRequisites(CommandButtonRequisiteArgs e)
        //{
        //    base.CommandButtonRequisites(e);

        //    if (OnlyUpdateAutoDraftAccountConfig)
        //    {
        //        e.SetCausesValidation(CommandButtonType.Save, true);
        //        e.SetNeed(CommandButtonType.Save);
        //        e.SetText(CommandButtonType.Save, ResourceManager.GetPhrase("MySaveButton", "Update"));
        //    }
        //}

        public override void Commit()
        {
            // First, run any base class commits
            base.Commit();
            // Second, run any iPart specific commits

            Decimal cartTotal = 0;

            var cartManager = new CartManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());
            var comboOrderManager = new ComboOrderManager(EntityManager, Asi.Security.Utility.SecurityHelper.GetSelectedImisId());

            foreach (OrderLineData line in cartManager.Cart.ComboOrder.Order.Lines)
            {
                cartTotal += Convert.ToDecimal(line.UnitPrice);
            }
            foreach (InvoiceSummaryData isd in cartManager.Cart.ComboOrder.Invoices)
            {
                cartTotal += isd.Balance.Amount;
            }
            if (cartTotal == 0)
            {
                cartManager.Cart.ComboOrder.Payments.Clear();
            }
            else
            {
                if (!OnlyUpdateAutoDraftAccountConfig)
                {
                    SaveDetails();
                }
            }
            if (AutoEnrollment)
                Session.Remove("AuthorizationCheckedForAutoEnrollment");

        }
        #endregion Method Overrides

        #region Static Methods

        #endregion
    }
}
