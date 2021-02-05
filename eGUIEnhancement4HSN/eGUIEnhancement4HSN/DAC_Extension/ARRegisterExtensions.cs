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
	}
}