using System;
using PX.Data;
using PX.Objects.CS;
using eGUICustomization4HSN.Graph;

namespace PX.Objects.AP
{
    public class APRegisterExt : PXCacheExtension<PX.Objects.AP.APRegister>
    {
        #region UsrVATINCODE
        public const string VATINFRMTName= "VATINFRMT";
        public class VATINFRMTNameAtt : PX.Data.BQL.BqlString.Constant<VATINFRMTNameAtt>
        {
           public VATINFRMTNameAtt() : base(VATINFRMTName) { }
        }

        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "VAT Format Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID,
                                  Where<CSAttributeDetail.attributeID, Equal<VATINFRMTNameAtt>>>),
                    typeof(CSAttributeDetail.description),
                    DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string UsrVATINCODE { get; set; }
        public abstract class usrVATINCODE : IBqlField { }
        #endregion

        #region UsrTaxID
        public const string TaxNbrName= "TAXNbr";
        public class TaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<TaxNbrNameAtt>
        {
           public TaxNbrNameAtt() : base(TaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Nbr")]
        [PXDefault(typeof(Search<CSAnswers.value, 
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<TaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<APInvoice.vendorID>))]        
        public virtual string UsrTaxID { get; set; }
        public abstract class usrTaxID : IBqlField { }
        #endregion

        #region UsrOurTaxID
        public const string OurTaxNbrName= "OURTAXNBR";
        public class OurTaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<OurTaxNbrNameAtt>
        {
           public OurTaxNbrNameAtt() : base(OurTaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Our Tax Nbr")]
        [PXDefault(typeof(Search<CSAnswers.value, 
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<OurTaxNbrNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<APInvoice.vendorID>))]        
        public virtual string UsrOurTaxID { get; set; }
        public abstract class usrOurTaxID : IBqlField { }
        #endregion

        #region UsrDEDUCTION
        public const string DeductionName = "DEDUCTCODE";
        public class DeductionNameAtt : PX.Data.BQL.BqlString.Constant<DeductionNameAtt>
        {
           public DeductionNameAtt () : base(DeductionName) { }
        }

        [PXDBString(1, IsUnicode = true)]
        [PXUIField(DisplayName = "Deduction Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<CSAnswers.value, 
                                 Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
                                       And<CSAnswers.attributeID, Equal<DeductionNameAtt>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<APInvoice.vendorID>))]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID,
                                  Where<CSAttributeDetail.attributeID, Equal<DeductionNameAtt>>>),
                    typeof(CSAttributeDetail.description),
                    DescriptionField = typeof(CSAttributeDetail.description))]         
        public virtual string UsrDEDUCTION { get; set; }
        public abstract class usrDEDUCTION : IBqlField { }
        #endregion

        #region UsrGUINO
        [PXUIField(DisplayName = "GUI Nbr")]
        [PXFormula(typeof(APInvoice.invoiceNbr))]
        [GUINumber(14, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaa")]       
        public virtual string UsrGUINO { get; set; }
        public abstract class usrGUINO : IBqlField { }
        #endregion

        #region UsrGUIDATE
        [PXDBDate()]        
        [PXUIField(DisplayName = "GUI Date")]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? UsrGUIDATE { get; set; }
        public abstract class usrGUIDATE : IBqlField { }
        #endregion
    }
}