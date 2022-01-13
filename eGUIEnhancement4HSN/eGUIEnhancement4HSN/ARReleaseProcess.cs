using System;
using System.Text;
using System.Collections.Generic;
using PX.SM;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.TX;
using PX.Objects.FS;
using PX.Objects.GL;
using eInvoiceLib;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.Graph;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Graph_Release;

namespace PX.Objects.AR
{
    public class ARReleaseProcess_Extension2 : PXGraphExtension<ARReleaseProcess_Extension, ARReleaseProcess>
    {
        #region Delegate Function
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            try
            {
                ARRegister    doc    = Base.ARDocument.Current;
                ARRegisterExt docExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(doc);

                // Check for document and released flag
                if (TWNGUIValidation.ActivateTWGUI(Base) == true &&
                    doc != null &&
                    doc.Released == true &&
                    (doc.DocType.Equals(ARDocType.Invoice) || doc.DocType.Equals(ARDocType.CreditMemo) || doc.DocType.Equals(ARDocType.CashSale)) &&
                    !string.IsNullOrEmpty(docExt.UsrVATOutCode)
                   )
                {
                    if ((docExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode33) || docExt.UsrVATOutCode.Equals(TWGUIFormatCode.vATOutCode34)) &&
                        docExt.UsrCreditAction.Equals(TWNCreditAction.NO))
                    {
                        throw new PXException(TWMessages.CRACIsNone);
                    }

                    // Avoid standard logic calling this method twice and inserting duplicate records into TWNGUITrans.
                    if (APReleaseProcess_Extension.CountExistedRec(Base, docExt.UsrGUINo, docExt.UsrVATOutCode, doc.RefNbr) > 0) { return; }

                    Customer customer = SelectFrom<Customer>.Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(Base, doc.CustomerID);

                    TaxTran xTran = APReleaseProcess_Extension.SelectTaxTran(Base, doc.DocType, doc.RefNbr, BatchModule.AR);

                    TaxExt taxExt = PXCache<Tax>.GetExtension<TaxExt>(APReleaseProcess_Extension.SelectTax(Base, xTran.TaxID));

                    if (taxExt.UsrTWNGUI == false || taxExt.UsrTWNGUI == null) { return; }

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                        TWNGUITrans tWNGUITrans = rp.InitAndCheckOnAR(docExt.UsrGUINo, docExt.UsrVATOutCode);

                        decimal? netAmt = xTran.TaxableAmt + xTran.RetainedTaxableAmt;
                        decimal? taxAmt = xTran.TaxAmt + xTran.RetainedTaxAmt;

                        FSAppointment appointment = SelectFrom<FSAppointment>.LeftJoin<FSPostDoc>.On<FSPostDoc.appointmentID.IsEqual<FSAppointment.appointmentID>>
                                                                             .Where<FSPostDoc.postDocType.IsEqual<@P.AsString>
                                                                                    .And<FSPostDoc.postRefNbr.IsEqual<@P.AsString>>>
                                                                             .View.ReadOnly.Select(Base, doc.DocType, doc.RefNbr);

                        string remark    = (appointment is null) ? string.Empty : appointment.RefNbr;
                        string taxCateID = string.Empty;
                        int    branchID  = 0;

                        foreach (ARTran row in Base.ARTran_TranType_RefNbr.Cache.Cached)
                        {
                            taxCateID = row.TaxCategoryID;
                            branchID  = row.BranchID.Value;

                            goto CreatGUI;
                        }

                    CreatGUI:
                        if (docExt.UsrCreditAction.IsIn(TWNCreditAction.CN, TWNCreditAction.NO))
                        {
                            TWNGUIPreferences gUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

                            string numberingSeq = (docExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode32) ? gUIPreferences.GUI2CopiesNumbering : gUIPreferences.GUI3CopiesNumbering;

                            docExt.UsrGUINo = ARGUINbrAutoNumAttribute.GetNextNumber(Base.ARDocument.Cache, doc, numberingSeq, doc.DocDate);

                            rp.CreateGUITrans(new STWNGUITran()
                            {
                                VATCode       = docExt.UsrVATOutCode,
                                GUINbr        = docExt.UsrGUINo,
                                GUIStatus     = doc.CuryOrigDocAmt.Equals(0m) ? TWNGUIStatus.Voided : TWNGUIStatus.Used,
                                BranchID      = branchID,
                                GUIDirection  = TWNGUIDirection.Issue,
                                GUIDate       = docExt.UsrGUIDate.Value.Date.Add(doc.CreatedDateTime.Value.TimeOfDay),
                                GUITitle      = PXCacheEx.GetExtension<ARRegisterExt2>(Base.ARDocument.Current).UsrGUITitle,//customer.AcctName,
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
                                CarrierType   = ARReleaseProcess_Extension.GetCarrierType(docExt.UsrCarrierID),
                                CarrierID     = (docExt.UsrB2CType == TWNB2CType.MC) ? ARReleaseProcess_Extension.GetCarrierID(docExt.UsrTaxNbr, docExt.UsrCarrierID) : null,
                                NPONbr        = (docExt.UsrB2CType == TWNB2CType.NPO) ? ARReleaseProcess_Extension.GetNPOBAN(docExt.UsrTaxNbr, docExt.UsrNPONbr) : null,
                                B2CPrinted    = (docExt.UsrB2CType == TWNB2CType.DEF && string.IsNullOrEmpty(docExt.UsrTaxNbr)) ? true : false,
                            });
                        }

                        if (tWNGUITrans != null)
                        {
                            if (docExt.UsrCreditAction == TWNCreditAction.VG)
                            {
                                Base1.ViewGUITrans.SetValueExt<TWNGUITrans.gUIStatus>(tWNGUITrans, TWNGUIStatus.Voided);
                                Base1.ViewGUITrans.SetValueExt<TWNGUITrans.eGUIExported>(tWNGUITrans, false);
                            }
                            else
                            {
                                if (tWNGUITrans.NetAmtRemain < netAmt) { throw new PXException(TWMessages.RemainAmt); }

                                Base1.ViewGUITrans.SetValueExt<TWNGUITrans.netAmtRemain>(tWNGUITrans, (tWNGUITrans.NetAmtRemain -= netAmt));
                                Base1.ViewGUITrans.SetValueExt<TWNGUITrans.taxAmtRemain>(tWNGUITrans, (tWNGUITrans.TaxAmtRemain -= taxAmt));
                            }

                            Base1.ViewGUITrans.Update(tWNGUITrans);
                        }

                        // Manually Saving as base code will not call base graph persis.
                        Base1.ViewGUITrans.Cache.Persist(PXDBOperation.Insert);
                        Base1.ViewGUITrans.Cache.Persist(PXDBOperation.Update);

                        ts.Complete(Base);

                        if (doc.DocType == ARDocType.Invoice && !string.IsNullOrEmpty(docExt.UsrGUINo) && rp.ViewGUITrans.Current.GUIStatus == TWNGUIStatus.Used && docExt.UsrB2CType == TWNB2CType.DEF)
                        {
                            Base.ARTran_TranType_RefNbr.WhereAnd<Where<ARTran.curyExtPrice, Greater<CS.decimal0>>>();
                            PXGraph.CreateInstance<eGUIInquiry2>().PrintReport(Base.ARTran_TranType_RefNbr.Select(doc.DocType, doc.RefNbr), rp.ViewGUITrans.Current, false);
                        }
                    }
                }
                // Triggering after save events.
                Base1.ViewGUITrans.Cache.Persisted(false);
                Base1.skipPersist = true;

