using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

using System;
using System.Collections.Generic;
using System.Linq;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
    using Interfaces;

    public class ProcessStageAdapter : IProcessStageAdapter
    {
        IRepository dbService;
        EXConnect_ServiceContext serviceContext;

        public ProcessStageAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
        }

        public IProcessStageRecord GetRecordFromStageRef(EntityReference stageRef)
        {
            return serviceContext.ProcessStageSet.Where(o => o.Id == stageRef.Id).FirstOrDefault();
        }

		public IList<IProcessStageRecord> GetAllProcessRecords( Guid id )
		{
			return serviceContext.ProcessStageSet
							.Where(s => s.ProcessId.Id == id)
							.ToList<IProcessStageRecord>();
		}

		public Dictionary<string,Guid> GetAllProcessStagesForPrimaryEntityName( string entityLogicalName )
		{
			var stageRecords = serviceContext.ProcessStageSet
							.Where(s => s.PrimaryEntityTypeCode == entityLogicalName)
							.ToList<IProcessStageRecord>();

            var dictionary = new Dictionary<string,Guid>();

            foreach (var stageRecord in stageRecords) 
            {
                dictionary.Add(stageRecord.StageName, stageRecord.Id);
            }

            return dictionary;
		}

	}
}
