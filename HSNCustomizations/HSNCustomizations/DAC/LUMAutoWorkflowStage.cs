using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.FS;
using HSNCustomizations.Descriptor;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("Auto Workflow Stage")]
    public class LUMAutoWorkflowStage : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<LUMAutoWorkflowStage>.By<srvOrdType, wFRule>
        {
            public static LUMAutoWorkflowStage Find(PXGraph graph, string srvOrdType, string wFRule) => FindBy(graph, srvOrdType, wFRule);
        }
        public class UK : PrimaryKeyOf<LUMAutoWorkflowStage>.By<srvOrdType, wFRule, currentStage>
        {
            public static LUMAutoWorkflowStage Find(PXGraph graph, string srvOrdType, string wFRule, int? currentStage) => FindBy(graph, srvOrdType, wFRule, currentStage);
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

        #region WFRule
        [PXDBString(12, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Rule")]
        [WorkflowRuleSelector()]
        [PXDefault()]
        public virtual string WFRule { get; set; }
        public abstract class wFRule : PX.Data.BQL.BqlString.Field<wFRule> { }
        #endregion

        #region Active
        [PXDBBool()]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        #endregion

        #region CurrentStage
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Current Stage")]
        [FSSelectorWorkflowStage(typeof(LUMAutoWorkflowStage.srvOrdType))]
        public virtual int? CurrentStage { get; set; }
        public abstract class currentStage : PX.Data.BQL.BqlInt.Field<currentStage> { }
        #endregion

        #region NextStage
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Next Stage")]
        [FSSelectorWorkflowStage(typeof(LUMAutoWorkflowStage.srvOrdType))]
        [PXDefault()]
        public virtual int? NextStage { get; set; }
        public abstract class nextStage : PX.Data.BQL.BqlInt.Field<nextStage> { }
        #endregion
  
        #region Descr
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        #endregion
  
        #region Remark
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Remark")]
        public virtual string Remark { get; set; }
        public abstract class remark : PX.Data.BQL.BqlString.Field<remark> { }
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