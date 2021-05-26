using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.TX;
using eGUICustomization4HSN.Graph;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.DAC
{
    [Serializable]
    [PXCacheName("Manual GUI Expense")]
    public class TWNManualGUIExpense : PX.Data.IBqlTable
    {
        #region RefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(EPExpenseClaim.refNbr), 
                     PersistingCheck = PXPersistingCheck.Nothing)]
        [PXParent(typeof(Select<EPExpenseClaim, 
                                Where<EPExpenseClaim.refNbr, Equal<Current<refNbr>>>>))]
        [PXUIField(Visible = false)]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion
    
        #region Status
        [PXDBString(1, IsUnicode = true)]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [TWNGUIManualStatus.List]
        [PXDefault(TWNGUIManualStatus.Open)]  
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion
    
        #region VendorID
        [PXDBInt()]
        [PXUIField(DisplayName = "Vendor")]
        [PXSelector(typeof(Search2<BAccount2.bAccountID,
                           LeftJoin<Vendor, On<Vendor.bAccountID, Equal<BAccount2.bAccountID>,
                                    And<Vendor.type, Equal<BAccountType.vendorType>>>>,
                           Where<BAccount2.type, Equal<BAccountType.vendorType>>>),
                    typeof(BAccount.acctCD), 
                    typeof(BAccount.acctName), 
                    typeof(BAccount.type),
                    SubstituteKey = typeof(BAccount.acctCD), 
                    DescriptionField = typeof(BAccount.acctName))]  
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlString.Field<vendorID> { }
        #endregion
    
        #region VATInCode
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "VAT In Code")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID,
                                  Where<CSAttributeDetail.attributeID, Equal<APRegisterExt.VATINFRMTNameAtt>>>),
                    typeof(CSAttributeDetail.description))]
        [PXDefault(typeof(Search<CSAnswers.value,
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<APRegisterExt.VATINFRMTNameAtt>>>>))]
        [PXFormula(typeof(Default<vendorID>))]
        public virtual string VATInCode { get; set; }
        public abstract class vATInCode : PX.Data.BQL.BqlString.Field<vATInCode> { }
        #endregion
    
        #region GUINbr
        [GUINumber(14, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaa")]
        [PXUIField(DisplayName = "GUI Nbr")]
        [PXDefault()]
        public virtual string GUINbr { get; set; }
        public abstract class gUINbr : PX.Data.BQL.BqlString.Field<gUINbr> { }
        #endregion
    
        #region GUIDate
        [PXDBDate()]
        [PXUIField(DisplayName = "GUI Date")]
        public virtual DateTime? GUIDate { get; set; }
        public abstract class gUIDate : PX.Data.BQL.BqlDateTime.Field<gUIDate> { }
        #endregion
    
        #region TaxZoneID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Zone")]
        [PXDefault(typeof(EPExpenseClaim.taxZoneID), 
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search3<TaxZone.taxZoneID, 
                                   OrderBy<Asc<TaxZone.taxZoneID>>>), 
                    CacheGlobal = true)]
        [PX.Data.EP.PXFieldDescription]  
        public virtual string TaxZoneID { get; set; }
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
        #endregion
    
        #region TaxCategoryID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category")]
        [PXDefault(typeof(EPExpenseClaimDetails.taxCategoryID), 
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), 
                    DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), 
                      PX.Objects.TX.Messages.InactiveTaxCategory, 
                      typeof(TaxCategory.taxCategoryID))]
        public virtual string TaxCategoryID { get; set; }
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        #endregion
    
        #region TaxID
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Tax ID")]
        [PXSelector(typeof(Tax.taxID), 
                    DescriptionField = typeof(Tax.descr), 
                    DirtyRead = true)]  
        [PXDefault(typeof(Search<TaxZoneDet.taxID,
                                 Where<TaxZoneDet.taxZoneID, Equal<Current<TWNManualGUIExpense.taxZoneID>>>>))]
        public virtual string TaxID { get; set; }
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        #endregion
    
        #region TaxNbr
        [TaxNbrVerify(8, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Tax Nbr")]
        [PXDefault(typeof(Search<CSAnswers.value,
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<APRegisterExt.TaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<vendorID>))]
        public virtual string TaxNbr { get; set; }
        public abstract class taxNbr : PX.Data.BQL.BqlString.Field<taxNbr> { }
        #endregion
    
        #region OurTaxNbr
        [TaxNbrVerify(8, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Our Tax Nbr")]
        [PXDefault(typeof(Search<CSAnswers.value,
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<APRegisterExt.OurTaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<vendorID>))]
        public virtual string OurTaxNbr { get; set; }
        public abstract class ourTaxNbr : PX.Data.BQL.BqlString.Field<ourTaxNbr> { }
        #endregion
    
        #region Deduction
        [PXDBString(2, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Deduction")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID,
                                  Where<CSAttributeDetail.attributeID, Equal<APRegisterExt.DeductionNameAtt>>>), 
                    typeof(CSAttributeDetail.description))]
        [PXDefault(typeof(Search<CSAnswers.value,
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<APRegisterExt.DeductionNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<vendorID>))]
        public virtual string Deduction { get; set; }
        public abstract class deduction : PX.Data.BQL.BqlString.Field<deduction> { }
        #endregion

        #region NetAmt
        [PXDBDecimal(0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Net Amt")]
        public virtual decimal? NetAmt { get; set; }
        public abstract class netAmt : PX.Data.BQL.BqlDecimal.Field<netAmt> { }
        #endregion

        #region TaxAmt
        [PXDBDecimal(0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tax Amt")]
        public virtual decimal? TaxAmt { get; set; }
        public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
        #endregion

        #region Remark
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Remark")]
        public virtual string Remark { get; set; }
        public abstract class remark : PX.Data.BQL.BqlString.Field<remark> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime,
                   Enabled = false,
                   IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID       
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime,
                   Enabled = false,
                   IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region tstamp        
        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        #endregion
    }
}