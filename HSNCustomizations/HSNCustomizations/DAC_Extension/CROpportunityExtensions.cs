using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
    public class CROpportunityExt : PXCacheExtension<PX.Objects.CR.CROpportunity>
    {
        #region UsrTotalMargin
        [PXDBDecimal(BqlField = typeof(CROpportunityStandaloneExt.usrTotalMargin))]
        [PXUIField(DisplayName = "Margin", Enabled = false)]
        public virtual decimal? UsrTotalMargin { get; set; }
        public abstract class usrTotalMargin : PX.Data.BQL.BqlDecimal.Field<usrTotalMargin> { }
        #endregion

        #region UsrTotalMarginPct
        [PXDBDecimal(BqlField = typeof(CROpportunityStandaloneExt.usrTotalMarginPct))]
        [PXUIField(DisplayName = "Margin %", Enabled = false)]
        [PXFormula(typeof(IIf<Where<CROpportunity.curyProductsAmount, Greater<decimal0>>, Mult<Div<usrTotalMargin, CROpportunity.curyProductsAmount>, decimal100>, decimal0>))]
        public virtual decimal? UsrTotalMarginPct { get; set; }
        public abstract class usrTotalMarginPct : PX.Data.BQL.BqlDecimal.Field<usrTotalMarginPct> { }
        #endregion

        #region UsrValidityDate
        [PXDBDate(BqlField = typeof(CROpportunityStandaloneExt.usrValidityDate))]
        [PXUIField(DisplayName = "Validity Date")]
        public virtual DateTime? UsrValidityDate { get; set; }
        public abstract class usrValidityDate : PX.Data.BQL.BqlDateTime.Field<usrValidityDate> { }
        #endregion

        #region UsrPODate
        [PXDBDate(BqlField = typeof(CROpportunityStandaloneExt.usrPODate))]
        [PXUIField(DisplayName = "PO Date")]
        public virtual DateTime? UsrPODate { get; set; }
        public abstract class usrPODate : PX.Data.BQL.BqlDateTime.Field<usrPODate> { }
        #endregion

        #region UsrBilledDate
        [PXDBDate(BqlField = typeof(CROpportunityStandaloneExt.usrBilledDate))]
        [PXUIField(DisplayName = "Billed Date")]
        public virtual DateTime? UsrBilledDate { get; set; }
        public abstract class usrBilledDate : PX.Data.BQL.BqlDateTime.Field<usrBilledDate> { }
        #endregion
    }

    /// <summary>
    /// This cache extension is needed because of is working with PX.Objects.CR.CROpportunity what is a PXProjection and not with PX.Objects.CR.Standalone wich is the table in the Database. 
    /// Without this, fields are not saved in the DB.
    /// </summary>
    public class CROpportunityStandaloneExt : PXCacheExtension<PX.Objects.CR.Standalone.CROpportunity>
    {
        #region UsrTotalMargin
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Margin", Enabled = false)]
        public virtual decimal? UsrTotalMargin { get; set; }
        public abstract class usrTotalMargin : PX.Data.BQL.BqlDecimal.Field<usrTotalMargin> { }
        #endregion

        #region UsrTotalMarginPct
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Margin %", Enabled = false)]
        //[PXFormula(typeof(IIf<Where<CROpportunity.curyProductsAmount, Greater<decimal0>>, Mult<Div<usrTotalMargin, CROpportunity.curyProductsAmount>, decimal100>, decimal0>))]
        public virtual decimal? UsrTotalMarginPct { get; set; }
        public abstract class usrTotalMarginPct : PX.Data.BQL.BqlDecimal.Field<usrTotalMarginPct> { }
        #endregion

        #region UsrValidityDate
        [PXDBDate]
        [PXUIField(DisplayName = "Validity Date")]
        public virtual DateTime? UsrValidityDate { get; set; }
        public abstract class usrValidityDate : PX.Data.BQL.BqlDateTime.Field<usrValidityDate> { }
        #endregion

        #region UsrPODate
        [PXDBDate]
        [PXUIField(DisplayName = "PO Date")]
        public virtual DateTime? UsrPODate { get; set; }
        public abstract class usrPODate : PX.Data.BQL.BqlDateTime.Field<usrPODate> { }
        #endregion

        #region UsrBilledDate
        [PXDBDate]
        [PXUIField(DisplayName = "Billed Date")]
        public virtual DateTime? UsrBilledDate { get; set; }
        public abstract class usrBilledDate : PX.Data.BQL.BqlDateTime.Field<usrBilledDate> { }
        #endregion
    }
}
