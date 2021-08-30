using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("Opportunity Terms & Conditions")]
    public class LUMOpprTermCond : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<LUMOpprTermCond>.By<identityID>
        {
            public static LUMOpprTermCond Find(PXGraph graph, int? identityID) => FindBy(graph, identityID);
        }
        #endregion

        #region IdentityID
        [PXDBIdentity(IsKey = true)]
        public virtual int? IdentityID { get; set; }
        public abstract class identityID : PX.Data.BQL.BqlInt.Field<identityID> { }
        #endregion

        #region OpportunityID
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Opportunity ID")]
        [PXSelector(typeof(Search2<CROpportunity.opportunityID, LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
                                                                         LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>>>,
                                                                Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>,
                                                                OrderBy<Desc<CROpportunity.opportunityID>>>),
                    new[] { typeof(CROpportunity.opportunityID),
                            typeof(CROpportunity.subject),
                            typeof(CROpportunity.status),
                            typeof(CROpportunity.curyAmount),
                            typeof(CROpportunity.curyID),
                            typeof(CROpportunity.closeDate),
                            typeof(CROpportunity.stageID),
                            typeof(CROpportunity.classID),
                            typeof(CROpportunity.isActive),
                            typeof(BAccount.acctName),
                            typeof(Contact.displayName) },
                    Filterable = true, ValidateValue = false)]
        [PXDBDefault(typeof(CROpportunity.opportunityID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string OpportunityID { get; set; }
        public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
        #endregion

        #region QuoteID
        [PXDBGuid()]
        [PXUIField(DisplayName = "Quote ID")]
        [PXDBDefault(typeof(CRQuote.quoteID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Guid? QuoteID { get; set; }
        public abstract class quoteID : PX.Data.BQL.BqlGuid.Field<quoteID> { }
        #endregion

        #region SortOrder
        [PXDBInt()]
        [PXUIField(DisplayName = "Sort Order")]
        public virtual int? SortOrder { get; set; }
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
        #endregion

        #region IsActive
        [PXDBBool()]
        [PXUIField(DisplayName = "Active")]
        [PXDefault(true)]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion

        #region Title
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Title")]
        public virtual string Title { get; set; }
        public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
        #endregion

        #region Definition
        [PXDBString(IsUnicode = true)]
        [PXUIField(DisplayName = "Terms & Conditions")]
        public virtual string Definition { get; set; }
        public abstract class definition : PX.Data.BQL.BqlString.Field<definition> { }
        #endregion
    
        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion
    
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion
    
        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
    
        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion
    
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion
    
        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
    
        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    
        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}