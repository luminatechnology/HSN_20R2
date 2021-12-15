﻿using HSNCustomizations.DAC;
using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.FS.FSSODet;

namespace PX.Objects.FS
{
    //[PXTable(IsOptional = true)]
    public class FSSODetExtension : PXCacheExtension<FSSODet>
    {
        public static bool IsActive()
        {
            return (SelectFrom<LUMHSNSetup>.View.Select(new PXGraph()).RowCast<LUMHSNSetup>().FirstOrDefault()?.GetExtension<LUMHSNSetupExtension>().EnableHighcareFunction ?? false);
        }

        #region SMEquipmentID
        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [PXSelector(typeof(SelectFrom<FSEquipment>
                           .InnerJoin<FSSrvOrdType>.On<FSSrvOrdType.srvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>>
                           .CrossJoin<FSSetup>.SingleTableOnly
                           .Where<FSEquipment.requireMaintenance.IsEqual<True>
                               .And<FSSetup.enableAllTargetEquipment.IsEqual<True>>
                               .And<FSEquipment.ownerID.IsEqual<FSServiceOrder.customerID.FromCurrent>>>
                           .SearchFor<FSEquipment.SMequipmentID>),
                    typeof(FSEquipment.refNbr),
                    typeof(FSEquipment.descr),
                    typeof(FSEquipment.serialNumber),
                    typeof(FSEquipment.ownerType),
                    typeof(FSEquipment.ownerID),
                    typeof(FSEquipment.locationType),
                    typeof(FSEquipment.status),
            SubstituteKey = typeof(FSEquipment.refNbr))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                       TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion

    }
}