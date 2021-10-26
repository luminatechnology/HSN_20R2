using System;
using PX.Data;

namespace HSNCustomizations
{
    [Serializable]
    [PXCacheName("v_APRegisterUDFAttir")]
    public class v_APRegisterUDFAttir : IBqlTable
    {
        #region DocType
        [PXDBString(3, IsFixed = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Doc Type")]
        public virtual string DocType { get; set; }
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion

        #region FieldName
        [PXDBString(50, InputMask = "")]
        [PXUIField(DisplayName = "Field Name")]
        public virtual string FieldName { get; set; }
        public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }
        #endregion

        #region ValueString
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Value String")]
        public virtual string ValueString { get; set; }
        public abstract class valueString : PX.Data.BQL.BqlString.Field<valueString> { }
        #endregion

        #region ValueText
        [PXDBString(IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Value Text")]
        public virtual string ValueText { get; set; }
        public abstract class valueText : PX.Data.BQL.BqlString.Field<valueText> { }
        #endregion
    }
}