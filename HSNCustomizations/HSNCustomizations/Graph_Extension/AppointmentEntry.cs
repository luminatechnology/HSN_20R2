using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Common.Collection;
using System.Linq;
using PX.Data.BQL;
using System.Collections;
using System.Collections.Generic;

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
        }
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

            bool enabled = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnablePartReqInAppt == true;

            openPartRequest.SetEnabled(enabled);
        }
        #endregion

        #region Actions
        public PXAction<FSAppointmentDet> openPartRequest;
        [PXUIField(DisplayName = HSNMessages.PartRequest, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenPartRequest()
        {
            //if (Base.AppointmentDetails.Current.LineType != ID.LineType_ALL.INVENTORY_ITEM)
            //{
            //    throw new PXSetPropertyException(HSNMessages.ApptLineTypeInvt);
            //}

            INTransferEntry transferEntry = PXGraph.CreateInstance<INTransferEntry>();

            InitTransferEntry(ref transferEntry, Base);

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
        public static void InitTransferEntry(ref INTransferEntry transferEntry, AppointmentEntry apptEntry)
        {
            FSAppointment appointment = apptEntry.AppointmentSelected.Current;

            INRegister    register = transferEntry.CurrentDocument.Cache.CreateInstance() as INRegister;
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
                INTran iNTran = new INTran()
                {
                    InventoryID = row.InventoryID,
                    Qty = row.EstimatedQty
                };

                iNTran = transferEntry.transactions.Insert(iNTran);

                iNTran.GetExtension<INTranExt>().UsrApptLineRef = row.LineRef;

                transferEntry.transactions.Update(iNTran);

                if (toSiteID == null) { toSiteID = row.SiteID; }
            }

            transferEntry.CurrentDocument.Current.ToSiteID = toSiteID;
            transferEntry.CurrentDocument.UpdateCurrent();
        }
        #endregion
    }
}