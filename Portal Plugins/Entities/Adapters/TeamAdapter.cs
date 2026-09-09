
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using Portal_Plugins.Entities.Interfaces;


namespace Portal_Plugins.Entities.Adapters
{
    using Interfaces;
	using Plugins_CommonLibrary.Entities.Interfaces;

	public class TeamAdapter : ITeamAdapter
    {
        IRepository dbService;
		F3S_ServiceContext serviceContext;

        public TeamAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as F3S_ServiceContext;
        }

        public string GetEmailFromId(Guid id)
        {
            var teamRecord = serviceContext.TeamSet.Where(a => a.Id == id).FirstOrDefault();
            if (teamRecord == null) return string.Empty;

            return teamRecord.EMailAddress;
        }

        public EntityReference GetEntityReferenceFromEmail(string emailAddress)
        {
            var teamRecord = serviceContext.TeamSet
                                                .Where(a => a.EMailAddress == emailAddress).FirstOrDefault();
            if (teamRecord == null) return null;

            return teamRecord.ToEntityReference();
        }

        public string GetCodeFromId(Guid id)
        {
            var teamRecord = serviceContext.TeamSet.Where(a => a.Id == id).FirstOrDefault();
            if (teamRecord == null) return string.Empty;

            return teamRecord.Name;
        }

        public EntityReference GetEntityReferenceFromCode(string name)
        {
            var teamRecord = serviceContext.TeamSet.Where(a => a.Name == name).FirstOrDefault();
            if (teamRecord == null) return null;

            return teamRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var teamRecord = serviceContext.TeamSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (teamRecord == null) return null;

            return teamRecord.ToEntityReference();
        }

        public ITeamRecord CreateRecord()
        {
            return new Team();
        }
        public IEnumerable<ITeamRecord> GetAllRecords()
        {
            return serviceContext.TeamSet as IEnumerable<ITeamRecord>;
        }
		public ITeamRecord GetRecordFromId(Guid id)
		{
			return (ITeamRecord)serviceContext.TeamSet.Where(t => t.Id == id).FirstOrDefault();
		}
		public ITeamRecord GetRecordFromName(string name)
		{
			return (ITeamRecord)serviceContext.TeamSet.Where(t => t.Name == name).FirstOrDefault();
		}
		public ITeamRecord GetRecordFromEntity(Entity entity)
        {
            return (ITeamRecord)entity.ToEntity<Team>();
        }

    }
}
