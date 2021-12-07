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
        public PXSave<ServicScopeFilter> Save;
        public PXCancel<ServicScopeFilter> Cancel;
        public PXFilter<ServicScopeFilter> Filter;

        [PXFilterable]
        public SelectFrom<LUMServiceScope>
               .Where<LUMServiceScope.cPriceClassID.IsEqual<ServicScopeFilter.cPriceClassID.FromCurrent>>
               .View ScopeList;

        public virtual void _(Events.RowInserting<LUMServiceScope> e)
        {
            if (this.Filter.Current.CPriceClassID == null)
                throw new PXException("Please Select Price Class!!");
            if (e.Row is LUMServiceScope && e.Row != null && !string.IsNullOrEmpty(this.Filter.Current.CPriceClassID))
                this.ScopeList.Cache.SetValueExt<LUMServiceScope.cPriceClassID>(e.Row, this.Filter.Current.CPriceClassID);
        }
    }

    public class ServicScopeFilter : IBqlTable
    {
        #region CPriceClassID
        [PXString(10, IsUnicode = true, InputMask = "")]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Price Class ID")]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion
    }
}
