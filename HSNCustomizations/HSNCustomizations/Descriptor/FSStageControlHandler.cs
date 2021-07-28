using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Descriptor
{
    public class FSStageControlHandler
    {
        /// <summary> Get Availalbe Stage </summary>
        public virtual IEnumerable<LumStageControl> GetStageAction(string srvOrderType, int? currentStage)
        {
            return SelectFrom<LumStageControl>
                   .Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                            .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                   .View.Select(new PXGraph(),srvOrderType,currentStage).RowCast<LumStageControl>().ToList();
        }
    }
}
