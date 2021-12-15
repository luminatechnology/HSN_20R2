using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using HSNFinance.DAC;
using System.Linq;

namespace HSNFinance
{
    public class LSLedgerStlmtEntry : PXGraph<LSLedgerStlmtEntry>
    {
        #region Constant String Class & Property
        public const string steldAmtExceedRmngAmt = "Settle Amount Can't Be Greater Than Remaining Amount.";

        public const string ZZ_UOM = "ZZ";
        public const string YY_UOM = "YY";

        public class ZZUOM : PX.Data.BQL.BqlString.Constant<ZZUOM>
        {
            public ZZUOM () : base(ZZ_UOM) { }
        }

        public class YYUOM : PX.Data.BQL.BqlString.Constant<YYUOM>
        {
            public YYUOM () : base(YY_UOM) { }
        }
        #endregion

        #region Selects & Features
        public PXCancel<LedgerTranFilter> Cancel;  
        public PXFilter<LedgerTranFilter> Filter;
        public SelectFrom<LSLedgerSettlement>.View LedgerStlmt;
        [PXCopyPasteHiddenView]
        public SelectFrom<GLTran>.InnerJoin<Ledger>.On<Ledger.ledgerID.IsEqual<GLTran.ledgerID>
                                                       .And<Ledger.balanceType.IsEqual<LedgerBalanceType.actual>>>
                                 .Where</*NotExists<Select<LSLedgerSettlement,
                                                     Where<LSLedgerSettlement.branchID.IsEqual<GLTran.branchID>
                                                           .And<LSLedgerSettlement.lineNbr.IsEqual<GLTran.lineNbr>
                                                                .And<LSLedgerSettlement.module.IsEqual<GLTran.module>
                                                                     .And<LSLedgerSettlement.batchNbr.IsEqual<GLTran.batchNbr>>>>>>>
                                        .And<GLTran.curyDebitAmt.IsGreater<PX.Objects.CS.decimal0>*/
                                        GLTran.accountID.IsEqual<LedgerTranFilter.stlmtAcctID.FromCurrent>
                                                  //.And<GLTran.branchID.IsEqual<LedgerTranFilter.branchID.FromCurrent>>
                                                       .And<GLTran.released.IsEqual<True>>
                                                            .And<GLTran.posted.IsEqual<True>
                                                                 .And<Where<GLTran.uOM.IsNotEqual<ZZUOM>.Or<GLTran.uOM.IsNull>>>>>.View GLTranDebit;
        [PXCopyPasteHiddenView]
        public SelectFrom<GLTran>.InnerJoin<Ledger>.On<Ledger.ledgerID.IsEqual<GLTran.ledgerID>
                                                       .And<Ledger.balanceType.IsEqual<LedgerBalanceType.actual>>>
                                 .Where</*NotExists<Select<LSLedgerSettlement,
                                                         Where<LSLedgerSettlement.branchID.IsEqual<GLTran.branchID>
                                                               .And<LSLedgerSettlement.lineNbr.IsEqual<GLTran.lineNbr>
                                                                    .And<LSLedgerSettlement.module.IsEqual<GLTran.module>
                                                                         .And<LSLedgerSettlement.batchNbr.IsEqual<GLTran.batchNbr>>>>>>>
                                       .And<GLTran.curyCreditAmt.IsGreater<PX.Objects.CS.decimal0>*/
                                       GLTran.accountID.IsEqual<LedgerTranFilter.stlmtAcctID.FromCurrent>
                                                  //.And<GLTran.branchID.IsEqual<LedgerTranFilter.branchID.FromCurrent>>
                                                       .And<GLTran.released.IsEqual<True>>
                                                            .And<GLTran.posted.IsEqual<True>
                                                                 .And<Where<GLTran.uOM.IsNotEqual<ZZUOM>.Or<GLTran.uOM.IsNull>>>>>.View GLTranCredit;
        #endregion

