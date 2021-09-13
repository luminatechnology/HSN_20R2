using System.Collections;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AP
{
    public class APPaymentEntry_Extension : PXGraphExtension<APPaymentEntry>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<APPayment> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            bool activePartRequest = hSNSetup?.EnableSCBPaymentFile == true;

            PXUIFieldAttribute.SetVisible<APPaymentExt.usrSCBPaymentExported>(e.Cache, null, activePartRequest);
            PXUIFieldAttribute.SetVisible<APPaymentExt.usrSCBPaymentDateTime>(e.Cache, null, activePartRequest);
        }
        #endregion
    }
}