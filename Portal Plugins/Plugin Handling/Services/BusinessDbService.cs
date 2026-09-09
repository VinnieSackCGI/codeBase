using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Services;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using Portal_Plugins.Entities;
using Portal_Plugins.Entities.Adapters;
using Portal_Plugins.Entities.Adapters.Interfaces;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Plugin_Handling.Services
{

    public class BusinessDbService : BaseBusinessDbService, IRepository
    {
        public BusinessDbService(IOrganizationService organizationService, ITracingService tracer) 
				: base(organizationService, tracer)
        {
            // Create the service context for the OBO database 
            base.OrgServiceContext = new F3S_ServiceContext(organizationService);
        }

        #region Data Retrieval

        #region System Tables

        private ISystemUserAdapter systemUserTable;
        public ISystemUserAdapter SystemUser
        {
            get
            {
                if (systemUserTable == null)
                {
                    systemUserTable = new SystemUserAdapter(this);
                }
                return systemUserTable;
            }
        }

        private IContactAdapter contactTable;

        public IContactAdapter Contact
        {
            get
            {
                if (contactTable == null)
                {
                    contactTable = new ContactAdapter(this);
                }
                return contactTable;
            }
        }

        private ITeamAdapter teamTable;

        public ITeamAdapter Team
        {
            get
            {
                if (teamTable == null)
                {
                    teamTable = new TeamAdapter(this);
                }
                return teamTable;
            }
        }

		/*
				private IEnvironmentVariablesAdapter environmentVariablesTable;

				public IEnvironmentVariablesAdapter EnvironmentVariables
				{
					get
					{
						if (environmentVariablesTable == null)
						{
							environmentVariablesTable = new EnvironmentVariablesAdapter(this);
						}
						return environmentVariablesTable;
					}
				}
		*/

		#endregion

		private IWebRoleAdapter webRoleTable;
		public IWebRoleAdapter WebRole
		{
			get
			{
				if (webRoleTable == null)
				{
					webRoleTable = new WebRoleAdapter(this);
				}
				return webRoleTable;
			}
		}

		private IPortalWebRoleAdapter portalWebRoleTable;
		public IPortalWebRoleAdapter PortalWebRole
		{
			get
			{
				if (portalWebRoleTable == null)
				{
					portalWebRoleTable = new PortalWebRoleAdapter(this, tracer);
				}
				return portalWebRoleTable;
			}
		}

		private ISharePointDocumentLocationAdapter spDocLocationTable;
		public ISharePointDocumentLocationAdapter SPDocumentLocation
		{
			get
			{
				if (spDocLocationTable == null)
				{
					spDocLocationTable = new SharePointDocumentLocationAdapter(this);
				}
				return spDocLocationTable;
			}
		}

		private ISupportDocumentsAdapter supportDocTable;
		public ISupportDocumentsAdapter SupportDoc
		{
			get
			{
				if (supportDocTable == null)
				{
					supportDocTable = new SupportDocumentsAdapter(this);
				}
				return supportDocTable;
			}
		}

		private IDocumentCatalogSPactionAdapter catalogSPactionTable;
		public IDocumentCatalogSPactionAdapter CatalogSPaction
		{
			get
			{
				if (catalogSPactionTable == null)
				{
					catalogSPactionTable = new DocumentCatalogSPactionAdapter(this);
				}
				return catalogSPactionTable;
			}
		}

		private ISupportDocumentsUserSPactionAdapter userSPactionTable;

        public ISupportDocumentsUserSPactionAdapter UserSPaction
		{
			get
			{
				if (userSPactionTable == null)
				{
					userSPactionTable = new SupportDocumentsUserSPactionAdapter(this);
				}
				return userSPactionTable;
			}
		}

		#endregion
	}
}