        #region Delegate Data View
        protected virtual IEnumerable gLTranDebit()
        {
            LedgerTranFilter filter = Filter.Current;

            PXView debitView = new PXView(this, false, GLTranDebit.View.BqlSelect);

            List<object> lists = debitView.SelectMulti().ToList();

            if (filter.StlmtAcctID != null)
            {
                switch (filter.StlmtAcctType)
                {
                    case AccountType.Asset:
                        debitView.WhereAnd<Where<GLTran.curyDebitAmt, Greater<PX.Objects.CS.decimal0>>>();
                        break;
                    case AccountType.Liability:
                        debitView.WhereAnd<Where<GLTran.curyCreditAmt, Greater<PX.Objects.CS.decimal0>>>();
                        break;
                }

                for (int i = 0; i < lists.Count; i++)
                {
                    PXResult<GLTran, Ledger> result = lists[i] as PXResult<GLTran, Ledger>;
                    GLTran tran = result;
                    GLTranExt tranExt = tran.GetExtension<GLTranExt>();

                    LSLedgerSettlement settlement = SelectSumStldTran(this, tran.Module, tran.BatchNbr, tran.LineNbr);

                    tranExt.UsrRmngDebitAmt = tran.CuryDebitAmt - settlement?.SettledDebitAmt;
                    tranExt.UsrRmngCreditAmt = tran.CuryCreditAmt - settlement?.SettledCreditAmt;
                    tranExt.UsrSetldCreditAmt = (tranExt.UsrRmngCreditAmt ?? 0m) == 0m ? tran.CuryCreditAmt : tranExt.UsrRmngCreditAmt;

                    Filter.Cache.SetValue<LedgerTranFilter.balanceAmt>(filter, (filter.BalanceAmt ?? 0m) + tranExt.UsrSetldCreditAmt);
                }

                //int totalrow = 0;
                //int startrow = PXView.StartRow;

                //foreach (PXResult<GLTran, Ledger> result in debitView.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow))
                //{
                //    GLTran tran = result;
                //    GLTranExt tranExt = tran.GetExtension<GLTranExt>();

                //    LSLedgerSettlement settlement = SelectSumStldTran(this, tran.Module, tran.BatchNbr, tran.LineNbr);

                //    tranExt.UsrRmngDebitAmt = tran.CuryDebitAmt - settlement?.SettledDebitAmt;
                //    tranExt.UsrRmngCreditAmt = tran.CuryCreditAmt - settlement?.SettledCreditAmt;
                //    tranExt.UsrSetldCreditAmt = (tranExt.UsrRmngCreditAmt ?? 0m) == 0m ? tran.CuryCreditAmt : tranExt.UsrRmngCreditAmt;

                //    Filter.Cache.SetValue<LedgerTranFilter.balanceAmt>(filter, (filter.BalanceAmt ?? 0m) + tranExt.UsrSetldCreditAmt);

                //    yield return tran;
                //}

                //PXView.StartRow = 0;
            }

            return lists;
        }

        protected virtual IEnumerable gLTranCredit()
        {
            LedgerTranFilter filter = Filter.Current;

            PXView creditView = new PXView(this, false, GLTranCredit.View.BqlSelect);

            if (filter.StlmtAcctID != null)
            {
                switch (filter.StlmtAcctType)
                {
                    case AccountType.Asset:
                        creditView.WhereAnd<Where<GLTran.curyCreditAmt, Greater<PX.Objects.CS.decimal0>>>();
                        break;
                    case AccountType.Liability:
                        creditView.WhereAnd<Where<GLTran.curyDebitAmt, Greater<PX.Objects.CS.decimal0>>>();
                        break;
                }

                //int totalrow = 0;
                //int startrow = PXView.StartRow;

                //foreach (PXResult<GLTran, Ledger> result in creditView.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow))
                //{
                //    GLTran tran = result;
                //    GLTranExt tranExt = tran.GetExtension<GLTranExt>();

                //    LSLedgerSettlement settlement = SelectSumStldTran(this, tran.Module, tran.BatchNbr, tran.LineNbr);

                //    creditView.Cache.SetValue<GLTranExt.usrRmngDebitAmt>(tran, tran.CuryDebitAmt - settlement?.SettledDebitAmt);
                //    creditView.Cache.SetValue<GLTranExt.usrRmngCreditAmt>(tran, tran.CuryCreditAmt - settlement?.SettledCreditAmt);

                //    if (tran.Selected == true)
                //    {
                //        GLTranDebit.Cache.SetValue<GLTranExt.usrSetldCreditAmt>(tran, (tranExt.UsrRmngCreditAmt ?? 0m) == 0m ? tran.CuryCreditAmt : tranExt.UsrRmngCreditAmt);

                //        Filter.Cache.SetValue<LedgerTranFilter.balanceAmt>(filter, filter.BalanceAmt - settlement?.SettledCreditAmt);
                //    }

                //    yield return tran;
                //}

                //PXView.StartRow = 0;
            }

            return creditView.SelectMulti();
        }
        #endregion

        #region Actions
        public PXAction<LedgerTranFilter> Match;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Match", MapEnableRights = PXCacheRights.Select)]
        public IEnumerable match(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate ()
            {
                CreateLedgerSettlement();
            });

