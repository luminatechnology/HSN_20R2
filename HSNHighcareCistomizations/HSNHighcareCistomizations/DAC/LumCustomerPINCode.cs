using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.Licensing;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using static PX.Data.PXAccess.BAccount;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LumCustomerPINCode")]
    public class LumCustomerPINCode : IBqlTable
    {

        public static class FK
        {
            public class Customer : PX.Objects.AR.Customer.PK.ForeignKeyOf<LumCustomerPINCode>.By<bAccountID> { }
        }

        #region BAccountID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(FK.Customer))]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Account ID")]
        public virtual int? BAccountID { get; set; }
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion

        #region Pin
        [PXDBString(100, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Pin")]
        public virtual string Pin { get; set; }
        public abstract class pin : PX.Data.BQL.BqlString.Field<pin> { }
        #endregion

        #region StartDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Start Date", Enabled = false)]
        public virtual DateTime? StartDate { get; set; }
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        #endregion

        #region EndDate
        [PXDBDate()]
        [PXUIField(DisplayName = "End Date", Enabled = false)]
        public virtual DateTime? EndDate { get; set; }
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
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
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}