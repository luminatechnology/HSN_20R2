using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Graph;
using eGUICustomization4HSN.StringList;

namespace PX.Objects.AR
{
    public class ARRegisterExt : PXCacheExtension<PX.Objects.AR.ARRegister>
    {
        #region UsrVATOutCode
        public const string VATOUTFRMTName = "VATOUTFRMT";
        public class VATOUTFRMTNameAtt : PX.Data.BQL.BqlString.Constant<VATOUTFRMTNameAtt>
        {
            public VATOUTFRMTNameAtt() : base(VATOUTFRMTName) { }
        }
        
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "VAT Out Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID,
                                  Where<CSAttributeDetail.attributeID, Equal<VATOUTFRMTNameAtt>>>),
                    typeof(CSAttributeDetail.description),
                    DescriptionField = typeof(CSAttributeDetail.description), 
                    ValidateValue = false)]
        public virtual string UsrVATOutCode { get; set; }
        public abstract class usrVATOutCode : IBqlField { }
        #endregion

        #region UsrTaxNbr
        public const string TaxNbrName = "TAXNBR";
        public class TaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<TaxNbrNameAtt>
        {
           public TaxNbrNameAtt() : base(TaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Nbr")]
        [PXDefault(typeof(Search2<CSAnswers.value, InnerJoin<Customer, On<Customer.noteID, Equal<CSAnswers.refNoteID>>>,
                                                   Where<Customer.bAccountID, Equal<Current<ARRegister.customerID>>,
                                                         And<CSAnswers.attributeID, Equal<TaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<ARInvoice.customerID>))]      
        public virtual string UsrTaxNbr { get; set; }
        public abstract class usrTaxNbr : IBqlField { }
        #endregion

        #region UsrOurTaxNbr
        public const string OurTaxNbrName = "OURTAXNBR";
        public class OurTaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<OurTaxNbrNameAtt>
        {
           public OurTaxNbrNameAtt() : base(OurTaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Our Tax Nbr")]
        [PXDefault(typeof(Search2<CSAnswers.value, InnerJoin<Customer, On<Customer.noteID, Equal<CSAnswers.refNoteID>>>,
                                                   Where<Customer.bAccountID, Equal<Current<ARRegister.customerID>>,
                                                         And<CSAnswers.attributeID, Equal<OurTaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<ARInvoice.customerID>))]      
        public virtual string UsrOurTaxNbr { get; set; }
        public abstract class usrOurTaxNbr : IBqlField { }
        #endregion

        #region UsrGUINo
        public class VATOut33Att : PX.Data.BQL.BqlString.Constant<VATOut33Att>
        {
            public VATOut33Att() : base(TWGUIFormatCode.vATOutCode33) { }
        }

        public class VATOut34Att : PX.Data.BQL.BqlString.Constant<VATOut34Att>
        {
            public VATOut34Att() : base(TWGUIFormatCode.vATOutCode34) { }
        }

        [GUINumber(14, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "GUI Nbr")]
        //[ARGUINbrAutoNum(typeof(TWNGUIPreferences.gUI2CopiesNumbering), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search2<TWNGUITrans.gUINbr,
                                   InnerJoin<BAccount, On<BAccount.acctCD, Equal<TWNGUITrans.custVend>>>,
                                   Where<BAccount.bAccountID, Equal<Current<ARRegister.customerID>>,
                                       And<TWNGUITrans.gUIStatus,Equal<TWNGUIStatus.used>,
                                           And<TWNGUITrans.gUIDirection, Equal<TWNGUIDirection.issue>,
                                               And<Where<TWNGUITrans.gUIFormatcode, NotEqual<VATOut33Att>,
                                                         And<TWNGUITrans.gUIFormatcode, NotEqual<VATOut34Att>>>>>>>>),
                    typeof(TWNGUITrans.gUIFormatcode),
                    typeof(TWNGUITrans.netAmtRemain),
                    typeof(TWNGUITrans.taxAmtRemain),
                    ValidateValue = false,
                    Filterable = true)]
        public virtual string UsrGUINo { get; set; }
        public abstract class usrGUINo : IBqlField { }
        #endregion

        #region UsrGUIDate
        [PXDBDate]
        [PXUIField(DisplayName = "GUI Date")]
        [PXDefault(typeof(AccessInfo.businessDate), 
                   PersistingCheck = PXPersistingCheck.Nothing)]        
        public virtual DateTime? UsrGUIDate { get; set; }
        public abstract class usrGUIDate : IBqlField { }
        #endregion

        #region UsrB2CType
        [PXDBString(3, IsUnicode = true)]
        [PXUIField(DisplayName = "B2C Type")]
        [PXDefault(TWNB2CType.DEF, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrB2CType { get; set; }
        public abstract class usrB2CType : PX.Data.BQL.BqlString.Field<usrB2CType> { }
        #endregion

        #region UsrCarrierID
        [PXDBString(64, IsUnicode = true)]
        [PXUIField(DisplayName = "Carrier ID")]
        public virtual string UsrCarrierID { get; set; }
        public abstract class usrCarrierID : PX.Data.BQL.BqlString.Field<usrCarrierID> { }
        #endregion

        #region UsrNPONbr
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "NPO Nbr", Visibility = PXUIVisibility.SelectorVisible, IsDirty = true)]
        [NPONbrSelector]
        public virtual string UsrNPONbr { get; set; }
        public abstract class usrNPONbr : PX.Data.BQL.BqlString.Field<usrNPONbr> { }
        #endregion

        #region UsrCreditAction
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Credit Action")]
        [PXDefault(TWNCreditAction.NO, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrCreditAction { get; set; }
        public abstract class usrCreditAction : PX.Data.BQL.BqlString.Field<usrCreditAction> { }
        #endregion
    }
}