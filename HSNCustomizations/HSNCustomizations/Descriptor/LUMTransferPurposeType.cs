using PX.Data;

namespace HSNCustomizations.Descriptor
{
    public class LUMTransferPurposeType : PXStringListAttribute
    {
		public const string Transfer = "TRX";
		public const string Receipt  = "RCT";
		public const string PartReq  = "PRQ";
		public const string PartRcv  = "PRV";
        public const string RMAInit  = "RMI";
		public const string RMARetu  = "RMR";
		public const string RMARcpt  = "RMC";

		public LUMTransferPurposeType(): base(new[] { Transfer, Receipt, PartReq, PartRcv, RMAInit, RMARetu, RMARcpt },
											  new[] { PX.Objects.FA.Messages.Transfer, PX.Objects.EP.Messages.Receipt, HSNMessages.PartRequest, HSNMessages.PartReceive, HSNMessages.RMAInitiated, HSNMessages.RMAReturned, HSNMessages.RMAReceipted }){ }

		public sealed class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { }
		}

		public sealed class receipt : PX.Data.BQL.BqlString.Constant<receipt>
        {
			public receipt() : base(Receipt) { }
        }

		public sealed class partReq : PX.Data.BQL.BqlString.Constant<partReq>
		{
			public partReq() : base(PartReq) { }
		}

		public sealed class partRcv : PX.Data.BQL.BqlString.Constant<partRcv>
		{
			public partRcv() : base(PartRcv) { }
		}

		public sealed class rMAInit : PX.Data.BQL.BqlString.Constant<rMAInit>
		{
			public rMAInit() : base(RMAInit) { }
		}

		public sealed class rMARetu : PX.Data.BQL.BqlString.Constant<rMARetu>
		{
			public rMARetu() : base(RMARetu) { }
		}

		public sealed class rMARcpt : PX.Data.BQL.BqlString.Constant<rMARcpt>
		{
			public rMARcpt() : base(RMARcpt) { }
		}
	}
}
