using System;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{
	public interface IRecordLockAdapter
	{
		bool LockRollupRecord(Guid requesterId, string appropriation, string account, string fundingType, string fiscalYear, DateTime? dateTimeLocal = null);
		bool UnlockRecord( bool useWebAPI = false );
	}
}
