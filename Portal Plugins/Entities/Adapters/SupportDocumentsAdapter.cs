
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;

    public class SupportDocumentsAdapter : ISupportDocumentsAdapter
    {
        IRepository dbService;
        F3S_ServiceContext serviceContext;

        public SupportDocumentsAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

		public ISupportDocumentsRecord GetRecordFromId(Guid id)
		{
			return serviceContext.f3s_SupportDocumentsSet.Where(a => a.Id == id).FirstOrDefault();
		}
		public ISupportDocumentsRecord GetRecordFromName(string catalogName)
		{
			return GetAllRecords().Where(a => a.CatalogName == catalogName).FirstOrDefault();
		}

		public ISupportDocumentsRecord CreateRecord()
        {
            return new f3s_SupportDocuments();
        }

        public IEnumerable<ISupportDocumentsRecord> GetAllRecords()
        {
            return serviceContext.f3s_SupportDocumentsSet as IEnumerable<ISupportDocumentsRecord>;
        }

		public ISupportDocumentsRecord GetRecordFromEntity(Entity entity)
		{
			return entity.ToEntity<f3s_SupportDocuments>();
		}

	}
}
