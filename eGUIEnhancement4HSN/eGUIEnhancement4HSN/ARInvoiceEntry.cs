using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
    public class ARInvoiceEntry_Extension2 : PXGraphExtension<ARInvoiceEntry_Extension, ARInvoiceEntry>
    {
        #region String Constant Class
        public const string PrnTitleName = "PRNTITLE";
        public class PrnTitleAttr : PX.Data.BQL.BqlString.Constant<PrnTitleAttr> 
        {
            public PrnTitleAttr() : base(PrnTitleName) { }
        }

        public const string PrnPaytName  = "PRNPAYMENT";
        public class PrnPaymentAttr : PX.Data.BQL.BqlString.Constant<PrnPaymentAttr>
        {
            public PrnPaymentAttr() : base(PrnPaytName) { }
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<ARInvoice> e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            bool statusClosed = e.Row.Status.Equals(ARDocStatus.Open) || e.Row.Status.Equals(ARDocStatus.Closed);

            PXUIFieldAttribute.SetVisible<ARRegisterExt2.usrGUITitle>   (e.Cache, null, Base1.activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt2.usrPrnGUITitle>(e.Cache, null, Base1.activateGUI);
            PXUIFieldAttribute.SetVisible<ARRegisterExt2.usrPrnPayment> (e.Cache, null, Base1.activateGUI);
            PXUIFieldAttribute.SetVisible<ARInvoiceExt.usrPaymMethodID> (e.Cache, null, Base1.activateGUI);

            PXUIFieldAttribute.SetEnabled<ARRegisterExt2.usrGUITitle>(e.Cache, e.Row, !statusClosed && !string.IsNullOrEmpty(PXCacheEx.GetExtension<ARRegisterExt>(e.Row).UsrTaxNbr));
        }

        protected void _(Events.FieldUpdated<ARInvoice, ARInvoice.customerID> e, PXFieldUpdated InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as ARInvoice;

            if (Base1.activateGUI && !string.IsNullOrEmpty(PXCacheEx.GetExtension<ARRegisterExt>(row).UsrTaxNbr) && Base.customer.Current != null)
            {
                PXCacheEx.GetExtension<ARRegisterExt2>(row).UsrGUITitle = Base.customer.Current.AcctName;
            }

            foreach (CSAnswers cSAnswers in PXSelectReadonly<CSAnswers,
                                                             Where<CSAnswers.refNoteID, Equal<Required<Customer.noteID>>,
                                                                   And<CSAnswers.attributeID, Equal<PrnTitleAttr>,
                                                                       Or<CSAnswers.attributeID, Equal<PrnPaymentAttr>>>>>.Select(Base, Base.customer.Current.NoteID) )
            {
                if (cSAnswers.AttributeID.Equals(PrnTitleName)) { PXCacheEx.GetExtension<ARRegisterExt2>(row).UsrPrnGUITitle = Convert.ToBoolean(Convert.ToInt32(cSAnswers.Value)); }
                if (cSAnswers.AttributeID.Equals(PrnPaytName))  { PXCacheEx.GetExtension<ARRegisterExt2>(row).UsrPrnPayment  = Convert.ToBoolean(Convert.ToInt32(cSAnswers.Value)); }
            }
        }
        #endregion
    }
}