using System;
using PX.Data;
using PX.Objects.GL;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("Lease Liability Setup")]
    [PXPrimaryGraph(typeof(LUMLLSetupMaint))]
    public class LUMLLSetup : IBqlTable
    {
        #region InterestExpAcctID
        [Account(typeof(AccessInfo.branchID), DisplayName = "Interest Expense Account")]
        [PXDefault()]
        public virtual int? InterestExpAcctID { get; set; }
        public abstract class interestExpAcctID : PX.Data.BQL.BqlInt.Field<interestExpAcctID> { }
        #endregion
    
        #region InterestExpSubID
        [SubAccount(typeof(LUMLLSetup.interestExpAcctID), typeof(AccessInfo.branchID), DisplayName = "Interest Expense Subaccount")]
        public virtual int? InterestExpSubID { get; set; }
        public abstract class interestExpSubID : PX.Data.BQL.BqlInt.Field<interestExpSubID> { }
        #endregion
    
        #region LeaseLiabAcctID
        [Account(typeof(AccessInfo.branchID), DisplayName = "Lease Liability Account")]
        [PXDefault()]
        public virtual int? LeaseLiabAcctID { get; set; }
        public abstract class leaseLiabAcctID : PX.Data.BQL.BqlInt.Field<leaseLiabAcctID> { }
        #endregion
    
        #region LeaseLiabSubID
        [SubAccount(typeof(LUMLLSetup.leaseLiabAcctID), typeof(AccessInfo.branchID), DisplayName = "Lease Liability Subaccount")]
        public virtual int? LeaseLiabSubID { get; set; }
        public abstract class leaseLiabSubID : PX.Data.BQL.BqlInt.Field<leaseLiabSubID> { }
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