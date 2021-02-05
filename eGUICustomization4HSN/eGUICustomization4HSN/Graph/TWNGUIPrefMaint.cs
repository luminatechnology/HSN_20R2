using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomization4HSN.DAC;

namespace eGUICustomization4HSN.Graph
{
    public class TWNGUIPrefMaint : PXGraph<TWNGUIPrefMaint>
    { 
        public PXSave<TWNGUIPreferences> Save;
        public PXCancel<TWNGUIPreferences> Cancel;

        public SelectFrom<TWNGUIPreferences>.View GUIPreferences;
    }
}