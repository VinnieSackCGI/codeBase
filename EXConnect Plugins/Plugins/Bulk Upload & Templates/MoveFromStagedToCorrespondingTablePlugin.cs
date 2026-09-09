using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using EXConnect_Plugins.Entities.Adapters;
using System.Linq;

namespace EXConnect_Plugins
{
	using Common;
	using EXConnect_Plugins.Entities.Adapters.Interfaces;
	using Plugins_CommonLibrary.Extensions;

	// The following plugin moves any deactivated aSTaged Record that has been set as valid
	// to the EXConect Transaction table. Finally, the record is deleted.

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class MoveFromStagedToCorrespondingTablePlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public MoveFromStagedToCorrespondingTablePlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(IStagedBulkUploadRecord updStagedRecord )
		{

#else

	[CrmPluginRegistration(MessageNameEnum.Update,
	"eca_stagedbulkuploadrecords",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "eca_isvalid",
	"Move Staged Records when Validated", 2,
	IsolationModeEnum.Sandbox,
	Description = "Move validated Staged record into the Corresponding table",
	Id = "a5b9745e-45b6-45c7-a841-845aeffa2213")]

	public class MoveFromStagedToCorrespondingTablePlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext, true);

			tracer.Trace("Beginning execution of Staged Bulk Upload move plugin");

			// Get the Staged record object from the target entity
			var updStagedRecord = dbService.StagedBulkUploadRecord.GetRecordFromEntity(context.GetTargetEntity());
#endif

			if (!updStagedRecord.IsValid)
			{
				tracer.Trace("The Staged record is not valid so it will not be moved.");
				return;
			}

			// Retrieve the record from the table 
			var stagedRecord = dbService.StagedBulkUploadRecord.GetRecordFromId(updStagedRecord.Id);
			if ( stagedRecord == null)
			{
				tracer.Trace($"An unexpected error encountered. The record with Id \"{stagedRecord.Id}\" cannot be found.");
				return;
			}

#if !DESKTOP
			// Verify that current user is set as the creator of the new transaction
			if (dbService.UserId != stagedRecord.CreatedBy.Id)
			{
				// Use the record creator as the user
				var userId = stagedRecord.CreatedBy.Id;
				ResetUserContext(userId);
			}

#endif

			// Geneate the propper records based on the template type

			Entity entity = null;

			if (stagedRecord.IsTransaction)
			{
				// Get a new transaction from the Staged one
				var newTransaction = dbService.Transaction.ComposeTransactionRecordFromStaged(stagedRecord);

				entity = newTransaction.Entity;
			}
			else if (stagedRecord.IsSpendPlanRequest) 
			{
				var newRequest = dbService.SpendPlanRequest.ComposeSpendPlanRequestRecordFromStaged(stagedRecord);
				entity = newRequest.Entity;
			}

			// Store the transaction into the EXConnect Transaction table
			tracer.Trace("Moving the validated record from Staged to EXConnect.");

			dbService.Create(entity, true);

			// Although the inactive record could be deleted at this point, the user is currently
			// editing the record and deleting it now will result in an error in the interface.
			// As a result, the record cleanup will be left to the Power Automate flow the next time it runs.			
		}
	}
}