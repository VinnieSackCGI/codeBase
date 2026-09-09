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
	public class StagedBulkUploadRecordAdapter : IStagedBulkUploadRecordAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public StagedBulkUploadRecordAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<IStagedBulkUploadRecord> GetAllRecords()
		{
			return serviceContext.eca_StagedBulkUploadRecordsSet as IEnumerable<IStagedBulkUploadRecord>;
		}
		public IStagedBulkUploadRecord GetRecordFromId( Guid id )
		{
			return serviceContext.eca_StagedBulkUploadRecordsSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public IStagedBulkUploadRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public IStagedBulkUploadRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_StagedBulkUploadRecords>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var stagedRecord = serviceContext.eca_StagedBulkUploadRecordsSet.Where(a => a.Name == name).FirstOrDefault();
			if (stagedRecord == null) return null;

			return stagedRecord.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var transactionRecord = serviceContext.eca_StagedBulkUploadRecordsSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (transactionRecord == null) return null;

			return transactionRecord.ToEntityReference();
		}

		public IStagedBulkUploadRecord CreateRecord()
		{
			return new eca_StagedBulkUploadRecords();
		}

		#region Methods

		public IStagedBulkUploadRecord ComposeStagedRecordFromTransaction( ICARTtransactionRecord transaction )
		{
			// Construct a Staged Transaction record from the incoming CART Transaction record.

			var newStagedRecord = CreateRecord();

			newStagedRecord.Appropriation = transaction.Appropriation;
			newStagedRecord.CUFFaccount = transaction.CUFFaccount;
			newStagedRecord.FundingSource = transaction.FundingSource;
			newStagedRecord.FundingType = transaction.FundingType;
			newStagedRecord.FiscalYear = transaction.FiscalYear;
			newStagedRecord.TemplateType = transaction.Type;
			newStagedRecord.Amount = transaction.Amount;
			newStagedRecord.Description_Justification = transaction.Description;
			newStagedRecord.IBIS_RequestCode = transaction.IBIS_RequestCode;
			newStagedRecord.ProjectCode = transaction.ProjectCode;
			newStagedRecord.FromCUFFaccount = transaction.FromCUFFaccount;

			return newStagedRecord;
		}

		public IStagedBulkUploadRecord ComposeStagedRecordFromSpendPlanRequest( ISpendPlanRequestRecord spenPlanRequest )
		{
			// Construct a Staged Transaction record from the incoming CART Transaction record.

			var newStagedRecord = CreateRecord();

			newStagedRecord.Appropriation = spenPlanRequest.Appropriation;
			newStagedRecord.CUFFaccount = spenPlanRequest.CUFFaccount;
			newStagedRecord.FundingType = spenPlanRequest.FundingType;
			newStagedRecord.FiscalYear = Convert.ToInt32(spenPlanRequest.FiscalYear);
			newStagedRecord.TemplateType = eca_TemplateTypes.SpendPlanRequest.ToString();
			newStagedRecord.Amount = spenPlanRequest.Amount;
			newStagedRecord.Description_Justification = spenPlanRequest.Justification;
			newStagedRecord.Priority = spenPlanRequest.Priority;
			newStagedRecord.RequestName = spenPlanRequest.Name;

			return newStagedRecord;
		}

		#endregion

	}
}
