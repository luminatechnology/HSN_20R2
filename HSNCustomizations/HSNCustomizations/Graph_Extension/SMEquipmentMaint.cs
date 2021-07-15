using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.FS
{
    public class SMEquipmentMaint_Extension : PXGraphExtension<SMEquipmentMaint>
    {
        #region Event Handlers
        protected void _(Events.RowPersisting<FSEquipment> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as FSEquipment;

            if (row != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) )
            {
                bool duplicate = SelectFrom<FSEquipment>.Where<FSEquipment.serialNumber.IsEqual<@P.AsString>
                                                               .And<FSEquipment.equipmentTypeID.IsEqual<@P.AsInt>>>.View.Select(new PXGraph(), row.SerialNumber ?? string.Empty, row.EquipmentTypeID).Count > 0;

                if (!string.IsNullOrEmpty(row.SerialNumber) && SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableUniqSerialNbrByEquipType == true && duplicate == true)
                {
                    e.Cache.RaiseExceptionHandling<FSEquipment.serialNumber>(row, row.SerialNumber, new PXSetPropertyException(HSNMessages.NonUniqueSerNbr));
                }
            }           
        }
        #endregion
    }
}