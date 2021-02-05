using System;
using PX.Data;
using PX.Objects.AR;
using eGUICustomization4HSN.DAC;

namespace eGUICustomization4HSN.Descriptor
{
    public class ARGUINbrAutoNumAttribute : PX.Objects.CS.AutoNumberAttribute
    {
        public ARGUINbrAutoNumAttribute(Type doctypeField, Type dateField) : base(doctypeField, dateField) { }

        public ARGUINbrAutoNumAttribute(Type doctypeField, Type dateField, string[] doctypeValues, Type[] setupFields) : 
                                        base(doctypeField, dateField, doctypeValues, setupFields) { }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Operation == PXDBOperation.Delete) { return; }

            string vATOutCode = string.Empty;

            if (this.BqlTable.Name == nameof(DAC.TWNManualGUIAR))
            {
                var row = (TWNManualGUIAR)e.Row;

                vATOutCode = row.VatOutCode;
            }
            else
            {
                var row = (ARRegister)e.Row;

                vATOutCode = PXCache<ARRegister>.GetExtension<ARRegisterExt>(row).UsrVATOutCode;
            }

            if (vATOutCode != TWGUIFormatCode.vATOutCode33 && vATOutCode != TWGUIFormatCode.vATOutCode34 && vATOutCode != null)
            {
                base.RowPersisting(sender, e);
            }
            
            sender.SetValue(e.Row, _FieldName, (string)sender.GetValue(e.Row, _FieldName));
        }
    }
}