
using Microsoft.Xrm.Sdk;

using System.Collections.Generic;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;


    public class SupportDocumentsUserSPactionAdapter : ISupportDocumentsUserSPactionAdapter
	{
        IRepository dbService;
		F3S_ServiceContext serviceContext;

		public SupportDocumentsUserSPactionAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public ISupportDocumentsUserSPactionRecord CreateRecord()
        {
            return new f3s_SupportDocumentsUserSPAction();
        }

        public IEnumerable<ISupportDocumentsUserSPactionRecord> GetAllRecords()
        {
            return serviceContext.f3s_SupportDocumentsUserSPActionSet as IEnumerable<ISupportDocumentsUserSPactionRecord>;
        }

		public ISupportDocumentsUserSPactionRecord GetRecordFromEntity(Entity entity)
		{
			return entity.ToEntity<f3s_SupportDocumentsUserSPAction>();
		}

	}
}
