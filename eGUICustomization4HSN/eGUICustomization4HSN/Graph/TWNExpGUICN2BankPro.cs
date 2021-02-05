using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNExpGUICN2BankPro : PXGraph<TWNExpGUICN2BankPro>
    {
        #region Process & Setup
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, Equal<False>,
                                  And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                             Or<TWNGUITrans.eGUIExported, IsNull>>,
                                      And<TWNGUITrans.gUIFormatcode, Equal<ARRegisterExt.VATOut33Att>>>>> GUITranProc;
        public PXSetup<TWNGUIPreferences> gUIPreferSetup;
        #endregion

        #region Constructor
        public TWNExpGUICN2BankPro()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(Upload);
            //GUITranProc.SetProcessDelegate<TWNExpGUICN2BankPro>(delegate(TWNExpGUICN2BankPro graph, TWNGUITrans trans)
            //{
            //    try
            //    {
            //        graph.Clear();
            //        graph.Upload(trans);
            //    }
            //    catch (Exception e)
            //    {
            //        PXProcessing<TWNGUITrans>.SetError(e);
            //    }
            //});
        }
        #endregion

        #region Function
        public void Upload(List<TWNGUITrans> tWNGUITrans)
        {
            try
            {
                // Avoid to create empty content file in automation schedule.
                if (tWNGUITrans.Count == 0) { return; }

                string lines = "", fileName = "", segSymbol = "|";

                fileName = gUIPreferSetup.Current.OurTaxNbr + "-AllowanceMD--Paper-" + DateTime.Today.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".txt";

                TWNExpGUIInv2BankPro graph = PXGraph.CreateInstance<TWNExpGUIInv2BankPro>();

                foreach (TWNGUITrans gUITrans in tWNGUITrans)
                {
                    // File Type
                    lines += "M" + segSymbol;
                    // Bill type
                    lines += TWNExpGUIInv2BankPro.GetBillType(gUITrans) + segSymbol;
                    // Invoice No
                    lines += segSymbol;
                    // Invoice Date Time
                    lines += segSymbol;
                    // Allowance Date
                    lines += gUITrans.GUIDate.Value.ToString("yyyyMMdd") + segSymbol;
                    // Cancel Date
                    lines += TWNExpGUIInv2BankPro.GetCancelDate(gUITrans) + segSymbol;
                    // Bill Attribute
                    lines += segSymbol;
                    // Seller Ban
                    lines += gUITrans.OurTaxNbr + segSymbol;
                    // Seller Code
                    lines += segSymbol;
                    // Buyer Ban
                    lines += gUITrans.TaxNbr + segSymbol;
                    // Buyer Code
                    lines += segSymbol;
                    // Buyer CName
                    lines += gUITrans.GUITitle + segSymbol;
                    // Sales Amount
                    lines += TWNExpGUIInv2BankPro.GetSalesAmt(gUITrans) + segSymbol;
                    // Tax Type
                    lines += TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + segSymbol;
                    // Tax Rate
                    lines += TWNExpGUIInv2BankPro.GetTaxRate(gUITrans.VATType) + segSymbol;
                    // Tax Amount
                    lines += TWNExpGUIInv2BankPro.GetTaxAmt(gUITrans) + segSymbol;
                    // Total Amount
                    lines += (gUITrans.NetAmount + gUITrans.TaxAmount).Value + segSymbol;
                    // Health Tax
                    lines += "0" + segSymbol;
                    // Buyer Remark
                    lines += segSymbol;
                    // Main Remark
                    lines += segSymbol;
                    // Order No = Relate Number1
                    lines += (gUITrans.OrderNbr.Length > 16) ? gUITrans.OrderNbr.Substring(0, 16) : gUITrans.OrderNbr + segSymbol;
                    // Relate Number2                           
                    // Relate Number3
                    // Relate Number4
                    // Relate Number5
                    // Group Mark
                    // Customs Clearance Mark
                    lines += new string(char.Parse(segSymbol), 5) + TWNExpGUIInv2BankPro.GetCustomClearance(gUITrans) + segSymbol;
                    // Bonded Area Enum
                    lines += segSymbol;
                    // Random Number
                    lines += (gUITrans.BatchNbr != null) ? gUITrans.BatchNbr.Substring(0, 4) : null;
                    // Carrier Type
                    // Carrier ID
                    // NPOBAN
                    // Request Paper
                    // Void Reason
                    // Project Number Void Approved
                    lines += new string(char.Parse(segSymbol), 6) + "\r\n";

                    foreach (PXResult<ARTran> result in graph.RetrieveAPTran(gUITrans.OrderNbr))
                    {
                        ARTran aRTran = result;

                        // File Type
                        lines += "D" + segSymbol;
                        // Description
                        lines += aRTran.TranDesc + segSymbol;
                        // Quantity
                        lines += aRTran.Qty + segSymbol;
                        // Unit Price
                        // Amount
                        if (gUITrans.TaxNbr != null)
                        {
                            lines += aRTran.UnitPrice + segSymbol;
                            lines += aRTran.TranAmt + segSymbol;
                        }
                        else
                        {
                            lines += (aRTran.UnitPrice * TWNExpGUIInv2BankPro.fixedRate) + segSymbol;
                            lines += (aRTran.TranAmt * TWNExpGUIInv2BankPro.fixedRate) + segSymbol;
                        }
                        // Unit
                        lines += segSymbol;
                        // Package
                        lines += "0" + segSymbol;
                        // Gift Number 1 (Box)
                        lines += "0" + segSymbol;
                        // Gift Number 2 (Piece)
                        lines += "0" + segSymbol;
                        // Order No
                        lines += (gUITrans.OrderNbr.Length > 16) ? gUITrans.OrderNbr.Substring(0, 16) : gUITrans.OrderNbr + segSymbol;
                        // Buyer Barcode
                        // Buyer Prod No
                        // Seller Prod No
                        // Seller Account No
                        // Seller Shipping No
                        // Remark
                        // Relate Number1
                        // Relate Number2 (Invoice No)
                        lines += new string(char.Parse(segSymbol), 7) + gUITrans.GUINbr + segSymbol;
                        // Relate Number3 (Invoice Date)
                        // Relate Number4
                        // Relate Number5
                        lines += gUITrans.GUIDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                        lines += new string(char.Parse(segSymbol), 2) + "\r\n";
                    }

                    // The following method is only for voided invoice.
                    if (gUITrans.GUIStatus == TWNGUIStatus.Voided)
                    {
                        TWNExpGUIInv2BankPro.CreateVoidedDetailLine(segSymbol, gUITrans.OrderNbr, ref lines);
                    }
                }

                // Total Records
                lines += tWNGUITrans.Count;

                graph.UpdateGUITran(tWNGUITrans);
                graph.UploadFile2FTP(fileName, lines);
            }
            catch (Exception ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }
        #endregion
    }
}