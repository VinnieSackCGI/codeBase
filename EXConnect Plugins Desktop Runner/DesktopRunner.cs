
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.IO.Compression;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

using EXConnect_Plugins;
using EXConnect_Plugins.Plugin_Handling.Services;
using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Plugins;
using EXConnect_Plugins.Entities.Interfaces;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using PluginsRunner_CommonLibrary;
using Plugins_CommonLibrary.Extensions;

using System.Data.OleDb;
using System.Web.Util;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using Plugins_CommonLibrary.Interfaces;
using EXConnect_Plugins.Common;

using Plugins_CommonLibrary.Services;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;
using static EXConnect_Plugins.Common.BulkUpload;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.Data.Common;

namespace DesktopInterface
{
	// The program starting parameters are controlled by arguments in the Debug section of the project.
	// To modify, open the project's Properties page and under the Debug tab, specify the desired parameters.
	// Speficy the parameters as a space separated string starting with the environment (SIO, DEV, UAT),
	// followed by the debug option (1 for Plugin debug and 2 for Plugin logic debug).

	// NOTES:
	// =========================================================================================================
	// 1 - When testing the plugins, check the  == BEFORE TESTING BEGINS == blocks below for setup instructions.
	// 2 - When testing Plugin logic, the project must be compiled for Desktop since there are compiler
	// directives that strip out the plugin execution and allow to call the plugin to test just the logic.
	// =========================================================================================================

	class DesktopRunnerHelper
	{

		private const int DEBUG_PLUGIN = 1;
		private const int DEBUG_PLUGIN_LOGIC = 2;

		public static string userName = "";

		static void Main( string[] args )
		{
			var ENV = args[0];
			var processToRun = int.Parse(args[1]);

			// Reset the userid based on the environment (SIO or other)
			var statePrefix = "saifaa2"; 
			var envURL = string.Empty;

			var conn = Runner.GetConnectionString(statePrefix, ENV, out envURL);

			var tracer = new TracerService();

			using (var svc = new CrmServiceClient(conn))
			{
				if (!svc.IsReady)
				{
					Console.WriteLine($"ERROR: Connection to environment \"{envURL}\" could not be established.");
					return;
				}

				userName = svc.OAuthUserId;

				if (processToRun == DEBUG_PLUGIN)
				{
					string jsonFileText = string.Empty;
					jsonFileText = Runner.GetJsonTextFromCompressedFile();
					JObject objectsFromJson = JsonConvert.DeserializeObject<JObject>(jsonFileText);

					// ============================================================================
					// ** BEFORE TESTING BEGINS ***
					// ============================================================================

					// 1 - Modify the typeof plugin class to test
					Type typeOfPluginToRun = typeof(SpendingTransactionValidationPlugin);
					// 2 - Define the entity class that the plugin triggers on
					// ============================================================================
					Runner.ExecutePlugin<rcade_CARTTransaction>(typeOfPluginToRun, objectsFromJson, svc, tracer);
					// ============================================================================
				}
				else if (processToRun == DEBUG_PLUGIN_LOGIC)
				{
					// Get the EXConnect service context
					var dbService = new BusinessDbService(svc, tracer);
					// Set my ID as the user
					var request = new WhoAmIRequest();
					var response = (WhoAmIResponse)svc.Execute(request);

					dbService.UserId = response.UserId;
					dbService.LocalDateTime = DateTime.Now;

#if DESKTOP
					#region Methods

					//LoadTransaction(dbService, tracer);
					//ReCalculateRollupFields(dbService, tracer);
					//TestAssociateAppropriationToAccounts(dbService, tracer);
					//TestAssociateAccountToAppropriations(dbService, tracer);
					//TestTransactionValidation(dbService, tracer);
					//TestTransactionMonthlyRollup(dbService, tracer);
					//TestTransactionYearlyRollup(dbService, tracer);
					//TestTransactionWorkflowSync(dbService, tracer);
					//TestTransactionStaqeSync(dbService, tracer);
					//TestObligationValidation(dbService, tracer);
					//TestCommitmentValidation(dbService, tracer);
					//TestDocumentPackageStatusSync(dbService, tracer);
					//TestDocumentNumberForCreditCardObligation(dbService, tracer);
					//TestDocumentNumberForInternationalTravelObligation(dbService, tracer);
					//TestStagedBulkUploadRecordValidation(dbService, tracer);
					//TestStagedRecordToTransactionWhenValid(dbService, tracer);
					//TestFundingSourceAppropriationChange(dbService, tracer);
					//TestConvertChoiceToString(dbService, tracer);
					//TestQuotaSheetParsing(dbService, tracer);

					#endregion

					#region Code Activities

					//TestTemplateDataCreateFromJsonRecord(dbService, tracer, true);
					//TestUpdateTemplateRefTables(dbService, tracer, true);
					TestExcelQuotaSheetParsing(dbService, tracer, true);

					#endregion

#endif
				}

				tracer.Trace("\nProcess Completed...\n");
				tracer.Trace("\f");

				Console.WriteLine("Press any key to end the session..");
				Console.ReadKey();
			}

		}

#if DESKTOP

