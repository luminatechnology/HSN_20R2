using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNGUICustomType
    {
        public const string NotThruCustom = "1";
        public const string ThruCustom    = "2";

        public static readonly string[] Values =
        {
            NotThruCustom, ThruCustom
        };

        public static readonly string[] Labels =
        {
            "Not Thru Custom", "Thru Custom"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }

        public class notThruCustom : PX.Data.BQL.BqlString.Constant<notThruCustom>
        {
            public notThruCustom() : base(NotThruCustom) { }
        }

        public class thruCustom : PX.Data.BQL.BqlString.Constant<thruCustom>
        {
            public thruCustom() : base(ThruCustom) { }
        }
    }
}
