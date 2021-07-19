using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System.Collections;
using System.Linq;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {

        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            var baseResult = baseMethod(adapter);
            if(string.IsNullOrEmpty(Base.receipt.Current.TransferNbr))
                return baseResult;
            // Process Appointment & Service Order Stage Change
            PXLongOperation.WaitCompletion(Base.UID);
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                // Release Success
                if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.Completed)
                    if (UpdateAppointmentStageManual())
                        ts.Complete();

            }

            return baseResult;
        }

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
            row.TranDesc = transfer.TranDesc;

            rowExt.UsrSrvOrdType = transferExt.UsrSrvOrdType;
            rowExt.UsrAppointmentNbr = transferExt.UsrAppointmentNbr;
            rowExt.UsrSORefNbr = transferExt.UsrSORefNbr;
            rowExt.UsrTransferPurp = transferExt.UsrTransferPurp;
        }
        #endregion

        #region MyRegion
        public bool UpdateAppointmentStageManual()
        {
            var row = Base.receipt.Current;
            if (row == null)
                return false;

            var transferRow = SelectFrom<INRegister>.Where<INRegister.refNbr.IsEqual<P.AsString>>
                              .View.Select(Base, row.TransferNbr).RowCast<INRegister>().FirstOrDefault();
            // Check Transfer data is Exists
            if (transferRow == null || string.IsNullOrEmpty(row.TransferNbr))
                return false;

            var srvType = transferRow.GetExtension<INRegisterExt>().UsrSrvOrdType;
            var appNbr = transferRow.GetExtension<INRegisterExt>().UsrAppointmentNbr;
            var soRef = transferRow.GetExtension<INRegisterExt>().UsrSORefNbr;
            if (string.IsNullOrEmpty(soRef) || string.IsNullOrEmpty(appNbr) || string.IsNullOrEmpty(srvType))
                return false;

            var apptData = FSWorkflowStageHandler.GetCurrentAppointment(srvType, appNbr);
            var srvData = FSWorkflowStageHandler.GetCurrentServiceOrder(srvType, soRef);
            if (apptData == null || srvData == null)
                return false;

            FSWorkflowStageHandler.InitStageList();
            LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();
            // AWSPARE07
            if (row.Status == INDocStatus.Released && transferRow.Status == INDocStatus.Released && transferRow.TransferType == INTransferType.TwoStep)
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.AWSPARE07), apptData.WFStageID);
            if (autoWFStage != null && autoWFStage.Active == true)
            {
                // update Appointment and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(AppointmentEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(AppointmentEntry), autoWFStage, srvType, appNbr, soRef);

                // update ServiceOrder and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(ServiceOrderEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(ServiceOrderEntry), autoWFStage, srvType, appNbr, soRef);
            }
            return true;
        }
    }
    #endregion
}
