using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CA
{
    public class CATranExt : PXCacheExtension<PX.Objects.CA.CATran>
    {
        #region UsrCFGroup1
        [PXString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Casflow Group 1st", Enabled = false)]
        public virtual string UsrCFGroup1 { get; set; }
        public abstract class usrCFGroup1 : PX.Data.BQL.BqlString.Field<usrCFGroup1> { }
        #endregion

        #region UsrCFGroup2
        [PXString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Casflow Group 2 & 3", Enabled = false)]
        public virtual string UsrCFGroup2 { get; set; }
        public abstract class usrCFGroup2 : PX.Data.BQL.BqlString.Field<usrCFGroup2> { }
        #endregion
    }
}
