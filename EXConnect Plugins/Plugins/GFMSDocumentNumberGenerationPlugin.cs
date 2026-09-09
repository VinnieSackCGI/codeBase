
using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using System.Xml.Linq;
using EXConnect_Plugins.Common;
using System.Threading;
using EXConnect_Plugins.Plugins.Common;

namespace EXConnect_Plugins
{

#if DESKTOP
	using Common;
	using Entities.Interfaces;
	using Microsoft.Xrm.Sdk;

	// This block allows to debug the logic of the plugin directly from the Desktop Interface project
	public class GFMSDocumentNumberGenerationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public GFMSDocumentNumberGenerationPlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin( IGFMSdocumentRecord newDocument )
		{
#else
/*
	[CrmPluginRegistration(MessageNameEnum.Create,
	"eca_gfmsdocument",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "",
	"GFMS Document Number Generation On Create", 1,
	IsolationModeEnum.Sandbox,
	Description = "Generate the GFMS Document Number when a record is created",
	Id = "1077f353-4eeb-48cc-8247-990996d89891")]
*/
	// This plugin will genearate the GFNMS Document "Document Number" that all child transactions
	// will use as their Document Number. It is up to the Page to set this number, otherwise,
	// the transaction plugin will retrieve it on the Validation plugin and use it if a parent exists.

	public class GFMSDocumentNumberGenerationPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of GFMS Document Number Generation plugin");

			// Process only On Create
			if (this.context.MessageName != MessageNameEnum.Create.ToString())
			{
				tracer.Trace($"Plugin called on \"{this.context.MessageName}\". Call will be ignored.");
				return;
			}

			// Get the incoming GFMS Documewnt record object from the target entity
			var newDocument = dbService.GFMSdocument.GetRecordFromEntity(context.GetTargetEntity());

#endif

			if (string.IsNullOrEmpty(newDocument.DocumentNumber))
			{
				// Generate the Document Number
				var generateDocNumber = new DocumentNumberGeneration(dbService, tracer);
				if (newDocument.IsCommitment)
				{
					//newDocument.DocumentNumber = generateDocNumber.GenerateCommitmentDocumentNumber(newDocument.Allotment, transaction.Appropriation,
					//																				transaction.Code, transaction.FiscalYear.Value);
				}

			}

		}
	}

}