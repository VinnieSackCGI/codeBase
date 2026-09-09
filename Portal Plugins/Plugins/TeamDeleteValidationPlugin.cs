
using System;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using Portal_Plugins.Plugin_Handling;

namespace Portal_Plugins.Plugins
{
	[CrmPluginRegistration(MessageNameEnum.Delete,
	"team",
	StageEnum.PreValidation,
	ExecutionModeEnum.Synchronous, "",
	"Team Deletion Validation", 1,
	IsolationModeEnum.Sandbox,
	Description = "Prevent Team deletion if it has associated Web Roles and Users",
	Id = "1a334a64-fd4b-4f13-b81e-3ac54e8d3cfc")]

	public class TeamDeleteValidationPlugin : MainPlugin
	{
		//==========================================
		// Whenever a Team is deleted, no diassociation events are fired. As a result, this plugin
		// will prevent the Team from being deleted if it is found with associated Web Roles and Users.
		//==========================================

		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Team Delete Validation");

			// Get the Contact user record object from the target entity
			var teamRef = context.GetTargetEntityReference();

			// Get the list of team associated F3S Web Roles or Users 
			var teamRolesCount = GetTeamWebRolesCount(teamRef.Id);
			var teamUsersCount = GetTeamUsersCount(teamRef.Id);

			tracer.Trace($"Found \"{teamRolesCount}\" web roles and \"{teamUsersCount}\" for team being deleted Id = {teamRef.Id}.");

			if (teamRolesCount > 0 && teamUsersCount > 0)
			{
				tracer.Trace("Throwing the exception to terminate the Delete operation.");
				throw new InvalidPluginExecutionException("The team cannot be deleted with both associated Users and Web Roles. Please remove all users OR Web Roles and try again.");
			}
			
		}

		#region Auxiliary Methods

		private int GetTeamWebRolesCount(Guid teamId)
		{
			var fetchXml = $@"
<fetch aggregate='true'>
  <entity name='f3s_webroles_team'>
    <attribute name='f3s_webroles_teamid' alias='count' aggregate='count' />
	<filter>
		<condition attribute='teamid' operator='eq' value='{teamId}' />
	</filter>
  </entity>
</fetch>";

			return GetQueryCount(fetchXml);

		}

		private int GetTeamUsersCount(Guid teamId)
		{
			var fetchXml = $@"
<fetch aggregate='true'>
  <entity name='teammembership'>
    <attribute name='teammembershipid' alias='count' aggregate='count' />
	<filter>
		<condition attribute='teamid' operator='eq' value='{teamId}' />
	</filter>
  </entity>
</fetch>";

			return GetQueryCount(fetchXml);

		}

		private int GetQueryCount(string fetchXml)
		{
			var count = 0;

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				var entity = results.Entities[0];
				count = (int)((AliasedValue)entity["count"]).Value;
			}

			return count;
		}
		#endregion

	}
}
