using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System.Linq;
using System.Collections;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using System.Collections.Generic;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class AppointmentEntry_Extension : PXGraphExtension<AppointmentEntry>
    {
        #region Constant String & Classes
        public const string TransferScr = "IN304000";
        public const string ReceiptScr = "IN301000";
        public const string RMAReqAttr = "RMAREQ";

        public class rMAReqAttrID : PX.Data.BQL.BqlString.Constant<rMAReqAttrID>
        {
            public rMAReqAttrID() : base(RMAReqAttr) { }
        }
        #endregion

        #region Selects
        public SelectFrom<LUMAppEventHistory>.Where<LUMAppEventHistory.srvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                    .And<LUMAppEventHistory.apptRefNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>.View EventHistory;

        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSAppointment.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrAppointmentNbr.IsEqual<FSAppointment.refNbr.FromCurrent>>>>.View INRegisterView;

        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        #endregion

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();

            Base.menuDetailActions.AddMenuAction(openPartRequest);
            Base.menuDetailActions.AddMenuAction(openPartReceive);
            Base.menuDetailActions.AddMenuAction(openInitiateRMA);
            Base.menuDetailActions.AddMenuAction(openReturnRMA);
            FSWorkflowStageHandler.InitStageList();
            AddAllStageButton();
        }
        #endregion

        #region Delegate Method

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.AppointmentRecords.Current.Status != FSAppointment.status.CLOSED && HSNSetupView.Select().TopFirst?.EnableHeaderNoteSync == true)
            {
                SyncNoteApptOrSrvOrd(Base, typeof(FSAppointment), typeof(FSServiceOrder));
            }

            var isNewData = Base.AppointmentRecords.Cache.Inserted.RowCast<FSAppointment>().Count() > 0;
            // Check Status is Dirty
            var statusDirtyResult = CheckStatusIsDirty(Base.AppointmentRecords.Current);
            // Check Stage is Dirty
            var wfStageDirtyResult = CheckWFStageIsDirty(Base.AppointmentRecords.Current);
            // Get New Staff Record
            var newStaffRecords = Base.AppointmentServiceEmployees.Cache.Inserted.RowCast<FSAppointmentEmployee>().ToList();
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    // Init object
                    bool isDriveStaff = false;
                    FSWorkflowStageHandler.apptEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    // insert log if status is change
                    if (statusDirtyResult.IsDirty && !string.IsNullOrEmpty(statusDirtyResult.oldValue))
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(AppointmentEntry), statusDirtyResult.oldValue, statusDirtyResult.newValue);

                    LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();

                    // check staff is driver
                    foreach (var staff in newStaffRecords)
                    {
                        var employee = EPEmployee.PK.Find(Base, staff.EmployeeID);
                        var attr = CSAnswers.PK.Find(Base, employee?.NoteID, "DRIVER");
                        if (attr != null && attr.Value == "1")
                        {
                            isDriveStaff = true;
                            break;
                        }
                    }

                    #region WorkFlower

                    // New Data
                    if (isNewData)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.OPEN01));
                    // Manual Chagne Stage
                    else if (wfStageDirtyResult.IsDirty && wfStageDirtyResult.oldValue.HasValue && wfStageDirtyResult.newValue.HasValue)
                        autoWFStage = new LUMAutoWorkflowStage()
                        {
                            SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType,
                            WFRule = "MANUAL",
                            Active = true,
                            CurrentStage = wfStageDirtyResult.oldValue,
                            NextStage = wfStageDirtyResult.newValue,
                            Descr = "Manual change Stage"
                        };
                    // Staff Drive stage
                    else if (isDriveStaff)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.AppointmentRecords.Current.SrvOrdType, nameof(WFRule.ASSIGN03));
                    // Workflow
                    else
                        autoWFStage = FSWorkflowStageHandler.AutoWFStageRule(nameof(AppointmentEntry));

                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(AppointmentEntry), autoWFStage);

                    #endregion

                    baseMethod();
                    ts.Complete();
                }
            }
            catch (PXException)
            {
                throw;
            }
        }

        public delegate IEnumerable CloseAppointmentDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable CloseAppointment(PXAdapter adapter, CloseAppointmentDelegate baseMethod)
        {
            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Receipt && x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMAInit).Count() <= 0 &&
                Base.AppointmentDetails.Select().RowCast<FSAppointmentDet>().Where(x => x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true).Count() > 0)
            {
                throw new PXException(HSNMessages.NoInitRMARcpt);
            }

            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.Status != INDocStatus.Released).Count() > 0)
            {
                throw new PXException(HSNMessages.InvtTranNoAllRlsd);
            }

            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Transfer && x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMARetu).Count() <= 0)
            {
                throw new PXException(HSNMessages.MustReturnRMA);
            }

            return baseMethod(adapter);
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

            LUMHSNSetup hSNSetup = HSNSetupView.Select();

            openPartRequest.SetEnabled(hSNSetup?.EnablePartReqInAppt == true);
            openPartReceive.SetEnabled(hSNSetup?.EnablePartReqInAppt == true);
            openInitiateRMA.SetEnabled(hSNSetup?.EnableRMAProcInAppt == true);
            openReturnRMA.SetEnabled(hSNSetup?.EnableRMAProcInAppt == true);

            PXUIFieldAttribute.SetVisible<FSAppointmentExt.usrTransferToHQ>(e.Cache, e.Row, hSNSetup?.DisplayTransferToHQ ?? false);
            SettingStageButton();
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

            OpenNewForm(transferEntry, TransferScr);
        }

        public PXAction<FSAppointmentDet> openPartReceive;
        [PXUIField(DisplayName = HSNMessages.PartReceive, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenPartReceive()
        {
            if (this.INRegisterView.Select().RowCast<INRegister>().Where(x => x.DocType == INDocType.Transfer && x.Released == true && x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.PartReq).Count() <= 0)
            {
                throw new PXException(HSNMessages.PartReqNotRlsd);
            }

            string transferNbr = null;

            INReceiptEntry receiptEntry = PXGraph.CreateInstance<INReceiptEntry>();

            foreach (INRegister row in INRegisterView.Select())
            {
                switch (row.DocType)
                {
                    case INDocType.Receipt:
                        if (row.Released == true) { goto BlankReceipt; }
                        break;

                    case INDocType.Transfer:
                        transferNbr = row.Released == true && row.TransferType == INTransferType.TwoStep ? row.RefNbr : null;
                        break;
                }
            }

            InitReceiptEntry(ref receiptEntry, Base, transferNbr);

            BlankReceipt:
            OpenNewForm(receiptEntry, ReceiptScr);
        }

        public PXAction<FSAppointmentDet> openInitiateRMA;
        [PXUIField(DisplayName = HSNMessages.InitiateRMA, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenInitiateRMA()
        {
            INReceiptEntry receiptEntry = PXGraph.CreateInstance<INReceiptEntry>();

            InitReceiptEntry(ref receiptEntry, Base);

            OpenNewForm(receiptEntry, ReceiptScr);
        }

        public PXAction<FSAppointmentDet> openReturnRMA;
        [PXUIField(DisplayName = HSNMessages.ReturnRMA, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenReturnRMA()
        {
            if (new PXView(Base, true, this.INRegisterView.View.BqlSelect).SelectMulti().RowCast<INRegister>().Where(x => x.DocType == INDocType.Receipt &&
                                                                                                                          x.Status == INDocStatus.Released &&
                                                                                                                          x.GetExtension<INRegisterExt>().UsrTransferPurp == LUMTransferPurposeType.RMAInit).Count() <= 0)
            {
                throw new PXException(HSNMessages.ReturnRMAB4Init);
            }

            INTransferEntry transferEntry = PXGraph.CreateInstance<INTransferEntry>();

            InitTransferEntry(ref transferEntry, Base, HSNMessages.RMAReturned);

            OpenNewForm(transferEntry, TransferScr);
        }

        public PXMenuAction<FSAppointment> lumStages;
        [PXUIField(DisplayName = "STAGES", MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true, CommitChanges = true)]
        public virtual void LumStages() { }

        public PXMenuAction<FSAppointment> cleanUpStageButton;
        [PXUIField(DisplayName = "Clean up Button", MapEnableRights = PXCacheRights.Select,Visible = false)]
        [PXButton(CommitChanges = true)]
        public virtual void CleanUpStageButton()
        {
            var btn = Base.Actions["lumStages"].GetState(null) as PXButtonState;
            foreach (ButtonMenu item in btn.Menus)
                Base.Actions[item.Command].SetEnabled(false);
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

            int? prefSiteID = LUMBranchWarehouse.PK.Find(apptEntry, apptEntry.Accessinfo.BranchID)?.SiteID;
            bool isRMA = descrType == HSNMessages.RMAReturned;

            register.TransferType = INTransferType.TwoStep;
            register.ExtRefNbr = appointment.SrvOrdType + " | " + apptEntry.ServiceOrderRelated.Current?.CustWorkOrderRefNbr;
            register.TranDesc = descrType + " | " + appointment.DocDesc;
            regisExt.UsrSrvOrdType = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr = appointment.SORefNbr;
            regisExt.UsrTransferPurp = isRMA ? LUMTransferPurposeType.RMARetu : LUMTransferPurposeType.PartReq;

            transferEntry.CurrentDocument.Insert(register);

            PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

            var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM);

            if (isRMA == true)
            {
                list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM && x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true);
            }

            int? toSiteID = list.FirstOrDefault<FSAppointmentDet>()?.SiteID;

            transferEntry.CurrentDocument.Current.SiteID = isRMA ? toSiteID : prefSiteID;
            transferEntry.CurrentDocument.Current.ToSiteID = isRMA ? prefSiteID : toSiteID;
            transferEntry.CurrentDocument.UpdateCurrent();

            foreach (FSAppointmentDet row in list)
            {
                CreateINTran(transferEntry, row);
            }
        }

        /// <summary>
        /// Manually insert records into the receipt data view from appointment to open a new window.
        /// </summary>
        /// <param name="receiptEntry"></param>
        /// <param name="apptEntry"></param>
        public static void InitReceiptEntry(ref INReceiptEntry receiptEntry, AppointmentEntry apptEntry, string transferNbr = null)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;

            INRegister register = receiptEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
            INRegisterExt regisExt = register.GetExtension<INRegisterExt>();

            register.ExtRefNbr = appointment.SrvOrdType + " | " + apptEntry.ServiceOrderRelated.Current?.CustWorkOrderRefNbr;
            register.TranDesc = (!string.IsNullOrEmpty(transferNbr) ? HSNMessages.PartReceive : HSNMessages.RMAInitiated) + " | " + appointment.DocDesc;
            regisExt.UsrSrvOrdType = appointment.SrvOrdType;
            regisExt.UsrAppointmentNbr = appointment.RefNbr;
            regisExt.UsrSORefNbr = appointment.SORefNbr;
            regisExt.UsrTransferPurp = !string.IsNullOrEmpty(transferNbr) ? LUMTransferPurposeType.PartRcv : LUMTransferPurposeType.RMAInit;

            register = receiptEntry.CurrentDocument.Insert(register);

            if (string.IsNullOrEmpty(transferNbr))
            {
                PXView view = new PXView(apptEntry, true, apptEntry.AppointmentDetails.View.BqlSelect);

                var list = view.SelectMulti().RowCast<FSAppointmentDet>().Where(x => x.LineType == ID.LineType_ALL.INVENTORY_ITEM && x.GetExtension<FSAppointmentDetExt>().UsrRMARequired == true);

                foreach (FSAppointmentDet row in list)
                {
                    CreateINTran(receiptEntry, row, true);
                }
            }
            else
            {
                register.TransferNbr = transferNbr;

                receiptEntry.CurrentDocument.Update(register);
            }
        }

        /// <summary>
        /// Create IN trans record from appointment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="apptDet"></param>
        /// <param name="defective"></param>
        public static void CreateINTran(PXGraph graph, FSAppointmentDet apptDet, bool defective = false)
        {
            INTran iNTran = new INTran()
            {
                InventoryID = apptDet.InventoryID,
                Qty = apptDet.EstimatedQty
            };

            if (defective == true)
            {
                iNTran.SiteID = apptDet.SiteID;
                iNTran.LocationID = apptDet.LocationID;
            }

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

        /// <summary>
        /// Enable Header Note Sync between Service Order and Appointment.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        public static void SyncNoteApptOrSrvOrd(PXGraph graph, System.Type fromType, System.Type toType)
        {
            //string note = PXNoteAttribute.GetNote(graph.Caches[fromType], graph.Caches[fromType].Current);

            //if (!string.IsNullOrEmpty(note))
            //{
            //    PXNoteAttribute.SetNote(graph.Caches[toType], graph.Caches[toType].Current, note);
            //    graph.Caches[toType].Update(graph.Caches[toType].Current);
            //}
            PXNoteAttribute.CopyNoteAndFiles(graph.Caches[fromType], graph.Caches[fromType].Current, graph.Caches[toType], graph.Caches[toType].Current, true, false);

            //graph.Caches[toType].Update(graph.Caches[toType].Current);
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

            return (!string.IsNullOrEmpty(oldVale) && oldVale != newValue, oldVale, newValue);
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

        /// <summary> Add All Stage Button </summary>
        public void AddAllStageButton()
        {
            var primatryView = Base.AppointmentRecords.Cache.GetItemType();
            var list = FSWorkflowStageHandler.stageList.Select(x => new { x.WFStageID, x.WFStageCD }).Distinct();
            var actionLst = new List<PXAction>();
            foreach (var item in list)
            {
                var temp = PXNamedAction.AddAction(Base, primatryView, item.WFStageCD, item.WFStageCD,
                    adapter =>
                    {
                        CleanUpStageButton();
                        var row = Base.AppointmentRecords.Current;
                        if (row != null)
                        {
                            var srvOrderData = FSSrvOrdType.PK.Find(new PXGraph(), row.SrvOrdType);
                            var stageList = FSWorkflowStageHandler.stageList.Where(x => x.WFID == srvOrderData.SrvOrdTypeID);
                            var currStageIDByType = stageList.Where(x => x.WFStageCD == item.WFStageCD).FirstOrDefault().WFStageID;
                            Base.AppointmentRecords.Cache.SetValueExt<FSAppointment.wFStageID>(Base.AppointmentRecords.Current, currStageIDByType);
                            Base.AppointmentRecords.Cache.MarkUpdated(Base.AppointmentRecords.Current);
                            Base.AppointmentRecords.Update(Base.AppointmentRecords.Current);

                            Base.AppointmentRecords.Cache.AllowUpdate = true;
                            Base.AppointmentRecords.Cache.SetStatus(Base.AppointmentRecords.Current, PXEntryStatus.Updated);
                            return Base.Save.Press(adapter);
                        }
                        return adapter.Get();
                    },
                    new PXEventSubscriberAttribute[] { new PXButtonAttribute() { CommitChanges = true } }
                );
                temp.SetEnabled(false);
                actionLst.Add(temp);
            }
            foreach (var a in actionLst)
                this.lumStages.AddMenuAction(a);
        }

        /// <summary> Setting Stage Button Status </summary>
        public void SettingStageButton()
        {
            this.cleanUpStageButton.PressButton();
            var row = Base.AppointmentRecords.Current;
            if (row != null && !string.IsNullOrEmpty(row.SrvOrdType))
            {
                var stageActions = SelectFrom<LumStageControl>
                                   .Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                                        .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                                    .View.Select(Base, row.SrvOrdType, row.WFStageID).RowCast<LumStageControl>().ToList();
                foreach (var item in stageActions)
                    Base.Actions[FSWorkflowStageHandler.GetStageName(item.ToStage)].SetEnabled(true);
            }
        }

        #endregion
    }
}