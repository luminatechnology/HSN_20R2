using PX.Data;

namespace PX.Objects.FS
{
    public class FSAppointmentExt : PXCacheExtension<PX.Objects.FS.FSAppointment>
    {
        #region UsrTransferToHQ
        [PXDBBool()]
        [PXUIField(DisplayName = "Transfer To HQ")]
        public virtual bool? UsrTransferToHQ { get; set; }
        public abstract class usrTransferToHQ : PX.Data.BQL.BqlBool.Field<usrTransferToHQ> { }
        #endregion
    }
}