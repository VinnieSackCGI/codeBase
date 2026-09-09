using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	using System.Web.Util;
	using Interfaces;

	public class CARTtransactionAdapter : ICARTtransactionAdapter
    {
        IRepository dbService;
        EXConnect_ServiceContext serviceContext;

        public CARTtransactionAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
        }

        public IEnumerable<ICARTtransactionRecord> GetAllRecords()
        {
			return serviceContext.rcade_CARTTransactionSet as IEnumerable<ICARTtransactionRecord>;
        }
        public ICARTtransactionRecord GetRecordFromId(Guid id)
        {
            return serviceContext.rcade_CARTTransactionSet.Where(t => t.Id == id).FirstOrDefault();
        }
        public ICARTtransactionRecord GetRecordFromName(string name)
        {
            return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
        }
        public ICARTtransactionRecord GetRecordFromEntity(Entity entity)
        {
            rcade_CARTTransaction result;
            try
            {
				result = entity.ToEntity<rcade_CARTTransaction>();
			}
			catch
            {
                result = null;
            }
            return result;
        }

        public EntityReference GetEntityReferenceFromName(string name)
        {
            var transactionRecord = serviceContext.rcade_CARTTransactionSet.Where(a => a.Name == name).FirstOrDefault();
            if (transactionRecord == null) return null;

            return transactionRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var transactionRecord = serviceContext.rcade_CARTTransactionSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (transactionRecord == null) return null;

            return transactionRecord.ToEntityReference();
        }

		public ICARTtransactionRecord CreateRecord()
        {
            return new rcade_CARTTransaction();
        }

		public string GetLogicalName( string fieldName )
		{
			var propertyInfo = typeof(rcade_CARTTransaction).GetProperty(fieldName);
			var attribute = (AttributeLogicalNameAttribute)Attribute.GetCustomAttribute(
					propertyInfo, typeof(AttributeLogicalNameAttribute));
			return attribute.LogicalName;
		}

        /// <summary>
        /// Get the highest number from all obligations for the same TransCode identifier
        /// </summary>
        /// <param name="fiscalYear"></param>
        /// <param name="codeString"></param>
        /// <returns></returns>
        public int GetHighestDocumentNumberByObligationCodeString(int fiscalYear, string codeString, int countLimit)
        {

            //var codeLabel = string.Concat(fiscalYear.ToString().Substring(2), codeString);
            var codeLabel = codeString;


            var fetchXml = $@"
<fetch>
  <entity name='rcade_carttransaction'>
    <attribute name='rcade_documentnumber' />
    <link-entity name='eca_obligationtypes' to='eca_obligationtype' from='eca_obligationtypesid' alias='O' link-type='inner'>
      <filter>
        <condition attribute='statecode' operator='eq' value='0' />
        <condition attribute='eca_obligationtypename' operator='not-null' />
        <condition attribute='eca_obligationtypename' operator='ne' value='Bulk Funding Purchase Card' />
        <condition attribute='eca_obligationtypename' operator='ne' value='International Travel' />
      </filter>
    </link-entity>
    <filter>
      <condition attribute='rcade_documentnumber' operator='not-null' />
      <condition attribute='eca_ecatransactiontype' operator='eq' value='{(int)eca_ECATransactionTypes.Obligation}' />
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='rcade_fiscalyear' operator='eq' value='{fiscalYear}' />
      <condition attribute='rcade_documentnumber' operator='like' value='%{codeLabel}%' />
    </filter>
  </entity>
</fetch>
";
            var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            var docNumberList = new List<int>();

            /*if (results.Entities.Count > 0)
            {
                foreach (var entity in results.Entities)
                {
                    var documentNum = entity["rcade_documentnumber"].ToString();

                    // Extract the number part from it
                    if (documentNum.Length >= 10 && documentNum.Length <= 15)
                    {
                        var number = 1;
                        docNumberList.Add(number);
                    }
                }
            }*/
            if (results.Entities.Count > 0)
            {
                foreach (var entity in results.Entities)
                {
                    var documentNum = entity["rcade_documentnumber"].ToString();

                    // Determine suffix digit length based on countLimit
                    var numLength = countLimit == 99 ? 2 : countLimit == 999 ? 3 : 4;
                    var codeLength = codeString.Length;

                    // Number is now before the codeString: e.g. 10722602FEE -> extract "02"
                    if (documentNum.Length >= 10 && documentNum.Length <= 15 && documentNum.EndsWith(codeString, StringComparison.OrdinalIgnoreCase))
                    {
                        var numberPart = documentNum.Substring(documentNum.Length - codeLength - numLength, numLength);
                        if (int.TryParse(numberPart, out var number))
                        {
                            docNumberList.Add(number);
                        }
                    }
                }
            }

            var docNumber = 1;

            if (docNumberList != null && docNumberList.Count() >= 1)
            {
                docNumber = docNumberList.Max() + 1;
            }

            return docNumber;


        }

		public int GetHighestDocumentNumberByCommitmentCodeString( int fiscalYear, string codeString, int countLimit )
		{
			var yearCode = fiscalYear.ToString().Substring(2, 2);

			// This query appears to be too complicated for even this system so I had to break it up

			var docNumbers = from t in serviceContext.rcade_CARTTransactionSet
							 where t.rcade_DocumentNumber != null
							  && t.StateCode == rcade_CARTTransactionState.Active
									&& t.rcade_FiscalYear == fiscalYear
							 && t.eca_ECATransactionType == eca_ECATransactionTypes.Commitment
							 && t.rcade_DocumentNumber.Contains(codeString)
							 select new { documentNumber = t.rcade_DocumentNumber };

            IEnumerable<int> docNumberList = null;

            if (codeString.StartsWith("AQ"))
            {
                docNumberList = docNumbers.ToList()
                                .Where(t => t.documentNumber.Length > 10
                                    && t.documentNumber.Length < 15
                                    && t.documentNumber.StartsWith(codeString))
                                .OrderByDescending(t => Convert.ToInt32(t.documentNumber.Substring(2,3)))
                                .Select(t => Convert.ToInt32(t.documentNumber.Substring(2,3)));
            }
            /*
			else if (codeString.StartsWith("FA"))
			{
				docNumberList = docNumbers.ToList()
								.Where(t => t.documentNumber.Length > 10
									&& t.documentNumber.Length < 15
									&& t.documentNumber.StartsWith(codeString))
								.OrderByDescending(t => Convert.ToInt32(t.documentNumber.Substring(2)))
								.Select(t => Convert.ToInt32(t.documentNumber.Substring(2)));
			}
			else
			{
				docNumberList = docNumbers.ToList()
								.Where(t => t.documentNumber.Length > 10
									&& t.documentNumber.Length < 15
									&& t.documentNumber.StartsWith(codeString))
								.OrderByDescending(t => Convert.ToInt32(t.documentNumber.Substring(4)))
								.Select(t => Convert.ToInt32(t.documentNumber.Substring(4)));
			}
            */

			// Return 1 if no documents found or the highest number surpasses the maxNumber limit
			var docNumber = 1;

			if (docNumberList != null && docNumberList.Count() >= 1)
			{
				docNumber = docNumberList.First();
				if (docNumber < countLimit)
				{
					docNumber += 1;
				}
				else
				{
					docNumber = 1;
				}
			}

			return docNumber;
		}

		public bool CanBeAutoApproved( ICARTtransactionRecord transaction )
		{
			// Verify the transaction modifier/creator can approve this transaction
			// First we need to know the Neighborhood of the transaction
			var cuffAccount = dbService.CUFFaccount.GetRecordFromId(transaction.CUFFaccount.Id);
			var neighborhood = serviceContext.eca_NeighborhoodSet
									.Where(n => n.Id == cuffAccount.Neighborhood.Id)
									.FirstOrDefault();
			// Now determine if the executing user is contined in the list of Budget Analysts for the neighborhood
			if (IsUserABudgetAnalystsForNeighborhood(neighborhood.Id, dbService.UserId))
			{
				return true;
			}

			return false;
		}

		public ICARTtransactionRecord ComposeTransactionRecordFromStaged( IStagedBulkUploadRecord stagedRecord )
		{
			// Construct a CART Transaction record from the incoming Staged transaction record.

			var newRecord = CreateRecord();

			newRecord.Appropriation = stagedRecord.Appropriation;
			newRecord.CUFFaccount = stagedRecord.CUFFaccount;
			newRecord.FromCUFFaccount = stagedRecord.FromCUFFaccount;
			newRecord.FundingSource = stagedRecord.FundingSource;
            newRecord.FundingType = stagedRecord.FundingType ;
            newRecord.FiscalYear = stagedRecord.FiscalYear;
            newRecord.Type = stagedRecord.TemplateType;
            newRecord.IBIS_RequestCode = stagedRecord.IBIS_RequestCode;
            newRecord.ProjectCode = stagedRecord.ProjectCode;
			newRecord.Description = stagedRecord.Description_Justification;
			newRecord.Amount = stagedRecord.Amount;

			return newRecord;
		}

		#region Private Methods

		/// <summary>
		/// Get the list of Budget Analsysts for a given Neighhborhood
		/// </summary>
		/// <param name="neighborhoodId"></param>
		/// <returns></returns>
		private bool IsUserABudgetAnalystsForNeighborhood( Guid neighborhoodId, Guid userId )
        {
            // Prepare a count query to degermine if the user is listed as a Budget Analayst for the neighborhood
            var fetchXml = $@"
<fetch aggregate='true'>
  <entity name='eca_neighborhood_businessanalysts'>
    <attribute name='eca_neighborhood_businessanalystsid' alias='count' aggregate='count' />
    <link-entity name='eca_neighborhood' to='eca_neighborhoodid' from='eca_neighborhoodid' alias='N' link-type='inner'>
      <filter>
        <condition attribute='eca_neighborhoodid' operator='eq' value='{neighborhoodId}' />
      </filter>
    </link-entity>
    <link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='U' link-type='inner'>
      <filter>
        <condition attribute='systemuserid' operator='eq' value='{userId}' />
      </filter>
    </link-entity>
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
            {
                // Expecting only a count, so let's get to it
                var entity = results.Entities[0];
                if (entity.Attributes.Contains("count"))
                {
                    if ((int)((AliasedValue)entity["count"]).Value > 0)
                    {
                        return true;
                    }
				}
			}

			return false;
        }

        #endregion
    }
}
