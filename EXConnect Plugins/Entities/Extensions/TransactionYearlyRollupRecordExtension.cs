using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;
using Newtonsoft.Json.Linq;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_YearlyRollupForAppAccAssocs : ITransactionYearlyRollupRecord
	{
		#region Interrogation Properties

		#endregion

		#region Data Properties

		public string Name
		{
			get => rcade_Name;

			set
			{
				rcade_Name = value;
			}
		}

		public int? FiscalYear
		{
			get => rcade_FiscalYear;
			set
			{
				rcade_FiscalYear = value;
			}
		}

		public EntityReference FundingType
		{
			get => eca_FundingTypeonRollup;
			set => eca_FundingTypeonRollup = value;
		}

		public EntityReference Appropriation
		{
			get => rcade_Appropriation;
			set
			{
				rcade_Appropriation = value;
			}
		}

		public EntityReference Account
		{
			get => rcade_CUFFAccount;
			set => rcade_CUFFAccount = value;
		}

		public EntityReference ParentRollup
		{
			get => eca_ParentYearlyRollup;
			set => eca_ParentYearlyRollup = value;
		}

		public bool RecalculateFields
		{
			get => eca_RecomputeRollupFields.GetValueOrDefault(false);
			set => eca_RecomputeRollupFields = value;
		}

		public decimal? ApprovedFinPlan
		{
			get => (rcade_FinPlan == null ? 0 : rcade_FinPlan.Value);
			set => rcade_FinPlan = new Money(value ?? 0);
		}
		public decimal? ApprovedInternalTransfers
		{
			get => (rcade_InternalTransfers == null ? 0 : rcade_InternalTransfers.Value);
			set => rcade_InternalTransfers = new Money(value ?? 0);
		}
		public decimal? ApprovedAOAamount
		{
			get => (eca_ApprovedAOAAmount == null ? 0 : eca_ApprovedAOAAmount.Value);
			set => eca_ApprovedAOAAmount = new Money(value ?? 0);
		}
		public decimal? ApprovedCommittedAmount
		{
			get => (eca_ApprovedCommitmentAmount == null ? 0 : eca_ApprovedCommitmentAmount.Value);
			set => eca_ApprovedCommitmentAmount = new Money(value ?? 0);
		}
		public decimal? ApprovedObligatedAmount
		{
			get => (eca_ApprovedObligatedAmount == null ? 0 : eca_ApprovedObligatedAmount.Value);
			set => eca_ApprovedObligatedAmount = new Money(value ?? 0);
		}

		public decimal? PendingFinPlan
		{
			get => (eca_PendingFinPlan == null ? 0 : eca_PendingFinPlan.Value);
			set => eca_PendingFinPlan = new Money(value ?? 0);
		}
		public decimal? PendingInternalTransfers
		{
			get => (eca_PendingInternalTransfer == null ? 0 : eca_PendingInternalTransfer.Value);
			set => eca_PendingInternalTransfer = new Money(value ?? 0);
		}
		public decimal? PendingAOAamount
		{
			get => (eca_PendingAOAAmount == null ? 0 : eca_PendingAOAAmount.Value);
			set => eca_PendingAOAAmount = new Money(value ?? 0);
		}
		public decimal? PendingCommittedAmount
		{
			get => (eca_PendingCommitmentAmount == null ? 0 : eca_PendingCommitmentAmount.Value);
			set => eca_PendingCommitmentAmount = new Money(value ?? 0);
		}
		public decimal? PendingObligatedAmount
		{
			get => (eca_PendingObligatedAmount == null ? 0 : eca_PendingObligatedAmount.Value);
			set => eca_PendingObligatedAmount = new Money(value ?? 0);
		}

		public decimal? ApprovedFinPlanRollup
		{
			get => (eca_ApprovedFinPlanRollup == null ? 0 : eca_ApprovedFinPlanRollup.Value);
		}
		public decimal? ApprovedInternalTransferRollup
		{
			get => (eca_ApprovedInternalTransfersRollup == null ? 0 : eca_ApprovedInternalTransfersRollup.Value);
		}
		public decimal? ApprovedAOAamountRollup
		{
			get => (eca_ApprovedAOAAmount == null ? 0 : eca_ApprovedAOAAmount.Value);
		}
		public decimal? ApprovedCommittedAmountRollup
		{
			get => (eca_ApprovedCommitmentAmountRollup == null ? 0 : eca_ApprovedCommitmentAmountRollup.Value);
		}
		public decimal? ApprovedObligatedAmountRollup
		{
			get => (eca_ApprovedObligatedAmountRollup == null ? 0 : eca_ApprovedObligatedAmountRollup.Value);
		}

		public decimal? PendingFinPlanRollup
		{
			get => (eca_PendingFinPlanRollup == null ? 0 : eca_PendingFinPlanRollup.Value);
		}
		public decimal? PendingInternalTransferRollup
		{
			get => (eca_PendingInternalTransferRollup == null ? 0 : eca_PendingInternalTransferRollup.Value);
		}
		public decimal? PendingAOAamountRollup
		{
			get => (eca_PendingAOAAmountRollup == null ? 0 : eca_PendingAOAAmountRollup.Value);
		}
		public decimal? PendingCommittedAmountRollup
		{
			get => (eca_PendingCommitmentAmountRollup == null ? 0 : eca_PendingCommitmentAmountRollup.Value);
		}
		public decimal? PendingObligatedAmountRollup
		{
			get => (eca_PendingObligatedAmountRollup == null ? 0 : eca_PendingObligatedAmountRollup.Value);
		}

		public decimal? ApprovedAvailableBalance
		{
			get => (eca_ApprovedAvailableBalance == null ? 0 : eca_ApprovedAvailableBalance.Value);
		}
		public decimal? PendingAvailableBalance
		{
			get => (eca_PendingAvailableBalance == null ? 0 : eca_PendingAvailableBalance.Value);
		}

		public Entity Entity
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region Public Methods

		public void Initialize( EntityReference fundingType, EntityReference appropriation, EntityReference account, int fiscalYear )
		{
			// Initialize the record
			Name = string.Concat(appropriation.Name, " : ", account.Name, " : ", fundingType.Name, " : ", fiscalYear.ToString());
			
			FiscalYear = fiscalYear;
			FundingType = fundingType;

			Appropriation = appropriation;
			Account = account;

			ApprovedFinPlan = 0;
			ApprovedInternalTransfers = 0;
			ApprovedAOAamount = 0;
			ApprovedCommittedAmount = 0;
			ApprovedObligatedAmount = 0;

			PendingFinPlan = 0;
			PendingInternalTransfers = 0;
			PendingAOAamount = 0;
			PendingCommittedAmount = 0;
			PendingObligatedAmount = 0;
		}

		public void Initialize( ITransactionMonthlyRollupRecord monthlyRollup )
		{
			// Initialize the record
			var rollupName = monthlyRollup.Name;
			// Remove the month from the name
			Name = rollupName.Substring(0, rollupName.LastIndexOf("-")).Trim();

			FiscalYear = monthlyRollup.FiscalYear;
			FundingType = monthlyRollup.FundingType;

			Appropriation = monthlyRollup.Appropriation;
			Account = monthlyRollup.Account;

			ApprovedFinPlan = 0;
			ApprovedInternalTransfers = 0;
			ApprovedAOAamount = 0;
			ApprovedCommittedAmount = 0;
			ApprovedObligatedAmount = 0;

			PendingFinPlan = 0;
			PendingInternalTransfers = 0;
			PendingAOAamount = 0;
			PendingCommittedAmount = 0;
			PendingObligatedAmount = 0;
		}

	}
	#endregion

}
