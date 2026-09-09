
using Microsoft.Xrm.Sdk;

using System;
using System.Linq;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;
using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Entities.Adapters
{
	public class SystemUserAdapter : ISystemUserAdapter
    {
        IRepository dbService;
		F3S_ServiceContext serviceContext;

        public SystemUserAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public string GetEmailFromId(Guid id)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return string.Empty;

            return systemUserRecord.InternalEMailAddress;
        }

        public EntityReference GetEntityReferenceFromEmail(string emailAddress)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.InternalEMailAddress == emailAddress).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public string GetCodeFromId(Guid id)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return string.Empty;

            return systemUserRecord.FullName;
        }

        public Guid? GetIdFromCode(string fullName)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.FullName == fullName).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.Id;
        }

        public EntityReference GetEntityReferenceFromCode(string fullName)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.FullName == fullName).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return null;

            return systemUserRecord.ToEntityReference();
        }

        public bool IsUserActive(Guid id)
        {
            var systemUserRecord = serviceContext.SystemUserSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (systemUserRecord == null) return false;

            return !systemUserRecord.IsDisabled.GetValueOrDefault();
        }

        public bool IsUserInRole(Guid userId, string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
