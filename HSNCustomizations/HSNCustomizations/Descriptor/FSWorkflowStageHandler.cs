using PX.Data;
using PX.Objects.FS;
using HSNCustomizations.DAC;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Linq;
using HSNCustomizations;
using System.Collections.Generic;
using System;
using System.Reflection;
using static PX.Objects.FS.ID;

namespace HSNCustomizations.Descriptor
{
    public class FSWorkflowStageHandler
    {
        public static AppointmentEntry apptEntry = null;
        public static ServiceOrderEntry srvEntry = null;
        public static List<FSWFStage> stageList = null;
        public static bool IsNewDetailRecord = false;
        /*
         * Rule OPEN01 – Change to Open Stage when appointment is created when a new appointment nbr is assigned.
         * Rule ASSIGN01 – Change to Assigned Stage when staff is assigned when the (WFStageID of Appointment=Current Stage or blank) and new record is inserted into table FSAppointmentEmployee.
         * Rule DIAGNOSE01 – Change to Under Diagnose Stage when appointment is started when the (WFStageID of Appointment=Current Stage or blank) and user click ‘START’ to change FSAppointment.status to ‘In Process’.
        */
        public static LUMAutoWorkflowStage AutoWFStageRule(string initForm)
        {
            string ruleID = string.Empty;

            switch (initForm)
            {
                case nameof(AppointmentEntry):
                    var appt = apptEntry.AppointmentRecords.Current;
                    var detail = apptEntry.AppointmentDetails.Cache.Inserted.RowCast<FSAppointmentDet>().ToList();

                    // Valid
                    if (appt == null)
                        return null;

                    // ASSIGN01
                    if (apptEntry.AppointmentServiceEmployees.Select().Count > 0)
                        ruleID = nameof(WFRule.ASSIGN01);

                    // START01
                    if (appt.Status == FSAppointment.status.IN_PROCESS)
                        ruleID = nameof(WFRule.START01);

                    // QUOTATION01
                    if (IsNewDetailRecord)
                        ruleID = nameof(WFRule.QUOTATION01);

                    // FINISH01
                    if (appt.Finished ?? false)
                        ruleID = nameof(WFRule.FINISH01);

                    // COMPLETE01
                    if (appt.Status == FSAppointment.status.COMPLETED)
                        ruleID = nameof(WFRule.COMPLETE01);

                    return LUMAutoWorkflowStage.UK.Find(apptEntry, appt.SrvOrdType, ruleID, appt.WFStageID);
                case nameof(ServiceOrderEntry):
                    var srv = srvEntry.ServiceOrderRecords.Current;
                    // Valid
                    if (srv == null)
                        return null;

                    // ASSIGN01
                    if (srvEntry.ServiceOrderEmployees.Select().Count > 0)
                        ruleID = nameof(WFRule.ASSIGN01);

                    // COMPLETE03
                    if (srv.Status == FSAppointment.status.COMPLETED)
                        ruleID = nameof(WFRule.COMPLETE03);

                    return LUMAutoWorkflowStage.UK.Find(srvEntry, srv.SrvOrdType, ruleID, srv.WFStageID);
            }
            return null;
        }

