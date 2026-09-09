
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
	public class QuotaSheetAccountMapItem
	{
		public Guid Id;
		public string Name;
		public string Category;
		public string Region;
		public EntityReference CUFFAccount;
		public bool UsedForUSStudents;
		public bool UsedForForeignStudents;
		public bool UsedForUSScholars;
		public bool UsedForVisitingScholars;
	}

	public class QuotaSheetAccountMapAdapter : IQuotaSheetAccountMapAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public QuotaSheetAccountMapAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}


		public IList<QuotaSheetAccountMapItem> GetRecordsList()
		{
			var recordList = serviceContext.eca_QuotaSheetToCUFFAccountMapSet
							.Select(m => new QuotaSheetAccountMapItem
							{
								Id = m.Id,
								Category = m.eca_Category,
								Region = m.eca_RegionCode,
								Name = m.eca_Name,
								CUFFAccount = m.eca_CUFFAccount,
								UsedForUSStudents = m.eca_UsedforUSStudents.Value,
								UsedForForeignStudents = m.eca_UsedforForeignStudents.Value,
								UsedForUSScholars = m.eca_UsedforUSScholars.Value,
								UsedForVisitingScholars = m.eca_UsedforVisitingScholars.Value
							}).ToList();

			return recordList;
		}
	}
}
