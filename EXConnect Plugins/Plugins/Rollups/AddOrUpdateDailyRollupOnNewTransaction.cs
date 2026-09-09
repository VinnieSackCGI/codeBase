using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Entities.Interfaces;
using EXConnect_Plugins.Plugin_Handling;

namespace RCADE.Plugins.Entities.CARTTransaction.Create
{
    /// <summary>
    /// Class to create or update daily rollup records on transaction creation.
    /// </summary>
    [CrmPluginRegistration(MessageNameEnum.Create,
    "rcade_carttransaction",
    StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "rcade_appropriation,rcade_cuffaccount,rcade_fulfillmenttype",
    "Daily Rollup on Transaction Create", 4,
    IsolationModeEnum.Sandbox,
    Description = "Add or update daily rollup record on new Transactions",
    Id = "b98e35b0-c7e8-470c-9d60-1c6af333d635")]

    public class AddOrUpdateDailyRollupOnNewTransaction : MainPlugin
	{
        private IEnumerable<rcade_TransactionTypes> allocationTransactions;

        public AddOrUpdateDailyRollupOnNewTransaction()
        {
            allocationTransactions = new List<rcade_TransactionTypes> 
                { 
                    rcade_TransactionTypes.FinPlan, 
                    rcade_TransactionTypes.EOY, 
                    rcade_TransactionTypes.MidYear, 
                    rcade_TransactionTypes.Advance, 
                    rcade_TransactionTypes.IFT, 
                    rcade_TransactionTypes.Direct, 
                    rcade_TransactionTypes.Reimbursement, 
                    rcade_TransactionTypes.PostReimbursement, 
                    rcade_TransactionTypes.Adjustment, 
                    rcade_TransactionTypes.Sweep, 
                    rcade_TransactionTypes.AllotRepFunds, 
                    rcade_TransactionTypes.FinPlanIncremental,
					rcade_TransactionTypes.DomesticRepTransfer 
            };
        }

