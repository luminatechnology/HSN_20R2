using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.FS;
using PX.Objects.PR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph
{
    public class ClossPrepaymentProcess : PXGraph<ClossPrepaymentProcess>
    {
        #region Action

        public PXCancel<ARPayment> Cancel;

        #endregion

        #region Data View
        public PXProcessingJoin<ARPayment,
                                InnerJoin<FSAdjust, On<ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                                                  And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                                Where<ARPayment.docType, Equal<ARPaymentType.prepayment>, And<ARPayment.status, Equal<OpenAttr>>>,
                                OrderBy<Desc<ARPayment.adjDate>>> PrepaymentList;

        #endregion

        public ClossPrepaymentProcess()
        {
            PrepaymentList.SetProcessDelegate(
               delegate (List<ARPayment> list)
               {
                   GoClosePrepayment(list);
               });
        }

        #region Override DAC

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXFormula(typeof(Sub<ARPayment.curyDocBal, Add<IIf<Where<ARPayment.curyApplAmt, IsNull>, decimal0, ARPayment.curyApplAmt>,
                                                        IIf<Where<ARPayment.curySOApplAmt, IsNull>, decimal0, ARPayment.curySOApplAmt>>>))]
        public virtual void _(Events.CacheAttached<ARPayment.curyUnappliedBal> e) { }

        #endregion

        public static void GoClosePrepayment(List<ARPayment> list)
        {
            ClossPrepaymentProcess graph = PXGraph.CreateInstance<ClossPrepaymentProcess>();
            graph.ClosePrepayment(graph, list);
        }

        public virtual void ClosePrepayment(ClossPrepaymentProcess graph, List<ARPayment> list)
        {
            PXLongOperation.StartOperation(graph, delegate ()
            {
                var _acctid = SelectFrom<PX.Objects.GL.Account>.Where<PX.Objects.GL.Account.accountCD.IsEqual<P.AsString>>.View.Select(this, "999000000").RowCast<PX.Objects.GL.Account>().FirstOrDefault()?.AccountID;
                var _subid = SelectFrom<PX.Objects.GL.Sub>.Where<PX.Objects.GL.Sub.subCD.IsEqual<P.AsString>>.View.Select(this, "000000000").RowCast<PX.Objects.GL.Sub>().FirstOrDefault()?.SubID;
                foreach (var grpList in list.GroupBy(x => x.CustomerID).ToList())
                {
                    try
                    {
                        using (PXTransactionScope ts = new PXTransactionScope())
                        {
                            var invoiceGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
                            // Document
                            var doc = invoiceGraph.Document.Insert((ARInvoice)invoiceGraph.Document.Cache.CreateInstance());
                            doc.DocType = ARInvoiceType.DebitMemo;
                            invoiceGraph.Document.SetValueExt<ARInvoice.customerID>(doc, grpList.Key);
                            invoiceGraph.Document.SetValueExt<ARInvoice.customerLocationID>(doc, grpList.FirstOrDefault().CustomerLocationID);
                            doc.ARAccountID = _acctid;
                            doc.ARSubID = _subid;

                            // Create Details
                            var line = invoiceGraph.Transactions.Insert((ARTran)invoiceGraph.Transactions.Cache.CreateInstance());
                            // CuryUnappliedBal
                            var price = grpList.Sum(x => (x.CuryDocBal ?? 0) - (x.CuryApplAmt ?? 0) - (x.CurySOApplAmt ?? 0));
                            line.Qty = 1;
                            invoiceGraph.Transactions.SetValueExt<ARTran.curyUnitPrice>(line, price);
                            line.AccountID = _acctid;
                            line.SubID = _subid;

                            doc.CuryDocBal = price;
                            doc.DocBal = price;
                            doc.CuryLineTotal = price;
                            doc.LineTotal = price;
                            doc.Hold = false;
                            // Create save
                            invoiceGraph.Save.Press();

                            // Application
                            foreach (var item in grpList)
                            {
                                var adj = invoiceGraph.Adjustments.Insert((ARAdjust2)invoiceGraph.Adjustments.Cache.CreateInstance());

                                adj.CuryAdjdAmt = (item.CuryDocBal ?? 0) - (item.CuryApplAmt ?? 0) - (item.CurySOApplAmt ?? 0);
                                adj.AdjdDocType = invoiceGraph.Document.Current.DocType;
                                adj.CuryDocBal = 0;
                                adj.DocBal = 0;
                                adj.AdjdRefNbr = invoiceGraph.Document.Current.RefNbr;
                                adj.AdjgDocType = item.DocType;
                                adj.AdjgRefNbr = item.RefNbr;
                                adj.AdjNbr = item.AdjCntr;
                                adj.CustomerID = item.CustomerID;
                                adj.AdjdCustomerID = invoiceGraph.Document.Current.CustomerID;
                                adj.AdjdBranchID = invoiceGraph.Document.Current.BranchID;
                                adj.AdjgBranchID = item.BranchID;
                                adj.AdjgCuryInfoID = item.CuryInfoID;
                                adj.AdjdOrigCuryInfoID = invoiceGraph.Document.Current.CuryInfoID;
                                //if LE constraint is removed from payment selection this must be reconsidered
                                adj.AdjdCuryInfoID = invoiceGraph.Document.Current.CuryInfoID;
                                adj.AdjgFinPeriodID = item.FinPeriodID;
                                adj.AdjgTranPeriodID = item.TranPeriodID;
                                adj.PaymentPendingProcessing = false;
                                adj.PaymentReleased = true;
                                adj.IsCCAuthorized = false;
                                adj.IsCCCaptured = false;
                                adj.IsCCPayment = false;
                                adj.PaymentCaptureFailed = false;
                                // trigger selected update event
                                foreach (ARPayment payment in PXSelectReadonly<
                                                               ARPayment,
                                                               Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
                                                                   And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
                                                               .Select(this, adj.AdjgDocType, adj.AdjgRefNbr))
                                {
                                    foreach (ARInvoice invoice in invoiceGraph.ARInvoice_CustomerID_DocType_RefNbr.Select(adj.AdjdCustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
                                    {
                                        InvoiceEntry.CalcBalancesFromInvoiceSide(adj, invoice, payment, invoiceGraph.CurrencyInfo_CuryInfoID, true, false);
                                    }
                                }
                            }
                            // Save
                            invoiceGraph.Save.Press();
                            // release
                            invoiceGraph.release.Press();
                            // commit 
                            ts.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        PXProcessing.SetError(ex.Message);
                    }
                }
            });

        }
    }

    public class OpenAttr : PX.Data.BQL.BqlString.Constant<OpenAttr>
    {
        public OpenAttr() : base("N") { }
    }
}
