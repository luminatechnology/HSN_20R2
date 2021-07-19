using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {
        #region Event Handlers
        protected void _(Events.FieldUpdated<INRegister.transferNbr> e, PXFieldUpdated baseHandler) 
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as INRegister;

            /// <summary>
            /// Rule 7: When user save the inventory receive and the Unit cost=0, system show warning message.
            /// </summary>
            if (row != null && !string.IsNullOrEmpty(row.GetExtension<INRegisterExt>().UsrAppointmentNbr) && Base.transactions.Select().RowCast<INTran>().Where(x => x.UnitCost == 0).Count() > 0 )
            {
                throw new PXSetPropertyException<INTran.unitCost>(HSNMessages.UnitCostIsZero, PXErrorLevel.Warning);
            }
        }

        //protected void _(Events.FieldUpdated<INRegister.transferNbr> e, PXFieldUpdated baseHandler) 
        //{
        //    baseHandler?.Invoke(e.Cache, e.Args);

        //    var row = e.Row as INRegister;
        //    var rowExt = row.GetExtension<INRegisterExt>();

        //    INRegister transfer = SelectFrom<INRegister>.Where<INRegister.docType.IsEqual<INDocType.transfer>
        //                                                       .And<INRegister.refNbr.IsEqual<@P.AsString>>>.View.Select(Base, e.NewValue.ToString());

        //    INRegisterExt transferExt = transfer.GetExtension<INRegisterExt>();

        //    row.ExtRefNbr = transfer.ExtRefNbr;
        //    row.TranDesc  = transfer.TranDesc;

        //    rowExt.UsrSrvOrdType     = transferExt.UsrSrvOrdType;
        //    rowExt.UsrAppointmentNbr = transferExt.UsrAppointmentNbr;
        //    rowExt.UsrSORefNbr       = transferExt.UsrSORefNbr;
        //    rowExt.UsrTransferPurp   = transferExt.UsrTransferPurp;
        //}
        #endregion

        #region Methods
        /// <summary>
        /// When user release the Receipts and the Appointment Nbr is not blank, then if the INTRAN.InventoryID of the ‘Detail Ref Nbr’ <> FSAppointmentDet.InventoryID of the ‘Detail Ref Nbr’ then
        /// Set Line Status =’Canceled’ of FSAppointmentDet.InventoryID of the ‘Detail Ref Nbr’
        /// Insert a new line into FSAppointmentDet with inventoryid = INTRAN.InventoryID of the ‘Detail Ref Nbr’. The ‘Estimated Quantity’ is the same as the canceled line.
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
                            FSAppointmentDet apptLine = SelectFrom<FSAppointmentDet>.Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                                                                           .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>
                                                                                                .And<FSAppointmentDet.lineRef.IsEqual<@P.AsString>>>>
                                                                                    .View.SelectSingleBound(Base, null, regisExt.UsrSrvOrdType, regisExt.UsrAppointmentNbr, row.GetExtension<INTranExt>().UsrApptLineRef);
                            if (!apptLine.InventoryID.Equals(row.InventoryID))
                            {
                                FSAppointmentDet newLine = apptEntry.AppointmentDetails.Cache.CreateCopy(apptLine) as FSAppointmentDet;

                                newLine.NoteID = null;
                                newLine.InventoryID = row.InventoryID;
                                newLine.EstimatedQty = row.Qty;

                                apptEntry.AppointmentDetails.Insert(newLine);

                                apptEntry.AppointmentDetails.Cache.SetValue<FSAppointmentDet.status>(apptLine, FSAppointmentDet.status.CANCELED);
                                apptEntry.AppointmentDetails.UpdateCurrent();
                            }
                        }

            row.ExtRefNbr = transfer.ExtRefNbr;
            row.TranDesc  = transfer.TranDesc;

            rowExt.UsrSrvOrdType     = transferExt.UsrSrvOrdType;
            rowExt.UsrAppointmentNbr = transferExt.UsrAppointmentNbr;
            rowExt.UsrSORefNbr       = transferExt.UsrSORefNbr;
            rowExt.UsrTransferPurp   = transferExt.UsrTransferPurp;
        }
        #endregion

        #region MyRegion
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

            var srvType = transferRow.GetExtension<INRegisterExt>().UsrSrvOrdType;
            var appNbr = transferRow.GetExtension<INRegisterExt>().UsrAppointmentNbr;
            var soRef = transferRow.GetExtension<INRegisterExt>().UsrSORefNbr;
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
    }
    #endregion
}
