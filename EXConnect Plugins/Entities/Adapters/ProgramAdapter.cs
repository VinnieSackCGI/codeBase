using System.Collections.Generic;
using System;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class ProgramAdapter : IProgramAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public ProgramAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IProgramRecord CreateRecord()
		{
			return new eca_Program();
		}

		public IEnumerable<IProgramRecord> GetAllRecords()
		{
			return serviceContext.eca_ProgramSet as IEnumerable<IProgramRecord>;
		}

		public IProgramRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_Program>();
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

