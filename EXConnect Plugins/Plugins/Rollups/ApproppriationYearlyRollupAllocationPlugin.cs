using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using System.Workflow.Activities;

namespace EXConnect_Plugins
{

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktip Interface project
	public class ApproppriationYearlyRollupAllocationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public ApproppriationYearlyRollupAllocationPlugin( IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

#else
/*
	[CrmPluginRegistration(MessageNameEnum.Create,
	"rcade_carttransaction",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Appropriations Yearly Rollup On Transaction Create", 1,
	IsolationModeEnum.Sandbox,
	Description = "Update the Allocated values of the Appropriation Yearly Rollups when an Appropriation Allocation type Transaction is created.",
	Id = "4bed88d2-572a-4e83-af93-796da3375afe")]
*/
	[CrmPluginRegistration(MessageNameEnum.Update,
	"rcade_carttransaction",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_ecatransactiontype,rcade_cuffaccount,eca_fundingtype,statuscode",
	"Appropriations Yearly Rollup On Transaction Update", 2,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_ecatransactiontype,rcade_cuffaccount,eca_fundingtype,statuscode",
	Description = "Update the Allocated values of the Appropriation Yearly Rollups when a Appropriation Allocation type Transaction is updated.",
	Id = "a6049a4a-79ca-4a4c-bba3-98ccc9f3438f")]

	public class ApproppriationYearlyRollupAllocationPlugin : MainPlugin
	{

		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

            tracer.Trace("Beginning execution of Appropriations Yearly Rollup plugin");

			// Get the Transaction record object from the target entity
			var newTransaction = dbService.Transaction.GetRecordFromEntity( context.GetTargetEntity() );
			ICARTtransactionRecord oldTransaction = null;

			// Make sure this is processed after the Transaction Workflow has been created by the system
			if (dbService.TransactionWorkflow.GetRecordForEntityId(newTransaction.Id) == null)
			{
				tracer.Trace("The Transaction Workflow has not been created yet, the plugin will terminate.");
				return;
			}

			if (context.MessageName == MessageNameEnum.Update.ToString())
			{
				oldTransaction = dbService.Transaction.GetRecordFromEntity(context.GetPreImage());
				ProcessUpdatedTransaction( newTransaction, oldTransaction );
			}
/*
			else
			{
				ProcessNewTransaction( newTransaction );
			}
*/
        }

#endif

		#region Auxiliary Methods

		/// <summary>
		/// Process a new transaction into the Appropriation Yearly Rollup table
		/// </summary>
		/// <returns></returns>
		public void ProcessNewTransaction(ICARTtransactionRecord transaction)
		{
			// Retrieve the corresponding Yearly Rollup for the given transaction 
			
			// Ignore if the new transaction is not an Appropriation Allocation type
			if (!transaction.IsAppropriationAllocation)
			{
				tracer.Trace($"The transaction is set as \"{transaction.Type}\". Only Appropriation Allocation is considered by this plugin.");
				return;
			}
			// Ignore if the Initial Status is Draft
			if (transaction.InitialStatus == "Draft")
			{
				tracer.Trace("The transaction is set as Draft. The plugin will terminate.");
				return;
			}

			var rollup = dbService.AppropriationRollup.GetRecordFromKeys( transaction.FiscalYear.GetValueOrDefault(), transaction.Appropriation );

			if (rollup == null)
			{
				tracer.Trace($"Could not find an Appropriation Yearly Rollup record for appropriation \"{transaction.Appropriation.Id}\"" +
							$" and Fiscal Year \"{transaction.FiscalYear.GetValueOrDefault()}\". \nProcess terminated.");
				return;
			}

			// Apply transaction amount to Total Allocated
			if (ApplyAmountToTotalAllocated(rollup, transaction))
			{
				// Save the changes
				dbService.Update(rollup.Entity);
				tracer.Trace("Saved the changes");
			}

        }

		public void ProcessUpdatedTransaction( ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction )
		{
            // Determine what changed and process accordingly
            tracer.Trace("Begin ProcessUpdatedTransaction");
			// Ignore if the new transaction is not an Appropriation Allocation type
			if (!oldTransaction.IsAppropriationAllocation )
			{
				tracer.Trace($"The transaction is set as \"{oldTransaction.Type}\". Only Appropriation Allocation is considered by this plugin.");
				return;
			}

			var oldAllocationRemoved = false;

            tracer.Trace("Begin statusChanged check");
			// Determine the changes to process
			var statusChanged = (newTransaction.Status != null && newTransaction.Status != oldTransaction.Status);
			if (statusChanged)
			{
				// Verify that the Status changed due to the Workflow
				var workflow = dbService.TransactionWorkflow.GetRecordForEntityId(newTransaction.Id);
				if (workflow == null)
				{
					statusChanged = false;
				}
			}

            tracer.Trace("Begin dataChanged creation");
			var dataChanged = (newTransaction.Appropriation != null && newTransaction.Appropriation.Id != oldTransaction.Appropriation.Id);
			dataChanged |= (newTransaction.FundingType?.Id != oldTransaction.FundingType?.Id);
			dataChanged |= (newTransaction.FiscalYear != null && newTransaction.FiscalYear != oldTransaction.FiscalYear);
			var amountChanged = (newTransaction.Amount != null && newTransaction.Amount != oldTransaction.Amount);

			if (dataChanged || amountChanged || statusChanged)
			{
                tracer.Trace("Begin dataChanged/amountChanged/statusChanged check");
				// When the Appropriation, the Funding Type, or the Status changes, we need to remove the the old Transaction from the Appropriation Yearly Rollup table
				var oldRollup = dbService.AppropriationRollup.GetRecordFromKeys(oldTransaction.FiscalYear.GetValueOrDefault(), oldTransaction.Appropriation);
				if (oldRollup == null)
				{
					tracer.Trace($"Could not find an Appropriation Yearly Rollup record for appropriation \"{oldTransaction.Appropriation.Id}\"" +
								$" and Fiscal Year \"{oldTransaction.FiscalYear.GetValueOrDefault()}\". \nStep ignored.");
				}
				else
				{
					if (ApplyAmountToTotalAllocated(oldRollup, oldTransaction, false))
					{
						dbService.Update(oldRollup.Entity);
						oldAllocationRemoved = true;
					}
				}

				// Finally, process the new Transaction
				var combinedTransaction = newTransaction.GetCombinedTransactionRecord(oldTransaction);

				if (!oldAllocationRemoved && amountChanged)
				{
					// If the oldTransaction is not processed, then this is an amount change only
					combinedTransaction.Amount = newTransaction.Amount;
					if (newTransaction.Amount != null && newTransaction.Amount != oldTransaction.Amount)
					{
						combinedTransaction.Amount -= oldTransaction.Amount;
					}
					else if (newTransaction.Amount == null)
					{
						combinedTransaction.Amount = oldTransaction.Amount;
					}
				}
				else
				{
					// Add the new transaction with the combined values
					combinedTransaction.Amount = (newTransaction.Amount == null ? oldTransaction.Amount : newTransaction.Amount);
				}

				var rollup = dbService.AppropriationRollup.GetRecordFromKeys(combinedTransaction.FiscalYear.GetValueOrDefault(), combinedTransaction.Appropriation);
				if (rollup != null && ApplyAmountToTotalAllocated(rollup, combinedTransaction, true))
				{
					dbService.Update(rollup.Entity);
				}
			}
		}
		
		private bool ApplyAmountToTotalAllocated(IAppropriationYearlyRollupRecord rollup, ICARTtransactionRecord transaction, bool addAmount = true)
		{
            var amount = transaction.Amount;
            if (!addAmount)
            {
                amount *= -1;
            };

			if (transaction.IsPending)
			{
				tracer.Trace($"Updating the Pending Total Allocated amount of {rollup.PendingTotalAllocated} by {amount}.");
				rollup.PendingTotalAllocated += amount;
			}
			else if (transaction.IsApproved)
			{
				tracer.Trace($"Updating the Approved Total Allocated amount of {rollup.TotalAllocated} by {amount}.");
				rollup.TotalAllocated += amount;
			}

			return true;
		}

        #endregion

    }
}
