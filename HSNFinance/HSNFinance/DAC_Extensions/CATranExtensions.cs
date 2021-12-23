using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using HSNFinance.DAC;

namespace PX.Objects.CA
{
    public class CATranExt : PXCacheExtension<PX.Objects.CA.CATran>
    {
        #region Attribute Constant Variables & Classes
        public const string CFGROUP1 = "CFGROUP1";
        public class CFGROUP1Attr : PX.Data.BQL.BqlString.Constant<CFGROUP1Attr>
        {
            public CFGROUP1Attr() : base(CFGROUP1) { }
        }

        public const string CFGROUP2 = "CFGROUP2";
        public class CFGROUP2Attr : PX.Data.BQL.BqlString.Constant<CFGROUP1Attr>
        {
            public CFGROUP2Attr() : base(CFGROUP2) { }
        }
        #endregion

        #region UsrARPaymentCFGrp1
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<ARRegisterKvExt, On<ARRegisterKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                            And<CSAttributeDetail.attributeID, Equal<CFGROUP1Attr>>>,
                                                                        InnerJoin<ARRegister, On<ARRegister.noteID, Equal<ARRegisterKvExt.recordID>>>>,
                                                              Where<ARRegister.docType, Equal<CATran.origTranType>,
                                                                    And<ARRegister.refNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrARPaymentCFGrp1 { get; set; }
        public abstract class usrARPaymentCFGrp1 : PX.Data.BQL.BqlString.Field<usrARPaymentCFGrp1> { }
        #endregion

        #region UsrAPPaymentCFGrp1
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<APRegisterKvExt, On<APRegisterKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                            And<CSAttributeDetail.attributeID, Equal<CFGROUP1Attr>>>,
                                                                        InnerJoin<APRegister, On<APRegister.noteID, Equal<APRegisterKvExt.recordID>>>>,
                                                              Where<APRegister.docType, Equal<CATran.origTranType>,
                                                                    And<APRegister.refNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrAPPaymentCFGrp1 { get; set; }
        public abstract class usrAPPaymentCFGrp1 : PX.Data.BQL.BqlString.Field<usrAPPaymentCFGrp1> { }
        #endregion

        #region UsrBatchCFGrp1
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<BatchKvExt, On<BatchKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                       And<CSAttributeDetail.attributeID, Equal<CFGROUP1Attr>>>,
                                                                        InnerJoin<Batch, On<Batch.noteID, Equal<BatchKvExt.recordID>>>>,
                                                              Where<Batch.module, Equal<CATran.origModule>,
                                                                    And<Batch.batchNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrBatchCFGrp1 { get; set; }
        public abstract class usrBatchCFGrp1 : PX.Data.BQL.BqlString.Field<usrBatchCFGrp1> { }
        #endregion

        #region UsrCFGroup1
        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Casflow Group 1st", Enabled = false)]
        [PXFormula(typeof(IIf<Where<CATran.origModule, Equal<BatchModule.moduleGL>>, usrBatchCFGrp1, IIf<Where<CATran.origModule, Equal<BatchModule.moduleAR>>, usrARPaymentCFGrp1, usrAPPaymentCFGrp1>>))]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<CFGROUP1Attr>>>),
                    DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string UsrCFGroup1 { get; set; }
        public abstract class usrCFGroup1 : PX.Data.BQL.BqlString.Field<usrCFGroup1> { }
        #endregion

        #region UsrARPaymentCFGrp2
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<ARRegisterKvExt, On<ARRegisterKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                            And<CSAttributeDetail.attributeID, Equal<CFGROUP2Attr>>>,
                                                                        InnerJoin<ARRegister, On<ARRegister.noteID, Equal<ARRegisterKvExt.recordID>>>>,
                                                              Where<ARRegister.docType, Equal<CATran.origTranType>,
                                                                    And<ARRegister.refNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrARPaymentCFGrp2 { get; set; }
        public abstract class usrARPaymentCFGrp2 : PX.Data.BQL.BqlString.Field<usrARPaymentCFGrp2> { }
        #endregion

        #region UsrAPPaymentCFGrp2
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<APRegisterKvExt, On<APRegisterKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                            And<CSAttributeDetail.attributeID, Equal<CFGROUP2Attr>>>,
                                                                        InnerJoin<APRegister, On<APRegister.noteID, Equal<APRegisterKvExt.recordID>>>>,
                                                              Where<APRegister.docType, Equal<CATran.origTranType>,
                                                                    And<APRegister.refNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrAPPaymentCFGrp2 { get; set; }
        public abstract class usrAPPaymentCFGrp2 : PX.Data.BQL.BqlString.Field<usrAPPaymentCFGrp2> { }
        #endregion

        #region UsrBatchCFGrp2
        [PXString(10, IsUnicode = true)]
        [PXDBScalar(typeof(Search2<CSAttributeDetail.valueID, InnerJoin<BatchKvExt, On<BatchKvExt.valueString, Equal<CSAttributeDetail.valueID>,
                                                                                       And<CSAttributeDetail.attributeID, Equal<CFGROUP2Attr>>>,
                                                                        InnerJoin<Batch, On<Batch.noteID, Equal<BatchKvExt.recordID>>>>,
                                                              Where<Batch.module, Equal<CATran.origModule>,
                                                                    And<Batch.batchNbr, Equal<CATran.origRefNbr>>>>))]
        public virtual string UsrBatchCFGrp2 { get; set; }
        public abstract class usrBatchCFGrp2 : PX.Data.BQL.BqlString.Field<usrBatchCFGrp2> { }
        #endregion

        #region UsrCFGroup2
        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Casflow Group 2 & 3", Enabled = false)]
        [PXFormula(typeof(IIf<Where<CATran.origModule, Equal<BatchModule.moduleGL>>, usrBatchCFGrp2, IIf<Where<CATran.origModule, Equal<BatchModule.moduleAR>>, usrARPaymentCFGrp2, usrAPPaymentCFGrp2>>))]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<CFGROUP2Attr>>>),
                    DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string UsrCFGroup2 { get; set; }
        public abstract class usrCFGroup2 : PX.Data.BQL.BqlString.Field<usrCFGroup2> { }
        #endregion
    }
}
