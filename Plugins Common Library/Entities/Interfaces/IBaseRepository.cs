using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Plugins_CommonLibrary.Entities.Interfaces
{
    public interface IBaseRepository
    {
        IOrganizationService OrganizationService { get; }
        OrganizationServiceContext OrgServiceContext { get; }
        DateTime LocalDateTime { get; set; }

        Guid UserId { get; set; }

        #region Data Manipulation methods

        Guid Create(Entity entity, bool withSave = false);
        void Delete(Entity entity);
        void SaveChanges();
        void Update(Entity entity, bool withSave = true);
        Guid Upsert(Entity entity);

		ExecuteMultipleRequest GetExecuteMultipleRequest();

		UpdateResponse Patch( Entity entity );

		#endregion

	}
}
