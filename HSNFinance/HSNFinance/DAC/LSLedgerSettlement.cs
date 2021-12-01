using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("Ledger Settlement")]
    public class LSLedgerSettlement : PX.Data.IBqlTable
    {
        #region Selected
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region RecordID
        [PXDBIdentity(IsKey = true)]
        public virtual int? RecordID { get; set; }
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        #endregion

        #region SettlementNbr
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Settlement Nbr.", IsReadOnly = true)]
        public virtual string SettlementNbr { get; set; }
        public abstract class settlementNbr : PX.Data.BQL.BqlString.Field<settlementNbr> { }
        #endregion

        #region BranchID
        [Branch(typeof(Batch.branchID))]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region BatchNbr
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Batch Nbr")]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, IsNotNull>>))]
        public virtual string BatchNbr { get; set; }
        public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        #endregion

        #region LineNbr
        [PXDBInt()]
        [PXUIField(DisplayName = "Line Nbr")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region Module
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXDBDefault(typeof(Batch))]
        [BatchModule.List()]
        public virtual string Module { get; set; }
        public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
        #endregion

        #region LedgerID
        [PXDBInt()]
        [PXUIField(DisplayName = "Ledger")]
        //[PXFormula(typeof(Switch<Case<Where<Selector<Current<Batch.ledgerID>, Ledger.balanceType>, Equal<LedgerBalanceType.actual>>, 
        //                                    Selector<GLTran.branchID, Branch.ledgerID>>, Current<Batch.ledgerID>>))]
        [PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
        public virtual int? LedgerID { get; set; }
        public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        #endregion

        #region AccountID
        [PXUIField(DisplayName = "Account")]
        [Account(typeof(LSLedgerSettlement.branchID), LedgerID = typeof(LSLedgerSettlement.ledgerID), DescriptionField = typeof(Account.description))]
        public virtual int? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region SubID
        [PXUIField(DisplayName = "Sub")]
        [SubAccount(typeof(LSLedgerSettlement.accountID), typeof(LSLedgerSettlement.branchID), true)]
        public virtual int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion

        #region OrigCreditAmt
        [PXDBBaseCury(typeof(GLTran.ledgerID))]
        [PXUIField(DisplayName = "Orig. Credit Amt")]
        public virtual decimal? OrigCreditAmt { get; set; }
        public abstract class origCreditAmt : PX.Data.BQL.BqlDecimal.Field<origCreditAmt> { }
        #endregion

        #region OrigDebitAmt
        [PXDBBaseCury(typeof(GLTran.ledgerID))]
        [PXUIField(DisplayName = "Orig. Debit Amt")]
        public virtual decimal? OrigDebitAmt { get; set; }
        public abstract class origDebitAmt : PX.Data.BQL.BqlDecimal.Field<origDebitAmt> { }
        #endregion

        #region SettledCreditAmt
        [PXDBBaseCury(typeof(GLTran.ledgerID))]
        [PXUIField(DisplayName = "Settled Credit Amt")]
        public virtual decimal? SettledCreditAmt { get; set; }
        public abstract class settledCreditAmt : PX.Data.BQL.BqlDecimal.Field<settledCreditAmt> { }
        #endregion

        #region SettledDebitAmt
        [PXDBBaseCury(typeof(GLTran.ledgerID))]
        [PXUIField(DisplayName = "Settled Debit Amt")]
        public virtual decimal? SettledDebitAmt { get; set; }
        public abstract class settledDebitAmt : PX.Data.BQL.BqlDecimal.Field<settledDebitAmt> { }
        #endregion

        #region TranDesc
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Tran Desc")]
        public virtual string TranDesc { get; set; }
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
        #endregion

        #region TranDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Tran Date")]
        public virtual DateTime? TranDate { get; set; }
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsUnicode = true)]
        //[PXDBLiteDefault(typeof(Batch.refNbr), DefaultForUpdate = false, DefaultForInsert = false)]
        [PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.Visible)]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion

        #region InventoryID
        [Inventory()]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region ProjectID
        [GLProjectDefault(typeof(GLTran.ledgerID), AccountType = typeof(GLTran.accountID), PersistingCheck = PXPersistingCheck.Nothing)]
        //[ActiveProjectOrContractForGLAttribute(AccountFieldType = typeof(accountID))]
        [Project()]
        [PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
        public virtual int? ProjectID { get; set; }
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        #endregion

        #region TaskID
        [ActiveProjectTask(typeof(LSLedgerSettlement.projectID), BatchModule.GL, DisplayName = "Project Task")]
        //[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? TaskID { get; set; }
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
        #endregion

        #region CostCodeID
        [CostCode(typeof(LSLedgerSettlement.accountID), typeof(LSLedgerSettlement.taskID), ReleasedField = typeof(GLTran.released))]
        //[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
        public virtual int? CostCodeID { get; set; }
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
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

    public struct LedgerStlmtKey
    {
        public readonly int BranchID;
        public readonly int LineNbr;
        public readonly string Module;
        public readonly string BatchNbr;

        public LedgerStlmtKey(int branchID, int lineNbr, string module, string batchNbr)
        {
            BranchID = branchID;
            LineNbr  = lineNbr;
            Module   = module;
            BatchNbr = batchNbr;
        }
    }
}