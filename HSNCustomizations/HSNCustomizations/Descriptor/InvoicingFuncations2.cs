using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.FS;
using PX.Objects.SO;
using static PX.Objects.FS.InvoicingFunctions;

namespace HSNCustomizations.Descriptor
{
    public class InvoicingFuncations2
    {
        public static void SetContactAndAddressFromSOContact(PXGraph graph, FSServiceOrder fsServiceOrderRow, bool isCachSale)
        {
            int? billCustomerID = fsServiceOrderRow.BillCustomerID;
            int? billLocationID = fsServiceOrderRow.BillLocationID;
            Customer billCustomer = SharedFunctions.GetCustomerRow(graph, billCustomerID);

            ContactAddressSource contactAddressSource = GetBillingContactAddressSource(graph, fsServiceOrderRow, billCustomer);

            if (contactAddressSource == null
                || (contactAddressSource != null && string.IsNullOrEmpty(contactAddressSource.BillingSource)))
            {
                throw new PXException(TX.Error.MISSING_CUSTOMER_BILLING_ADDRESS_SOURCE);
            }

            IAddress addressRow = null;
            IContact contactRow = null;

            switch (contactAddressSource.BillingSource)
            {
                case ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO:
                    if (isCachSale == true)
                    {
                        contactRow = FSContact.PK.Find(graph, fsServiceOrderRow.ServiceOrderContactID);
                        addressRow = FSAddress.PK.Find(graph, fsServiceOrderRow.ServiceOrderAddressID);
                        ///<remarks> Per user's request to do the following customization.</remarks>
                        if (contactRow != null && fsServiceOrderRow.ContactID != null)
                        {
                            contactRow.FullName = Contact.PK.Find(graph, fsServiceOrderRow.ContactID).DisplayName;
                        }
                    }
                    else
                    {
                        contactRow = ContactAddressHelper.GetIContact(PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(graph, billCustomer.DefBillContactID));
                        addressRow = ContactAddressHelper.GetIAddress(PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(graph, billCustomer.DefBillAddressID));
                    }
                    break;

                case ID.Send_Invoices_To.SO_BILLING_CUSTOMER_LOCATION:
                    PXResult<Location, Contact, Address> locData = (PXResult<Location, Contact, Address>)PXSelectJoin<Location, LeftJoin<Contact, On<Contact.contactID, Equal<Location.defContactID>>,
                                                                                                                                         LeftJoin<Address, On<Address.addressID, Equal<Location.defAddressID>>>>,
                                                                                                                                Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(graph, billLocationID);

                    if (locData != null)
                    {
                        addressRow = ContactAddressHelper.GetIAddress(locData);
                        contactRow = ContactAddressHelper.GetIContact(locData);
                    }
                    break;

                case ID.Send_Invoices_To.SERVICE_ORDER_ADDRESS:
                    GetSrvOrdContactAddress(graph, fsServiceOrderRow, out FSContact fsContact, out FSAddress fsAddress);
                    contactRow = fsContact;
                    addressRow = fsAddress;
                    break;

                default:
                    PXResult<Location, Customer, Contact, Address> defaultLocData = (PXResult<Location, Customer, Contact, Address>)
                                                                                     PXSelectJoin<Location, InnerJoin<Customer, On<Customer.bAccountID, Equal<Location.bAccountID>,
                                                                                                                                   And<Customer.defLocationID, Equal<Location.locationID>>>,
                                                                                                                      LeftJoin<Contact, On<Contact.contactID, Equal<Location.defContactID>>,
                                                                                                                               LeftJoin<Address,On<Address.addressID, Equal<Location.defAddressID>>>>>,
                                                                                                            Where<Location.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(graph, billCustomerID);

                    if (defaultLocData != null)
                    {
                        addressRow = ContactAddressHelper.GetIAddress(defaultLocData);
                        contactRow = ContactAddressHelper.GetIContact(defaultLocData);
                    }
                    break;
            }

            if (addressRow == null)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.MessageParm.ADDRESS), PXErrorLevel.Error);
            }

            if (contactRow == null)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.MessageParm.CONTACT), PXErrorLevel.Error);
            }

            if (graph is SOOrderEntry)
            {
                SOOrderEntry SOgraph = (SOOrderEntry)graph;

                SOBillingContact billContact = new SOBillingContact();
                SOBillingAddress billAddress = new SOBillingAddress();

                ContactAddressHelper.CopyContact(billContact, contactRow);
                billContact.CustomerID = SOgraph.customer.Current.BAccountID;
                billContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(billAddress, addressRow);
                billAddress.CustomerID = SOgraph.customer.Current.BAccountID;
                billAddress.CustomerAddressID = SOgraph.customer.Current.DefAddressID;
                billAddress.RevisionID = 0;

                billContact.IsDefaultContact = false;
                billAddress.IsDefaultAddress = false;

                SOgraph.Billing_Contact.Current = billContact = SOgraph.Billing_Contact.Insert(billContact);
                SOgraph.Billing_Address.Current = billAddress = SOgraph.Billing_Address.Insert(billAddress);

                SOgraph.Document.Current.BillAddressID = billAddress.AddressID;
                SOgraph.Document.Current.BillContactID = billContact.ContactID;

                addressRow = null;
                contactRow = null;

                GetShippingContactAddress(graph, contactAddressSource.ShippingSource, billCustomerID, fsServiceOrderRow, out contactRow, out addressRow);

                if (addressRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_ADDRESS), PXErrorLevel.Error);
                }

                if (contactRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_CONTACT), PXErrorLevel.Error);
                }

                SOShippingContact shipContact = new SOShippingContact();
                SOShippingAddress shipAddress = new SOShippingAddress();

                ContactAddressHelper.CopyContact(shipContact, contactRow);
                shipContact.CustomerID = SOgraph.customer.Current.BAccountID;
                shipContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(shipAddress, addressRow);
                shipAddress.CustomerID = SOgraph.customer.Current.BAccountID;
                shipAddress.CustomerAddressID = SOgraph.customer.Current.DefAddressID;
                shipAddress.RevisionID = 0;

                shipContact.IsDefaultContact = false;
                shipAddress.IsDefaultAddress = false;

                SOgraph.Shipping_Contact.Current = shipContact = SOgraph.Shipping_Contact.Insert(shipContact);
                SOgraph.Shipping_Address.Current = shipAddress = SOgraph.Shipping_Address.Insert(shipAddress);

                SOgraph.Document.Current.ShipAddressID = shipAddress.AddressID;
                SOgraph.Document.Current.ShipContactID = shipContact.ContactID;
            }
            else if (graph is ARInvoiceEntry)
            {
                ARInvoiceEntry ARgraph = (ARInvoiceEntry)graph;

                ARContact arContact = new ARContact();
                ARAddress arAddress = new ARAddress();

                ContactAddressHelper.CopyContact(arContact, contactRow);
                arContact.CustomerID = ARgraph.customer.Current.BAccountID;
                arContact.RevisionID = 0;
                arContact.IsDefaultContact = false;

                ContactAddressHelper.CopyAddress(arAddress, addressRow);
                arAddress.CustomerID = ARgraph.customer.Current.BAccountID;
                arAddress.CustomerAddressID = ARgraph.customer.Current.DefAddressID;
                arAddress.RevisionID = 0;
                arAddress.IsDefaultBillAddress = false;

                ARgraph.Billing_Contact.Current = arContact = ARgraph.Billing_Contact.Update(arContact);
                ARgraph.Billing_Address.Current = arAddress = ARgraph.Billing_Address.Update(arAddress);

                ARgraph.Document.Current.BillAddressID = arAddress.AddressID;
                ARgraph.Document.Current.BillContactID = arContact.ContactID;

                addressRow = null;
                contactRow = null;

                GetShippingContactAddress(graph, contactAddressSource.ShippingSource, billCustomerID, fsServiceOrderRow, out contactRow, out addressRow);

                if (addressRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_ADDRESS), PXErrorLevel.Error);
                }

                if (contactRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_CONTACT), PXErrorLevel.Error);
                }

                ARShippingContact shipContact = new ARShippingContact();
                ARShippingAddress shipAddress = new ARShippingAddress();

                ContactAddressHelper.CopyContact(shipContact, contactRow);
                shipContact.CustomerID = ARgraph.customer.Current.BAccountID;
                shipContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(shipAddress, addressRow);
                shipAddress.CustomerID = ARgraph.customer.Current.BAccountID;
                shipAddress.CustomerAddressID = ARgraph.customer.Current.DefAddressID;
                shipAddress.RevisionID = 0;

                shipContact.IsDefaultContact = false;
                shipAddress.IsDefaultAddress = false;

                ARgraph.Shipping_Contact.Current = shipContact = ARgraph.Shipping_Contact.Insert(shipContact);
                ARgraph.Shipping_Address.Current = shipAddress = ARgraph.Shipping_Address.Insert(shipAddress);

                ARgraph.Document.Current.ShipAddressID = shipAddress.AddressID;
                ARgraph.Document.Current.ShipContactID = shipContact.ContactID;
            }
        }

        private static void GetShippingContactAddress(PXGraph graph, string contactAddressSource, int? billCustomerID, FSServiceOrder fsServiceOrderRow,
                                                      out IContact contactRow, out IAddress addressRow)
        {
            contactRow = null;
            addressRow = null;
            PXResult<Location, Contact, Address> locData = null;

            switch (contactAddressSource)
            {
                // The name of the following constant and its corresponding label
                // does not correspond with the data sought.
                case ID.Ship_To.BILLING_CUSTOMER_BILL_TO:
                    PXResult<Location, Customer, Contact, Address> defaultLocData = null;
                    defaultLocData = (PXResult<Location, Customer, Contact, Address>)PXSelectJoin<Location, InnerJoin<Customer, On<Customer.bAccountID, Equal<Location.bAccountID>,
                                                                                                                                   And<Customer.defLocationID, Equal<Location.locationID>>>,
                                                                                                                      LeftJoin<Contact,On<Contact.contactID, Equal<Location.defContactID>>,
                                                                                                                               LeftJoin<Address,On<Address.addressID, Equal<Location.defAddressID>>>>>,
                                                                                                            Where<Location.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(graph, billCustomerID);

                    contactRow = ContactAddressHelper.GetIContact(defaultLocData);
                    addressRow = ContactAddressHelper.GetIAddress(defaultLocData);

                    break;

                case ID.Ship_To.SERVICE_ORDER_ADDRESS:
                    GetSrvOrdContactAddress(graph, fsServiceOrderRow, out FSContact fsContact, out FSAddress fsAddress);
                    contactRow = fsContact;
                    addressRow = fsAddress;

                    break;

                case ID.Ship_To.SO_CUSTOMER_LOCATION:
                    locData = (PXResult<Location, Contact, Address>)PXSelectJoin<Location, LeftJoin<Contact, On<Contact.contactID, Equal<Location.defContactID>>,
                                                                                                    LeftJoin<Address, On<Address.addressID, Equal<Location.defAddressID>>>>,
                                                                                           Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(graph, fsServiceOrderRow.LocationID);
                    contactRow = ContactAddressHelper.GetIContact(locData);
                    addressRow = ContactAddressHelper.GetIAddress(locData);

                    break;

                case ID.Ship_To.SO_BILLING_CUSTOMER_LOCATION:
                    locData = (PXResult<Location, Contact, Address>)PXSelectJoin<Location, LeftJoin<Contact, On<Contact.contactID, Equal<Location.defContactID>>,
                                                                                                    LeftJoin<Address, On<Address.addressID, Equal<Location.defAddressID>>>>,
                                                                                           Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(graph, fsServiceOrderRow.BillLocationID);
                    contactRow = ContactAddressHelper.GetIContact(locData);
                    addressRow = ContactAddressHelper.GetIAddress(locData);

                    break;
            }
        }
    }
}
