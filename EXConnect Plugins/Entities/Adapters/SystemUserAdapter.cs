using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	using Interfaces;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Query;

    public class SystemUserAdapter : ISystemUserAdapter
    {
        IRepository dbService;
        EXConnect_ServiceContext serviceContext;

        public SystemUserAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
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

        public bool IsUserInRole( Guid userId, string roleName )
        {
			var fetchXml = $@"
    <fetch aggregate='true'>
      <entity name='teammembership'>
        <attribute name='teammembershipid' alias='count' aggregate='count' />
        <filter>
          <condition attribute='systemuserid' operator='eq' value='{userId}' />
        </filter>
        <link-entity name='team' to='teamid' from='teamid' alias='T' link-type='inner'>
          <filter>
            <condition attribute='name' operator='like' value='{roleName}' />
          </filter>
        </link-entity>
      </entity>
    </fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				// Expecting only a count, so let's get to it
				var entity = results.Entities[0];
				if (entity.Attributes.Contains("count"))
				{
					if ((int)((AliasedValue)entity["count"]).Value > 0)
					{
						return true;
					}
				}
			}

			return false;

		}

	}
}
