using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.TX;
using System.Collections;
using eGUICustomization4HSN.DAC;
using eGUICustomization4HSN.Descriptor;
using eGUICustomization4HSN.Graph_Release;
using eGUICustomization4HSN.StringList;

namespace eGUICustomization4HSN.Graph
{
    public class TWNManGUIAPEntry : PXGraph<TWNManGUIAPEntry>
    {
        #region Selects & Setup
        public PXSave<TWNManualGUIAP> Save;
        public PXCancel<TWNManualGUIAP> Cancel;

        public PXSetup<TWNGUIPreferences> GUIPreferences;

        [PXImport(typeof(TWNManualGUIAP))]
        [PXFilterable]
        public SelectFrom<TWNManualGUIAP>
                          .Where<TWNManualGUIAP.status.IsEqual<TWNGUIManualStatus.open>>.View manualGUIAP_Open;

        public SelectFrom<TWNManualGUIAP>
                          .Where<TWNManualGUIAP.status.IsEqual<TWNGUIManualStatus.released>>.View.ReadOnly manualGUIAP_Released;

        public SelectFrom<TWNGUITrans>
                          .Where<TWNGUITrans.gUINbr.IsEqual<TWNManualGUIAP.gUINbr.FromCurrent>>.View ViewGUITrans;
        #endregion

        #region Action
        public PXAction<TWNManualGUIAP> Release;
        [PXProcessButton]
        [PXUIField(DisplayName = ActionsMessages.Release)]
        public IEnumerable release(PXAdapter adapter)
        {
            TWNManualGUIAP manualGUIAP = manualGUIAP_Open.Current;

            if (manualGUIAP is null || string.IsNullOrEmpty(manualGUIAP.GUINbr))
            {
                throw new PXException(TWMessages.GUINbrIsMandat);
            }
            else
            {
                PXLongOperation.StartOperation(this, delegate
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                        TWNGUITrans tWNGUITrans = rp.InitAndCheckOnAP(manualGUIAP.GUINbr, manualGUIAP.VATInCode);

                        rp.CreateGUITrans(new STWNGUITran()
                        {
                            VATCode       = manualGUIAP.VATInCode,
                            GUINbr        = manualGUIAP.GUINbr,
                            GUIStatus     = TWNGUIStatus.Used,
                            GUIDirection  = TWNGUIDirection.Receipt,
                            GUIDate       = manualGUIAP.GUIDate,
                            GUITitle      = (string)PXSelectorAttribute.GetField(manualGUIAP_Open.Cache,
                                                                                 manualGUIAP,
                                                                                 typeof(TWNManualGUIAP.vendorID).Name, manualGUIAP.VendorID,
                                                                                 typeof(Vendor.acctName).Name),
                            TaxZoneID     = manualGUIAP.TaxZoneID,
                            TaxCategoryID = manualGUIAP.TaxCategoryID,
                            TaxID         = manualGUIAP.TaxID,
                            TaxNbr        = manualGUIAP.TaxNbr,
                            OurTaxNbr     = manualGUIAP.OurTaxNbr,
                            NetAmount     = manualGUIAP.NetAmt,
                            TaxAmount     = manualGUIAP.TaxAmt,
                            AcctCD        = (string)PXSelectorAttribute.GetField(manualGUIAP_Open.Cache,
                                                                                 manualGUIAP,
                                                                                 typeof(TWNManualGUIAP.vendorID).Name, manualGUIAP.VendorID,
                                                                                 typeof(Vendor.acctCD).Name),
                            AcctName      = (string)PXSelectorAttribute.GetField(manualGUIAP_Open.Cache,
                                                                                 manualGUIAP,
                                                                                 typeof(TWNManualGUIAP.vendorID).Name, manualGUIAP.VendorID,
                                                                                 typeof(Vendor.acctName).Name),
                            DeductionCode = manualGUIAP.Deduction,
                            Remark        = manualGUIAP.Remark,
                            OrderNbr      = string.Empty
                        });

                        manualGUIAP.Status = TWNGUIManualStatus.Released;
                        manualGUIAP_Open.View.Cache.MarkUpdated(manualGUIAP);//.Update(manualGUIAP_Open.Current);

                        if (tWNGUITrans != null)
                        {
                            ViewGUITrans.SetValueExt<TWNGUITrans.netAmtRemain>(tWNGUITrans, (tWNGUITrans.NetAmtRemain -= manualGUIAP.NetAmt));
                            ViewGUITrans.SetValueExt<TWNGUITrans.taxAmtRemain>(tWNGUITrans, (tWNGUITrans.TaxAmtRemain -= manualGUIAP.TaxAmt));
                            ViewGUITrans.Update(tWNGUITrans);
                        }

                        Actions.PressSave();

                        ts.Complete();
                    }
                });
            }