        protected override void ExecutePlugin(IExtendedPluginContext executeContext)
        {
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Transaction Daily Rollup plugin");

			// Get the Transaction record object from the target entity
			var transaction = dbService.Transaction.GetRecordFromEntity(context.GetTargetEntity());

            var rollup = dbService.DailyRollup.GetRecordFromKeys()

            tracer.Trace($"Transaction Typew: {transaction.Type}");

            if (!this.rollupBL.ShouldRollupTransaction(transaction.rcade_TransactionType.Value, transaction.rcade_reimbursementaction != null, transaction.rcade_ParentCARTTransaction != null))
            {
                this.context.Trace($"will not rollup");
                return;
            }

            // don't run if transaction is rejected, draft, or prior year
            if (transaction.rcade_StatusValue?.Name == "Rejected" || transaction.rcade_StatusValue?.Name == "Draft" || transaction.IsPriorYear)
            {
                return;
            }
            //Also try to retrieve the status value if the field is populated as the name might not populate otherwise
            if (transaction.rcade_StatusValue != null)
            {
                var statusValueName = this.statusFlowRepository.GetStatusFlowByID(transaction.rcade_StatusValue.Id);
                if (statusValueName?.rcade_Name == "Rejected" || statusValueName?.rcade_Name == "Draft")
                {
                    return;
                }
            }

            //if a bulk uploaded transaction was valid, but the balance has since became unavailable, reject the transaction with a comment and exit out.
            if (!this.allocationTransactions.Contains(transaction.rcade_TransactionType.Value))
            {

                var errorModel = this.CheckIfBalanceIsAvailableForBulkUploadedTransaction(transaction);
                if (!errorModel.IsValid)
                {
                    if (statusFlowBL.TransactionTypeHasStatusFlow(transaction.rcade_TransactionType))
                    {
                        var rejectedStatus = this.statusFlowRepository.GetStatusFlowByQueryModel(new StatusFlowQueryModel() { TransactionType = transaction.rcade_TransactionType, TransactionCode = transaction.rcade_TransCode, StatusValueName = "Rejected" }).ToEntityReference();
                        transaction.rcade_StatusValue = rejectedStatus;
                    }
                    var dateToUse = this.userService.GetCurrentUsersCurrentTime().ToString("MM-dd-yyyy");
                    transaction.rcade_dscfocomment += $"\n{dateToUse} Bulk Upload Rejection Reason: {errorModel.ErrorString}";
                    return;
                }

            }
            //If a bulk uploaded comptroller form with unavailable balance, reject the transaction with a comment and exit out.
            if (transaction.rcade_TransactionType == rcade_transactiontypes.Adjustment || transaction.rcade_TransactionType == rcade_transactiontypes.Direct || transaction.rcade_TransactionType == rcade_transactiontypes.MidYear ||
                transaction.rcade_TransactionType == rcade_transactiontypes.EOY || transaction.rcade_TransactionType == rcade_transactiontypes.Advance || transaction.rcade_TransactionType == rcade_transactiontypes.FinPlanIncremental)
            {

                var errorModel = this.CheckIfBalanceIsAvailableForBulkUploadedComptroller(transaction);
                if (!errorModel.IsValid)
                {
                    if (statusFlowBL.TransactionTypeHasStatusFlow(transaction.rcade_TransactionType))
                    {
                        var rejectedStatus = this.statusFlowRepository.GetStatusFlowByQueryModel(new StatusFlowQueryModel() { TransactionType = transaction.rcade_TransactionType, TransactionCode = transaction.rcade_TransCode, StatusValueName = "Rejected" }).ToEntityReference();
                        transaction.rcade_StatusValue = rejectedStatus;
                    }
                    var dateToUse = this.userService.GetCurrentUsersCurrentTime().ToString("MM-dd-yyyy");
                    transaction.rcade_dscfocomment += $"\n{dateToUse} Bulk Upload Rejection Reason: {errorModel.ErrorString}";
                    return;
                }

            }
            if (transaction.rcade_Appropriation != null)
            {
                if (transaction.rcade_CUFFAccount != null)
                {
                    this.context.Trace($"start rollup checks");

                    var rollup = this.rollupRepository.GetDailyRollupRecordForCurrentDay(transaction.rcade_Appropriation.Id, transaction.rcade_CUFFAccount.Id, transaction.FiscalYear);

                    if (rollup == null)
                    {
                        this.CreateNewDailyRollup(transaction);
                    }
                    else
                    {
                        this.UpdateRollupForAssociation(rollup, transaction);
                    }
                }

                if (transaction.rcade_fromcuffaccount != null)
                {
                    this.context.Trace($"from cuff account: {transaction.rcade_fromcuffaccount}");

                    var fromAccountRollup = this.rollupRepository.GetDailyRollupRecordForCurrentDay(transaction.rcade_Appropriation.Id, transaction.rcade_fromcuffaccount.Id, transaction.FiscalYear);
                    var negativeTransaction = new rcade_CARTTransaction() { IsInMemoryTransactionBeingUsedForFromAccountRollupTransactionCalculations = true }.CoallesceEntity(transaction);
                    if (negativeTransaction.rcade_TransactionType != rcade_transactiontypes.RequestRepFunds)
                    {
                        //we create transactions of negatives to mimic behavior for from accounts for transacitons types that aren't Request Rep Funds
                        negativeTransaction.rcade_Amount = new Money(-negativeTransaction.rcade_Amount.Value);
                    }
                    if (fromAccountRollup == null)
                    {
                        var newFromAccountDailyRollup = this.CreateNewDailyRollupForFromAccount(negativeTransaction);
                        transaction.rcade_FromAccountsDailyRollup = newFromAccountDailyRollup.ToEntityReference();
                    }
                    else
                    {
                        var updatedRollup = this.UpdateRollupForAssociation(fromAccountRollup, negativeTransaction);
                        transaction.rcade_FromAccountsDailyRollup = updatedRollup.ToEntityReference();
                    }
                }
            }
            else
            {
                this.context.Trace("Does not have a valid appropriation, exiting");
            }
        }

        private ErrorModel CheckIfBalanceIsAvailableForBulkUploadedTransaction(rcade_CARTTransaction transaction)
        {
            var errorModel = new ErrorModel(true);
            if (transaction.rcade_CARTBulkUploadProcess != null)
            {
                var appropriation = this.appropriationRepository.GetRelatedEntity(transaction.rcade_Appropriation);
                var cuffAccount = this.accountRepository.GetRelatedEntity(transaction.rcade_CUFFAccount);
                if (appropriation != null && cuffAccount != null)
                {
                    var yearlyRollup = this.rollupRepository.GetYearlyRollupRecordForCurrentYear(appropriation.Id, cuffAccount.Id);
                    errorModel = this.rollupValidator.ValidateYearlyRollupBalanceAvailable(transaction, appropriation, cuffAccount, yearlyRollup);
                }
                else
                {
                    string errorString = $"No valid appropriation or account is selected on this transaction.";
                    errorModel = new ErrorModel(false, errorString);
                }
            }
            return errorModel;
        }

