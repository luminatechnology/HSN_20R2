using System;
using PX.Data;

namespace PX.Objects.AP
{
    public class APPaymentExt : PXCacheExtension<APPayment>
    {
        #region Selected
        [PXBool()]
        [PXUIField(DisplayName = "Selected", Visible = false)]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region UsrSCBPaymentDateTime
        [PXDBDate(PreserveTime = true, InputMask = "g")]
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
