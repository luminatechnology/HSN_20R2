﻿using PX.Common;

namespace HSNCustomizations.Descriptor
{
    [PXLocalizable("HSN")]
    public class HSNMessages
    {
        public const string PartRequest       = "Part Request";
        public const string PartReceive       = "Part Receive";
        public const string InitiateRMA       = "Initiate RMA";
        public const string RMAInitiated      = "RMA Initiated";
        public const string ReturnRMA         = "Return RMA";
        public const string RMAReturned       = "RMA Returned";
        public const string RMAReceipted      = "RMA Receipted";
        public const string NonUniqueSerNbr   = "The Serial Number Isn't Unique For This Equipment Type.";
        public const string ApptLineTypeInvt  = "The Button Is Disabled Because Line Type Isn't Invetory Item";
        public const string InvtTranNoAllRlsd = "There Are Related Inventory Transactions Of This Appointment Are Not Yet Released";
        public const string UnitCostIsZero    = "The Unit Cost Is 0, Please Double Check.";
        public const string InitRMANotCompl   = "The Initiate RMA Receipt Not Yet Complete.";
        public const string NoInitRMARcpt     = "You Must Initiate RMA Process For The Inventory.";
        public const string MustReturnRMA     = "Please Return The RMA.";
    }
}
