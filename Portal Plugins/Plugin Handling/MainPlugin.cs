using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Entities.Interfaces;

using Portal_Plugins.Entities.Interfaces;
using Portal_Plugins.Plugin_Handling.Services;

namespace Portal_Plugins.Plugin_Handling
{
	public abstract class MainPlugin : BasePlugin
    {
        protected IRepository dbService;

        protected MainPlugin() : base()
        {
        }

        protected override IBaseRepository GetDbService(IOrganizationService orgService)
        {
            dbService = new BusinessDbService(orgService, tracer);
            return dbService;
        }
    }
}