                Base.ARDocument.Cache.SetValue<ARRegisterExt.usrGUINo>(doc, docExt.UsrGUINo);
                Base.ARDocument.Cache.MarkUpdated(doc);

                baseMethod();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Extended Classes
        public class eGUIInquiry2 : eGUIInquiry
        {
            public override void PrintReport(PXResultset<ARTran> results, TWNGUITrans tWNGUITrans, bool rePrint)
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
                    bool onlyDetl = !string.IsNullOrEmpty(ViewGUITrans.Current.CarrierID) || !string.IsNullOrEmpty(ViewGUITrans.Current.NPONbr);

                    SMPrinter sMPrinter = GetSMPrinter(this.Accessinfo.UserID);

                    TWNB2CPrinter.GetPrinter = sMPrinter != null ? sMPrinter.PrinterName : throw new PXException(TWMessages.DefPrinter);

                    TWNB2CPrinter2.PrintOnRP100(new List<string>()
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
                        aRTrans,
                        rePrint,
                        onlyDetl,
                        ViewGUITrans.Current.TaxAmount.Value);

                    UpdatePrintCount();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public class TWNB2CPrinter2 : TWNB2CPrinter
        {
            public static void PrintOnRP100(List<string> header, List<ARTran> result, bool rePrint, bool onlyDetl, decimal taxAmt)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                ESCPOS eSCPOS = new ESCPOS();

                eSCPOS.StartLPTPrinter(GetPrinter, TWMessages.eGUI); // Printer name & Task name in printing.              

                if (onlyDetl == false)
                {
                    new AIDA_P1226D(eSCPOS).PrintInvoice(header[0], header[1], header[2], header[3], header[4], header[5],
                                                         header[6], header[7], header[8], rePrint, header[9], header[10]);


                    eSCPOS.SendTo("退款憑電子發票證明聯正本辦理\n");
                    eSCPOS.SendTo("----------------------------\n");

                    if (string.IsNullOrEmpty(header[9]))
                    {
                        eSCPOS.CutPaper(0x42, 0x00);
                    }
                }

                //eSCPOS.SelectCharSize((byte)15);
                // Print details
                eSCPOS.SendTo("銷售明細表\n");
                eSCPOS.Align(0);
                eSCPOS.SendTo(string.Format("名稱: {0}\n", header[11]));
                eSCPOS.SendTo(string.Format("發票號碼: {0}\n", header[4].Trim(new char[] { '-' })));
                eSCPOS.SendTo("品名/數量   單價         金額\n");
                eSCPOS.Align(0);
                
                bool hasSummary = false;
                decimal? prcAmt = 0;
                decimal? txbAmt = 0;
                decimal? extAmt = 0;
                decimal? netAmt = 0;
                decimal? disAmt = 0;

                ARRegister     register  = new ARRegister();
                ARRegisterExt2 regisExt2 = new ARRegisterExt2();

                int rowLen;
                foreach (ARTran aRTran in result)
                {
                    register = ARRegister.PK.Find(new PXGraph(), aRTran.TranType, aRTran.RefNbr);
                    regisExt2 = register.GetExtension<ARRegisterExt2>();

                    hasSummary = regisExt2.UsrSummaryPrint == true && !string.IsNullOrEmpty(regisExt2.UsrGUISummary);

                    if (hasSummary == false)
                    {
                        string qty, uPr, ePr, dAm;

                        eSCPOS.SendTo(string.Format("{0}\n", aRTran.TranDesc));

                        qty = string.Format("{0:N0}", aRTran.Qty);

                        // According to the suggestion of Joyce & YJ, make "unit price" equal to "extended price" when calculation including 5% tax.
                        // B2C
                        if (string.IsNullOrEmpty(header[9]))
                        {
                            if (register.TaxCalcMode == TX.TaxCalculationMode.Gross)
                            {
                                uPr = ePr = string.Format("{0:N2}", aRTran.CuryExtPrice);
                                dAm = string.Format("{0:N2}", -aRTran.CuryDiscAmt);
                            }
                            else
                            {
                                uPr = ePr = string.Format("{0:N2}", decimal.Multiply(aRTran.CuryExtPrice.Value, (decimal)1.05));
                                dAm = string.Format("{0:N2}", decimal.Multiply(-aRTran.CuryDiscAmt.Value, (decimal)1.05));
                            }
                        }
                        // B2B
                        else
                        {
                            if (register.TaxCalcMode == TX.TaxCalculationMode.Gross)
                            {
                                uPr = ePr = string.Format("{0:N2}", decimal.Divide(aRTran.CuryExtPrice.Value, (decimal)1.05));
                                dAm = string.Format("{0:N2}", decimal.Divide(-aRTran.CuryDiscAmt.Value, (decimal)1.05));
                            }
                            else
                            {
                                uPr = ePr = string.Format("{0:N2}", aRTran.CuryExtPrice);
                                dAm = string.Format("{0:N2}", -aRTran.CuryDiscAmt);
                            }
                        }

                        // One row has position of 30 bytes.
                        rowLen = (30 - 3 - qty.Length - ePr.Length - uPr.Length) / 2;

                        eSCPOS.SendTo(new string(' ', 3) + qty + new string(' ', rowLen) + uPr + new string(' ', rowLen) + ePr + '\n');

                        if (aRTran.CuryDiscAmt != decimal.Zero)
                        {
                            eSCPOS.SendTo("折扣" + new string(' ', 30 - dAm.Length - 4) + dAm + '\n');
                        }
                    }

                    prcAmt += aRTran.CuryUnitPrice;
                    txbAmt += aRTran.CuryTaxableAmt;
                    extAmt += aRTran.CuryExtPrice;
                    disAmt += aRTran.CuryDiscAmt;
                    netAmt += aRTran.CuryTranAmt;
                }

                if (hasSummary == true)
                {
                    string prc, ext, dis;

                    // B2C
                    if (string.IsNullOrEmpty(header[9]))
                    { 
                        if (register.TaxCalcMode == TX.TaxCalculationMode.Gross)
                        {
                            prc = ext = string.Format("{0:N2}", extAmt);
                            dis = string.Format("{0:N2}", -disAmt);
                        }
                        else
                        {
                            prc = ext = string.Format("{0:N2}", decimal.Multiply(extAmt.Value, (decimal)1.05));
                            dis = string.Format("{0:N2}", decimal.Multiply(-disAmt.Value, (decimal)1.05));
                        }
                    }
                    // B2B
                    else
                    {
                        if (register.TaxCalcMode == TX.TaxCalculationMode.Gross)
                        {
                            prc = ext = string.Format("{0:N2}", decimal.Divide(extAmt.Value, (decimal)1.05));
                            dis = string.Format("{0:N2}", decimal.Divide(-disAmt.Value, (decimal)1.05));
                        }
                        else
                        {
                            prc = ext = string.Format("{0:N2}", extAmt);
                            dis = string.Format("{0:N2}", -disAmt);
                        }
                    }

                    rowLen = (30 - 3 - 1 - prc.Length - ext.Length) / 2;

                    eSCPOS.SendTo($"{CS.CSAttributeDetail.PK.Find(new PXGraph(), ARInvoiceEntry_Extension2.GUISummary, regisExt2.UsrGUISummary).Description}\n");
                    eSCPOS.SendTo($"{new string(' ', 3)}1{new string(' ', rowLen)}{prc}{new string(' ', rowLen)}{ext}\n");
                    
                    if (disAmt != decimal.Zero)
                    {
                        eSCPOS.SendTo($"折扣{new string(' ', 30 - dis.Length - 4)}{dis}\n");
                    }
                }

                eSCPOS.SendTo(string.Format("共 {0} 項\n", hasSummary == false ? result.Count : 1));

                if (string.IsNullOrEmpty(header[9]) == false)
                {
                    string net, tax;

                    if (register.TaxCalcMode.Equals(TX.TaxCalculationMode.Gross))
                    {
                        net = string.Format("{0:N0}", decimal.Parse(header[6]) - taxAmt);
                        tax = string.Format("{0:N0}", taxAmt);
                    }
                    else
                    {
                        net = string.Format("{0:N0}", netAmt);
                        tax = string.Format("{0:N0}", decimal.Parse(header[6]) - netAmt);
                    }

                    eSCPOS.SendTo("銷售額:" + new string(' ', (30 - net.Length - 7)) + net + '\n');  // 7 -> 銷售額 is a traditional Chinese word has two bytes.
                    eSCPOS.SendTo(string.Format("稅  額:" + new string(' ', (30 - tax.Length - 7)) + tax + '\n'));
                }

                string total = string.Format("{0:N0}", header[6]);

                eSCPOS.SendTo("總  計:" + new string(' ', (30 - total.Length - 7)) + total + '\n');
                eSCPOS.SendTo(string.Format("課稅別:{0}\n", header[12]));
                eSCPOS.SendTo(string.Format("備  註:{0}\n", header[13]));

                if (string.IsNullOrEmpty(header[9]).Equals(false))
                {
                    eSCPOS.SendTo(string.Format("採購號碼:{0}\n", header[14]));
                }

                eSCPOS.SendTo(string.Format("開立發票部門:{0}\n", header[15]));

                if (regisExt2.UsrPrnPayment == true)
                {
                    eSCPOS.SendTo(string.Format("付款方式:{0}\n", header[16]));
                }

                if (!string.IsNullOrEmpty(header[9]) && regisExt2.UsrPrnGUITitle == true)
                {
                    eSCPOS.SendTo(string.Format("發票抬頭:{0}\n", header[17]));
                }

                if (register.GetExtension<ARRegisterExt>().UsrB2CType.Equals(TWNB2CType.MC))
                {
                    eSCPOS.SendTo(string.Format("手機載具:{0}\n", register.GetExtension<ARRegisterExt>().UsrCarrierID));
                }

                eSCPOS.CutPaper(0x42, 0x00);
                eSCPOS.EndLPTPrinter();
            }
        }
        #endregion
    }
}