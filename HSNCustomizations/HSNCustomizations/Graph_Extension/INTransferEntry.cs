using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;

namespace PX.Objects.IN
{
    public class INTransferEntry_Extension : PXGraphExtension<INTransferEntry>
    {
        #region Actions
        public PXAction<INRegister> copyItemFromAppt;
        [PXButton()]
        [PXUIField(DisplayName = "Copy Item From Appt.", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable CopyItemFromAppt(PXAdapter adapter)
        {
            var register = Base.CurrentDocument.Current;

            if (register != null)
            {
                var regisExt = register.GetExtension<INRegisterExt>();

                foreach (FSAppointmentDet row in SelectFrom<FSAppointmentDet>.Where<FSAppointmentDet.srvOrdType.IsEqual<@P.AsString>
                                                                                .And<FSAppointmentDet.refNbr.IsEqual<@P.AsString>
                                                                                     .And<FSAppointmentDet.lineType.IsEqual<FSLineType.Inventory_Item>>>>.View.Select(Base, regisExt.UsrSrvOrdType, regisExt.UsrAppointmentNbr))
                {
                    AppointmentEntry_Extension.CreateINTran(Base, row);
                }

                Base.Save.Press();
            }
            
            return adapter.Get();
        }
        #endregion

        #region Event Handlerd
        protected void _(Events.RowSelected<INRegister> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            copyItemFromAppt.SetEnabled(Base.transactions.Select().Count == 0);
        }
        #endregion
    }
}
