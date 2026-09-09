
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;

    public class PortalWebRoleAdapter : IPortalWebRoleAdapter
    {

        IRepository dbService;
		F3S_ServiceContext serviceContext;
        ITracingService tracer;

        public PortalWebRoleAdapter(IRepository dbService, ITracingService tracer)
        {
            this.dbService = dbService;
            this.tracer = tracer;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public string GetNameFromId(Guid id)
        {
            var webRoleRecord = serviceContext.mspp_webroleSet.Where(a => a.Id == id).FirstOrDefault();
            if (webRoleRecord == null) return string.Empty;

            return webRoleRecord.Name;
        }

        public EntityReference GetEntityReferenceFromName(string name)
        {
            var webRoleRecord = serviceContext.mspp_webroleSet.Where(a => a.Name == name).FirstOrDefault();
            if (webRoleRecord == null) return null;

            return webRoleRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var webRoleRecord = serviceContext.mspp_webroleSet
												.Where(a => a.Id == id).FirstOrDefault();
            if (webRoleRecord == null) return null;

            return webRoleRecord.ToEntityReference();
        }

        public IPortalWebRoleRecord CreateRecord()
        {
            return new mspp_webrole();
        }
        public IEnumerable<IPortalWebRoleRecord> GetAllRecords()
        {
            return serviceContext.mspp_webroleSet as IEnumerable<IPortalWebRoleRecord>;
        }
		public IPortalWebRoleRecord GetRecordFromId(Guid id)
		{
			return serviceContext.mspp_webroleSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public IPortalWebRoleRecord GetRecordFromName(string name)
		{
            // Make sure we are looking for Web Roles in the F3S Portal (just in case)
            if (F3S_WebSiteRef != null)
            {
                return GetAllRecords().Where(r => r.Name == name && r.WebsiteRef.Id == F3S_WebSiteRef.Id).FirstOrDefault();
            }
            return null;
		}
		public IPortalWebRoleRecord GetRecordFromEntity(Entity entity)
        {
            return entity.ToEntity<mspp_webrole>();
        }

        private static EntityReference F3SwebSiteRef = null;
        public EntityReference F3S_WebSiteRef
        {
            get
            {
				if (F3SwebSiteRef == null)
				{
					var fetchXml = $@"
<fetch>
  <entity name='mspp_website'>
    <attribute name='mspp_websiteid' />
    <filter>
      <condition attribute='mspp_name' operator='eq' value='{Configuration.Data.PortalName}' />
    </filter>
  </entity>
</fetch>";
					var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));
					if (results.Entities.Count > 0 && results.Entities[0].Id != null)
					{
                        tracer.Trace($"{Configuration.Data.PortalName} Portal was found...");
						F3SwebSiteRef = new EntityReference("mspp_website", results.Entities[0].Id);
					}
                    else
                    {
						tracer.Trace($"{Configuration.Data.PortalName} Portal was NOT found...");
					}
				}
				return F3SwebSiteRef;
			}
		}

    }
}
