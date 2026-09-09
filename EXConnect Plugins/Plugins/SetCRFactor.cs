using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Plugins_CommonLibrary.Extensions;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConect_Plugins
{
	using System.CodeDom;
	using System.Data.OleDb;
	using System.Net.NetworkInformation;
	using System.Threading;
	using EXConnect_Plugins.Plugins.Common;
	using EXConnect_Plugins.Entities.Adapters.Interfaces;
	using Plugins_CommonLibrary.Extensions;
	using System.Globalization;

	/// <summary>
	/// Sets the cr factor on the appropriation. CR Factor = CR days/number of days in fiscal year. 
	/// If no CR Days, default CR Factor to 1.
	/// </summary>
	[CrmPluginRegistration(MessageNameEnum.Create,
    "rcade_appropriation",
    StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "",
	"SetAppropriationCRFactorOnCreate", 1,
    IsolationModeEnum.Sandbox,
    Description = "Sets the CR Factor on Appropriation Create",
    Id = "30851f5c-2276-4456-b490-937a57576ebb")]

    [CrmPluginRegistration(MessageNameEnum.Update,
    "rcade_appropriation",
    StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "rcade_crdays",
    "SetAppropriationCRFactorOnUpdate", 1,
    IsolationModeEnum.Sandbox,
    Image1Type = ImageTypeEnum.PreImage,
    Image1Name = ImageTypeName.PreImage,
    Image1Attributes = "",
    Description = "Sets the CR Factor on Appropriation Update",
    Id = "d149645c-3104-4317-9f77-6608170dc41a")]
    public class SetCRFactor : MainPlugin
	{
        protected override void ExecutePlugin(IExtendedPluginContext executeContext)
        {
			InitializeContext(executeContext, true);

			tracer.Trace("Beginning execution of Transaction Validation plugin");

            var appropriation = dbService.Appropriation.GetRecordFromEntity(context.GetTargetEntity());

            //Only grab the preimage if it's an update and check if the CR days has changed to 365.
            if (context.MessageName == "Update")
            {
                var appropriationPreImage = dbService.Appropriation.GetRecordFromEntity(context.GetPreImage());
                if (appropriationPreImage.CRDays != 365 && appropriation.CRDays == 365)
                {
                    appropriation.AllowNegativeAccountBalances = false;
                }
            }
            if (!appropriation.CRDays.HasValue)
            {
                appropriation.CRFactor = 1M;
            }
            else
            {
                // Compare total Days against CR Days
				var fyDays = dbService.LocalDateTime.FYTotalDays();
                if(appropriation.CRDays > fyDays)
                {
                    throw new InvalidPluginExecutionException($"The value of {appropriation.CRDays} set for CR Days is greater than the number of days in the current fiscal year.");
                }
				appropriation.CRFactor = ((decimal)appropriation.CRDays.Value) / ((decimal)fyDays);
			}
		}
    }
}
