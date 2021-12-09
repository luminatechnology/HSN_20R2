using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class FSEquipmentExtension : PXCacheExtension<FSEquipment>
    {
        #region UsrPINCode
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "PIN Code")]
        public virtual string UsrPINCode { get; set; }
        public abstract class usrPINCode : PX.Data.BQL.BqlString.Field<usrPINCode> { }
        #endregion
    }
}
