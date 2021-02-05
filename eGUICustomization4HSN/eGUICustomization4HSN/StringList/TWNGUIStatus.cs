using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNGUIStatus
    {
        public const string Used = "Used";
        public const string Voided = "Voided";
        public const string Blank = "Blank";

        public static readonly string[] Values =
        {
            Used,
            Voided,
            Blank
        };

        public static readonly string[] Labels =
        {
            "Used",
            "Voided",
            "Black"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }

        public class used : PX.Data.BQL.BqlString.Constant<used>
        {
            public used() : base(Used) { }
        }

        public class voided : PX.Data.BQL.BqlString.Constant<voided>
        {
            public voided() : base(Voided) { }
        }

        public class blank : PX.Data.BQL.BqlString.Constant<blank>
        {
            public blank() : base(Blank) { }
        }
    }
}
