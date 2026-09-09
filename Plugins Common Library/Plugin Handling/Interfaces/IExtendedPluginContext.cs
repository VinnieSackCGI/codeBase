using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

using Plugins_CommonLibrary.Plugin_Handling.Extensions;
using System;

namespace Plugins_CommonLibrary.Plugin_Handling.Interfaces
{
	public interface IExtendedPluginContext : IPluginExecutionContext
	{
		string PluginTypeName { get; }
		RegisteredEvent Event { get; }
		EntityReference PrimaryEntity { get; }
		IPluginExecutionContext ExecutionContext { get; }
		ITracingService TracingService { get; }
		IOrganizationService SystemOrganizationService { get; }
		IOrganizationService UserOrganizationService { get; }
		void ResetUserOrganizationService( Guid userId );

		void Trace(string message);
		Entity GetPreImage(string imageAlias = null);
		Entity GetPostImage(string imageAlias = null);
		Entity GetTargetEntity();
		EntityReference GetTargetEntityReference();
		Relationship GetRelationship();
		EntityReferenceCollection GetRelatedEntities();
	}
}