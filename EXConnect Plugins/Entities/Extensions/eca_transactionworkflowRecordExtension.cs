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
    /// A table adapter class for a new_uloprocessworkflow record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class eca_transactionworkflow : ITransactionWorkflowRecord
    {

        private ITracingService tracer;
        private IRepository dbService;

		private Dictionary<string, string> fieldLogicalNames =
	            new Dictionary<string, string>
	            {
                    {"TableLogicalName", eca_transactionworkflow.EntityLogicalName },
				    {"ActiveStageRef","activestageid"},
				    {"ProcessRef","processid" },
				    {"TransactionRef", "bpf_rcade_transactionid" },
				    {"TraversedPath","traversedpath" },
                    {"StatusCode", "statuscode" },
                    {"StateCode", "statecode" },
	            };

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

        #region Interrogation fields

        public bool IsActive
        {
            get
            {
                return StateCode == eca_transactionworkflowState.Active;
            }
        }

        public bool IsFinished
        {
            get
            {
                return StatusCode == eca_transactionworkflow_StatusCode.Finished;
            }
        }

        #endregion

        #region Data Properties

        public Dictionary<string, string> FieldLogicalNames => fieldLogicalNames;

        public EntityReference ActiveStageRef
        {
            get
            {
                return ActiveStageId;
            }
            set
            {
                ActiveStageId = value;
            }
        }

        public EntityReference ProcessRef
        {
            get
            {
                return ProcessId;
            }
            set
            {
                ProcessId = value;
            }
        }

        public EntityReference TransactionRef
        {
            get
            {
                return bpf_rcade_carttransactionid;
            }
            set
            {
				bpf_rcade_carttransactionid = value;
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

        #region Methods

        public void Reactivate(bool withUpdate = true )
        {
            StatusCode = eca_transactionworkflow_StatusCode.Active;
            StateCode = eca_transactionworkflowState.Active;
            if (withUpdate)
            {
				dbService.Update(this);
			}
		}

        #endregion

    }
}

