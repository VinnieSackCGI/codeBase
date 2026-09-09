using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface IStagedTransactionRecord : ITableRecord
	{
		#region Interrogation Properties

		bool IsActive { get; }

		bool IsAOA { get; }
		bool IsAppropriationAllocation { get; }
		bool IsCommitment { get; }
		bool IsObligation { get; }
		bool IsProgramTransfer { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string Type { get; set; }
		int? FiscalYear { get; set; }
		string Description { get; set; }
		decimal? Amount { get; set; }
		string IBIS_RequestCode { get; set; }
		string ProjectCode { get; set; }
		bool IsValid { get; set; }

		string ValidationErrors { get; set; }

		string State { get; set; }
		string Status { get; set; }

		EntityReference Appropriation { get; set; }
		EntityReference CUFFaccount { get; set; }
		EntityReference FromCUFFaccount { get; set; }
		EntityReference FundingType { get; set; }
		EntityReference FundingSource { get; set; }

		#endregion

		#region Methods

		IStagedTransactionRecord GetCombinedTransactionRecord( IStagedTransactionRecord transaction );

		#endregion
	}
}
