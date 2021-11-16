using PX.Common;

namespace HSNCustomizations.Descriptor
{
    [PXLocalizable("HSN")]
    public class HSNMessages
    {
        public const string Attention_Attr   = "ATTENTION";
        public const string CompanyName_Attr = "COMPANYNAM";

        public const string PartRequest        = "Part Request";
        public const string PartReceive        = "Part Receive";
        public const string InitiateRMA        = "Initiate RMA";
        public const string RMAInitiated       = "RMA Initiated";
        public const string ReturnRMA          = "Return RMA";
        public const string RMAReturned        = "RMA Returned";
        public const string RMAReceipted       = "RMA Received";
        public const string NonUniqueSerNbr    = "The Serial Number Isn't Unique For This Equipment Type.";
        public const string ApptLineTypeInvt   = "The Button Is Disabled Because Line Type Isn't Invetory Item";
        public const string InvtTranNoAllRlsd  = "There Are Related Inventory Transactions Of This Appointment Are Not Yet Released";
        public const string UnitCostIsZero     = "The Unit Cost Is 0, Please Double Check.";
        public const string InitRMANotCompl    = "The Initiate RMA Receipt Not Yet Complete.";
        public const string NoInitRMARcpt      = "You Must Initiate RMA Process For The Inventory.";
        public const string MustReturnRMA      = "Please Return The RMA.";
        public const string ReturnRMAB4Init    = "Please Initiate RMA Before Return RMA";
        public const string PartReqNotRlsd     = "Please Request Part Before Receiving And Eensure Part Request Is Released";
        public const string TotalQtyIsZero     = "System Cannot Save Records With 0 Quantity.";
        public const string DuplicSortOrder    = "The Sort Order Cannot Be Repeated.";
        public const string SortOrdMustGreater = "The Sort Order Must Be Greater Than The Last One.";
        public const string NoPartRequest      = "All Parts Requested Have Been Submitted.";
        public const string WHLocDiffFromAppt  = "The Warehouse / Warehouse Location Is Different Between The Receipts And Appointment Details.";
        public const string StartApptNoStaff   = "You Cannot Start Appointment Without Staff Assigned";
    }
}
