using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;


namespace EXConnect_Plugins.Entities
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System;
	using Interfaces;

	public partial class eca_documentworkflow : IDocumentWorkflowRecord
	{
		private ITracingService tracer;
		private IRepository dbService;

		private Dictionary<string, string> fieldLogicalNames =
				new Dictionary<string, string>
				{
					{"TableLogicalName", eca_documentworkflow.EntityLogicalName },
					{"ActiveStageRef","activestageid"},
					{"ProcessRef","processid" },
					{"TransactionRef", "bpf_eca_gfmsdocumentid" },
					{"TraversedPath","traversedpath" },
					{"StatusCode", "statuscode" },
					{"StateCode", "statecode" },
				};

		public void Initialize( IRepository dbService, ITracingService tracer )
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
				return StateCode == eca_documentworkflowState.Active;
			}
		}

		public bool IsFinished
		{
			get
			{
				return StatusCode == eca_documentworkflow_StatusCode.Finished;
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

		public EntityReference DocumentRef
		{
			get
			{
				return bpf_eca_gfmsdocumentid;
			}
			set
			{
				bpf_eca_gfmsdocumentid = value;
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

		public void Reactivate( bool withUpdate = true )
		{
			StatusCode = eca_documentworkflow_StatusCode.Active;
			StateCode = eca_documentworkflowState.Active;
			if (withUpdate)
			{
				dbService.Update(this);
			}
		}

		#endregion

	}
}
