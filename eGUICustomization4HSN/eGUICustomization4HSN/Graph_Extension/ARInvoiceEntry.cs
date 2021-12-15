using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.FS;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Graph_Release;

namespace PX.Objects.AR
{
    public class ARInvoiceEntry_Extension : PXGraphExtension<ARInvoiceEntry>
    {
        #region Delegate Action Menu
        public delegate IEnumerable ReportDelegate(PXAdapter adapter, string reportID);
        [PXOverride]
        public IEnumerable Report(PXAdapter adapter, string reportID, ReportDelegate baseMethod)
        {
            IEnumerable records = baseMethod(adapter, reportID);

            //if we are here that means that report is not identified by base method
            PXReportRequiredException ex = null;

            var parameters = new Dictionary<string, string>();
            Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

            foreach (ARInvoice doc in records)
            {
                if (reportID == "TW601000")
                {
                    parameters["ARInvoice.DocType"] = doc.DocType;
                    parameters["ARInvoice.RefNbr"] = doc.RefNbr;

                    ex = PXReportRequiredException.CombineReport(ex, reportID, parameters);
                }
            }

            //if (ex != null) throw ex;
            string actualReportID = new NotificationUtility(Base).SearchReport(ARNotificationSource.Customer, Base.Document, reportID, Base.Document.Current.BranchID);

            reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter, 
                                                                           new NotificationUtility(Base).SearchPrinter, 
                                                                           ARNotificationSource.Customer, reportID, actualReportID, 
                                                                           Base.Document.Current.BranchID);

            if (ex != null)
            {
                PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

                throw ex;
            }

            return records;
        }
        #endregion

        #region Action
        public PXAction<PX.Objects.AR.ARInvoice> BuyPlasticBag;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Buy Plastic Bag")]
        protected void buyPlasticBag()
        {
            ARTran aRTran = Base.Transactions.Cache.CreateInstance() as ARTran;

            TWNGUIPreferences GUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

            if (GUIPreferences.PlasticBag == null)
            {
                throw new MissingMemberException(TWMessages.NoPlasticBag);
            }

            aRTran.InventoryID = GUIPreferences.PlasticBag;
            aRTran.Qty         = 1;

            Base.Transactions.Cache.Insert(aRTran);
        }
        #endregion

        #region Event Handlers
        public bool activateGUI = TWNGUIValidation.ActivateTWGUI(new PXGraph());

        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

        protected void _(Events.RowPersisting<ARInvoice> e, PXRowPersisting InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            ARRegisterExt regisExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(e.Row);

            if (e.Row.CuryDocBal.Equals(decimal.Zero))
            {
                regisExt.UsrVATOutCode = string.Empty;
            }

            if (!string.IsNullOrEmpty(regisExt.UsrGUINo) && (regisExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode31 ||
                                                            regisExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode32 ||
                                                            regisExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode35)
               )
            {
                //TWNGUIPreferences gUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

                //string numberingSeq = regisExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode32) ? gUIPreferences.GUI2CopiesNumbering : gUIPreferences.GUI3CopiesNumbering;

                //AutoNumberAttribute.SetNumberingId<ARRegisterExt.usrGUINo>(e.Cache, numberingSeq);

                tWNGUIValidation.CheckGUINbrExisted(Base, regisExt.UsrGUINo, regisExt.UsrVATOutCode);
            }
        }

        protected void _(Events.RowInserting<ARInvoice> e)
        {
            if (activateGUI && e.Row.DocType.Equals(ARDocType.CreditMemo))
            {
                ARRegisterExt registerExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(e.Row);

                switch (registerExt.UsrVATOutCode)
                {
                    case TWGUIFormatCode.vATOutCode31:
                    case TWGUIFormatCode.vATOutCode35:
                        registerExt.UsrVATOutCode = TWGUIFormatCode.vATOutCode33;
                        break;

                    case TWGUIFormatCode.vATOutCode32:
                        registerExt.UsrVATOutCode = TWGUIFormatCode.vATOutCode34;
                        break;
                }
            }
        }

