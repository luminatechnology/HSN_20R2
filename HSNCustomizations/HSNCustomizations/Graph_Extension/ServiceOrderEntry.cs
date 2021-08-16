﻿using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System.Linq;
using PX.Objects.IN;
using PX.Objects.CR;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class ServiceOrderEntry_Extension : PXGraphExtension<ServiceOrderEntry>
    {
        #region Selects
        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrSORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>>.View INRegisterView;

        public SelectFrom<LUMSrvEventHistory>.Where<LUMSrvEventHistory.srvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                    .And<LUMSrvEventHistory.sORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>.View EventHistory;

        public CRActivityListReadonly<FSAppointment> Activities;
        #endregion

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();
            FSWorkflowStageHandler.InitStageList();
            AddAllStageButton();
        }
        #endregion

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.ServiceOrderRecords.Current != null &&
                Base.ServiceOrderRecords.Current.Status != FSAppointment.status.CLOSED && 
                SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableHeaderNoteSync == true)
            {
                AppointmentEntry_Extension.SyncNoteApptOrSrvOrd(Base, typeof(FSServiceOrder), typeof(FSAppointment));
            }

            var isNewData = Base.ServiceOrderRecords.Cache.Inserted.RowCast<FSServiceOrder>().Count() > 0;
            // Check Status is Dirty
            var statusDirtyResult = CheckStatusIsDirty(Base.ServiceOrderRecords.Current);
            // Check Stage is Dirty
            var wfStageDirtyResult = CheckWFStageIsDirty(Base.ServiceOrderRecords.Current);
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    FSWorkflowStageHandler.srvEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    // insert log if status is change
                    if (statusDirtyResult.IsDirty && !string.IsNullOrEmpty(statusDirtyResult.oldValue))
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(ServiceOrderEntry), statusDirtyResult.oldValue, statusDirtyResult.newValue);

                    LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();

                    // New Data
                    if (isNewData)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.ServiceOrderRecords.Current.SrvOrdType, nameof(WFRule.OPEN01));
                    // Manual Chagne Stage
                    else if (wfStageDirtyResult.IsDirty && wfStageDirtyResult.oldValue.HasValue && wfStageDirtyResult.newValue.HasValue)
                        autoWFStage = new LUMAutoWorkflowStage()
                        {
                            SrvOrdType = Base.ServiceOrderRecords.Current.SrvOrdType,
                            WFRule = "MANUAL",
                            Active = true,
                            CurrentStage = wfStageDirtyResult.oldValue,
                            NextStage = wfStageDirtyResult.newValue,
                            Descr = "Manual change Stage"
                        };
                    // Workflow
                    else
                        autoWFStage = FSWorkflowStageHandler.AutoWFStageRule(nameof(ServiceOrderEntry));
                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(ServiceOrderEntry), autoWFStage);

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

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBScalar(typeof(Search<INTran.origRefNbr, Where<INTran.docType, Equal<INRegister.docType>,
                                                           And<INTran.refNbr, Equal<INRegister.refNbr>>>>))]
        protected void _(Events.CacheAttached<INRegister.transferNbr> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<FSServiceOrder> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            bool activeWFStageCtrl = hSNSetup?.EnableWFStageCtrlInAppt == true;

            lumStages.SetVisible(activeWFStageCtrl);

            Activities.AllowSelect = hSNSetup?.DispApptActiviteInSrvOrd ?? false;
            EventHistory.AllowSelect = activeWFStageCtrl;
            INRegisterView.AllowSelect = hSNSetup?.EnablePartReqInAppt == true;

            SettingStageButton();
        }
        #endregion

        #region Action
        public PXMenuAction<FSServiceOrder> lumStages;
        [PXUIField(DisplayName = "STAGES", MapEnableRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true, CommitChanges = true)]
        public virtual void LumStages() { }
        #endregion

        #region Methods
        /// <summary>Check Status Is Drity </summary>
        public (bool IsDirty, string oldValue, string newValue) CheckStatusIsDirty(FSServiceOrder row)
        {
            if (row == null)
                return (false, string.Empty, string.Empty);

            var oldVale = SelectFrom<FSServiceOrder>
                             .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                 .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                              .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                              .RowCast<FSServiceOrder>()?.FirstOrDefault()?.Status;
            var newValue = row.Status;

            return (!string.IsNullOrEmpty(oldVale) && oldVale != newValue, oldVale, newValue);
        }

        /// <summary>Check Stage Is Dirty </summary>
        public (bool IsDirty, int? oldValue, int? newValue) CheckWFStageIsDirty(FSServiceOrder row)
        {
            if (row == null)
                return (false, null, null);

            var oldVale = SelectFrom<FSServiceOrder>
                             .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                 .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                              .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                              .RowCast<FSServiceOrder>()?.FirstOrDefault()?.WFStageID;
            var newValue = row.WFStageID;

            return (oldVale.HasValue && oldVale != newValue, oldVale, newValue);
        }

        /// <summary> Add All Stage Button </summary>
        public void AddAllStageButton()
        {
            var primatryView = Base.ServiceOrderRecords.Cache.GetItemType();
            var list = FSWorkflowStageHandler.stageList.Select(x => new { x.WFStageID, x.WFStageCD }).Distinct();
            var actionLst = new List<PXAction>();
            foreach (var item in list)
            {
                var temp = PXNamedAction.AddAction(Base, primatryView, item.WFStageCD, item.WFStageCD,
                    adapter =>
                    {
                        var row = Base.ServiceOrderRecords.Current;
                        if (row != null)
                        {
                            var srvOrderData = FSSrvOrdType.PK.Find(new PXGraph(), row.SrvOrdType);
                            var stageList = FSWorkflowStageHandler.stageList.Where(x => x.WFID == srvOrderData.SrvOrdTypeID);
                            var currStageIDByType = stageList.Where(x => x.WFStageCD == item.WFStageCD).FirstOrDefault().WFStageID;
                            Base.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.wFStageID>(Base.ServiceOrderRecords.Current, currStageIDByType);
                            Base.ServiceOrderRecords.Cache.MarkUpdated(Base.ServiceOrderRecords.Current);
                            Base.ServiceOrderRecords.Update(Base.ServiceOrderRecords.Current);
                            Base.Persist();
                        }
                        return adapter.Get();
                    },
                    new PXEventSubscriberAttribute[] { new PXButtonAttribute() { CommitChanges = true } }
                );
                actionLst.Add(temp);
            }
            foreach (var a in actionLst)
                this.lumStages.AddMenuAction(a);
        }

        /// <summary> Setting Stage Button Status </summary>
        public void SettingStageButton()
        {
            var row = Base.ServiceOrderRecords.Current;

            if (row != null && !string.IsNullOrEmpty(row.SrvOrdType))
            {
                List<PXResult<LumStageControl>> lists = SelectFrom<LumStageControl>.Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                                                                                          .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                                                                                   .View.Select(Base, row.SrvOrdType, row.WFStageID).ToList();

                var btn = this.lumStages.GetState(null) as PXButtonState;

                if (btn.Menus != null)
                {
                    foreach (ButtonMenu btnMenu in btn.Menus)
                    {
                        this.lumStages.SetVisible(btnMenu.Command, lists.Exists(x => FSWorkflowStageHandler.GetStageName(x.GetItem<LumStageControl>().ToStage) == btnMenu.Command));
                    }
                }
            }
        }
        #endregion
    }
}