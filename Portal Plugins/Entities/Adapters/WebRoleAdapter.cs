
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;


    public class WebRoleAdapter : IWebRoleAdapter
    {
        IRepository dbService;
        F3S_ServiceContext serviceContext;

        public WebRoleAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public string GetNameFromId(Guid id)
        {
            var webRoleRecord = serviceContext.f3s_WebRolesSet.Where(a => a.Id == id).FirstOrDefault();
            if (webRoleRecord == null) return string.Empty;

            return webRoleRecord.Name;
        }

        public EntityReference GetEntityReferenceFromName(string name)
        {
            var webRoleRecord = serviceContext.f3s_WebRolesSet.Where(a => a.Name == name).FirstOrDefault();
            if (webRoleRecord == null) return null;

            return webRoleRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var webRoleRecord = serviceContext.f3s_WebRolesSet
												.Where(a => a.Id == id).FirstOrDefault();
            if (webRoleRecord == null) return null;

            return webRoleRecord.ToEntityReference();
        }

        public IWebRolesRecord CreateRecord()
        {
#if UNITTEST
            return new f3s_WebRole();
#else
            return null;
#endif
        }
        public IEnumerable<IWebRolesRecord> GetAllRecords()
        {
            return serviceContext.f3s_WebRolesSet as IEnumerable<IWebRolesRecord>;
        }
		public IWebRolesRecord GetRecordFromId(Guid id)
		{
			return serviceContext.f3s_WebRolesSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public IWebRolesRecord GetRecordFromName(string name)
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public IWebRolesRecord GetRecordFromEntity(Entity entity)
        {
            return entity.ToEntity<f3s_WebRoles>();
        }

    }
}
