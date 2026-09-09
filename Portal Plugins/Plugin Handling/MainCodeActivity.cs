
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Entities.Interfaces;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Plugin_Handling
{
    using Services;

    public abstract class MainCodeActivity : BaseCodeActivity
    {
        protected IRepository dbService;

        protected MainCodeActivity() : base() {}

		protected override IBaseRepository GetDbService( IOrganizationService orgService, ITracingService tracer )
		{
			dbService = new BusinessDbService(orgService, tracer);
			return dbService;
		}

	}
}
