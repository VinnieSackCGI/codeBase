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
    using System.CodeDom;
    using System.Data.OleDb;
    using System.Net.NetworkInformation;
    using System.Threading;
    using Common;
    using DocumentFormat.OpenXml.Drawing;
    using DocumentFormat.OpenXml.Presentation;
    using EXConnect_Plugins.Entities.Adapters.Interfaces;
    using EXConnect_Plugins.Plugins.Common;
    using Plugins_CommonLibrary.Extensions;

    // The following plugin is meant to validate whether a "Spending" transaction
    // can proceed based on the available funds. The available funds will be
    // verified from the corresponding Yearly Rollup table for the given account's
    // lowest tracking level (sub-Program, Program, and Division).

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class SpendingTransactionValidationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public SpendingTransactionValidationPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(string messageName, ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction = null)
		{

#else

    [CrmPluginRegistration(MessageNameEnum.Create,
    "rcade_carttransaction",
    StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "",
    "Funds Availability Validation On Transaction Create", 1,
    IsolationModeEnum.Sandbox,
    Description = "Verify that funds are available for a given amount when a Transaction is created.",
    Id = "10708991-d5ee-44da-b322-c68bf6d7385e")]

    [CrmPluginRegistration(MessageNameEnum.Update,
    "rcade_carttransaction",
    StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_fundingtype,rcade_cuffaccount,rcade_fromcuffaccount,"
                                    + "eca_obligationtype,rcade_documentnumber,rcade_obligationnumber,cr15a_approver,statuscode,eca_initialstatus,"
                                    + "rcade_transcode,rcade_ibisrequestcode,rcade_comment",
    "Funds Availability Validation On Transaction Update", 2,
    IsolationModeEnum.Sandbox,
    Image1Type = ImageTypeEnum.PreImage,
    Image1Name = ImageTypeName.PreImage,
    Image1Attributes = "rcade_amount,rcade_appropriation,rcade_fiscalyear,eca_ecatransactiontype,rcade_cuffaccount,rcade_fromcuffaccount,eca_fundingtype,"
                        + "eca_obligationtype,rcade_documentnumber,rcade_obligationnumber,cr15a_approver,statuscode,eca_initialstatus,rcade_parentcarttransaction,"
                        + "eca_allotment,rcade_transcode,rcade_ibisrequestcode,rcade_comment,createdby",
    Description = "Verify that funds are available for a given amount when a Transaction is updated and generate Document Number when necessary.",
    Id = "052acc0b-31e1-424d-bf0e-b83453d64230")]

    public class SpendingTransactionValidationPlugin : MainPlugin
    {
        const string ECA_PROGRAM_ANALYST = "% Analyst%";
        const string PROGAM_TRANSFER_TYPE = "ProgramTransfer";
        const string PENDING_STATUS = "Pending";
        const string REJECTED_STATUS = "Rejected";
        const string REJECTED_STATUS_VALUE = "784510004";

        protected override void ExecutePlugin(IExtendedPluginContext executeContext)
        {
            InitializeContext(executeContext, true);

            tracer.Trace("Beginning execution of Transaction Validation plugin");

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
            tracer.Trace("Obligation Type: " + newTransaction?.ObligationType?.Name);
            tracer.Trace("Obligation Number: " + newTransaction?.ObligationNumber + " Document Number: " + newTransaction?.DocumentNumber);

            ICARTtransactionRecord oldTransaction = null;

            var messageName = context.MessageName;
            if (messageName == MessageNameEnum.Update.ToString())
            {
                oldTransaction = dbService.Transaction.GetRecordFromEntity(context.GetPreImage());

                // Verify that current user is not System when vaidating on Update
                var fullname = dbService.SystemUser.GetCodeFromId(dbService.UserId);
                if (fullname == "SYSTEM")
                {
                    // Use the record creator as the user
                    var userId = oldTransaction.CreatedBy.Id;
                    ResetUserContext(userId);
                }
            }

#endif
            if (messageName == MessageNameEnum.Create.ToString())
            {
                //Is this necessary, a new Transaction would never be rejected right?
                if (IsRejectedProgramTransfer(newTransaction))
                {
                    return;
                }

                ValidateTransaction(newTransaction);
            }
            else
            {
                //Resolves an issue where newTransaction doesn't include ProgramTransfer related information
                var combinedTransaction = newTransaction.GetCombinedTransactionRecord(oldTransaction);
                if (IsRejectedProgramTransfer(combinedTransaction))
                {
                    return;
                }

                ValidateUpdatedTransaction(newTransaction, oldTransaction);
            }

            ValidateProgramTransfer(newTransaction);
        }


        #region Auxiliary Methods
        private void ValidateProgramTransfer(ICARTtransactionRecord transaction)
        {
            //Set Pending Status for Program Transfers processed by Program Analysts
            //var isProgramAnalyst = dbService.SystemUser.IsUserInRole(dbService.UserId, ECA_PROGRAM_ANALYST);
            var isProgramTransferType = transaction?.Type == PROGAM_TRANSFER_TYPE;

            if (isProgramTransferType)
            {
                tracer.Trace($"{PROGAM_TRANSFER_TYPE} type processed for a Program Analyst. Setting Status to {PENDING_STATUS}.");
                transaction.InitialStatus = PENDING_STATUS;
                transaction.Status = PENDING_STATUS;
            }
        }

        private bool IsRejectedProgramTransfer(ICARTtransactionRecord transaction)
        {
            if (transaction == null)
            {
                tracer.Trace("Null transaction found. Skipping IsRejectedProgramTransfer check.");
                return false;
            }
            var isProgramTransferType = transaction?.Type == PROGAM_TRANSFER_TYPE;
            if (isProgramTransferType) 
            {
                tracer.Trace("Transaction is a Program Transfer");
                var initialStatus = transaction?.InitialStatus;
                var status = transaction?.Status;

                if (initialStatus == REJECTED_STATUS || status == REJECTED_STATUS || status == REJECTED_STATUS_VALUE)             
               {
                    tracer.Trace("Transaction is a Rejected Program Transfer. Plugin will terminate");
                    return true;
                }
                else
                {
                    tracer.Trace($"Transaction Initial Status: {initialStatus}.\nTransaction Status: {status}");
                }
            }
            else
            {
                tracer.Trace($"Transaction type is {transaction?.Type}.");
            }
            return false;
        }

        /// <summary>
        /// Validate a new transaction amount against the Yearly Rollup available balance
        /// </summary>
        /// <returns></returns> 
        private string ValidateTransaction(ICARTtransactionRecord transaction, bool fromUpdate = false)
        {
            // Bypass the validation if the Status of the transaction is set to Draft or Pending
            // and there's a workflow for it.

            tracer.Trace($"Processing a \"{transaction.Type}\" transaction.");

            var errMsg = string.Empty;


#if !DESKTOP
            if (!fromUpdate && transaction.FiscalYear == null)
            {
                transaction.FiscalYear = this.dateTimeLocal.FiscalYear();
            }
#endif

            // Generate the Document Number before everything else
            if (transaction.IsCommitment || transaction.IsObligation)
            {
                //if (string.IsNullOrEmpty(transaction.DocumentNumber) || transaction.IsAutoGenerateDocumentNumber){
                // D9 Surcharge — obligation number is set by the canvas app, skip generation
                var obligationTypeNameCheck = GetObligationTypeName(transaction.ObligationType);
                if (string.Equals(obligationTypeNameCheck, "Surcharge - D9", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(transaction.ObligationNumber))
                {
                    tracer.Trace($"Surcharge - D9: obligation number already set to \"{transaction.ObligationNumber}\". Document Number = \"{transaction.DocumentNumber}\". Skipping generation.");
                }
                else if (string.IsNullOrEmpty(transaction.DocumentNumber) || transaction.IsAutoGenerateDocumentNumber)
                {
                    // Retrieve the document number from the parent document, if one exists
                    IGFMSdocumentRecord parentDocument = null;
                    if (transaction.ParentDocument != null)
                    {
                        parentDocument = dbService.GFMSdocument.GetRecordFromId(transaction.ParentDocument.Id);
                        if (!string.IsNullOrEmpty(parentDocument.DocumentNumber))
                        {
                            // Verify the Document Number is not a GUID
                            Guid docNum = Guid.Empty;
                            if (!Guid.TryParse(parentDocument.DocumentNumber, out docNum))
                            {
                                transaction.DocumentNumber = parentDocument.DocumentNumber;
                                tracer.Trace($"Document Number \"{parentDocument.DocumentNumber}\" extracted from the Parent Document.");
                            }
                            else
                            {
                                tracer.Trace($"Document Number \"{parentDocument.DocumentNumber}\" is a GUID and will be regenerated.");
                            }
                        }
                    }

                    // Generate the document number for the transaction if we could not get one from the parent document
                    if (string.IsNullOrEmpty(transaction.DocumentNumber))
                    {
                        var generateDocNumber = new DocumentNumberGeneration(dbService, tracer);

                        if (transaction.IsObligation)
                        {
                            tracer.Trace("Setting up a Document Number");
                            var obligationType = transaction.ObligationType;
                            if (obligationType != null && string.IsNullOrEmpty(obligationType.Name))
                            {
                                obligationType = dbService.ObligationType.GetExtendedEntityReference(obligationType);
                            }

                            if (transaction.Code == "M9" && obligationType.Name == "Bulk Funding Purchase Card")
                            {
                                // Retrieve the number from the associated Credit card Obligation Number
                                tracer.Trace("Retrieving from the associated Credit card Obligation Number");
                                transaction.DocumentNumber = transaction.GetCreditCardObligationNumber(dbService);
                            }

                            else
                            {

                                transaction.DocumentNumber = generateDocNumber.GenerateObligationDocumentNumber(transaction.Allotment, obligationType, transaction.Post,
                                                                                                                transaction.Code, transaction.FiscalYear.Value,
                                                                                                                parentDocument != null);
                            }

                            tracer.Trace($"New number \"{transaction.DocumentNumber}\".");

                            if (!string.IsNullOrEmpty(transaction.DocumentNumber))
                            {
                                transaction.AutoGenerateDocumentNumber = "No";
                            }
                        }
                        else if (transaction.IsCommitment)
                        {
                            transaction.DocumentNumber = generateDocNumber.GenerateCommitmentDocumentNumber(transaction.Allotment, transaction.Appropriation,
                                                                                                            transaction.Code, transaction.FiscalYear.Value,
                                                                                                            parentDocument != null);
                        }

                        // Keep track of the document number

                        // Store the document number on the parent, if one exists
                        if (transaction.ParentDocument != null)
                        {
                            tracer.Trace("Generated Document Number being stored in Parent Dcoument.");
                            parentDocument.DocumentNumber = transaction.DocumentNumber;
                            dbService.Update(parentDocument.Entity);

                        }
                    }
                }
                var obligationTypeNameSync = GetObligationTypeName(transaction.ObligationType);
                if (!string.Equals(obligationTypeNameSync, "Surcharge - D9", StringComparison.OrdinalIgnoreCase))
                {
                    if (transaction.ObligationNumber != transaction.DocumentNumber)
                    {
                        transaction.ObligationNumber = transaction.DocumentNumber;
                    }
                }
                /*if (transaction.ObligationNumber != transaction.DocumentNumber) 
				{
					transaction.ObligationNumber = transaction.DocumentNumber;
				}*/
            }

            IWorkflowRecord transactionWorkflow = null;
            if (fromUpdate)
            {
                transactionWorkflow = dbService.TransactionWorkflow.GetRecordForEntityId(transaction.Id);
            }
            var found = (transactionWorkflow != null ? "Found" : "Not Found");
            tracer.Trace($"Workflow for transaction {found}.");

            if (transactionWorkflow != null && transaction.InitialStatus != "Reset"
                && transaction.IsDraft)
            {
                tracer.Trace("The transaction is set to Draft. Validation will be ignored.");
                return errMsg;
            }
            else if (transaction.InitialStatus != null && ("Draft Reset").Contains(transaction.InitialStatus))
            {
                // Ignore also if this transaction has been set as a Draft
                if (transaction.InitialStatus == "Draft")
                {
                    tracer.Trace("The transaction is saved as Draft. Validation will be ignored.");
                    return null;
                }

                if (transactionWorkflow != null)
                {
                    // Clear the Initial Status if the workflow has already been processed
                    transaction.InitialStatus = null;
                }
            }

            var error = TransactionValidator.TransactionKeyValidation(transaction, dbService, tracer);

            if (!string.IsNullOrEmpty(error))
            {

                tracer.Trace(error);
                throw new InvalidPluginExecutionException(OperationStatus.Failed, error);
            }

            // if approver is null assign Jen as the approver
            if (transaction.Approver == null)
            {
                //transaction.Approver = dbService.SystemUser.GetEntityReferenceFromCode("Jenyleen Fernandez");
                transaction.Approver = dbService.SystemUser.GetEntityReferenceFromCode(dbService.SystemUser.GetCodeFromId(dbService.UserId));

            }

            // Ignore the call if this transaction is in Draft mode by choice
            if (transaction.InitialStatus == "Draft")
            {
                transaction.InitialStatus = null;
                return errMsg;
            }
            /* Outdated auto approve logic no longer applicable
			else if (!fromUpdate)
			{

				// Start by checking if the transaction can be auto approved
				if (!TransactionValidator.CanProceedWithValidation(transaction, dbService, tracer))
				{
					transaction.InitialStatus = "Pending";
				}
			}
            */

            tracer.Trace($"Transaction Status = {transaction.Status}, Initial Status set to \"{transaction.InitialStatus}\".");

            // ==========================================================================================
            // Check the type of transaction we are dealing with to determine what we will be using for the validation as follows.
            //   Appropriation Allocation - validate against the Appropriation Yearly Rollup table
            //   Transfers and Spending - validate the From against the Yearly Transaction Rollup table.
            //   Obligations - Validate against the referenced Commitment (if any), otherwise against the Yearly Rollup table
            // ==========================================================================================

            // Check ONLY if the transaction is not "0"
            if (transaction.Amount != 0)
            {
                // Bypass if this is a prior fiscal year
                if (transaction.FiscalYear != null && transaction.FiscalYear.Value < DateTime.Now.FiscalYear())
                {
                    return errMsg;
                }
                
                /*
                if (transaction.IsAppropriationAllocation)
                {
                    // Validate against the Appropriation Yearly Rollup
                    var rollup = dbService.AppropriationRollup.GetRecordFromKeys(transaction.FiscalYear.GetValueOrDefault(), transaction.Appropriation);
                    if (rollup == null || transaction.Amount > rollup?.AppropriatedBalance)
                    {
                        if (rollup == null)
                        {
                            tracer.Trace("An Appropriation Yearly Rollup record for the given Fiscal Year and Appropriation could not be found. The transaction will be set to Pending.");
                        }
                        else
                        {
                            var appropName = dbService.Appropriation.GetCodeFromId(transaction.Appropriation.Id);
                            errMsg = $"The transaction amount change of {transaction.Amount} exceeds the available funds for the appropriation \"{appropName}\". The transaction will be set to Pending.";
                            tracer.Trace(errMsg);
                        }
                        transaction.InitialStatus = "Pending";
                    }

                    // Validation complete for Appropriation Allocation
                    return errMsg;
                }
                */

                // Set the target Available Balance based on whether the user has Budget Lead role and the Appropriation is under Funds Control 
                // A budget Lead user can impose a negativbe balance ONLY if the Appropriation is under Funds Control which allows the Availalble Balance
                // to exceed the Allocated Balance as long as it does not surpass the Available Balance for the overall Appropriation.
                // This cap is computed by adding the Appropriation Rollup Avaialble Balance and the Yearly Rollup Avaliable Balance.

                // First check if the submitter has thee Budget Lead role
                var isBudgetLead = dbService.SystemUser.IsUserInRole(dbService.UserId, "% Budget Lead%");
                var isExecutionAnalyst = dbService.SystemUser.IsUserInRole(dbService.UserId, "% Execution Analyst%");


                var maxAvailableBalance = 0M;

                if ((isBudgetLead || isExecutionAnalyst) && !transaction.IsProgramTransfer)
                {
                    tracer.Trace("The user is in the Budget Lead team. Funds Available will use a different basis.");
                    // Check for the whether the transaction's Appropriation is under Funds Control
                    var appropRecord = dbService.Appropriation.GetRecordFromId(transaction.Appropriation.Id);
                    if (appropRecord.IsFundsControlled)
                    {
                        // Get the Appropriation Yearly Rollup's Available Balance
                        var appropRollup = dbService.AppropriationRollup.GetRecordFromKeys(transaction.FiscalYear.Value, transaction.Appropriation);
                        if (appropRollup != null)
                        {
                            maxAvailableBalance = appropRollup.AppropriatedBalance.GetValueOrDefault(0);
                        }
                        // Add the Yearly Rollup Available balance for the given appropriation
                        maxAvailableBalance += dbService.YearlyRollup.GetAvailableBalanceForAppropriation(transaction.Appropriation, transaction.FiscalYear.Value);
                    }
                }

                // If this is an Obligation that references a commitment, validate against the Approved Balance on the Commitment
                if (transaction.IsObligation && transaction.ParentCommitment != null)
                {
                    // Verify that the Parent Commitment record is approved
                    var commitment = dbService.Transaction.GetRecordFromId(transaction.ParentCommitment.Id);
                    if (commitment == null)
                    {
                        throw new InvalidPluginExecutionException("The Commitment Reference for this Obligation cannot be found. Transactrion cannot continue.");
                    }

                    if (!commitment.IsApproved)
                    {
                        tracer.Trace("Referenced Commitment is not approved. This obligation will be set to the same status as the commitment.");
                        transaction.InitialStatus = commitment.Status;
                    }
                    else
                    {
                        // Verify the commitment has funds
                        var obligatedFunds = commitment.GetTotalObligatedFunds(dbService);
                        var availableFunds = commitment.Amount - obligatedFunds;

                        // Reset the available funds if the submitter is a Budget Lead 
                        if ((isBudgetLead || isExecutionAnalyst) && maxAvailableBalance > 0)
                        {
                            availableFunds = maxAvailableBalance - obligatedFunds;
                        }

                        if (availableFunds < transaction.Amount)
                        {
                            // Commented out this logic as new requirement is to auto-approve transactions for BE analysts and budget lead
                            //errMsg = "The referenced Commitment has insufficient funds for this obligation. This obligation will be set to Pending.";
                            //tracer.Trace(errMsg);
                            //transaction.InitialStatus = "Pending";
                            transaction.InitialStatus = "Approved";
                            transaction.Status = "Approved";
                        }
                        else
                        {
                            transaction.InitialStatus = "Approved";
                            transaction.Status = "Approved";
                        }
                    }
                    /*

                    if (transaction.ObligationType.Equals("Travel - TO"))
                    {
                        transaction.InitialStatus = "Pending";
                        tracer.Trace("Inside setting pending because Travel");
                    }
                    else
                    {
                        transaction.InitialStatus = "Approved"; //added new to make transactions approved
                        tracer.Trace("inside setting approved because not a TO");
                    }
                    */

                    return errMsg;
                }

                // Finally, retrieve the corresponding Yearly Rollup for the given transaction 
                // and verify that the Approved transaction amount does not exceed the Available Funds
                // If there is no rollup record, the transaction should not be approved and it will stay as Pending
                // regardless who the user is issuing it.

                // But first, determine what account to validate against based on the Transaction Type
                // For Program Transfers or AOAs, only the FROM account needs to be validated since funds MUST be available from the source.
                EntityReference account = transaction.CUFFaccount;
                if (transaction.IsProgramTransfer || transaction.IsAOA)
                {
                    account = transaction.FromCUFFaccount;
                }

                var yearlyRollup = dbService.YearlyRollup.GetRecord(transaction.FundingType, transaction.Appropriation,
                                                                    account, transaction.FiscalYear, false);
                if (yearlyRollup == null)
                {
                    errMsg = "The referenced Commitment has insufficient funds for this obligation. This obligation will be set to Pending.";
                    tracer.Trace(errMsg);


                    transaction.InitialStatus = "Approved"; //added new to make transactions approved
                    transaction.Status = "Approved";

                    return errMsg;
                }
                else if (!(isBudgetLead || isExecutionAnalyst) || transaction.IsProgramTransfer)
                {
                    maxAvailableBalance = yearlyRollup.ApprovedAvailableBalance.GetValueOrDefault(0);
                }

                // If the rollup is found, check that the transaction can be covered by the available balance, if is to be approved.
                // On Obligations, check that the Obligation amount can be covered by the Committed amount.

                if (transaction.IsApproved || transaction.InitialStatus == "Approved")
                {
                    if (transaction.Amount > maxAvailableBalance)
                    {
                        errMsg = "The transaction amount exceeds the Yearly Rollup's Approved Available Balance amount. The Status will be set to \"Pending\".";
                        tracer.Trace(errMsg);



                        transaction.InitialStatus = "Approved"; //added new to make transactions approved
                        transaction.Status = "Approved";

                    }
                }

            }


            return errMsg;
        }

        private void ValidateUpdatedTransaction(ICARTtransactionRecord newTransaction, ICARTtransactionRecord oldTransaction)
        {
            var combinedTransaction = newTransaction.GetCombinedTransactionRecord(oldTransaction);

            var locked = oldTransaction.IsApproved || string.Equals(oldTransaction.Status, "Pending", StringComparison.OrdinalIgnoreCase);

            // Use combinedTransaction for checks (merged record), not newTransaction (target-only)
            var cuffOnlyChange =
                Different(combinedTransaction.CUFFaccount, oldTransaction.CUFFaccount)
                && !Different(combinedTransaction.Appropriation, oldTransaction.Appropriation)
                && !Different(combinedTransaction.FromCUFFaccount, oldTransaction.FromCUFFaccount)
                && !Different(combinedTransaction.FundingType, oldTransaction.FundingType)
                && (!combinedTransaction.FiscalYear.HasValue || combinedTransaction.FiscalYear == oldTransaction.FiscalYear)
                && (string.IsNullOrEmpty(combinedTransaction.Type) || combinedTransaction.Type == oldTransaction.Type);

            // If locked and not CUFF-only, prevent fiscal-strip changes (do NOT include CUFF here)
            if (locked && !cuffOnlyChange && (
                Different(combinedTransaction.Appropriation, oldTransaction.Appropriation) ||
                Different(combinedTransaction.FromCUFFaccount, oldTransaction.FromCUFFaccount) ||
                Different(combinedTransaction.FundingType, oldTransaction.FundingType) ||
                (combinedTransaction.FiscalYear.HasValue && combinedTransaction.FiscalYear != oldTransaction.FiscalYear) ||
                (!string.IsNullOrEmpty(combinedTransaction.Type) && combinedTransaction.Type != oldTransaction.Type)
            ))
            {
                throw new InvalidPluginExecutionException(
                    "Changes to Appropriation, From CUFF, Funding Type, Transaction Type, or Fiscal Year are not allowed once Pending/Approved."
                );
            }

            /// TESTING START
            /// 
            var targetEntity = context.GetTargetEntity();
            EnforceEditRules_PendingG2_ApprovedNonG2(newTransaction, oldTransaction, combinedTransaction);


            // --- Targeted exception: allow CUFF change only when moving/being in Draft ---
            var movingToDraft = string.Equals(combinedTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(newTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase);

            // If user is setting to Draft, allow ONLY CUFF change (no other fiscal strip changes)
            if (movingToDraft)
            {
                /* REVERT BACK 
                var otherFiscalStripChanged =
                    Different(newTransaction.Appropriation, oldTransaction.Appropriation) ||
                    Different(newTransaction.FromCUFFaccount, oldTransaction.FromCUFFaccount) ||
                    Different(newTransaction.FundingType, oldTransaction.FundingType) ||
                    (newTransaction.FiscalYear.HasValue && newTransaction.FiscalYear != oldTransaction.FiscalYear) ||
                    (!string.IsNullOrEmpty(newTransaction.Type) && newTransaction.Type != oldTransaction.Type);*/

                ///TESTING
                var otherFiscalStripChanged =
                    Different(combinedTransaction.Appropriation, oldTransaction.Appropriation) ||
                    Different(combinedTransaction.FromCUFFaccount, oldTransaction.FromCUFFaccount) ||
                    Different(combinedTransaction.FundingType, oldTransaction.FundingType) ||
                    (combinedTransaction.FiscalYear.HasValue && combinedTransaction.FiscalYear != oldTransaction.FiscalYear) ||
                    (!string.IsNullOrEmpty(combinedTransaction.Type) && combinedTransaction.Type != oldTransaction.Type);
                ///TESTING

                //All changes should be allowed for Draft, so we can disable this check
                //if (otherFiscalStripChanged)
                //{
                //    throw new InvalidPluginExecutionException(
                //        "When setting a transaction to Draft, only the CUFF Account can be changed."
                //    );
                //}

                // Allow CUFF change in Draft; also make sure we don't force approval later
                newTransaction.InitialStatus = null;
                return;
            }
            // --- end targeted exception ---
            ///TESTING END


            // If the transaction was approved already and the amount changes, we need to verify the funds only on the difference.
            var cuffChanged = Different(newTransaction.CUFFaccount, oldTransaction.CUFFaccount);
            var amountToVerify = combinedTransaction.Amount;
            var statusChanged = (!oldTransaction.IsApproved && (newTransaction.IsApproved && oldTransaction.InitialStatus != "Approved"));
            if (!statusChanged)
            {
                if (cuffChanged)
                {
                    // CUFF changed → validate full amount against new rollup basis
                    amountToVerify = combinedTransaction.Amount;
                }
                else
                {
                    if (amountToVerify != oldTransaction.Amount)
                    {
                        amountToVerify -= oldTransaction.Amount;
                    }
                    else
                    {
                        amountToVerify = 0;
                    }
                }
            }

            /*if (!statusChanged)
			{
				if (amountToVerify != oldTransaction.Amount)
				{
					amountToVerify -= oldTransaction.Amount;
				}
				else
				{
					amountToVerify = 0;
				}
			}*/

            combinedTransaction.Amount = amountToVerify;

            // Determine G2 and D9 BEFORE validation — single source of truth for the entire method
            var obligationTypeName = GetObligationTypeName(combinedTransaction.ObligationType ?? oldTransaction.ObligationType);
            tracer.Trace($"Resolved obligationTypeName = \"{obligationTypeName}\", combined ref = {combinedTransaction.ObligationType?.Id}, old ref = {oldTransaction.ObligationType?.Id}");
            var isG2 = string.Equals(obligationTypeName, "Quota Sheet - G2", StringComparison.OrdinalIgnoreCase);

            var error = ValidateTransaction(combinedTransaction, true);

            // G2 transactions are managed by a flow and must always remain Pending.
            // They are never auto-approved regardless of funds or user role.
            //if (isG2)
            //{
            //    if (!string.IsNullOrEmpty(error))
            //    {
            //        throw new InvalidPluginExecutionException(error);
            //    }
            //    newTransaction.Status = "Pending";
            //    newTransaction.InitialStatus = "Pending";
            //    tracer.Trace("G2 transaction: status locked to Pending. Exiting without auto-approve.");
            //    return;
            //}

            /*combinedTransaction.Amount = amountToVerify;

			var error = ValidateTransaction(combinedTransaction, true);
            if (isG2 && !string.IsNullOrEmpty(error))
            {
                throw new InvalidPluginExecutionException(error);
            }

            var obligationTypeName = GetObligationTypeName(oldTransaction.ObligationType);
            var isG2 = string.Equals(obligationTypeName, "Quota Sheet - G2", StringComparison.OrdinalIgnoreCase);



            // if this is G2 and user edited allowed fields, keep it Pending (never auto-approve)
            if (isG2)
            {
                newTransaction.Status = "Pending";
                newTransaction.InitialStatus = "Pending";
            }*/


            // Verify that we can continue with the validation
            if ((string.IsNullOrEmpty(combinedTransaction.InitialStatus) || combinedTransaction.InitialStatus == "Draft")
             && (combinedTransaction.IsDraft))
            {
                // No point to continue. Clear the Initial Status set to Draft
                combinedTransaction.InitialStatus = null;
                return;
            }

            /*
			// For Commitments or Obligations, do not allow the transaction change if the 
			// parent document is approved and the transaction change cannot be approved 
			if ((combinedRecord.IsCommitment || combinedRecord.IsObligation)
			  && combinedRecord.ParentDocument != null && !combinedRecord.IsApproved)
			{
				var parentDocument = dbService.GFMSdocument.GetRecordFromId(combinedRecord.ParentDocument.Id);
				if (parentDocument != null && parentDocument.IsApproved && combinedRecord.InitialStatus != "Approved")
				{
					throw new InvalidPluginExecutionException("The change cannot be approved while the Parent Document is Approved. Change will not be allowed.");
				}
			}
			*/

            ///Revert back if new changes don't work
            /* Do not allow the Fiscal Strip keys to change once the transaction is Pending or Approved 
			if ((newTransaction.Appropriation != null && newTransaction.Appropriation.Id != oldTransaction.Appropriation.Id)
			 || (newTransaction.CUFFaccount != null && newTransaction.CUFFaccount.Id != oldTransaction.CUFFaccount.Id)
			 || (newTransaction.FromCUFFaccount != null && newTransaction.FromCUFFaccount.Id != oldTransaction.FromCUFFaccount.Id)
			 || (newTransaction.FundingType != null && newTransaction.FundingType.Id != oldTransaction.FundingType.Id)
			 || (newTransaction.FiscalYear != null && newTransaction.FiscalYear != oldTransaction.FiscalYear)
			 || (!string.IsNullOrEmpty(newTransaction.Type) && newTransaction.Type != oldTransaction.Type))
			{
				var msg = "Changes to the Appropriation, CUFF Account, Funding Type, Transaction Type, or Fiscal Year, are not allowed.";
				throw new InvalidPluginExecutionException(msg);
			}*/

            // Sync the Status and BPF on Status change, if any
            if (!string.IsNullOrEmpty(combinedTransaction.InitialStatus) && newTransaction.Status != combinedTransaction.InitialStatus)
            {
                var transactionWorkflow = new BusinessProcessFlow(dbService, tracer);

                Thread.Sleep(5000); // Add a 5 second delay to wait for the BPF to refresh
                var workflowRecord = dbService.TransactionWorkflow.GetRecordForEntityId(combinedTransaction.Id);

                if (workflowRecord != null)
                {
                    transactionWorkflow.SetTransactionStage(combinedTransaction, null, workflowRecord, false);
                    // Throw an exception so the user can see why the transaction Status cannot be changed (Approved more likely)
                    var throwException = false;
                    if (statusChanged && newTransaction.Status == "Approved" && !string.IsNullOrEmpty(error))
                    {
                        throwException = true;
                    }

                    newTransaction.Status = combinedTransaction.InitialStatus;
                    newTransaction.InitialStatus = null;

                    // Do a patch and then throw the exception
                    if (throwException)
                    {
                        // Create the necessary entity record for a Patch Update
                        var transactionEntity = new Entity(dbService.Transaction.GetLogicalName("Entity"), combinedTransaction.Id);
                        transactionEntity[dbService.Transaction.GetLogicalName("InitialStatus")] = null;
                        transactionEntity[dbService.Transaction.GetLogicalName("Status")] =
                                                  (newTransaction.StatusValue != null ? new OptionSetValue(newTransaction.StatusValue.GetValueOrDefault()) : null);

                        dbService.OrgServiceContext.Detach(newTransaction.Entity);

                        tracer.Trace("Transaction record has been detached.");

                        var updResponse = (UpdateResponse)dbService.Patch(transactionEntity);

                        if (updResponse.Results.Count > 0)
                        {
                            tracer.Trace($"Response from Update call is : {updResponse.Results.ToString()}");
                        }
                        else
                        {
                            tracer.Trace($"Response from Update call has no results.");
                        }

                        throw new InvalidPluginExecutionException(error);
                    }
                }
                else
                {
                    tracer.Trace("else: newtransaction.initialstatus = combined transaction.initialstatus");
                    newTransaction.InitialStatus = combinedTransaction.InitialStatus;
                }
            }
            else
            {
                newTransaction.InitialStatus = null; // Clear once all validation is complete
            }

            /* revert back if new changes don't work
            newTransaction.Status = "Approved";
            newTransaction.InitialStatus = "Approved";
            */
            //TESTING START
            /* Auto-approve all non-G2 transactions 
            var userWantsDraft = string.Equals(combinedTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(newTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase);

            if (!userWantsDraft && (statusChanged
                || string.Equals(newTransaction.Status, "Approved", StringComparison.OrdinalIgnoreCase)
                || string.Equals(oldTransaction.Status, "Approved", StringComparison.OrdinalIgnoreCase)))
            {
                newTransaction.Status = "Approved";
                newTransaction.InitialStatus = "Approved";
            }*/
            var userWantsDraft = string.Equals(combinedTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase)
    || string.Equals(newTransaction.Status, "Draft", StringComparison.OrdinalIgnoreCase);
            if (!userWantsDraft && !isG2)
            {
                newTransaction.Status = "Approved";
                newTransaction.InitialStatus = "Approved";
            }

            //TESTING END

            tracer.Trace($"Final transaction Status = {newTransaction.Status}, Initial Status set to \"{newTransaction.InitialStatus}\".");

        }
        private string GetObligationTypeName(EntityReference obligationTypeRef)
        {
            if (obligationTypeRef == null) return null;

            // If name isn't populated, load it
            if (string.IsNullOrEmpty(obligationTypeRef.Name))
            {
                var ext = dbService.ObligationType.GetExtendedEntityReference(obligationTypeRef);
                if (ext != null && !string.IsNullOrEmpty(ext.Name))
                    return ext.Name;
            }

            return obligationTypeRef.Name;
        }

        //TESTING START
        private void EnforceEditRules_PendingG2_ApprovedNonG2(
            ICARTtransactionRecord newTransaction,
            ICARTtransactionRecord oldTransaction,
            ICARTtransactionRecord combined)
        {
            if (oldTransaction == null || combined == null) return;
            if (!oldTransaction.IsObligation) return;

            bool amountChanged = combined.Amount != oldTransaction.Amount;
            bool approverChanged = Different(combined.Approver, oldTransaction.Approver);
            bool cuffChanged = Different(combined.CUFFaccount, oldTransaction.CUFFaccount);

            if (!amountChanged && !approverChanged && !cuffChanged) return;

            var obligationTypeName = GetObligationTypeName(oldTransaction.ObligationType);
            var isG2 = string.Equals(obligationTypeName, "Quota Sheet - G2", StringComparison.OrdinalIgnoreCase);

            // Pending G2 → Amount + Approver only
            if (isG2 && string.Equals(oldTransaction.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                if (cuffChanged)
                {
                    throw new InvalidPluginExecutionException(
                        "Pending Quota Sheet - G2: only Amount and Approver can be edited."
                    );
                }
                return;
            }

            // Approved non-G2 → CUFF only
            /*
            if (!isG2 && oldTransaction.IsApproved)
            {
                if (amountChanged || approverChanged)
                {
                    throw new InvalidPluginExecutionException(
                        $"Approved '{obligationTypeName}': only the CUFF Account can be edited."
                    );
                }
                return;
            }
            */
        }

        private static bool Different(EntityReference a, EntityReference b)
        {
            return (a?.Id ?? Guid.Empty) != (b?.Id ?? Guid.Empty);
        }
        //TESTING END
        #endregion

    }
}
