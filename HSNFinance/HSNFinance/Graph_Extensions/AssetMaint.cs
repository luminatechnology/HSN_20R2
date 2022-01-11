using PX.Data;
using HSNFinance.DAC;

namespace PX.Objects.FA
{
    public class AssetMaint_Extension : PXGraphExtension<AssetMaint>
    {
        public const string IE = "IE";
        public const string InterestExp = "Interest Expense";

        public PXSelectReadonly<LUMLAInterestExp, Where<LUMLAInterestExp.assetID, Equal<Current<FixedAsset.assetID>>>> LAInterestExp;
    }
}