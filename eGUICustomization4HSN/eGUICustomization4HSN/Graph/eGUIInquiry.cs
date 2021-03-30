using PX.SM;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.FS;
using System;
using System.Globalization;
using System.Collections.Generic;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;
using Branch = PX.Objects.GL.Branch;

namespace eGUICustomization4HSN.Graph
{
    public class eGUIInquiry : PXGraph<eGUIInquiry>
    {
        #region Select & Features
        public PXSavePerRow<TWNGUITrans> Save;
        public PXCancel<TWNGUITrans> Cancel;

        [PXFilterable]
        [PXImport(typeof(TWNGUITrans))]
        public SelectFrom<TWNGUITrans>.View ViewGUITrans;
        public SelectFrom<ARTran>.Where<ARTran.refNbr.IsEqual<TWNGUITrans.orderNbr.FromCurrent>
                                        .And<ARTran.tranType.IsEqual<ARInvoiceType.invoice>
                                            .And<ARTran.curyExtPrice.IsGreater<decimal0>>>>.View.ReadOnly ARTranView;
        #endregion

        #region Action
        public PXAction<TWNGUITrans> PatchPrint;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = TWMessages.PatchPrint)]
        public virtual void patchPrint()
        {
            ButtonValidation(ViewGUITrans.Current);

            PrintReport(ARTranView.Select(), ViewGUITrans.Current, false);
        }

        public PXAction<TWNGUITrans> RePrint;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = TWMessages.RePrint)]
        public virtual void rePrint()
        {
            ButtonValidation(ViewGUITrans.Current);

            PrintReport(ARTranView.Select(), ViewGUITrans.Current, true);
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<TWNGUITrans> e)
        {
            PXUIFieldAttribute.SetEnabled<TWNGUITrans.gUINbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<TWNGUITrans.batchNbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<TWNGUITrans.orderNbr>(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<TWNGUITrans.qREncrypter>(e.Cache, e.Row, false);

            PXImportAttribute.SetEnabled(this, ViewGUITrans.Name, AssignedRole("Financial Supervisor")/*this.Accessinfo.UserName.Equals("admin")*/);
        }
        #endregion

        #region Functions
        public virtual void PrintReport(PXResultset<ARTran> results, TWNGUITrans tWNGUITrans, bool rePrint)
        {
            ViewGUITrans.Current = ViewGUITrans.Current ?? tWNGUITrans;

            List<ARTran> aRTrans = new List<ARTran>();

            try
            { 
                foreach (ARTran row in results)
                {
                   aRTrans.Add(row);
                }

                string taxNbr = ViewGUITrans.Current.TaxNbr ?? string.Empty;

                SMPrinter sMPrinter = GetSMPrinter(this.Accessinfo.UserID);

                TWNB2CPrinter.GetPrinter = sMPrinter != null ? sMPrinter.PrinterName : throw new PXException(TWMessages.DefPrinter);

                TWNB2CPrinter.PrintOnRP100(new List<string>()
                {
                    // #0, #1, #2, #3, #4, #5
                    GetCode39(), GetQRCode1(aRTrans), GetQRCode2(aRTrans), GetMonth(), GetInvoiceNo(), GetRandom(),
                    // #6
                    string.Format("{0:N0}", ViewGUITrans.Current.NetAmount + ViewGUITrans.Current.TaxAmount),
                    // #7
                    ViewGUITrans.Current.OurTaxNbr,
                    // #8
                    taxNbr,
                    // #9
                    string.IsNullOrEmpty(taxNbr) ? string.Empty : "25",
                    // #10
                    ViewGUITrans.Current.GUIDate.Value.Date.Add(ViewGUITrans.Current.CreatedDateTime.Value.TimeOfDay).ToString("yyyy-MM-dd HH:mm:ss"),
                    // #11
                    GetCompanyName(),
                    // #12
                    GetVATTransl(),
                    // #13 ³Æµù = ARTrans.ApporintmentID(The alternative is to write ARTrans.ApporintmentID to GUITrans.Remark, so ³Æµù = GUITrans.Remark)
                    ViewGUITrans.Current.Remark + GetNoteInfo(),
                    // #14 If the ARTran.CuryExtPrice = 0, then don¡¦t print out this line.
                    GetCustOrdNbr(),
                    // #15 
                    GetDefBranchLoc(aRTrans),
                    // #16 
                    GetPaymMethod(),
                    // #17 If GUITrans.TaxNbr is blank / null(¤GÁp¦¡) then don¡¦t print µo²¼©ïÀY else µo²¼©ïÀY = GUITrnas.GUITitle
                    ViewGUITrans.Current.GUITitle
                },
                    rePrint,
                    aRTrans);

                UpdatePrintCount();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// YYYMM of GUITrans.GUIDate + 
        /// GUIItrans.GUINbr + 
        /// If GUITrans.BatchNbr is not null then Right(Guitrans.bachNbr,4) else Right(Guitrans.OrderNbrr,4)
        /// </summary>
        protected string GetCode39()
        {
            return string.Format("{0}{1}{2}", PXGraph.CreateInstance<TWNGenGUIMediaFile>().GetGUILegal(ViewGUITrans.Current.GUIDate.Value),
                                              ViewGUITrans.Current.GUINbr,
                                              string.IsNullOrEmpty(ViewGUITrans.Current.BatchNbr) ? ViewGUITrans.Current.OrderNbr.Substring(ViewGUITrans.Current.OrderNbr.Length - 4) :
                                                                                                    ViewGUITrans.Current.BatchNbr.Substring(ViewGUITrans.Current.BatchNbr.Length - 4));
        }

        /// <summary>
        /// GUITrans.QREncrypter + ':' + '**********' + ':' +
        /// No. of order lines+':' + No. of order lines + ':' + '0' + ':' +
        /// Line description + ':' + Qty (no decimal places) + ':'+Unit Price*1.05(No decimal places)
        /// </summary>
        protected string GetQRCode1(List<ARTran> aRTrans)
        {
            string desc = aRTrans[0].TranDesc;
            
            SubTranDescr(ref desc);

            return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}", ViewGUITrans.Current.QREncrypter, "**********",
                                                                    aRTrans.Count, aRTrans.Count, aRTrans[0].LineNbr, desc, 
                                                                    (int)aRTrans[0].Qty, (int)(aRTrans[0].UnitPrice * (decimal)1.05));
        }

        /// <summary>
        /// **'+Line description+':'+Qty (no decimal places)+':'+Unit Price*1.05(No decimal places)
        /// </summary>
        protected string GetQRCode2(List<ARTran> aRTrans)
        {
            return string.Format("{0}{1}:{2}:{3}", "**", aRTrans[0].TranDesc, (int)aRTrans[0].Qty, (int)(aRTrans[0].UnitPrice * (decimal)1.05));
        }

        /// <summary>
        /// 102¦~11-12
        /// </summary>
        protected string GetMonth()
        {
            TaiwanCalendar tc = new TaiwanCalendar();

            string monFmt = string.Empty;

            switch (tc.GetMonth(ViewGUITrans.Current.GUIDate.Value))
            {
                case 1: 
                case 2:
                    monFmt = "01-02";
                    break;
                case 3: 
                case 4:
                    monFmt = "03-04";
                    break;
                case 5:
                case 6:
                    monFmt = "05-06";
                    break;
                case 7:
                case 8:
                    monFmt = "07-08";
                    break;
                case 9:
                case 10:
                    monFmt = "09-10";
                    break;
                case 11:
                case 12:
                    monFmt = "11-12";
                    break;
            }

            return string.Format("{0}年{1}月", tc.GetYear(ViewGUITrans.Current.GUIDate.Value), monFmt);
        }

        /// <summary>
        /// GUITrans.GUINbr -> YQ-12345678
        /// </summary>
        protected string GetInvoiceNo()
        {
            return string.Format("{0}-{1}", ViewGUITrans.Current.GUINbr.Substring(0, 2), ViewGUITrans.Current.GUINbr.Substring(2));
        }

        /// <summary>
        /// If GUITrans.BatchNbr is not null then Right(Guitrans.bachNbrr,4) else Right(Guitrans.OrderNbrr,4)
        /// </summary>
        protected string GetRandom()
        {
            return string.IsNullOrEmpty(ViewGUITrans.Current.BatchNbr) ? ViewGUITrans.Current.OrderNbr.Substring(ViewGUITrans.Current.OrderNbr.Length - 4) :
                                                                         ViewGUITrans.Current.BatchNbr.Substring(ViewGUITrans.Current.BatchNbr.Length - 4);
        }

        /// <summary>
        /// Get the company name instead of the tenant name.
        /// </summary>
        protected string GetCompanyName()
        {
            PX.Objects.GL.DAC.Organization organization = SelectFrom<PX.Objects.GL.DAC.Organization>
                                                                     .InnerJoin<Branch>.On<Branch.organizationID.IsEqual<PX.Objects.GL.DAC.Organization.organizationID>>
                                                                     .Where<Branch.branchID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(this, this.Accessinfo.BranchID);

            return organization.OrganizationName;
        }

        /// <summary>
        /// ½Òµ|§O = If TAXID =¡¦VAT05¡¦ then ¡§À³µ|¡¨ else if TAXID =¡¦VATEX¡¦ then ¡§§Kµ|¡¨ else ¡§¹sµ|²v¡¨
        /// </summary>
        /// <returns></returns>
        protected string GetVATTransl()
        {
            switch (ViewGUITrans.Current.VATType)
            {
                case TWNGUIVATType.Five:
                    return "(應稅)TX";
                case TWNGUIVATType.Exclude:
                    return "(免稅)";
                default:
                    return "(零稅率))";
            }
        }

        /// <summary>
        /// Get SO Invoice¡¦s Note if the Note is not blank.
        /// </summary>
        protected string GetNoteInfo()
        {
            Note note = SelectFrom<Note>.InnerJoin<ARRegister>.On<ARRegister.noteID.IsEqual<Note.noteID>>
                                        .Where<ARRegister.refNbr.IsEqual<@P.AsString>>.View.ReadOnly.Select(this, ViewGUITrans.Current.OrderNbr);

            return note == null ? string.Empty : note.NoteText;
        }

        /// <summary>
        /// Get AR invoice customer order nbr.
        /// </summary>
        protected string GetCustOrdNbr()
        {
            ARInvoice invoice = SelectFrom<ARInvoice>.Where<ARInvoice.refNbr.IsEqual<TWNGUITrans.orderNbr.FromCurrent>>.View.Select(this);

            return invoice.InvoiceNbr ?? string.Empty;
        }

        /// <summary>
        /// µo²¼¶}¥ß³¡ªù = First ARTran line BranchID to get locatin
        /// </summary>
        protected string GetDefBranchLoc(List<ARTran> aRTrans)
        {
            FSBranchLocation fSBranchLoc = SelectFrom<FSBranchLocation>
                                                      .Where<FSBranchLocation.branchID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(this, aRTrans[0].BranchID);

            return (fSBranchLoc == null || fSBranchLoc.BranchLocationCD == null) ? string.Empty : fSBranchLoc.BranchLocationCD;
        }

        /// <summary>
        /// ¥I´Ú¤è¦¡ = If GUITrans.TaxNbr is not blank  then ¡§¤ëµ²¡¨ else description of ARinvoice.paymentmehtodID
        /// </summary>
        protected string GetPaymMethod()
        {
            PaymentMethod paymMth = SelectFrom<PaymentMethod>.LeftJoin<ARInvoice>.On<ARInvoice.paymentMethodID.IsEqual<PaymentMethod.paymentMethodID>>
                                                             .Where<ARInvoice.refNbr.IsEqual<@P.AsString>>.View.ReadOnly.Select(this, ViewGUITrans.Current.OrderNbr);

            return paymMth.Descr ?? string.Empty;
        }

        /// <summary>
        /// After print the GUI invoice, this field be updated once.
        /// </summary>
        protected void UpdatePrintCount()
        {
            ViewGUITrans.Current.PrintCount = ViewGUITrans.Current.PrintCount?? 0;
            ViewGUITrans.Current.PrintCount += 1;

            ViewGUITrans.Cache.Update(ViewGUITrans.Current);

            this.Actions.PressSave();
        }

        /// <summary>
        /// Check whether the access user is assigned role parameters.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        protected bool AssignedRole(params string[] roleName)
        {
            return GetUsersInRoles(this, roleName) != null;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Get all defined roles through the user parameter.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static UsersInRoles GetUsersInRoles(PXGraph graph, params string[] userName)
        {
            return SelectFrom<UsersInRoles>.Where<UsersInRoles.rolename.IsEqual<@P.AsString>
                                                  .And<UsersInRoles.username.IsEqual<AccessInfo.userName.FromCurrent>>>.View.ReadOnly.Select(graph, userName);
        }

        /// <summary>
        /// Get table buffer from device hub settings.
        /// </summary>
        public static SMPrinter GetSMPrinter(Guid? userID)
        {
            return SelectFrom<SMPrinter>.InnerJoin<UserPreferences>.On<UserPreferences.defaultPrinterID.IsEqual<SMPrinter.printerID>>
                                        .Where<UserPreferences.userID.IsEqual<@P.AsGuid>>.View.ReadOnly.Select(PXGraph.CreateInstance<eGUIInquiry>(), userID);
        }

        /// <summary>
        /// When clicking the print button, verify the main GUI information.
        /// </summary>
        /// <param name="gUITrans"></param>
        public static void ButtonValidation(TWNGUITrans gUITrans)
        {
            string message = string.Empty;

            if (gUITrans == null)
            {
                message = TWMessages.SeltCorrGUI;
            }
            else if (gUITrans.GUIStatus != TWNGUIStatus.Used)
            {
                message = string.Format(TWMessages.StatusNotUsed, gUITrans.GUINbr); 
            }
            else if (gUITrans.GUIFormatcode != TWGUIFormatCode.vATOutCode31 && gUITrans.GUIFormatcode != TWGUIFormatCode.vATOutCode35)
            {
                message = string.Format(TWMessages.FmtCodeIncort, gUITrans.GUINbr);
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new PXException(message);
            }
        }

        /// <summary>
        /// Because eInvoice.dll left QRCode description has byte length limitation (40 bytes).
        /// </summary>
        /// <param name="descr"></param>
        private static void SubTranDescr(ref string descr)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(descr);

            if (bytes.Length > 40)
            {
                descr = System.Text.Encoding.Default.GetString(bytes, 0, 40);
            }
        }
        #endregion
    }

    #region Custom Attribute
    public class GUINumberAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
    {
        public GUINumberAttribute(int length) : base(length) { }

        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (string.IsNullOrEmpty((string)e.NewValue) ||
                e.NewValue.ToString().Length.Equals(10) ||
                TWNGUIValidation.ActivateTWGUI(new PXGraph()).Equals(false)) { return; }

            object obj = null;
            string vATCode = null;

            switch (this.BqlTable.Name)
            {
                case nameof(APRegister):
                    obj = sender.GetValueExt<APRegisterExt.usrVATINCODE>(e.Row);
                    break;
                case nameof(ARRegister):
                    obj = sender.GetValueExt<ARRegisterExt.usrVATOutCode>(e.Row);
                    break;
                case nameof(TWNGUITrans):
                    obj = sender.GetValueExt<TWNGUITrans.gUIFormatcode>(e.Row);
                    break;
                case nameof(TWNManualGUIAP):
                    obj = sender.GetValueExt<TWNManualGUIAP.vATInCode>(e.Row);
                    break;
                case nameof(TWNManualGUIAR):
                    obj = sender.GetValueExt<TWNManualGUIAR.vatOutCode>(e.Row);
                    break;
                case nameof(TWNManualGUIBank):
                    obj = sender.GetValueExt<TWNManualGUIBank.vATInCode>(e.Row);
                    break;
                case nameof(TWNManualGUIExpense):
                    obj = sender.GetValueExt<TWNManualGUIExpense.vATInCode>(e.Row);
                    break;
            }

            vATCode = obj is null ? string.Empty : obj.ToString();

            if (vATCode.StartsWith("3") || vATCode.Equals(TWGUIFormatCode.vATInCode21) || vATCode.Equals(TWGUIFormatCode.vATInCode23) || vATCode.Equals(TWGUIFormatCode.vATInCode25)
               )
            {
                throw new PXSetPropertyException(e.NewValue.ToString().Length > 10 ? TWMessages.GUINbrLength : TWMessages.GUINbrMini, PXErrorLevel.Error);
            }
        }
    }

    public class TaxNbrVerifyAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
    {
        public TaxNbrVerifyAttribute(int length) : base(length) { }

        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (e.NewValue == null) { return; }

            TWNGUIValidation validation = new TWNGUIValidation();

            validation.CheckTabNbr(e.NewValue.ToString());

            if (validation.errorOccurred == true)
            {
                throw new PXSetPropertyException(validation.errorMessage, (PXErrorLevel)validation.errorLevel);
            }

        }
    }

    public class TWNetAmountAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber
    {
        public TWNetAmountAttribute(int percision) : base(percision) { }

        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal)e.NewValue < 0)
            {
                // Throwing an exception to cancel assignment of the new value to the field
                throw new PXSetPropertyException(TWMessages.NetAmtNegError);
            }
        }
    }

    public class TWTaxAmountAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber
    {
        public TWTaxAmountAttribute(int percision) : base(percision) { }

        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal)e.NewValue < 0)
            {
                // Throwing an exception to cancel assignment of the new value to the field
                throw new PXSetPropertyException(TWMessages.TaxAmtNegError);
            }
        }
    }
    #endregion
}