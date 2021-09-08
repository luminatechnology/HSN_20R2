using System;
using PX.Data;

namespace PX.Objects.AP
{
    public class APPaymentExt : PXCacheExtension<APPayment>
    {
        #region UsrSCBPaymentDateTime
        [PXDBDate()]
        [PXUIField(DisplayName = "SCB Payment DateTime", Enabled = false)]
        public virtual DateTime? UsrSCBPaymentDateTime { get; set; }
        public abstract class usrSCBPaymentDateTime : PX.Data.BQL.BqlDateTime.Field<usrSCBPaymentDateTime> { }
        #endregion

        #region UsrSCBPaymentExported
        [PXDBBool()]
        [PXUIField(DisplayName = "SCB Payment Exported", Enabled = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrSCBPaymentExported { get; set; }
        public abstract class usrSCBPaymentExported : PX.Data.BQL.BqlBool.Field<usrSCBPaymentExported> { }
        #endregion
    }
}
