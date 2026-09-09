using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Services;

namespace EXConnect_Plugins.Common
{
	using System.Workflow.Runtime.Tracking;
	using Entities;
	using Entities.Interfaces;
	using Microsoft.Xrm.Sdk.Workflow.Activities;
	using Services;

	public class BusinessProcessFlow
	{
		private IRepository dbService;
		private ITracingService tracer;

		public BusinessProcessFlow(IRepository dbService, ITracingService tracer)
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
			this.tracer = tracer;
		}

		public void SetTransactionStage(ICARTtransactionRecord transaction, ICARTtransactionRecord prevTransaction, IWorkflowRecord workflowRecord,
										bool saveUpdate = true)
		{
			// Determine the current process stage and process accordingly
			var currentTransactionStatus = transaction.Status;
			var newTransactionStatus = transaction.Status;

			tracer.Trace($"Incoming Transaction Status Reason = {transaction.Status}.");

			// Ignore if already synchronize
			if (prevTransaction == null)
			{
				// If prevTransaction is null, then this is a New transactions
				if (string.IsNullOrEmpty(transaction.InitialStatus))
				{
					// If the InitialStatus is blank when the workflow is created,
					// then set it to Pending.
					newTransactionStatus = transaction.InitialStatus = "Pending";
					tracer.Trace($"Detected Initial Status set to {transaction.InitialStatus}.");
				}
				else
				{
					newTransactionStatus = transaction.InitialStatus;
					tracer.Trace($"Detected Initial Status set to {transaction.InitialStatus}.");
				}
			}
			else if (currentTransactionStatus == null)
			{
				newTransactionStatus = currentTransactionStatus = prevTransaction.Status;
				tracer.Trace($"Setting new Transaction Status to previous Status set to {prevTransaction.Status}.");
			}

			SyncRecordAndProcessBPFStage("Transaction", transaction.Id, currentTransactionStatus, newTransactionStatus, workflowRecord);

			// Finally set the Transaction Status, if different
			if (saveUpdate && (newTransactionStatus != currentTransactionStatus || transaction.InitialStatus != null))
			{
				transaction.Status = newTransactionStatus;
				transaction.InitialStatus = (transaction.InitialStatus == "Pending" ? "Reset" : null); // force an update
				dbService.Update(transaction.Entity);
			}
		}

		public void SetGFMSdocumentStage( IGFMSdocumentRecord document, string newDocumentStatus, IWorkflowRecord workflowRecord,
								bool saveUpdate = true )
		{
			// Determine the current process stage and process accordingly
			var currentDocumentStatus = document.Status;

			tracer.Trace($"Incoming Document Status Reason = {document.Status}.");
             
			// Ignore if already synchronize
			if (currentDocumentStatus == null)
			{
				newDocumentStatus = currentDocumentStatus = newDocumentStatus;
                tracer.Trace($"ignore if already synchronize block: {newDocumentStatus }");
			}

			SyncRecordAndProcessBPFStage("GFMS Document", document.Id, currentDocumentStatus, newDocumentStatus, workflowRecord);

			// Finally set the Document Status, if different
			if (newDocumentStatus != currentDocumentStatus)
			{
				document.Status = newDocumentStatus;
				if (saveUpdate)
				{
					dbService.Update(document.Entity);
				}
			}
		}

		#region Auxialiry Methods

		private void SyncRecordAndProcessBPFStage( string recordTypeName, Guid recordId, string currentRecordStatus,
													string newRecordStatus, IWorkflowRecord workflowRecord )
		{
			var currentRecordStage = ProcessStages.WorkflowStageFromRecordStatus(dbService, currentRecordStatus);
			var newWorkflowStage = ProcessStages.WorkflowStageFromRecordStatus(dbService, newRecordStatus);
			var curentWorkflowStage = ProcessStages.WorkflowStageToRecordStage(dbService, workflowRecord.ActiveStageRef.Name);

			// Ignore if both the current Workflow and Entity stage are consistent
			if (currentRecordStage == newWorkflowStage && curentWorkflowStage == newWorkflowStage)
			{
				tracer.Trace($"The Workflow and the {recordTypeName} stages are set to \"{newWorkflowStage}\" and \"{currentRecordStatus}\", respectively. Plugin will terminate.");
			}
			else
			{
				// Set the Workflow stage
				tracer.Trace($"\nProcessing {recordTypeName} record \"{recordId}\" with Status = \"{newRecordStatus}\" and Workflow Stage = \"{newWorkflowStage}\".");
				// Send a reset request if the stage moving to is Draft.
				ProcessStages.SetWorkflowStage(dbService, tracer, workflowRecord, newWorkflowStage, true);
			}
		}

		#endregion

	}
}