		private static void TestConvertChoiceToString(IRepository dbService, ITracingService tracer )
		{
			var choice = "784510002";
			var strType = "AOA";
			
			eca_ECATransactionTypes transType;
			Enum.TryParse<eca_ECATransactionTypes>(choice, out transType);

			eca_ECATransactionTypes transType2;
			Enum.TryParse<eca_ECATransactionTypes>(strType, out transType2);
		}


		private static void ReCalculateRollupFields(IRepository dbService, ITracingService tracer)
		{
			// Set the target references for the Neighborhood, Division, and Program tables for a specific account name
			var accountId = Guid.Parse("f19c3e27-093c-f011-8c4e-001dd80b1717");

			var targetRefs = GetParentAccountReferences(accountId, dbService, tracer);

			var fieldNames = new List<string> { "eca_rollupapprovedallocatedamount","eca_rollupapprovedcommittedamount" };

			// Contruct the mutliple request for every target and every field
			// Create an ExecuteMutipleRequest object

			var multipleRequest = dbService.GetExecuteMultipleRequest();

			foreach (var target in targetRefs)
			{
				foreach (var fieldName in fieldNames)
				{
					var recalcRequest = new CalculateRollupFieldRequest
					{
						Target = target,
						FieldName = fieldName
					};
					multipleRequest.Requests.Add(recalcRequest);
				}
			}

			// Execute the request
			var response = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
		}

		private static EntityReferenceCollection GetParentAccountReferences(Guid accountid, IRepository dbService, ITracingService tracer )
		{
			// Retrieve the list of 
			var parentRefs = new EntityReferenceCollection();

			var fetchXml = $@"
<fetch>
  <entity name='eca_accountrollup'>
    <attribute name='eca_accountrollupid' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='eca_accountrollupid' operator='eq-or-above' value='{accountid}' />
    </filter>
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					parentRefs.Add(entity.ToEntityReference());
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of CUFF Accounts records list.");
			}

			return parentRefs;
		}

		/// <summary>
		/// Test the association of a new appropriation to CUFF accounts
		/// </summary>
		/// <param name="dbService"></param>
		/// <param name="tracer"></param>
		private static void TestAssociateAppropriationToAccounts(IRepository dbService, ITracingService tracer)
        {
            var appropriationRef = new EntityReference("rcade_appropriation", Guid.Parse("3abd6063-ba1f-f011-998a-001dd80bcf22"));

            // instantiate the AppropriattionToAccounts Plugin
            var appropToAccountPlugin = new AppropriationAccountAssociationPlugin(dbService, tracer);

            appropToAccountPlugin.ExecutePlugin(appropriationRef);

        }

        /// <summary>
        /// Test the association of a new CUFF account to all Appropriations
        /// </summary>
        /// <param name="dbService"></param>
        /// <param name="tracer"></param>
        private static void TestAssociateAccountToAppropriations(IRepository dbService, ITracingService tracer)
        {
            var accountRef = new EntityReference("rcade_cuffaccount", Guid.Parse("cd6cce98-6e20-f011-998b-001dd80822c1"));

            // instantiate the AppropriattionToAccounts Plugin
            var accountToAppropriationPlugin = new AccountAppropriationAssociationPlugin(dbService, tracer);

            accountToAppropriationPlugin.ExecutePlugin(accountRef);

        }

