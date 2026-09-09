
using System;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Plugins_CommonLibrary.Extensions;

using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Crm.Sdk;
using System.CodeDom;
using System.Web.UI.WebControls;

namespace EXConnect_Plugins
{

	// This plugin will post a transaction into the Monthly Rollup table based for the given fiscal strip
	// (Appropriation, CUFF Account, Funding Type, Fiscal Year, and Fiscal Month) which will be derived
	// from the current date. Because the Rollup tables (both Monthly and Yearly) are setup for Hierarchical
	// capabilites, the CUFF Account tree, related to the transaction, needs to exist in the rollup table.
	// If it does not, in its totality or partially, the missing records will need to be appended by this plugin as well.

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktip Interface project
	public class MonthlyTransactionRollupPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public MonthlyTransactionRollupPlugin( IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}
		public void ExecutePlugin( string messageName, ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction = null )
		{
			if (messageName == MessageNameEnum.Update.ToString())
			{
				ProcessUpdatedTransaction(newTransaction, oldTransaction);
			}
		}

#else
/*
	[CrmPluginRegistration(MessageNameEnum.Create,
	"rcade_carttransaction",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "",
	"Update/Create Monthly Rollup On Transaction Create", 4,
	IsolationModeEnum.Sandbox,
	Description = "Add or Update the Transaction Monthly Rollups table when a Transaction is created.",
	Id = "c62b3ee5-f43e-4f0f-a7b6-68889907ed35")]
*/
	[CrmPluginRegistration(MessageNameEnum.Update,
	"rcade_carttransaction",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_ecatransactiontype,rcade_cuffaccount,rcade_fromcuffaccount,eca_fundingtype,statuscode,eca_parentdocument",
	"Update/Create Monthly Rollup On Transaction Update", 5,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_ecatransactiontype,rcade_cuffaccount,rcade_fromcuffaccount,eca_fundingtype,statuscode,eca_parentdocument",
	Description = "Add or Update the Transaction Monthly Rollups table when a Transaction is updated.",
	Id = "e1c2fb1a-6899-4616-906b-d9558c33cbd4")]

	public class MonthlyTransactionRollupPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Transaction Monthly Rollup plugin");

            ICARTtransactionRecord newTransaction;
            try
            {
                //Get the Transaction record object from the target entity
                newTransaction = dbService.Transaction.GetRecordFromEntity(context.GetTargetEntity());
            }
            catch (Exception ex)
            {
                tracer.Trace("Unexpected Error. Plugin will terminate.\n" + ex.Message);
                return;
            }

            if (newTransaction == null)
			{
				tracer.Trace("Unexpected Target entity. Plugin will terminate.");
				return;
			}

            tracer.Trace("newTransaction record has been found.");
            ICARTtransactionRecord oldTransaction = null;

			if (context.MessageName == MessageNameEnum.Update.ToString())
			{
				oldTransaction = dbService.Transaction.GetRecordFromEntity(context.GetPreImage());
				ProcessUpdatedTransaction(newTransaction, oldTransaction);
			}
			/*
			else
			{
				ProcessNewTransaction(newTransaction);
			}
			*/
		}

#endif

		#region Auxiliary Methods

