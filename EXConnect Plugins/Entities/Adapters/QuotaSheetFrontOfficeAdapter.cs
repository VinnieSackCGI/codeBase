
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
	public class QuotaSheetFrontOfficeAdapter : IQuotaSheetFrontOfficeDataAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public QuotaSheetFrontOfficeAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IQuotaSheetFrontOfficeDataRecord CreateRecord()
		{
			return new eca_FrontOfficeQuotaSheet();
		}

		public IEnumerable<IQuotaSheetFrontOfficeDataRecord> GetAllRecords()
		{
			return serviceContext.eca_FrontOfficeQuotaSheetSet as IEnumerable<IQuotaSheetFrontOfficeDataRecord>;
		}

		public IQuotaSheetFrontOfficeDataRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_FrontOfficeQuotaSheet>();
		}

		public bool IsFrontOfficeDataType( IQuotaSheetDataRecord dataRecord )
		{
			if (dataRecord.GetType() == typeof(eca_FrontOfficeQuotaSheet))
			{
				return true;
			}
			return false;
		}

	}
}

