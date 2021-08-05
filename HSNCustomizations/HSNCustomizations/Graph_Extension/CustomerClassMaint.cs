using PX.Data;
using HSNCustomizations.DAC;

namespace PX.Objects.AR
{
    public class CustomerClassMaint_Extension : PXGraphExtension<CustomerClassMaint>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<CustomerClass> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            PXUIFieldAttribute.SetVisible<CustomerClassExt.usrInvoiceDocType>(e.Cache, null, PXSelectReadonly<LUMHSNSetup>.Select(Base).TopFirst?.EnableChgInvTypeOnBill ?? false);
        }
        #endregion
    }
}