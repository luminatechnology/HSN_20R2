using PX.Data;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;

namespace HSNCustomizations
{
    public class LUMHSNSetupMaint : PXGraph<LUMHSNSetupMaint>
    {
        public PXSave<LUMHSNSetup> Save;
        public PXCancel<LUMHSNSetup> Cancel;

        public SelectFrom<LUMHSNSetup>.View hSNSetup;

        [PXImport(typeof(LUMBranchWarehouse))]
        public SelectFrom<LUMBranchWarehouse>.View BranchWarehouse;

        [PXImport(typeof(LUMTermsConditions))]
        public SelectFrom<LUMTermsConditions>.View TermsConditions;
    }
}