using System;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins
{
	using System.Runtime.Remoting.Contexts;

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class TransactionStageSyncPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public TransactionStageSyncPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(ITransactionWorkflowRecord workflow)
		{
#else

	[CrmPluginRegistration(MessageNameEnum.Create,
	"eca_transactionworkflow",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Transaction Workflow Record Initialization", 1,
	IsolationModeEnum.Sandbox,
//	Image1Type = ImageTypeEnum.PreImage,
//	Image1Name = ImageTypeName.PreImage,
//	Image1Attributes = "bpf_rcade_carttransactionid,completedon,activestageid",
	Description = "Synchronizes the Transaction Status Reason and the Transaction Workflow Stage once the Workflow record is created",
	Id = "3ba734c2-df40-4b84-99f8-b3dec0501b3c")]

	public class TransactionStageSyncPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Transaction Stage Sync plugin");

			// Process only On Create
			if (this.context.MessageName != MessageNameEnum.Create.ToString())
			{
				tracer.Trace($"Plugin called on \"{this.context.MessageName}\". Call will be ignored.");
				return;
			}

			// Retrieve the incoming workflow record
			var workflow = dbService.TransactionWorkflow.GetRecordFromEntity(context.GetTargetEntity()) as ITransactionWorkflowRecord;

#endif
			// Get the associated Transaction record object from the workflow record
			var transaction = dbService.Transaction.GetRecordFromId(workflow.TransactionRef.Id);

			if (transaction == null) 
			{
				tracer.Trace($"Cannot retrieve a value for the associated Transaction. Workflow id = {workflow.Id} and transaction Id = {(workflow.TransactionRef?.Id)}");
				return;
			}

			// If we get here, sync the active Stage with the transaction Initial Status
			var transactionWorkflow = new Common.BusinessProcessFlow(dbService, tracer);

			transactionWorkflow.SetTransactionStage(transaction, null, workflow);
		}

	}

}
