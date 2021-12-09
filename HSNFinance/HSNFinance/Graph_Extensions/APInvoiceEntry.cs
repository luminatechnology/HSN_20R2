using System;
using PX.Data;

namespace PX.Objects.AP
{
    public class APInvoiceEntry_Extensions : PXGraphExtension<PX.Objects.AP.APInvoiceEntry>
    {
        #region Event Handlers
        public virtual void _(Events.FieldUpdated<APRegisterExt.usrInvoiceDate> e)
        {
            var row = e.Row as APInvoice;

            if (row != null && e.NewValue != null)
            {
                DateTime? dueDate, discDate;

                CS.TermsAttribute.CalcTermsDates(CS.Terms.PK.Find(Base, row.TermsID), (DateTime)e.NewValue, out dueDate, out discDate);

                row.DueDate  = dueDate;
                row.DiscDate = discDate;

                e.Cache.SetValueExt(row, "usrGUIDate", e.NewValue);
            }
        }
        #endregion
    }
}
