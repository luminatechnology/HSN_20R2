using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.Licensing;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.DR;
using static PX.Data.PXAccess.BAccount;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMCustomerPINCode")]
    public class LUMCustomerPINCode : IBqlTable
    {

        public static class FK
        {
            public class Customer : PX.Objects.AR.Customer.PK.ForeignKeyOf<LUMCustomerPINCode>.By<bAccountID> { }
        }

        #region Selected
        [PXBool]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region BAccountID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(FK.Customer))]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Account ID")]
        public virtual int? BAccountID { get; set; }
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion

        #region Pin
        [PXDBString(100, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Pin Code")]
        public virtual string Pin { get; set; }
        public abstract class pin : PX.Data.BQL.BqlString.Field<pin> { }
        #endregion

        #region SerialNbr
        [PXString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "HC Serial Nbr.", Enabled = false)]
        public virtual string SerialNbr { get; set; }
        public abstract class serialNbr : PX.Data.BQL.BqlString.Field<serialNbr> { }
        #endregion

        #region CPriceClassID
        [PXDefault]
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Customer Price Class", Visibility = PXUIVisibility.Visible)]
        public virtual String CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion

        #region ScheduleNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Deferred Schedule No.", Enabled = false)]
        [PXSelector(
            typeof(DRSchedule.scheduleNbr),
            typeof(DRSchedule.scheduleNbr),
            typeof(DRSchedule.documentTypeEx),
            typeof(DRSchedule.refNbr),
            typeof(DRSchedule.bAccountID))]
        public virtual string ScheduleNbr { get; set; }
        public abstract class scheduleNbr : PX.Data.BQL.BqlString.Field<scheduleNbr> { }
        #endregion

        #region IsActive
        [PXDBBool]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion

        #region StartDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Start Date", Enabled = false)]
        public virtual DateTime? StartDate { get; set; }
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        #endregion

        #region EndDate
        [PXDBDate()]
        [PXUIField(DisplayName = "End Date")]
        public virtual DateTime? EndDate { get; set; }
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        #endregion

        #region SOOrderNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Sales Order Nbr.", Enabled = false)]
        public virtual string SOOrderNbr { get; set; }
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
        #endregion

        #region InvoiceNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Invoice Nbr.", Enabled = false)]
        public virtual string InvoiceNbr { get; set; }
        public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
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
        [PXUIField(Enabled = false)]
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
        [PXUIField(Enabled = false)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}