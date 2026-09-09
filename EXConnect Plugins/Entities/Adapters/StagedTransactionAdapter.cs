using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class StagedTransactionAdapter : IStagedTransactionAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public StagedTransactionAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<IStagedTransactionRecord> GetAllRecords()
		{
			return serviceContext.rcade_CARTTransactionSet as IEnumerable<IStagedTransactionRecord>;
		}
		public IStagedTransactionRecord GetRecordFromId( Guid id )
		{
			return serviceContext.eca_StagedTransactionSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public IStagedTransactionRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public IStagedTransactionRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_StagedTransaction>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var transactionRecord = serviceContext.eca_StagedTransactionSet.Where(a => a.Name == name).FirstOrDefault();
			if (transactionRecord == null) return null;

			return transactionRecord.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var transactionRecord = serviceContext.eca_StagedTransactionSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (transactionRecord == null) return null;

			return transactionRecord.ToEntityReference();
		}

		public IStagedTransactionRecord CreateRecord()
		{
			return new eca_StagedTransaction();
		}

		#region Methods

		public IStagedTransactionRecord ComposeTransactionRecordFromEXConnect( ICARTtransactionRecord transaction )
		{
			// Construct a Staged Transaction record from the incoming CART Transaction record.

			var newRecord = CreateRecord();

			newRecord.Appropriation = transaction.Appropriation;
			newRecord.CUFFaccount = transaction.CUFFaccount;
			newRecord.FundingSource = transaction.FundingSource;
			newRecord.FundingType = transaction.FundingType;
			newRecord.FiscalYear = transaction.FiscalYear;
			newRecord.Type = transaction.Type;
			newRecord.Amount = transaction.Amount;
			newRecord.Description = transaction.Description;
			newRecord.IBIS_RequestCode = transaction.IBIS_RequestCode;
			newRecord.ProjectCode = transaction.ProjectCode;
			newRecord.FromCUFFaccount = transaction.FromCUFFaccount;

			return newRecord;
		}

		#endregion

	}
}
