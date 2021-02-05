using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.FS;
using PX.Objects.TX;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Graph;
using eGUICustomization4HSN.Graph_Release;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.AR
{
    public class ARReleaseProcess_Extension : PXGraphExtension<ARReleaseProcess>
    {
        #region Select
        public SelectFrom<TWNGUITrans>
                          .Where<TWNGUITrans.orderNbr.IsEqual<APInvoice.refNbr.FromCurrent>>.View ViewGUITrans;
        #endregion

        /// <summary>
        ///  The variable works for skip first overriding Persist when the second BLC extension has override the same method.
        /// </summary>
        public bool skipPersist = false;

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (skipPersist.Equals(false))
            {
                ARRegister    doc    = Base.ARDocument.Current;
                ARRegisterExt docExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(doc);

                // Check for document and released flag
                if (TWNGUIValidation.ActivateTWGUI(Base).Equals(true) &&
                    doc != null &&
                    doc.Released.Equals(true) &&
                    (doc.DocType.Equals(ARDocType.Invoice) || doc.DocType.Equals(ARDocType.CreditMemo) || doc.DocType.Equals(ARDocType.CashSale)) &&
                    !string.IsNullOrEmpty(docExt.UsrGUINo) &&
                    !string.IsNullOrEmpty(docExt.UsrVATOutCode)
                   )
                {
                    if ((docExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode33) || docExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode34)) &&
                        docExt.UsrCreditAction.Equals(TWNCreditAction.NO))
                    {
                        throw new PXException(TWMessages.CRACIsNone);
                    }

                    //new TWNGUIValidation().CheckCorrespondingInv(Base, docExt.UsrGUINo, docExt.UsrVATOutCode);

                    // Avoid standard logic calling this method twice and inserting duplicate records into TWNGUITrans.
                    if (APReleaseProcess_Extension.CountExistedRec(Base, docExt.UsrGUINo, docExt.UsrVATOutCode, doc.RefNbr) > 0) { return; }

                    Customer customer = SelectFrom<Customer>.Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(Base, doc.CustomerID);

                    TaxTran xTran = APReleaseProcess_Extension.SelectTaxTran(Base, doc.DocType, doc.RefNbr, BatchModule.AR);

                    TaxExt taxExt = PXCache<Tax>.GetExtension<TaxExt>(APReleaseProcess_Extension.SelectTax(Base, xTran.TaxID));

                    if (taxExt.UsrTWNGUI.Equals(false) || taxExt.UsrTWNGUI == null) { return; }

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                        TWNGUITrans tWNGUITrans = rp.InitAndCheckOnAR(docExt.UsrGUINo, docExt.UsrVATOutCode);

                        decimal? netAmt = xTran.TaxableAmt + xTran.RetainedTaxableAmt;
                        decimal? taxAmt = xTran.TaxAmt + xTran.RetainedTaxAmt;

                        string taxCateID = string.Empty;

                        FSAppointment appointment = SelectFrom<FSAppointment>.LeftJoin<FSPostDoc>.On<FSPostDoc.appointmentID.IsEqual<FSAppointment.appointmentID>>
                                                                             .Where<FSPostDoc.postDocType.IsEqual<@P.AsString>
                                                                                    .And<FSPostDoc.postRefNbr.IsEqual<@P.AsString>>>
                                                                             .View.ReadOnly.Select(Base, doc.DocType, doc.RefNbr);

                        string remark = (appointment is null) ? string.Empty : appointment.RefNbr;

                        foreach (ARTran row in Base.ARTran_TranType_RefNbr.Cache.Cached)
                        {
                            taxCateID = row.TaxCategoryID;

                            goto CreatGUI;
                        }
                    //FSxARTran fSxAPTran = (FSxARTran)Base.Caches[typeof(FSxARTran)].Current;
                    //remark = fSxAPTran.AppointmentID;
                    //foreach (CurrencyInfo curyinfo in Base.CurrencyInfo_CuryInfoID.Cache.Cached)
                    //{
                    //    remark = string.Format("幣別:{0}\r\n匯率:{1:N4}\r\n客戶訂單:{2}", doc.CuryID, curyinfo.CuryRate, Base.ARInvoice_DocType_RefNbr.Current.InvoiceNbr);
                    //    if (remark != null) { break; }
                    //}

                    CreatGUI:
                        if (docExt.UsrCreditAction.Equals(TWNCreditAction.CN) || docExt.UsrCreditAction.Equals(TWNCreditAction.NO))
                        {
                            TWNGUIPreferences gUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

                            string numberingSeq = (docExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode32) ? gUIPreferences.GUI2CopiesNumbering : gUIPreferences.GUI3CopiesNumbering;

                            docExt.UsrGUINo = ARGUINbrAutoNumAttribute.GetNextNumber(Base.ARDocument.Cache, doc, numberingSeq, doc.DocDate);

                            rp.CreateGUITrans(new STWNGUITran()
                            {
                                VATCode       = docExt.UsrVATOutCode,
                                GUINbr        = docExt.UsrGUINo,
                                GUIStatus     = doc.CuryOrigDocAmt.Equals(0m) ? TWNGUIStatus.Voided : TWNGUIStatus.Used,
                                BranchID      = Base.ARTran_TranType_RefNbr.Current.BranchID,
                                GUIDirection  = TWNGUIDirection.Issue,
                                GUIDate       = docExt.UsrGUIDate.Value.Date.Add(doc.CreatedDateTime.Value.TimeOfDay),
                                GUITitle      = customer.AcctName,
                                TaxZoneID     = Base.ARInvoice_DocType_RefNbr.Current.TaxZoneID,
                                TaxCategoryID = taxCateID,
                                TaxID         = xTran.TaxID,
                                TaxNbr        = docExt.UsrTaxNbr,
                                OurTaxNbr     = docExt.UsrOurTaxNbr,
                                NetAmount     = netAmt,
                                TaxAmount     = taxAmt,
                                AcctCD        = customer.AcctCD,
                                AcctName      = customer.AcctName,
                                Remark        = remark,
                                BatchNbr      = doc.BatchNbr,
                                OrderNbr      = doc.RefNbr,
                                CarrierType   = GetCarrierType(docExt.UsrCarrierID),
                                CarrierID     = docExt.UsrB2CType.Equals(TWNB2CType.MC) ? GetCarrierID(docExt.UsrTaxNbr, docExt.UsrCarrierID) : null,
                                NPONbr        = docExt.UsrB2CType.Equals(TWNB2CType.NPO) ? GetNPOBAN(docExt.UsrTaxNbr, docExt.UsrNPONbr) : null,
                                B2CPrinted    = (docExt.UsrB2CType.Equals(TWNB2CType.DEF) && string.IsNullOrEmpty(docExt.UsrTaxNbr)) ? true : false,
                            });
                        }

                        if (tWNGUITrans != null)
                        {
                            if (docExt.UsrCreditAction.Equals(TWNCreditAction.VG))
                            {
                                ViewGUITrans.SetValueExt<TWNGUITrans.gUIStatus>(tWNGUITrans, TWNGUIStatus.Voided);
                                ViewGUITrans.SetValueExt<TWNGUITrans.eGUIExported>(tWNGUITrans, false);
                            }
                            else
                            {
                                if (tWNGUITrans.NetAmtRemain < netAmt) { throw new PXException(TWMessages.RemainAmt); }

                                ViewGUITrans.SetValueExt<TWNGUITrans.netAmtRemain>(tWNGUITrans, (tWNGUITrans.NetAmtRemain -= netAmt));
                                ViewGUITrans.SetValueExt<TWNGUITrans.taxAmtRemain>(tWNGUITrans, (tWNGUITrans.TaxAmtRemain -= taxAmt));
                            }

                            ViewGUITrans.Update(tWNGUITrans);
                        }

                        // Manually Saving as base code will not call base graph persis.
                        ViewGUITrans.Cache.Persist(PXDBOperation.Insert);
                        ViewGUITrans.Cache.Persist(PXDBOperation.Update);

                        ts.Complete(Base);

                        if (doc.DocType == ARDocType.Invoice && docExt.UsrGUINo != null && rp.ViewGUITrans.Current.GUIStatus.Equals(TWNGUIStatus.Used) && docExt.UsrB2CType.Equals(TWNB2CType.DEF))
                        {
                            Base.ARTran_TranType_RefNbr.WhereAnd<Where<ARTran.curyExtPrice, Greater<decimal0>>>();
                            PXGraph.CreateInstance<eGUIInquiry>().PrintReport(Base.ARTran_TranType_RefNbr.Select(doc.DocType, doc.RefNbr), rp.ViewGUITrans.Current, false);
                        }
                    }
                }
                // Triggering after save events.
                ViewGUITrans.Cache.Persisted(false);

                Base.ARDocument.Cache.SetValue<ARRegisterExt.usrGUINo>(doc, docExt.UsrGUINo);
                Base.ARDocument.Cache.MarkUpdated(doc);
            }

            baseMethod();
        }
        #endregion

        #region Static Methods
        public static string GetCarrierType(string carrierID)
        {
            // 手機條碼：3J0002, 自然人憑證：CQ0001
            string num1 = "3J0002", num2 = "CQ0001";

            if (string.IsNullOrEmpty(carrierID))
            {               
                return null;
            }
            else
            {
                return carrierID.Substring(0, 1).Equals("/") ? num1 : num2;
            }
        }

        public static string GetCarrierID(string taxNbr, string carrierID)
        {
            return string.IsNullOrEmpty(taxNbr) ? carrierID : null;
        }

        public static string GetNPOBAN(string taxNbr, string nPONbr)
        {
            return string.IsNullOrEmpty(taxNbr) ? nPONbr : null;
        }
        #endregion
    }
}