using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Services;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_MonthlyRollupForAppAccAssocs : ITransactionMonthlyRollupRecord
	{
		#region Data Properties
		public string Name
		{
			get => rcade_Name;
			set => rcade_Name = value;
		}

		public int? FiscalYear
		{
			get => rcade_FiscalYear;
			set => rcade_FiscalYear = value;
		}

		public int? FiscalMonth
		{
			get => eca_FiscalMonth;
			set => eca_FiscalMonth = value;
		}

		public EntityReference FundingType
		{
			get => eca_FundingTypeonRollup;
			set => eca_FundingTypeonRollup = value;
		}
		public EntityReference Appropriation
		{
			get => rcade_Appropriation; 
			set => rcade_Appropriation = value;
		}

		public EntityReference Account
		{
			get => rcade_CUFFAccount; 
			set => rcade_CUFFAccount = value;
		}

		public EntityReference ParentRollup
		{
			get => eca_ParentMonthlyRollup;
			set => eca_ParentMonthlyRollup = value;
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
			get => (eca_PendingInternalTransfers == null ? 0 : eca_PendingInternalTransfers.Value);
			set => eca_PendingInternalTransfers = new Money(value ?? 0);
		}
		public decimal? PendingAOAamount
		{
			get => (eca_PendingAOAAmount == null ? 0 : eca_PendingAOAAmount.Value);
			set => eca_PendingAOAAmount = new Money(value ?? 0);
		}
		public decimal? PendingCommittedAmount
		{
			get => (eca_PendingCommittedAmount == null ? 0 : eca_PendingCommittedAmount.Value);
			set => eca_PendingCommittedAmount = new Money(value ?? 0);
		}
		public decimal? PendingObligatedAmount
		{
			get => (eca_PendingObligatedAmount == null ? 0 : eca_PendingObligatedAmount.Value);
			set => eca_PendingObligatedAmount = new Money(value ?? 0);
		}

		public decimal? ApprovedFinPlanRollup
		{
			get => (eca_ApprovedFinPlanAmountRollup == null ? 0 : eca_ApprovedFinPlanAmountRollup.Value);
		}
		public decimal? ApprovedInternalTransferRollup
		{
			get => (eca_ApprovedInternalTransferAmountRollup == null ? 0 : eca_ApprovedInternalTransferAmountRollup.Value);
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
			get => (eca_PendingInternalTransferAmountRollup == null ? 0 : eca_PendingInternalTransferAmountRollup.Value);
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

		public void Initialize( EntityReference fundingType, EntityReference appropriation, EntityReference account, int fiscalMonth, int fiscalYear )
		{
			// Initialize the record
			Name = string.Concat(appropriation.Name, " : ", account.Name, " : ", fundingType.Name, " : ", 
										fiscalYear.ToString(), "-", fiscalMonth.ToString());

			FiscalYear = fiscalYear;
			FiscalMonth = fiscalMonth;
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

		public ITransactionMonthlyRollupRecord GetCombinedRollupDeltaRecord( ITransactionRollupRecord origRollup, out bool amountsChanged )
		{
			// Generate a combined transaction record from "this" rollup and the original one. Emphasis on "this" transaction.

			var combinedDeltaRecord = new rcade_MonthlyRollupForAppAccAssocs();

			combinedDeltaRecord.Id = this.Id;
			combinedDeltaRecord.Name = (!string.IsNullOrEmpty(this.Name) ? this.Name : origRollup.Name);
			combinedDeltaRecord.Appropriation = (this.Appropriation != null ? this.Appropriation : origRollup.Appropriation);
			combinedDeltaRecord.Account = (this.Account != null ? this.Account : origRollup.Account);
			combinedDeltaRecord.FundingType = (this.FundingType != null ? this.FundingType : origRollup.FundingType);
			combinedDeltaRecord.FiscalYear = (this.FiscalYear != null ? this.FiscalYear : origRollup.FiscalYear);

			amountsChanged = false;

			if (this.Attributes.Contains("rcade_finplan"))
			{
				combinedDeltaRecord.ApprovedFinPlan = this.ApprovedFinPlan - origRollup.ApprovedFinPlan;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.ApprovedFinPlan = 0;
			}

			if (this.Attributes.Contains("rcade_internaltransfers"))
			{
				combinedDeltaRecord.ApprovedInternalTransfers = this.ApprovedInternalTransfers - origRollup.ApprovedInternalTransfers;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.ApprovedInternalTransfers = 0;
			}

			if (this.Attributes.Contains("eca_approvedaoaamount"))
			{
				combinedDeltaRecord.ApprovedAOAamount = this.ApprovedAOAamount - origRollup.ApprovedAOAamount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.ApprovedAOAamount = 0;
			}

			if (this.Attributes.Contains("eca_approvedcommitmentamount"))
			{
				combinedDeltaRecord.ApprovedCommittedAmount = this.ApprovedCommittedAmount - origRollup.ApprovedCommittedAmount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.ApprovedCommittedAmount = 0;
			}

			if (this.Attributes.Contains("eca_approvedobligatedamount"))
			{
				combinedDeltaRecord.ApprovedObligatedAmount = this.ApprovedObligatedAmount - origRollup.ApprovedObligatedAmount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.ApprovedObligatedAmount = 0;
			}

			if (this.Attributes.Contains("eca_pendingfinplan"))
			{
				combinedDeltaRecord.PendingFinPlan = this.PendingFinPlan - origRollup.PendingFinPlan;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.PendingFinPlan = 0;
			}

			if (this.Attributes.Contains("eca_pendinginternaltransfers"))
			{
				combinedDeltaRecord.PendingInternalTransfers = this.PendingInternalTransfers - origRollup.PendingInternalTransfers;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.PendingInternalTransfers = 0;
			}

			if (this.Attributes.Contains("eca_pendingaoaamount"))
			{
				combinedDeltaRecord.PendingAOAamount = this.PendingAOAamount - origRollup.PendingAOAamount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.PendingAOAamount = 0;
			}

			if (this.Attributes.Contains("eca_pendingcommittedamount"))
			{
				combinedDeltaRecord.PendingCommittedAmount = this.PendingCommittedAmount - origRollup.PendingCommittedAmount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.PendingCommittedAmount = 0;
			}

			if (this.Attributes.Contains("eca_pendingobligatedamount"))
			{
				combinedDeltaRecord.PendingObligatedAmount = this.PendingObligatedAmount - origRollup.PendingObligatedAmount;
				amountsChanged = true;
			}
			else
			{
				combinedDeltaRecord.PendingObligatedAmount = 0;
			}

			return combinedDeltaRecord;

		}

		#endregion
	}
}