		private static void TestTransactionValidation(IRepository dbService, ITracingService tracer )
		{
			var transaction = dbService.Transaction.GetRecordFromId(Guid.Parse("1ff29573-3a21-f011-998b-001dd803b4d1"));

			// instantiate the Validation Plugin
			var validationplugin = new SpendingTransactionValidationPlugin(dbService, tracer);

			validationplugin.ExecutePlugin("Create", transaction);
		}

		private static void TestTransactionMonthlyRollup(IRepository dbService, ITracingService tracer)
		{
			var oldTransaction = dbService.Transaction.CreateRecord();
			oldTransaction.FiscalYear = DateTime.Now.FiscalYear();
			oldTransaction.Type = "AppropriationAllocation";
			oldTransaction.FundingType = dbService.FundingType.GetEntityReferenceFromName("Gift Funds");
			oldTransaction.Status = "Approved";
			oldTransaction.Appropriation = dbService.Appropriation.GetAllRecords().Where(a => a.Code == "19___X02090000").FirstOrDefault().Entity.ToEntityReference();
			oldTransaction.CUFFaccount = dbService.CUFFaccount.GetAllRecords().Where( c => c.Name == "4. AP - Regional Programs").FirstOrDefault().Entity.ToEntityReference();
			oldTransaction.Amount = 100000;
			oldTransaction.CreatedOn = DateTime.Now;
			oldTransaction.Id = Guid.NewGuid();

			// instantiate the Validation Plugin
			var monthlyRollupPlugin = new MonthlyTransactionRollupPlugin( dbService, tracer );

			//			monthlyRollupPlugin.ExecutePlugin("Create", newTransaction);

			var newTransaction = dbService.Transaction.CreateRecord();
			newTransaction.Id = oldTransaction.Id;
			newTransaction.Amount = 50000;
			newTransaction.ModifiedOn = DateTime.Now;

			monthlyRollupPlugin.ExecutePlugin("Update", newTransaction, oldTransaction);

		}

		private static void TestTransactionYearlyRollup( IRepository dbService, ITracingService tracer )
		{
			var oldMonthlyRollup = dbService.MonthlyRollup.GetRecordFromId(Guid.Parse("a9f76644-1b43-f011-877a-001dd803dca7"));
			oldMonthlyRollup.ApprovedFinPlan = 100000;
			oldMonthlyRollup.PendingAOAamount = 250000;

			// instantiate the Validation Plugin
			var yearlyRollupPlugin = new YearlyTransactionRollupPlugin(dbService, tracer);

			// yearlyRollupPlugin.ExecutePlugin("Create", oldMonthlyRollup);

			var newMonthlyRollup = dbService.MonthlyRollup.CreateRecord();
			newMonthlyRollup.Id = oldMonthlyRollup.Id;

			newMonthlyRollup.ApprovedFinPlan = 110000;
			newMonthlyRollup.PendingAOAamount = 240000;

			newMonthlyRollup.ModifiedOn = DateTime.Now;

			yearlyRollupPlugin.ExecutePlugin("Update", newMonthlyRollup, oldMonthlyRollup);
		}

		private static void TestTransactionWorkflowSync( IRepository dbService, ITracingService tracer )
		{
			//var transaction = dbService.Transaction.GetRecordFromId(Guid.Parse("f08c7404-2f5c-f011-bec2-001dd803dca7"));
			//transaction.InitialStatus = "Approved";
			
			//var syncPlugin = new TransactionWorkflowSyncPlugin(dbService, tracer);
			
			//syncPlugin.ExecutePlugin(transaction, null);
		}

		private static void TestTransactionStaqeSync( IRepository dbService, ITracingService tracer )
		{
			var workflow = dbService.TransactionWorkflow.GetRecordForEntityId(Guid.Parse("4aaee7c6-4358-f011-877b-001dd803dca7"));

			var syncPlugin = new TransactionStageSyncPlugin(dbService, tracer);

			syncPlugin.ExecutePlugin(workflow as ITransactionWorkflowRecord);
		}

