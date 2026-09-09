
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
    public interface IAppropriationYearlyRollupRecord : ITableRecord
    {
        #region Interrogation Properties

        #endregion

        #region Data fields

        string Name { get; set; }
        EntityReference Appropriation { get; set; }
        int? FiscalYear { get; set; }
		decimal? AllocatedAppropriated { get; set; }
		decimal? AllocatedCarryover { get; set; }
		decimal? AllocatedRecoveries { get; set; }
		decimal? AllocatedReimbursements { get; set; }
		decimal? TotalAllocated { get; set; }
		decimal? PendingTotalAllocated { get; set; }
		decimal? TotalResources { get; }
		decimal? AppropriatedBalance { get; }

		#endregion
	}
}
