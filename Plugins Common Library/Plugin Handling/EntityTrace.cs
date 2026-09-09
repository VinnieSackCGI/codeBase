using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Plugins_CommonLibrary.Plugin_Handling
{
    public class ExecutionAttributes
    {
        public int? Stage;
        public string Message;
        public int IsolationMode;
        public Guid InitiatingUser;
        public int Depth;
		public string EntityLogicalName;
		public Guid EntityId;
	}

	public class EntityTrace
    {
		public EntityTrace( Entity targetEntity, EntityReference targetReference, Relationship relationship, EntityReferenceCollection relatedEntities,
							Entity preImageEntity, Entity postImageEntity, ExecutionAttributes attributes )
		{
			this.TargetEntity = targetEntity;
			this.TargetReference = targetReference;
			this.PreImageEntity = preImageEntity;
			this.PostImageEntity = postImageEntity;
			this.Relationship = relationship;
			this.RelatedEntities = relatedEntities;
			this.Attributes = attributes;
		}

		public Entity TargetEntity { get; set; }

		public EntityReference TargetReference { get; set; }

		public Relationship Relationship { get; set; }
		public EntityReferenceCollection RelatedEntities { get; set; }

		public Entity PreImageEntity { get; set; }

		public Entity PostImageEntity { get; set; }

		public ExecutionAttributes Attributes { get; set; }
	}
}
