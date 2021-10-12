using PX.Common;
using PX.Data;
using PX.Objects.AR;
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
        public PXCancel<ARPayment> Cancel;
        public PXProcessingJoin<ARPayment,
                                InnerJoin<FSAdjust, On<ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                                                  And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                                Where<ARPayment.docType, Equal<ARPaymentType.prepayment>, And<ARPayment.openDoc, Equal<True>>>> PrepaymentList;

        public ClossPrepaymentProcess()
        {
            PrepaymentList.SetProcessDelegate(
               delegate (List<ARPayment> list)
               {
                   GoClosePrepayment(list);
               });
        }

        public IEnumerable prepaymentList()
        {
            PXView select = new PXView(this, true, PrepaymentList.View.BqlSelect);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, PXView.Parameters, PXView.Searches,
                PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);
            PXView.StartRow = 0;

            foreach (PXResult<ARPayment, FSAdjust> row in result)
            {
                ARPayment payment = (ARPayment)row;
                payment.CuryUnappliedBal = (payment.CuryDocBal ?? 0) - (payment.CuryApplAmt ?? 0) - (payment.CurySOApplAmt ?? 0);
            }
            return result;
        }

        public static void GoClosePrepayment(List<ARPayment> list)
        {
            ClossPrepaymentProcess graph = PXGraph.CreateInstance<ClossPrepaymentProcess>();
            graph.ClosePrepayment(graph, list);
        }

        public virtual void ClosePrepayment(ClossPrepaymentProcess graph, List<ARPayment> list)
        {
            PXLongOperation.StartOperation(graph, delegate ()
            {
                foreach (var item in list)
                {
                    try
                    {
                        var paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();
                        paymentGraph.Document.Current = paymentGraph.Document.Search<ARPayment.docType,ARPayment.refNbr>(item.DocType,item.RefNbr,item.DocType);
                        paymentGraph.Document.Current.ClosedDate = PXTimeZoneInfo.Now;
                        paymentGraph.Document.Current.ClosedFinPeriodID = $"{PXTimeZoneInfo.Now.Year}{PXTimeZoneInfo.Now.Month:00}";
                        paymentGraph.Document.Current.CuryUnappliedBal = 0;
                        paymentGraph.Document.Current.UnappliedBal = 0;
                        paymentGraph.Document.Current.CuryDocBal = 0;
                        paymentGraph.Document.Current.DocBal = 0;
                        paymentGraph.Document.Current.OpenDoc = false;
                        paymentGraph.Document.Current.Status = PaymentStatus.Closed;
                        paymentGraph.Document.Cache.MarkUpdated(paymentGraph.Document.Current);
                        paymentGraph.Save.Press();
                    }
                    catch (Exception ex)
                    {
                        PXProcessing.SetError(ex.Message);
                    }
                }
            });

        }
    }
}
