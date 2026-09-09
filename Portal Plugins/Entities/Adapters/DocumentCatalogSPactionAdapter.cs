
using Microsoft.Xrm.Sdk;

using System.Collections.Generic;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;


    public class DocumentCatalogSPactionAdapter : IDocumentCatalogSPactionAdapter
    {
        IRepository dbService;
		F3S_ServiceContext serviceContext;

		public DocumentCatalogSPactionAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public IDocumentCatalogSPactionRecord CreateRecord()
        {
            return new f3s_DocumentCatalogSPAction();
        }

        public IEnumerable<IDocumentCatalogSPactionRecord> GetAllRecords()
        {
            return serviceContext.f3s_DocumentCatalogSPActionSet as IEnumerable<IDocumentCatalogSPactionRecord>;
        }

		public IDocumentCatalogSPactionRecord GetRecordFromEntity(Entity entity)
		{
			return entity.ToEntity<f3s_DocumentCatalogSPAction>();
		}

	}
}
