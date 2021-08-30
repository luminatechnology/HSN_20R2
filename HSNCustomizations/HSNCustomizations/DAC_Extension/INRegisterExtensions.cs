using System;
using PX.Data;
using PX.Objects.FS;
using HSNCustomizations.Descriptor;

namespace PX.Objects.IN
{
    [PXNonInstantiatedExtension]
    public class INRegister_ExistingColumn : PXCacheExtension<PX.Objects.IN.INRegister>
    {
        #region TotalQty
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        //[PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [INTotalQtyVerification]
        public virtual decimal? TotalQty { get; set; }
        #endregion
    }

    public class INRegisterExt : PXCacheExtension<PX.Objects.IN.INRegister>
    {
        #region UsrSrvOrdType
        [PXDBString(4, IsFixed = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        public virtual string UsrSrvOrdType { get; set; }
        public abstract class usrSrvOrdType : PX.Data.BQL.BqlString.Field<usrSrvOrdType> { }
        #endregion

        #region UsrAppointmentNbr
        [PXDBString(20, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", IsReadOnly = true)]
        [PXSelector(typeof(Search<FSAppointment.refNbr, Where<FSAppointment.srvOrdType, Equal<Optional<INRegisterExt.usrSrvOrdType>>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)})]
        public virtual string UsrAppointmentNbr { get; set; }
        public abstract class usrAppointmentNbr : PX.Data.BQL.BqlString.Field<usrAppointmentNbr> { }
        #endregion

        #region UsrSORefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [FSSelectorSORefNbr_Appointment(ValidateValue = false)]
        public virtual string UsrSORefNbr { get; set; }
        public abstract class usrSORefNbr : PX.Data.BQL.BqlString.Field<usrSORefNbr> { }
        #endregion  

        #region UsrTransferPurp
        [PXDBString(3, IsFixed = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Transfer Purpose", IsReadOnly = true)]
        [LUMTransferPurposeType()]
        [PXDefault(typeof(IIf<Where<INRegister.docType, Equal<INDocType.receipt>>, LUMTransferPurposeType.receipt, IIf<Where<INRegister.docType, Equal<INDocType.transfer>>, LUMTransferPurposeType.transfer, Null>>), 
                   PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrTransferPurp { get; set; }
        public abstract class usrTransferPurp : PX.Data.BQL.BqlString.Field<usrTransferPurp> { }
        #endregion

        #region UsrPLIsPrinted
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Picking List Printed", Enabled = false)]
        public bool? UsrPLIsPrinted { get; set; }
        public abstract class usrPLIsPrinted : PX.Data.BQL.BqlBool.Field<usrPLIsPrinted> { }
        #endregion

        #region UsrDOIsPrinted
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Delivery Order Printed", Enabled = false)]
        public bool? UsrDOIsPrinted { get; set; }
        public abstract class usrDOIsPrinted : PX.Data.BQL.BqlBool.Field<usrDOIsPrinted> { }
        #endregion

        #region UsrPickingListNumber
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Picking List Number", Enabled = false)]
        public virtual string UsrPickingListNumber { get; set; }
        public abstract class usrPickingListNumber : PX.Data.BQL.BqlString.Field<usrPickingListNumber> { }
        #endregion

        #region UsrDeliveryOrderNumber
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Delivery Order Number", Enabled = false)]
        public virtual string UsrDeliveryOrderNumber { get; set; }
        public abstract class usrDeliveryOrderNumber : PX.Data.BQL.BqlString.Field<usrDeliveryOrderNumber> { }
        #endregion
    }
}