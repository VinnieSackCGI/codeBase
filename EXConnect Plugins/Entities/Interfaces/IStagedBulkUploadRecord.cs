using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface IStagedBulkUploadRecord : ITableRecord
	{
		#region Interrogation Properties

		bool IsActive { get; }
		bool IsTransaction { get; }

		bool IsAOA { get; }
		bool IsAppropriationAllocation { get; }
		bool IsSpendPlanRequest { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string TemplateType { get; set; }
		int? FiscalYear { get; set; }
		string Description_Justification { get; set; }
		decimal? Amount { get; set; }
		string IBIS_RequestCode { get; set; }
		string RequestName { get; set; }
		string ProjectCode { get; set; }
		string Priority { get; set; }
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

		IStagedBulkUploadRecord GetCombinedRecord( IStagedBulkUploadRecord bkuRecord );

		#endregion
	}
}
