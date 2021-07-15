using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Common.Collection;
using System.Linq;
using PX.Data.BQL;

namespace PX.Objects.FS
{
    public class AppointmentEntry_Extension : PXGraphExtension<AppointmentEntry>
    {
        #region Selects
        public SelectFrom<LUMAppEventHistory>.Where<LUMAppEventHistory.srvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                    .And<LUMAppEventHistory.apptRefNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>.View EventHistory;
        #endregion

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var isNewData = Base.AppointmentRecords.Cache.Inserted.RowCast<FSAppointment>().Count() > 0;
            var oldStatus = SelectFrom<FSAppointment>
                                .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>
                                    .And<FSAppointment.refNbr.IsEqual<P.AsString>>>
                                 .View.Select(new PXGraph(), Base.AppointmentRecords.Current.SrvOrdType, Base.AppointmentRecords.Current.RefNbr)
                                 .RowCast< FSAppointment>()?.FirstOrDefault()?.Status;
            var nowStatus = Base.AppointmentRecords.Current.Status;
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    FSWorkflowStageHandler.apptEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    if (oldStatus != nowStatus && oldStatus != null)
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(AppointmentEntry),oldStatus,nowStatus);

                    LUMAutoWorkflowStage autoWFStage = isNewData ?
                        LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.OPEN01)) :
                        FSWorkflowStageHandler.AutoWFStageRule(nameof(AppointmentEntry));
                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(AppointmentEntry), autoWFStage);
                    baseMethod();
                    ts.Complete();
                }
            }
            catch (PXException)
            {
                throw;
            }
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<FSAppointment> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            EventHistory.AllowDelete = EventHistory.AllowInsert = EventHistory.AllowUpdate = false;
        }
        #endregion
    }
}