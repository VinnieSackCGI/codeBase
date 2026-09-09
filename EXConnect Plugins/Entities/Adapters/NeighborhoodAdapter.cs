using System.Collections.Generic;
using System;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class NeighborhoodAdapter : INeighborhoodAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public NeighborhoodAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public INeighborhoodRecord CreateRecord()
		{
			return new eca_Neighborhood();
		}

		public IEnumerable<INeighborhoodRecord> GetAllRecords()
		{
			return serviceContext.eca_NeighborhoodSet as IEnumerable<INeighborhoodRecord>;
		}

		public INeighborhoodRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_Neighborhood>();
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
	}
}
