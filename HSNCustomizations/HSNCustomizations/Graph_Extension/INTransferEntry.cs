using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN
{
    public class INTransferEntry_Extension : PXGraphExtension<INTransferEntry>
    {
        #region Delegate

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            baseMethod();
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (UpdateAppointmentStageManual())
                    ts.Complete();
            }
        }

        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            var baseResult = baseMethod(adapter);
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

        #endregion

        #region Method

        public bool UpdateAppointmentStageManual()
        {
            var row = Base.transfer.Current;
            if (row == null)
                return false;
            var srvType = row.GetExtension<INRegisterExt>().UsrSrvOrdType;
            var appNbr = row.GetExtension<INRegisterExt>().UsrAppointmentNbr;
            var soRef = row.GetExtension<INRegisterExt>().UsrSORefNbr;
            if (string.IsNullOrEmpty(soRef) || string.IsNullOrEmpty(appNbr) || string.IsNullOrEmpty(srvType))
                return false;
            var apptData = FSWorkflowStageHandler.GetCurrentAppointment(srvType, appNbr);
            var srvData = FSWorkflowStageHandler.GetCurrentServiceOrder(srvType, soRef);
            if (apptData == null || srvData == null)
                return false;

            FSWorkflowStageHandler.InitStageList();
            LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();
            // AWSPARE01
            if (row.Status == INDocStatus.Hold || row.Status == INDocStatus.Balanced)
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.AWSPARE01), apptData.WFStageID);
            // AWSPARE03
            if (row.Status == INDocStatus.Released && row.TransferType == INTransferType.OneStep)
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.AWSPARE03), apptData.WFStageID);
            // AWSPARE05
            if (row.Status == INDocStatus.Released && row.TransferType == INTransferType.TwoStep)
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.AWSPARE05), apptData.WFStageID);
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

        #endregion

    }
}
