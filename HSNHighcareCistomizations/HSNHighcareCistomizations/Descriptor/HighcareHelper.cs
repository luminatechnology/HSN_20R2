using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Descriptor
{
    public class HighcareHelper
    {
        public INItemClass GetItemclass(int? inventoryID)
        {
            return SelectFrom<INItemClass>
                   .InnerJoin<InventoryItem>.On<INItemClass.itemClassID.IsEqual<InventoryItem.itemClassID>>
                   .Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>
                   .View.Select(new PXGraph(), inventoryID).RowCast<INItemClass>().FirstOrDefault();
        }

        public FSEquipment GetEquipmentInfo(int? equipmentID)
        {
            return FSEquipment.PK.Find(new PXGraph(), equipmentID);
        }
    }
}
