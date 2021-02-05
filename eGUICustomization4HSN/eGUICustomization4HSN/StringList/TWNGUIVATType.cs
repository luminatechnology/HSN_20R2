using PX.Data;

namespace eGUICustomization4HSN.StringList
{
    public static class TWNGUIVATType
    {
        /// <summary>
        /// The GUI VAT type is "00".
        /// </summary>
        public const string Zero = "00";
  
        /// <summary>
        /// The GUI VAT type is "05".
        /// </summary>
        public const string Five = "05";
      
        /// <summary>
        /// The GUI VAT type is "EX".
        /// </summary>
        public const string Exclude = "EX";
  
        public static readonly string[] Values =
        {
            Zero,
            Five,
            Exclude
        };
  
        public static readonly string[] Labels =
        {
            "VAT00",
            "VAT05",
            "VATEX"
        };
  
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(Values, Labels) { }
        }
        
        public class zero: PX.Data.BQL.BqlString.Constant<zero>
        {
            public zero() : base(Zero) { }
        }   
        public class five: PX.Data.BQL.BqlString.Constant<five>
        {
            public five() : base(Five) { }
        }
        public class exclude: PX.Data.BQL.BqlString.Constant<exclude>
        {
            public exclude() : base(Exclude) { }
        }
    }
}