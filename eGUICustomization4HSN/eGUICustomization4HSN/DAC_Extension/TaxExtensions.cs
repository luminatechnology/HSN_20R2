using PX.Data;
using eGUICustomization4HSN.StringList;

namespace PX.Objects.TX
{
    public class TaxExt : PXCacheExtension<PX.Objects.TX.Tax>
    {
        #region UsrTWNGUI
        [PXDBBool()]
        [PXUIField(DisplayName = "GUI Enabled")]
        public virtual bool? UsrTWNGUI { get; set; }
        public abstract class usrTWNGUI : IBqlField { }
        #endregion

        #region UsrGUIType
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "GUI VAT Type")]
        [TWNGUIVATType.List]
        public virtual string UsrGUIType { get; set; }
        public abstract class usrGUIType : IBqlField { }
        #endregion
    }
}