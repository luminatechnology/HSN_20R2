using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data.BQL;

namespace PX.Objects.FS
{
    public class SM_SOInvoiceEntry_Extension : PXGraphExtension<SM_SOInvoiceEntry, SOInvoiceEntry>
    {
        #region Delegate Method
        public delegate void CreateInvoiceDelegate(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID,
                                                   OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow);
        [PXOverride]
        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID,
                                          OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow,
                                          CreateInvoiceDelegate baseMethod)
        {
            if (docLines.Count == 0)
            {
                return;
            }

            bool? initialHold = false;

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            Base.FieldDefaulting.AddHandler<ARInvoice.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            ARInvoice arInvoiceRow = new ARInvoice();

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            if (invtMult >= 0)
            {
                /// <summary>
                /// Add the following logic per spec [Design Concept - Service Order Enhancement-V1.7] 1.10.2
                /// </summary>
                string doctype = ARInvoiceType.Invoice;

                if (hSNSetup?.EnableChgInvTypeOnBill ?? false)
                {
                    CustomerClass custClass = CustomerClass.PK.Find(Base, Customer.PK.Find(Base, fsServiceOrderRow.BillCustomerID)?.CustomerClassID);

                    doctype = custClass?.GetExtension<CustomerClassExt>().UsrInvoiceDocType ?? doctype;
                }

                arInvoiceRow.DocType = doctype; //ARInvoiceType.Invoice;
                AutoNumberHelper.CheckAutoNumbering(Base, Base.ARSetup.SelectSingle().InvoiceNumberingID);
            }
            else
            {
                arInvoiceRow.DocType = ARInvoiceType.CreditMemo;
                AutoNumberHelper.CheckAutoNumbering(Base, Base.ARSetup.SelectSingle().CreditAdjNumberingID);
            }

            arInvoiceRow.DocDate = invoiceDate;
            arInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            arInvoiceRow.InvoiceNbr = fsServiceOrderRow.CustPORefNbr;
            arInvoiceRow = Base.Document.Insert(arInvoiceRow);
            initialHold = arInvoiceRow.Hold;
            arInvoiceRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, arInvoiceRow);

            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.hold>(Base.Document.Cache, arInvoiceRow, true);
            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.customerID>(Base.Document.Cache, arInvoiceRow, fsServiceOrderRow.BillCustomerID);
            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.customerLocationID>(Base.Document.Cache, arInvoiceRow, fsServiceOrderRow.BillLocationID);
            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.curyID>(Base.Document.Cache, arInvoiceRow, fsServiceOrderRow.CuryID);

            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.taxZoneID>(Base.Document.Cache, arInvoiceRow, fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID);
            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.taxCalcMode>(Base.Document.Cache, arInvoiceRow, fsAppointmentRow != null ? fsAppointmentRow.TaxCalcMode : fsServiceOrderRow.TaxCalcMode);

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(graphProcess, fsServiceOrderRow.BillCustomerID, null);
            if (termsID != null)
            {
                FSGraphHeler.SetValueExtIfDifferent<ARInvoice.termsID>(Base.Document.Cache, arInvoiceRow, termsID);
            }
            else
            {
                FSGraphHeler.SetValueExtIfDifferent<ARInvoice.termsID>(Base.Document.Cache, arInvoiceRow, fsSrvOrdTypeRow.DfltTermIDARSO);
            }

            if (fsServiceOrderRow.ProjectID != null)
            {
                FSGraphHeler.SetValueExtIfDifferent<ARInvoice.projectID>(Base.Document.Cache, arInvoiceRow, fsServiceOrderRow.ProjectID);
            }

            FSGraphHeler.SetValueExtIfDifferent<ARInvoice.docDesc>(Base.Document.Cache, arInvoiceRow, fsServiceOrderRow.DocDesc);
            arInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            arInvoiceRow = Base.Document.Update(arInvoiceRow);

            if (hSNSetup?.EnableChgInvTypeOnBill ?? false)
            {
                InvoicingFuncations2.SetContactAndAddressFromSOContact(Base, fsServiceOrderRow, arInvoiceRow.DocType == ARDocType.CashSale);
            }
            else
            {
                InvoicingFunctions.SetContactAndAddress(Base, fsServiceOrderRow);
            }

            if (onDocumentHeaderInserted != null)
            {
                onDocumentHeaderInserted(Base, arInvoiceRow);
            }

            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();
            FSSODet soDet = null;
            FSAppointmentDet appointmentDet = null;
            ARTran arTranRow = null;

