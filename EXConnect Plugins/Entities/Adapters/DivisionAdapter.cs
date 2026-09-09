using System.Collections.Generic;
using System;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class DivisionAdapter : IDivisionAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public DivisionAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IDivisionRecord CreateRecord()
		{
			return new eca_Division();
		}

		public IEnumerable<IDivisionRecord> GetAllRecords()
		{
			return serviceContext.eca_DivisionSet as IEnumerable<IDivisionRecord>;
		}

		public IDivisionRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_Division>();
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
