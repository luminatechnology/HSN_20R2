using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System.Collections;
using System.Collections.Generic;

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
            if (CheckRefInfo())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    if (UpdateAppointmentStageManual())
                        ts.Complete();
                }
            }
        }

        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            var baseResult = baseMethod(adapter);
            // Process Appointment & Service Order Stage Change
            if (CheckRefInfo())
            {
                PXLongOperation.WaitCompletion(Base.UID);
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    // Release Success
                    if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.Completed)
                        if (UpdateAppointmentStageManual())
                            ts.Complete();
                }
            }
            return baseResult;
        }
        #endregion

        #region Actions
        public PXAction<INRegister> copyItemFromAppt;
        [PXButton()]
        [PXUIField(DisplayName = "Copy Item From Appt.", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable CopyItemFromAppt(PXAdapter adapter)
        {
            var register = Base.CurrentDocument.Current;

            if (register != null)
            {
                var regisExt = register.GetExtension<INRegisterExt>();

                foreach (FSAppointmentDet row in SelectFrom<FSAppointmentDet>.Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                                                                .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>
                                                                                     .And<FSAppointmentDet.lineType.IsEqual<FSLineType.Inventory_Item>>>>.View.Select(Base, regisExt.UsrSrvOrdType, regisExt.UsrAppointmentNbr))
                {
                    AppointmentEntry_Extension.CreateINTran(Base, row);
                }

                Base.Save.Press();
            }

            return adapter.Get();
        }

        public PXAction<INRegister> PickingListReport;
        [PXButton()]
        [PXUIField(DisplayName = "Print Picking List", Enabled = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        protected virtual IEnumerable pickingListReport(PXAdapter adapter)
        {
            if (Base.transfer.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.transfer.Current.DocType;
                parameters["RefNbr"] = Base.transfer.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM644005", "Report LM644005");
            }
            return adapter.Get();
        }

        public PXAction<INRegister> DeliveryOrderReport;
        [PXButton()]
        [PXUIField(DisplayName = "Print Delivery Order", Enabled = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        protected virtual IEnumerable deliveryOrderReport(PXAdapter adapter)
        {
            if (Base.transfer.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.transfer.Current.DocType;
                parameters["RefNbr"] = Base.transfer.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM644010", "Report LM644010");
            }
            return adapter.Get();
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<INRegister> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            bool activePartRequest = hSNSetup?.EnablePartReqInAppt == true;
            bool activePrintTransferProcess = hSNSetup?.EnablePrintTransferProcess == true;

            copyItemFromAppt.SetEnabled(Base.transactions.Select().Count == 0);
            copyItemFromAppt.SetVisible(activePartRequest);

            PXUIFieldAttribute.SetVisible<INRegisterExt.usrAppointmentNbr>(e.Cache, null, activePartRequest);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrTransferPurp>(e.Cache, null, activePartRequest);
            PXUIFieldAttribute.SetVisible<INTranExt.usrApptLineRef>(Base.transactions.Cache, null, activePartRequest);

            PXUIFieldAttribute.SetVisible<INRegisterExt.usrPLIsPrinted>(e.Cache, null, activePrintTransferProcess);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrDOIsPrinted>(e.Cache, null, activePrintTransferProcess);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrPickingListNumber>(e.Cache, null, activePrintTransferProcess);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrDeliveryOrderNumber>(e.Cache, null, activePrintTransferProcess);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrTrackingNbr>(e.Cache, null, activePrintTransferProcess);
        }
        #endregion

        #region Method

        public bool CheckRefInfo()
        {
            var row = Base.transfer.Current;
            var srvType = row?.GetExtension<INRegisterExt>()?.UsrSrvOrdType;
            var appNbr = row?.GetExtension<INRegisterExt>()?.UsrAppointmentNbr;
            var soRef = row?.GetExtension<INRegisterExt>()?.UsrSORefNbr;
            if (!string.IsNullOrEmpty(soRef) && !string.IsNullOrEmpty(appNbr) && !string.IsNullOrEmpty(srvType))
                return true;
            return false;
        }

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
