using System.Linq;
using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using HSNCustomizations.Descriptor;

namespace PX.Objects.IN
{
    public class INReceiptEntry_Extension : PXGraphExtension<INReceiptEntry>
    {
        #region Delegate Method
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable release(PXAdapter adapter)
        {
            Base.release.PressButton();

            AdjustRcptAndApptInventory();

            return adapter.Get();
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowPersisting<INRegister> e, PXRowPersisting baseHandler)
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

                        apptEntry.Save.Press();

                        ts.Complete();
                    }
                }
            }
            catch (PXException)
            {
                throw;
            }
        }
        #endregion
    }
}