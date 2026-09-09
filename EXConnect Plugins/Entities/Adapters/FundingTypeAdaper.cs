using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class FundingTypeAdapter : IFundingTypeAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public FundingTypeAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IFundingTypeRecord CreateRecord()
		{
			return new eca_FundingType();
		}

		public IEnumerable<IFundingTypeRecord> GetAllRecords()
		{
			return serviceContext.eca_FundingTypeSet as IEnumerable<IFundingTypeRecord>;
		}

		public IFundingTypeRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_FundingType>();
		}

		public string GetNameFromId( Guid id )
		{
			var approp = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (approp == null)
			{
				return string.Empty;
			}
			return approp.Name;
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var entity = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (entity == null)
			{
				return null;
			}
			var entityRef = entity.Entity.ToEntityReference();
			entityRef.Name = entity.Name;

			return entityRef;
		}
		public EntityReference GetEntityReferenceFromName( string name )
		{
			var entity = GetAllRecords().Where(a => a.Name == name && a.IsActive).FirstOrDefault();
			return GetEntityReferenceFromEntity(entity);	
		}

		private EntityReference GetEntityReferenceFromEntity( IFundingTypeRecord record )
		{
			if (record == null)
			{
				return null;
			}

			var entityRef = record.Entity.ToEntityReference();
			entityRef.Name = record.Name;

			return entityRef;

		}

		public EntityReference GetExtendedEntityReference( EntityReference fundingType )
		{
			if (string.IsNullOrEmpty(fundingType.Name))
			{
				fundingType.Name = GetNameFromId(fundingType.Id);
			}

			return fundingType;
		}
	}

}

