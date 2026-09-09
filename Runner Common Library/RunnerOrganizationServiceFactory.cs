using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsRunner_CommonLibrary
{
    public class RunnerOrganizationServiceFactory : IOrganizationServiceFactory
    {
        private IOrganizationService testingOrganziationService;
        public RunnerOrganizationServiceFactory(IOrganizationService organizationService)
        {
            this.testingOrganziationService = organizationService;
        }
        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return testingOrganziationService;
        }
    }
}
