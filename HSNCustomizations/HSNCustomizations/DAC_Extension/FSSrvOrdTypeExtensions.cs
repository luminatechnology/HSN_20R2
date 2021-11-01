using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class FSSrvOrdTypeExtensions : PXCacheExtension<FSSrvOrdType>
    {
        #region EnableEquipmentMandatory
        [PXDBBool()]
        [PXUIField(DisplayName = "Target Equipment ID is mandatory")]
        public virtual bool? UsrEnableEquipmentMandatory { get; set; }
        public abstract class usrEnableEquipmentMandatory : PX.Data.BQL.BqlBool.Field<usrEnableEquipmentMandatory> { }
        #endregion
    }
}
