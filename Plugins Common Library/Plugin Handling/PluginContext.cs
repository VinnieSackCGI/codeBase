using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Plugins_CommonLibrary.Plugin_Handling.Extensions;

namespace Plugins_CommonLibrary.Plugin_Handling
{

	public class PluginContext : IExtendedPluginContext
    {
        IOrganizationService systemOrganizationService;
        IOrganizationService userOrganizationService;
        IOrganizationServiceFactory serviceFactory;

        public PluginContext(IServiceProvider serviceProvider, IPluginHandler pluginHandle)
        {
            this.ExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            this.TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            this.Event = this.ExecutionContext.GetEvent(pluginHandle.RegisteredEvents);

            this.serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            
            this.PluginTypeName = pluginHandle.GetType().FullName;

            this.TracingService.Trace("Plugin context was created");
        }

        public RegisteredEvent Event { get; set; }

        public IOrganizationService SystemOrganizationService
        {
            get
            {
                if (systemOrganizationService == null)
                {
                    systemOrganizationService = serviceFactory.CreateOrganizationService(null);
                }
                return systemOrganizationService;
            }
        }

        public IOrganizationService UserOrganizationService
        {
            get
            {
                if (userOrganizationService == null)
                {
                    TracingService.Trace("ReInitializing UserOrgService with : {0}", ExecutionContext.UserId);
                    userOrganizationService = serviceFactory.CreateOrganizationService(ExecutionContext.UserId);
                }
                return userOrganizationService;
            }
        }

        public void ResetUserOrganizationService(Guid userId)
        {
            userOrganizationService = serviceFactory.CreateOrganizationService(userId);
            TracingService.Trace("UserID passed into UserOrgService method: {0}", userId.ToString());
        }

        public IPluginExecutionContext ExecutionContext { get; }

        public ITracingService TracingService { get; set; }

		/// <summary>
		///     Writes a trace message to the CRM trace log.
		/// </summary>
		/// <param name="message">Message name to trace.</param>
		public void Trace( string message )
		{
			if (string.IsNullOrWhiteSpace(message) || (this.TracingService == null)) return;

			this.TracingService.Trace(message);
		}


		public string PluginTypeName { get; }
        public EntityReference PrimaryEntity => new EntityReference(this.PrimaryEntityName, this.PrimaryEntityId);

		public int Stage => this.ExecutionContext.Stage; 
        public IPluginExecutionContext ParentContext => this.ExecutionContext.ParentContext;
        public int Mode => this.ExecutionContext.Mode;
		public int IsolationMode => this.ExecutionContext.IsolationMode;
		public int Depth => this.ExecutionContext.Depth;
		public string MessageName => this.ExecutionContext.MessageName;

		public string PrimaryEntityName => this.ExecutionContext.PrimaryEntityName;
		public Guid PrimaryEntityId => this.ExecutionContext.PrimaryEntityId;
		public string SecondaryEntityName => this.ExecutionContext.SecondaryEntityName;

		public Guid? RequestId => this.ExecutionContext.RequestId;
		public ParameterCollection InputParameters => this.ExecutionContext.InputParameters;
		public ParameterCollection OutputParameters => this.ExecutionContext.OutputParameters;
		public ParameterCollection SharedVariables => this.ExecutionContext.SharedVariables;

		public Guid BusinessUnitId => this.ExecutionContext.BusinessUnitId;
		public Guid OrganizationId => this.ExecutionContext.OrganizationId;
		public string OrganizationName => this.ExecutionContext.OrganizationName;
	
		public EntityImageCollection PreEntityImages => this.ExecutionContext.PreEntityImages;
		public EntityImageCollection PostEntityImages => this.ExecutionContext.PostEntityImages;
		public EntityReference OwningExtension => this.ExecutionContext.OwningExtension;
		public Guid CorrelationId => this.ExecutionContext.CorrelationId;
		public bool IsExecutingOffline => this.ExecutionContext.IsExecutingOffline;
		public bool IsOfflinePlayback => this.ExecutionContext.IsOfflinePlayback;
		public bool IsInTransaction => this.ExecutionContext.IsInTransaction;
		public Guid OperationId => this.ExecutionContext.OperationId;
		public DateTime OperationCreatedOn => this.ExecutionContext.OperationCreatedOn;

		public Entity GetPostImage(string imageName = null)
        {
            imageName = imageName ?? ImageTypeName.PostImage;
            return this.GetImageEntity(imageName, false);
        }

        public Entity GetPreImage(string imageName = null)
        {
            imageName = imageName ?? ImageTypeName.PreImage;
            return this.GetImageEntity(imageName, true);
        }

        public Entity GetTargetEntity()
        {
            if( this.ExecutionContext.InputParameters.Contains("Target") 
             && this.ExecutionContext.InputParameters["Target"] is Entity)
            {
                return (Entity)this.ExecutionContext.InputParameters["Target"];
            }
            return null;
        }

        public EntityReference GetTargetEntityReference()
        {
            if (this.ExecutionContext.InputParameters.Contains("Target") 
             && this.ExecutionContext.InputParameters["Target"] is EntityReference)
            {
                this.TracingService.Trace("Target is EntityReference");
                return (EntityReference)this.ExecutionContext.InputParameters["Target"];
            }
            if (this.ExecutionContext.InputParameters.Contains("Target")
             && this.ExecutionContext.InputParameters["Target"] is Entity)
            {
                this.TracingService.Trace("Target is Entity");
                return ((Entity)this.ExecutionContext.InputParameters["Target"]).ToEntityReference();
            }
            return null;
        }

        public Relationship GetRelationship()
        {
            if (this.ExecutionContext.InputParameters.Contains("Relationship")
#if DEBUG
			 && this.ExecutionContext.InputParameters["Target"] == null)
#else
			 && this.ExecutionContext.InputParameters["Target"] is EntityReference)
#endif
			{
				return (Relationship)this.ExecutionContext.InputParameters["Relationship"];
            }
            return null;
        }

        public EntityReferenceCollection GetRelatedEntities()
        {
            if (this.ExecutionContext.InputParameters.Contains("RelatedEntities")
             && this.ExecutionContext.InputParameters["RelatedEntities"] is EntityReferenceCollection)
            {
                return (EntityReferenceCollection)this.ExecutionContext.InputParameters["RelatedEntities"];
            }
            return null;
        }

		public Guid UserId => this.ExecutionContext.UserId;
		public Guid InitiatingUserId => this.ExecutionContext.InitiatingUserId;

        #region Private Methods

		private Entity GetImageEntity(string imageName, bool preImage)
        {
            var images = preImage ? this.ExecutionContext.PreEntityImages : this.ExecutionContext.PostEntityImages;
            if (images.Contains(imageName) && images[imageName] != null)
            {
                return images[imageName];
            }
            return null;
        }

        #endregion

    }
}
