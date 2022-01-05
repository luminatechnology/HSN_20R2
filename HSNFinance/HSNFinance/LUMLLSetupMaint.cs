using PX.Data;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LUMLLSetupMaint : PXGraph<LUMLLSetupMaint>
    {
        public PXSave<LUMLLSetup> Save;
        public PXCancel<LUMLLSetup> Cancel;
      
        public PXSelect<LUMLLSetup> SetupRecord;
    }
}