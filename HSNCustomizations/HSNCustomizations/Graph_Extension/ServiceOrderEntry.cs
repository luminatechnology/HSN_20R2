using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class ServiceOrderEntry_Extension : PXGraphExtension<ServiceOrderEntry>
    {
        #region Selects
        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrSORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>>.View INRegisterView;
        #endregion

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBScalar(typeof(Search<INTran.origRefNbr, Where<INTran.docType, Equal<INRegister.docType>,
                                                           And<INTran.refNbr, Equal<INRegister.refNbr>>>>))]
        protected void _(Events.CacheAttached<INRegister.transferNbr> e) { }
        #endregion
    }
}