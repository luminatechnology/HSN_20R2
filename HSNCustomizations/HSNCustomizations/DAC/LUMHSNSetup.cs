using System;
using PX.Data;
using PX.Objects.CS;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("HSN Preferences")]
    [PXPrimaryGraph(typeof(LUMHSNSetupMaint))]
    public class LUMHSNSetup : IBqlTable
    {
        #region Keys
        public static class FK
        {
            public class CPrepaymentNumberingID : Numbering.PK.ForeignKeyOf<LUMHSNSetup>.By<cPrepaymentNumberingID> { }
        }
        #endregion

        #region CPrepaymentNumberingID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Customer Prepayment Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual string CPrepaymentNumberingID { get; set; }
        public abstract class cPrepaymentNumberingID : PX.Data.BQL.BqlString.Field<cPrepaymentNumberingID> { }
        #endregion

        #region PickingListNumberingID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Picking List Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual string PickingListNumberingID { get; set; }
        public abstract class pickingListNumberingID : PX.Data.BQL.BqlString.Field<pickingListNumberingID> { }
        #endregion

        #region DeliveryOrderNumberingID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Delivery Order Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual string DeliveryOrderNumberingID { get; set; }
        public abstract class deliveryOrderNumberingID : PX.Data.BQL.BqlString.Field<deliveryOrderNumberingID> { }
        #endregion

        #region EnableUniqSerialNbrByEquipType
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Unique Serial Nbr By Equipment Type")]
        public virtual bool? EnableUniqSerialNbrByEquipType { get; set; }
        public abstract class enableUniqSerialNbrByEquipType : PX.Data.BQL.BqlBool.Field<enableUniqSerialNbrByEquipType> { }
        #endregion

        #region EnableWFStageCtrlInAppt
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Workflow Stages Control In Appointment")]
        public virtual bool? EnableWFStageCtrlInAppt { get; set; }
        public abstract class enableWFStageCtrlInAppt : PX.Data.BQL.BqlBool.Field<enableWFStageCtrlInAppt> { }
        #endregion

        #region EnablePartReqInAppt
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Part Request In Appointment")]
        public virtual bool? EnablePartReqInAppt { get; set; }
        public abstract class enablePartReqInAppt : PX.Data.BQL.BqlBool.Field<enablePartReqInAppt> { }
        #endregion
    
        #region EnableRMAProcInAppt
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable RMA Process In Appointment")]
        public virtual bool? EnableRMAProcInAppt { get; set; }
        public abstract class enableRMAProcInAppt : PX.Data.BQL.BqlBool.Field<enableRMAProcInAppt> { }
        #endregion

        #region EnableHeaderNoteSync
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Header Notes Synchronization")]
        public virtual bool? EnableHeaderNoteSync { get; set; }
        public abstract class enableHeaderNoteSync : PX.Data.BQL.BqlBool.Field<enableHeaderNoteSync> { }
        #endregion

        #region EnableChgInvTypeOnBill
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Invoice Type Change When Run Billing")]
        public virtual bool? EnableChgInvTypeOnBill { get; set; }
        public abstract class enableChgInvTypeOnBill : PX.Data.BQL.BqlBool.Field<enableChgInvTypeOnBill> { }
        #endregion

        #region DisplayTransferToHQ
        [PXDBBool()]
        [PXUIField(DisplayName = "Display Transfer To HQ")]
        public virtual bool? DisplayTransferToHQ { get; set; }
        public abstract class displayTransferToHQ : PX.Data.BQL.BqlBool.Field<displayTransferToHQ> { }
        #endregion

        #region DispApptActiviteInSrvOrd
        [PXDBBool()]
        [PXUIField(DisplayName = "Display Appointment Activities in Service Order")]
        public virtual bool? DispApptActiviteInSrvOrd { get; set; }
        public abstract class dispApptActiviteInSrvOrd : PX.Data.BQL.BqlBool.Field<dispApptActiviteInSrvOrd> { }
        #endregion

        #region EnableAppointmentUpdateEndDate
        [PXDBBool()]
        [PXUIField(DisplayName = "Finish Appointment to update End Date")]
        public virtual bool? EnableAppointmentUpdateEndDate { get; set; }
        public abstract class enableAppointmentUpdateEndDate : PX.Data.BQL.BqlBool.Field<enableAppointmentUpdateEndDate> { }
        #endregion

        #region EnableOpportunityEnhance
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Opportunity Enhancements")]
        public virtual bool? EnableOpportunityEnhance { get; set; }
        public abstract class enableOpportunityEnhance : PX.Data.BQL.BqlBool.Field<enableOpportunityEnhance> { }
        #endregion

        #region EnablePrintTransferProcess
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Print Transfer Process")]
        public virtual bool? EnablePrintTransferProcess { get; set; }
        public abstract class enablePrintTransferProcess : PX.Data.BQL.BqlBool.Field<enablePrintTransferProcess> { }
        #endregion

        #region EnableSCBPaymentFile
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable SCB Payment File")]
        public virtual bool? EnableSCBPaymentFile { get; set; }
        public abstract class enableSCBPaymentFile : PX.Data.BQL.BqlBool.Field<enableSCBPaymentFile> { }
        #endregion

        #region EnableCitiPaymentFile
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Citi Payment File")]
        public virtual bool? EnableCitiPaymentFile { get; set; }
        public abstract class enableCitiPaymentFile : PX.Data.BQL.BqlBool.Field<enableCitiPaymentFile> { }
        #endregion

        #region EnableCitiReturnCheckFile
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Citi Return Check File")]
        public virtual bool? EnableCitiReturnCheckFile { get; set; }
        public abstract class enableCitiReturnCheckFile : PX.Data.BQL.BqlBool.Field<enableCitiReturnCheckFile> { }
        #endregion

        #region EnableCitiOutSourceCheckFile
        [PXDBBool()]
        [PXUIField(DisplayName = "Enable Citi Out Source Check File")]
        public virtual bool? EnableCitiOutSourceCheckFile { get; set; }
        public abstract class enableCitiOutSourceCheckFile : PX.Data.BQL.BqlBool.Field<enableCitiOutSourceCheckFile> { }
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
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}