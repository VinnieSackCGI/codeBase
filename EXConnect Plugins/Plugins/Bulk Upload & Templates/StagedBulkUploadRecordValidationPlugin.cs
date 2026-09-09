using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using EXConnect_Plugins.Plugin_Handling;

using EXConnect_Plugins.Common;

namespace EXConnect_Plugins.Plugins
{
	// The following plugin is meant to validate any record in the Staged Bulk Upload Records table.
	// Once validated, the record can be moved to the corresponding table based on the Template Type.

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class StagedBulkUploadRecordValidationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public StagedBulkUploadRecordValidationPlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin( IStagedBulkUploadRecord updStagedRecord, IStagedBulkUploadRecord oldStagedRecord )
		{

#else


	[CrmPluginRegistration(MessageNameEnum.Update,
	"eca_stagedbulkuploadrecords",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "eca_amount,eca_fiscalyear,eca_appropriation,eca_fundingsource,eca_fundingtype,eca_cuffaccount,"
									+ "eca_descriptionjustification,eca_ibisrequestcode,eca_requestname,eca_projectcode,eca_priority",
	"Fiscal Strip Validation On Bulk Upload Record Update", 1,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "eca_amount,eca_fiscalyear,eca_appropriation,eca_fundingsource,eca_fundingtype,eca_cuffaccount,"
						+ "eca_descriptionjustification,eca_ibisrequestcode,eca_requestname,eca_projectcode,eca_priority,createdby",
	Description = "Verify that updated Fiscal Strip are valid for the Staged Bulk Upload record",
	Id = "fbd279dd-c057-4ad2-8351-19b425f659a0")]

	public class StagedBulkUploadRecordValidationPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext, true);

			tracer.Trace("Beginning execution of Staged Bulk Upload Record Validation plugin");

			// Get the Transaction record object from the target entity
			var updStagedRecord = dbService.StagedBulkUploadRecord.GetRecordFromEntity(context.GetTargetEntity());

			IStagedBulkUploadRecord oldStagedRecord = null;

			var messageName = context.MessageName;
			if (messageName == MessageNameEnum.Update.ToString())
			{
				oldStagedRecord = dbService.StagedBulkUploadRecord.GetRecordFromId(updStagedRecord.Id);

				// Verify that current user is not System when validating on Update
				var fullname = dbService.SystemUser.GetCodeFromId(dbService.UserId);
				if (fullname == "SYSTEM")
				{
					// Use the record creator as the user
					var userId = oldStagedRecord.CreatedBy.Id;
					ResetUserContext(userId);
				}
			}
#endif

			// Get the combined staged record
			var combinedRecord = updStagedRecord.GetCombinedRecord(oldStagedRecord);

			if (combinedRecord.IsTransaction)
			{
				// Get a new transaction from the Staged one
				var newTransaction = dbService.Transaction.ComposeTransactionRecordFromStaged(combinedRecord);

				// Validate the Fiscal Strip info

				var errors = TransactionValidator.TransactionKeyValidation(newTransaction, dbService, tracer);

				// If errors, post the error in the record and return
				if (!string.IsNullOrEmpty(errors))
				{
					updStagedRecord.ValidationErrors = errors;
					return;
				}
			}
			else if (combinedRecord.IsSpendPlanRequest)
			{
				// Get a new record from the Staged record
				var newSpendPlan = dbService.SpendPlanRequest.ComposeSpendPlanRequestRecordFromStaged(combinedRecord);

				// Validate the Fiscal Strip info

				var errors = SpendPlanValidator.KeyValidation(newSpendPlan, dbService, tracer);

				// If errors, post the error in the record and return
				if (!string.IsNullOrEmpty(errors))
				{
					updStagedRecord.ValidationErrors = errors;
					return;
				}
			}

			// Otherwise, clear the Validation errors and Inactive the Staged record
			updStagedRecord.ValidationErrors = string.Empty;
			updStagedRecord.IsValid = true;

			updStagedRecord.State = "Inactive";
			updStagedRecord.Status = "Inactive";

		}

	}
}
