using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using System.Xml.Linq;
using EXConnect_Plugins.Common;
using System.Threading;

namespace EXConnect_Plugins
{

#if DESKTOP
	using Common;
	using Entities.Interfaces;

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class GFMSdocumentPackageStatusSyncPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public GFMSdocumentPackageStatusSyncPlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin( IGFMSdocumentRecord newDocument, IGFMSdocumentRecord oldDocument )
		{
#else

	[CrmPluginRegistration(MessageNameEnum.Update,
	"eca_gfmsdocument",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "eca_needsvalidation",
	"GFMS Document Status And Child Transaction Status Syncronization", 4,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "statuscode,eca_needsvalidation",
	Description = "Synchronize the GFMS Document Active Stage with the current Approval Status of the Child Transactions",
	Id = "09413167-b775-4f5f-9b0f-d708bfbda654")]
	
	// This plugin will verify that all child transactions will either be ALL approved or ALL pending.
	// If any of the child transactions is in Pending Status, then All should be in Pending Status."

	public class GFMSdocumentPackageStatusSyncPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of GFMS Document Workflow Stage Sync plugin");

			// Process only On Update
			if (this.context.MessageName != MessageNameEnum.Update.ToString())
			{
				tracer.Trace($"Plugin called on \"{this.context.MessageName}\". Call will be ignored.");
				return;
			}

			// Get the incoming GFMS Documewnt record object from the target entity
			// and another from the preImage if Updating
			var newDocument = dbService.GFMSdocument.GetRecordFromEntity(context.GetTargetEntity());
			var oldDocument = dbService.GFMSdocument.GetRecordFromEntity(context.GetPreImage());

#endif
			// Ignore the call if the Needs Validation flag has not changed or the BPF does not exist yet
			
			var documentWorkflow = dbService.DocumentWorkflow.GetRecordForEntityId(newDocument.Id);

            // Prevent the call for Needs Validation flag because BPF is not detached
            /*
			if (newDocument.NeedsValidation == oldDocument.NeedsValidation || documentWorkflow == null)
			{
				tracer.Trace("The Needs Validation flag has not been updated or the documentWorkflow does not exist. Process will be ignored.");
				return;
			}
            */

            // Get the list of Child Transactions for this document and determine the combined status
            // The document status will be set to the lowest status found for the children.

            Thread.Sleep(2000); // Add a 2 second delay to wait for the Children Statuses to refresh
      

            var childTransactions = dbService.GFMSdocument.GetChildTransactionsList(newDocument.Id);

			var approvedCount = 0;
			var pendingCount = 0;
			var draftCount = 0;
			foreach (var childTransaction in childTransactions)
			{ 
				if (childTransaction.IsDraft)
				{
					draftCount++;
				}
				else if (childTransaction.IsPending) 
				{
					pendingCount++;
				}
				else if (childTransaction.IsApproved)
				{
					 approvedCount++;
				}
			}

            //var documentStatus = "Pending";
            var documentStatus = "Approved";

			if ( draftCount > 0)
			{
				documentStatus = "Draft";
			}
			else if (draftCount + pendingCount <= 0)
			{
				documentStatus = "Approved";
			}

			tracer.Trace($"Found a total of {childTransactions.Count} child Transactions, {approvedCount} Approved and "
				+ $"{draftCount} in Draft and {pendingCount} in Pending. The Document will be set to {documentStatus}.");

			// Change the Document Status and BPF Stage to match
			var processFlow = new BusinessProcessFlow(dbService, tracer);

			processFlow.SetGFMSdocumentStage(newDocument, documentStatus, documentWorkflow, true);

		}
	}

}