        protected void _(Events.RowSelected<ARInvoice> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null) { return; }

            ARRegisterExt registerExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(e.Row);

            bool taxNbrBlank  = string.IsNullOrEmpty(registerExt.UsrTaxNbr);
            bool statusClosed = e.Row.Status.IsIn(ARDocStatus.Open, ARDocStatus.Closed);

            BuyPlasticBag.SetVisible(activateGUI);

            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrGUIDate>     (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrGUINo>       (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrOurTaxNbr>   (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrTaxNbr>      (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrVATOutCode>  (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrB2CType>     (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrCarrierID>   (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrNPONbr>      (e.Cache, null, activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt.usrCreditAction>(e.Cache, null, activateGUI &&
                                                                                        !string.IsNullOrEmpty(registerExt.UsrVATOutCode) &&
                                                                                        (registerExt.UsrVATOutCode.IsIn(TWGUIFormatCode.vATOutCode33, TWGUIFormatCode.vATOutCode34)) );

            PXUIFieldAttribute.SetEnabled<ARRegisterExt.usrB2CType>     (e.Cache, e.Row, !statusClosed && taxNbrBlank);
            PXUIFieldAttribute.SetEnabled<ARRegisterExt.usrCarrierID>   (e.Cache, e.Row, !statusClosed && taxNbrBlank && registerExt.UsrB2CType == TWNB2CType.MC);
            PXUIFieldAttribute.SetEnabled<ARRegisterExt.usrNPONbr>      (e.Cache, e.Row, !statusClosed && taxNbrBlank && registerExt.UsrB2CType == TWNB2CType.NPO);
            PXUIFieldAttribute.SetEnabled<ARRegisterExt.usrVATOutCode>  (e.Cache, e.Row, string.IsNullOrEmpty(registerExt.UsrGUINo));
        }

        protected void _(Events.RowDeleted<ARInvoice> e)
        {
            ARRegisterExt aRRegisterExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(e.Row);

            if (string.IsNullOrEmpty(aRRegisterExt.UsrGUINo) || !activateGUI) { return; }

            string taxID = string.Empty;

            /// When deleted, ARTax does not keep the current record, but can fetches the record from the cache.
            foreach (ARTax aRTax in Base.Tax_Rows.Cache.Deleted)
            {
                taxID = aRTax.TaxID;

                goto CreateGUI;
            }

        CreateGUI:
            if (string.IsNullOrEmpty(taxID))
            {
                throw new PXException(TWMessages.NoInvTaxDtls);
            }

            rp.CreateGUITrans(new STWNGUITran()
            {
                VATCode = aRRegisterExt.UsrVATOutCode,
                GUINbr = aRRegisterExt.UsrGUINo,
                GUIStatus = TWNGUIStatus.Voided,
                GUIDirection = TWNGUIDirection.Issue,
                GUIDate = aRRegisterExt.UsrGUIDate,
                TaxZoneID = e.Row.TaxZoneID,
                TaxCategoryID = Base.taxzone.Current.DfltTaxCategoryID,
                TaxID = taxID,
                TaxNbr = aRRegisterExt.UsrTaxNbr,
                OurTaxNbr = aRRegisterExt.UsrOurTaxNbr,
                NetAmount = 0,
                TaxAmount = 0,
                AcctCD = Base.customer.Current.AcctCD,
                AcctName = Base.customer.Current.AcctName,
                DeductionCode = string.Empty,
                Remark = string.Format(TWMessages.DeleteInfo, Base.Accessinfo.UserName),
                OrderNbr = e.Row.RefNbr
            });
        }

        protected void _(Events.FieldUpdated<ARInvoice, ARInvoice.customerID> e)
        {
            ARInvoice row = e.Row as ARInvoice;

            if (row != null && activateGUI && row.DocType == ARDocType.CreditMemo)
            {
                PXCache<ARRegister>.GetExtension<ARRegisterExt>(row).UsrVATOutCode = TWGUIFormatCode.vATOutCode33;
            }
            else if (row != null && activateGUI &&
                     (row.DocType == ARDocType.Invoice || row.DocType == ARDocType.CashSale)
                    )
            {
                CSAnswers cSAnswers = SelectFrom<CSAnswers>.Where<CSAnswers.refNoteID.IsEqual<@P.AsGuid>
                                                                  .And<CSAnswers.attributeID.IsEqual<ARRegisterExt.VATOUTFRMTNameAtt>>>
                                                           .View.ReadOnly.Select(Base, Base.customer.Current.NoteID);
                if (cSAnswers != null)
                {
                    PXCache<ARRegister>.GetExtension<ARRegisterExt>(row).UsrVATOutCode = cSAnswers.Value;
                }
            }
        }

        protected void _(Events.FieldVerifying<ARInvoice, ARInvoice.hold> e)
        {
            if (activateGUI && e.NewValue.Equals(false) && e.OldValue.Equals(true))
            {
                var row = e.Row as ARInvoice;

                if (!row.CuryDocBal.Equals(decimal.Zero) && string.IsNullOrEmpty(PXCache<ARRegister>.GetExtension<ARRegisterExt>(row).UsrVATOutCode))
                {
                    Base.Document.Ask(TWMessages.RemindHeader, TWMessages.ReminderMesg, MessageButtons.OKCancel);

                    if (Base.Document.AskExt() == WebDialogResult.Cancel) { e.Cancel = true; }
                }
            }
        }

        //protected void _(Events.RowUpdated<ARInvoice> e)
        //{
        //    /// When using the copy and paste feature, don't bring the source GUI number into the new one.
        //    if (Base.IsCopyPasteContext.Equals(true))
        //    {
        //        PXCache<ARRegister>.GetExtension<ARRegisterExt>(e.Row).UsrGUINo = string.Empty;
        //    }
        //}

        //protected void _(Events.FieldUpdated<ARRegisterExt.usrVATOutCode> e)
        //{
        //    var row = (ARInvoice)e.Row;

        //    ARRegisterExt regisExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(row);

        //    if (activateGUI && !string.IsNullOrEmpty(regisExt.UsrVATOutCode) && row.CuryDocBal > decimal.Zero)
        //    {
        //        TWNGUIPreferences gUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

        //        regisExt.UsrGUINo = AutoNumberAttribute.GetNextNumber(e.Cache,
        //                                                              row,
        //                                                              regisExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode32) ? gUIPreferences.GUI2CopiesNumbering : gUIPreferences.GUI3CopiesNumbering,
        //                                                              regisExt.UsrGUIDate);
        //    }
        //}
        #endregion

        #region Custom Attribute
        /// <summary>
        /// Create custom attribute that convert number to Chinese word.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
        public class ChineseAmountAttribute : PXStringAttribute, IPXFieldSelectingSubscriber
        {
            void IPXFieldSelectingSubscriber.FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                ARInvoice invoice = sender.Graph.Caches[typeof(ARInvoice)].Current as ARInvoice;

                if (invoice != null)
                {
                    e.ReturnValue = AmtInChinese((int)invoice.CuryDocBal);
                }
            }
        }
        #endregion

        #region Static Methods
        public static string AmtInChinese(int amount)
        {
            try
            {
                string m_1, m_2, m_3, m_4, m_5, m_6, m_7, m_8, m_9, Num, type;

                Num  = amount.ToString();
                type = "";
                m_1  = Num;
                
                string numNum = "0123456789.";
                string numChina = "零壹貳參肆伍陸柒捌玖點";
                string numChinaWeigh = "個拾佰仟萬拾佰仟億拾佰仟萬";

                if (Num.Substring(0, 1) == "0") // 0123 -> 123
                {
                    Num = Num.Substring(1, Num.Length - 1);
                }
                if (!Num.Contains('.'))
                {
                    Num += ".00";
                }
                else//123.234  123.23 123.2
                {
                    Num = Num.Substring(0, Num.IndexOf('.') + 1 + (Num.Split('.')[1].Length > 2 ? 3 : Num.Split('.')[1].Length));
                }
                m_1 = Num;
                m_2 = m_1;
                m_3 = m_4 = "";
                //m_2:1234-> 壹貳叁肆
                for (int i = 0; i < 11; i++)
                {
                    m_2 = m_2.Replace(numNum.Substring(i, 1), numChina.Substring(i, 1));
                }
                // m_3:佰拾萬仟佰拾個

                int iLen = m_1.Length;
                if (m_1.IndexOf('.') > 0)
                {
                    iLen = m_1.IndexOf('.');//獲取整數位數
                }
                for (int j = iLen; j >= 1; j--)
                {
                    m_3 += numChinaWeigh.Substring(j - 1, 1);
                }
                //m_4:2行+3行
                for (int i = 0; i < m_3.Length; i++)
                {
                    m_4 += m_2.Substring(i, 1) + m_3.Substring(i, 1);
                }
                //m_5:4行去"0"後拾佰仟
                m_5 = m_4;
                m_5 = m_5.Replace("零拾", "零");
                m_5 = m_5.Replace("零佰", "零");
                m_5 = m_5.Replace("零仟", "零");
                //m_6:00-> 0,000-> 0
                m_6 = m_5;
                for (int i = 0; i < iLen; i++)
                {
                    m_6 = m_6.Replace("零零", "零");
                }
                //m_7:6行去億,萬,個位"0"
                m_7 = m_6;
                m_7 = m_7.Replace("億零萬零", "億零");
                m_7 = m_7.Replace("億零萬", "億零");
                m_7 = m_7.Replace("零億", "億");
                m_7 = m_7.Replace("零萬", "萬");

                if (m_7.Length > 2)
                {
                    m_7 = m_7.Replace("零個", "個");
                }
                //m_8:7行+2行小數-> 數目
                m_8 = m_7;
                m_8 = m_8.Replace("個", "");
                if (m_2.Substring(m_2.Length - 3, 3) != "點零零")
                {
                    m_8 += m_2.Substring(m_2.Length - 3, 3);
                }
                //m_9:7行+2行小數-> 價格
                m_9 = m_7;
                m_9 = m_9.Replace("個", "元");
                if (m_2.Substring(m_2.Length - 3, 3) != "點零零")
                {
                    m_9 += m_2.Substring(m_2.Length - 2, 2);
                    m_9 = m_9.Insert(m_9.Length - 1, "角");
                    m_9 += "分";
                }
                else
                {
                    m_9 += "整";
                }
                if (m_9 != "零元整")
                {
                    m_9 = m_9.Replace("零元", "");
                }
                m_9 = m_9.Replace("零分", "整");

                if (type == "數量")
                {
                    return m_8;
                }
                else
                {
                    return m_9;
                }
            }
            catch
            {
                throw;
            }
        }

        public static bool CheckAppointmentAmt(ARInvoice invoice)
        {
            FSAppointment appointment = SelectFrom<FSAppointment>.InnerJoin<FSPostDoc>.On<FSPostDoc.appointmentID.IsEqual<FSAppointment.appointmentID>>
                                                                 .Where<FSPostDoc.postDocType.IsEqual<@P.AsString>
                                                                        .And<FSPostDoc.postRefNbr.IsEqual<@P.AsString>>>
                                                                 .View.Select(PXGraph.CreateInstance<ARInvoiceEntry>(), invoice.DocType, invoice.RefNbr);
            
            return appointment != null && appointment.CostTotal.Equals(decimal.Zero);
        }
        #endregion
    }
}