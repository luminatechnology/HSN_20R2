using PX.Data;

namespace PX.Objects.AR
{
    public class CustomerClassExt : PXCacheExtension<PX.Objects.AR.CustomerClass>
    {
        #region UsrInvoiceDocType
        [PXDBString(ARRegister.docType.Length, IsFixed = true)]
        [ARDocType.List()]
        [PXUIField(DisplayName = "Invoice Type")]
        [PXDefault(ARDocType.Invoice, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrInvoiceDocType { get; set; }
        public abstract class usrInvoiceDocType : PX.Data.BQL.BqlString.Field<usrInvoiceDocType> { }
        #endregion
    }
}