using System;
using System.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace Plugins_CommonLibrary.Entities.Services
{
    public class BaseBusinessDbService : IBaseRepository
    {
        protected IOrganizationService organizationService;
        protected ITracingService tracer;

        public OrganizationServiceContext OrgServiceContext { get; protected set; }

        public BaseBusinessDbService(IOrganizationService organizationService, ITracingService tracer)
        {
            if (organizationService == null)
            {
                throw new ArgumentNullException("Organization service");
            }
            if (tracer == null)
            {
                throw new ArgumentNullException("Tracer");
            }

            this.organizationService = organizationService;
            this.tracer = tracer;
        }

        public DateTime LocalDateTime { get; set; }
        public Guid UserId { get; set; }

        public Guid Create(Entity entity, bool withSave = false)
        {
            var entityId = this.organizationService.Create(entity);
            if (withSave)
            {
                SaveChanges();
            }
            return entityId;
        }

        public virtual void Delete(Entity entity)
        {
            this.organizationService.Delete(entity.LogicalName, entity.Id);
        }

        public virtual void SaveChanges()
        {
            OrgServiceContext.SaveChanges();
        }

        public virtual void Update(Entity entity, bool withSave = true)
        {
            if (!OrgServiceContext.GetAttachedEntities().Contains(entity))
            {
                OrgServiceContext.Attach(entity);
            }
            OrgServiceContext.UpdateObject(entity);
            if (withSave)
            {
                this.SaveChanges();
            }
        }

        public virtual Guid Upsert(Entity entity)
        {
            if (entity.Id == default(Guid))
            {
                return this.organizationService.Create(entity);
            }
            else
            {
                this.Update(entity);
                return entity.Id;
            }
        }

        public ExecuteMultipleRequest GetExecuteMultipleRequest()
        {
            return new ExecuteMultipleRequest
                    {
                        Settings = new ExecuteMultipleSettings
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        },
                        // Create an empty organization request collection
                        Requests = new OrganizationRequestCollection()
                    };
        }

		public UpdateResponse Patch(Entity entity)
		{
			var updRequest = new UpdateRequest
			{
				Target = entity
			};

			return (UpdateResponse)OrganizationService.Execute(updRequest);
		}

		#region Data Retrieval

		public IOrganizationService OrganizationService
        {
            get
            {
                return organizationService;
            }
        }

        #endregion

    }
}
