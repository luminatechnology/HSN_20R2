using System;
using PX.Data;

namespace HSNFinance.DAC
{
    [Serializable]
    [PXCacheName("LumARAgedPeriod")]
    public class LumARAgedPeriod : IBqlTable
    {
        #region LineNbr
        [PXDBIdentity(IsKey = true)]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region ConditionPeriodID
        [PXDBString(6, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Financial Period")]
        public virtual string ConditionPeriodID { get; set; }
        public abstract class conditionPeriodID : PX.Data.BQL.BqlString.Field<conditionPeriodID> { }
        #endregion

        #region Current
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Current")]
        public virtual Decimal? Current { get; set; }
        public abstract class current : PX.Data.BQL.BqlDecimal.Field<current> { }
        #endregion

        #region OneMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "1 - 30 Days")]
        public virtual Decimal? OneMDays { get; set; }
        public abstract class oneMDays : PX.Data.BQL.BqlDecimal.Field<oneMDays> { }
        #endregion

        #region TwoMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "31 - 60 Days")]
        public virtual Decimal? TwoMDays { get; set; }
        public abstract class twoMDays : PX.Data.BQL.BqlDecimal.Field<twoMDays> { }
        #endregion

        #region ThreeMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "61 - 90 Days")]
        public virtual Decimal? ThreeMDays { get; set; }
        public abstract class threeMDays : PX.Data.BQL.BqlDecimal.Field<threeMDays> { }
        #endregion

        #region FourMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "91 - 120 Days")]
        public virtual Decimal? FourMDays { get; set; }
        public abstract class fourMDays : PX.Data.BQL.BqlDecimal.Field<fourMDays> { }
        #endregion

        #region FiveMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "121 - 150 Days")]
        public virtual Decimal? FiveMDays { get; set; }
        public abstract class fiveMDays : PX.Data.BQL.BqlDecimal.Field<fiveMDays> { }
        #endregion

        #region SixMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "151 - 180 Days")]
        public virtual Decimal? SixMDays { get; set; }
        public abstract class sixMDays : PX.Data.BQL.BqlDecimal.Field<sixMDays> { }
        #endregion

        #region SevenMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "181 - 210 Days")]
        public virtual Decimal? SevenMDays { get; set; }
        public abstract class sevenMDays : PX.Data.BQL.BqlDecimal.Field<sevenMDays> { }
        #endregion

        #region OverSevenMDays
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Over 210 Days")]
        public virtual Decimal? OverSevenMDays { get; set; }
        public abstract class overSevenMDays : PX.Data.BQL.BqlDecimal.Field<overSevenMDays> { }
        #endregion

        #region Total
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Total Balance")]
        public virtual Decimal? Total { get; set; }
        public abstract class total : PX.Data.BQL.BqlDecimal.Field<total> { }
        #endregion
    }
}