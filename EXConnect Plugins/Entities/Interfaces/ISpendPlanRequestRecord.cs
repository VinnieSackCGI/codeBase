
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface ISpendPlanRequestRecord : ITableRecord
	{
		#region Interrogation Properties

		bool IsActive { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string FiscalYear { get; set; }
		EntityReference Appropriation { get; set; }
		EntityReference CUFFaccount { get; set; }
		EntityReference FundingType { get; set; }
		EntityReference FunctionCode { get; set; }

		string Type { get; set; }
		string Justification { get; set; }
		string Priority { get; set; }
		decimal? Amount { get; set; }

		string Status { get; set; }

		#endregion

		#region Public Methods

		ISpendPlanRequestRecord GetCombinedRecord( ISpendPlanRequestRecord sprRecord );

		#endregion
	}
}
