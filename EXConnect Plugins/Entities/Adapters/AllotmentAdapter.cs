using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class AllotmentAdapter : IAllotmentAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public AllotmentAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public string GetCodeFromId( Guid id )
		{
			var allotment = serviceContext.eca_AllotmentSet.Where(a => a.Id == id).FirstOrDefault();
			return allotment.eca_Code;
		}
		public string GetNameFromId( Guid id )
		{
			var allotment = serviceContext.eca_AllotmentSet.Where(a => a.Id == id).FirstOrDefault();
			return allotment.eca_Name;
		}

	}
}
