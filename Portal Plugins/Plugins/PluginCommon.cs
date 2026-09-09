
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Plugins
{
	// A series of common methods that are shared among plugins

	public class UserSPAction
	{
		public string Action;
		public string EmailAddress;
		public string PermissionLevel;
	}

	public class PluginCommon
	{
		ITracingService tracer;
		IRepository dbService;

		public PluginCommon(IRepository dbService, ITracingService tracer)
		{
			this.tracer = tracer;
			this.dbService = dbService;
		}

		public void PopulateSupportDocsUserSPactionTable(IList<UserSPAction> spActionList)
		{
			if (spActionList.Count > 0)
			{
				foreach (var user in spActionList)
				{
					var spRecord = dbService.UserSPaction.CreateRecord();
					spRecord.EmailAddress = user.EmailAddress;
					spRecord.Action = user.Action;
					spRecord.PermissionLevel = user.PermissionLevel;
					dbService.Create(spRecord.Entity);
					tracer.Trace($"Adding SPAction record for \"{user.Action}\", \"{user.PermissionLevel}\", \"{user.EmailAddress}\".");
				}
				dbService.SaveChanges();
			}
		}

	}
}
