using PX.Data;

namespace PX.Objects.IN
{
    public class INSiteExt : PXCacheExtension<PX.Objects.IN.INSite>
    {
        #region UsrIsFaultySite
        [PXDBBool]
        [PXUIField(DisplayName = "Is Faulty Warehouse")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]  
        public virtual bool? UsrIsFaultySite { get; set; }
        public abstract class usrIsFaultySite : PX.Data.BQL.BqlBool.Field<usrIsFaultySite> { }
        #endregion
    }
}