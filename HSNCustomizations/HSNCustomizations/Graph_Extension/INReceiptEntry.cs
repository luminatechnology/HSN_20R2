using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using System.Collections;
using System.Linq;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {
        #region Delegate Method
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            // Doing release
            baseMethod(adapter);
            // Process Appointment & Service Order Stage Change
            // if TransferNbr is not null then wait for release complete (need to cheange status to  release)
            if (!string.IsNullOrEmpty(Base.receipt.Current.TransferNbr))
                PXLongOperation.WaitCompletion(Base.UID);
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                // Release Success
                if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.Completed)
                    if (UpdateAppointmentStageManual())
                        ts.Complete();
            }

            AdjustRcptAndApptInventory();

            if (VerifySiteAndLocationFromAppt() == false)
            {
                throw new PXException(HSNMessages.WHLocDiffFromAppt);
            }

            return adapter.Get();
        }
        #endregion

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible)]
        protected void _(Events.CacheAttached<INRegister.toSiteID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible)]
        protected void _(Events.CacheAttached<INRegister.siteID> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<INRegister> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            bool activePartRequest = hSNSetup?.EnablePartReqInAppt == true;

            PXUIFieldAttribute.SetVisible<INRegisterExt.usrAppointmentNbr>(e.Cache, null, activePartRequest);
            PXUIFieldAttribute.SetVisible<INRegisterExt.usrTransferPurp>(e.Cache, null, activePartRequest);
            PXUIFieldAttribute.SetVisible<INTranExt.usrApptLineRef>(Base.transactions.Cache, null, activePartRequest);
        }

        protected void _(Events.RowPersisting<INTran> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as INTran;
            var header = Base.CurrentDocument.Current;

            /// <summary>
            /// Rule 7: When user save the inventory receive and the Unit cost=0, system show warning message.
            /// </summary>
            if (header != null && row != null && !string.IsNullOrEmpty(header.GetExtension<INRegisterExt>().UsrAppointmentNbr) && row.UnitCost == 0)
            {
                e.Cache.RaiseExceptionHandling<INTran.unitCost>(row, row.UnitCost, new PXSetPropertyException<INTran.unitCost>(HSNMessages.UnitCostIsZero, PXErrorLevel.Warning));
            }
        }

        protected void _(Events.FieldUpdated<INRegister.transferNbr> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row    = e.Row as INRegister;
            var rowExt = row.GetExtension<INRegisterExt>();

            INRegister transfer = SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<INDocType.transfer>
                                                               .And<INRegister.refNbr.IsEqual<@P.AsString>>>.View.Select(Base, e.NewValue.ToString());

            INRegisterExt transferExt = transfer.GetExtension<INRegisterExt>();

            row.ExtRefNbr = transfer.ExtRefNbr;
            row.TranDesc  = transfer.TranDesc;

            rowExt.UsrSrvOrdType     = transferExt.UsrSrvOrdType;
            rowExt.UsrAppointmentNbr = transferExt.UsrAppointmentNbr;
            rowExt.UsrSORefNbr       = transferExt.UsrSORefNbr;
            rowExt.UsrTransferPurp   = transferExt.UsrTransferPurp == LUMTransferPurposeType.RMARetu ? LUMTransferPurposeType.RMARcpt : rowExt.UsrTransferPurp;
        }
        #endregion

        #region Methods
        /// <summary>
        /// When user release the Receipts and the Appointment Nbr is not blank, then if the INTRAN.InventoryID of the Detail Ref Nbr?<> FSAppointmentDet.InventoryID of the Detail Ref Nbr?then
        /// Set Line Status = Canceled of FSAppointmentDet.InventoryID of the Detail Ref Nbr?
        /// Insert a new line into FSAppointmentDet with inventoryid = INTRAN.InventoryID of the �Detail Ref Nbr? The �Estimated Quantity?is the same as the canceled line.
        /// In other words, if the inventory ID received is different with the inventory id requested.System cancels the original line, and create a new line with new inventory ID.
        /// </summary>
        public virtual void AdjustRcptAndApptInventory()
        {
            try
            {
                var register = Base.CurrentDocument.Current;
                var regisExt = register.GetExtension<INRegisterExt>();

                if (!string.IsNullOrEmpty(regisExt.UsrAppointmentNbr))
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        AppointmentEntry apptEntry = PXGraph.CreateInstance<AppointmentEntry>();

                        foreach (INTran row in Base.transactions.Cache.Cached)
                        {
                            PXResult<FSAppointmentDet, FSAppointment> result = (PXResult<FSAppointmentDet, FSAppointment>)SelectFrom<FSAppointmentDet>.InnerJoin<FSAppointment>.On<FSAppointment.srvOrdType.IsEqual<FSAppointmentDet.srvOrdType>
                                                                                                                                                                                   .And<FSAppointment.refNbr.IsEqual<FSAppointmentDet.refNbr>>>
                                                                                                                                                       .Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                                                                                                                                              .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>
                                                                                                                                                                   .And<FSAppointmentDet.lineRef.IsEqual<@P.AsString>>>>
                                                                                                                                                       .View.SelectSingleBound(Base, null, regisExt.UsrSrvOrdType, regisExt.UsrAppointmentNbr, row.GetExtension<INTranExt>().UsrApptLineRef);
                            FSAppointmentDet apptLine = result;

                            if (apptLine != null && !apptLine.InventoryID.Equals(row.InventoryID) && !apptLine.UIStatus.IsIn(ID.Status_AppointmentDet.CANCELED, ID.Status_AppointmentDet.COMPLETED))
                            {
                                apptEntry.AppointmentRecords.Current = result;

                                FSAppointmentDet newLine = PXCache<FSAppointmentDet>.CreateCopy(apptEntry.AppointmentDetails.Insert(new FSAppointmentDet()));

                                newLine.InventoryID  = row.InventoryID;
                                newLine.EstimatedQty = apptLine.EstimatedQty;
                                newLine.Status       = apptLine.Status;

                                newLine = PXCache<FSAppointmentDet>.CreateCopy(apptEntry.AppointmentDetails.Update(newLine));

                                newLine.EquipmentAction = apptLine.EquipmentAction;
                                newLine.OrigLineNbr     = apptLine.OrigLineNbr;
                                newLine.CuryUnitPrice   = apptLine.CuryUnitPrice;
                                // Per YJ's request to include the following two fields
                                newLine.SiteID          = apptLine.SiteID;
                                newLine.SiteLocationID  = apptLine.SiteLocationID;
                                newLine.LineRef         = row.GetExtension<INTranExt>().UsrApptLineRef;

                                apptEntry.AppointmentDetails.Update(newLine);

                                apptEntry.AppointmentDetails.Cache.SetValue<FSAppointmentDet.status>(apptLine, FSAppointmentDet.status.CANCELED);
                                apptEntry.AppointmentDetails.Update(apptLine);
                            }
                        }
                        if (apptEntry.AppointmentRecords.Current != null) { apptEntry.Save.Press(); }

                        ts.Complete();
                    }
                }
            }
            catch (PXException)
            {
                throw;
            }
        }

        /// <summary> Update Appointmnet/ServiceOrder Stage Manual </summary>
        public bool UpdateAppointmentStageManual()
        {
            var row = Base.receipt.Current;
            if (row == null)
                return false;

            var transferRow = SelectFrom<INRegister>.Where<INRegister.refNbr.IsEqual<P.AsString>>
                                .View.Select(Base, row.TransferNbr).RowCast<INRegister>().FirstOrDefault();
            // Check Transfer data is Exists
            if (transferRow == null || string.IsNullOrEmpty(row.TransferNbr))
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
            // AWSPARE07
            if (row.Status == INDocStatus.Released && transferRow.Status == INDocStatus.Released && transferRow.TransferType == INTransferType.TwoStep)
                autoWFStage = LUMAutoWorkflowStage.UK.Find(new PXGraph(), srvType, nameof(WFRule.AWSPARE07), apptData.WFStageID);
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

        /// <summary>
        /// When user release the Inventory Receipts and INRegister.UsrAppointmentNbr is not blank.
        /// Compare the warehouse and warehouse location for the same inventory ID and Detail Ref.Nbr between the Receipts and Appointment Details.
        /// If the warehouse or warehouse location is different, please throw error message
        /// </summary>
        /// <returns></returns>
        protected virtual bool VerifySiteAndLocationFromAppt()
        {
            var regisExt = Base.CurrentDocument.Current?.GetExtension<INRegisterExt>();

            //bool activePartReq  = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnablePartReqInAppt?? true;
            if (!string.IsNullOrEmpty(regisExt.UsrAppointmentNbr) && regisExt.UsrTransferPurp == LUMTransferPurposeType.PartRcv)
            {
                var list = SelectFrom<FSAppointmentDet>.Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                                              .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>>>.View.Select(Base, regisExt.UsrSrvOrdType, regisExt.UsrAppointmentNbr).RowCast<FSAppointmentDet>().ToList();

                foreach (INTran row in Base.transactions.Cache.Cached)
                {
                    if (list.Exists(x => x.Status != ID.Status_AppointmentDet.CANCELED && x.InventoryID == row.InventoryID &&
                                         x.LineRef == row.GetExtension<INTranExt>().UsrApptLineRef && x.SiteID == row.SiteID && x.LocationID == row.LocationID) == false) { return false; }
                }
            }

            return true;
        }
        #endregion
    }
}
