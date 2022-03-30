using System;
using PX.Data;

namespace PX.Objects.AP
{
    public class APRegisterExt : PXCacheExtension<PX.Objects.AP.APRegister>
    {
        #region UsrInvoiceDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Invoice Date")]
        [PXDefault()]
        [PXFormula(typeof(IIf<Where<APRegister.origRefNbr, IsNotNull>, APRegister.docDate, IIf<Where<APPayment.docType, Equal<APDocType.check>>, APPayment.adjDate, Null>>))]
        public virtual DateTime? UsrInvoiceDate { get; set; }
        public abstract class usrInvoiceDate : PX.Data.BQL.BqlDateTime.Field<usrInvoiceDate> { }
        #endregion
    }
}