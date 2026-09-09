using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{
	public interface IWorkflowAdapter
	{
		IWorkflowRecord GetRecordForEntityId( Guid id );
		IWorkflowRecord GetRecordFromEntity( Entity entity );
	}

	public interface IProcessStageAdapter
	{
		IProcessStageRecord GetRecordFromStageRef( EntityReference stageRef );
		IList<IProcessStageRecord> GetAllProcessRecords( Guid id );
		Dictionary<string, Guid> GetAllProcessStagesForPrimaryEntityName( string entityLogicalName );
	}
}
