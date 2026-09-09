using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Services;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Entities.Adapters;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Plugin_Handling.Services
{
    public class BusinessDbService : BaseBusinessDbService, IRepository
    {
        public BusinessDbService(IOrganizationService organizationService, ITracingService tracer)
                : base(organizationService, tracer)
        {
            // Create the service context for the OBO database 
            base.OrgServiceContext = new EXConnect_ServiceContext(organizationService);
        }

        #region Data Retrieval

		#region System Tables

		private ISystemUserAdapter systemUserTable;
        public ISystemUserAdapter SystemUser
        {
            get
            {
                if (systemUserTable == null)
                {
                    systemUserTable = new SystemUserAdapter(this);
                }
                return systemUserTable;
            }
        }

        private ITeamAdapter teamTable;

        public ITeamAdapter Team
        {
            get
            {
                if (teamTable == null)
                {
                    teamTable = new TeamAdapter(this);
                }
                return teamTable;
            }
        }

		private IEnvironmentVariablesAdapter environmentVariablesTable;

		public IEnvironmentVariablesAdapter EnvironmentVariables
		{
			get
			{
				if (environmentVariablesTable == null)
				{
					environmentVariablesTable = new EnvironmentVariablesAdapter(this);
				}
				return environmentVariablesTable;
			}
		}

		#endregion

		#region Reference Tables

		private IAllotmentAdapter allotmentTable;
		public IAllotmentAdapter Allotment
		{
			get
			{
				if (allotmentTable == null)
				{
					allotmentTable = new AllotmentAdapter(this);
				}
				return allotmentTable;
			}
		}

		private IAppropriationAdapter appropriationTable;
		public IAppropriationAdapter Appropriation
		{
			get
			{
				if (appropriationTable == null)
				{
					appropriationTable = new AppropriationAdapter(this);
				}
				return appropriationTable;
			}
		}

		private IFundingSourceAdapter fundingSourceTable;
		public IFundingSourceAdapter FundingSource
		{
			get
			{
				if (fundingSourceTable == null)
				{
					fundingSourceTable = new FundingSourceAdapter(this);
				}
				return fundingSourceTable;
			}
		}

		private IFundingTypeAdapter fundingTypeTable;
		public IFundingTypeAdapter FundingType
		{
			get
			{
				if (fundingTypeTable == null)
				{
					fundingTypeTable = new FundingTypeAdapter(this);
				}
				return fundingTypeTable;
			}
		}

		private IObligationTypeAdapter obligationTypeTable;
		public IObligationTypeAdapter ObligationType
		{
			get
			{
				if (obligationTypeTable == null)
				{
					obligationTypeTable = new ObligationTypeAdapter(this);
				}
				return obligationTypeTable;
			}
		}

		private IPostAdapter postTable;
		public IPostAdapter Post
		{
			get
			{
				if (postTable == null)
				{
					postTable = new PostAdapter(this);
				}
				return postTable;
			}
		}

		private INeighborhoodAdapter neighborhoodTable;
		public INeighborhoodAdapter Neighborhood
		{
			get
			{
				if (neighborhoodTable == null)
				{
					neighborhoodTable = new NeighborhoodAdapter(this);
				}
				return neighborhoodTable;
			}
		}

		private IDivisionAdapter divisionTable;
		public IDivisionAdapter Division
		{
			get
			{
				if (divisionTable == null)
				{
					divisionTable = new DivisionAdapter(this);
				}
				return divisionTable;
			}
		}

		private IProgramAdapter programTable;
		public IProgramAdapter Program
		{
			get
			{
				if (programTable == null)
				{
					programTable = new ProgramAdapter(this);
				}
				return programTable;
			}
		}

		private ISubProgramAdapter subProgramTable;
		public ISubProgramAdapter SubProgram
		{
			get
			{
				if (subProgramTable == null)
				{
					subProgramTable = new SubProgramAdapter(this);
				}
				return subProgramTable;
			}
		}

		private ITemplatesAdapter templatesTable;
		public ITemplatesAdapter Templates
		{
			get
			{
				if (templatesTable == null)
				{
					templatesTable = new TemplatesAdapter(this, tracer);
				}
				return templatesTable;
			}
		}

		#endregion

		private ICARTuserRoleAdapter userRoleTable;
		public ICARTuserRoleAdapter UserRole
		{
			get
			{
				if (userRoleTable == null)
				{
                    userRoleTable = new CARTuserRoleAdapter(this);
				}
				return userRoleTable;
			}
		}

		private ICARTtransactionAdapter transactionTable;
		public ICARTtransactionAdapter Transaction
		{
			get
			{
				if (transactionTable == null)
				{
					transactionTable = new CARTtransactionAdapter(this);
				}
				return transactionTable;
			}
		}

		private IStagedBulkUploadRecordAdapter stagedRecordTable;
		public IStagedBulkUploadRecordAdapter StagedBulkUploadRecord
		{
			get
			{
				if (stagedRecordTable == null)
				{
					stagedRecordTable = new StagedBulkUploadRecordAdapter(this);
				}
				return stagedRecordTable;
			}
		}

		private IGFMSdocumentAdapter gfmsDocumentTable;
		public IGFMSdocumentAdapter GFMSdocument
		{
			get
			{
				if (gfmsDocumentTable == null)
				{
					gfmsDocumentTable = new GFMSdocumentAdapter(this);
				}
				return gfmsDocumentTable;
			}
		}

		private ICUFFaccountAdapter accountTable;
		public ICUFFaccountAdapter CUFFaccount
		{
			get
			{
				if (accountTable == null)
				{
					accountTable = new CUFFaccountAdapter(this);
				}
				return accountTable;
			}
		}

		private IAppropriationYearlyRollupAdapter appropriationRollupTable;
		public IAppropriationYearlyRollupAdapter AppropriationRollup
		{
			get
			{
				if (appropriationRollupTable == null)
				{
					appropriationRollupTable = new AppropriationYearlyRollupnAdapter(this);
				}
				return appropriationRollupTable;
			}
		}

		private ITransactionMonthlyRollupAdapter monthlyRollupTable;
		public ITransactionMonthlyRollupAdapter MonthlyRollup
		{
			get
			{
				if (monthlyRollupTable == null)
				{
					monthlyRollupTable = new TransactionMonthlyRollupAdapter(this, tracer);
				}
				return monthlyRollupTable;
			}
		}

		private ITransactionYearlyRollupAdapter yearlyRollupTable;
		public ITransactionYearlyRollupAdapter YearlyRollup
		{
			get
			{
				if (yearlyRollupTable == null)
				{
					yearlyRollupTable = new TransactionYearlyRollupAdapter(this);
				}
				return yearlyRollupTable;
			}
		}

		private IRecordLockAdapter recordLockTable;

		public IRecordLockAdapter RecordLock
		{
			get
			{
				if (recordLockTable == null)
				{
					recordLockTable = new RecordLockAdapter(this, tracer);
				}
				return recordLockTable;
			}
		}

		IWorkflowAdapter transactionWorkflowTable;

		public IWorkflowAdapter TransactionWorkflow
		{
			get
			{
				if (transactionWorkflowTable == null)
				{
					transactionWorkflowTable = new TransactionWorkflowAdapter(this, tracer);
				}
				return transactionWorkflowTable;
			}
		}

		IWorkflowAdapter documentWorkflowTable;

		public IWorkflowAdapter DocumentWorkflow
		{
			get
			{
				if (documentWorkflowTable == null)
				{
					documentWorkflowTable = new DocumentWorkflowAdapter(this, tracer);
				}
				return documentWorkflowTable;
			}
		}

		IProcessStageAdapter processStageTable;

		public IProcessStageAdapter ProcessStage
		{
			get
			{
				if (processStageTable == null)
				{
					processStageTable = new ProcessStageAdapter(this);
				}
				return processStageTable;
			}
		}

		ISpendPlanRequestAdapter spendRequestTable;

		public ISpendPlanRequestAdapter SpendPlanRequest
		{
			get
			{
				if (spendRequestTable == null)
				{
					spendRequestTable = new SpendPlanRequestAdapter(this);
				}
				return spendRequestTable;
			}
		}

		IQuotaSheetAccountMapAdapter qsAccountMapTable;

		public IQuotaSheetAccountMapAdapter QuotaSheetAccountMap
		{
			get
			{
				if (qsAccountMapTable == null)
				{
					qsAccountMapTable = new QuotaSheetAccountMapAdapter(this);
				}
				return qsAccountMapTable;
			}
		}

		IQuotaSheetHeaderAdapter qsHeaderTable;

		public IQuotaSheetHeaderAdapter QuotaSheetHeader
		{
			get
			{
				if (qsHeaderTable == null)
				{
					qsHeaderTable = new QuotaSheetHeaderAdapter(this);
				}
				return qsHeaderTable;
			}
		}

		IQuotaSheetRegionalDataAdapter qsRegionalTable;

		public IQuotaSheetRegionalDataAdapter QuotaSheetRegional
		{
			get
			{
				if (qsRegionalTable == null)
				{
					qsRegionalTable = new QuotaSheetRegionalDataAdapter(this);
				}
				return qsRegionalTable;
			}
		}

		IQuotaSheetFrontOfficeDataAdapter qsFontOfficeTable;

		public IQuotaSheetFrontOfficeDataAdapter QuotaSheetFrontOffice
		{
			get
			{
				if (qsFontOfficeTable == null)
				{
					qsFontOfficeTable = new QuotaSheetFrontOfficeAdapter(this);
				}
				return qsFontOfficeTable;
			}
		}

		#endregion
	}
}
