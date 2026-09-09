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
using Plugins_CommonLibrary.Services;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using DocumentFormat.OpenXml.Presentation;
using System.Data;
using System.Web.UI.WebControls.WebParts;
using EXConnect_Plugins.Common;
using static EXConnect_Plugins.Common.BulkUpload;
using Plugins_CommonLibrary.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;

namespace EXConnect_Plugins
{
	[CrmPluginRegistration("ProcessExcelQuotaSheetCodeActivity",
	"Process a Quota Sheet Excel Template",
	"Process a given Quota Sheet and loads into the corresponding tables",
	"EXConnect_CodeActivites",
	IsolationModeEnum.Sandbox)]

	// This Code/Workflow Activity will process a Quota Sheet file
	// and load the data into the proper tables

	public class ProcessExcelQuotaSheetCodeActivity : MainCodeActivity
	{
		[Input("Excel Content")]
		[RequiredArgument]
		public InArgument<string> ExcelContent { get; set; }

		[Input("Executing UserId")]
		public InArgument<string> ExecutingUserId { get; set; }

		[Output("Fiscal Year")]
		public OutArgument<int> FiscalYear { get; set; }

		[Output("Region")]
		public OutArgument<string> Region { get; set; }

		[Output("Processed Name")]
		public OutArgument<string> ProcessedFileName { get; set; }

		[Output("Success Flag")]
		public OutArgument<bool> SuccessFlag { get; set; }

#if DESKTOP

		public ProcessExcelQuotaSheetCodeActivity( IRepository dbService,
														ITracingService tracer = null )
		{
			if (tracer != null)
			{
				this.tracer = tracer;
				this.dbService = dbService;
			}
		}

#endif

		protected override void Execute( CodeActivityContext context )
		{
			// Change the executing User if one given
			Guid? userId = null;

			var execUserId = ExecutingUserId.Get(context);
			if (execUserId == null)
			{
				userId = Guid.Parse(execUserId);				
			}

			InitializeContext(context, userId);

			tracer.Trace("Processing the Code Activity");

			var excelContent = ExcelContent.Get(context);

			if (string.IsNullOrEmpty(excelContent))
			{
				var errMsg = "No Excel file content given. Process will terminate.";
				tracer.Trace(errMsg);
				SuccessFlag.Set(context, false);
				return;
			}

            // Get the BulkUpload template record from the target entity

            tracer.Trace("ProcessExcelQuoteSheetCodeActivity: Before ParseQuotaSheetFileContent");
            var results = ParseQuotaSheetFileContent(excelContent);

			if (results != null)
			{
                tracer.Trace("ProcessExcelQuoteSheetCodeActivity: Phase One");
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
						if (grantTypes.Length > 3)
						{
							grantTypeLabel = "Scholar";
						}
						else
						{
							grantTypeLabel = $"{grantTypes[0]} & {grantTypes[1]}";
						}
					}
					else
					{
						grantTypeLabel = results.GranteeTypes;
					}
				}

                tracer.Trace("ProcessExcelQuoteSheetCodeActivity: Phase Two");
                var version = (results.VersionNumber != null ? $" ({results.VersionNumber.Value})" : string.Empty);

				var name = $"{(results.Region == QuotaSheetContants.FrontOfficeRegion ? "Front Office" : results.Region == "EUR" ? "EUR Europe & Eurasia" : results.Region)} "
						+ $"FY {results.FiscalYear} {grantTypeLabel} ({results.RevisionPhase}){version}";

				Region.Set(context, results.Region);
				FiscalYear.Set(context, results.FiscalYear);
				ProcessedFileName.Set(context, name);
                tracer.Trace("ProcessExcelQuoteSheetCodeActivity: Phase Three");


            }

            SuccessFlag.Set(context, results != null);

			tracer.Trace($"The transaction validation success is set to {results != null}.");

		}

		/// <summary>
		/// Function to allow the testing from the desktop
		/// </summary>
		/// <param name="templateType"></param>
		public QuotaSheetParseResults ParseQuotaSheetFileContent( string fileContent )
		{
			var fileBytes = Convert.FromBase64String(fileContent);

			var quotaSheetParser = new QuotaSheetsParsing(dbService, tracer);

			return quotaSheetParser.ProcessExcelQuotaSheet(fileBytes);
		}

	}
}
