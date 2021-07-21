using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR
{
    public class CREmailActivityMaint_Extension : PXGraphExtension<CREmailActivityMaint>
    {
        #region Delegate
        public delegate IEnumerable sendDelegate(PXAdapter adapter);

        [PXOverride]
        public virtual IEnumerable send(PXAdapter adapter, sendDelegate baseMethod)
        {
            var baseResult = baseMethod(adapter);
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (UpdateAppointmentStageManual())
                    ts.Complete();
            }
            return baseResult;
        }

        #endregion

        #region Method

        public bool UpdateAppointmentStageManual()
        {
            var IsSuccess = false;
            var sourRefNoteID = Base.Message.Current.RefNoteID;
            if(sourRefNoteID == null)
                return IsSuccess;
            try
            {
                var srvData = FSWorkflowStageHandler.GetCurrentServiceOrderByGuid(sourRefNoteID);
                if(srvData == null)
                    return IsSuccess;

                var srvType = srvData.SrvOrdType;
                var soRef = srvData.RefNbr;
                var apptData = FSWorkflowStageHandler.GetCurrentAppointmentBySoRef(srvType, soRef);
                if (apptData == null)
                    return IsSuccess;
                var appNbr = apptData.RefNbr;

                FSWorkflowStageHandler.InitStageList();
                LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.QUOTATION03), srvData.WFStageID);
                if (autoWFStage != null && autoWFStage.Active == true)
                {
                    // update Appointment and Insert log
                    FSWorkflowStageHandler.UpdateTargetFormStage(nameof(AppointmentEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                    FSWorkflowStageHandler.InsertTargetFormHistory(nameof(AppointmentEntry), autoWFStage, srvType, appNbr, soRef);

                    // update ServiceOrder and Insert log
                    FSWorkflowStageHandler.UpdateTargetFormStage(nameof(ServiceOrderEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                    FSWorkflowStageHandler.InsertTargetFormHistory(nameof(ServiceOrderEntry), autoWFStage, srvType, appNbr, soRef);
                }
                IsSuccess = true;
            }
            catch (Exception)
            {
               return IsSuccess;
            }
            return IsSuccess;
        }

        #endregion
    }
}
