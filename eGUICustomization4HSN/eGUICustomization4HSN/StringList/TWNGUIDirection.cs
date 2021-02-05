using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNGUIDirection
    {
        public const string Issue   = "GUI Issue";
        public const string Receipt = "GUI Receipt";

        public static readonly string[] Values =
        {
            Issue,
            Receipt
        };

        public static readonly string[] Labels =
        {
            "GUI Issue",
            "GUI Receipt"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }

        public class issue : PX.Data.BQL.BqlString.Constant<issue>
        {
            public issue() : base(Issue) { }
        }

        public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
        {
            public receipt() : base(Receipt) { }
        }
    }
}