            foreach (DocLineExt docLineExt in docLines)
            {
                soDet = docLineExt.fsSODet;
                appointmentDet = docLineExt.fsAppointmentDet;

                bool isLotSerialRequired = SharedFunctions.IsLotSerialRequired(Base.Transactions.Cache, docLineExt.docLine.InventoryID);

                if (isLotSerialRequired)
                {
                    bool validateLotSerQty = false;
                    decimal? qtyInserted = 0m;
                    if (fsAppointmentRow == null) // is posted by Service Order
                    {
                        foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
                                                       Where<
                                                            FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                            And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                            And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>,
                                                       OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
                                                       .Select(Base, soDet.SrvOrdType, soDet.RefNbr, soDet.LineNbr)
                                                       .RowCast<FSSODetSplit>()
                                                       .Where(x => string.IsNullOrEmpty(x.LotSerialNbr) == false))
                        {
                            arTranRow = Base1.InsertSOInvoiceLine(
                                                            graphProcess,
                                                            arInvoiceRow,
                                                            docLineExt,
                                                            invtMult,
                                                            split.Qty,
                                                            split.UOM,
                                                            split.SiteID,
                                                            split.LocationID,
                                                            split.LotSerialNbr,
                                                            onTransactionInserted,
                                                            componentList);
                            validateLotSerQty = true;
                            qtyInserted += arTranRow.Qty;
                        }
                    }
                    else if (fsAppointmentRow != null)
                    {
                        foreach (FSApptLineSplit split in PXSelect<FSApptLineSplit,
                                                           Where<
                                                                FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
                                                                And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
                                                                And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>>>>,
                                                           OrderBy<Asc<FSApptLineSplit.splitLineNbr>>>
                                                           .Select(Base, appointmentDet.SrvOrdType, appointmentDet.RefNbr, appointmentDet.LineNbr)
                                                           .RowCast<FSApptLineSplit>()
                                                           .Where(x => string.IsNullOrEmpty(x.LotSerialNbr) == false))
                        {
                            arTranRow = Base1.InsertSOInvoiceLine(
                                                            graphProcess,
                                                            arInvoiceRow,
                                                            docLineExt,
                                                            invtMult,
                                                            split.Qty,
                                                            split.UOM,
                                                            split.SiteID,
                                                            split.LocationID,
                                                            split.LotSerialNbr,
                                                            onTransactionInserted,
                                                            componentList);
                            validateLotSerQty = true;
                            qtyInserted += arTranRow.Qty;
                        }
                    }

                    if (validateLotSerQty == false)
                    {
                        arTranRow = Base1.InsertSOInvoiceLine(graphProcess,
                                                              arInvoiceRow,
                                                              docLineExt,
                                                              invtMult,
                                                              docLineExt.docLine.GetQty(FieldType.BillableField),
                                                              docLineExt.docLine.UOM,
                                                              docLineExt.docLine.SiteID,
                                                              docLineExt.docLine.SiteLocationID,
                                                              docLineExt.docLine.LotSerialNbr,
                                                              onTransactionInserted,
                                                              componentList);
                    }
                    else
                    {
                        if (qtyInserted != docLineExt.docLine.GetQty(FieldType.BillableField))
                        {
                            throw new PXException(TX.Error.QTY_POSTED_ERROR);
                        }
                    }
                }
                else
                {
                    arTranRow = Base1.InsertSOInvoiceLine(graphProcess,
                                                          arInvoiceRow,
                                                          docLineExt,
                                                          invtMult,
                                                          docLineExt.docLine.GetQty(FieldType.BillableField),
                                                          docLineExt.docLine.UOM,
                                                          docLineExt.docLine.SiteID,
                                                          docLineExt.docLine.SiteLocationID,
                                                          docLineExt.docLine.LotSerialNbr,
                                                          onTransactionInserted,
                                                          componentList);
                }
            }

            if (componentList.Count > 0)
            {
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                {
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.NONE)))
                    {
                        if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                        {
                            currLineComponent.fsxARTranRow.ComponentID = currLineComponent.componentID;
                            currLineComponent.fsxARTranRow.NewTargetEquipmentLineNbr = currLineModel.currentLineRef;
                        }
                    }
                }
            }

            arInvoiceRow = Base.Document.Update(arInvoiceRow);

            if (Base.ARSetup.Current.RequireControlTotal == true)
            {
                FSGraphHeler.SetValueExtIfDifferent<ARInvoice.curyOrigDocAmt>(Base.Document.Cache, arInvoiceRow, arInvoiceRow.CuryDocBal);
            }

            if (initialHold != true || quickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
            {
                FSGraphHeler.SetValueExtIfDifferent<ARInvoice.hold>(Base.Document.Cache, arInvoiceRow, false);
            }

            arInvoiceRow = Base.Document.Update(arInvoiceRow);

            // Add new Trans Row for Service order prepayment data
            GetPrepaymentRemaining(fsServiceOrderRow);
            if (fsServiceOrderRow != null && arInvoiceRow.DocType == "CSL" && fsServiceOrderRow?.SOPrepaymentRemaining > 0)
            {
                var fsAdjd = SelectFrom<FSAdjust>
                            .Where<FSAdjust.adjdOrderType.IsEqual<P.AsString>
                                  .And<FSAdjust.adjdOrderNbr.IsEqual<P.AsString>>>
                            .View.Select(Base, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr).RowCast<FSAdjust>().FirstOrDefault();
                var customerInfo = Customer.PK.Find(Base, fsServiceOrderRow.CustomerID);
                if (fsAdjd != null && customerInfo != null)
                {
                    var newLine = new ARTran();
                    newLine.TranDesc = fsAdjd.AdjgRefNbr;
                    newLine.Qty = 1;
                    newLine.CuryUnitPrice = fsServiceOrderRow?.SOPrepaymentRemaining * -1;
                    newLine.CuryExtPrice = fsServiceOrderRow?.SOPrepaymentRemaining * -1;
                    newLine.AccountID = customerInfo.PrepaymentAcctID;
                    newLine.SubID = customerInfo.PrepaymentSubID;
                    Base.InsertInvoiceDirectLine(newLine);
                }
            }
        }
        #endregion

        #region Method

        public void GetPrepaymentRemaining(FSServiceOrder fsServiceOrderRow)
        {
            if(fsServiceOrderRow == null)
                return;
            PXResultset<ARPayment> resultSet = null;

            resultSet = (PXResultset<ARPayment>)PXSelectJoin<ARPayment,
                                                 InnerJoin<FSAdjust,
                                                 On<
                                                     ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                                                     And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                                                 Where<
                                                     FSAdjust.adjdOrderType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                                     And<FSAdjust.adjdOrderNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                                 .Select(new PXGraph(), fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr);

            fsServiceOrderRow.SOCuryUnpaidBalanace = fsServiceOrderRow.CuryDocTotal;
            fsServiceOrderRow.SOCuryBillableUnpaidBalanace = fsServiceOrderRow.SOCuryCompletedBillableTotal;
            fsServiceOrderRow.SOPrepaymentRemaining = 0;
            foreach (PXResult<ARPayment> row in resultSet)
            {
                ARPayment arPaymentRow = (ARPayment)row;
                fsServiceOrderRow.SOPrepaymentRemaining += (arPaymentRow.CuryDocBal ?? 0m) - ((arPaymentRow.CuryApplAmt ?? 0) + (arPaymentRow.CurySOApplAmt ?? 0));
            }
        }

        #endregion

    }

    #region Static Class & Methods
    /// <summary>
    /// Copy the following static methods from PX.Objects.FS.GraphHelper
    /// </summary>
    public static class FSGraphHeler
    {
        public const string SpecifyValueError = "The following error occurred on specifying a value in {0}: {1}";

        public static void SetValueExtIfDifferent<Field>(this PXCache cache, object data, object newValue, bool verifyAcceptanceOfNewValue = true) where Field : IBqlField
        {
            object currentValue = cache.GetValue<Field>(data);
            if ((currentValue == null && newValue != null) || (currentValue != null && newValue == null) || (currentValue != null && !currentValue.Equals(newValue)))
            {
                cache.SetValueExt<Field>(data, newValue);
                if (verifyAcceptanceOfNewValue)
                {
                    currentValue = cache.GetValue<Field>(data);
                    if (!AreEquivalentValues(currentValue, newValue))
                    {
                        string fieldMessage = string.Empty;
                        PXFieldState fieldState;
                        try
                        {
                            fieldState = (PXFieldState)cache.GetStateExt<Field>(data);
                        }
                        catch
                        {
                            fieldState = null;
                        }
                        if (fieldState != null && fieldState.Error != null)
                        {
                            fieldMessage = fieldState.Error;
                        }
                        throw new PXException(SpecifyValueError, new object[]
                        {
                            PXUIFieldAttribute.GetDisplayName<Field>(cache),
                            fieldMessage
                        });
                    }
                }
            }
        }

        public static bool AreEquivalentValues(object value1, object value2)
        {
            if (value1 != null)
            {
                return AreEquivalentValuesBasedOnValue1Type(value1, value2);
            }
            return value2 == null || AreEquivalentValuesBasedOnValue1Type(value2, value1);
        }

        public static bool AreEquivalentValuesBasedOnValue1Type(object value1, object value2)
        {
            if (value1 == null)
            {
                throw new ArgumentException();
            }
            if (value1 is string)
            {
                return AreEquivalentStrings((string)value1, value2);
            }
            if (value1 is decimal || value1 is double)
            {
                return AreEquivalentDecimals((decimal)value1, value2);
            }
            return value1.Equals(value2);
        }

        public static bool AreEquivalentStrings(string value1, object value2)
        {
            if (value1 == null)
            {
                throw new ArgumentException();
            }
            string str = value1.Trim();
            string str2 = string.Empty;
            if (value2 != null)
            {
                if (!(value2 is string))
                {
                    return false;
                }
                str2 = ((string)value2).Trim();
            }
            return str.Equals(str2);
        }

        public static bool AreEquivalentDecimals(decimal value1, object value2)
        {
            decimal dec2 = 0.0m;
            if (value2 == null)
            {
                return false;
            }
            if (!(value2 is decimal) && !(value2 is double))
            {
                return false;
            }
            dec2 = (decimal)value2;
            return Math.Round(value1, 2, MidpointRounding.AwayFromZero) == Math.Round(dec2, 2, MidpointRounding.AwayFromZero);
        }
    }
    #endregion
}
