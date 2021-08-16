using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;

namespace PX.Objects.CR
{
    public class QuoteMaint_Extension : PXGraphExtension<QuoteMaint>
    {
        #region Selects
        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        public SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<CRQuote.quoteID.FromCurrent>>.View TermsConditions;
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