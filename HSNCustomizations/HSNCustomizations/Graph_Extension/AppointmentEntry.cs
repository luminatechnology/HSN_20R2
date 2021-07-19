using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.FS
{
    public class AppointmentEntry_Extension : PXGraphExtension<AppointmentEntry>
    {
        #region Selects
        public SelectFrom<LUMAppEventHistory>.Where<LUMAppEventHistory.srvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                    .And<LUMAppEventHistory.apptRefNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>.View EventHistory;

        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrAppointmentNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>>.View INRegisterView;
        #endregion

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            Base.menuDetailActions.AddMenuAction(openPartRequest);
            Base.menuDetailActions.AddMenuAction(openPartReceive);
            Base.menuDetailActions.AddMenuAction(openInitiateRMA);
            Base.menuDetailActions.AddMenuAction(openReturnRMA);
        }
        #endregion

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var isNewData = Base.AppointmentRecords.Cache.Inserted.RowCast<FSAppointment>().Count() > 0;
            // Check Status is Dirty
            var statusDirtyResult = CheckStatusIsDirty(Base.AppointmentRecords.Current);
            // Check Stage is Dirty
            var wfStageDirtyResult = CheckWFStageIsDirty(Base.AppointmentRecords.Current);
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    // Init object
                    FSWorkflowStageHandler.apptEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    // insert log if status is change
                    if (statusDirtyResult.IsDirty && !string.IsNullOrEmpty(statusDirtyResult.oldValue))
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(AppointmentEntry), statusDirtyResult.oldValue, statusDirtyResult.newValue);

                    LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();

                    // New Data
                    if(isNewData)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.OPEN01));
                    // Manual Chagne Stage
                    else if (wfStageDirtyResult.IsDirty && wfStageDirtyResult.oldValue.HasValue && wfStageDirtyResult.newValue.HasValue )
                        autoWFStage = new LUMAutoWorkflowStage()
                        {
                            SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType,
                            WFRule = "MANUAL",
                            Active = true,
                            CurrentStage = wfStageDirtyResult.oldValue,
                            NextStage = wfStageDirtyResult.newValue,
                            Descr = "Manual change Stage"
                        };
                    // Workflow
                    else
                        autoWFStage = FSWorkflowStageHandler.AutoWFStageRule(nameof(AppointmentEntry));

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

        [PXUIField(DisplayName = "Close Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable closeAppointment(PXAdapter adapter)
        {
            List<INRegister> list = this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.Status != INDocStatus.Released).ToList();

            if (list.Count > 0) { throw new PXException(HSNMessages.InvtTranNoAllRlsd); }

            Base.closeAppointment.PressButton();

            return adapter.Get();
        }
        #endregion

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBScalar(typeof(Search<INTran.origRefNbr, Where<INTran.docType, Equal<INRegister.docType>,
                                                           And<INTran.refNbr, Equal<INRegister.refNbr>>>>))]
        protected void _(Events.CacheAttached<INRegister.transferNbr> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<FSAppointment> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            EventHistory.AllowDelete = EventHistory.AllowInsert = EventHistory.AllowUpdate = INRegisterView.AllowDelete = INRegisterView.AllowInsert = INRegisterView.AllowUpdate = false;

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            openPartRequest.SetEnabled(hSNSetup?.EnablePartReqInAppt == true);
        }
        #endregion

        #region Actions
        public PXAction<FSAppointmentDet> openPartRequest;
        [PXUIField(DisplayName = HSNMessages.PartRequest, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenPartRequest()
        {
            INTransferEntry transferEntry = PXGraph.CreateInstance<INTransferEntry>();

            InitTransferEntry(ref transferEntry, Base, HSNMessages.PartRequest);

            throw new PXRedirectRequiredException(transferEntry, false, PXSiteMap.Provider.FindSiteMapNodeByScreenID("IN304000").Title) 
            { 
                Mode = PXBaseRedirectException.WindowMode.New 
            };
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Manually insert records into the transfer data view from appointment to open a new window.
        /// </summary>
        /// <param name="transferEntry"></param>
        /// <param name="apptEntry"></param>
        public static void InitTransferEntry(ref INTransferEntry transferEntry, AppointmentEntry apptEntry, string descrType = null)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;

            INRegister register = transferEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
            INRegisterExt regisExt = register.GetExtension<INRegisterExt>();

            register.SiteID            = LUMBranchWarehouse.PK.Find(apptEntry, apptEntry.Accessinfo.BranchID)?.SiteID;
            register.TransferType      = INTransferType.TwoStep;
            register.ExtRefNbr         = appointment.SrvOrdType + " | " + appointment.RefNbr;
            register.TranDesc          = HSNMessages.PartRequest + " | " + appointment.DocDesc;
            regisExt.UsrSrvOrdType     = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr       = appointment.SORefNbr;
            regisExt.UsrTransferPurp   = LUMTransferPurposeType.PartReq;

            transferEntry.CurrentDocument.Insert(register);

            int? toSiteID = null;

            PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

            var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM);

            foreach (FSAppointmentDet row in list)
            {
                CreateINTran(transferEntry, row);

                if (toSiteID == null) { toSiteID = row.SiteID; }
            }

            transferEntry.CurrentDocument.Current.ToSiteID = toSiteID;
            transferEntry.CurrentDocument.UpdateCurrent();
        }

        /// <summary>
        /// Manually insert records into the receipt data view from appointment to open a new window.
        /// </summary>
        /// <param name="receiptEntry"></param>
        /// <param name="apptEntry"></param>
        public static void InitReceiptEntry(ref INReceiptEntry receiptEntry, AppointmentEntry apptEntry, string purposeType, string transferNbr = null)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;

            INRegister    register = receiptEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
            INRegisterExt regisExt = register.GetExtension<INRegisterExt>();

            register.TransferNbr       = transferNbr;
            register.ExtRefNbr         = appointment.SrvOrdType + " | " + apptEntry.ServiceOrderRelated.Current?.CustWorkOrderRefNbr;
            register.TranDesc          = (purposeType.Equals(LUMTransferPurposeType.PartRcv) ? HSNMessages.PartReceive : HSNMessages.RMAInitiated) + " | " + appointment.DocDesc;
            regisExt.UsrSrvOrdType     = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr       = appointment.SORefNbr;
            regisExt.UsrTransferPurp   = purposeType;

            receiptEntry.CurrentDocument.Insert(register);

            PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

            var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM);

            foreach (FSAppointmentDet row in list)
            {
                CreateINTran(receiptEntry, row);
            }
        }

        public static void CreateINTran(PXGraph graph, FSAppointmentDet apptDet)
        {
            INTran iNTran = new INTran()
            {
                InventoryID = apptDet.InventoryID,
                Qty = apptDet.EstimatedQty,
                SiteID = apptDet.SiteID,
                LocationID = apptDet.LocationID
            };

            iNTran = graph.Caches[typeof(INTran)].Insert(iNTran) as INTran;

            iNTran.GetExtension<INTranExt>().UsrApptLineRef = apptDet.LineRef;

            graph.Caches[typeof(INTran)].Update(iNTran);
        }

        /// <summary>
        /// Redirect to the specified form.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="screenID"></param>
        private static void OpenNewForm(PXGraph graph, string screenID)
        {
            throw new PXRedirectRequiredException(graph, false, PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID).Title)
            {
                Mode = PXBaseRedirectException.WindowMode.New
            };
        }
        #endregion

        #region Method

        /// <summary>Check Status Is Drity </summary>
        public (bool IsDirty, string oldValue, string newValue) CheckStatusIsDirty(FSAppointment row)
        {
            if (row == null)
                return (false, string.Empty, string.Empty);

            string oldVale = SelectFrom<FSAppointment>
                               .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>
                                   .And<FSAppointment.refNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                                .RowCast<FSAppointment>()?.FirstOrDefault()?.Status;
            string newValue = row.Status;

            return (!string.IsNullOrEmpty(oldVale) && oldVale != newValue , oldVale, newValue);
        }

        /// <summary>Check Stage Is Dirty </summary>
        public (bool IsDirty, int? oldValue, int? newValue) CheckWFStageIsDirty(FSAppointment row)
        {
            if (row == null)
                return (false, null, null);

            int? oldVale = SelectFrom<FSAppointment>
                               .Where<FSAppointment.srvOrdType.IsEqual<P.AsString>
                                   .And<FSAppointment.refNbr.IsEqual<P.AsString>>>
                                .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                                .RowCast<FSAppointment>()?.FirstOrDefault()?.WFStageID;
            int? newValue = row.WFStageID;

            return (oldVale.HasValue && oldVale != newValue, oldVale, newValue);
        }

        #endregion
    }
}