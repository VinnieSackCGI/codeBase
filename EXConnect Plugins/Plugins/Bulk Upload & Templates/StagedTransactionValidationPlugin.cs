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

	// The following plugin is meant to validate whether a "Spending" transaction
	// can proceed based on the available funds. The available funds will be
	// verified from the corresponding Yearly Rollup table for the given account's
	// lowest tracking level (sub-Program, Program, and Division).

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class StagedTransactionValidationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public StagedTransactionValidationPlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin( IStagedTransactionRecord updTransaction, IStagedTransactionRecord oldTransaction )
		{

#else


	[CrmPluginRegistration(MessageNameEnum.Update,
	"eca_stagedtransaction",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "eca_amount,eca_fiscalyear,eca_appropriation,eca_fundingsource,eca_fundingtype,eca_cuffaccount,eca_description,eca_ibisrequestcode,eca_projectcode",
	"Fiscal Strip Validation On Transaction Update", 1,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "eca_amount,eca_fiscalyear,eca_appropriation,eca_fundingsource,eca_fundingtype,eca_cuffaccount,eca_description,eca_ibisrequestcode,eca_projectcode,createdby",
	Description = "Verify that updated Fiscal Strip are valid for the Staged transaction record",
	Id = "fbd279dd-c057-4ad2-8351-19b425f659a0")]

	public class StagedTransactionValidationPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext, true);

			tracer.Trace("Beginning execution of Staged Transaction Validation plugin");

			// Get the Transaction record object from the target entity
			var updTransaction = dbService.StagedTransaction.GetRecordFromEntity(context.GetTargetEntity());

			IStagedTransactionRecord oldTransaction = null;

			var messageName = context.MessageName;
			if (messageName == MessageNameEnum.Update.ToString())
			{
				oldTransaction = dbService.StagedTransaction.GetRecordFromId(updTransaction.Id);

				// Verify that current user is not System when validating on Update
				var fullname = dbService.SystemUser.GetCodeFromId(dbService.UserId);
				if (fullname == "SYSTEM")
				{
					// Use the record creator as the user
					var userId = oldTransaction.CreatedBy.Id;
					ResetUserContext(userId);
				}
			}
#endif

			// Get the combined transaction
			var combinedTransaction = updTransaction.GetCombinedTransactionRecord(oldTransaction);

			// Get a new transaction from the Staged one
			var newTransaction = dbService.Transaction.ComposeTransactionRecordFromStaged(combinedTransaction);

			// Validate the Fiscal Strip info

			var errors = Common.TransactionValidator.KeyValidation(newTransaction, dbService, tracer);

			// If errors, post the error in the record and return
			if (!string.IsNullOrEmpty(errors)) 
			{
				updTransaction.ValidationErrors = errors;
				return;
			}

			// Otherwise, clear the Validation errors and Inactive the Staged record
			updTransaction.ValidationErrors = string.Empty;
			updTransaction.IsValid = true;

			updTransaction.State = "Inactive";
			updTransaction.Status = "Inactive";

		}
	}
}