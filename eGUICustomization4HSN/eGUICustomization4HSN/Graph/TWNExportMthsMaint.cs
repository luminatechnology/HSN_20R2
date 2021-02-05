using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4HSN.DAC;

namespace eGUICustomization4HSN.Graph
{
    public class TWNExportMthsMaint : PXGraph<TWNExportMthsMaint>
    {
        public PXSavePerRow<TWNExportMethods> Save;
        public PXCancel<TWNExportMethods> Cancel;

        [PXImport(typeof(TWNExportMethods))]
        public SelectFrom<TWNExportMethods>.View ExportMethods;
    }
}