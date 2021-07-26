using System;
using PX.Data;
using PX.Objects.FS;
using HSNCustomizations.Descriptor;

namespace PX.Objects.IN
{
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
    }
}