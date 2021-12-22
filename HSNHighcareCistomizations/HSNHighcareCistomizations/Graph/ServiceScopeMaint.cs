using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Graph
{
    public class ServiceScopeMaint : PXGraph<ServiceScopeMaint>
    {
        public PXSave<LUMServiceScopeHeader> Save;
        public PXCancel<LUMServiceScopeHeader> Cancel;

        public SelectFrom<LUMServiceScopeHeader>.View Document;

        public SelectFrom<LUMServiceScope>
               .Where<LUMServiceScope.cPriceClassID.IsEqual<LUMServiceScopeHeader.cPriceClassID.FromCurrent>>
               .View ScopeList;

        public virtual void _(Events.RowInserting<LUMServiceScope> e)
        {
            if (this.Document.Current.CPriceClassID == null)
                throw new PXException("Please Select Price Class!!");
            if (e.Row is LUMServiceScope && e.Row != null && !string.IsNullOrEmpty(this.Document.Current.CPriceClassID))
                this.ScopeList.Cache.SetValueExt<LUMServiceScope.cPriceClassID>(e.Row, this.Document.Current.CPriceClassID);
        }
    }
}
