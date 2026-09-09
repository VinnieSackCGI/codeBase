using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class CUFFaccountAdapter : ICUFFaccountAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public CUFFaccountAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IEnumerable<ICUFFaccountRecord> GetAllRecords()
		{
			return serviceContext.rcade_CUFFAccountSet as IEnumerable<ICUFFaccountRecord>;
		}
		public ICUFFaccountRecord GetRecordFromId( Guid id )
		{
			return serviceContext.rcade_CUFFAccountSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public ICUFFaccountRecord GetRecordFromName( string name )
		{
			return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
		}
		public ICUFFaccountRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<rcade_CUFFAccount>();
		}

		public EntityReference GetEntityReferenceFromName( string name )
		{
			var cuffAccount = serviceContext.rcade_CUFFAccountSet.Where(a => a.rcade_CUFFAccountName == name).FirstOrDefault();
			if (cuffAccount == null) return null;

			return cuffAccount.ToEntityReference();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			var cuffAccount = serviceContext.rcade_CUFFAccountSet
												.Where(a => a.Id == id).FirstOrDefault();
			if (cuffAccount == null) return null;

			// Set the Name in the Reference cuz ToEntityReference function ignores it
			var accountRef = cuffAccount.ToEntityReference();
			accountRef.Name = cuffAccount.Name;

			return accountRef;
		}

		private string GetNameFromId( Guid id )
		{
			var account = GetAllRecords().Where(a => a.Id == id).FirstOrDefault();
			if (account == null)
			{
				return string.Empty;
			}
			return account.Name;
		}

		public EntityReference GetExtendedEntityReference( EntityReference accountRef )
		{
			if (string.IsNullOrEmpty(accountRef.Name))
			{
				accountRef.Name = GetNameFromId( accountRef.Id );
			}
			return accountRef;
		}


		public ICUFFaccountRecord CreateRecord()
		{
			return new rcade_CUFFAccount();
		}

		public DateTime GetLastRevisionDate()
		{
			// Retrieve the last date the records were modified

			var fetchXml = $@"
		<fetch aggregate='true'> 
		  <entity name='rcade_cuffaccount'>
			<attribute name='modifiedon' alias='modifiedon_max' aggregate='max' />
			<filter>
				<condition attribute='statecode' operator='eq' value='0' />
			</filter>
		  </entity>
		</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				// Expecting only a max value, so let's get to it
				var entity = results.Entities[0];
				if (entity.Attributes.Contains("modifiedon_max"))
				{
					return (DateTime)((AliasedValue)entity["modifiedon_max"]).Value;
				}
			}
			return DateTime.Now;
		}

	}
}
