using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins_CommonLibrary.Extensions;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class TransactionMonthlyRollupAdapter : ITransactionMonthlyRollupAdapter
	{
		IRepository dbService;
		ITracingService tracer;
		EXConnect_ServiceContext serviceContext;

		public TransactionMonthlyRollupAdapter( IRepository dbService, ITracingService tracer )
		{
			this.dbService = dbService;
			this.tracer = tracer;

			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<ITransactionMonthlyRollupRecord> GetAllRecords()
		{
			return serviceContext.rcade_MonthlyRollupForAppAccAssocsSet as IEnumerable<ITransactionMonthlyRollupRecord>;
		}
		public ITransactionMonthlyRollupRecord GetRecordFromId( Guid id )
		{
			return serviceContext.rcade_MonthlyRollupForAppAccAssocsSet
									.Where(t => t.Id == id).FirstOrDefault();
		}
		public ITransactionMonthlyRollupRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public ITransactionMonthlyRollupRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<rcade_MonthlyRollupForAppAccAssocs>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var rollup = serviceContext.rcade_MonthlyRollupForAppAccAssocsSet
									.Where(r => r.Name == name).FirstOrDefault();
			if (rollup == null) return null;

			return rollup.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var rollup = serviceContext.rcade_MonthlyRollupForAppAccAssocsSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (rollup == null) return null;

			return rollup.ToEntityReference();
		}

		public ITransactionMonthlyRollupRecord CreateRecord()
		{
			var rollupRecord = new rcade_MonthlyRollupForAppAccAssocs();
			return rollupRecord;
		}

		public ITransactionMonthlyRollupRecord GetRecord( Guid id, EntityReference fundingType, EntityReference appropriation, EntityReference account,
																	int? fiscalMonth, int? fiscalYear, bool createIfNotFound )
		{
			// Set both the fiscal Year and/or Month to current values if they are null
			var givenMonth = fiscalMonth;
			var givenYear = fiscalYear;
			if (givenMonth == null || givenYear == null)
			{
				// Derive the fiscal month and/or year from the current date
				var today = dbService.LocalDateTime;
				if (givenMonth == null)
				{
					givenMonth = today.MonthOfFiscalYear();
				}
				if (givenYear == null)
				{
					givenYear = today.FiscalYear();
				}
			}

			tracer.Trace($"MonthlyRollup GetRecord called for requestorId \"{id}\", Funding Type \"{fundingType.Name}\", Appropriation \"{appropriation.Name}\", account \"{account.Name}\", Fiscal Year \"{givenYear}\", and Fiscal Month \"{givenMonth}\".");

			var record = (ITransactionMonthlyRollupRecord) serviceContext.rcade_MonthlyRollupForAppAccAssocsSet
											.Where(r => r.rcade_Appropriation.Id == appropriation.Id && r.rcade_CUFFAccount.Id == account.Id 
													&& r.rcade_FiscalYear == givenYear && r.eca_FundingTypeonRollup.Id == fundingType.Id
													&& r.eca_FiscalMonth == givenMonth)
											.FirstOrDefault();

			if (record == null && createIfNotFound)
			{
				tracer.Trace("No records found for requested parameters. A record will be created.");

				// Repeat the search in case the record is now created
				record = (ITransactionMonthlyRollupRecord)serviceContext.rcade_MonthlyRollupForAppAccAssocsSet
															.Where(r => r.rcade_Appropriation.Id == appropriation.Id && r.rcade_CUFFAccount.Id == account.Id
																	&& r.rcade_FiscalYear == givenYear && r.eca_FundingTypeonRollup.Id == fundingType.Id
																	&& r.eca_FiscalMonth == givenMonth)
															.FirstOrDefault(); 
				if (record == null)
				{
					record = CreateRecord();
					record.Initialize(fundingType, appropriation, account, givenMonth.Value, givenYear.Value);
				}
			}
			return record;
		}

		/// <summary>
		/// Determine the parent record for the given transaction, and create the missing nodes if any
		/// </summary>
		/// <param name="transaction"></param>
		/// <returns></returns>
		public EntityReference GetOrSetTransactionRollupParent(TransferDirection toFrom, ICARTtransactionRecord transaction, int fiscalMonth, int fiscalYear )
		{
			// Query the CUFF Account table to extract all related records for the transaction's CUFF Account
			var parentTree = (toFrom == TransferDirection.To ? GetParentCUFFAccountNodes(transaction.CUFFaccount.Id)
															 : GetParentCUFFAccountNodes(transaction.FromCUFFaccount.Id));
			if (parentTree == null)
			{
				var msg = "The parent tree structure for the specified CUFF Account cannot be found. Please review the CUFF Account structure before proceeding.";
				throw new InvalidPluginExecutionException(OperationStatus.Failed, msg);
			}

			// Prepare the EntityReferences so they have a Name in case we need to create new Records

			var appropRef = dbService.Appropriation.GetExtendedEntityReference(transaction.Appropriation);
			var fundingTypeRef = dbService.FundingType.GetExtendedEntityReference(transaction.FundingType);
			var accountRef = (toFrom == TransferDirection.To ? dbService.CUFFaccount.GetExtendedEntityReference(transaction.CUFFaccount)
															 : dbService.CUFFaccount.GetExtendedEntityReference(transaction.FromCUFFaccount));

			// Add parent records if they to do exist starting from the top level
			int nodeProcessed = 0;
			Guid? accountParentNode = null;
			EntityReference previousParentRef = null;

			while ( nodeProcessed < parentTree.Count )
			{
				var node = parentTree.Where(n => n.ParentId == accountParentNode).First();

				var nodeAccount = dbService.CUFFaccount.GetEntityReferenceFromId(node.AccountId.Value);

				try
				{
					// Try getting a lock on this record, in case someone else is creating it or modifying it.
					if (dbService.RecordLock.LockRollupRecord(transaction.Id, appropRef.Name, nodeAccount.Name, fundingTypeRef.Name, fiscalYear.ToString(), dbService.LocalDateTime))
					{
						// Check if found and create if not
						var parentRecord = dbService.MonthlyRollup.GetRecord(transaction.Id, fundingTypeRef, appropRef, nodeAccount,
															fiscalMonth, fiscalYear, true);
						if (parentRecord != null && parentRecord?.Id == Guid.Empty)
						{
							// Save the record if created (ID should be null)
							// Set the parent record to the previous Parent reference
							var withSave = false;
#if !DESKTOP
							withSave = true;
#endif
							parentRecord.ParentRollup = previousParentRef;
							parentRecord.Id = dbService.Create(parentRecord.Entity, withSave);
						}

						// Save the reference to the parent record
						previousParentRef = parentRecord.Entity.ToEntityReference();
						previousParentRef.Name = parentRecord.Name;

						accountParentNode = nodeAccount.Id;
						nodeProcessed++;
					}
				}
				finally
				{
					dbService.RecordLock.UnlockRecord(true);
				}
			}

			return previousParentRef;
		}

		#region Auxiliary Methods and Classes

		public class CUFFAccountNode
		{
			public Guid? AccountId = null;
			public Guid? ParentId = null;
		}

		private List<CUFFAccountNode> GetParentCUFFAccountNodes(Guid accountId )
		{
			// Retrieve the list of parent nodes for the given CUFF Account record Id
			var parentNodes = new List<CUFFAccountNode>();

			var fetchXml = $@"
<fetch>
  <entity name='rcade_cuffaccount'>
    <attribute name='rcade_cuffaccountid' />
    <attribute name='cr15a_parentcuffaccount' />
    <filter>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='rcade_cuffaccountid' operator='above' value='{accountId}' />
    </filter>
    <order attribute='cr15a_parentcuffaccount' />
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					Guid? parentId = null;
					if (entity.Attributes.ContainsKey("cr15a_parentcuffaccount"))
					{
						parentId = ((EntityReference)entity["cr15a_parentcuffaccount"]).Id;
					}

					parentNodes.Add( new CUFFAccountNode
					{
						AccountId = entity.Id,
						ParentId = parentId
					});
				}
			}

			return parentNodes;
		}

		#endregion
	}

}
