using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;

namespace PX.Objects.IN
{
    public class INSiteMaint_Extension : PXGraphExtension<INSiteMaint>
    {
        #region Event Handlers
        protected void _(Events.RowSelected<INSite> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            PXUIFieldAttribute.SetVisible<INSiteExt.usrIsFaultySite>(e.Cache, e.Row, SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableRMAProcInAppt ?? false);
        }
        #endregion
    }
}