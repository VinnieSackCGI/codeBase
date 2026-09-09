using System;
using System.Activities;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

using Plugins_CommonLibrary.Entities.Interfaces;
using Plugins_CommonLibrary.Extensions;

namespace Plugins_CommonLibrary.Plugin_Handling
{

	public abstract class BaseCodeActivity : CodeActivity
    {
        protected IBaseRepository baseDbService;
        protected ITracingService tracer;
        protected DateTime dateTimeLocal;
        protected Guid executingUserId;
        protected IOrganizationService orgService;

        protected virtual void InitializeContext(CodeActivityContext context, Guid? userId = null)
        {
            // Getting OrganizationService from Context  
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();

            tracer = context.GetExtension<ITracingService>();

            executingUserId = workflowContext.UserId;

            tracer.Trace($"User ID for the service is \"{userId}\" Executing UserID is \"{executingUserId}\".");
            if (userId != null && executingUserId != userId.Value)
            {
                executingUserId = userId.GetValueOrDefault();
                tracer.Trace($" --> Replacing ID for service with \"{executingUserId}\".");
            }

            Guid? modUserId = executingUserId;
            if (modUserId == Guid.Empty)
            {
                modUserId = null;
            }

            var orgService = serviceFactory.CreateOrganizationService(modUserId);

            // =================================================
            // Composition Root
            // -------------------------------------------------
            // Initialize the context required for the CodeActivity
            //  .. Database service which created at the BusinessDbService
            baseDbService = GetDbService(orgService, tracer);

            var now = DateTime.Now;
            dateTimeLocal = orgService.RetrieveLocalTimeFromUTCTime(now);
            baseDbService.LocalDateTime = dateTimeLocal;
            baseDbService.UserId = executingUserId;
        }

        protected abstract IBaseRepository GetDbService(IOrganizationService orgService, ITracingService tracer);
    }
}
