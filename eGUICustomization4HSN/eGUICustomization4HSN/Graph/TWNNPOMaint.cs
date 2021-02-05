using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4HSN.DAC;

namespace eGUICustomization4HSN.Graph
{
    public class TWNNPOMaint : PXGraph<TWNNPOMaint>
    { 
        public PXSavePerRow<TWNNPOTable> Save;
        public PXCancel<TWNNPOTable> Cancel;

        [PXImport(typeof(TWNNPOTable))]
        public SelectFrom<TWNNPOTable>.View NPOTable;
    }
}