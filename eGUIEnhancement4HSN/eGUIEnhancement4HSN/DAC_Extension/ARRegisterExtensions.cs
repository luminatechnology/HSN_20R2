using PX.Data;

namespace PX.Objects.AR
{
    public sealed class ARRegisterExt2 : PXCacheExtension<ARRegisterExt, ARRegister>
    {
        #region UsrGUITitle
        [PXDBString(80, IsUnicode = true)]
        [PXUIField(DisplayName = "GUI Title")]
        public string UsrGUITitle { get; set; }
        public abstract class usrGUITitle : PX.Data.BQL.BqlString.Field<usrGUITitle> { }
        #endregion

        #region UsrPrnGUITitle
        [PXDBBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Print GUI Title")]
        public bool? UsrPrnGUITitle { get; set; }
        public abstract class usrPrnGUITitle : PX.Data.BQL.BqlBool.Field<usrPrnGUITitle> { }
        #endregion

        #region UsrPrnPayment
        [PXDBBool()]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Print Payment")]
        public bool? UsrPrnPayment { get; set; }
        public abstract class usrPrnPayment : PX.Data.BQL.BqlBool.Field<usrPrnPayment> { }
        #endregion

        #region UsrSummaryPrint
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Summary Printing")]
        public bool? UsrSummaryPrint { get; set; }
        public abstract class usrSummaryPrint : PX.Data.BQL.BqlBool.Field<usrSummaryPrint> { }
        #endregion

        #region UsrGUISummary
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Invoice Summary")]
        [PXSelector(typeof(Search<CS.CSAttributeDetail.valueID, Where<CS.CSAttributeDetail.attributeID, Equal<ARInvoiceEntry_Extension2.GUISmryAttr>>>),
                    typeof(CS.CSAttributeDetail.valueID),
                    DescriptionField = typeof(CS.CSAttributeDetail.description))]
        public string UsrGUISummary { get; set; }
        public abstract class usrGUISummary : PX.Data.BQL.BqlString.Field<usrGUISummary> { }
        #endregion
    }
}