        /// <summary>
        /// Update appointment workflow stage by parameter.
        /// </summary>
        /// <param name="stageID"></param>
        public static void UpdateWFStageID(string initForm, LUMAutoWorkflowStage autoWFStage)
        {
            switch (initForm)
            {
                case nameof(AppointmentEntry):
                    apptEntry.AppointmentRecords.Cache.SetValue<FSAppointment.wFStageID>(apptEntry.AppointmentRecords.Current, autoWFStage.NextStage);
                    apptEntry.AppointmentRecords.Cache.MarkUpdated(apptEntry.AppointmentRecords.Current);
                    InsertEventHistory(nameof(AppointmentEntry), autoWFStage);

                    // update Serviec Order Stage
                    UpdateTargetFormStage(
                        nameof(ServiceOrderEntry),
                        autoWFStage.NextStage,
                        apptEntry.AppointmentRecords.Current.SrvOrdType,
                        apptEntry.AppointmentRecords.Current.RefNbr,
                        apptEntry.AppointmentRecords.Current.SORefNbr);

                    InsertTargetFormHistory(
                        nameof(ServiceOrderEntry),
                        autoWFStage,
                        apptEntry.AppointmentRecords.Current.SrvOrdType,
                        apptEntry.AppointmentRecords.Current.RefNbr,
                        apptEntry.AppointmentRecords.Current.SORefNbr);

                    break;
                case nameof(ServiceOrderEntry):
                    srvEntry.ServiceOrderRecords.Cache.SetValue<FSServiceOrder.wFStageID>(srvEntry.ServiceOrderRecords.Current, autoWFStage.NextStage);
                    srvEntry.ServiceOrderRecords.Cache.MarkUpdated(srvEntry.ServiceOrderRecords.Current);
                    InsertEventHistory(nameof(ServiceOrderEntry), autoWFStage);

                    // update Serviec Order Stage
                    UpdateTargetFormStage(
                        nameof(AppointmentEntry),
                        autoWFStage.NextStage,
                        srvEntry.ServiceOrderRecords.Current.SrvOrdType,
                        GetAppointmentNbr(srvEntry.ServiceOrderRecords.Current.SOID),
                        srvEntry.ServiceOrderRecords.Current.RefNbr);

                    InsertTargetFormHistory(
                        nameof(AppointmentEntry),
                        autoWFStage,
                        srvEntry.ServiceOrderRecords.Current.SrvOrdType,
                        GetAppointmentNbr(srvEntry.ServiceOrderRecords.Current.SOID),
                        srvEntry.ServiceOrderRecords.Current.RefNbr);
                    break;
            }
        }

        /// <summary>
        /// Create event handler record from parameter and appointment.
        /// </summary>
        /// <param name="autoWFStage"></param>
        public static void InsertEventHistory(string initForm, LUMAutoWorkflowStage autoWFStage)
        {
            switch (initForm)
            {
                case nameof(AppointmentEntry):
                    AppointmentEntry_Extension entryExt = apptEntry.GetExtension<AppointmentEntry_Extension>();
                    var row = apptEntry.AppointmentRecords.Current;
                    if (row != null)
                    {
                        LUMAppEventHistory eventHist = entryExt.EventHistory.Cache.CreateInstance() as LUMAppEventHistory;

                        eventHist.SrvOrdType = row.SrvOrdType;
                        eventHist.ApptRefNbr = row.RefNbr;
                        eventHist.ApptStatus = row.Status;
                        eventHist.SORefNbr = row.SORefNbr;
                        eventHist.WFRule = autoWFStage.WFRule;
                        eventHist.Descr = autoWFStage.Descr;
                        eventHist.FromStage = GetStageName(autoWFStage.CurrentStage);
                        eventHist.ToStage = GetStageName(autoWFStage.NextStage);

                        entryExt.EventHistory.Insert(eventHist);
                    }
                    break;
                case nameof(ServiceOrderEntry):
                    ServiceOrderEntry_Extension srvEntryExt = srvEntry.GetExtension<ServiceOrderEntry_Extension>();
                    var srvRow = srvEntry.ServiceOrderRecords.Current;
                    if (srvRow != null)
                    {
                        LUMSrvEventHistory eventHist = srvEntryExt.EventHistory.Cache.CreateInstance() as LUMSrvEventHistory;
                        eventHist.SrvOrdType = srvRow.SrvOrdType;
                        eventHist.ApptRefNbr = GetAppointmentNbr(srvRow.SOID);
                        eventHist.ApptStatus = srvRow.Status;
                        eventHist.SORefNbr = srvRow.RefNbr;
                        eventHist.WFRule = autoWFStage.WFRule;
                        eventHist.Descr = autoWFStage.Descr;
                        eventHist.FromStage = GetStageName(autoWFStage.CurrentStage);
                        eventHist.ToStage = GetStageName(autoWFStage.NextStage);
                        srvEntryExt.EventHistory.Insert(eventHist);
                    }
                    break;
            }

        }

