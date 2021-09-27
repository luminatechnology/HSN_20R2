using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.FS;
using HSNCustomizations.Descriptor;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("Stage Control")]
    public class LumStageControl : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<LUMAutoWorkflowStage>.By<srvOrdType, currentStage, toStage>
        {
            public static LUMAutoWorkflowStage Find(PXGraph graph, string srvOrdType, int currentStage, int toStage) => FindBy(graph, srvOrdType, currentStage, toStage);
        }
        #endregion

        #region SrvOrdType
        [PXDBString(4, IsKey = true, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Srv Ord Type", Visible = false)]
        [FSSelectorSrvOrdTypeNOTQuote]
        [PXDBDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion

        #region CurrentStage
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "From Stage")]
        [FSSelectorWorkflowStage(typeof(LumStageControl.srvOrdType))]
        public virtual int? CurrentStage { get; set; }
        public abstract class currentStage : PX.Data.BQL.BqlInt.Field<currentStage> { }
        #endregion

        #region ToStage
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "To Stage")]
        [FSSelectorWorkflowStage(typeof(LumStageControl.srvOrdType))]
        [PXDefault()]
        public virtual int? ToStage { get; set; }
        public abstract class toStage : PX.Data.BQL.BqlInt.Field<toStage> { }
        #endregion

        #region AdminOnly
        [PXDBBool]
        [PXUIField(DisplayName = "Admin Only")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? AdminOnly { get; set; }
        public abstract class adminOnly : PX.Data.BQL.BqlBool.Field<adminOnly> { }
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