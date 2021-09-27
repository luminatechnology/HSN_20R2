using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        #region Delegate Method
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            var releaseResult = baseMethod.Invoke(adapter);
            var tranRows = Base.Transactions.Select().RowCast<ARTran>().ToList();
            foreach (var item in tranRows)
                if (item.GetExtension<FSxARTran>().AppointmentID.HasValue && item.GetExtension<FSxARTran>().SOID.HasValue)
                    UpdateAppointmentStageManual(item.GetExtension<FSxARTran>().AppointmentID, item.GetExtension<FSxARTran>().SOID);
            return releaseResult;
        }

        public delegate IEnumerable PrintInvoiceDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable PrintInvoice(PXAdapter adapter, PrintInvoiceDelegate baseMethod)
        {
            try
            {
                baseMethod(adapter);
            }
            catch (Exception ex)
            {
                var prepaymentPrice = Base.Transactions.Select().RowCast<ARTran>().ToList().Where(x => x.CuryUnitPrice < 0).Sum(x => x.CuryUnitPrice) ?? 0;
               ((PXReportRequiredException)ex).Parameters.Add("PrepaymentPrice", prepaymentPrice.ToString());
                throw ex;
            }
            return adapter.Get();
        }

        #endregion

        #region Method
        public bool UpdateAppointmentStageManual(int? _AppointmentID, int? _SOID)
        {
            var srvRow = SelectFrom<FSServiceOrder>.Where<FSServiceOrder.sOID.IsEqual<P.AsInt>>.View.Select(Base, _SOID).RowCast<FSServiceOrder>().FirstOrDefault();
            var srvType = srvRow?.SrvOrdType;
            var appNbr = SelectFrom<FSAppointment>.Where<FSAppointment.appointmentID.IsEqual<P.AsInt>>.View.Select(Base, _AppointmentID).RowCast<FSAppointment>().FirstOrDefault()?.RefNbr;
            var soRef = srvRow?.RefNbr;
            if (string.IsNullOrEmpty(soRef) || string.IsNullOrEmpty(appNbr) || string.IsNullOrEmpty(srvType))
                return false;

            var apptData = FSWorkflowStageHandler.GetCurrentAppointment(srvType, appNbr);
            var srvData = FSWorkflowStageHandler.GetCurrentServiceOrder(srvType, soRef);
            if (apptData == null || srvData == null)
                return false;

            FSWorkflowStageHandler.InitStageList();
            LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();
            // INVOICE01
            autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.INVOICE01), apptData.WFStageID);
            if (autoWFStage != null && autoWFStage.Active == true)
            {
                // update Appointment and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(AppointmentEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(AppointmentEntry), autoWFStage, srvType, appNbr, soRef);

                // update ServiceOrder and Insert log
                FSWorkflowStageHandler.UpdateTargetFormStage(nameof(ServiceOrderEntry), autoWFStage.NextStage, srvType, appNbr, soRef);
                FSWorkflowStageHandler.InsertTargetFormHistory(nameof(ServiceOrderEntry), autoWFStage, srvType, appNbr, soRef);
                return true;
            }
            else
                return false;

        }
        #endregion

    }
}