		private static void TestObligationValidation( IRepository dbService, ITracingService tracer )
		{
			var oldObligation = dbService.Transaction.GetRecordFromId(Guid.Parse("2b91dbf1-4d5c-f011-bec2-001dd803dca7"));
			var newObligation = dbService.Transaction.CreateRecord();
			newObligation.Id = oldObligation.Id;
			newObligation.Status = null;

			var validationPlugin = new SpendingTransactionValidationPlugin(dbService, tracer);

			validationPlugin.ExecutePlugin("Update", newObligation, oldObligation);
		}
		private static void TestCommitmentValidation( IRepository dbService, ITracingService tracer )
		{
			// Commitment with Appropriation = 19___501130007
			var newCommitment = dbService.Transaction.GetRecordFromId(Guid.Parse("d63cbd64-df7d-f011-b4cc-001dd803dca7"));
			newCommitment.Code = "GIQ";

			var validationPlugin = new SpendingTransactionValidationPlugin(dbService, tracer);

			validationPlugin.ExecutePlugin("Create", newCommitment, null);
		}

		private static void LoadTransaction( IRepository dbService, ITracingService tracer )
		{
			// Load a Transaction manually
			var transaction = dbService.Transaction.CreateRecord();
			transaction.Appropriation = dbService.Appropriation.GetAllRecords().Where(a => a.Code == "19___501130007").FirstOrDefault().Entity.ToEntityReference();
			transaction.Code = "VO-FA";
			transaction.Type = "Commitment";
			transaction.FundingType = dbService.FundingType.GetEntityReferenceFromName("Appropriation Transfer");
			transaction.CUFFaccount = dbService.CUFFaccount.GetAllRecords().Where( a => a.Name == "2. GF - Gift Funds Recoveries (Unconditional)").FirstOrDefault().Entity.ToEntityReference();
			transaction.FiscalYear = 2025;
			transaction.Amount = 678;
			transaction.DocumentNumber = "1bab874f-aae5-4d85-8f57-ff3a3142d2ab";
			transaction.BOC = new EntityReference("eca_budgetobjectcode", Guid.Parse("dcefcc60-6753-f011-877a-001dd803dca7"));          //"Domestic Base Pay";
			transaction.VendorCode = "962827031";
			transaction.Vendor = "20 / 20 TRANSLATIONS, INC.";
			transaction.FundingSource = new EntityReference("eca_fundingsource", Guid.Parse("8eeb7497-2f58-f011-877b-001dd8084aea"));   // "Assistance for Europe & Euras.";

			dbService.Create(transaction.Entity, true);
		}

		private static void TestDocumentPackageStatusSync( IRepository dbService, ITracingService tracer )
		{
			var oldDocument = dbService.GFMSdocument.GetRecordFromId(Guid.Parse("ec1158d8-4166-f011-bec2-001dd803dca7"));
			var newDocument = dbService.GFMSdocument.CreateRecord();

			newDocument.Id = oldDocument.Id;
			newDocument.NeedsValidation = !oldDocument.NeedsValidation;

			var syncPlugin = new GFMSdocumentPackageStatusSyncPlugin(dbService, tracer);

			syncPlugin.ExecutePlugin(newDocument, oldDocument);

		}

		private static void TestDocumentNumberForCreditCardObligation( IRepository dbService, ITracingService tracer )
		{
			var newObligation = dbService.Transaction.GetRecordFromId(Guid.Parse("c6613255-e872-f011-bec3-001dd803dca7"));

			var validationPlugin = new SpendingTransactionValidationPlugin(dbService, tracer);

			validationPlugin.ExecutePlugin("Create", newObligation, null);
		}

