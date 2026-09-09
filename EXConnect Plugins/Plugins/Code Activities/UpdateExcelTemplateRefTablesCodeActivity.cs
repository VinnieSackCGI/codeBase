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

namespace EXConnect_Plugins
{
	[CrmPluginRegistration("UpdateExcelTemplateRefTablesCodeActivity",
	"Update the Excel Template Reference Tables",
	"Update the reference tables embedded in the EXConnect Excel Templates used in Bulk Upload",
	"EXConnect_CodeActivites",
	IsolationModeEnum.Sandbox)]

	// This Code/Workflow Activity will update the reference tables embedded in
	// the given Excel Template

	public class UpdateExcelTemplateRefTablesCodeActivity : MainCodeActivity
	{
		[Input("Template Type")]
		[RequiredArgument]
		public InArgument<string> TemplateType { get; set; }

		[Output("Success Flag")]
		public OutArgument<bool> SuccessFlag { get; set; }

#if DESKTOP

		public UpdateExcelTemplateRefTablesCodeActivity( IRepository dbService,
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

			InitializeContext(context);

			tracer.Trace("Processing the Code Activity");

			var templateType = TemplateType.Get(context);

			if (string.IsNullOrEmpty(templateType))
			{
				var errMsg = "No Template Type given. Process will terminate.";
				tracer.Trace(errMsg);
				SuccessFlag.Set(context, false);
				return;
			}

			// Get the BulkUpload template record from the target entity

			var success = UpdateExcelTemplate(templateType);

			SuccessFlag.Set(context, success);

			tracer.Trace($"The transaction validation success is set to {success}.");

		}

		/// <summary>
		/// Function to allow the testing from the desktop
		/// </summary>
		/// <param name="templateType"></param>
		public bool UpdateExcelTemplate( string templateType )
		{
			// Verify the incoming Template Type is the string representation.
			var templateChoice = dbService.Templates.ValidateTemplateTypeChoice(templateType);

			var templateRecord = dbService.Templates.GetAllRecords()
													.Where(t => t.TemplateType == templateChoice && t.IsActive)
													.FirstOrDefault();
			if (templateRecord == null)
			{
				tracer.Trace($"Unexpected error! A template for {templateType} could not be retrieved.");
				return false;
			}

			templateRecord.Initialize(dbService.OrganizationService, tracer);

			IFileData fileContents = templateRecord.TemplateContents;

			var bulkUpload = new BulkUpload(dbService, tracer);

			var excelStream = bulkUpload.UpdateTemplateReferenceTables(fileContents, templateChoice);

			// Reload the modified File

			var xlFileData = new FileData()
			{
				FileBytes = excelStream.ToArray(),
				FileName = fileContents.FileName,
				MimeType = fileContents.MimeType
			};

			templateRecord.TemplateContents = xlFileData;

			tracer.Trace("Excel template to be updated as follows.");
			tracer.Trace($"  File Name: {xlFileData.FileName}, Size: {xlFileData.FileBytes.Length}, Mime Type: {xlFileData.MimeType}");

			// Clear the Needs Update flag
			templateRecord.NeedsUpdate = false;

			dbService.Update(templateRecord.Entity, true);

			return true;
		}

	}
}
