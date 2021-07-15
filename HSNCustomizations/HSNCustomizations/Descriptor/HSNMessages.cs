using PX.Common;

namespace HSNCustomizations.Descriptor
{
    [PXLocalizable("HSN")]
    public class HSNMessages
    {
        public const string PartRequest       = "Part Request";
        public const string NonUniqueSerNbr   = "The Serial Number Isn't Unique For This Equipment Type.";
        public const string ApptLineTypeInvt  = "The Button Is Disabled Because Line Type Isn't Invetory Item";
        public const string InvtTranNoAllRlsd = "There Are Related Inventory Transactions Of This Appointment Are Not Yet Released";
    }
}