            return adapter.Get();
        }
        #endregion

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Selected", Visible = true)]
        protected void _(Events.CacheAttached<GLTran.selected> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        protected void _(Events.CacheAttached<GLTran.lineNbr> e) { }
        
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PX.Objects.IN.Inventory(Enabled = false, Visible = true)]
        protected void _(Events.CacheAttached<GLTran.inventoryID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = true)]
        protected void _(Events.CacheAttached<GLTran.batchNbr> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Customer/Vendor", Enabled = false, Visible = true)]
        protected void _(Events.CacheAttached<GLTran.referenceID> e) { }
        #endregion

        #region Event Handlers

        #region LedgerTranFilter
        protected void _(Events.RowSelected<LedgerTranFilter> e)
        {
            Match.SetEnabled(e.Row.BalanceAmt == decimal.Zero);

            PXUIFieldAttribute.SetEnabled<GLTran.batchNbr>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.branchID>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.subID>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.refNbr>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.curyDebitAmt>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.curyCreditAmt>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.tranDesc>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.projectID>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.taskID>(GLTranDebit.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<GLTran.costCodeID>(GLTranDebit.Cache, null, false);
        }
        #endregion

        #region GLTran
        protected void _(Events.RowSelected<GLTran> e)
        {
            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<GLTranExt.usrSetldDebitAmt>(e.Cache, e.Row, e.Row.CuryDebitAmt != 0m);
                PXUIFieldAttribute.SetEnabled<GLTranExt.usrSetldCreditAmt>(e.Cache, e.Row, e.Row.CuryCreditAmt != 0m);
            }
        }

        //protected void _(Events.FieldUpdated<GLTran.selected> e)
        //{
        //    var row = e.Row as GLTran;

        //    if (row != null)
        //    {
        //        GLTranExt tranExt = PXCacheEx.GetExtension<GLTranExt>(row);

        //        decimal debit  = tranExt.UsrRmngDebitAmt ?? 0m;
        //        decimal credit = tranExt.UsrRmngCreditAmt ?? 0m;

        //        GLTranDebit.Cache.SetValue<GLTranExt.usrSetldDebitAmt>  (row, (bool)e.NewValue == true ? (debit == 0m  ? row.CuryDebitAmt  : debit)  : 0m);
        //        GLTranCredit.Cache.SetValue<GLTranExt.usrSetldCreditAmt>(row, (bool)e.NewValue == true ? (credit == 0m ? row.CuryCreditAmt : credit) : 0m);
        //    }
        //}

        protected void _(Events.FieldVerifying<GLTranExt.usrSetldDebitAmt> e)
        {
            var row = e.Row as GLTran;

            GLTranExt tranExt = PXCacheEx.GetExtension<GLTranExt>(row);

            if (e.NewValue != null && tranExt.UsrRmngDebitAmt != 0m && (decimal)e.NewValue > tranExt.UsrRmngDebitAmt)
            {
                throw new PXSetPropertyException<GLTranExt.usrSetldDebitAmt>(steldAmtExceedRmngAmt);
            }
        }

        protected void _(Events.FieldVerifying<GLTranExt.usrSetldCreditAmt> e)
        {
            var row = e.Row as GLTran;

            GLTranExt tranExt = PXCacheEx.GetExtension<GLTranExt>(row);

            if (e.NewValue != null && tranExt.UsrRmngCreditAmt != 0m && (decimal)e.NewValue > tranExt.UsrRmngCreditAmt)
            {
                throw new PXSetPropertyException<GLTranExt.usrSetldCreditAmt>(steldAmtExceedRmngAmt);
            }
        }
        #endregion

        #endregion

        #region Methods
        public virtual void CreateLedgerSettlement()
        {
            string stlmtNbr = DateTime.UtcNow.ToString("yyyyMMddhhmmss");

            foreach (GLTran tran in SelectFrom<GLTran>.InnerJoin<Ledger>.On<Ledger.ledgerID.IsEqual<GLTran.ledgerID>
                                                                            .And<Ledger.balanceType.IsEqual<LedgerBalanceType.actual>>>
                                                      .Where<GLTran.selected.IsEqual<True>
                                                             .And<GLTran.accountID.IsEqual<LedgerTranFilter.stlmtAcctID.FromCurrent>>
                                                                  //.And<GLTran.branchID.IsEqual<LedgerTranFilter.branchID.FromCurrent>>
                                                                       .And<GLTran.released.IsEqual<True>>
                                                                            .And<GLTran.posted.IsEqual<True>>>.View.Select(this))
            {
                GLTranExt tranExt = PXCacheEx.GetExtension<GLTranExt>(tran);

                LSLedgerSettlement row = LedgerStlmt.Cache.CreateInstance() as LSLedgerSettlement;

                row.SettlementNbr    = stlmtNbr;
                row.BranchID         = tran.BranchID;
                row.LineNbr          = tran.LineNbr;
                row.Module           = tran.Module;
                row.BatchNbr         = tran.BatchNbr;
                row.LedgerID         = tran.LedgerID;
                row.AccountID        = tran.AccountID;
                row.SubID            = tran.SubID;
                row.OrigCreditAmt    = tran.CreditAmt;
                row.OrigDebitAmt     = tran.DebitAmt;
                row.SettledCreditAmt = tranExt.UsrSetldCreditAmt;
                row.SettledDebitAmt  = tranExt.UsrSetldDebitAmt;
                row.TranDesc         = tran.TranDesc;
                row.TranDate         = tran.TranDate;
                row.RefNbr           = tran.RefNbr;
                row.InventoryID      = tran.InventoryID;
                row.ProjectID        = tran.ProjectID;
                row.TaskID           = tran.TaskID;
                row.CostCodeID       = tran.CostCodeID;

                row = (LSLedgerSettlement)LedgerStlmt.Insert(row);

                GLTranDebit.Current = tran;

                decimal debit = tranExt.UsrRmngDebitAmt ?? 0m;
                decimal credit = tranExt.UsrRmngCreditAmt ?? 0m;

                UpdateGLTranUOM(GLTranDebit.Cache, (row.OrigCreditAmt + row.OrigDebitAmt == row.SettledCreditAmt + row.SettledDebitAmt || debit + credit == row.SettledCreditAmt + row.SettledDebitAmt) ? "ZZ" : "YY");
            }

            this.Actions.PressSave();
        }

        private LedgerStlmtKey GetKey(LSLedgerSettlement record)
        {
            return new LedgerStlmtKey(record.BranchID.Value, record.LineNbr.Value, record.Module, record.BatchNbr);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Update GLTran UOM to filter report (LS601000) data source.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="uOM"></param>
        public static void UpdateGLTranUOM(PXCache cache, string uOM)
        {        
            cache.SetValue<GLTran.uOM>(cache.Current, uOM);

            cache.Update(cache.Current);
        }

        /// <summary>
        /// Summarize settled credit & debit amount by parameters with different object types.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static LSLedgerSettlement SelectSumStldTran(PXGraph graph, params object[] objects)
        {
            string module   = (string)objects[0];
            string batchNbr = (string)objects[1];
            int    lineNbr  = (int)objects[2];

            return SelectFrom<LSLedgerSettlement>.Where<LSLedgerSettlement.module.IsEqual<@P.AsString>
                                                        .And<LSLedgerSettlement.batchNbr.IsEqual<@P.AsString>
                                                             .And<LSLedgerSettlement.lineNbr.IsEqual<@P.AsInt>>>>
                                                 .AggregateTo<Sum<LSLedgerSettlement.settledCreditAmt,
                                                                  Sum<LSLedgerSettlement.settledDebitAmt>>>.View.SelectSingleBound(graph, null, module, batchNbr, lineNbr);
        }
        #endregion
    }

    #region Filter DAC
    [Serializable]
    [PXCacheName("Ledger Transaction Filter")]
    public partial class LedgerTranFilter : IBqlTable
    {
        #region BranchID
        [Branch()]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region StlmtAcctID
        [PXDBInt()]
        [PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<LSSettlementAccount.accountID, Where<LSSettlementAccount.type, Equal<AccountType.asset>,
                                                                       Or<LSSettlementAccount.type, Equal<AccountType.liability>>>>),
                    typeof(LSSettlementAccount.accountID),
                    typeof(LSSettlementAccount.type),    
                    SubstituteKey = typeof(LSSettlementAccount.accountCD),
                    DescriptionField = typeof(LSSettlementAccount.description))]
        public virtual int? StlmtAcctID { get; set; }
        public abstract class stlmtAcctID : PX.Data.BQL.BqlInt.Field<stlmtAcctID> { }
        #endregion

        #region StlmtAcctType
        [PXDBString(1, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Type", IsReadOnly = true)]
        [PXDefault(typeof(Search<Account.type, Where<Account.accountID, Equal<Current<stlmtAcctID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<stlmtAcctID>))]
        public virtual string StlmtAcctType { get; set; }
        public abstract class stlmtAcctType : PX.Data.BQL.BqlString.Field<stlmtAcctType> { }
        #endregion

        #region BalanceAmt
        [PX.Objects.CM.PXDBBaseCury(typeof(GLTran.ledgerID))]
        [PXUIField(DisplayName = "Balance", IsReadOnly = true)]
        public virtual decimal? BalanceAmt { get; set; }
        public abstract class balanceAmt : PX.Data.BQL.BqlDecimal.Field<balanceAmt> { }
        #endregion
    }
    #endregion
}