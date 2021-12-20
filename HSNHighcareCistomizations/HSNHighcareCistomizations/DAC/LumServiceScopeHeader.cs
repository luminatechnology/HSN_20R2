using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.DR;
using PX.Objects.IN;

namespace HSNHighcareCistomizations
{
    [Serializable]
    [PXCacheName("LUMServiceScopeHeader")]
    public class LUMServiceScopeHeader : IBqlTable
    {
        #region CPriceClassID
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Customer Price Class ID")]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion

        #region InventoryID
        [PXDBInt()]
        [PXDefault]
        [PXUIField(DisplayName = "Component ID")]
        [DRComponentSelector(SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region DefCode
        [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<DRScheduleDetail.defCodeType>>>>))]
        [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), PX.Objects.DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string DefCode { get; set; }
        public abstract class defCode : PX.Data.BQL.BqlString.Field<defCode> { }
        #endregion

        #region TotalAmt
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Total Amount", Visible = false)]
        public virtual Decimal? TotalAmt { get; set; }
        public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt> { }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
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

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}