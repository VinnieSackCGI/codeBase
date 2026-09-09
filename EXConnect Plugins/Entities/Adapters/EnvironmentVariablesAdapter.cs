using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class EnvironmentVariablesAdapter : IEnvironmentVariablesAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public EnvironmentVariablesAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public string GetValue( string schemaName )
		{
			var envVarDefition = serviceContext.EnvironmentVariableDefinitionSet
										   .Where(e => e.SchemaName == schemaName).FirstOrDefault();
			if (envVarDefition != null)
			{
				var envVarRecord = serviceContext.EnvironmentVariableValueSet
											   .Where(e => e.EnvironmentVariableDefinitionId.Id == envVarDefition.Id)
											   .FirstOrDefault();
				if (envVarRecord == null)
				{
					if (!string.IsNullOrEmpty(envVarDefition.DefaultValue))
					{
						return envVarDefition.DefaultValue;
					}
					else
					{
						return string.Empty;
					}
				}
				return envVarRecord.Value;
			}
			else
				return string.Empty;
		}

	}
}
