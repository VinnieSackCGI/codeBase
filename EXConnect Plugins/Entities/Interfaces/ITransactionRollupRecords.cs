using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface ITransactionRollupRecord : ITableRecord
	{
		#region Properties

		string Name { get; set; }
		int? FiscalYear { get; set; }

		EntityReference FundingType { get; set; }
		EntityReference Appropriation { get; set; }
		EntityReference Account { get; set; }

		EntityReference ParentRollup { get; set; }	

		decimal? ApprovedFinPlan { get; set; }
		decimal? ApprovedInternalTransfers { get; set; }
		decimal? ApprovedAOAamount { get; set; }
		decimal? ApprovedCommittedAmount { get; set; }
		decimal? ApprovedObligatedAmount { get; set; }

		decimal? PendingFinPlan { get; set; }
		decimal? PendingInternalTransfers { get; set; }
		decimal? PendingAOAamount { get; set; }
		decimal? PendingCommittedAmount { get; set; }
		decimal? PendingObligatedAmount { get; set; }

		decimal? ApprovedFinPlanRollup { get; }
		decimal? ApprovedInternalTransferRollup { get; }
		decimal? ApprovedAOAamountRollup { get; }
		decimal? ApprovedCommittedAmountRollup { get; }
		decimal? ApprovedObligatedAmountRollup { get; }

		decimal? PendingFinPlanRollup { get; }
		decimal? PendingInternalTransferRollup { get; }
		decimal? PendingAOAamountRollup { get; }
		decimal? PendingCommittedAmountRollup { get; }
		decimal? PendingObligatedAmountRollup { get; }

		decimal? ApprovedAvailableBalance { get; }
		decimal? PendingAvailableBalance { get; }

		#endregion
	}


	public interface ITransactionMonthlyRollupRecord : ITransactionRollupRecord
	{
		#region Properties

		int? FiscalMonth { get; set; }

		#endregion

		#region Public Methods

		void Initialize( EntityReference fundingType, EntityReference appropriation, EntityReference account,
						 int fiscalMonth, int fiscalYear );

		ITransactionMonthlyRollupRecord GetCombinedRollupDeltaRecord( ITransactionRollupRecord oldRollup, out bool amountsChanged );

		#endregion
	}

	public interface ITransactionYearlyRollupRecord : ITransactionRollupRecord
	{
		#region Properties

		bool RecalculateFields { get; set;  }

		#endregion

		#region Public Methods

		void Initialize( EntityReference fundingType, EntityReference appropriation, EntityReference account, int fiscalYear );
		void Initialize( ITransactionMonthlyRollupRecord monthlyRollup );

		#endregion
	}

}
