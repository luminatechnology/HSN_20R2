using PX.Data;

namespace PX.Objects.IN
{
    public class INTranExt : PXCacheExtension<PX.Objects.IN.INTran>
    {
        #region UsrApptLineRef
        [PXDBString(4, IsFixed = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Detail Ref. Nbr")]
        public virtual string UsrApptLineRef { get; set; }
        public abstract class usrApptLineRef : PX.Data.BQL.BqlString.Field<usrApptLineRef> { }
        #endregion
    }
}