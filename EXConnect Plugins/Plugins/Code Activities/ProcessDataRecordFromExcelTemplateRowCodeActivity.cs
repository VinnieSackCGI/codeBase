using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.ServiceModel;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;

using Newtonsoft.Json.Linq;

using Microsoft.Xrm.Sdk.Query;
using System;

using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Plugin_Handling;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using DocumentFormat.OpenXml.Presentation;
using System.Data;
using System.Web.UI.WebControls.WebParts;
using System.Runtime.ConstrainedExecution;

namespace EXConnect_Plugins
{
	[CrmPluginRegistration("ProcessDataRecordFromExcelTemplateRowCodeActivity",
    "Process Data Record From Excel Template Row",
    "Process a Record loaded from Excel table row containing de-referenced data",
    "EXConnect_CodeActivites",
    IsolationModeEnum.Sandbox)]

    // This Code/Workflow Activity will convert the incoming EXcel row into
	// a Transaction or Spend Plan record based on the Template Type.
	// The record will be validated against the expecte data for the
	// Template Type and, if valid, the record will be posted into the corresponging
	// data table (Transactions or Spend Plan Requests). If errors are found,
	// the record will be posted to the corresponding "Staged Bulk Upload" with any
	// Validation Errors found.

    public class ProcessDataRecordFromExcelTemplateRowCodeActivity : MainCodeActivity
    {
        [Input("Template Type")]
        [RequiredArgument]
        public InArgument<string> TemplateType { get; set; }
        
        [Input("Record Data Object")]
        [RequiredArgument]
        public InArgument<string> JsonDataRecord { get; set; }

        [Input("Executing User Id")]
        public InArgument<string> ExecutingUserId { get; set; }

        [Output("Success Flag")]
        public OutArgument<bool> SuccessFlag { get; set; }

#if DESKTOP

        public ProcessDataRecordFromExcelTemplateRowCodeActivity(Guid userId, IRepository dbService, 
                                                        ITracingService tracer = null)
        {
            if (tracer != null)
            {
                this.tracer = tracer;
                this.dbService = dbService;
            }
            this.executingUserId = userId;
        }

#endif

        protected override void Execute(CodeActivityContext context)
        {
            var execUserId = ExecutingUserId.Get(context);

            Guid? userId = null;
            if (!string.IsNullOrEmpty(execUserId))
            {
                userId = Guid.Parse(execUserId);
            }

            InitializeContext(context, userId);

            tracer.Trace("Processing the Code Activity");

            var templateType = TemplateType.Get(context);
            var recordData = JsonDataRecord.Get(context);

            if (string.IsNullOrEmpty(templateType) || string.IsNullOrEmpty(recordData))
            {
                var errMsg = "No Template Type or Record Data given. Process will terminate.";
                tracer.Trace(errMsg);
                SuccessFlag.Set(context, false);
                return;
            }

            var jsonRecordData = JObject.Parse(recordData);

			var success = ProcessDataObject(templateType, jsonRecordData);

			SuccessFlag.Set(context, success);

			tracer.Trace($"The transaction validation success is set to {success}.");
		}

