using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

namespace PluginsRunner_CommonLibrary
{
    public class RunnerPluginContext : IExtendedPluginContext
    {
        private Guid userId;
        private ParameterCollection _inputParameters;
        private EntityImageCollection _preEntityImages;
        private EntityImageCollection _postEntityImages;
		private Entity targetEntity;
		private EntityReference targetReference;
		private ExecutionAttributes basePlugin;
        private ParameterCollection sharedCollection;

		public RunnerPluginContext( Guid userId, Entity targetEntity, EntityReference targetReference, ExecutionAttributes attribute,
									Relationship relationship, EntityReferenceCollection relatedEntities,
									Entity preImage = null, Entity postImage = null )
		{
			this.userId = userId;

			this.basePlugin = attribute;

			this._inputParameters = new ParameterCollection();

			if (targetEntity != null)
			{
				this._inputParameters.Add("Target", targetEntity);
			}
			else if (targetReference != null)
			{
				this._inputParameters.Add("Target", targetReference);
			}

			if (relationship != null)
			{
				this._inputParameters.Add("Relationship", relationship);
			}
			if (relatedEntities != null)
			{
				this._inputParameters.Add("RelatedEntities", relatedEntities);
			}
			
			this.sharedCollection = new ParameterCollection();
            this.targetEntity = targetEntity; 
			this.targetReference = targetReference;
			if (targetEntity != null)
			{
				this.targetReference = targetEntity.ToEntityReference();
			}

            this._preEntityImages = new EntityImageCollection();
            this._postEntityImages = new EntityImageCollection();

            if (preImage != null)
            {
                this._preEntityImages.Add(ImageTypeName.PreImage, preImage);
            }
            if(postImage != null)
            {
                this._postEntityImages.Add(ImageTypeName.PostImage, postImage);
            }

        }

		public RegisteredEvent Event => throw new NotImplementedException();

		public IOrganizationService SystemOrganizationService => throw new NotImplementedException();
		public IOrganizationService UserOrganizationService => throw new NotImplementedException();
		public void ResetUserOrganizationService(Guid id) => throw new NotImplementedException();

		public IPluginExecutionContext ExecutionContext => throw new NotImplementedException();

		public ITracingService TracingService => null;

		public void Trace( string message ) => throw new NotImplementedException();

		public string PluginTypeName => throw new NotImplementedException();
		public EntityReference PrimaryEntity => targetReference;

		public int Stage => (int)basePlugin.Stage.Value;
        public IPluginExecutionContext ParentContext => null;
        public int Mode => throw new NotImplementedException();
        public int IsolationMode => (int)basePlugin.IsolationMode;
        public int Depth => 1;
		public string MessageName => basePlugin.Message;
		public string PrimaryEntityName => basePlugin.EntityLogicalName;
		public Guid PrimaryEntityId => targetReference.Id;
		
		public string SecondaryEntityName => null;

		public Guid? RequestId => Guid.NewGuid();
		public ParameterCollection InputParameters => this._inputParameters;
        public ParameterCollection OutputParameters => throw new NotImplementedException();
        public ParameterCollection SharedVariables => this.sharedCollection;

		public Guid BusinessUnitId => Guid.NewGuid();
		public Guid OrganizationId => Guid.NewGuid();
		public string OrganizationName => null;

		public EntityImageCollection PreEntityImages => this._preEntityImages;
		public EntityImageCollection PostEntityImages => this._postEntityImages;
		public EntityReference OwningExtension => throw new NotImplementedException();
		public Guid CorrelationId => throw new NotImplementedException();
		public bool IsExecutingOffline => throw new NotImplementedException();
		public bool IsOfflinePlayback => throw new NotImplementedException();
		public bool IsInTransaction => throw new NotImplementedException();
		public Guid OperationId => throw new NotImplementedException();
		public DateTime OperationCreatedOn => throw new NotImplementedException();

		public Entity GetPostImage( string imageAlias = null ) => throw new NotImplementedException();
		public Entity GetPreImage( string imageAlias = null ) => throw new NotImplementedException();
		public Entity GetTargetEntity() => throw new NotImplementedException();
		public EntityReference GetTargetEntityReference() => throw new NotImplementedException();
		public Relationship GetRelationship() => throw new NotImplementedException();
		public EntityReferenceCollection GetRelatedEntities() => throw new NotImplementedException();

		public Guid UserId => this.userId;
		public Guid InitiatingUserId => this.basePlugin.InitiatingUser;


        public OrganizationRequest Request => throw new NotImplementedException();

        public OrganizationResponse Response => throw new NotImplementedException();

        public IOrganizationService OrganizationService => throw new NotImplementedException();

    }
}
