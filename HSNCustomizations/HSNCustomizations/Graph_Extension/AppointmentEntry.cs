using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

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
            baseMethod();

            try
            {
                using (PXTransactionScope ts = new PXTransactionScope() )
                {
                    FSWorkflowStageHandler.apptEntry = Base;

                    LUMAutoWorkflowStage autoWFStage = FSWorkflowStageHandler.AutoWFStageRule();

                    if (autoWFStage != null && autoWFStage.Active == true)
                    {
                        FSWorkflowStageHandler.UpdateWFStageID(autoWFStage.NextStage);
                        FSWorkflowStageHandler.InsertEventHistory(autoWFStage);
                    }

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