using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Extensions;

using EXConnect_Plugins.Entities.Interfaces;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.CodeDom;
using System.Threading;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class TransactionYearlyRollupAdapter : ITransactionYearlyRollupAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public TransactionYearlyRollupAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<ITransactionYearlyRollupRecord> GetAllRecords()
		{
			return serviceContext.rcade_YearlyRollupForAppAccAssocsSet as IEnumerable<ITransactionYearlyRollupRecord>;
		}
		public ITransactionYearlyRollupRecord GetRecordFromId( Guid id )
		{
			return serviceContext.rcade_YearlyRollupForAppAccAssocsSet
									.Where(t => t.Id == id).FirstOrDefault();
		}
		public ITransactionYearlyRollupRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public ITransactionYearlyRollupRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<rcade_YearlyRollupForAppAccAssocs>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var rollup = serviceContext.rcade_YearlyRollupForAppAccAssocsSet
									.Where(r => r.Name == name).FirstOrDefault();
			if (rollup == null) return null;

			return rollup.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var rollup = serviceContext.rcade_YearlyRollupForAppAccAssocsSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (rollup == null) return null;

			return rollup.ToEntityReference();
		}

		public ITransactionYearlyRollupRecord CreateRecord()
		{
			var rollupRecord = new rcade_YearlyRollupForAppAccAssocs();
			return rollupRecord;
		}

		public ITransactionYearlyRollupRecord GetRecord( EntityReference fundingType, EntityReference appropriation, EntityReference account,
																	int? fiscalYear, bool createIfNotFound )
		{
			// Set the fiscal Year to current values if they are null
			var givenYear = fiscalYear;
			if ( givenYear == null)
			{
				// Derive the fiscal year from the current date
				var today = dbService.LocalDateTime;
				givenYear = today.FiscalYear();
			}

			var rollup = (ITransactionYearlyRollupRecord) serviceContext.rcade_YearlyRollupForAppAccAssocsSet
										.Where(r => r.rcade_Appropriation.Id == appropriation.Id && r.rcade_CUFFAccount.Id == account.Id
												&& r.rcade_FiscalYear.Value == fiscalYear && r.eca_FundingTypeonRollup.Id == fundingType.Id)
										.FirstOrDefault();
			if (rollup == null && createIfNotFound)
			{
				rollup = CreateRecord();
				rollup.Initialize(fundingType, appropriation, account, givenYear.GetValueOrDefault());
			}

			return rollup;
		}

		/// <summary>
		/// Determine the parent record for the given Monthly Transaction Rollup, and create the missing node if any
		/// </summary>
		/// <param name="monthlyRollup"></param>
		/// <returns></returns>
		public EntityReference GetOrSetRollupParent( ITransactionMonthlyRollupRecord monthlyRollup )
		{
			// The assumption is that the parent of the node that needs to be added or modified,
			// already exists, from a previous event.

			// Find the corresponding parent record from the Monthly Rollup parent's keys
			ITransactionYearlyRollupRecord parentRecord = null;

			if ( monthlyRollup.ParentRollup == null)
			{
				// This is the top parent record
				return null;
			}
			else
			{
				// Get the monthlyRollup's parent and extract the corresponding Yearly Rollup. Create one if not found
				var parentMonthlyRollup = dbService.MonthlyRollup.GetRecordFromId(monthlyRollup.ParentRollup.Id);
				parentRecord = dbService.YearlyRollup.GetRecord(parentMonthlyRollup.FundingType, parentMonthlyRollup.Appropriation,
																parentMonthlyRollup.Account, parentMonthlyRollup.FiscalYear, true);
				if ( parentRecord.ParentRollup == null && parentMonthlyRollup.ParentRollup != null)
				{
					// Get the previous parent from the monthly previous parent
					var previousMonthlyParent = dbService.MonthlyRollup.GetRecordFromId(parentMonthlyRollup.ParentRollup.Id);
					var previousYearlyParent = dbService.YearlyRollup.GetRecord(previousMonthlyParent.FundingType, previousMonthlyParent.Appropriation,
																previousMonthlyParent.Account, previousMonthlyParent.FiscalYear, false); // The reocrd MUST exist

					parentRecord.ParentRollup = previousYearlyParent.Entity.ToEntityReference();
				}
			}

			if (parentRecord != null && parentRecord?.Id == Guid.Empty)
			{
				// Save the record, if created (ID should be null)

				var withSave = false;
#if DESKTOP
				withSave = true;
#endif
				
				parentRecord.Id = dbService.Create(parentRecord.Entity, withSave);
			}

			return parentRecord.Entity.ToEntityReference(); 
		}

		public decimal GetAvailableBalanceForAppropriation( EntityReference appropriationRef, int fiscalYear )
		{
			var balance = 0M;

			// Retrieve the Approved Available Balance for a given Appropriaitno and Fiscal Year

			var fetchXml = $@"
<fetch aggregate='true'>
  <entity name='rcade_yearlyrollupforappaccassocs'>
    <attribute name='eca_approvedavailablebalance' alias='eca_approvedavailablebalance_sum' aggregate='sum' />
    <filter>
      <condition attribute='rcade_appropriation' operator='eq' value='{appropriationRef.Id}' />
      <condition attribute='rcade_fiscalyear' operator='eq' value='{fiscalYear}' />
      <condition attribute='rcade_cuffaccountname' operator='like' value='1.%' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>
";
            try
            {
                var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results?.Entities?.Count > 0)
                {
                    var result = results.Entities[0];
                    if (result.Attributes.Contains("eca_approvedavailablebalance_sum"))
                    {
                        balance = ((Money)((AliasedValue)result["eca_approvedavailablebalance_sum"]).Value).Value;
                    }
                }

            }
            catch (NullReferenceException) 
            {
                //Avoid crashing the entire process due to missing attribute
            }
			
			return balance;
		}

		/// <summary>
		/// Force a recalculate on all rollup fields on the given Yearly Rollup record Id
		/// </summary>
		/// <returns></returns>
		public bool RecomputeRollupFields(Guid rollupId, ITracingService tracer)
		{
			var targetRefs = GetParentReferences(rollupId);

			var fieldNames = new List<string> 
									{ 
										"eca_approvedaoaamountrollup", "eca_pendingaoaamountrollup",
										"eca_approvedcommitmentamountrollup", "eca_pendingcommitmentamountrollup",
										"eca_approvedfinplanrollup", "eca_pendingfinplanrollup",
										"eca_approvedinternaltransfersrollup", "eca_pendinginternaltransferrollup",
										"eca_approvedobligatedamountrollup", "eca_pendingobligatedamountrollup",
									};

			// Contruct the mutliple request for every target and every field
			// Create an ExecuteMutipleRequest object

			var multipleRequest = dbService.GetExecuteMultipleRequest();

			foreach (var target in targetRefs)
			{
				foreach (var fieldName in fieldNames)
				{
					var recalcRequest = new CalculateRollupFieldRequest
					{
						Target = target,
						FieldName = fieldName
					};
					multipleRequest.Requests.Add(recalcRequest);
				}
			}

			if (multipleRequest.Requests.Count > 0)
			{
				// Retry for 10 seconds to see if the request succeeds
				var iteration = 1;
				var response = new ExecuteMultipleResponse();
				while (iteration < 10)
				{
					iteration++;
					// Execute the request
					response = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
					if (response.IsFaulted)
					{
						tracer.Trace("The recalculation request failed, sleeping...");
						Thread.Sleep(1000); // Wait 1 second and try again
					}
					else
					{
						break;
					}
				}
				return !response.IsFaulted;
			}
			else
			{
				return true;
			}
		}

		private EntityReferenceCollection GetParentReferences( Guid rollupId )
		{
			// Retrieve the list of Parent Entities
			var parentRefs = new EntityReferenceCollection();

			var fetchXml = $@"
<fetch>
  <entity name='rcade_yearlyrollupforappaccassocs'>
    <attribute name='rcade_yearlyrollupforappaccassocsid' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='rcade_yearlyrollupforappaccassocsid' operator='eq-or-above' value='{rollupId}' />
    </filter>
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					parentRefs.Add(entity.ToEntityReference());
				}
			}

			return parentRefs;
		}

	}

}
