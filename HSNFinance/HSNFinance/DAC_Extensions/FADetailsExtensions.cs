using PX.Data;

namespace PX.Objects.FA
{
    public class FADetailsExt : PXCacheExtension<PX.Objects.FA.FADetails>
    {
        #region UsrMthlyInterestRatePct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Monthly Interest Rate %")]
        public virtual decimal? UsrMthlyInterestRatePct { get; set; }
        public abstract class usrMthlyInterestRatePct : PX.Data.BQL.BqlDecimal.Field<usrMthlyInterestRatePct> { }
        #endregion
    }
}