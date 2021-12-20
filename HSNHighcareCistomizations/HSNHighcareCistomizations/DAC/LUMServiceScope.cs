using System;
using HSNHighcareCistomizations.Graph;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMServiceScope")]
    public class LUMServiceScope : IBqlTable
    {
        #region CPriceClassID
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Price Class ID", Visible = false, Enabled = false)]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion

        #region LineNbr
        [PXDBIdentity(IsKey = true)]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region PriceClassID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Item Price Class ID")]
        [PXSelector(typeof(INPriceClass.priceClassID), DescriptionField = typeof(INPriceClass.description))]
        public virtual string PriceClassID { get; set; }
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
        #endregion

        #region InventoryID
        [PXDBInt()]
        [PXSelector(typeof(SelectFrom<InventoryItem>
                          .Where<InventoryItem.stkItem.IsEqual<False>
                            .And<InventoryItem.itemType.IsEqual<serviceTypeAttr>>>
                          .SearchFor<InventoryItem.inventoryID>),
                    typeof(InventoryItem.inventoryCD),
                    typeof(InventoryItem.descr),
                    typeof(InventoryItem.itemClassID),
                    typeof(InventoryItem.itemType),
                    SubstituteKey = typeof(InventoryItem.inventoryCD),
                    DescriptionField = typeof(InventoryItem.descr))]
        [PXUIField(DisplayName = "Nonstock item ")]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region DiscountPrecent
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Discount Percent")]
        public virtual Decimal? DiscountPrecent { get; set; }
        public abstract class discountPrecent : PX.Data.BQL.BqlDecimal.Field<discountPrecent> { }
        #endregion

        #region LimitedCount
        [PXDBInt()]
        [PXDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = " Limited Count")]
        public virtual int? LimitedCount { get; set; }
        public abstract class limitedCount : PX.Data.BQL.BqlInt.Field<limitedCount> { }
        #endregion

        #region Description
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description { get; set; }
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
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

    public class serviceTypeAttr : PX.Data.BQL.BqlString.Constant<serviceTypeAttr>
    {
        public serviceTypeAttr() : base("S") { }
    }

}