		public bool ProcessDataObject( string templType, JObject recordData )
		{
			// Load and Validate the Data Record received

			bool isTransaction = true;
			var templateType = string.Empty;
			if (templType.StartsWith("Appropriation"))
			{
				templateType = "AppropriationAllocation";
			}
			else if (templType.StartsWith("AOA"))
			{
				templateType = "AOA";
			}
			else if (templType.StartsWith("Spend Plan"))
			{
				templateType = "SpendPlanRequest";
				isTransaction = false;
			}
			else
			{
				tracer.Trace($"The given transaction type \"{templType}\" cannot be processed.");
				return false;
			}

			// Extract the expected fields from the JSON record

			var fundSrcName = (recordData.TryGetValue("Funding Source", out JToken jsonFundingSource) ? jsonFundingSource.ToString() : string.Empty);
			var fundTypName = (recordData.TryGetValue("Funding Type", out JToken jsonFundingType) ? jsonFundingType.ToString() : string.Empty);
			var approprCode = (recordData.TryGetValue("Appropriation", out JToken jsonAppropriation) ? jsonAppropriation.ToString() : string.Empty);
			var account = (recordData.TryGetValue("Account", out JToken jsonAccount) ? jsonAccount.ToString() : string.Empty);
			var amount = (recordData.TryGetValue("Amount", out JToken jsonAmount) ? jsonAmount.ToString() : string.Empty);
			var description = (recordData.TryGetValue("Description", out JToken jsonDescription) ? jsonDescription.ToString() : string.Empty);
			var ibisCode = (recordData.TryGetValue("IBIS Request Code", out JToken jsonRequestCode) ? jsonRequestCode.ToString() : string.Empty);
			var projectCode = (recordData.TryGetValue("Project Code", out JToken jsonProjectCode) ? jsonProjectCode.ToString() : string.Empty);
			var requestName = (recordData.TryGetValue("Request Name", out JToken jsonRequestName) ? jsonRequestName.ToString() : string.Empty);
			var priority = (recordData.TryGetValue("Priority", out JToken jsonPriority) ? jsonPriority.ToString() : string.Empty);
			var justification = (recordData.TryGetValue("Justification", out JToken jsonJustification) ? jsonJustification.ToString() : string.Empty);
			var functionCode = (recordData.TryGetValue("Function Code", out JToken jsonFunctionCode) ? jsonFunctionCode.ToString() : string.Empty);

			// Initialize the error string
			var error = string.Empty;

			// Initialize transaction and spendRequest records for later processing
			ICARTtransactionRecord transactionRecord = null;
			ISpendPlanRequestRecord spendPlanRequest = null;

			// Process the record found
			if ( isTransaction )
			{
				transactionRecord = dbService.Transaction.CreateRecord();

				transactionRecord.Type = templateType;

				transactionRecord.CUFFaccount = dbService.CUFFaccount.GetEntityReferenceFromName(account);
				transactionRecord.Description = description;
				
				// Compare the Funding Source, Type, and Appropriation against the Funding Source table settings
				// to get the proper reference
				var fundingSrcList = dbService.FundingSource.GetAppropriationFundingDataList();
				var fundRec = fundingSrcList.Where(f => f.FundingSrcName == fundSrcName
												&& f.Appropriation == approprCode
												&& f.FundingType == fundTypName)
											.FirstOrDefault();
				if (fundRec == null)
				{
					transactionRecord.FundingSource = dbService.FundingSource.GetEntityReferenceFromName(fundSrcName);
					transactionRecord.FundingType = dbService.FundingType.GetEntityReferenceFromName(fundTypName);
					transactionRecord.Appropriation = dbService.Appropriation.GetEntityReferenceFromCode(approprCode);
				}
				else
				{
					transactionRecord.FundingSource = dbService.FundingSource.GetEntityReferenceFromId(fundRec.FundingSrcId);
					transactionRecord.FundingType = dbService.FundingType.GetEntityReferenceFromId(fundRec.FundingTypeId);
					transactionRecord.Appropriation = dbService.Appropriation.GetEntityReferenceFromId(fundRec.AppropriationId);
				}

				// Add additional fields for other transaction types
				if (transactionRecord.IsAOA)
				{
					transactionRecord.IBIS_RequestCode = ibisCode;
					transactionRecord.ProjectCode = projectCode;
				}

				error = Common.TransactionValidator.TransactionKeyValidation(transactionRecord, dbService, tracer);

				if (transactionRecord.CUFFaccount == null)
				{
					if (!string.IsNullOrEmpty(account))
					{
						error += "\nA CUFF Account error could be an inconsistency in the Template's Accounts table. "
								+ $"Download the latest Bulk Upload Template for \"{transactionRecord.Type}\" transactions.";
					}
				}
				else if (transactionRecord.IsAOA)
				{
					// Make sure the given CUFF Account is AOA eligible
					var cuffAccount = dbService.CUFFaccount.GetRecordFromId(transactionRecord.CUFFaccount.Id);
					if (!cuffAccount.IsAOAeligible)
					{
						if (!string.IsNullOrEmpty(error))
						{
							error += "\n";
						}
						error += $"The given CUFF Account \"{cuffAccount.Name}\" is not AOA Eligible.";
					}
				}

			}
			else
			{
				// Process the Spend Plan Request record
				spendPlanRequest = dbService.SpendPlanRequest.CreateRecord();

				spendPlanRequest.Name = requestName;
				spendPlanRequest.Appropriation = dbService.Appropriation.GetEntityReferenceFromCode(approprCode);
				spendPlanRequest.FundingType = dbService.FundingType.GetEntityReferenceFromName(fundTypName);
				spendPlanRequest.CUFFaccount = dbService.CUFFaccount.GetEntityReferenceFromName(account);
				spendPlanRequest.Justification = justification;
				spendPlanRequest.Priority = priority;
				spendPlanRequest.FunctionCode = null;  // dbService.FunctionCode.GetEntityReferenceFromCode(functionCode);

				// Add additional fields for other transaction types
				error = Common.SpendPlanValidator.KeyValidation(spendPlanRequest, dbService, tracer);

				if (spendPlanRequest.CUFFaccount == null)
				{
					if (!string.IsNullOrEmpty(account))
					{
						error += "\nA CUFF Account error could be an inconsistency in the Template's Accounts table. "
								+ $"Download the latest Bulk Upload Template for \"{templType}\".";
					}
				}
			}

			// Check for final errors common to all templates
			decimal? recordAmount = null;
			if (!string.IsNullOrEmpty(amount))
			{
				recordAmount = Convert.ToDecimal(amount);
			}

			if (recordAmount == null)
			{
				error += "\nThe Amount field needs to be supplied.";
			}
			else
			{
				if (isTransaction)
				{
					transactionRecord.Amount = recordAmount;
				}
				else
				{
					spendPlanRequest.Amount = recordAmount;
				}
			}

			if (!string.IsNullOrEmpty(error))
			{
				// Prepare the proper record based on the template Type
				IStagedBulkUploadRecord stagedRecord = null;
				if ( isTransaction)
				{
					stagedRecord = dbService.StagedBulkUploadRecord.ComposeStagedRecordFromTransaction(transactionRecord);
				}
				else
				{
					stagedRecord = dbService.StagedBulkUploadRecord.ComposeStagedRecordFromSpendPlanRequest(spendPlanRequest);
				}

				stagedRecord.ValidationErrors = error;

				dbService.Create(stagedRecord.Entity, true);

				return false;
			}

			// When no errors, post the record to the proper table
			if (isTransaction)
			{
				dbService.Create(transactionRecord.Entity, true);
			}
			else
			{
				dbService.Create(spendPlanRequest.Entity, true);
			}

			return true;
        }

    }

}
