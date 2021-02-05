using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNExpGUIInv2BankPro : PXGraph<TWNExpGUIInv2BankPro>
    {
        #region String Class
        public class VATOutCode31 : PX.Data.BQL.BqlString.Constant<VATOutCode31>
        {
            public VATOutCode31() : base(TWGUIFormatCode.vATOutCode31) { }
        }

        public class VATOutCode32 : PX.Data.BQL.BqlString.Constant<VATOutCode32>
        {
            public VATOutCode32() : base(TWGUIFormatCode.vATOutCode32) { }
        }

        public class VATOutCode35 : PX.Data.BQL.BqlString.Constant<VATOutCode35>
        {
            public VATOutCode35() : base(TWGUIFormatCode.vATOutCode35) { }
        }
        #endregion

        #region Features & Setup
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, Equal<False>,
                                  And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                             Or<TWNGUITrans.eGUIExported, IsNull>>,
                                       And<Where<TWNGUITrans.gUIFormatcode, Equal<VATOutCode31>,
                                                 Or<TWNGUITrans.gUIFormatcode, Equal<VATOutCode32>,
                                                    Or<TWNGUITrans.gUIFormatcode, Equal<VATOutCode35>>>>>>>> GUITranProc;
        public PXSetup<TWNGUIPreferences> gUIPreferSetup;
        #endregion

        public const decimal fixedRate = (decimal)1.05;

        #region Constructor
        public TWNExpGUIInv2BankPro()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(Upload);
        }
        #endregion

        #region Functions
        public void Upload(List<TWNGUITrans> tWNGUITrans)
        {
            try
            {
                // Avoid to create empty content file in automation schedule.
                if (tWNGUITrans.Count == 0) { return; }

                string lines = "", fileName = "", segSymbol = "|";

                fileName = gUIPreferSetup.Current.OurTaxNbr + "-InvoiceMD--Paper-" + DateTime.Today.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".txt";

                foreach (TWNGUITrans gUITrans in tWNGUITrans)
                {
                    // File Type
                    lines += "M" + segSymbol;
                    // Bill type
                    lines += GetBillType(gUITrans) + segSymbol;
                    // Invoice No
                    lines += gUITrans.GUINbr + segSymbol;
                    // Invoice Date Time
                    lines += gUITrans.GUIDate.Value.ToString("yyyy/MM/dd HH:mm:ss") + segSymbol;
                    // Allowance Date
                    // Cancel Date
                    lines += segSymbol + GetCancelDate(gUITrans) + segSymbol;
                    // Bill Attribute
                    // Seller Ban
                    lines += segSymbol + gUITrans.OurTaxNbr + segSymbol;
                    // Seller Code
                    lines += segSymbol;
                    // Buyer Ban
                    lines += gUITrans.TaxNbr + segSymbol;
                    // Buyer Code
                    lines += segSymbol;
                    // Buyer CName
                    lines += gUITrans.GUITitle + segSymbol;
                    // Sales Amount
                    lines += GetSalesAmt(gUITrans) + segSymbol;
                    // Tax Type
                    lines += GetTaxType(gUITrans.VATType) + segSymbol;
                    // Tax Rate
                    lines += GetTaxRate(gUITrans.VATType) + segSymbol;
                    // Tax Amount
                    lines += GetTaxAmt(gUITrans) + segSymbol;
                    // Total Amount
                    lines += (gUITrans.NetAmount + gUITrans.TaxAmount).Value + segSymbol;
                    // Health Tax
                    lines += "0" + segSymbol;
                    // Buyer Remark
                    lines += segSymbol;
                    // Main Remark
                    lines += segSymbol;
                    // Order No = Relate Number1
                    lines += gUITrans.OrderNbr + segSymbol;
                    // Relate Number2
                    // Relate Number3
                    // Relate Number4
                    // Relate Number5
                    // Group Mark
                    // Customs Clearance Mark
                    lines += new string(char.Parse(segSymbol), 5) + GetCustomClearance(gUITrans) + segSymbol;
                    // Bonded Area Enum
                    lines += segSymbol;
                    // Random Number
                    lines += (gUITrans.BatchNbr != null) ? gUITrans.BatchNbr.Substring(gUITrans.BatchNbr.Length - 4, 4) : null;
                    lines += segSymbol;
                    // Carrier Type
                    lines += ARReleaseProcess_Extension.GetCarrierType(gUITrans.CarrierID) + segSymbol;
                    // Carrier ID
                    lines += ARReleaseProcess_Extension.GetCarrierID(gUITrans.TaxNbr, gUITrans.CarrierID) + segSymbol;
                    // NPOBAN
                    lines += ARReleaseProcess_Extension.GetNPOBAN(gUITrans.TaxNbr, gUITrans.NPONbr) + segSymbol;
                    // Request Paper
                    lines += gUITrans.B2CPrinted.Equals(true) ? "Y" : "N" + segSymbol;
                    // Void Reason
                    // Project Number Void Approved
                    lines += new string(char.Parse(segSymbol), 2) + "\r\n";

                    foreach (PXResult<ARTran> result in RetrieveAPTran(gUITrans.OrderNbr))
                    {
                        ARTran aRTran = result;

                        string taxCalcMode = SelectFrom<ARRegister>.Where<ARRegister.docType.IsEqual<@P.AsString>
                                                                          .And<ARRegister.refNbr.IsEqual<@P.AsString>>>
                                                                   .View.ReadOnly.Select(this, aRTran.TranType, aRTran.RefNbr).TopFirst.TaxCalcMode;
                        // File Type
                        lines += "D" + segSymbol;
                        // Description
                        lines += aRTran.TranDesc + segSymbol;
                        // Quantity
                        lines += (aRTran.Qty?? 1) + segSymbol;
                        // Unit Price
                        // Amount
                        #region Convert design spec logic to code.
                        //if (aRTran.CuryDiscAmt == 0m)
                        //{
                        //    if (taxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross)
                        //    {
                        //        if (!string.IsNullOrEmpty(gUITrans.TaxNbr))
                        //        {
                        //            lines += aRTran.UnitPrice + segSymbol;
                        //        }
                        //        else
                        //        {
                        //            lines += aRTran.UnitPrice * fixedRate + segSymbol;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (!string.IsNullOrEmpty(gUITrans.TaxNbr))
                        //        {
                        //            lines += aRTran.UnitPrice / fixedRate + segSymbol;
                        //        }
                        //        else
                        //        {
                        //            lines += aRTran.UnitPrice + segSymbol;
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    if (taxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross)
                        //    {
                        //        if (!string.IsNullOrEmpty(gUITrans.TaxNbr))
                        //        {
                        //            lines += aRTran.TranAmt / aRTran.Qty + segSymbol;
                        //        }
                        //        else
                        //        {
                        //            lines += aRTran.TranAmt / aRTran.Qty * fixedRate + segSymbol;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (!string.IsNullOrEmpty(gUITrans.TaxNbr))
                        //        {
                        //            lines += aRTran.TranAmt / aRTran.Qty / fixedRate + segSymbol;
                        //        }
                        //        else
                        //        {
                        //            lines += aRTran.TranAmt / aRTran.Qty + segSymbol;
                        //        }
                        //    }
                        //}
                        #endregion
                        decimal? unitPrice = (aRTran.CuryDiscAmt == 0m) ? aRTran.UnitPrice : (aRTran.TranAmt / aRTran.Qty);
                        decimal? tranAmt   = aRTran.TranAmt;

                        if (string.IsNullOrEmpty(gUITrans.TaxNbr) && taxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross)
                        {
                            unitPrice *= fixedRate;
                            tranAmt   *= fixedRate;
                        }
                        else if (!string.IsNullOrEmpty(gUITrans.TaxNbr) && taxCalcMode == PX.Objects.TX.TaxCalculationMode.Gross)
                        {
                            unitPrice /= fixedRate;
                            tranAmt   /= fixedRate;
                        }
                        lines += string.Format("{0:0.####}", unitPrice) + segSymbol;
                        lines += string.Format("{0:0.####}", tranAmt) + segSymbol;
                        // Unit
                        lines += segSymbol;
                        // Package
                        lines += "0" + segSymbol;
                        // Gift Number 1 (Box)
                        lines += "0" + segSymbol;
                        // Gift Number 2 (Piece)
                        lines += "0" + segSymbol;
                        // Order No
                        lines += gUITrans.OrderNbr;
                        // Buyer Barcode
                        // Buyer Prod No
                        // Seller Prod No
                        // Seller Account No
                        // Seller Shipping No
                        // Remark
                        // Relate Number1
                        // Relate Number2 (Invoice No)
                        // Relate Number3 (Invoice Date)
                        // Relate Number4
                        // Relate Number5
                        lines += new string(char.Parse(segSymbol), 11) + "\r\n";
                    }

                    // The following method is only for voided invoice.
                    if (gUITrans.GUIStatus == TWNGUIStatus.Voided)
                    {
                        CreateVoidedDetailLine(segSymbol, gUITrans.OrderNbr, ref lines);
                    }
                }

                // Total Records
                lines += tWNGUITrans.Count;

                UpdateGUITran(tWNGUITrans);
                UploadFile2FTP(fileName, lines);
            }
            catch (Exception ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }

        public void UpdateGUITran(List<TWNGUITrans> tWNGUITrans)
        {
            foreach (TWNGUITrans trans in tWNGUITrans)
            {
                trans.EGUIExported = true;
                trans.EGUIExportedDateTime = DateTime.UtcNow;

                GUITranProc.Cache.Update(trans);
            }

            this.Actions.PressSave();
        }

        public void UploadFile2FTP(string fileName, string content)
        {
            string message = "Upload Processing Completed";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(gUIPreferSetup.Current.Url + fileName));

            request.Credentials = new NetworkCredential(gUIPreferSetup.Current.UserName, gUIPreferSetup.Current.Password);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);

            request.ContentLength = data.Length;

            Stream requestStream = request.GetRequestStream();

            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            requestStream.Dispose();

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                response.Close();
                /// Close FTP
                request.Abort();

                throw new PXOperationCompletedException(message);
            }
        }
        #endregion

        #region Static Methods
        public static string GetBillType(TWNGUITrans gUITran)
        {
            string billType = null;

            switch (gUITran.GUIFormatcode)
            {
                case TWGUIFormatCode.vATOutCode35:
                case TWGUIFormatCode.vATOutCode31:
                    if (gUITran.GUIStatus == TWNGUIStatus.Used) { billType = "O"; }
                    else if (gUITran.GUIStatus == TWNGUIStatus.Voided) { billType = "C"; }
                    break;

                case TWGUIFormatCode.vATOutCode33:
                    if (gUITran.GUIStatus == TWNGUIStatus.Used) { billType = "A2"; }
                    else if (gUITran.GUIStatus == TWNGUIStatus.Voided) { billType = "D"; }
                    break;
            }

            return billType;
        }

        public static string GetTaxType(string vATType)
        {
            if (vATType == TWNGUIVATType.Five) { return "1"; }
            else if (vATType == TWNGUIVATType.Zero) { return "2"; }
            else { return "3"; }
        }

        public static string GetTaxRate(string vATType)
        {
            return (vATType == TWNGUIVATType.Five) ? "0.05" : "0";
        }

        public static string GetCustomClearance(TWNGUITrans gUITran)
        {
            if (gUITran.CustomType == TWNGUICustomType.NotThruCustom &&
                gUITran.VATType == TWNGUIVATType.Five)
            {
                return "1";
            }
            if (gUITran.CustomType == TWNGUICustomType.ThruCustom &&
                gUITran.VATType == TWNGUIVATType.Zero)
            {
                return "2";
            }
            else
            {
                return null;
            }
        }

        public static string GetCancelDate(TWNGUITrans gUITran)
        {
            return gUITran.GUIStatus.Equals(TWNGUIStatus.Voided) ? gUITran.GUIDate.Value.ToString("yyyyMMdd") : string.Empty;
        }

        public static decimal GetSalesAmt(TWNGUITrans gUITran)
        {
            return string.IsNullOrEmpty(gUITran.TaxNbr) ? (gUITran.NetAmount + gUITran.TaxAmount).Value : gUITran.NetAmount.Value;
        }

        public static decimal GetTaxAmt(TWNGUITrans gUITran)
        {
            return string.IsNullOrEmpty(gUITran.TaxNbr) ? 0 : gUITran.TaxAmount.Value;
        }

        public static void CreateVoidedDetailLine(string segSymbol, string refNbr, ref string lines)
        {
            // File Type
            lines += "D" + segSymbol;
            // Description
            lines += "Service" + segSymbol;
            // Quantity
            lines += "0" + segSymbol;
            // Unit Price
            lines += "0" + segSymbol;
            // Amount
            lines += "0" + segSymbol;
            // Unit
            lines += segSymbol;
            // Package
            lines += "0" + segSymbol;
            // Gift Number 1 (Box)
            lines += "0" + segSymbol;
            // Gift Number 2 (Piece)
            lines += "0" + segSymbol;
            // Order No
            lines += refNbr;
            // Buyer Barcode
            // Buyer Prod No
            // Seller Prod No
            // Seller Account No
            // Seller Shipping No
            // Remark
            // Relate Number1
            // Relate Number2 (Invoice No)
            // Relate Number3 (Invoice Date)
            // Relate Number4
            // Relate Number5
            lines += new string(char.Parse(segSymbol), 11) + "\r\n";
        }
        #endregion

        #region Search Result
        public PXResultset<ARTran> RetrieveAPTran(string orderNbr)
        {
            return SelectFrom<ARTran>.Where<ARTran.refNbr.IsEqual<@P.AsString>
                                            .And<Where<ARTran.lineType.IsNotEqual<SOLineType.discount>
                                                 .Or<ARTran.lineType.IsNull>>>>.View.ReadOnly.Select(this, orderNbr);
        }
        #endregion
    }
}