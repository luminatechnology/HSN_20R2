using PX.Data;
using System;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNGUIManualStatus
    {
        public const string Open = "0";
        public const string Released= "1";

        public static readonly string[] Values =
        {
            Open,
            Released
        };

        public static readonly string[] Labels =
        {
            "Open",
            "Released"
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }
      
        public class open: PX.Data.BQL.BqlString.Constant<open>
        {
            public open() : base(Open) { }
        }
  
        public class released : PX.Data.BQL.BqlString.Constant<released>
        {
            public released() : base(Released) { }
        }
    }
}