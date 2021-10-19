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

            bool activePartRequest_SCB = hSNSetup?.EnableSCBPaymentFile == true;
            PXUIFieldAttribute.SetVisible<APPaymentExt.usrSCBPaymentExported>(e.Cache, null, activePartRequest_SCB);
            PXUIFieldAttribute.SetVisible<APPaymentExt.usrSCBPaymentDateTime>(e.Cache, null, activePartRequest_SCB);

            bool activePartRequest_Citi = hSNSetup?.EnableCitiPaymentFile == true;
            PXUIFieldAttribute.SetVisible<APPaymentExt.usrCitiPaymentExported>(e.Cache, null, activePartRequest_Citi);
            PXUIFieldAttribute.SetVisible<APPaymentExt.usrCitiPaymentDateTime>(e.Cache, null, activePartRequest_Citi);
        }
        #endregion
    }
}