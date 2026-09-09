using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
    using Interfaces;

    public class CARTuserRoleAdapter : ICARTuserRoleAdapter
    {
        IRepository dbService;
        EXConnect_ServiceContext serviceContext;

        public CARTuserRoleAdapter(IRepository dbService)
        {
            this.dbService = dbService;
            this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
        }

        public IEnumerable<ICARTuserRoleRecord> GetAllRecords()
        {
            return serviceContext.rcade_CARTUserRolesSet as IEnumerable<ICARTuserRoleRecord>;
        }
        public ICARTuserRoleRecord GetRecordFromId(Guid id)
        {
            return serviceContext.rcade_CARTUserRolesSet.Where(t => t.Id == id).FirstOrDefault();
        }
        public ICARTuserRoleRecord GetRecordFromName(string name)
        {
            return GetAllRecords().Where(t => t.Name == name).FirstOrDefault();
        }
        public ICARTuserRoleRecord GetRecordFromEntity(Entity entity)
        {
            return entity.ToEntity<rcade_CARTUserRoles>();
        }

        public EntityReference GetEntityReferenceFromName(string name)
        {
            var userRoleRecord = serviceContext.rcade_CARTUserRolesSet.Where(a => a.Name == name).FirstOrDefault();
            if (userRoleRecord == null) return null;

            return userRoleRecord.ToEntityReference();
        }

        public EntityReference GetEntityReferenceFromId(Guid id)
        {
            var userRoleRecord = serviceContext.rcade_CARTUserRolesSet
                                                .Where(a => a.Id == id).FirstOrDefault();
            if (userRoleRecord == null) return null;

            return userRoleRecord.ToEntityReference();
        }

        public ICARTuserRoleRecord CreateRecord()
        {
            return new rcade_CARTUserRoles();
        }

    }
}
