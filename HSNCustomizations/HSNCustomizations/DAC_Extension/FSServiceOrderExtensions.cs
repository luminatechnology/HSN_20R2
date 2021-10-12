using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class FSServiceOrderExt : PXCacheExtension<FSServiceOrder>
    {
        #region UsrLastSatusModDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Last Satus ModDate")]
        public virtual DateTime? UsrLastSatusModDate { get; set; }
        public abstract class usrLastSatusModDate : PX.Data.BQL.BqlDateTime.Field<usrLastSatusModDate> { }
        #endregion
    }
}
