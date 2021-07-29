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
    public static class FSStageControlHandler
    {
        public static IEnumerable<LumStageControl> StageControls;
        /// <summary> Get Availalbe Stage </summary>
        public static void InitialStageControl(string srvOrderType, int? currentStage)
        {
            if (!string.IsNullOrEmpty(srvOrderType) && currentStage.HasValue)
                StageControls = SelectFrom<LumStageControl>
                       .Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                                .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                       .View.Select(new PXGraph(), srvOrderType, currentStage).RowCast<LumStageControl>().ToList();
        }
    }
}
