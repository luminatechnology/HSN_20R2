using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.IN;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.StringList;
using eGUICustomization4HSN.Graph_Release;

namespace PX.Objects.AP
{
    public class APReleaseProcess_Extension : PXGraphExtension<APReleaseProcess>
    {
        #region Select
        public SelectFrom<TWNGUITrans>
                          .Where<TWNGUITrans.orderNbr.IsEqual<APInvoice.refNbr.FromCurrent>>.View ViewGUITrans;
        #endregion

        #region Delegate Funcation
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            baseMethod();

            APRegister    doc = Base.APDocument.Current;
            APRegisterExt docExt = PXCache<APRegister>.GetExtension<APRegisterExt>(doc);

            // Check for document and released flag
            if (TWNGUIValidation.ActivateTWGUI(Base) == true &&
                doc != null && 
                doc.Released == true &&
                (doc.DocType.Equals(APDocType.Invoice) || doc.DocType.Equals(APDocType.DebitAdj) ) &&
                !string.IsNullOrEmpty(docExt.UsrGUINO) &&
                !string.IsNullOrEmpty(docExt.UsrVATINCODE)
               )
            {
                if (Base.APTaxTran_TranType_RefNbr == null)
                {
                    throw new PXException(TWMessages.NoInvTaxDtls);
                }

                // Avoid standard logic calling this method twice and inserting duplicate records into TWNGUITrans.
                if (CountExistedRec(Base, docExt.UsrGUINO, docExt.UsrVATINCODE, doc.RefNbr) >= 1) { return; }

                TaxExt taxExt = PXCache<Tax>.GetExtension<TaxExt>(SelectTax(Base, Base.APTaxTran_TranType_RefNbr.Current.TaxID) );

                if (taxExt.UsrTWNGUI.Equals(false) || taxExt.UsrTWNGUI == null) { return; }

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                    TWNGUITrans tWNGUITrans = rp.InitAndCheckOnAP(docExt.UsrGUINO, docExt.UsrVATINCODE);

                    TaxTran xTran = SelectTaxTran(Base, doc.DocType, doc.RefNbr, BatchModule.AP);

                    decimal? netAmt = xTran.TaxableAmt + xTran.RetainedTaxableAmt;
                    decimal? taxAmt = xTran.TaxAmt + xTran.RetainedTaxAmt;

                    rp.CreateGUITrans(new STWNGUITran()
                    {
                        VATCode       = docExt.UsrVATINCODE,
                        GUINbr        = docExt.UsrGUINO,
                        GUIStatus     = TWNGUIStatus.Used,
                        BranchID      = Base.APTran_TranType_RefNbr.Current.BranchID,
                        GUIDirection  = TWNGUIDirection.Receipt,
                        GUIDate       = docExt.UsrGUIDATE,
                        GUITitle      = (string)PXSelectorAttribute.GetField(Base.APDocument.Cache, doc,
                                                                             typeof(APRegister.vendorID).Name, doc.VendorID,
                                                                             typeof(Vendor.acctName).Name),
                        TaxZoneID     = Base.APInvoice_DocType_RefNbr.Current.TaxZoneID,
                        TaxCategoryID = Base.APTran_TranType_RefNbr.Current.TaxCategoryID,
                        TaxID         = xTran.TaxID,
                        TaxNbr        = docExt.UsrTaxID,
                        OurTaxNbr     = docExt.UsrOurTaxID,
                        NetAmount     = netAmt,
                        TaxAmount     = taxAmt,
                        AcctCD        = (string)PXSelectorAttribute.GetField(Base.APDocument.Cache, doc,
                                                                             typeof(APRegister.vendorID).Name, doc.VendorID,
                                                                             typeof(Vendor.acctCD).Name),
                        AcctName      = (string)PXSelectorAttribute.GetField(Base.APDocument.Cache, doc,
                                                                             typeof(APRegister.vendorID).Name, doc.VendorID,
                                                                             typeof(Vendor.acctName).Name),
                        DeductionCode = docExt.UsrDEDUCTION,
                        Remark        = doc.DocDesc,
                        BatchNbr      = doc.BatchNbr,
                        OrderNbr      = doc.RefNbr
                    });

                    if (tWNGUITrans != null)
                    {
                        if (tWNGUITrans.NetAmtRemain < netAmt)
                        {
                            throw new PXException(TWMessages.RemainAmt);
                        }

                        ViewGUITrans.SetValueExt<TWNGUITrans.netAmtRemain>(tWNGUITrans, (tWNGUITrans.NetAmtRemain -= netAmt));
                        ViewGUITrans.SetValueExt<TWNGUITrans.taxAmtRemain>(tWNGUITrans, (tWNGUITrans.TaxAmtRemain -= taxAmt));

                        tWNGUITrans = ViewGUITrans.Update(tWNGUITrans);
                    }

                    // Manually Saving as base code will not call base graph persis.
                    ViewGUITrans.Cache.Persist(PXDBOperation.Insert);
                    ViewGUITrans.Cache.Persist(PXDBOperation.Update);

                    ts.Complete(Base);
                }
            }

