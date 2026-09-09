
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;

    public class ContactAdapter : IContactAdapter
    {
        IRepository dbService;
		F3S_ServiceContext serviceContext;

        public ContactAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public string GetEmailFromId(Guid id)
        {
            var systemUserRecord = serviceContext.ContactSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return string.Empty;

            return systemUserRecord.EMailAddress1;
        }

        public EntityReference GetEntityReferenceFromEmail(string emailAddress)
        {
            var systemUserRecord = serviceContext.ContactSet
                                                .Where(a => a.EMailAddress1 == emailAddress).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public string GetCodeFromId(Guid id)
        {
            var systemUserRecord = serviceContext.ContactSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return string.Empty;

            return systemUserRecord.FullName;
        }

        public EntityReference GetEntityReferenceFromCode(string fullName)
        {
            var systemUserRecord = serviceContext.ContactSet
                                                .Where(a => a.FullName == fullName).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var systemUserRecord = serviceContext.ContactSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public IContactRecord CreateRecord()
        {
            return new Contact();
        }
        public IEnumerable<IContactRecord> GetAllRecords()
        {
            return serviceContext.ContactSet as IEnumerable<IContactRecord>;

		}
        public IContactRecord GetRecordFromEntity(Entity entity)
        {
            return entity.ToEntity<Contact>();
        }

    }
}
