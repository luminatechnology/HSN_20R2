using System;
using PX.Data;
using PX.Objects.CS;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("HSN Preferences")]
    [PXPrimaryGraph(typeof(LUMHSNSetupMaint))]
    public class LUMHSNSetup : IBqlTable
    {
        #region Keys
        public static class FK
        {
            public class CPrepaymentNumberingID : Numbering.PK.ForeignKeyOf<LUMHSNSetup>.By<cPrepaymentNumberingID> { }
        }
        #endregion

        #region CPrepaymentNumberingID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Customer Prepayment Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual string CPrepaymentNumberingID { get; set; }
        public abstract class cPrepaymentNumberingID : PX.Data.BQL.BqlString.Field<cPrepaymentNumberingID> { }
        #endregion
    
        #region EnableUniqSerialNbrByEquipType
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Unique Serial Nbr By Equipment Type")]
        public virtual bool? EnableUniqSerialNbrByEquipType { get; set; }
        public abstract class enableUniqSerialNbrByEquipType : PX.Data.BQL.BqlBool.Field<enableUniqSerialNbrByEquipType> { }
        #endregion
    
        #region EnablePartReqInAppt
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Part Request In Appointment")]
        public virtual bool? EnablePartReqInAppt { get; set; }
        public abstract class enablePartReqInAppt : PX.Data.BQL.BqlBool.Field<enablePartReqInAppt> { }
        #endregion
    
        #region EnableRMAProcInAppt
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable RMA Process In Appointment")]
        public virtual bool? EnableRMAProcInAppt { get; set; }
        public abstract class enableRMAProcInAppt : PX.Data.BQL.BqlBool.Field<enableRMAProcInAppt> { }
        #endregion
    
        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion
    
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion
    
        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
    
        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion
    
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion
    
        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
    
        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}