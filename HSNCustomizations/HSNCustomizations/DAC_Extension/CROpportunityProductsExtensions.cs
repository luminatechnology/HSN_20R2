using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
    public class CROpportunityProductsExt : PXCacheExtension<PX.Objects.CR.CROpportunityProducts>
    {
        #region UsrMargin
        [PXDecimal]
        [PXUIField(DisplayName = "Margin", Enabled = false)]
        [PXFormula(typeof(Sub<CROpportunityProducts.curyAmount, CROpportunityProducts.curyExtCost/*Mult<CROpportunityProducts.quantity, CROpportunityProducts.curyUnitCost>*/>))]
        [PXUnboundFormula(typeof(Sub<CROpportunityProducts.curyAmount, CROpportunityProducts.curyExtCost>), typeof(SumCalc<CROpportunityExt.usrTotalMargin>), ForceAggregateRecalculation = true)]
        public virtual decimal? UsrMargin { get; set; }
        public abstract class usrMargin : PX.Data.BQL.BqlDecimal.Field<usrMargin> { }
        #endregion

        #region UsrMarginPct
        [PXDecimal]
        [PXUIField(DisplayName = "Margin %", Enabled = false)]
        [PXFormula(typeof(IIf<Where<CROpportunityProducts.curyAmount, Greater<decimal0>>, Mult<Div<usrMargin, CROpportunityProducts.curyAmount>, decimal100>, decimal0>))]
        public virtual decimal? UsrMarginPct { get; set; }
        public abstract class usrMarginPct : PX.Data.BQL.BqlDecimal.Field<usrMarginPct> { }
        #endregion
    }
}