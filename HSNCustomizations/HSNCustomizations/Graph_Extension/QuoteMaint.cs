using PX.Data;
using PX.Data.BQL.Fluent;
using System.Collections.Generic;
using HSNCustomizations.DAC;

namespace PX.Objects.CR
{
    public class QuoteMaint_Extension : PXGraphExtension<QuoteMaint>
    {
        public const string QuoteMyRptID = "LM604500";

        #region Selects
        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        public SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<CRQuote.quoteID.FromCurrent>>.View TermsConditions;
        #endregion

        #region Override Methods
        public override void Initialize()
        {
            base.Initialize();

            Base.actionsFolder.AddMenuAction(printQuoteMY, nameof(Base.PrintQuote), true);
        }
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var quote = Base.QuoteCurrent.Current;

            if (HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance == true &&
                quote?.ExpirationDate != null && Base.CurrentOpportunity.Select().TopFirst?.GetExtension<CROpportunityExt>().UsrValidityDate == null)
            {
                PXUpdate<Set<CROpportunityExt.usrValidityDate, Required<CRQuote.expirationDate>>,
                         CROpportunity,
                         Where<CROpportunity.opportunityID, Equal<Required<CRQuote.opportunityID>>,
                               And<CROpportunity.defQuoteID, Equal<Required<CRQuote.quoteID>>>>>.Update(Base, quote.ExpirationDate, quote.OpportunityID, quote.QuoteID);
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

                throw new PXReportRequiredException(parameters, QuoteMyRptID, QuoteMyRptID) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<CRQuote> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            TermsConditions.AllowSelect = HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance ?? false;
        }
        #endregion
    }
}