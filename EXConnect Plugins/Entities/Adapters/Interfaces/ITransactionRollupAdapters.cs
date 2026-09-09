using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{
	public interface ITransactionRollupAdapter<T> : ITableAdapter<T>
	{
		T GetRecordFromId( Guid id );
	}

	public interface ITransactionMonthlyRollupAdapter : ITransactionRollupAdapter<ITransactionMonthlyRollupRecord>
	{
		EntityReference GetOrSetTransactionRollupParent( TransferDirection toFrom, ICARTtransactionRecord transaction, int fiscalMonth, int fiscalYear );
		ITransactionMonthlyRollupRecord GetRecord( Guid requestId, EntityReference fundingType, EntityReference appropriation, EntityReference account,
													int? fiscalMonth = null, int? fiscalYear = null, bool createIfNotFound = true );
	}

	public interface ITransactionYearlyRollupAdapter : ITransactionRollupAdapter<ITransactionYearlyRollupRecord>
	{
		ITransactionYearlyRollupRecord GetRecord( EntityReference fundingType, EntityReference appropriation, EntityReference account,
																int? fiscalYear =  null, bool createIfNotFound = true );
		EntityReference GetOrSetRollupParent( ITransactionMonthlyRollupRecord monthlyRollup );
		bool RecomputeRollupFields(Guid rollupId, ITracingService tracer);

		decimal GetAvailableBalanceForAppropriation( EntityReference appropriationRef, int fiscalYear );
	}

}
