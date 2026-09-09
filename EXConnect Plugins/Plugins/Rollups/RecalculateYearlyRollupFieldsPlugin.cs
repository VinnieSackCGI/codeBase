using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Plugins_CommonLibrary.Extensions;

using EXConnect_Plugins.Plugin_Handling;

namespace EXConnect_Plugins.Plugins.Rollups
{
	// This plugin checks for the RecalculateRollupFieds flag in the Yearly Transaction Rollup table
	// and executes the RecomputeRollupFields method on the table. This action was previously been executed
	// by the YearlyTransactionRollup plugin, but it was found to time out. As a result, it was moved to this
	// new plugin which runs on Post-Operation, hopefully, circumventing the timeout issues. 

	[CrmPluginRegistration(MessageNameEnum.Update,
	"rcade_yearlyrollupforappaccassocs",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "eca_recomputerollupfields",
	"Recalculate Rollup fields on Yearly Rollup Update", 8,
	IsolationModeEnum.Sandbox,
	Description = "Recalculate the Transaction Yearly Rollups fields when the recompute fields flag is updated.",
	Id = "c9e7a099-c399-4b1b-9aa9-c86a6a4f7c1e")]

	public class RecalculateYearlyRollupFieldsPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Yearly Rollup Fields Recalculation plugin");

			// Get the YearlyRollup record object from the target entity
			var yearlyRollup = dbService.YearlyRollup.GetRecordFromEntity(context.GetTargetEntity());

			if (context.MessageName != MessageNameEnum.Update.ToString() || !yearlyRollup.RecalculateFields)
			{
				tracer.Trace("No changes found in the YearlyRollup update to recompute. Process terminated.");
				return;
			}

			// Call the Recompute Rollup Fields in the YearlyRollup record to force the recalculation
			if (!dbService.YearlyRollup.RecomputeRollupFields(yearlyRollup.Id, tracer))
			{
				tracer.Trace("Transaction Yearly Rollup fields recalculation failed to execute.");
			}

			// Rewset the recompute flag
			yearlyRollup.RecalculateFields = false;
			dbService.Update(yearlyRollup.Entity);
		}
	}
}