		private static void TestDocumentNumberForInternationalTravelObligation( IRepository dbService, ITracingService tracer )
		{
			var newObligation = dbService.Transaction.CreateRecord();
			newObligation.Appropriation = dbService.Appropriation.GetAllRecords()
											.Where(a => a.Code == "19___X02090000" && a.IsActive)
											.FirstOrDefault().Entity.ToEntityReference();
			newObligation.CUFFaccount = dbService.CUFFaccount.GetEntityReferenceFromId(Guid.Parse("50989def-b147-f011-877a-001dd8006bc2"));

			// Funding Source: Base Funsing for ECA - New Base Year
			newObligation.FundingSource = new EntityReference("eca_fundingsource", Guid.Parse("78761b99-2f58-f011-877b-001dd80bcf22"));
			newObligation.FundingType = dbService.FundingType.GetEntityReferenceFromName("New Base Year");
			newObligation.FiscalYear = 2025;

			// Domestic ECA Allotment
			newObligation.Allotment = new EntityReference("rcade_allotment", Guid.Parse("b847a022-3304-f011-bae2-001dd8006beb"));
			newObligation.Type = "Obligation";
			newObligation.Code = "M9";
			newObligation.ObligationType = dbService.ObligationType.GetEntityReferenceFromName("International Travel");
			newObligation.Post = dbService.Post.GetAllRecords()
										.Where(p => p.Code == "E506" && p.IsActive)
										.FirstOrDefault().Entity.ToEntityReference();

			newObligation.Amount = 100;

			var validationPlugin = new SpendingTransactionValidationPlugin(dbService, tracer);

			validationPlugin.ExecutePlugin("Create", newObligation, null);

		}

		private static void TestFundingSourceAppropriationChange(IRepository dbService, ITracingService tracer )
		{
			// instantiate the Funding Source Appropriation Plugin
			var fundingAppropPlugin = new TemplateUpdateOnFundingSourceAppropriationChangePlugin(dbService, tracer);

			fundingAppropPlugin.ExecutePlugin();
		}

		#region Parse and store Excel Quota Sheets

		private static void TestQuotaSheetParsing( IRepository dbService, ITracingService tracer )
		{
			var path = @"C:\Development\Testing\Bulk Upload Tests\";
			var fileName = @"SCA FY 2026 Visting Scholar & US Scholar (QS Amendment 4) v4.xlsx";

			var filePath = path + fileName;

			// Open the file
			var fileBytes = File.ReadAllBytes(filePath);

			var quotaSheetParsing = new EXConnect_Plugins.Common.QuotaSheetsParsing(dbService, tracer);

			tracer.Trace($"Processing file \"{filePath}\" with {fileBytes.Length} bytes.");

			var results = quotaSheetParsing.ProcessExcelQuotaSheet( fileBytes );

		}

		private static decimal GetDecimal( object value )
		{
			if (value.GetType().Name == "DBNull") return 0M;
			return Convert.ToDecimal(value);
		}

		#endregion



		#region Staged Transactions

		private static void TestStagedBulkUploadRecordValidation( IRepository dbService, ITracingService tracer )
		{
			var oldStagedRecord = dbService.StagedBulkUploadRecord.GetRecordFromId(Guid.Parse("47f65138-009f-f011-b4cc-001dd803dca7"));
			var updStagedRecord = dbService.StagedBulkUploadRecord.CreateRecord();
			updStagedRecord.Id = oldStagedRecord.Id;
			updStagedRecord.Appropriation = dbService.Appropriation.GetEntityReferenceFromCode("19___X02090000");

			var validationPlugin = new StagedBulkUploadRecordValidationPlugin(dbService, tracer);

			validationPlugin.ExecutePlugin(updStagedRecord, oldStagedRecord);

			dbService.OrgServiceContext.Detach(oldStagedRecord.Entity);

			dbService.Update(updStagedRecord.Entity);

		}

		private static void TestStagedRecordToTransactionWhenValid( IRepository dbService, ITracingService tracer )
		{
			var updStagedRecord = dbService.StagedBulkUploadRecord.CreateRecord();
			updStagedRecord.Id = Guid.Parse("47f65138-009f-f011-b4cc-001dd803dca7");
			updStagedRecord.IsValid = true;

			var movePlugin = new MoveFromStagedToCorrespondingTablePlugin(dbService, tracer);

			movePlugin.ExecutePlugin(updStagedRecord);
		}

		#endregion

		#region Code Activities

