
using System;
using EXConnect_Plugins.Entities;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Plugins_CommonLibrary.Extensions;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins
{

	// This plugin will post a transaction into the Yearly Rollup table based for the given Appropriation,
	// CUFF Account, Funding Type,and Fiscal Year. Eventhough the Yearly Rollup table is setup for Hierarchical
	// capabilites, and thus the CUFF Account tree, related to the transaction, needs to exist in the rollup table,
	// the tree contruction/updates will be triggered by those action in the Monthly Transactino Rollup table. 

#if DESKTOP

	// This block allows to debug the logic of the plugin directly from the Desktip Interface project
	public class YearlyTransactionRollupPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public YearlyTransactionRollupPlugin( IRepository dbserv, ITracingService trace)
		{ 
			dbService = dbserv;
			tracer = trace;
		}
		public void ExecutePlugin( string messageName, ITransactionMonthlyRollupRecord newMonthlyRollup, ITransactionMonthlyRollupRecord oldMonthlyRollup = null )
		{
			if (messageName == MessageNameEnum.Update.ToString())
			{
				ProcessUpdatedMonthlyRollup(newMonthlyRollup, oldMonthlyRollup);
			}
			else
			{
				ProcessNewMonthlyRollup(newMonthlyRollup);
			}
		}

#else

	[CrmPluginRegistration(MessageNameEnum.Create,
	"rcade_monthlyrollupforappaccassocs",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "",
	"Update/Create Yearly Rollup On MonthlyRollup Create", 6,
	IsolationModeEnum.Sandbox,
	Description = "Add or Update the Transaction Yearly Rollups table when a MonthlyRollup is created.",
	Id = "79198b84-c6dd-494c-8beb-2d908a8d9555")]

	[CrmPluginRegistration(MessageNameEnum.Update,
	"rcade_monthlyrollupforappaccassocs",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "rcade_appropriation,rcade_fiscalyear,rcade_cuffaccount,eca_fundingtypeonrollup,eca_parentmonthlyrollup,"
								+ "rcade_finplan,rcade_internaltransfers,eca_approvedaoaamount,eca_approvedcommitmentamount,eca_approvedobligatedamount,"
								+ "eca_pendingfinplan,eca_pendinginternaltransfers,eca_pendingaoaamount,eca_pendingcommittedamount,eca_pendingobligatedamount",
	"Update/Create Yearly Rollup On MonthlyRollup Update", 7,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "rcade_appropriation,rcade_fiscalyear,rcade_cuffaccount,eca_fundingtypeonrollup," 
						+ "rcade_finplan,rcade_internaltransfers,eca_approvedaoaamount,eca_approvedcommitmentamount,eca_approvedobligatedamount,"
						+ "eca_pendingfinplan,eca_pendinginternaltransfers,eca_pendingaoaamount,eca_pendingcommittedamount,eca_pendingobligatedamount",
	Description = "Add or Update the Transaction Yearly Rollups table when a MonthlyRollup is updated.",
	Id = "364bc79b-f8ec-4f21-b2e2-b0195a9d94d5")]

	public class YearlyTransactionRollupPlugin : MainPlugin
	{
		protected override void ExecutePlugin( IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of MonthlyRollup Yearly Rollup plugin");

			// Get the MonthlyRollup record object from the target entity
			var newMonthlyRollup = dbService.MonthlyRollup.GetRecordFromEntity(context.GetTargetEntity());

			ITransactionMonthlyRollupRecord oldMonthlyRollup = null;

			if (context.MessageName == MessageNameEnum.Update.ToString())
			{
				oldMonthlyRollup = dbService.MonthlyRollup.GetRecordFromEntity(context.GetPreImage());
				ProcessUpdatedMonthlyRollup(newMonthlyRollup, oldMonthlyRollup);
			}
			else
			{
				ProcessNewMonthlyRollup(newMonthlyRollup);
			}

		}

#endif

		#region Auxiliary Methods

		/// <summary>
		/// Process a new Monthly Rollup into the Yearly Rollup table
		/// </summary>
		/// <returns></returns>
		public void ProcessNewMonthlyRollup( ITransactionMonthlyRollupRecord monthlyRollup )
		{
			// Retrieve the corresponding Yearly Rollup for the given monthly 

			var fiscalYear = monthlyRollup.FiscalYear.Value;

			var yearlyRollup = RetrieveYearlyRollupRecord(monthlyRollup);

			var amountsChanged = ApplyAmountToRollup(yearlyRollup, monthlyRollup);

			if (amountsChanged)
			{
				// Set the recompute flag and save the changes
				yearlyRollup.RecalculateFields = true;

				dbService.Update(yearlyRollup.Entity);

				tracer.Trace("Transaction Yearly Rollup amounts changed. Recompute Fields flag set to true.");
			}
			else
			{
				tracer.Trace("Transaction Yearly Rollup amount fields did not change. No recalculation performed.");
			}

		}

		public void ProcessUpdatedMonthlyRollup( ITransactionMonthlyRollupRecord newMonthlyRollup, ITransactionMonthlyRollupRecord oldMonthlyRollup )
		{
			// Construct a combined MonthlyRollup so we can process
			bool amountsChanged = false; // Keep track of changes to the amounts
			var combinedDeltaMonthlyRollup = newMonthlyRollup.GetCombinedRollupDeltaRecord(oldMonthlyRollup, out amountsChanged);

			var yearlyRollup = RetrieveYearlyRollupRecord(combinedDeltaMonthlyRollup);

			ApplyAmountToRollup(yearlyRollup, combinedDeltaMonthlyRollup);

			if (amountsChanged)
			{
				// Set the recompute flag and save the changes
				yearlyRollup.RecalculateFields = true;

				dbService.Update(yearlyRollup.Entity);
				tracer.Trace("Transaction Yearly Rollup amounts changed. Recompute Fields flag set to true.");
			}
			else
			{
				tracer.Trace("Transaction Yearly Rollup amount fields did not change. No recalculation performed.");
			}
		}

		private ITransactionYearlyRollupRecord RetrieveYearlyRollupRecord( ITransactionMonthlyRollupRecord monthlyRollup )
		{
			var yearlyRollup = dbService.YearlyRollup.GetRecord(monthlyRollup.FundingType, monthlyRollup.Appropriation,
																monthlyRollup.Account, monthlyRollup.FiscalYear, false);

			if ( yearlyRollup == null )
			{
				// Retrieve the yearly rollup parent corresponding to the Monthly Rollup
				var parentRollup = dbService.YearlyRollup.GetOrSetRollupParent(monthlyRollup);

				// Create the Yearly Rollup record
				yearlyRollup = dbService.YearlyRollup.CreateRecord();
				yearlyRollup.Initialize(monthlyRollup);
				yearlyRollup.ParentRollup = parentRollup;

				yearlyRollup.Id = dbService.Create(yearlyRollup.Entity);
			}

			return yearlyRollup;

		}

		private bool ApplyAmountToRollup( ITransactionYearlyRollupRecord yearlyRollup, ITransactionMonthlyRollupRecord monthlyRollup )
		{
			// Apply the data values based on MonthlyRollup record
			yearlyRollup.ApprovedFinPlan += monthlyRollup.ApprovedFinPlan;
			var amountsChanged = (monthlyRollup.ApprovedFinPlan != 0);

			yearlyRollup.ApprovedObligatedAmount += monthlyRollup.ApprovedObligatedAmount;
			amountsChanged |= (monthlyRollup.ApprovedObligatedAmount != 0);

			yearlyRollup.ApprovedCommittedAmount += monthlyRollup.ApprovedCommittedAmount;
			amountsChanged |= (monthlyRollup.ApprovedObligatedAmount != 0);

			yearlyRollup.ApprovedAOAamount += monthlyRollup.ApprovedAOAamount;
			amountsChanged |= (monthlyRollup.ApprovedAOAamount != 0);

			yearlyRollup.ApprovedInternalTransfers += monthlyRollup.ApprovedInternalTransfers;
			amountsChanged |= (monthlyRollup.ApprovedInternalTransfers != 0);

			yearlyRollup.PendingFinPlan += monthlyRollup.PendingFinPlan;
			amountsChanged |= (monthlyRollup.PendingFinPlan != 0);

			yearlyRollup.PendingObligatedAmount += monthlyRollup.PendingObligatedAmount;
			amountsChanged |= (monthlyRollup.PendingObligatedAmount != 0);

			yearlyRollup.PendingCommittedAmount += monthlyRollup.PendingCommittedAmount;
			amountsChanged |= (monthlyRollup.PendingCommittedAmount != 0);

			yearlyRollup.PendingAOAamount += monthlyRollup.PendingAOAamount;
			amountsChanged |= (monthlyRollup.PendingAOAamount != 0);

			yearlyRollup.PendingInternalTransfers += monthlyRollup.PendingInternalTransfers;
			amountsChanged |= (monthlyRollup.PendingInternalTransfers != 0);

			return amountsChanged;
		}

		#endregion

	}

}