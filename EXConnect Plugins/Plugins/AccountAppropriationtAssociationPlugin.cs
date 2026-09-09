using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins
{

#if DESKTOP 

	// This block allows to debug the logic of the plugin directly from the Desktip Interface project
	public class AccountAppropriationAssociationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public AccountAppropriationAssociationPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(EntityReference accountRef)
		{
#else

    [CrmPluginRegistration(MessageNameEnum.Create,
	"rcade_cuffaccount",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"CUFF Account to Appropriations Association On Create", 1,
	IsolationModeEnum.Sandbox,
	Description = "Associates a CUFF Account to all Appropriations upon creation of a CUFF Account.",
	Id = "b210fe62-fe8c-4029-8cb9-92955a38f515")]

	public class AccountAppropriationAssociationPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

            tracer.Trace("Beginning execution of Account/Appropriation Association plugin");

			// Get the Contact user record object from the target entity
			var accountRef = context.GetTargetEntityReference();
#endif

            // Get the list of Appropriations in the system 
            var appropriations = GetActiveAppropriations();

			tracer.Trace($"Found \"{appropriations.Count}\" Appropriation records.");

			if (appropriations.Count > 0)
			{
                // Associate the incoming Account with all Appropriations

                var relatedAccounts = new EntityReferenceCollection();
				foreach (var appropId in appropriations) 
				{
					relatedAccounts.Add(new EntityReference("rcade_appropriation", appropId));
				}

                var request = new AssociateRequest
                {
                    Target = accountRef,
                    RelatedEntities = relatedAccounts,
					Relationship = new Relationship("rcade_CUFFAccount_rcade_Appropriation_rca")
                };
				try
				{
                    var result = dbService.OrgServiceContext.Execute(request);
                }
				catch(Exception e)
				{
					tracer.Trace(e.Message); 
					throw (new InvalidPluginExecutionException(e.Message));
				}

            }

        }

		#region Auxiliary Methods

		/// <summary>
		/// Retreive the list of active CUFF Accounts record Ids
		/// </summary>
		/// <returns></returns>
		private IList<Guid> GetActiveAppropriations()
		{
			// Retrieve the list of 
			var accountsList = new List<Guid>();

			var fetchXml = @"
<fetch>
  <entity name='rcade_appropriation'>
    <attribute name='rcade_appropriationid' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

            var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Count > 0)
            {
                foreach (var entity in results.Entities)
                {
                    if (entity.Id != null)
                    {
						accountsList.Add(entity.Id);
                    }
                }
            }
            else
            {
                tracer.Trace("No records returned during retrieval of CUFF Accounts records list.");
            }

            return accountsList;

        }

		#endregion

	}
}