            return adapter.Get();
        }
        #endregion   

        #region Event Handlers
        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        protected void _(Events.RowPersisting<TWNManualGUIAP> e)
        {
            tWNGUIValidation.CheckCorrespondingInv(this,e.Row.GUINbr, e.Row.VATInCode);

            if (tWNGUIValidation.errorOccurred.Equals(true) )
            {
                e.Cache.RaiseExceptionHandling<TWNManualGUIAP.gUINbr>(e.Row, e.Row.GUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.Error));
            }
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIAP, TWNManualGUIAP.deduction> e)
        {
            var row = (TWNManualGUIAP)e.Row;

            /// If user doesn't choose a vendor then bring the fixed default value from Attribure "DEDUCTCODE" first record.
            e.NewValue = row.VendorID is null ? "1" : e.NewValue;
        }

        protected void _(Events.FieldVerifying<TWNManualGUIAP, TWNManualGUIAP.gUINbr> e)
        {
            var row = (TWNManualGUIAP)e.Row;

            tWNGUIValidation.CheckGUINbrExisted(this, (string)e.NewValue, row.VATInCode);
        }

        protected void _(Events.FieldVerifying<TWNManualGUIAP, TWNManualGUIAP.taxAmt> e)
        {
            var row = (TWNManualGUIAP)e.Row;

            tWNGUIValidation.CheckTaxAmount((decimal)row.NetAmt, (decimal)e.NewValue);
        }

        protected void _(Events.FieldUpdated<TWNManualGUIAP, TWNManualGUIAP.vendorID> e)
        {
            var row = (TWNManualGUIAP)e.Row;

            PXResult<Location> result = SelectFrom<Location>.InnerJoin<Vendor>.On<Location.bAccountID.IsEqual<Vendor.bAccountID>
                                                                                  .And<Location.locationID.IsEqual<Vendor.defLocationID>>>
                                                            .InnerJoin<TaxZone>.On<TaxZone.taxZoneID.IsEqual<Location.vTaxZoneID>>
                                                            .InnerJoin<TaxZoneDet>.On<TaxZoneDet.taxZoneID.IsEqual<Location.vTaxZoneID>>
                                                            .Where<Vendor.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, row.VendorID);
            if (!result.Equals(null))
            {
                row.TaxZoneID     = result.GetItem<Location>().VTaxZoneID;
                row.TaxCategoryID = result.GetItem<TaxZone>().DfltTaxCategoryID;
                row.TaxID         = result.GetItem<TaxZoneDet>().TaxID;

                foreach (CSAnswers cS in SelectFrom<CSAnswers>.Where<CSAnswers.refNoteID.IsEqual<@P.AsGuid>>.View.Select(this, result.GetItem<Vendor>().NoteID))
                {
                    switch (cS.AttributeID)
                    {
                        case APRegisterExt.VATINFRMTName :
                            row.VATInCode = cS.Value;
                            break;
                        case APRegisterExt.OurTaxNbrName :
                            row.OurTaxNbr = cS.Value;
                            break;
                        case APRegisterExt.TaxNbrName :
                            row.TaxNbr = cS.Value;
                            break;
                        case APRegisterExt.DeductionName :
                            row.Deduction = cS.Value;
                            break;
                    }
                }
            }
        }

        protected void _(Events.FieldUpdated<TWNManualGUIAP, TWNManualGUIAP.netAmt> e)
        {
            var row = (TWNManualGUIAP)e.Row;

            foreach (TaxRev taxRev in SelectFrom<TaxRev>.Where<TaxRev.taxID.IsEqual<@P.AsString>
                                                               .And<TaxRev.taxType.IsEqual<TaxRev.taxType>>>.View.Select(this, row.TaxID, "P") ) // P = Group type (Input)
            { 
                row.TaxAmt = row.NetAmt * (taxRev.TaxRate / taxRev.NonDeductibleTaxRate);
            }
        }
        #endregion
    }
}