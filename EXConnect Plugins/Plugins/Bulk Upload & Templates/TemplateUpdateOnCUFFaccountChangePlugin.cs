
using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins
{
	using Plugin_Handling;

#if DESKTOP

	public class TemplateUpdateOnCUFFacountChangePlugin
	{
			IRepository dbService;
			ITracingService tracer;
		public TemplateUpdateOnCUFFacountChangePlugin( IRepository dbserv, ITracingService trace )
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin( string messageName, ICUFFaccountRecord newTransaction )
		{

#else

	[CrmPluginRegistration(MessageNameEnum.Create,
    "rcade_cuffaccount",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "",
    "TemplatesUpdateOnCUFFAccountCreate", 2,
    IsolationModeEnum.Sandbox,
    Description = "Set the Needs Update flag to the EXConnect Templates on CUFF Account Creation",
    Id = "8a653f62-4c40-48a7-b69d-b64cdd3c52ea")]

    [CrmPluginRegistration(MessageNameEnum.Update,
    "rcade_cuffaccount",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "rcade_cuffaccountname,eca_accountneighborhood,eca_accountdivision,eca_accountprogram,"
									+"eca_accountsubprogram,eca_functioncode,eca_eligibleforspending,cr15a_parentcuffaccount",
	"TemplatesUpdateOnCUFFAccountUpdate", 1,
    IsolationModeEnum.Sandbox,
    Description = "Set the Needs Update flag to the EXConnect Templates on CUFF Account Updates",
    Id = "0f2bb83f-c615-4871-88a8-d611348c33f6")]

	public class TemplateUpdateOnCUFFaccountChangePlugin : MainPlugin
    {

		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of the Template Update On CUFF Account Change Plugin");

			// Get the changes to the Account record object from the target entity
			var entity = context.GetTargetEntity();

#endif

			// Set the "Needs Update" flag for all Active Templates
			if (dbService.Templates.SetNeedsUpdateFlag(true))
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
