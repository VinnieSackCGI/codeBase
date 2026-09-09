using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class GFMSdocumentAdapter : IGFMSdocumentAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public GFMSdocumentAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IGFMSdocumentRecord CreateRecord()
		{
			return new eca_GFMSDocument();
		}

		public IEnumerable<IGFMSdocumentRecord> GetAllRecords()
		{
			return serviceContext.eca_GFMSDocumentSet as IEnumerable<IGFMSdocumentRecord>;
		}

		public IGFMSdocumentRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_GFMSDocument>();
		}

		public IGFMSdocumentRecord GetRecordFromId( Guid id )
		{
			return GetAllRecords().Where(d => d.Id == id).FirstOrDefault();
		}

		/// <summary>
		/// Get the list of Child transactions for the given GFMS document
		/// </summary>
		/// <param name="parentId">GFMS Document Id</param>
		/// <returns>List of ChildTransaction class</returns>
		public IList<ICARTtransactionRecord> GetChildTransactionsList( Guid parentId )
		{
			return dbService.Transaction
							.GetAllRecords().Where(t => t.ParentDocument?.Id == parentId && t.IsActive)
											.ToList();
		}

        /// <summary>
        /// Get the highest number from all obligation documents for the same TransCode identifier
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
  <entity name='eca_gfmsdocument'>
    <attribute name='eca_documentnumber' />
    <link-entity name='eca_obligationtypes' to='eca_obligationtype' from='eca_obligationtypesid' alias='O' link-type='inner'>
      <filter>
        <condition attribute='statecode' operator='eq' value='0' />
        <condition attribute='eca_obligationtypename' operator='not-null' />
        <condition attribute='eca_obligationtypename' operator='ne' value='Bulk Funding Purchase Card' />
        <condition attribute='eca_obligationtypename' operator='ne' value='International Travel' />
      </filter>
    </link-entity>
    <filter>
      <condition attribute='eca_documentnumber' operator='not-null' />
      <condition attribute='eca_ecatransactiontype' operator='eq' value='{(int)eca_ECATransactionTypes.Obligation}' />
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='eca_documentnumber' operator='like' value='%{codeLabel}%' />
    </filter>
  </entity>
</fetch>
";
            var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            var docNumberList = new List<int>();

            if (results.Entities.Count > 0)
            {
                foreach (var entity in results.Entities)
                {
                    var documentNum = entity["eca_documentnumber"].ToString();
                    var numLength = countLimit == 99 ? 2 : countLimit == 999 ? 3 : 4;
                    var codeLength = codeString.Length;
                    var salaryCodes = new HashSet<string> { "GFACS1", "GFACS", "FEE", "DHS", "SB" };

                    if (salaryCodes.Contains(codeString))
                    {
                        // Salary types: number is BEFORE codeString e.g. 10722603FEE
                        if (documentNum.Length >= 10 && documentNum.Length <= 15 && documentNum.EndsWith(codeString, StringComparison.OrdinalIgnoreCase))
                        {
                            var numberPart = documentNum.Substring(documentNum.Length - codeLength - numLength, numLength);
                            if (int.TryParse(numberPart, out var number))
                            {
                                docNumberList.Add(number);
                            }
                        }
                    }
                    else
                    {
                        // Non-salary types: number is AFTER codeString e.g. 107226Q0001
                        if (documentNum.Length >= 10 && documentNum.Length <= 15 && int.TryParse(documentNum.Substring(documentNum.Length - numLength), out _))
                        {
                            var number = int.Parse(documentNum.Substring(documentNum.Length - numLength));
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

            /*
            var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            var docNumberList = new List<int>();

            if (results.Entities.Count > 0)
            {
                /*foreach (var entity in results.Entities)
				{
					var documentNum = entity["eca_documentnumber"].ToString();
                    // Extract the number part from it
                    if (documentNum.Length >= 10 && documentNum.Length <= 15 && int.TryParse(documentNum.Substring(documentNum.Length - 4), out _))
                    {

                        var number = int.Parse(documentNum.Substring(documentNum.Length - 4));
                        docNumberList.Add(number);
					}
				}
            }
            //Based on new GFACS and other ob type requirement
            foreach (var entity in results.Entities)
            {
                var documentNum = entity["eca_documentnumber"].ToString();
                // Determine suffix digit length based on countLimit
                var numLength = countLimit == 99 ? 2 : countLimit == 999 ? 3 : 4;
                var codeLength = codeString.Length;
                var salaryCode = new HashSet<string> { "GFACS", "GFACS1", "FEE", "DHS", "SB" };

                var docNumber = 1;

                if (docNumberList != null && docNumberList.Count() >= 1)
                {
                    docNumber = docNumberList.Max() + 1;
                }

                return docNumber;
            }
        }

                /*
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
            */
        


        public int GetHighestDocumentNumberByCommitmentCodeString( int fiscalYear, string codeString, int countLimit )
		{
			var yearCode = fiscalYear.ToString().Substring(2, 2);

			// This query appears to be too complicated for even this system so I had to break it up

			var docNumbers = from t in serviceContext.eca_GFMSDocumentSet
							 where t.eca_DocumentNumber != null
							  && t.StateCode == eca_GFMSDocumentState.Active
							 && t.eca_ECATransactionType == eca_ECATransactionTypes.Commitment
							 && t.eca_DocumentNumber.Contains(codeString)
							 select new { documentNumber = t.eca_DocumentNumber };

			IEnumerable<int> docNumberList = null;

			if (codeString.StartsWith("AQ"))
			{
				docNumberList = docNumbers.ToList()
								.Where(t => t.documentNumber.Length > 10
									&& t.documentNumber.Length < 15
									&& t.documentNumber.StartsWith(codeString))
								.OrderByDescending(t => Convert.ToInt32(t.documentNumber.Substring(2, 3)))
								.Select(t => Convert.ToInt32(t.documentNumber.Substring(2, 3)));
			}

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

	}
}
