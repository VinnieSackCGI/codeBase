
using Plugins_CommonLibrary.Entities.Interfaces;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using EXConnect_Plugins.Entities.Adapters.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface IRepository : IBaseRepository
    {
 		#region System tables

		ISystemUserAdapter SystemUser { get; }
		ITeamAdapter Team { get; }
		IEnvironmentVariablesAdapter EnvironmentVariables { get; }

		#endregion

		#region Reference Tables

		IAllotmentAdapter Allotment { get; }
		IAppropriationAdapter Appropriation { get; }
		IObligationTypeAdapter ObligationType { get; }
		IPostAdapter Post { get; }
		IFundingSourceAdapter FundingSource { get; }
		IFundingTypeAdapter FundingType { get; }
		INeighborhoodAdapter Neighborhood { get; }
		IDivisionAdapter Division { get; }
		IProgramAdapter Program { get; }
		ISubProgramAdapter SubProgram { get; }
		ITemplatesAdapter Templates { get; }

		#endregion

		ICARTuserRoleAdapter UserRole { get; }
		ICARTtransactionAdapter Transaction { get; }
		IStagedBulkUploadRecordAdapter StagedBulkUploadRecord { get; }
		IGFMSdocumentAdapter GFMSdocument { get; }
		ICUFFaccountAdapter CUFFaccount { get; }
		IAppropriationYearlyRollupAdapter AppropriationRollup { get; }
		ITransactionMonthlyRollupAdapter MonthlyRollup { get; }
		ITransactionYearlyRollupAdapter YearlyRollup { get; }

		IRecordLockAdapter RecordLock { get; }

		IWorkflowAdapter TransactionWorkflow { get; }
		IWorkflowAdapter DocumentWorkflow { get; }
		IProcessStageAdapter ProcessStage { get; }

		ISpendPlanRequestAdapter SpendPlanRequest { get; }

		IQuotaSheetAccountMapAdapter QuotaSheetAccountMap { get; }
		IQuotaSheetHeaderAdapter QuotaSheetHeader { get; }
		IQuotaSheetRegionalDataAdapter QuotaSheetRegional { get; }
		IQuotaSheetFrontOfficeDataAdapter QuotaSheetFrontOffice { get; }

	}
}
