using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
    using Interfaces;

    public class AppropriationYearlyRollupnAdapter : IAppropriationYearlyRollupAdapter
    {
        IRepository dbService;
        EXConnect_ServiceContext serviceContext;

        public AppropriationYearlyRollupnAdapter( IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
        }

        public IEnumerable<IAppropriationYearlyRollupRecord> GetAllRecords()
        {
            return serviceContext.rcade_AppropriationYearlyRollupSet as IEnumerable<IAppropriationYearlyRollupRecord>;
        }
        public IAppropriationYearlyRollupRecord GetRecordFromId(Guid id)
        {
            return serviceContext.rcade_AppropriationYearlyRollupSet.Where(a => a.Id == id).FirstOrDefault();
        }
		public IAppropriationYearlyRollupRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(a => a.Name == name).FirstOrDefault();
		}
		public IAppropriationYearlyRollupRecord GetRecordFromKeys( int fiscalYear, EntityReference appropriation )
		{
            return GetAllRecords().Where(a => a.Appropriation.Id == appropriation.Id && a.FiscalYear.GetValueOrDefault() == fiscalYear).FirstOrDefault();
		}
		public IAppropriationYearlyRollupRecord GetRecordFromEntity(Entity entity)
        {
            return entity.ToEntity<rcade_AppropriationYearlyRollup>();
        }

        public EntityReference GetEntityReferenceFromName(string name)
        {
            var rollupRecord = serviceContext.rcade_AppropriationYearlyRollupSet.Where(a => a.Name == name).FirstOrDefault();
            if (rollupRecord == null) return null;

            return rollupRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var rollupRecord = serviceContext.rcade_AppropriationYearlyRollupSet
												.Where(a => a.Id == id).FirstOrDefault();
            if (rollupRecord == null) return null;

            return rollupRecord.ToEntityReference();
        }

        public IAppropriationYearlyRollupRecord CreateRecord()
        {
            return new rcade_AppropriationYearlyRollup();
        }

    }
}
