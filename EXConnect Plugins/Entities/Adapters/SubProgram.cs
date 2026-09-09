using System.Collections.Generic;
using System;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class SubProgramAdapter : ISubProgramAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public SubProgramAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public ISubProgramRecord CreateRecord()
		{
			return new eca_Subprogram();
		}

		public IEnumerable<ISubProgramRecord> GetAllRecords()
		{
			return serviceContext.eca_SubprogramSet as IEnumerable<ISubProgramRecord>;
		}

		public ISubProgramRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_Subprogram>();
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
