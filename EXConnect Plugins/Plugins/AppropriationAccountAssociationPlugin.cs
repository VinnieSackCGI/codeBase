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
	public class AppropriationAccountAssociationPlugin
	{
		IRepository dbService;
		ITracingService tracer;

		public AppropriationAccountAssociationPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(EntityReference appropriationRef)
		{
#else

    [CrmPluginRegistration(MessageNameEnum.Create,
	"rcade_appropriation",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Appropriation to CUFF Accounts Association On Create", 1,
	IsolationModeEnum.Sandbox,
	Description = "Associates an Appropriation to all CUFF Accounts upon creation of an Appropriation.",
	Id = "8cdd95a3-9194-4cd4-94c9-d532c0440818")]

	public class AppropriationAccountAssociationPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

            tracer.Trace("Beginning execution of Appropriation/Account Association plugin");

			// Get the appropriation record object from the target entity
			var appropriationRef = context.GetTargetEntityReference();
#endif
            
			// Get the list of CUFF Accounts in the system 
            var cuffAccounts = GetActiveCUFFAccounts();

			tracer.Trace($"Found \"{cuffAccounts.Count}\" CUFF Account records.");

			if (cuffAccounts.Count > 0)
			{
                // Associate the incoming Appropriation with all CUFF Accounts

                var relatedAccounts = new EntityReferenceCollection();
				foreach (var accountId in cuffAccounts) 
				{
					relatedAccounts.Add(new EntityReference("rcade_cuffaccount", accountId));
				}

                var request = new AssociateRequest
                {
                    Target = appropriationRef,
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
		private IList<Guid> GetActiveCUFFAccounts()
		{
			// Retrieve the list of 
			var accountsList = new List<Guid>();

			var fetchXml = @"
<fetch>
  <entity name='rcade_cuffaccount'>
    <attribute name='rcade_cuffaccountid' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='rcade_cuffaccountname' operator='ne' value='UNA' />
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