            // Triggering after save events.
            ViewGUITrans.Cache.Persisted(false);
        }

        public delegate List<APRegister> ReleaseInvoiceDelegate(JournalEntry je, ref APRegister doc, 
                                                                PXResult<APInvoice, CurrencyInfo, Terms, Vendor> res, 
                                                                bool isPrebooking, out List<INRegister> inDocs);
        [PXOverride]
        public List<APRegister> ReleaseInvoice(JournalEntry je, ref APRegister doc,
                                               PXResult<APInvoice, CurrencyInfo, Terms, Vendor> res,
                                               bool isPrebooking, out List<INRegister> inDocs,
                                               ReleaseInvoiceDelegate baseMethod)
        {
            var ret = baseMethod(je, ref doc, res, isPrebooking, out inDocs);

            if (Base.APTaxTran_TranType_RefNbr.Current != null)
            {
                Tax tax = SelectTax(Base, Base.APTran_TranType_RefNbr.Current.TaxID);

                foreach (GLTran gLTran in je.GLTranModuleBatNbr.Cache.Inserted)
                {
                    if (tax != null && (tax.PurchTaxAcctID.Equals(gLTran.AccountID) || tax.SalesTaxAcctID.Equals(gLTran.AccountID) ))
                    {
                        gLTran.TranDesc = string.Format("{0} / {1}", PXCache<APRegister>.GetExtension<APRegisterExt>(doc).UsrVATINCODE,
                                                                     PXCache<APRegister>.GetExtension<APRegisterExt>(doc).UsrGUINO);
                    }

                    //if (gLTran.ProjectID == ProjectDefaultAttribute.NonProject())
                    //{
                    //    gLTran.ProjectID = doc.ProjectID;
                    //    gLTran.TaskID    = Base.APTran_TranType_RefNbr.Current.TaskID;
                    //}
                }
            }

            return ret;
        }
        #endregion

        #region Static Methods
        public static int CountExistedRec(PXGraph graph, string gUINbr, string gUIFmtCode, string refNbr)
        {
            return SelectFrom<TWNGUITrans>.Where<TWNGUITrans.gUINbr.IsEqual<@P.AsString>
                                                 .And<TWNGUITrans.gUIFormatcode.IsEqual<@P.AsString>>
                                                      .And<TWNGUITrans.orderNbr.IsEqual<@P.AsString>>>
                                          .View.ReadOnly.Select(graph, gUINbr, gUIFmtCode, refNbr).Count;
        }

        public static Tax SelectTax(PXGraph graph, string taxID)
        {
            return SelectFrom<Tax>.Where<Tax.taxID.IsEqual<@P.AsString>>.View.ReadOnly.Select(graph, taxID);
        }

        public static TaxTran SelectTaxTran(PXGraph graph, string tranType, string refNbr, string module)
        {
            return SelectFrom<TaxTran>.Where<TaxTran.tranType.IsEqual<@P.AsString>
                                             .And<TaxTran.refNbr.IsEqual<@P.AsString>>
                                                  .And<TaxTran.module.IsEqual<@P.AsString>>>
                                      .View.ReadOnly.Select(graph, tranType, refNbr, module);
        }
        #endregion
    }
}