using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNCreditAction
    {
        public const string NO = "None";
        public const string CN = "CreditNote";
        public const string VG = "VoidedGUI";

        public static readonly string[] Values =
        {
            CN, VG
        };

        public static readonly string[] Labels =
        {
            "Credit Note", "Voided GUI"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }

        public class no : PX.Data.BQL.BqlString.Constant<no>
        {
            public no() : base(NO) { }
        }
        public class cn : PX.Data.BQL.BqlString.Constant<cn>
        {
            public cn() : base(CN) { }
        }
        public class vg : PX.Data.BQL.BqlString.Constant<vg>
        {
            public vg() : base(VG) { }
        }
    }
}
