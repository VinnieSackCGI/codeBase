
using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins
{
	using Plugin_Handling;

#if DESKTOP
	public class TemplateUpdateOnFundingSourceAppropriationChangePlugin
	{
		IRepository dbService;
		ITracingService tracer;

#else

	[CrmPluginRegistration(MessageNameEnum.Associate,
    "none",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "",
    "FundingSource_Appropriation_Associate", 2,
    IsolationModeEnum.Sandbox,
    Description = "Set the Needs Update flag to the EXConnect Templates on Appropriation Association",
    Id = "1c4498c8-e944-4194-b3f7-2469eae8cf11")]

    [CrmPluginRegistration(MessageNameEnum.Disassociate,
    "none",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "",
    "FundingSource_Appropriation_Disassociate", 1,
    IsolationModeEnum.Sandbox,
    Description = "Set the Needs Update flag to the EXConnect Templates on Appropriation Disassociation",
    Id = "870420e8-e741-4d8e-813e-e45e16435b79")]

	[CrmPluginRegistration(MessageNameEnum.Create,
	"eca_fundingsource",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "",
	"TemplatesUpdateOnFundingSourceCreate", 3,
	IsolationModeEnum.Sandbox,
	Description = "Set the Needs Update flag to the EXConnect Templates on Funding Source Creation",
	Id = "03a4c536-dadb-48ef-ae01-126aac816802")]

	[CrmPluginRegistration(MessageNameEnum.Update,
	"eca_fundingsource",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "eca_description,eca_fundingsource,eca_parentfundingtype,eca_sourcename",
	"TemplatesUpdateOnFundingSourceUpdate", 4,
	IsolationModeEnum.Sandbox,
	Description = "Set the Needs Update flag to the EXConnect Templates on Funding Source Updates",
	Id = "f10e34f1-e0b0-47b5-8650-d6de704992d8")]

	public class FundingSourceAppropriationPlugin : MainPlugin
    {

#endif

#if DESKTOP

		public TemplateUpdateOnFundingSourceAppropriationChangePlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin()
		{

#else
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Funding Source Appropriation Plugin");

			if (this.context.MessageName == MessageNameEnum.Associate.ToString()
			 || this.context.MessageName == MessageNameEnum.Disassociate.ToString())
			{

				// Make sure the relationship is the one we expecting. Otherwise quit.

				var relationshipName = context.GetRelationship().SchemaName;

				tracer.Trace($"Relationship name = {relationshipName}");

				if (!relationshipName.EndsWith("FundingSource_Appropriation"))
				{
					tracer.Trace($"Call ignored...");
					return;
				}
			}
			else if (this.context.MessageName != MessageNameEnum.Create.ToString()
				  && this.context.MessageName != MessageNameEnum.Update.ToString())
			{
				tracer.Trace($"The plugin was called with the \"{this.context.MessageName}\" message. The call will be ignored.");
				return;
			}

#endif

			// Set the "Needs Update" flag for all Active Templates
			if ( dbService.Templates.SetNeedsUpdateFlag(true))
			{
				tracer.Trace($"Setting the Needs Update flag succeeded.");
			}
			else
			{
				tracer.Trace($"Setting the Needs Update flag has faulted.");
			}

		}

	}
}