		private static void TestUpdateTemplateRefTables( IRepository dbService, ITracingService tracer, 
															bool testCodeActivity = true )
		{
			// Get the BulkUpload template record from the target entity
			var templateType = "Spend Plan Requests";

			if (testCodeActivity)
			{
				tracer.Trace($"Update the Reference Tables for {templateType} Excel Template.");

				var codeActivity = new UpdateExcelTemplateRefTablesCodeActivity( dbService, tracer);

				var success = codeActivity.UpdateExcelTemplate(templateType);

				tracer.Trace($"The Update to the template success was {success}.");
			}
			else
			{
				// Test the Dynamics Process Action
				var request = new OrganizationRequest("eca_UpdateTemplateReferenceTables");
				request["TemplateType"] = templateType;

				OrganizationResponse response = null;

				response = dbService.OrganizationService.Execute(request);

				tracer.Trace($"\nThe resulting Success Flag = {response.Results["Success"]}");
			}

		}

		private static void TestTemplateDataCreateFromJsonRecord( IRepository dbService, ITracingService tracer,
															bool testCodeActivity = true )
		{
			var templateType = "Appropriation-Allocations";
			var dataRecord = @"
{
  '@odata.etag': '',
  'ItemInternalId': '115aa6cf-23e4-492f-91fe-4ddd808b6581',
  'Appropriation & Funding Source': 'BASE : New Base Year',
  'Appropriation': '19___X02090000',
  'Funding Source': 'Base Funding for ECA - NBY',
  'Funding Type': 'New Base Year',
  'Account': '1. Academics',
  'Neighborhood': 'Academics',
  'Division': '',
  'Program': '',
  'Sub-Program': '',
  'Amount': '10',
  'Description': 'test'
}";

			var user = dbService.SystemUser.GetEntityReferenceFromEmail(userName);

			if (testCodeActivity)
			{
				var codeActivity = new ProcessDataRecordFromExcelTemplateRowCodeActivity(user.Id, dbService, tracer);

				var recordData = JObject.Parse(dataRecord);

				tracer.Trace($"Processing the data record: \n {recordData.ToString()} \n");

				var success = codeActivity.ProcessDataObject(templateType, recordData);

				tracer.Trace($"The data record success was {success}");
			}
			else
			{
				// Test the Dynamics Process Action
				var request = new OrganizationRequest("eca_ProcessRecordfromJSONData");
				request["TemplateType"] = templateType;
				request["RecordDataObject"] = dataRecord;
				request["ExecutingUserId"] = user.Id.ToString();

				OrganizationResponse response = null;

				response = dbService.OrganizationService.Execute(request);

				tracer.Trace($"\nThe resulting Success Flag = {response.Results["SuccessFlag"]}");
			}
		}

		private static void TestExcelQuotaSheetParsing( IRepository dbService, ITracingService tracer,
															bool testCodeActivity = true )
		{
			var fileContent = @"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";

			if (testCodeActivity)
			{
				var codeActivity = new ProcessExcelQuotaSheetCodeActivity(dbService, tracer);

				var results = codeActivity.ParseQuotaSheetFileContent(fileContent);

				if (results != null)
				{
					// Compose the name of the file from the incoming data
					var grantTypeLabel = string.Empty;
					if (!string.IsNullOrEmpty(results.SpecialCode))
					{
						grantTypeLabel = results.SpecialCode;
					}
					else
					{
						var grantTypes = results.GranteeTypes.Split(',');
						if (grantTypes.Length > 1)
						{
							grantTypeLabel = "Scholar";
						}
						else
						{
							grantTypeLabel = results.GranteeTypes;
						}
					}

					var name = $"{(results.Region == QuotaSheetContants.FrontOfficeRegion ? "Front Office" : results.Region == "EUR" ? "EUR Europe & Eurasia" : results.Region)} "
							+ $"FY {results.FiscalYear} {grantTypeLabel} : {results.RevisionPhase}";
				}

				tracer.Trace($"The data record success was {results != null}");
			}
			else
			{
				// Test the Dynamics Process Action
				var request = new OrganizationRequest("eca_ProcessQuotaSheetExcelFile");
				request["ExcelContent"] = fileContent;

				OrganizationResponse response = null;

				response = dbService.OrganizationService.Execute(request);

				tracer.Trace($"\nThe results follow: \n" +
					$"Fiscal Year = {response.Results["FiscalYear"]} \n" +
					$"Region = {response.Results["Region"]} \n" +
					$"Name = {response.Results["ProcessName"]} \n" +
					$"Success Flag = {response.Results["SuccessFlag"]} \n");
			}

		}

		#endregion

#endif

	}
}