        //Copy of the Bulk Upload Balance method but checks From Cuff Account instead
        private ErrorModel CheckIfBalanceIsAvailableForBulkUploadedComptroller(rcade_CARTTransaction transaction)
        {
            var errorModel = new ErrorModel(true);
            if (transaction.rcade_CARTBulkUploadProcess != null)
            {
                var appropriation = this.appropriationRepository.GetRelatedEntity(transaction.rcade_Appropriation);
                var cuffAccount = transaction.rcade_fromcuffaccount != null ? this.accountRepository.GetRelatedEntity(transaction.rcade_fromcuffaccount) : null;
                //Always validate that the amount is non-negative.
                if (transaction.rcade_Amount.Value <= 0)
                {
                    errorModel = new ErrorModel(false, "Transaction amount must be greater than zero.\n");
                }
                //If there is no From Cuff Account, then no other balance validation needs to be done.
                else if (appropriation != null && cuffAccount != null)
                {
                    var yearlyRollup = this.rollupRepository.GetYearlyRollupRecordForCurrentYear(appropriation.Id, cuffAccount.Id);
                    if (appropriation.rcade_CRFactor < 1M && (yearlyRollup?.rcade_TotalAllocatedAmount == null || yearlyRollup?.rcade_TotalAllocatedAmount.Value <= 0))
                    {
                        errorModel = new ErrorModel(false, $"The Total Allocated Amount for account {cuffAccount.rcade_CUFFAccountName} is 0 under this CR.\n");
                    }
                    else
                    {
                        errorModel = this.rollupValidator.ValidateYearlyRollupBalanceAvailable(transaction, appropriation, cuffAccount, yearlyRollup);
                    }
                }
            }
            return errorModel;
        }

        /// <summary>
        /// Create a new rollup record for the day for the appropriation/account association.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="appaccassoc"></param>
        private void CreateNewDailyRollup(rcade_CARTTransaction transaction)
        {
            rcade_DailyRollupForAppAccAssocs newDailyRollup = new rcade_DailyRollupForAppAccAssocs();
            newDailyRollup.InitializeValues(transaction.rcade_Appropriation, transaction.rcade_CUFFAccount, transaction.FiscalYear ?? this.fiscalYearService.GetCurrentFiscalYear(), transaction, RollupType.ToAccountRollup);
            this.rollupBL.AddAmountToDailyRollup(newDailyRollup, transaction);
            Guid newRollupGuid = this.rollupRepository.Create(newDailyRollup);
            var rollup = this.rollupRepository.GetDailyRollupRecord(newRollupGuid);
            transaction.rcade_DailyRollupForAppAccAssocs = rollup.ToEntityReference();

        }

        /// <summary>
        /// Create a new rollup record for the day for the appropriation/account association.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="appaccassoc"></param>
        private rcade_DailyRollupForAppAccAssocs CreateNewDailyRollupForFromAccount(rcade_CARTTransaction transaction)
        {
            rcade_DailyRollupForAppAccAssocs newDailyRollup = new rcade_DailyRollupForAppAccAssocs();
            newDailyRollup.InitializeValues(transaction.rcade_Appropriation, transaction.rcade_fromcuffaccount, transaction.FiscalYear ?? this.fiscalYearService.GetCurrentFiscalYear(), transaction, RollupType.FromAccountRollup);
            this.rollupBL.AddAmountToDailyRollup(newDailyRollup, transaction);
            Guid newRollupGuid = this.rollupRepository.Create(newDailyRollup);
            var rollup = this.rollupRepository.GetDailyRollupRecord(newRollupGuid);
            return rollup;

        }

        /// <summary>
        /// Update rollup for association.
        /// If a rollup has been made for an appropriation/account association. within the current day,
        /// add the value of this transaction to it.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="appaccassoc"></param>
        private rcade_DailyRollupForAppAccAssocs UpdateRollupForAssociation(rcade_DailyRollupForAppAccAssocs rollupToUpdate, rcade_CARTTransaction transaction)
        {
            this.rollupBL.AddAmountToDailyRollup(rollupToUpdate, transaction);
            this.rollupRepository.Update(rollupToUpdate);
            transaction.rcade_DailyRollupForAppAccAssocs = rollupToUpdate.ToEntityReference();
            return rollupToUpdate;
        }

        /// <summary>
        /// Initialize dependencies.
        /// </summary>
        private void InitializeDependencies(rcade_CARTTransaction transaction)
        {
            this.fiscalYearService = this.context.GetDependency<IFiscalYearService>(); ;
            this.rollupRepository = this.context.GetAdminRepoInstance<IRollupRepository>();
            this.rollupValidator = this.context.GetDependency<IRollupValidator>();
            this.appropriationRepository = this.context.GetUserRepoInstance<IAppropriationRepository>();
            this.accountRepository = this.context.GetUserRepoInstance<ICUFFAccountRepository>();
            this.rollupBLRetriever = this.context.GetDependency<IRollupBLRetriever>();
            this.rollupBL = this.rollupBLRetriever.GetDailyRollupBLByTransactionType(transaction.rcade_TransactionType.Value);
            this.statusFlowBL = this.context.GetDependency<IStatusFlowBL>();
            this.statusFlowRepository = this.context.GetDependency<IStatusFlowRepository>();
            this.userService = this.context.GetDependency<IUserService>();
        }
    }
}
