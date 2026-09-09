using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a Process Stage record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class ProcessStage : IProcessStageRecord
    {
        private ITracingService tracer;
        private IRepository dbService;

        public void Initialize(IRepository dbService, ITracingService tracer)
        {
            if (dbService == null)
            {
                throw new NullReferenceException("Business DB Service");
            }
            if (tracer == null)
            {
                throw new NullReferenceException("Tracing Service");
            }

            this.dbService = dbService;
            this.tracer = tracer;
        }

		#region Interrogation Properties

		public bool IsDraft => StageName == "Draft";
		public bool IsPendingApproval => StageName == "Pending Approval";
		public bool IsApproved => StageName == "Approved";
		public bool IsNeedsRevision => StageName == "Needs Revision";

		#endregion

		#region Data Properties

		public string PrimaryEntityType
        {
            get
            {
                return PrimaryEntityTypeCode;
            }
            set
            {
                PrimaryEntityTypeCode = (string.IsNullOrEmpty(value) ? null : value);
            }
        }

        public Guid? StageId
        {
            get
            {
                return ProcessStageId;
            }
            set
            {
                ProcessStageId = value;
            }
        }

        public Entity Entity
        {
            get
            {
                return this;
            }
        }

        #endregion

        // The following are required to satisfy the ITableRecord interface

        public DateTime? CreatedOn
        {
            get => null;
            set { }
        }
        public EntityReference CreatedBy
        {
            get => null;
            set { }
        }
        public DateTime? ModifiedOn
        {
            get => null;
            set { }
        }
        public EntityReference ModifiedBy
        {
            get => null;
            set { }
        }
    }
}

