using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace PluginsRunner_CommonLibrary
{
    public class RunnerServiceProvider : IServiceProvider
    {
        private IPluginExecutionContext context;
        private IOrganizationServiceFactory organizationServiceFactory;
        private ITracingService tracingService;

        public RunnerServiceProvider(IPluginExecutionContext context,IOrganizationServiceFactory organizationServiceFactory, ITracingService tracingService)
        {
            this.context = context;
            this.organizationServiceFactory = organizationServiceFactory;
            this.tracingService = tracingService;
        }
        public object GetService(Type serviceType)
        {
            return this.typeToObject[serviceType];
        }


        private Dictionary<Type, object> typeToObject => new Dictionary<Type, object>()
        {
            {typeof(IPluginExecutionContext), this.context },
            {typeof(IOrganizationServiceFactory), this.organizationServiceFactory },
            {typeof(ITracingService), this.tracingService }
        };
    }
}