/*
		/// <summary>
		/// Process a new transaction into the Monthly Rollup table
		/// </summary>
		/// <returns></returns>
		public void ProcessNewTransaction( ICARTtransactionRecord transaction )
		{
			// Retrieve the corresponding Monthly Rollup for the given transaction 

			// Ignore the trnasaction if this is Draft mode
			if (transaction.IsDraft)
			{
				tracer.Trace("The transaction is in Draft mode. No action will be taken and the plugin will terminate.");
				return;
			}

			tracer.Trace($"The transaction is set as {transaction.Status} and type of {transaction.Type}.");

			var fiscalYear = DateTime.Now.FiscalYear();
			if (transaction.FiscalYear != null)
			{
				fiscalYear = transaction.FiscalYear.Value;
			}
			var fiscalMonth = DateTime.Now.MonthOfFiscalYear();

			var monthlyRollup = RetrieveMonthlyRollupRecord(TransferDirection.To, transaction, fiscalMonth, fiscalYear);

			// Get a lock on the record before applying the data changes
			var appropRef = dbService.Appropriation.GetExtendedEntityReference(transaction.Appropriation);
			var accountRef = dbService.CUFFaccount.GetExtendedEntityReference(transaction.CUFFaccount);
			var fundingTypeRef = dbService.FundingType.GetExtendedEntityReference(transaction.FundingType);

			try
			{
				if (SetRecordLock(transaction))
				{
					tracer.Trace("Record lock obtained. Changes being applied.");
					ApplyAmountToRollup(monthlyRollup, transaction.Type, transaction.Status, transaction.Amount);
				}
				// Save the changes
				dbService.Update(monthlyRollup.Entity);
				dbService.RecordLock.UnlockRecord(true);
			}
			finally
			{
				// Make sure we clear the locks
				dbService.RecordLock.UnlockRecord(true);
			}

			if (transaction.IsProgramTransfer)
			{
				// Repeat the process for the FROM account
				monthlyRollup = RetrieveMonthlyRollupRecord(TransferDirection.From, transaction, fiscalMonth, fiscalYear);

				// Get a lock on the record before applying the data changes
				accountRef = dbService.CUFFaccount.GetExtendedEntityReference(transaction.FromCUFFaccount);

				try
				{
					if (SetRecordLock(transaction, TransferDirection.From))
					{
						tracer.Trace("Record lock obtained. Changes being applied on the FROM account.");
						ApplyAmountToRollup(monthlyRollup, transaction.Type, transaction.Status, transaction.Amount * -1);
					}
					// Save the changes
					dbService.Update(monthlyRollup.Entity);

					tracer.Trace("Unlocking the record.");
				}
				finally
				{
					dbService.RecordLock.UnlockRecord(true);
				}
			}
		}
*/

		/// <summary>
		/// Process the updated transaction.
		/// </summary>
		/// <param name="newTransaction"></param>
		/// <param name="oldTransaction"></param>
		/// <exception cref="InvalidPluginExecutionException"></exception>
		public void ProcessUpdatedTransaction( ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction )
		{
			// Verify the change is allowed (ONLY the amount and the status can change

			// Ignore if this transaction is in Draft mode and it is not a Status change
			var isDraft = false;
			if (!string.IsNullOrEmpty(newTransaction.Status) )
			{
				isDraft = newTransaction.Status == "Draft";
			}
			else
			{
				isDraft = oldTransaction.Status == "Draft";
			}

			if (isDraft && oldTransaction.IsDraft)
			{
				tracer.Trace("The transaction Status is set to Draft. No changes to process.");
				return;
			}

			// Ignore the process if the Status and Amount have not changed
			var statusChanged = (!string.IsNullOrEmpty(newTransaction.Status) && newTransaction.Status != oldTransaction.Status);
			var amountChanged = (newTransaction.Amount != null && newTransaction.Amount != oldTransaction.Amount);
			if (statusChanged)
			{
				// Verify that the Status changed due to the Workflow
				var workflow = dbService.TransactionWorkflow.GetRecordForEntityId(newTransaction.Id);
				if (workflow == null)
				{
					statusChanged = false;
				}
			}
			if (!statusChanged && !amountChanged)
			{
				tracer.Trace("The transaction Status and/or Amount has not changed. No changes to process.");
				return;
			}

			// Post any updates to todays/modified date on the transaction
			var fiscalMonth = newTransaction.ModifiedOn.Value.MonthOfFiscalYear();

			// Construct a combined Transaction so we can process
			var combinedTransaction = newTransaction.GetCombinedTransactionRecord(oldTransaction);

			// Set the Fiscal Yeart based on Today's date
			if (combinedTransaction.FiscalYear == null)
			{
				combinedTransaction.FiscalYear = DateTime.Now.FiscalYear();
				tracer.Trace($"Transaction is missing the Fiscal Year. Using current {combinedTransaction.FiscalYear.Value}.");
			}

			var monthlyRollup = RetrieveMonthlyRollupRecord(TransferDirection.To, combinedTransaction, fiscalMonth, combinedTransaction.FiscalYear.Value);

			var amountsChanged = false;

			try
			{
				if (SetRecordLock(combinedTransaction))
				{
					tracer.Trace("Record lock obtained. Changes being applied.");

					// If the status has changed, then decrement from the old status and post in the new one (if not Draft)
					if (statusChanged && !oldTransaction.IsDraft)
					{
						amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, oldTransaction.Status, (oldTransaction.Amount * -1));
					}

					// Apply the changes to the new Status, only if it is not Draft or Rejected and there is a change in the amount
					if (combinedTransaction.IsPending || combinedTransaction.IsApproved)
					{
						if (newTransaction.Amount != null && newTransaction.Amount != 0)
						{
							amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, combinedTransaction.Status, combinedTransaction.Amount);
						}
						else
						{
							// If only the amount has changed, apply just the delta of the transaction
							combinedTransaction.Amount = newTransaction.Amount;
							if (newTransaction.Amount != null && newTransaction.Amount != oldTransaction.Amount)
							{
								combinedTransaction.Amount -= oldTransaction.Amount;
							}

							amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, combinedTransaction.Status, combinedTransaction.Amount);
						}
					}

					if (amountsChanged)
					{
						// Save the changes
						dbService.Update(monthlyRollup.Entity);
					}
				}
			}
			finally
			{
				tracer.Trace("Unlocking the record.");
				dbService.RecordLock.UnlockRecord(true);
			}

			if (combinedTransaction.IsProgramTransfer || combinedTransaction.IsAOA)
			{
				// Repeat the process for the FROM account

				monthlyRollup = RetrieveMonthlyRollupRecord(TransferDirection.From, combinedTransaction, fiscalMonth, combinedTransaction.FiscalYear.Value);

				try
				{
					if (SetRecordLock(combinedTransaction, TransferDirection.From))
					{
						// If the status has changed, then increment to the old status and decrement from the new one
						tracer.Trace("Record lock obtained. Changes being applied on the FROM account.");

						if (statusChanged && !oldTransaction.IsDraft)
						{
							amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, oldTransaction.Status, (oldTransaction.Amount));
						}

						// Apply the changes to the new Status, only if it is not Draft or Rejected and there is a change in the amount
						if (combinedTransaction.IsPending || combinedTransaction.IsApproved)
						{
							if (newTransaction.Amount != null && newTransaction.Amount != 0)
							{
								amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, combinedTransaction.Status, combinedTransaction.Amount * -1);
							}
							else
							{
								// If only the amount has changed, apply just the delta of the transaction
								combinedTransaction.Amount = newTransaction.Amount;
								if (newTransaction.Amount != null && newTransaction.Amount != oldTransaction.Amount)
								{
									combinedTransaction.Amount -= oldTransaction.Amount;
								}

								amountsChanged |= ApplyAmountToRollup(monthlyRollup, combinedTransaction.Type, combinedTransaction.Status, combinedTransaction.Amount * -1);
							}
						}

						if (amountsChanged)
						{
							// Save the changes
							dbService.Update(monthlyRollup.Entity);
						}
					}
				}
				finally
				{
					tracer.Trace("Unlocking the record.");
					dbService.RecordLock.UnlockRecord(true);
				}
			}
			
			// Finally, touch the Parent Document, if one exists for this transaction, so that the 
			// GFMS Document Package Validator plugin executes
			if (statusChanged && combinedTransaction.ParentDocument != null)
			{
				// Toggle the needs Validation field in the Parent document
				var document = dbService.GFMSdocument.GetRecordFromId(combinedTransaction.ParentDocument.Id);
				document.NeedsValidation = !document.NeedsValidation;
				dbService.Update(document.Entity);
			}

		}

		private ITransactionMonthlyRollupRecord RetrieveMonthlyRollupRecord(TransferDirection toFrom, ICARTtransactionRecord transaction, int fiscalMonth, int fiscalYear )
		{
			// Prepare the EntityReferences so they have a Name in case we need to create new Records

			var appropRef = dbService.Appropriation.GetExtendedEntityReference(transaction.Appropriation);
			var fundingTypeRef = dbService.FundingType.GetExtendedEntityReference(transaction.FundingType);
			var accountRef = (toFrom == TransferDirection.To ? dbService.CUFFaccount.GetExtendedEntityReference(transaction.CUFFaccount)
															 : dbService.CUFFaccount.GetExtendedEntityReference(transaction.FromCUFFaccount));

			var monthlyRollup = dbService.MonthlyRollup.GetRecord(transaction.Id, fundingTypeRef, appropRef,
																	accountRef, fiscalMonth, fiscalYear, false);
			EntityReference parentRollup = null;

			if (monthlyRollup == null)
			{
				// Retrieve the transaction rollup parent
				parentRollup = dbService.MonthlyRollup.GetOrSetTransactionRollupParent(toFrom, transaction, fiscalMonth, fiscalYear);

				try
				{
					if (SetRecordLock(transaction, toFrom))
					{
						monthlyRollup = dbService.MonthlyRollup.CreateRecord();
						monthlyRollup.Initialize(fundingTypeRef, appropRef, accountRef, fiscalMonth, fiscalYear);

						monthlyRollup.ParentRollup = parentRollup;

						monthlyRollup.Id = dbService.Create(monthlyRollup.Entity, true);
					}
				}
				finally
				{
					// Clear the lock
					dbService.RecordLock.UnlockRecord(true);
				}
			}

			return monthlyRollup;

		}

		private bool ApplyAmountToRollup( ITransactionMonthlyRollupRecord rollup, string transType, string transStatus, decimal? deltaAmount )
		{
			var amountsChanged = false;
			// Apply the data values based on Transaction Type
			if (transStatus == "Approved")
			{
				if (transType == "AppropriationAllocation")
				{
					rollup.ApprovedFinPlan += deltaAmount;
				}
				else if (transType == "AOA")
				{
					rollup.ApprovedAOAamount += deltaAmount;
				}
				else if (transType == "Commitment")
				{
					rollup.ApprovedCommittedAmount += deltaAmount;
				}
				else if (transType == "ProgramTransfer")
				{
					rollup.ApprovedInternalTransfers += deltaAmount;
				}
				else if (transType == "Obligation")
				{
					rollup.ApprovedObligatedAmount += deltaAmount;
				}

				amountsChanged |= (deltaAmount != 0);

			}
			else if (transStatus == "Pending")
			{
				if (transType == "AppropriationAllocation")
				{
					rollup.PendingFinPlan += deltaAmount;
				}
				else if (transType == "AOA")
				{
					rollup.PendingAOAamount += deltaAmount;
				}
				else if (transType == "Commitment")
				{
					rollup.PendingCommittedAmount += deltaAmount;
				}
				else if (transType == "Obligation")
				{
					rollup.PendingObligatedAmount += deltaAmount;
				}
				else if (transType == "ProgramTransfer")
				{
					rollup.PendingInternalTransfers += deltaAmount;
				}

				amountsChanged |= (deltaAmount != 0);
			}
			else
			{
				tracer.Trace($"No changes made for Transaction Status of \"{transStatus}\" and Amount of {deltaAmount}.");
				return false;
			}

			tracer.Trace($"Changes made for Transaction Status of \"{transStatus}\" and Amount of {deltaAmount}.");

			return amountsChanged;
		}

		private bool SetRecordLock( ICARTtransactionRecord transaction, TransferDirection toFrom = TransferDirection.To)
		{
			// Get a lock on the requested record
			/*
			var appropRef = dbService.Appropriation.GetExtendedEntityReference(transaction.Appropriation);
			var fundingTypeRef = dbService.FundingType.GetExtendedEntityReference(transaction.FundingType);
			var accountRef = (toFrom == TransferDirection.To ? dbService.CUFFaccount.GetExtendedEntityReference(transaction.CUFFaccount)
															 : dbService.CUFFaccount.GetExtendedEntityReference(transaction.FromCUFFaccount) );
			*/

			// Need to get the extended EntityReference since most do not have the Name attribute
			var appropName = string.Empty;
			var fundTypeName = string.Empty;
			var accountName = string.Empty;
			var fiscalYear = string.Empty;

			transaction.GetFiscalStringKeys(toFrom, out appropName, out accountName, out fundTypeName, out fiscalYear, dbService);
			return dbService.RecordLock.LockRollupRecord(transaction.Id, appropName, accountName, fundTypeName, fiscalYear, dbService.LocalDateTime);

		}

		#endregion
	}
}