        /// <summary>
        /// Create event handler record from parameter and appointment.
        /// </summary>
        /// <param name="autoWFStage"></param>
        public static void InsertEventHistoryForStatus(string initForm, string oldStatus, string newStatus)
        {
            switch (initForm)
            {
                case nameof(AppointmentEntry):
                    AppointmentEntry_Extension entryExt = apptEntry.GetExtension<AppointmentEntry_Extension>();
                    var row = apptEntry.AppointmentRecords.Current;
                    if (row != null)
                    {
                        LUMAppEventHistory eventHist = entryExt.EventHistory.Cache.CreateInstance() as LUMAppEventHistory;

                        eventHist.SrvOrdType = row.SrvOrdType;
                        eventHist.ApptRefNbr = row.RefNbr;
                        eventHist.ApptStatus = row.Status;
                        eventHist.SORefNbr = row.SORefNbr;
                        eventHist.WFRule = "Status";
                        eventHist.Descr = "Status Change";
                        eventHist.FromStage = FindStatusConstantName<string>(typeof(ListField_Status_Appointment), oldStatus);
                        eventHist.ToStage = FindStatusConstantName<string>(typeof(ListField_Status_Appointment), newStatus);
                        entryExt.EventHistory.Insert(eventHist);
                    }
                    break;
                case nameof(ServiceOrderEntry):
                    ServiceOrderEntry_Extension srvEntryExt = srvEntry.GetExtension<ServiceOrderEntry_Extension>();
                    var srvRow = srvEntry.ServiceOrderRecords.Current;
                    if (srvRow != null)
                    {
                        LUMSrvEventHistory eventHist = srvEntryExt.EventHistory.Cache.CreateInstance() as LUMSrvEventHistory;
                        eventHist.SrvOrdType = srvRow.SrvOrdType;
                        eventHist.ApptRefNbr = GetAppointmentNbr(srvRow.SOID);
                        eventHist.ApptStatus = srvRow.Status;
                        eventHist.SORefNbr = srvRow.RefNbr;
                        eventHist.WFRule = "Status";
                        eventHist.Descr = "Status Change";
                        eventHist.FromStage = FindStatusConstantName<string>(typeof(Status_ServiceOrder), oldStatus);
                        eventHist.ToStage = FindStatusConstantName<string>(typeof(Status_ServiceOrder), newStatus);
                        srvEntryExt.EventHistory.Insert(eventHist);
                    }
                    break;
            }

        }

        /// <summary>
        /// Update Target Form Stage(Maunal)
        /// </summary>
        /// <param name="targetForm"></param>
        /// <param name="nextStage"></param>
        /// <param name="srvOrderType"></param>
        /// <param name="refNbr"></param>
        public static void UpdateTargetFormStage(string targetForm, int? nextStage, string srvOrderType, string refNbr, string soRefNbr)
        {
            switch (targetForm)
            {
                case nameof(AppointmentEntry):
                    PXUpdate<Set<FSAppointment.wFStageID, Required<FSAppointment.wFStageID>>,
                          FSAppointment,
                          Where<FSAppointment.srvOrdType, Equal<Required<FSAppointment.srvOrdType>>
                                  , And<FSAppointment.refNbr, Equal<Required<FSAppointment.refNbr>>>
                      >>.Update(new PXGraph(), nextStage, srvOrderType, refNbr);
                    break;
                case nameof(ServiceOrderEntry):
                    PXUpdate<Set<FSServiceOrder.wFStageID, Required<FSServiceOrder.wFStageID>>,
                           FSServiceOrder,
                           Where<FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>
                                   , And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>
                       >>.Update(new PXGraph(), nextStage, srvOrderType, soRefNbr);
                    break;
            }
        }

