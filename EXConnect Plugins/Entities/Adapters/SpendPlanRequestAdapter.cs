using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	using Interfaces;

	public class SpendPlanRequestAdapter : ISpendPlanRequestAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public SpendPlanRequestAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<ISpendPlanRequestRecord> GetAllRecords()
		{
			return serviceContext.eca_FormulationRequestSet as IEnumerable<ISpendPlanRequestRecord>;
		}
		public ISpendPlanRequestRecord GetRecordFromId( Guid id )
		{
			return serviceContext.eca_FormulationRequestSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public ISpendPlanRequestRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public ISpendPlanRequestRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_FormulationRequest>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var sprRecord = serviceContext.eca_FormulationRequestSet.Where(a => a.Name == name).FirstOrDefault();
			if (sprRecord == null) return null;

			return sprRecord.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var sprRecord = serviceContext.eca_FormulationRequestSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (sprRecord == null) return null;

			return sprRecord.ToEntityReference();
		}

		public ISpendPlanRequestRecord CreateRecord()
		{
			return new eca_FormulationRequest();
		}

		public ISpendPlanRequestRecord ComposeSpendPlanRequestRecordFromStaged( IStagedBulkUploadRecord stagedRecord )
		{
			// Construct a Spend Plan Request record from the incoming Staged transaction record.

			var newRecord = CreateRecord();

			newRecord.Appropriation = stagedRecord.Appropriation;
			newRecord.CUFFaccount = stagedRecord.CUFFaccount;
			newRecord.FundingType = stagedRecord.FundingType;
			newRecord.FiscalYear = stagedRecord.FiscalYear.ToString();
			newRecord.Type = stagedRecord.TemplateType;
			newRecord.Name = stagedRecord.RequestName;
			newRecord.Justification = stagedRecord.Description_Justification;
			newRecord.Amount = stagedRecord.Amount;

			return newRecord;
		}

		#region Private Methods

		#endregion
	}
}
