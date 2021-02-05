using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.TX;
using PX.Objects.AP;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Graph_Release;
using eGUICustomization4HSN.Descriptor;

namespace PX.Objects.CA
{
    public class CAReleaseProcess_Extension : PXGraphExtension<CAReleaseProcess>
    {
        #region Delegate Functions
        public delegate void OnReleaseCompleteDelegate(ICADocument doc);
        [PXOverride]
        public void OnReleaseComplete(ICADocument doc, OnReleaseCompleteDelegate baseMethod)
        {
            CAAdj cAAdj = doc as CAAdj;

            if (TWNGUIValidation.ActivateTWGUI(Base) == true &&
                cAAdj != null &&
                cAAdj.Released == true &&
                cAAdj.AdjTranType == CATranType.CAAdjustment)
            {
                TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                PXSelectBase<TWNManualGUIBank> ViewManGUIBank = new SelectFrom<TWNManualGUIBank>.Where<TWNManualGUIBank.adjRefNbr.IsEqual<@P.AsString>>.View(Base);

                foreach (TWNManualGUIBank manualGUIBank in ViewManGUIBank.Cache.Cached)
                {
                    if (PXCache<Tax>.GetExtension<TaxExt>(APReleaseProcess_Extension.SelectTax(Base, manualGUIBank.TaxID)).UsrTWNGUI != true) { continue; }

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        rp.CreateGUITrans(new STWNGUITran()
                        {
                            VATCode       = manualGUIBank.VATInCode,
                            GUINbr        = manualGUIBank.GUINbr,
                            GUIStatus     = TWNGUIStatus.Used,
                            BranchID      = Base.CATranCashTrans_Ordered.Current.BranchID,
                            GUIDirection  = TWNGUIDirection.Receipt,
                            GUIDate       = manualGUIBank.GUIDate,
                            GUITitle      = (string)PXSelectorAttribute.GetField(ViewManGUIBank.Cache, manualGUIBank,
                                                                                 typeof(APRegister.vendorID).Name, manualGUIBank.VendorID,
                                                                                 typeof(Vendor.acctName).Name),
                            TaxZoneID     = manualGUIBank.TaxZoneID,
                            TaxCategoryID = manualGUIBank.TaxCategoryID,
                            TaxID         = manualGUIBank.TaxID,
                            TaxNbr        = manualGUIBank.TaxNbr,
                            OurTaxNbr     = manualGUIBank.OurTaxNbr,
                            NetAmount     = manualGUIBank.NetAmt,
                            TaxAmount     = manualGUIBank.TaxAmt,
                            AcctCD        = (string)PXSelectorAttribute.GetField(ViewManGUIBank.Cache, manualGUIBank,
                                                                                 typeof(APRegister.vendorID).Name, manualGUIBank.VendorID,
                                                                                 typeof(Vendor.acctCD).Name),
                            AcctName      = (string)PXSelectorAttribute.GetField(ViewManGUIBank.Cache, manualGUIBank,
                                                                                 typeof(APRegister.vendorID).Name, manualGUIBank.VendorID,
                                                                                 typeof(Vendor.acctName).Name),
                            DeductionCode = manualGUIBank.Deduction,
                            Remark        = manualGUIBank.Remark,
                            OrderNbr      = manualGUIBank.AdjRefNbr
                        });

                        ts.Complete(Base);
                    }
                }
            }

            baseMethod(doc);
        }
        #endregion
    }
}