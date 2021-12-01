using PX.Data;
using PX.Data.BQL.Fluent;
using HSNFinance.DAC;

namespace HSNFinance
{
    public class LSStlmtAccountMaint : PXGraph<LSStlmtAccountMaint>
    {
        public PXSavePerRow<LSSettlementAccount> Save;
        public PXCancel<LSSettlementAccount> Cancel;
  
        [PXFilterable()]
        public SelectFrom<LSSettlementAccount>.View StlmtAccount;
    }
}