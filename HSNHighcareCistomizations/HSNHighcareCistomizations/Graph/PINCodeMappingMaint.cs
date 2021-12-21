using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Graph
{
    public class PINCodeMappingMaint : PXGraph<PINCodeMappingMaint>, PXImportAttribute.IPXPrepareItems
    {
        public PXSave<LUMPINCodeMapping> Save;
        public PXCancel<LUMPINCodeMapping> Cancel;

        [PXImport(typeof(LUMPINCodeMapping))]
        public SelectFrom<LUMPINCodeMapping>.View Transaction;

        #region Implement Import Method
        public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            return true;
        }

        public void PrepareItems(string viewName, IEnumerable items) { }

        public bool RowImported(string viewName, object row, object oldRow)
        {
            return true;
        }

        public bool RowImporting(string viewName, object row)
        {
            return true;
        } 
        #endregion
    }
}
