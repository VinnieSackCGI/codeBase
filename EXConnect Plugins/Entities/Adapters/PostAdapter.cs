using System.Collections.Generic;
using System;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class PostAdapter : IPostAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public PostAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IPostRecord CreateRecord()
		{
			return new rcade_Post();
		}

		public IEnumerable<IPostRecord> GetAllRecords()
		{
			return serviceContext.rcade_PostSet as IEnumerable<IPostRecord>;
		}

		public IPostRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<rcade_Post>();
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
		public IPostRecord GetRecordFromId( Guid id )
		{
			return serviceContext.rcade_PostSet.Where(a => a.Id == id).FirstOrDefault();
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
			var entity = GetAllRecords().Where(a => a.Name == name).FirstOrDefault();
			return GetEntityReferenceFromEntity(entity);
		}

		private EntityReference GetEntityReferenceFromEntity( IPostRecord record )
		{
			if (record == null)
			{
				return null;
			}

			var entityRef = record.Entity.ToEntityReference();
			entityRef.Name = record.Name;

			return entityRef;

		}

		public EntityReference GetExtendedEntityReference( EntityReference postRef )
		{
			if (string.IsNullOrEmpty(postRef.Name))
			{
				postRef.Name = GetNameFromId(postRef.Id);
			}

			return postRef;
		}

	}
}