        /// <summary>
        /// Insert Target Form History(Maunal)
        /// </summary>
        /// <param name="targetForm"></param>
        /// <param name="autoWFStage"></param>
        /// <param name="srvOrderType"></param>
        /// <param name="refNbr"></param>
        /// <param name="soRefNbr"></param>
        public static void InsertTargetFormHistory(string targetForm, LUMAutoWorkflowStage autoWFStage, string srvOrderType, string refNbr, string soRefNbr)
        {
            switch (targetForm)
            {
                case nameof(AppointmentEntry):
                    List<PXDataFieldAssign> assigns = new List<PXDataFieldAssign>();
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.srvOrdType>(srvOrderType));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.apptRefNbr>(refNbr));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.sORefNbr>(soRefNbr));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.wFRule>(autoWFStage.WFRule));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.descr>(autoWFStage.Descr));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.fromStage>(GetStageName(autoWFStage.CurrentStage)));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.toStage>(GetStageName(autoWFStage.NextStage)));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.createdByID>(PXAccess.GetUserID()));
                    assigns.Add(new PXDataFieldAssign<LUMAppEventHistory.createdDateTime>(PX.Common.PXTimeZoneInfo.Now));
                    PXDatabase.Insert<LUMAppEventHistory>(assigns.ToArray());
                    break;
                case nameof(ServiceOrderEntry):
                    List<PXDataFieldAssign> srvAssigns = new List<PXDataFieldAssign>();
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.srvOrdType>(srvOrderType));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.apptRefNbr>(refNbr));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.sORefNbr>(soRefNbr));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.wFRule>(autoWFStage.WFRule));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.descr>(autoWFStage.Descr));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.fromStage>(GetStageName(autoWFStage.CurrentStage)));
                    srvAssigns.Add(new PXDataFieldAssign<LUMSrvEventHistory.toStage>(GetStageName(autoWFStage.NextStage)));
                    srvAssigns.Add(new PXDataFieldAssign<LUMAppEventHistory.createdByID>(PXAccess.GetUserID()));
                    srvAssigns.Add(new PXDataFieldAssign<LUMAppEventHistory.createdDateTime>(PX.Common.PXTimeZoneInfo.Now));
                    PXDatabase.Insert<LUMSrvEventHistory>(srvAssigns.ToArray());
                    break;
            }
        }

        /// <summary>
        /// Find Appointment Status Constant Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containingType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FindStatusConstantName<T>(Type containingType, T value)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            foreach (FieldInfo field in containingType.GetFields
                     (BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(T) &&
                    comparer.Equals(value, (T)field.GetValue(null)))
                {
                    return field.Name; // There could be others, of course...
                }
            }
            return null; // Or throw an exception
        }

        public static void InitStageList()
        {
            stageList = SelectFrom<FSWFStage>.View.Select(new PXGraph()).RowCast<FSWFStage>().ToList();
        }

        /// <summary>
        /// Get Stage Description
        /// </summary>
        /// <param name="stageID"></param>
        /// <returns></returns>
        public static string GetStageName(int? stageID)
            => stageList.FirstOrDefault(x => x.WFStageID == stageID)?.WFStageCD;

        /// <summary>
        /// Get Appointment Ref Nbr.
        /// </summary>
        /// <param name="soID"></param>
        /// <returns></returns>
        public static string GetAppointmentNbr(int? soID)
            => SelectFrom<FSAppointment>.Where<FSAppointment.sOID.IsEqual<P.AsInt>>
                .View.Select(new PXGraph(), soID).RowCast<FSAppointment>().FirstOrDefault()?.RefNbr;

        public static FSAppointment GetCurrentAppointment(string srvType, string appNbr)
            => SelectFrom<FSAppointment>
               .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>.And<FSAppointment.refNbr.IsEqual<P.AsString>>>
               .View.Select(new PXGraph(), srvType, appNbr).RowCast<FSAppointment>().FirstOrDefault();

        public static FSAppointment GetCurrentAppointmentBySoRef(string srvType, string soRef)
           => SelectFrom<FSAppointment>
              .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>.And<FSAppointment.soRefNbr.IsEqual<P.AsString>>>
              .View.Select(new PXGraph(), srvType, soRef).RowCast<FSAppointment>().FirstOrDefault();

        public static FSServiceOrder GetCurrentServiceOrder(string srvType, string soRef)
            => SelectFrom<FSServiceOrder>
               .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>.And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
               .View.Select(new PXGraph(), srvType, soRef).RowCast<FSServiceOrder>().FirstOrDefault();

        public static FSServiceOrder GetCurrentServiceOrderByGuid(Guid? noteID)
           => SelectFrom<FSServiceOrder>
              .Where<FSServiceOrder.noteID.IsEqual<P.AsGuid>>
              .View.Select(new PXGraph(), noteID).RowCast<FSServiceOrder>().FirstOrDefault();
    }
}
