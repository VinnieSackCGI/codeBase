using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;

namespace EXConnect_Plugins
{
#if DESKTOP

	using EXConnect_Plugins.Entities.Interfaces;

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class TransactionWorkflowSyncPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public TransactionWorkflowSyncPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction)
		{
#else

// ***************************************************************
// The following plugin has been undefined since it seems to be irrelevant for now
// ***************************************************************
/*
	[CrmPluginRegistration(MessageNameEnum.Update,
	"rcade_carttransaction",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "statuscode",
	"Transaction Approval Status Workflow Stage Syncronization", 4,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "statuscode",
	Description = "Synchronize the Transaction Workflow Active Stage with the current Approval Status of the Transaction",
	Id = "8fd4fb69-e4b2-44d7-ac92-69a7a06ff8be")]
*/
	// This plugin will set the Transaction Workflow Active Stage based on the value of the Status Reason field in the
	// Transaction record. The logic is as follows.
	// 1. If the new Status is Pending, set the Workflow Stage to "Pending Approval"
	// 2. If the new Status is Approved, set the Workflow Stage to "Approved"
	// 3. If the new Status is Draft, and it came from Pending, set the Workflow Stage to "Needs Revision"

	public class TransactionWorkflowSyncPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Transaction Workflow Stage Sync plugin");

			// Process only On Update
			if (this.context.MessageName != MessageNameEnum.Update.ToString())
			{
				tracer.Trace($"Plugin called on \"{this.context.MessageName}\". Call will be ignored.");
				return;
			}

			// Get the incoming Process Workflow record object from the target entity
			// and another from the preImage if Updating
			var newTransaction = dbService.Transaction.GetRecordFromEntity(context.GetTargetEntity());
			var oldTransaction = dbService.Transaction.GetRecordFromEntity(context.GetPreImage());

#endif

			if (string.IsNullOrEmpty(newTransaction.Status))
			{
				tracer.Trace("The Transaction Status is null. Setting it to Draft");
				newTransaction.Status = "Draft";
			}

			// Set the Workflow stage based on the Transaction Status being modified

			// Get the current stage of the record
			var workflowRecord = dbService.TransactionWorkflow.GetRecordForTransactionId(newTransaction.Id);

			// and verify we have a workflow record
			if (workflowRecord == null)
			{
				tracer.Trace("The process has not been initiated.");
				return;
			}

			var transactionWorkflow = new Common.BusinessProcessFlow(dbService, tracer);

			transactionWorkflow.SetTransactionStage(newTransaction, oldTransaction, workflowRecord);

		}
	}
}
