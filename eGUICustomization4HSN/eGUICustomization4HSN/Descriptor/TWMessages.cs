using PX.Common;

namespace eGUICustomization4HSN.Descriptor
{
    [PXLocalizable("Taiwan")]
    public static class TWMessages
    {
        public const string eGUI           = "e-GUI Printing";
        public const string GUICacheName   = "GUI Transsactions";
        public const string ExportAll      = "Export All";
        public const string UploadAll      = "Upload All";
        public const string PatchPrint     = "Patch Print";
        public const string RePrint        = "Re-Print";
        public const string NetAmtNegError = "Net Amount Cannot Be Negative.";
        public const string TaxAmtNegError = "Tax Amount Cannot Be Negative.";
        public const string TaxAmtIsWrong  = "The Tax Amount Might Be Wrong, Please Double Check.";
        public const string GUINbrExisted  = "The GUI Issue/Receipt Number Already Exists.";
        public const string CNIsNotFound   = "The Original GUI Number Cannot Be Found For Credit Note.";
        public const string ManHasReleased = "Manual GUI Transaction Has Been Released.";
        public const string GUINbrIsMandat = "GUI Number Cannot Be Empty.";
        public const string ChkTotalGUIAmt = "Total GUI Tax Amout Must Equal Tax Total.";
        public const string CfmMegOnDelete = "Are You Sure To Void GUI Number?";
        public const string NoInvTaxDtls   = "No Tax Details, Please Double Check.";
        public const string DeleteInfo     = "Deleted By {0}";
        public const string TaxNbrLenNot8  = "Tax ID Number Must Be 8 Characters.";
        public const string TaxNbrWarning  = "Tax ID Number May Be Wrong, Please Check Again.";
        public const string NoPlasticBag   = "No Plastic Bag Pecified On GUI Preferences.";
        public const string StatusNotUsed  = "The GUI <{0}> Status Isn't [Used].";
        public const string FmtCodeIncort  = "The GUI <{0}> Format Code Isn't [31] Or [35].";
        public const string GUINbrLength   = "The Length Of GUI Number Should be 10 Characters";
        public const string GUINbrMini     = "The Minimum Length Of GUI Number Is 10.";
        public const string DefPrinter     = "Please Define Default Printer.";
        public const string RemainAmt      = "The Remaining Amount Isn't Enough to Settle.";
        public const string CRACIsNone     = "Please Select Credit Action When The VAT Out Code Is 33/34.";
        public const string RemindHeader   = "Reminder Header";
        public const string ReminderMesg   = "The VAT OUT Format Code Cannot Be Blank When The Tax Amount Is Greater Than 0.";
        public const string SeltCorrGUI    = "Please Select The Correct GUI Number To Print.";
    }
}
