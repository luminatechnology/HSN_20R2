using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("v_HighcareServiceHistory")]
    public class v_HighcareServiceHistory : IBqlTable
    {
        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region SrvOrdType
        [PXDBString(4, IsFixed = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion

        #region SoRefNbr
        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Service Order Nbr")]
        public virtual string SoRefNbr { get; set; }
        public abstract class soRefNbr : PX.Data.BQL.BqlString.Field<soRefNbr> { }
        #endregion

        #region AptRefNbr
        [PXDBString(20, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Appointment Nbr")]
        public virtual string AptRefNbr { get; set; }
        public abstract class aptRefNbr : PX.Data.BQL.BqlString.Field<aptRefNbr> { }
        #endregion

        #region CustomerID
        [PXDBInt()]
        [PXUIField(DisplayName = "Customer ID")]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion

        #region InventoryID
        [PXDBInt()]
        [PXUIField(DisplayName = "Inventory ID")]
        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>),
                    typeof(InventoryItem.inventoryCD),
                    typeof(InventoryItem.descr),
                    typeof(InventoryItem.itemClassID),
                    typeof(InventoryItem.itemType),
                    SubstituteKey = typeof(InventoryItem.inventoryCD),
                    DescriptionField = typeof(InventoryItem.descr))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region InventoryCD
        [PXDBString()]
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual string InventoryCD { get; set; }
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
        #endregion

        #region Descr
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        #endregion

        #region StkItem
        [PXDBBool()]
        [PXUIField(DisplayName = "Stk Item")]
        public virtual bool? StkItem { get; set; }
        public abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }
        #endregion

        #region PriceClassID
        [PXDBString()]
        [PXUIField(DisplayName = "Price Class ID")]
        public virtual string PriceClassID { get; set; }
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
        #endregion

        #region Uom
        [PXDBString(6, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "UOM")]
        public virtual string Uom { get; set; }
        public abstract class uom : PX.Data.BQL.BqlString.Field<uom> { }
        #endregion

        #region OrderDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Order Date")]
        public virtual DateTime? OrderDate { get; set; }
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
        #endregion

        #region Pincode
        [PXDBString(20, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Pin Code")]
        public virtual string Pincode { get; set; }
        public abstract class pincode : PX.Data.BQL.BqlString.Field<pincode> { }
        #endregion

        #region CustomerpriceClassID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Customer Price ClassID")]
        public virtual string CustomerpriceClassID { get; set; }
        public abstract class customerpriceClassID : PX.Data.BQL.BqlString.Field<customerpriceClassID> { }
        #endregion

        #region SMEquipmentID
        [PXDBInt]
        [PXUIField(DisplayName = "Equipment ID")]
        public virtual int? SMEquipmentID { get; set; }
        public abstract class sMEquipmentID : PX.Data.BQL.BqlInt.Field<sMEquipmentID> { }
        #endregion
    }
}