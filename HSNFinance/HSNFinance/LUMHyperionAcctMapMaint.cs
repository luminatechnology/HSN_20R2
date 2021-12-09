using PX.Data;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LUMHyperionAcctMapMaint : PXGraph<LUMHyperionAcctMapMaint>
    {
        public PXSave<LUMHyperionAcctMapping> Save;
        public PXCancel<LUMHyperionAcctMapping> Cancel;

        [PXImport(typeof(LUMHyperionAcctMapping))]
        [PXFilterable()]
        public PXSelect<LUMHyperionAcctMapping> HyperionAcctMapping;

        protected virtual void _(Events.RowSelected<LUMHyperionAcctMapping> e)
        {
            PXUIFieldAttribute.SetDisplayName(e.Cache, "AccountID_Account_description", "Account Descr");
            PXUIFieldAttribute.SetDisplayName<PX.Objects.GL.Sub.description>(e.Cache, "Subaccount Descr");
        }
    }
}