using System;
using PX.Data;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LumINTran")]
    public class LumINTran : IBqlTable
    {
        #region DocType
        [PXDBString(1, IsKey = true, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Doc Type")]
        public virtual string DocType { get; set; }
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion

        #region TranType
        [PXDBString(3, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Tran Type")]
        public virtual string TranType { get; set; }
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion

        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region TranDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Tran Date")]
        public virtual DateTime? TranDate { get; set; }
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
        #endregion

        #region InventoryID
        [PXDBInt()]
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region Siteid
        [PXDBInt()]
        [PXUIField(DisplayName = "Siteid")]
        public virtual int? Siteid { get; set; }
        public abstract class siteid : PX.Data.BQL.BqlInt.Field<siteid> { }
        #endregion

        #region LocationID
        [PXDBInt()]
        [PXUIField(DisplayName = "Location ID")]
        public virtual int? LocationID { get; set; }
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        #endregion

        #region ToLocationID
        [PXDBInt()]
        [PXUIField(DisplayName = "To Location ID")]
        public virtual int? ToLocationID { get; set; }
        public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
        #endregion

        #region InvtMult
        [PXDBShort()]
        [PXUIField(DisplayName = "Invt Mult")]
        public virtual short? InvtMult { get; set; }
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
        #endregion

        #region Qty
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Qty")]
        public virtual Decimal? Qty { get; set; }
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
        #endregion

        #region TranDesc
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Tran Desc")]
        public virtual string TranDesc { get; set; }
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
        #endregion

        #region Uom
        [PXDBString(6, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Uom")]
        public virtual string Uom { get; set; }
        public abstract class uom : PX.Data.BQL.BqlString.Field<uom> { }
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

        #region Tositeid
        [PXDBInt()]
        [PXUIField(DisplayName = "Tositeid")]
        public virtual int? Tositeid { get; set; }
        public abstract class tositeid : PX.Data.BQL.BqlInt.Field<tositeid> { }
        #endregion

        #region UsrAppointmentNbr
        [PXDBString(20, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Usr Appointment Nbr")]
        public virtual string UsrAppointmentNbr { get; set; }
        public abstract class usrAppointmentNbr : PX.Data.BQL.BqlString.Field<usrAppointmentNbr> { }
        #endregion

        #region UsrTrackingNbr
        [PXDBString(20, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Checking Number", Enabled = true)]
        public virtual string UsrTrackingNbr { get; set; }
        public abstract class usrTrackingNbr : PX.Data.BQL.BqlString.Field<usrTrackingNbr> { }
        #endregion
    }
}