using PX.Data;
using PX.Objects.FS;
using HSNCustomizations.DAC;

namespace HSNCustomizations.Descriptor
{
    public class FSWorkflowStageHandler
    {
        public static AppointmentEntry apptEntry;
        /*
         * Rule OPEN01 – Change to Open Stage when appointment is created when a new appointment nbr is assigned.
         * Rule ASSIGN01 – Change to Assigned Stage when staff is assigned when the (WFStageID of Appointment=Current Stage or blank) and new record is inserted into table FSAppointmentEmployee.
         * Rule DIAGNOSE01 – Change to Under Diagnose Stage when appointment is started when the (WFStageID of Appointment=Current Stage or blank) and user click ‘START’ to change FSAppointment.status to ‘In Process’.
        */
        public static LUMAutoWorkflowStage AutoWFStageRule()
        {
            string ruleID = string.Empty;

            var appt = apptEntry.AppointmentRecords.Current;

            if (appt.Status == FSAppointment.status.IN_PROCESS)
            {
                ruleID = nameof(WFRule.DIAGNOSE01);
            }
            else if (apptEntry.AppointmentServiceEmployees.Select().Count > 0)
            {
                ruleID = nameof(WFRule.ASSIGN01);
            }
            else if (appt.Status == FSAppointment.status.MANUAL_SCHEDULED)
            {
                ruleID = nameof(WFRule.OPEN01);
            }

            return LUMAutoWorkflowStage.UK.Find(apptEntry, appt.SrvOrdType, ruleID, appt.WFStageID);
        }

        /// <summary>
        /// Update appointment workflow stage by parameter.
        /// </summary>
        /// <param name="stageID"></param>
        public static void UpdateWFStageID(int? stageID)
        {
            apptEntry.AppointmentRecords.Cache.SetValue<FSAppointment.wFStageID>(apptEntry.AppointmentRecords.Current, stageID);
            apptEntry.AppointmentRecords.Cache.MarkUpdated(apptEntry.AppointmentRecords.Current);
        }

        /// <summary>
        /// Create event handler record from parameter and appointment.
        /// </summary>
        /// <param name="autoWFStage"></param>
        public static void InsertEventHistory(LUMAutoWorkflowStage autoWFStage)
        {
            AppointmentEntry_Extension entryExt = apptEntry.GetExtension<AppointmentEntry_Extension>();

            var row = apptEntry.AppointmentRecords.Current;

            if (row != null)
            {
                LUMAppEventHistory eventHist = entryExt.EventHistory.Cache.CreateInstance() as LUMAppEventHistory;

                eventHist.SrvOrdType = row.SrvOrdType;
                eventHist.ApptRefNbr = row.RefNbr;
                eventHist.ApptStatus = row.Status;
                eventHist.SORefNbr   = row.SORefNbr;
                eventHist.WFRule     = autoWFStage.WFRule;
                eventHist.Descr      = autoWFStage.Descr;
                eventHist.FromStage  = autoWFStage.CurrentStage;
                eventHist.ToStage    = autoWFStage.NextStage;

                entryExt.EventHistory.Insert(eventHist);
            }
        }
    }
}
