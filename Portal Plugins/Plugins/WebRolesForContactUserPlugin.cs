using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using Portal_Plugins.Plugin_Handling;

namespace Portal_Plugins.Plugins
{
	[CrmPluginRegistration(MessageNameEnum.Update,
	"contact",
	StageEnum.PostOperation,
	ExecutionModeEnum.Asynchronous, "adx_identity_emailaddress1confirmed",
	"Web Roles Assignment for Contact User", 100,
	IsolationModeEnum.Sandbox,
	Description = "Assign the proper Web Roles to based on User Team(s) on Contact Record Create",
	Id = "52eed7c5-8258-4e3d-a9d2-1dd18bd2eb42")]

	public class WebRolesForContactUserPlugin : MainPlugin
	{
		//==========================================
		// Although the purpose of this plugin was to execute on Contact Create, the underlined
		// Power Pages process, does not have all the necessary information by the time the plugin
		// is launched. As a result, the plugin was change to trigger on Update of the Email Confirmed
		// field from No to Yes and the Execution Mode from Synchronous to Asynchronous.
		//==========================================

		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Contact Web Role Assignment");

			if (this.context.MessageName != MessageNameEnum.Update.ToString())
			{
				tracer.Trace("Unexpected plugin action requested. Process will terminate.");
				return;
			}

			// Get the Contact user record object from the target entity
			var contactRecord = dbService.Contact.GetRecordFromEntity(context.GetTargetEntity());

			// Check that the Email Address has been confirmed
			if (!contactRecord.IsEmailConfirmed) 
			{
				tracer.Trace("The Contact's Email Address has not been confirmed. Process will terminate.");
				return;
			}

			tracer.Trace($"Processing \"{this.context.MessageName}\" action for Web Role \"{contactRecord.Name}\" with Id = {contactRecord.Id}.");

			var userRoleList = GetUserTeamsWebRoles(contactRecord.Id);

			if (userRoleList == null)
			{
				tracer.Trace("No Web Roles found from the user's associated teams. Process terminated.");
				return;
			}

			// Associate every web role found to the contact

			// Set the ground work for a multipleRequest
			var multipleRequest = new ExecuteMultipleRequest()
			{
				// Assign settings that define execution behavior: continue on error, return responses.
				Settings = new ExecuteMultipleSettings()
				{
					ContinueOnError = true,
					ReturnResponses = true
				},
				// Create an empty organization request collection.
				Requests = new OrganizationRequestCollection()
			};

			// Create a list to keep track of any records to be added to the Support Docuemtn Users SP Action table
			var userSPactionList = new List<UserSPAction>();

			// We are going to need the email address so retrieve it from the record if not in the input
			var userEmail = contactRecord.EmailAddress;
			var iteration = 0;
			while (string.IsNullOrEmpty(userEmail) && iteration < 20)
			{
				Thread.Sleep(1000);
				userEmail = dbService.Contact.GetEmailFromId(contactRecord.Id);
			}

			if (iteration > 20)
			{
				tracer.Trace("Unable to retrieve the contacts email address. Process will terminate.");
				return;
			}

			foreach ( var role in userRoleList )
			{
				// Add the Role Association
				var request = new AssociateRequest
				{
					Target = new EntityReference("contact", contactRecord.Id),
					RelatedEntities = new EntityReferenceCollection { new EntityReference("powerpagecomponent", role.Id) },
					Relationship = new Relationship("powerpagecomponent_mspp_webrole_contact"),
				};
				tracer.Trace($"Adding request to associate the {role.Name} Web Role to user {contactRecord.Name}.");
				multipleRequest.Requests.Add(request);

				// Add this user to the User SP Action table for sync with SharePoint
				userSPactionList.Add(new UserSPAction
				{
					Action = "Insert",
					EmailAddress = userEmail,
					PermissionLevel = (role.Name.Contains("Administrator") ? "Admin" : "User")
				});
			}

			var multipleResponse = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
			tracer.Trace($"Final Request results were {(multipleResponse.IsFaulted ? "Faulted" : "Not Faulted")}.");
			
			// Populate the Support Documents User SP Action table if insert or deletions are detected
			var common = new PluginCommon(dbService, tracer);
			common.PopulateSupportDocsUserSPactionTable(userSPactionList);

		}

		internal class TeamWebRole
		{
			internal Guid Id;
			internal String Name;
		}

		private IList<TeamWebRole> GetUserTeamsWebRoles(Guid contactId)
		{
			// Because there is no specific relationship between the mspp_webrole and
			// the Web Role tables, the query must be performed in two steps.
			// First, one to gather all the Web Roles associated with the user,
			// and the second, to get the corresponding Portal web roles fro the names
			// gathered from the frist step.

			// ============
			// First Step
			// ============

			var fetchXml = $@"
<fetch>
  <entity name='teammembership'>
    <attribute name='teammembershipid' />
    <link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='U' link-type='inner'>
      <attribute name='systemuserid' />
      <link-entity name='contact' to='internalemailaddress' from='emailaddress1' alias='C' link-type='inner'>
        <filter>
          <condition attribute='contactid' operator='eq' value='{contactId}' />
        </filter>
      </link-entity>
    </link-entity>
    <link-entity name='team' to='teamid' from='teamid' alias='T' link-type='inner'>
      <attribute name='teamid' />
    </link-entity>
    <link-entity name='f3s_webroles_team' to='teamid' from='teamid' alias='RT' link-type='inner'>
      <attribute name='f3s_webroles_teamid' />
      <link-entity name='f3s_webroles' to='f3s_webrolesid' from='f3s_webrolesid' alias='R' link-type='inner'>
        <attribute name='f3s_webrolesid' />
        <attribute name='f3s_rolename' />
		<filter>
			<condition attribute='f3s_rolename' operator='not-null' />
		</filter>
      </link-entity>
    </link-entity>
    <order attribute='teammembershipid' />
  </entity>
</fetch>";

			var roleNames = new List<string>();

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						roleNames.Add(((AliasedValue)entity["R.f3s_rolename"]).Value.ToString());
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Contact-Team/WebRoles Association list.");
				return null;
			}

			tracer.Trace($"Found {roleNames.Count} roles associated with the user");

			// ============
			// Second Step
			// ============

			var webRolesIdList = new List<TeamWebRole>();

			fetchXml = $@"
<fetch>
  <entity name='mspp_webrole'>
    <attribute name='mspp_webroleid' />
    <attribute name='mspp_name' />
	<filter>
		<condition attribute='mspp_name' operator='in'>";

			foreach(var role in roleNames )
			{
				fetchXml += $"<value>{role}</value>";
			}

			fetchXml += @"</condition>
	</filter>
  </entity>
</fetch>";

			results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						webRolesIdList.Add(new TeamWebRole
						{
							Id = (Guid)entity["mspp_webroleid"],
							Name = entity["mspp_name"].ToString()
						});
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Portal WebRoles list.");
				return null;
			}

			return webRolesIdList;
		}

    }
}
