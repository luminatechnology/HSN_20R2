using PX.Data;

namespace HSNCustomizations.Descriptor
{
    public class LUMTransferPurposeType : PXStringListAttribute
    {
		public const string Transfer = "TRX";
		public const string PartReq  = "PRQ";
        public const string RMAName  = "RMA";

		public LUMTransferPurposeType(): base(new[] { Transfer, PartReq, RMAName },
											  new[] { PX.Objects.FA.Messages.Transfer, HSNMessages.PartRequest, RMAName }){ }

		public sealed class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { }
		}

		public sealed class partReq : PX.Data.BQL.BqlString.Constant<partReq>
		{
			public partReq() : base(PartReq) { }
		}

		public sealed class rMAName : PX.Data.BQL.BqlString.Constant<rMAName>
		{
			public rMAName() : base(RMAName) { }
		}
	}
}
