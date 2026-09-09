using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class FundingSourceAdapter: IFundingSourceAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public FundingSourceAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IFundingSourceRecord CreateRecord()
		{
			return new eca_FundingSource() as IFundingSourceRecord;
		}

		public IEnumerable<IFundingSourceRecord> GetAllRecords()
		{
			return serviceContext.eca_FundingSourceSet as IEnumerable<IFundingSourceRecord>;
		}

		public IFundingSourceRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_FundingSource>() as IFundingSourceRecord;
		}

		public string GetNameFromId( Guid id )
		{
			var approp = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (approp == null)
			{
				return string.Empty;
			}
			return approp.Name;
		}

		public IList<FundingSrceRecord>GetAppropriationFundingDataList()
		{
			var fetchXml = $@"
<fetch>
  <entity name='eca_fundingsource_appropriation'>
    <link-entity name='eca_fundingsource' to='eca_fundingsourceid' from='eca_fundingsourceid' alias='S' link-type='outer'>
      <attribute name='eca_sourcename' />
      <attribute name='eca_fundingsource' />
      <attribute name='eca_fundingsourceid' />
      <order attribute='eca_fundingsource' />
      <link-entity name='eca_fundingtype' to='eca_parentfundingtype' from='eca_fundingtypeid' alias='T' link-type='inner'>
        <attribute name='eca_fundingtypename' />
        <attribute name='eca_fundingtypeid' />
      </link-entity>
    </link-entity>
    <link-entity name='rcade_appropriation' to='rcade_appropriationid' from='rcade_appropriationid' alias='A' link-type='inner'>
      <attribute name='rcade_appropriationname' />
      <attribute name='rcade_appropriationid' />
      <filter type='and'>
        <condition attribute='statecode' operator='eq' value='0' />
      </filter>
    </link-entity>
  </entity>
</fetch>
";
			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				var sourceList = new List<FundingSrceRecord>();

				foreach (var entity in results.Entities)
				{
					var srcRecord = new FundingSrceRecord();

					if (entity.Attributes.Contains("S.eca_fundingsource"))
					{
						srcRecord.FundingSrcCode = ((AliasedValue)entity["S.eca_fundingsource"]).Value.ToString();
					}
					if (entity.Attributes.Contains("S.eca_fundingsourceid"))
					{
						srcRecord.FundingSrcId = Guid.Parse(((AliasedValue)entity["S.eca_fundingsourceid"]).Value.ToString());
					}
					if (entity.Attributes.Contains("S.eca_sourcename"))
					{
						srcRecord.FundingSrcName = ((AliasedValue)entity["S.eca_sourcename"]).Value.ToString();
					}
					if (entity.Attributes.Contains("T.eca_fundingtypename"))
					{
						srcRecord.FundingType = ((AliasedValue)entity["T.eca_fundingtypename"]).Value.ToString();
					}
					if (entity.Attributes.Contains("T.eca_fundingtypeid"))
					{
						srcRecord.FundingTypeId = Guid.Parse(((AliasedValue)entity["T.eca_fundingtypeid"]).Value.ToString());
					}
					if (entity.Attributes.Contains("A.rcade_appropriationname"))
					{
						srcRecord.Appropriation = ((AliasedValue)entity["A.rcade_appropriationname"]).Value.ToString();
					}
					if (entity.Attributes.Contains("A.rcade_appropriationid"))
					{
						srcRecord.AppropriationId = Guid.Parse(((AliasedValue)entity["A.rcade_appropriationid"]).Value.ToString());
					}

					sourceList.Add(srcRecord);
				}
				return sourceList;
			}
			return null;
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var entity = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (entity == null)
			{
				return null;
			}
			var entityRef = entity.Entity.ToEntityReference();
			entityRef.Name = entity.Name;

			return entityRef;
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var entity = GetAllRecords().Where(a => a.Name == name).FirstOrDefault();
			return GetEntityReferenceFromEntity(entity);
		}

		private EntityReference GetEntityReferenceFromEntity( IFundingSourceRecord record )
		{
			if (record == null)
			{
				return null;
			}

			var entityRef = record.Entity.ToEntityReference();
			entityRef.Name = record.Name;

			return entityRef;

		}

		public EntityReference GetExtendedEntityReference( EntityReference fundingType )
		{
			if (string.IsNullOrEmpty(fundingType.Name))
			{
				fundingType.Name = GetNameFromId(fundingType.Id);
			}

			return fundingType;
		}
	}
}
