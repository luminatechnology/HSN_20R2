using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNB2CType
    {
        public const string DEF = "NA";
        public const string MC  = "MC";
        public const string NPO = "NPO";

        public static readonly string[] Values =
        {
            DEF, MC, NPO
        };

        public static readonly string[] Labels =
        {
            "NA", "MC", "NPO"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }

        public class na : PX.Data.BQL.BqlString.Constant<na>
        {
            public na() : base(DEF) { }
        }
        public class mc : PX.Data.BQL.BqlString.Constant<mc>
        {
            public mc() : base(MC) { }
        }
        public class npo : PX.Data.BQL.BqlString.Constant<npo>
        {
            public npo() : base(NPO) { }
        }
    }
}
