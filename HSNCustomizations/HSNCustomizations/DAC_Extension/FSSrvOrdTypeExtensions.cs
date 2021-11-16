using PX.Data;

namespace PX.Objects.FS
{
    public class FSSrvOrdTypeExt : PXCacheExtension<FSSrvOrdType>
    {
        #region UsrEnableEquipmentMandatory
        [PXDBBool()]
        [PXUIField(DisplayName = "Target Equipment ID Is Mandatory")]
        public virtual bool? UsrEnableEquipmentMandatory { get; set; }
        public abstract class usrEnableEquipmentMandatory : PX.Data.BQL.BqlBool.Field<usrEnableEquipmentMandatory> { }
		#endregion

		#region UsrOnStaffIsMandStartAppt
		[PXDBBool()]
		[PXUIField(DisplayName = "Staff Is Mandatory When Start Appointment")]
		public virtual bool? UsrOnStaffIsMandStartAppt { get; set; }
		public abstract class usrOnStaffIsMandStartAppt : PX.Data.BQL.BqlBool.Field<usrOnStaffIsMandStartAppt> { }
		#endregion

		#region UsrBringBrandAttr2Txfr
		[PXDBBool()]
		[PXUIField(DisplayName = "Bring Brand Attribute To Transfer")]
		public virtual bool? UsrBringBrandAttr2Txfr { get; set; }
		public abstract class usrBringBrandAttr2Txfr : PX.Data.BQL.BqlBool.Field<usrBringBrandAttr2Txfr> { }
		#endregion
	}
}
