using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class AppropriationAdapter : IAppropriationAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public AppropriationAdapter(IRepository dbService)
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IAppropriationRecord CreateRecord()
		{
			return new rcade_Appropriation();
		}

		public IEnumerable<IAppropriationRecord> GetAllRecords()
		{
			return serviceContext.rcade_AppropriationSet as IEnumerable<IAppropriationRecord>;
		}

		public IAppropriationRecord GetRecordFromId(Guid id)
		{
			return serviceContext.rcade_AppropriationSet.Where(a => a.Id == id).FirstOrDefault();
		}

		public IAppropriationRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<rcade_Appropriation>();
		}

		public string GetCodeFromId( Guid id ) 
		{
			var approp = GetAllRecords().Where( a => a.Id == id ).FirstOrDefault();
			if (approp == null)
			{
				return string.Empty;
			}
			return approp.Code;
		}

		public EntityReference GetExtendedEntityReference( EntityReference appropRef )
		{
			if (string.IsNullOrEmpty(appropRef.Name))
			{
				appropRef.Name = GetCodeFromId(appropRef.Id);
			}

			return appropRef;
		}

		public EntityReference GetEntityReferenceFromCode( string code )
		{
			var approp = GetAllRecords().Where(a => a.Code == code && a.IsActive).FirstOrDefault();
			if (approp == null)
			{
				return null;
			}
			return approp.Entity.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var approp = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (approp == null)
			{
				return null;
			}
			return approp.Entity.ToEntityReference();
		}

	}
}
