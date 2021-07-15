using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {
        #region Event Handlers
        protected void _(Events.FieldUpdated<INRegister.transferNbr> e, PXFieldUpdated baseHandler) 
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as INRegister;
            var rowExt = row.GetExtension<INRegisterExt>();

            INRegister transfer = SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<INDocType.transfer>
                                                               .And<INRegister.refNbr.IsEqual<@P.AsString>>>.View.Select(Base, e.NewValue.ToString());

            INRegisterExt transferExt = transfer.GetExtension<INRegisterExt>();

            row.ExtRefNbr = transfer.ExtRefNbr;
            row.TranDesc  = transfer.TranDesc;

            rowExt.UsrSrvOrdType     = transferExt.UsrSrvOrdType;
            rowExt.UsrAppointmentNbr = transferExt.UsrAppointmentNbr;
            rowExt.UsrSORefNbr       = transferExt.UsrSORefNbr;
            rowExt.UsrTransferPurp   = transferExt.UsrTransferPurp;
        }
        #endregion
    }
}