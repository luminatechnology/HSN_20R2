using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("LA Interest Expense")]
    public class LUMLAInterestExp : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<LUMLAInterestExp>.By<batchNbr, termCount>
        {
            public static LUMLAInterestExp Find(PXGraph graph, string batchNbr, int? termCount) => FindBy(graph, batchNbr, termCount);
        }

        public static class FK
        {
            public class FixedAsset : PX.Objects.FA.FixedAsset.PK.ForeignKeyOf<LUMLAInterestExp>.By<assetID> { }
            public class Book : FABook.PK.ForeignKeyOf<LUMLAInterestExp>.By<bookID> { }
            public class Branch : PX.Objects.GL.Branch.PK.ForeignKeyOf<LUMLAInterestExp>.By<branchID> { }
        }
        #endregion

        #region AssetID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Asset")]
        [PXSelector(typeof(Search2<FixedAsset.assetID, LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
                                                                LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FADetails.assetID>,
                                                                                               And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>,
                                                                         LeftJoin<Branch, On<Branch.branchID, Equal<FALocationHistory.locationID>>,
                                                                                  LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<FALocationHistory.employeeID>>>>>>,
                                                        Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
                    typeof(FixedAsset.assetCD),
                    typeof(FixedAsset.description),
                    typeof(FixedAsset.classID),
                    typeof(FixedAsset.usefulLife),
                    typeof(FixedAsset.assetTypeID),
                    typeof(FADetails.status),
                    typeof(Branch.branchCD),
                    typeof(EPEmployee.acctName),
                    typeof(FALocationHistory.department),
                    Filterable = true,
                    SubstituteKey = typeof(FixedAsset.assetCD),
                    DescriptionField = typeof(FixedAsset.description))]
        public virtual int? AssetID { get; set; }
        public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
        #endregion

        #region TermCount
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Term Count")]
        public virtual int? TermCount { get; set; }
        public abstract class termCount : PX.Data.BQL.BqlInt.Field<termCount> { }
        #endregion

        #region BookID
        [PXDBInt()]
        [PXUIField(DisplayName = "Book")]
        [PXSelector(typeof(Search2<FABook.bookID, InnerJoin<FABookBalance, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
                                                  Where<FABookBalance.assetID, Equal<Current<LUMLAInterestExp.assetID>>>>),
                    SubstituteKey = typeof(FABook.bookCode),
                    DescriptionField = typeof(FABook.description))]
        public virtual int? BookID { get; set; }
        public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
        #endregion
    
        #region BranchID
        [Branch()]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion
    
        #region LeaseRentTerm
        [PXDBInt()]
        [PXUIField(DisplayName = "Rent Term (Months)")]
        public virtual int? LeaseRentTerm { get; set; }
        public abstract class leaseRentTerm : PX.Data.BQL.BqlInt.Field<leaseRentTerm> { }
        #endregion
   
        #region FinPeriodID
        [FABookPeriodID()]
        [PXUIField(DisplayName = "Tran Period")]
        public virtual string FinPeriodID { get; set; }
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        #endregion

        #region BegBalance
        [PXDBBaseCury]
        [PXUIField(DisplayName = "Beginning Balance")]
        public virtual decimal? BegBalance { get; set; }
        public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
        #endregion

        #region MonthlyRent
        [PXDBBaseCury]
        [PXUIField(DisplayName = "Monthly Rent")]
        public virtual decimal? MonthlyRent { get; set; }
        public abstract class monthlyRent : PX.Data.BQL.BqlDecimal.Field<monthlyRent> { }
        #endregion

        #region InterestRate
        [PXDBBaseCury]
        [PXUIField(DisplayName = "Interest Expense")]
        public virtual decimal? InterestRate { get; set; }
        public abstract class interestRate : PX.Data.BQL.BqlDecimal.Field<interestRate> { }
        #endregion

        #region EndBalance
        [PXDBBaseCury]
        [PXUIField(DisplayName = "Ending Balance")]
        public virtual decimal? EndBalance { get; set; }
        public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
        #endregion

        #region BatchNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr.")]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleFA>,
                                                        Or<Batch.module, Equal<BatchModule.moduleGL>>>>))]
        public virtual string BatchNbr { get; set; }
        public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Reference Number", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
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
    
        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    
        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}