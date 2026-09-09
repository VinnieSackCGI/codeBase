
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class QuotaSheetRegionalDataAdapter : IQuotaSheetRegionalDataAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public QuotaSheetRegionalDataAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IQuotaSheetRegionalDataRecord CreateRecord()
		{
			return new eca_QuotaSheetRegionalData();
		}

		public IEnumerable<IQuotaSheetRegionalDataRecord> GetAllRecords()
		{
			return serviceContext.eca_QuotaSheetRegionalDataSet as IEnumerable<IQuotaSheetRegionalDataRecord>;
		}

		public IQuotaSheetRegionalDataRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_QuotaSheetRegionalData>();
		}

		public bool IsRegionalDataType( IQuotaSheetDataRecord dataRecord )
		{
			if (dataRecord.GetType() == typeof(eca_QuotaSheetRegionalData))
			{
				return true;
			}
			return false;
		}
	}
}

