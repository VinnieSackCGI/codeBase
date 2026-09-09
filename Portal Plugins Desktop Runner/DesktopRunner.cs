
using System;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

using Portal_Plugins.Plugins;
using Portal_Plugins.Entities;
using Portal_Plugins.Entities.Interfaces;
using Portal_Plugins.Plugin_Handling.Services;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using PluginsRunner_CommonLibrary;

namespace DesktopInterface
{
	// The program starting parameters are controlled by arguments in the Debug section of the project.
	// To modify, open the project's Properties page and under the Debug tab, specify the desired parameters.
	// Speficy the parameters as a space separated string starting with the environment (SIO, DEV, UAT),
	// followed by the debug option (1 for Plugin debug and 2 for Plugin logic debug).

	// NOTES:
	// =========================================================================================================
	// 1 - When testing the plugins, check the  == BEFORE TESTING BEGINS == blocks below for setup instructions.
	// 2 - When testing Plugin logic, the project must be compiled for Desktop since there are compiler
	// directives that strip out the plugin execution and allow to call the plugin to test just the logic.
	// =========================================================================================================

	class DesktopRunnerHelper
	{

		private const int DEBUG_PLUGIN = 1;
		private const int DEBUG_PLUGIN_LOGIC = 2;

		public static string userName = "";

		static void Main( string[] args )
		{
			var ENV = args[0];
			var processToRun = int.Parse(args[1]);

			// Reset the userid based on the environment (SIO or other)
			var statePrefix = "velezg2";
			var envURL = string.Empty;

			var conn = Runner.GetConnectionString(statePrefix, ENV, out envURL);

			var tracer = new TracerService();

			using (var svc = new CrmServiceClient(conn))
			{
				if (!svc.IsReady)
				{
					Console.WriteLine($"ERROR: Connection to environment \"{envURL}\" could not be established.");
					return;
				}

				if (processToRun == DEBUG_PLUGIN)
				{
					string jsonFileText = string.Empty;
					jsonFileText = Runner.GetJsonTextFromCompressedFile();
					JObject objectsFromJson = JsonConvert.DeserializeObject<JObject>(jsonFileText);

					// ============================================================================
					// ** BEFORE TESTING BEGINS ***
					// ============================================================================

					// 1 - Modify the typeof plugin class to test
					Type typeOfPluginToRun = typeof(TeamMembershipPlugin);

					// 2 - Define the entity class that the plugin triggers on
					// ============================================================================
					Runner.ExecutePlugin<Team>(typeOfPluginToRun, objectsFromJson, svc, tracer);
					// ============================================================================
				}
				else if (processToRun == DEBUG_PLUGIN_LOGIC)
				{
					// Get the EXConnect service context
					var dbService = new BusinessDbService(svc, tracer);
#if DESKTOP
					// The following plugins must be 
					// ================================================
					// ================================================
					//TestAddTeamWebRolesPlugin(dbService, tracer);
					//TestRemoveTeamWebRolesPlugin(dbService, tracer);
					//TestAddTeamUserWebRolesPlugin(dbService, tracer);
					//TestRemoveTeamUserWebRolesPlugin(dbService, tracer);
#endif
				}

				tracer.Trace("\nProcess Completed...\n");
				tracer.Trace("\f");

				Console.WriteLine("Press any key to end the session..");
				Console.ReadKey();
			}

		}

#if DESKTOP
        /// <summary>
        /// Test the addition of web role to a team
        /// </summary>
        /// <param name="dbService"></param>
        /// <param name="tracer"></param>
        private static void TestAddTeamWebRolesPlugin(IRepository dbService, ITracingService tracer)
        {
            var teamName = "System Developers";
            var webRoleName = "System Developer";

            // instantiate the TeamMembership Plugin
            var teamMemberPlugin = new TeamMembershipPlugin(dbService, tracer);

            // Specify the target and related references
            var targetRef = dbService.Team.GetRecordFromName(teamName).Entity.ToEntityReference();
            var webRoleRef = dbService.WebRole.GetRecordFromName(webRoleName).Entity.ToEntityReference();
            var relatedRefs = new EntityReferenceCollection { webRoleRef };

            teamMemberPlugin.ExecutePlugin(targetRef, relatedRefs,
                TeamMembershipPlugin.ActionCall.AddRole, TeamMembershipPlugin.RelationshipType.TeamWebRole);
        }

        /// <summary>
        /// Test the removal of a web role from a team
        /// </summary>
        /// <param name="dbService"></param>
        /// <param name="tracer"></param>
        private static void TestRemoveTeamWebRolesPlugin(IRepository dbService, ITracingService tracer)
        {
            var teamName = "System Developers";
            var webRoleName = "System Developer";

            // instantiate the TeamMembership Plugin
            var teamMemberPlugin = new TeamMembershipPlugin(dbService, tracer);

            // Specify the target and related references
            var targetRef = dbService.Team.GetRecordFromName(teamName).Entity.ToEntityReference();
            var webRoleRef = dbService.WebRole.GetRecordFromName(webRoleName).Entity.ToEntityReference();
            var relatedRefs = new EntityReferenceCollection { webRoleRef };

            teamMemberPlugin.ExecutePlugin(targetRef, relatedRefs,
                TeamMembershipPlugin.ActionCall.RemoveRole, TeamMembershipPlugin.RelationshipType.TeamWebRole);
        }
        /// <summary>
        ///  Test the addition of a user to a team
        /// </summary>
        /// <param name="dbService"></param>
        /// <param name="tracer"></param>
        private static void TestAddTeamUserWebRolesPlugin(IRepository dbService, ITracingService tracer)
        {
            var teamName = "ECA Default Users";
            var userName = "German Velez";

            // instantiate the TeamMembership Plugin
            var teamMemberPlugin = new TeamMembershipPlugin(dbService, tracer);

            // Specify the target and related references
            var targetRef = dbService.Team.GetRecordFromName(teamName).Entity.ToEntityReference();
            var userRef = dbService.SystemUser.GetEntityReferenceFromCode(userName);
            var relatedRefs = new EntityReferenceCollection { userRef };

            teamMemberPlugin.ExecutePlugin(targetRef, relatedRefs,
                TeamMembershipPlugin.ActionCall.AddRole, TeamMembershipPlugin.RelationshipType.TeamUser);
        }
        /// <summary>
        /// Test the removal of a user from a team
        /// </summary>
        /// <param name="dbService"></param>
        /// <param name="tracer"></param>
        private static void TestRemoveTeamUserWebRolesPlugin(IRepository dbService, ITracingService tracer)
        {
            var teamName = "ECA Default Users";
            var userName = "German Velez";

            // instantiate the TeamMembership Plugin
            var teamMemberPlugin = new TeamMembershipPlugin(dbService, tracer);

            // Specify the target and related references
            var targetRef = dbService.Team.GetRecordFromName(teamName).Entity.ToEntityReference();
            var userRef = dbService.SystemUser.GetEntityReferenceFromCode(userName);
            var relatedRefs = new EntityReferenceCollection { userRef };

            teamMemberPlugin.ExecutePlugin(targetRef, relatedRefs,
                TeamMembershipPlugin.ActionCall.RemoveRole, TeamMembershipPlugin.RelationshipType.TeamUser);
        }
#endif

	}
}
