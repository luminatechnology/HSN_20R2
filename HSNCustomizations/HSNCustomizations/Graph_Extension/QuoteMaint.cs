using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System.Linq;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;

namespace PX.Objects.CR
{
    public class QuoteMaint_Extension : PXGraphExtension<QuoteMaint>
    {
        public const string QuoteMYRptID  = "LM604500";
        public const string QuoteMY2RptID = "LM604501";

        #region Selects
        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        public SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<CRQuote.quoteID.FromCurrent>>.OrderBy<LUMOpprTermCond.sortOrder.Asc>.View TermsConditions;
        #endregion

        #region Override Methods
        public override void Initialize()
        {
            base.Initialize();

            Base.actionsFolder.AddMenuAction(printQuoteMY, nameof(Base.PrintQuote), true);
            Base.actionsFolder.AddMenuAction(printQuoteMY2, nameof(PrintQuoteMY), true);
        }
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var quote = Base.QuoteCurrent.Current;

            if (quote != null && HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance == true)
            {
                if (quote.ExpirationDate != null && Base.CurrentOpportunity.Select().TopFirst?.GetExtension<CROpportunityExt>().UsrValidityDate == null)
                {
                    PXUpdate<Set<CROpportunityExt.usrValidityDate, Required<CRQuote.expirationDate>>,
                             CROpportunity,
                             Where<CROpportunity.opportunityID, Equal<Required<CRQuote.opportunityID>>,
                                   And<CROpportunity.defQuoteID, Equal<Required<CRQuote.quoteID>>>>>.Update(Base, quote.ExpirationDate, quote.OpportunityID, quote.QuoteID);
                }

                if (TermsConditions.Select().Count <= 0)
                {
                    CopyRecordFromOpportunity();
                }
            }

            baseMethod();
        }
        #endregion

        #region Actions
        public PXAction<CRQuote> printQuoteMY;
        [PXButton()]
        [PXUIField(DisplayName = "Print Quote-MY", MapEnableRights = PXCacheRights.Select)]
        protected virtual void PrintQuoteMY()
        {
            if (Base.Quote.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(CRQuote.OpportunityID)] = Base.Quote.Current.OpportunityID,
                    [nameof(CRQuote.QuoteNbr)]      = Base.Quote.Current.QuoteNbr
                };

                throw new PXReportRequiredException(parameters, QuoteMYRptID, QuoteMYRptID) { Mode = PXBaseRedirectException.WindowMode.New };
            }
        }

        public PXAction<CRQuote> printQuoteMY2;
        [PXButton()]
        [PXUIField(DisplayName = "Print Quote-MY 2", MapEnableRights = PXCacheRights.Select)]
        protected virtual void PrintQuoteMY2()
        {
            if (Base.Quote.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(CRQuote.OpportunityID)] = Base.Quote.Current.OpportunityID,
                    [nameof(CRQuote.QuoteNbr)] = Base.Quote.Current.QuoteNbr
                };

                throw new PXReportRequiredException(parameters, QuoteMY2RptID, QuoteMY2RptID) { Mode = PXBaseRedirectException.WindowMode.New };
            }
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<CRQuote> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            TermsConditions.AllowSelect = HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance ?? false;

            printQuoteMY.SetEnabled(Base.printQuote.GetEnabled());
            printQuoteMY2.SetEnabled(Base.printQuote.GetEnabled());
        }

        protected void _(Events.FieldVerifying<LUMOpprTermCond.sortOrder> e)
        {
            ///<remarks> Add the following verification to avoid that this field is not numbered in order or maintained incorrectly.</remarks>
            if (TermsConditions.Select().RowCast<LUMOpprTermCond>().Where(x => x.SortOrder == (int)e.NewValue).Count() > 0)
            {
                throw new PXSetPropertyException<LUMOpprTermCond.sortOrder>(HSNMessages.DuplicSortOrder);
            }
            if (SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<@P.AsGuid>>.AggregateTo<Max<LUMOpprTermCond.sortOrder>>.View.Select(Base, Base.QuoteCurrent.Current?.QuoteID)?.TopFirst.SortOrder > (int)e.NewValue)
            {
                throw new PXSetPropertyException<LUMOpprTermCond.sortOrder>(HSNMessages.SortOrdMustGreater);
            }
        }
        #endregion

        #region Methods
        public virtual void CopyRecordFromOpportunity()
        {
            foreach (LUMOpprTermCond row in SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.opportunityID.IsEqual<@P.AsString>>.View.Select(Base, Base.Quote.Current.OpportunityID))
            {
                LUMOpprTermCond termCond = new LUMOpprTermCond()
                {
                    IsActive   = row.IsActive,
                    SortOrder  = row.SortOrder,
                    Title      = row.Title,
                    Definition = row.Definition
                };

                termCond.OpportunityID = null;

                TermsConditions.Insert(termCond);
            }
        }
        #endregion
    }
}