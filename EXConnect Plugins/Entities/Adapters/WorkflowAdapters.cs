using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
    using Interfaces;

	public class WorkflowAdapter : IWorkflowAdapter
	{
		protected ITracingService tracer = null;
		protected EXConnect_ServiceContext serviceContext;
		protected IRepository dbService;

		public WorkflowAdapter( IRepository dbService, ITracingService tracer )
		{
			if (dbService == null)
			{
				throw new ArgumentNullException("Business DB Service");
			}
			if (tracer == null)
			{
				throw new ArgumentNullException("Tracing Service");
			}
			this.dbService = dbService;
			serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
			this.tracer = tracer;
		}

		public virtual IWorkflowRecord GetRecordForEntityId( Guid id ) => throw new NotImplementedException();

		public virtual IWorkflowRecord GetRecordFromEntity( Entity workflowEntity ) => throw new NotImplementedException();

	}

	public class TransactionWorkflowAdapter : WorkflowAdapter
	{

		public TransactionWorkflowAdapter( IRepository dbService, ITracingService tracer ) : base( dbService, tracer ) { }

		public override IWorkflowRecord GetRecordForEntityId( Guid id )
		{
			return serviceContext.eca_transactionworkflowSet.Where(w => w.bpf_rcade_carttransactionid.Id == id)
															.FirstOrDefault();
		}

		public override IWorkflowRecord GetRecordFromEntity( Entity workflowEntity )
		{
			IWorkflowRecord workflow = workflowEntity.ToEntity<eca_transactionworkflow>();
			workflow.Initialize(dbService, tracer);
			return workflow;
		}

	}

	public class DocumentWorkflowAdapter : WorkflowAdapter
	{

		public DocumentWorkflowAdapter( IRepository dbService, ITracingService tracer ) : base(dbService, tracer) { }

		public override IWorkflowRecord GetRecordForEntityId( Guid id )
		{
			return serviceContext.eca_documentworkflowSet.Where(w => w.bpf_eca_gfmsdocumentid.Id == id)
														   .FirstOrDefault();
		}

		public override IWorkflowRecord GetRecordFromEntity( Entity workflowEntity )
		{
			IWorkflowRecord workflow = workflowEntity.ToEntity<eca_documentworkflow>();
			workflow.Initialize(dbService, tracer);
			return workflow;
		}
	}

}
