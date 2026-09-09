
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;


    public class SharePointDocumentLocationAdapter : ISharePointDocumentLocationAdapter
    {
        IRepository dbService;
		F3S_ServiceContext serviceContext;

        public SharePointDocumentLocationAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public ISharePointDocumentLocationRecord GetRecordFromId(Guid id)
        {
            return serviceContext.SharePointDocumentLocationSet.Where(a => a.Id == id).FirstOrDefault();
        }

        public ISharePointDocumentLocationRecord CreateRecord()
        {
            return new SharePointDocumentLocation();
        }

        public IEnumerable<ISharePointDocumentLocationRecord> GetAllRecords()
        {
            return serviceContext.SharePointDocumentLocationSet as IEnumerable<ISharePointDocumentLocationRecord>;
        }

		public ISharePointDocumentLocationRecord GetRecordFromRelativeUrl(string url)
		{
			return GetAllRecords().Where(t => t.RelativeUrl == url).FirstOrDefault();
		}
		public ISharePointDocumentLocationRecord GetRecordFromEntity(Entity entity)
		{
			return entity.ToEntity<SharePointDocumentLocation>();
		}
		public ISharePointDocumentLocationRecord GetRecordFromDocumentId(Guid docId)
		{
			return GetAllRecords().Where(t => t.RegardingObjectId?.Id == docId).FirstOrDefault();
		}

	}
}
