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

        #region UsrCitiPaymentExported
        [PXDBBool()]
        [PXUIField(DisplayName = "Citi Payment Exported", Enabled = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrCitiPaymentExported { get; set; }
        public abstract class usrCitiPaymentExported : PX.Data.BQL.BqlBool.Field<usrCitiPaymentExported> { }
        #endregion

        #region UsrCitiPaymentDateTime
        [PXDBDate(PreserveTime = true, InputMask = "g")]
        [PXUIField(DisplayName = "Citi Payment DateTime", Enabled = false)]
        public virtual DateTime? UsrCitiPaymentDateTime { get; set; }
        public abstract class usrCitiPaymentDateTime : PX.Data.BQL.BqlDateTime.Field<usrCitiPaymentDateTime> { }
        #endregion

        #region UsrBankSwiftAttributes
        [PXString(255)]
        [PXUIField(DisplayName = "Bank Swift Code", Enabled = false)]
        public virtual string UsrBankSwiftAttributes { get; set; }
        public abstract class usrBankSwiftAttributes : PX.Data.BQL.BqlString.Field<usrBankSwiftAttributes> { }
        #endregion

        #region UsrBankAccountNbrAttributes
        [PXString(255)]
        [PXUIField(DisplayName = "Bank Account Number", Enabled = false)]
        public virtual string UsrBankAccountNbr { get; set; }
        public abstract class usrBankAccountNbr : PX.Data.BQL.BqlString.Field<usrBankAccountNbr> { }
        #endregion
    }
}
