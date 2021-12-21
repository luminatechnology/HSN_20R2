using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.CR;
using System.Threading.Tasks;
using HSNHighcareCistomizations.DAC;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Data.EP;
using PX.Objects.DR;
using PX.Data.BQL;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.SO;

namespace HSNHighcareCistomizations.Graph
{
    public class CustomerPINCodeMaint : PXGraph<CustomerPINCodeMaint>
    {
        public PXSave<Customer> Save;
        public PXCancel<Customer> Cancel;

        public PXSelect<
                Customer,
            Where2<
                Match<Current<AccessInfo.userName>>,
                And<Where<BAccount.type, Equal<BAccountType.customerType>,
                    Or<BAccount.type, Equal<BAccountType.combinedType>>>>>> Document;

        public SelectFrom<LUMCustomerPINCode>
               .Where<LUMCustomerPINCode.bAccountID.IsEqual<Customer.bAccountID.FromCurrent>>.View Transaction;

        public PXAction<LUMCustomerPINCode> viewDefSchedule;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewDefSchedule(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<DraftScheduleMaint>();
            graph.Schedule.Current = SelectFrom<DRSchedule>
                                     .Where<DRSchedule.scheduleNbr.IsEqual<P.AsString>>
                                     .View.Select(this, row.ScheduleNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public PXAction<LUMCustomerPINCode> viewSalesOrder;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewSalesOrder(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<SOOrderEntry>();
            graph.Document.Current = SelectFrom<SOOrder>
                                     .Where<SOOrder.orderType.IsEqual<P.AsString>
                                       .And<SOOrder.orderNbr.IsEqual<P.AsString>>>
                                     .View.Select(this, "SO", row.SOOrderNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public PXAction<LUMCustomerPINCode> viewInvoice;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewInvoice(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<SOInvoiceEntry>();
            graph.Document.Current = SelectFrom<ARInvoice>
                                     .Where<ARInvoice.docType.IsEqual<P.AsString>
                                       .And<ARInvoice.refNbr.IsEqual<P.AsString>>>
                                     .View.Select(this, "INV", row.InvoiceNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }


        public virtual void _(Events.RowSelected<LUMCustomerPINCode> e)
        {
            if (e.Row != null)
                this.Transaction.Cache.SetValueExt<LUMCustomerPINCode.serialNbr>(e.Row, LUMPINCodeMapping.PK.Find(this, e.Row.Pin)?.SerialNbr);
        }

        public virtual void _(Events.RowPersisting<LUMCustomerPINCode> e)
        {
            if (e.Row is LUMCustomerPINCode row && row != null && this.Document.Current != null)
            {
                row.BAccountID = this.Document.Current.BAccountID;
                row.StartDate = DateTime.Now;
                row.EndDate = DateTime.Now.AddYears(1).AddDays(-1);
                row.IsActive = Accessinfo.BusinessDate?.Date >= row.StartDate?.Date && Accessinfo.BusinessDate?.Date <= row.EndDate?.Date;
            }
        }
    }

    public class HighcareAttr : PX.Data.BQL.BqlString.Constant<HighcareAttr>
    {
        public HighcareAttr() : base("HIGHCARE") { }
    }
}
