using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LUMCalcInterestExpProc : PXGraph<LUMCalcInterestExpProc>
    {
        #region Selects & Features
        public PXCancel<BalanceFilter> Cancel;
        public PXFilter<BalanceFilter> Filter;
        [PXFilterable()]
        public PXFilteredProcessing<LUMLAInterestExpTemp, BalanceFilter> InterestExp;
        public PXSelect<LUMLAInterestExp> LAInterestExp;
        #endregion

        #region Ctor
        public LUMCalcInterestExpProc()
        {
            BalanceFilter filter = Filter.Current;

            InterestExp.SetSelected<LUMLAInterestExpTemp.selected>();
            InterestExp.SetProcessDelegate(delegate (List<LUMLAInterestExpTemp> list)
            {
                GenerateGLTrans(list, filter);
            });
            //InterestExp.SetProcessDelegate(list => GenerateGLTrans(list, filter));
        }
        #endregion

        #region Delegate Data View
        public virtual IEnumerable interestExp()
        {
            BalanceFilter filter = Filter.Current;

            PXSelectBase<FixedAsset> select = new PXSelectJoin<FixedAsset, LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
                                                                                    LeftJoin<FABookBalance, On<FABookBalance.assetID, Equal<FixedAsset.assetID>>>>,
                                                                            Where<FixedAsset.active, Equal<True>,
                                                                                  And<FixedAsset.branchID, Equal<Optional<BalanceFilter.branchID>>>>>(this);
                                                  //SelectFrom<FixedAsset>.LeftJoin<FADetails>.On<FADetails.assetID.IsEqual<FixedAsset.assetID>>
                                                  //                                                        .LeftJoin<FABookBalance>.On<FABookBalance.assetID.IsEqual<FixedAsset.assetID>>
                                                  //                                                        .Where<FixedAsset.active.IsEqual<True>
                                                  //                                                               .And<FixedAsset.branchID.IsEqual<@P.AsInt>>>.View(this);
            if (filter != null)
            {
                if (filter.ClassID != null)
                {
                    select.WhereAnd<Where<FixedAsset.classID, Equal<Current<BalanceFilter.classID>>>>();
                }
                if (filter.BookID != null)
                {
                    select.WhereAnd<Where<FABookBalance.bookID, Equal<Current<BalanceFilter.bookID>>>>();
                }

                foreach (PXResult<FixedAsset, FADetails, FABookBalance> result in select.Select(/*filter.BranchID, filter.ClassID, filter.BookID*/))
                {
                    FixedAsset asset = result;
                    FADetails details = result;
                    FABookBalance book = result;

                    LUMLAInterestExpTemp temp = new LUMLAInterestExpTemp()
                    {
                        AssetID  = asset.AssetID,
                        BookID   = book.BookID,
                        BranchID = asset.BranchID
                    };

                    LUMLAInterestExp intE = SelectFrom<LUMLAInterestExp>.Where<LUMLAInterestExp.assetID.IsEqual<@P.AsInt>.And<LUMLAInterestExp.bookID.IsEqual<@P.AsInt>>>
                                                                        .OrderBy<LUMLAInterestExp.termCount.Desc>.View.SelectSingleBound(this, null, temp.AssetID, temp.BookID);

                    string lastEndMthPeriod = MasterFinPeriod.PK.Find(this, filter.PeriodID).StartDate.Value.AddDays(-1).ToString("yyyyMM");

                    if (intE == null)
                    {
                        temp.FinPeriodID = lastEndMthPeriod;
                        temp.TermCount   = 1;
                        temp.BegBalance  = details.AcquisitionCost;
                    }
                    else
                    {
                        if (intE.FinPeriodID == lastEndMthPeriod) { continue; }

                        temp.FinPeriodID = intE.FinPeriodID;
                        temp.TermCount   = intE.TermCount + 1;
                        temp.BegBalance  = intE.EndBalance;
                    }

                    temp.LeaseRentTerm = details.LeaseRentTerm;
                    temp.MonthlyRent   = details.RentAmount / (details.LeaseRentTerm == 0m ? 1 : details.LeaseRentTerm);
                    temp.InterestRate  = details.GetExtension<FADetailsExt>().UsrMthlyInterestRatePct * temp.BegBalance / 100;
                    temp.EndBalance    = temp.BegBalance - temp.MonthlyRent + temp.InterestRate;
                    temp.RefNbr        = asset.AssetCD;

                    // Make unbound DAC has data that can be selected manually.
                    InterestExp.Cache.Insert(temp);

                    yield return temp;
                }
            }
        }
        #endregion

        #region Event Handlers
        //protected virtual void _(Events.RowSelected<BalanceFilter> e)
        //{
        //    BalanceFilter filter = e.Row;

        //    InterestExp.SetProcessDelegate<LUMCalcInterestExpProc>(delegate (LUMCalcInterestExpProc graph, LUMLAInterestExpTemp list)
        //    {
        //        GenerateFATrans(list, filter);
        //    });
        //}

        protected virtual void _(Events.FieldDefaulting<BalanceFilter.classID> e)
        {
            // Because Cache Attached event with PXDefaultAttribute doesn't work properly.
            e.NewValue = FAClass.UK.Find(this, "LEASEASSET")?.AssetID;
        }

        protected virtual void _(Events.FieldDefaulting<BalanceFilter.bookID> e)
        {
            // Because Cache Attached event with PXDefaultAttribute doesn't work properly.
            e.NewValue = SelectFrom<FABook>.Where<FABook.description.IsEqual<@P.AsString>>.View.Select(this, "FA Posting Book").TopFirst?.BookID;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Generate one/multiple GL transactions.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="filter"></param>
        public static void GenerateGLTrans(List<LUMLAInterestExpTemp> list, BalanceFilter filter)
        {
            const string LLSetupNotSet = "Lease Liability Preferences Hasn't Been Set Yet.";
            const string TranDescInterestExp = "Interest Expense for Asset {0}";

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                JournalEntry je = CreateInstance<JournalEntry>();

                je.BatchModule.Cache.Insert(new Batch()
                {
                    Module      = BatchModule.GL,
                    DateEntered = MasterFinPeriod.PK.Find(je, filter.PeriodID).StartDate.Value.AddDays(-1),
                    Description = AssetMaint_Extension.InterestExp
                });

                LUMLLSetup setup = SelectFrom<LUMLLSetup>.View.SelectSingleBound(je, null);

                if (setup == null) { throw new PXException(LLSetupNotSet); }

                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        GLTran tran = new GLTran()
                        {
                            BranchID  = list[i].BranchID,
                            AccountID = setup.InterestExpAcctID,
                            SubID     = setup.InterestExpSubID,
                            TranDesc  = string.Format(TranDescInterestExp, list[i].RefNbr)
                        };

                        tran = (GLTran)je.GLTranModuleBatNbr.Cache.Insert(tran);

                        decimal? amount = list[i].TermCount == list[i].LeaseRentTerm ? list[i].MonthlyRent - list[i].InterestRate : list[i].InterestRate;

                        if (j == 0)
                        {
                            tran.CuryDebitAmt = amount;
                        }
                        else
                        {
                            tran.AccountID     = setup.LeaseLiabAcctID;
                            tran.SubID         = setup.LeaseLiabSubID;
                            tran.CuryCreditAmt = amount;
                        }

                        je.GLTranModuleBatNbr.Cache.Update(tran);
                    }
                }

                je.BatchModule.SetValueExt<Batch.hold>(je.BatchModule.Current, false);
                    
                je.Save.Press();

                je.release.Press();

                if (je.BatchModule.Current?.Released == true)
                {
                    CreateLAInterestExp(list, je.BatchModule.Current.BatchNbr);
                }

                ts.Complete();
            }
        }

        /// <summary>
        /// Unbound DAC and GLTran create records in the LUMLAInterestExp table in processing form.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dic"></param>
        public static void CreateLAInterestExp(List<LUMLAInterestExpTemp> list, string batchNbr)
        {
            try
            {
                LUMCalcInterestExpProc graph = CreateInstance<LUMCalcInterestExpProc>();

                for (int i = 0; i < list.Count; i++)
                {
                    GLTran tran = new GLTran();

                    LUMLAInterestExp row = (LUMLAInterestExp)graph.LAInterestExp.Cache.Insert(new LUMLAInterestExp()
                    {
                        AssetID = list[i].AssetID,
                        TermCount = list[i].TermCount,
                    });

                    row.BookID        = list[i].BookID;
                    row.BranchID      = list[i].BranchID;
                    row.FinPeriodID   = list[i].FinPeriodID;
                    row.LeaseRentTerm = list[i].LeaseRentTerm;
                    row.BegBalance    = list[i].BegBalance;
                    row.MonthlyRent   = list[i].MonthlyRent;
                    row.InterestRate  = list[i].InterestRate;
                    row.EndBalance    = list[i].EndBalance;
                    row.BatchNbr      = batchNbr;
                    row.RefNbr        = list[i].RefNbr;

                    graph.LAInterestExp.Cache.Update(row);
                }

                graph.Actions.PressSave();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }

    #region Unbound DAC
    [Serializable]
    [PXCacheName("Temp Table")]
    [PXProjection(typeof(Select<LUMLAInterestExp>))]
    public partial class LUMLAInterestExpTemp : PX.Data.IBqlTable
    {
        #region Selected
        [PXBool()]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region AssetID
        [PXDBInt(IsKey = true, BqlField = typeof(LUMLAInterestExp.assetID))]
        [PXUIField(DisplayName = "Asset")]
        [PXSelector(typeof(Search<FixedAsset.assetID>),
                    SubstituteKey = typeof(FixedAsset.assetCD),
                    DescriptionField = typeof(FixedAsset.description))]
        public virtual int? AssetID { get; set; }
        public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
        #endregion

        #region TermCount
        [PXDBInt(IsKey = true, BqlField = typeof(LUMLAInterestExp.termCount))]
        [PXUIField(DisplayName = "Term Count")]
        public virtual int? TermCount { get; set; }
        public abstract class termCount : PX.Data.BQL.BqlInt.Field<termCount> { }
        #endregion

        #region LeaseRentTerm
        [PXDBInt()]
        [PXUIField(DisplayName = "Lease/Rent Term, months")]
        public virtual int? LeaseRentTerm { get; set;}
        public abstract class leaseRentTerm : PX.Data.BQL.BqlInt.Field<leaseRentTerm> { }
        #endregion

        #region BookID
        [PXDBInt(BqlField = typeof(LUMLAInterestExp.bookID))]
        [PXUIField(DisplayName = "Book")]
        [PXSelector(typeof(Search2<FABook.bookID, InnerJoin<FABookBalance, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
                                                  Where<FABookBalance.assetID, Equal<Current<LUMLAInterestExpTemp.assetID>>>>),
                    SubstituteKey = typeof(FABook.bookCode),
                    DescriptionField = typeof(FABook.description))]
        public virtual int? BookID { get; set; }
        public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
        #endregion

        #region BranchID
        [Branch(BqlField = typeof(LUMLAInterestExp.branchID))]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region FinPeriodID
        [FABookPeriodID(BqlField = typeof(LUMLAInterestExp.finPeriodID))]
        public virtual string FinPeriodID { get; set; }
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        #endregion

        #region BatchNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr.")]
        [BatchNbr(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleFA>,
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

        #region BegBalance
        [PXDBDecimal(BqlField = typeof(LUMLAInterestExp.begBalance))]
        [PXUIField(DisplayName = "Beggining Balance")]
        public virtual decimal? BegBalance { get; set; }
        public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
        #endregion

        #region MonthlyRent
        [PXDBDecimal(BqlField = typeof(LUMLAInterestExp.monthlyRent))]
        [PXUIField(DisplayName = "Monthly Rent")]
        public virtual decimal? MonthlyRent { get; set; }
        public abstract class monthlyRent : PX.Data.BQL.BqlDecimal.Field<monthlyRent> { }
        #endregion

        #region InterestRate
        [PXDBDecimal(BqlField = typeof(LUMLAInterestExp.interestRate))]
        [PXUIField(DisplayName = "Interest Expense")]
        public virtual decimal? InterestRate { get; set; }
        public abstract class interestRate : PX.Data.BQL.BqlDecimal.Field<interestRate> { }
        #endregion

        #region EndBalance
        [PXDBDecimal(BqlField = typeof(LUMLAInterestExp.endBalance))]
        [PXUIField(DisplayName = "Ending Balance")]
        public virtual decimal? EndBalance { get; set; }
        public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
        #endregion
    }
    #endregion
}