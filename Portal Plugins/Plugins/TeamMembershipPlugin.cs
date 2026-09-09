
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using Portal_Plugins.Entities.Interfaces;

namespace Portal_Plugins.Plugins
{
    using Plugin_Handling;

#if DESKTOP
	public class TeamMembershipPlugin
	{
		IRepository dbService;
		ITracingService tracer;

#else

	[CrmPluginRegistration(MessageNameEnum.Associate,
    "none",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "",
    "TeamMembership_Associate", 2,
    IsolationModeEnum.Sandbox,
    Description = "Create/Update User Web Role(s) on Team Membership Addition",
    Id = "e7759107-5a49-4e93-800b-b343a83c01ee")]

    [CrmPluginRegistration(MessageNameEnum.Disassociate,
    "none",
    StageEnum.PostOperation,
    ExecutionModeEnum.Asynchronous, "",
    "TeamMembership_Disassociate", 1,
    IsolationModeEnum.Sandbox,
    Description = "Create/Update User Web Role(s) on Team Membership Removal",
    Id = "cba4d0c8-8bbd-4b9f-9900-f127eaa1c10b")]

	public class TeamMembershipPlugin : MainPlugin
    {

#endif

	public enum ActionCall
		{
			None,
			AddRole,
			RemoveRole
		}

		public enum RelationshipType
		{
			None,
			TeamUser,
			TeamWebRole
		}

#if DESKTOP

		public TeamMembershipPlugin(IRepository dbserv, ITracingService trace)
		{
			dbService = dbserv;
			tracer = trace;
		}

		public void ExecutePlugin(EntityReference targetRef, EntityReferenceCollection relatedRefs,
							ActionCall processAction, RelationshipType relationshipType)
		{

#else
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Team Membership Plugin");

			// Get the action to process
			var processAction = ActionCall.None;
			if (this.context.MessageName == MessageNameEnum.Associate.ToString())
			{
				processAction = ActionCall.AddRole;
			}
			else if (this.context.MessageName == MessageNameEnum.Disassociate.ToString())
			{
				processAction = ActionCall.RemoveRole;
			}
			else
			{
				return;
			}

			// Process either the team membership (team or queue)
			// or the web Role association. Otherwise quit.

			var relationshipName = context.GetRelationship().SchemaName;

			tracer.Trace($"Relationship name = {relationshipName}");

			var relationshipType = (relationshipName.EndsWith("membership_association") ? RelationshipType.TeamUser
								   : relationshipName.EndsWith("WebRoles_Team_Association") ? RelationshipType.TeamWebRole : RelationshipType.None);

			if (relationshipType == RelationshipType.None)
			{
				tracer.Trace($"Call ignored...");
				return;
			}

			var targetRef = context.GetTargetEntityReference();
			var relatedRefs = context.GetRelatedEntities();

#endif

			// Get the team, system user, or web role depending on the incoming relationship.
			// Depending on how the relationship is being added/removed from the team
			// (starting from the SystemUser form or the Team form), the target and
			// related entity differ.

			EntityReference teamRef = null;
			EntityReference userRef = null;
			EntityReference roleRef = null;

			// Process either a team or queue
			var membershipType = new string[] { "team", "queue" };

			if (membershipType.Contains(targetRef.LogicalName))
			{
				teamRef = targetRef;
				if (targetRef.LogicalName == "queue")
				{
					teamRef = GetTeamReferenceFromQueueId(targetRef.Id);
				}
				if (relationshipType == RelationshipType.TeamUser)
				{
					userRef = relatedRefs.Where(r => r.LogicalName == "systemuser").FirstOrDefault();
				}
				else if (relationshipType == RelationshipType.TeamWebRole)
				{
					roleRef = relatedRefs.Where(r => r.LogicalName == "f3s_webroles").FirstOrDefault();
				}
			}
			else
			{
				if (targetRef.LogicalName == "systemuser" && relationshipType == RelationshipType.TeamUser)
				{
					userRef = targetRef;
				}
				else if (targetRef.LogicalName == "f3s_webroles" && relationshipType == RelationshipType.TeamWebRole)
				{
					roleRef = targetRef;
				}
				teamRef = relatedRefs.Where(r => r.LogicalName == "team").FirstOrDefault();
			}

			if (teamRef == null || (userRef == null && roleRef == null))
			{
				tracer.Trace("Unexpected Team, User, and/or Web Role null reference. Process will terminate.");
				return;
			}

			if (relationshipType == RelationshipType.TeamUser)
			{
				ProcessTeamUserWebRoles(userRef, teamRef, processAction);
			}
			else
			{
				ProcessTeamWebRoles(roleRef, teamRef, processAction);
			}
		}

		/// <summary>
		/// Associate/Disassociate web roles for a user based on the Team-WebRoles Association/Disassociation
		/// </summary>
		/// <param name="webRoleRef"></param>
		/// <param name="teamRef"></param>
		/// <param name="action"></param>
		private void ProcessTeamWebRoles(EntityReference webRoleRef, EntityReference teamRef, ActionCall action)
		{
			// Get the Web Roles to be associated/disassociated with the team

			var teamRecord = dbService.Team.GetRecordFromId(teamRef.Id);
			var roleRecord = dbService.WebRole.GetRecordFromId(webRoleRef.Id);

			tracer.Trace($"\"{roleRecord.Name}\" webrole will be {(action == ActionCall.AddRole ? "Associated" : "Disassociated")} with team \"{teamRecord.Name}\".");

			// Find the users in the team

			var usersList = GetTeamUsersList(teamRecord.Name);

			// Return if no users found for the team
			if (usersList == null || usersList.Count == 0)
			{
				tracer.Trace("No users found for the given team. Process will terminate.");
				return;
			}

			// Add/Remove the web role for each user in the team
			// But first find the associate "portal" webrole (mspp_webrole)

			var portalWebRoleId = GetAssociatedPortalWebRoleId(roleRecord.Name);

			if (portalWebRoleId == null)
			{
				return;
			}

			// Get the list of users with the Web Role
			var contactsList = GetContactsInWebRoleList(portalWebRoleId.Value, roleRecord.Name, teamRef.Id);

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

			foreach ( var user in usersList)
			{
				// Find if the user has the web role
				if (user.ContactId != null)
				{
					var hasWebRole = false;
					var hasRoleInOtherTeam = false;
					if (contactsList != null)
					{
						hasWebRole = (contactsList.Where(w => w.ContactId == user.ContactId.GetValueOrDefault()).Count() > 0);
						if (hasWebRole)
						{
							hasRoleInOtherTeam = (contactsList.Where(w => w.TeamId != teamRef.Id
														&& w.ContactId == user.ContactId.GetValueOrDefault()).Count() > 0);
						}
					}
					if (action == ActionCall.AddRole)
					{
						if (!hasWebRole)
						{
							// Add the Role Association
							var request = new AssociateRequest
							{
								Target = new EntityReference("contact", user.ContactId.GetValueOrDefault()),
								RelatedEntities = new EntityReferenceCollection { new EntityReference("powerpagecomponent", portalWebRoleId.Value) },
								Relationship = new Relationship("powerpagecomponent_mspp_webrole_contact"),
							};
							tracer.Trace($"Adding request to associate the {roleRecord.Name} Web Role to user {user.Name}.");
							multipleRequest.Requests.Add(request);

							// Add this user to the User SP Action table for sync with SharePoint
							userSPactionList.Add(new UserSPAction
							{
								Action = "Insert",
								EmailAddress = user.EmailAddress,
								PermissionLevel = (roleRecord.Name.Contains("Administrator") ? "Admin" : "User")
							});
						}
					}
					else
					{
						if (hasWebRole && !hasRoleInOtherTeam)
						{
							// Remove the Role Association
							var request = new DisassociateRequest
							{
								Target = new EntityReference("contact", user.ContactId.GetValueOrDefault()),
								RelatedEntities = new EntityReferenceCollection { new EntityReference("powerpagecomponent", portalWebRoleId.Value) },
								Relationship = new Relationship("powerpagecomponent_mspp_webrole_contact"),
							};
							tracer.Trace($"Adding request to disassociate the {roleRecord.Name} Web Role from user {user.Name}.");
							multipleRequest.Requests.Add(request);

							// Add this user to the User SP Action table for sync with SharePoint
							userSPactionList.Add(new UserSPAction
							{
								Action = "Remove",
								EmailAddress = user.EmailAddress,
								PermissionLevel = (roleRecord.Name.Contains("Administrator") ? "Admin" : "User")
							});
						}
					}
				}

				if (multipleRequest.Requests.Count > 500)
				{
					var multipleResponse = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
					multipleRequest.Requests.Clear();
					tracer.Trace($"First Request results were {(multipleResponse.IsFaulted ? "Faulted" : "Not Faulted")}.");
				}
			}
			if (multipleRequest.Requests.Count > 0)
			{
				var multipleResponse = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
				tracer.Trace($"Final Request results were {(multipleResponse.IsFaulted ? "Faulted" : "Not Faulted")}.");
			}

			// Populate the Support Documents User SP Action table if insert or deletions are detected
			var common = new PluginCommon(dbService, tracer);
			common.PopulateSupportDocsUserSPactionTable(userSPactionList);
		}

		/// <summary>
		/// Associate/Disassociate Web Roles for a user based on the Team-User Association/Disassociation
		/// </summary>
		/// <param name="userRef"></param>
		/// <param name="teamRef"></param>
		/// <param name="action"></param>
		private void ProcessTeamUserWebRoles(EntityReference userRef, EntityReference teamRef, ActionCall action)
        {
			// Start with the user email, associated contact record, and Team

			var userEmail = dbService.SystemUser.GetEmailFromId(userRef.Id);
			var contactRef = dbService.Contact.GetEntityReferenceFromEmail(userEmail);
			var teamName = dbService.Team.GetRecordFromId(teamRef.Id).Name;

			// Return if no Contact record; no reason for association since there is no record.
			if (contactRef == null)
			{
				tracer.Trace($"No Contact record found for user with email \"{userEmail}\" roles found for the given team. Process will terminate.");
				return;
			}
            
			// Get the Web Roles associated with the team
			var webRolesList = GetTeamWebRolesList(teamName);

			// Return if no web roles found for the team
			if (webRolesList == null || webRolesList.Count == 0)
			{
				tracer.Trace("No web roles found for the given team. Process will terminate.");
				return;
			}

			// Get the user's current webroles for all teams
			var userRoleList = GetUserCurrentTeamWebRolesList(contactRef.Id, teamRef.Id);

			if (userRoleList != null)
			{
				tracer.Trace("List of Web Roles for this user");
				foreach (var role in userRoleList)
				{
					tracer.Trace($"Name: {role.WebRoleName}, TeamId: {role.TeamId}.");
				}
			}

			// ===============================================
			// Add/Remove the associated web role for the user
			// ===============================================

			// Create a list to keep track of any records to be added to the Support Document User SP Action table
			var userSPactionList = new List<UserSPAction>();

			foreach ( var webRole in webRolesList )
			{
				if (webRole.PortalWebRoleId != null)
				{
					var hasWebRole = false;
					var hasRoleInOtherTeam = false;
					if (userRoleList != null )
					{
						hasWebRole = (userRoleList.Where( w => w.WebRoleId == webRole.PortalWebRoleId).Count() > 0);
						if (hasWebRole)
						{
							hasRoleInOtherTeam = (userRoleList.Where(w => w.TeamId != teamRef.Id
											&& w.WebRoleId == webRole.PortalWebRoleId).Count() > 0);
						}
					}
					if (action == ActionCall.AddRole)
					{
						if (!hasWebRole)
						{
							var request = new AssociateRequest
							{
								Target = new EntityReference("contact", contactRef.Id),
								RelatedEntities = new EntityReferenceCollection { new EntityReference("powerpagecomponent", webRole.PortalWebRoleId) },
								Relationship = new Relationship("powerpagecomponent_mspp_webrole_contact")
							};
							tracer.Trace($"Executing request to associate the {webRole.Name} Web Role from user {userEmail}.");
							var response = dbService.OrgServiceContext.Execute(request);
							tracer.Trace($"Request resulted in {response.Results.Count} errors.");

							// Add this user to the User SP Action table for sync with SharePoint
							userSPactionList.Add(new UserSPAction
							{
								Action = "Insert",
								EmailAddress = userEmail,
								PermissionLevel = (webRole.Name.Contains("Administrator") ? "Admin" : "User")
							});
						}
					}
					else
					{
						if (hasWebRole && !hasRoleInOtherTeam)
						{
							var request = new DisassociateRequest
							{
								Target = new EntityReference("contact", contactRef.Id),
								RelatedEntities = new EntityReferenceCollection { new EntityReference("powerpagecomponent", webRole.PortalWebRoleId) },
								Relationship = new Relationship("powerpagecomponent_mspp_webrole_contact"),
							};
							tracer.Trace($"Executing request to disassociate the {webRole.Name} Web Role from user {userEmail}.");
							var response = dbService.OrgServiceContext.Execute(request);
							tracer.Trace($"Request resulted in {response.Results.Count} errors.");

							// Add this user to the User SP Action table for sync with SharePoint
							// but first, find if the user has other non-Admin roles
							var removeFromSharePoint = webRole.Name.Contains("Administrator");
							if (!removeFromSharePoint)
							{
								var nonAdminRoleCount = userRoleList.Where(w => w.WebRoleName != webRole.Name 
																			&& !w.WebRoleName.Contains("Administrator")).Count();
								tracer.Trace($"This non-Admin web role is found in {nonAdminRoleCount} times.");
								// Check if this is the only non-Admin web role
								removeFromSharePoint = (nonAdminRoleCount == 0);
							}
							if (removeFromSharePoint)
							{
								userSPactionList.Add(new UserSPAction
								{
									Action = "Remove",
									EmailAddress = userEmail,
									PermissionLevel = (webRole.Name.Contains("Administrator") ? "Admin" : "User")
								});
							}
						}
					}
				}
			}

			// Populate the Support Documents User SP Action table if insert or deletions are detected
			var common = new PluginCommon(dbService, tracer);
			common.PopulateSupportDocsUserSPactionTable(userSPactionList);
		}

		#region Auxiliary Methods

		/// <summary>
		/// Retrieve the Team EntityReference from a given Queue
		/// </summary>
		/// <param name="queueId"></param>
		/// <returns></returns>
		private EntityReference GetTeamReferenceFromQueueId(Guid queueId)
		{
/*
			// Find the Team Name from the Queue Name
			var fetchXml = $@"
<fetch>
  <entity name='queue'>
    <attribute name='description' />
    <filter>
      <condition attribute='description' operator='like' value='Lead Voucher%' />
    </filter>
  </entity>
</fetch>
";
			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));
			if (results.Entities.Count > 0 && results.Entities[0].Contains("description"))
			{
				var teamName = results.Entities[0]["description"].ToString();
				var teamRecord = dbService.Team.GetAllRecords().Where(t => t.Name == teamName).FirstOrDefault();
				if (teamRecord != null)
				{
					return teamRecord.Entity.ToEntityReference();
				}
			}
*/
			return null;
		}

		public class TeamUser
		{
			public string Name = string.Empty;
			public string EmailAddress;
			public Guid Id;
			public Guid? ContactId;
		}

		/// <summary>
		/// Retrieve the list of Users associated with a given team
		/// </summary>
		/// <param name="teamName"></param>
		/// <returns></returns>
		private IList<TeamUser> GetTeamUsersList(string teamName)
		{
			var usersList = new List<TeamUser>();

			var fetchXml = $@"
<fetch>
  <entity name='teammembership'>
    <attribute name='teammembershipid' />
    <link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='U' link-type='inner'>
      <attribute name='fullname' />
      <attribute name='systemuserid' />
      <attribute name='internalemailaddress' />
      <link-entity name='contact' to='internalemailaddress' from='emailaddress1' alias='C' link-type='outer'>
        <attribute name='contactid' />
      </link-entity>
      <order attribute='fullname' />
    </link-entity>
    <link-entity name='team' to='teamid' from='teamid' alias='T' link-type='inner'>
      <attribute name='teamid' />
      <filter>
        <condition attribute='name' operator='eq' value='{teamName}' />
      </filter>
      <order attribute='teamid' />
    </link-entity>
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						var user = new TeamUser
						{
							Name = ((AliasedValue)entity["U.fullname"]).Value.ToString(),
							EmailAddress = ((AliasedValue)entity["U.internalemailaddress"]).Value.ToString(),
							Id = (Guid)((AliasedValue)entity["U.systemuserid"]).Value
						};
						if (entity.Attributes.Contains("C.contactid"))
						{
							user.ContactId = (Guid)((AliasedValue)entity["C.contactid"])?.Value;
						}
						usersList.Add(user);
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Team Users list.");
				return null;
			}

			return usersList;

		}

		public class TeamWebRole
		{
			public string Name = string.Empty;
			public Guid PortalWebRoleId;
		}

		/// <summary>
		/// Retrieve the list Web Roles associatated to a given team
		/// </summary>
		/// <param name="teamName"></param>
		/// <returns></returns>
		private IList<TeamWebRole> GetTeamWebRolesList(string teamName)
		{
			// Because there is no specific relationship between the mspp_webrole and
			// the F3S Web Role tables, the query must be performed in two steps.
			// First, one to gather all the F3S Web Roles associated with the user,
			// and the second, to get the corresponding Portal web roles fro the names
			// gathered from the frist step.

			// ============
			// First Step
			// ============

			var fetchXml = $@"
<fetch>
  <entity name='f3s_webroles_team'>
    <attribute name='f3s_webroles_teamid' />
    <link-entity name='f3s_webroles' to='f3s_webrolesid' from='f3s_webrolesid' alias='R' link-type='inner'>
      <attribute name='f3s_webrolesid' />
      <attribute name='f3s_rolename' />
    </link-entity>
    <link-entity name='team' to='teamid' from='teamid' alias='T' link-type='inner'>
      <attribute name='teamid' />
      <filter>
        <condition attribute='name' operator='eq' value='{teamName}' />
      </filter>
    </link-entity>
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
				tracer.Trace("No records returned during retrieval of User-Team/WebRoles Association list.");
				return null;
			}

			tracer.Trace($"Found {roleNames.Count} roles associated with the user");

			// ============
			// Second Step
			// ============

			var webRolesList = new List<TeamWebRole>();

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

			fetchXml += @"
			</condition>
		</filter>
    </entity>
</fetch>
";

			tracer.Trace($"FetchXml = \n{fetchXml}");

			results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					/*
					tracer.Trace($"Entity has {entity.Attributes.Count} attributes.");
					foreach (var key in entity.Attributes.Keys)
					{
						tracer.Trace($"Key: {key}, Value: {entity[key].ToString()}");
					}
					*/
					if (entity.Id != null)
					{
						webRolesList.Add( new TeamWebRole
						{
							Name = entity["mspp_name"].ToString(),
							PortalWebRoleId = (Guid)entity["mspp_webroleid"]
						});
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Team Web Roles list.");
				return null;
			}

			return webRolesList;

		}

		/// <summary>
		/// Get the associated Power Pages Web Role (mspp_webrole) 
		/// from the F3S Web Role name
		/// </summary>
		/// <param name="webRoleName"></param>
		/// <returns></returns>
		private Guid? GetAssociatedPortalWebRoleId(string webRoleName)
		{
			Guid? webRoleId = null;

			var fetchXml = $@"
<fetch>
  <entity name='mspp_webrole'>
    <attribute name='mspp_webroleid' />
    <link-entity name='mspp_website' to='mspp_websiteid' from='mspp_websiteid' alias='S' link-type='inner'>
      <filter>
        <condition attribute='mspp_name' operator='eq' value='{Configuration.Data.PortalName}' />
      </filter>
    </link-entity>
    <filter>
      <condition attribute='mspp_name' operator='eq' value='{webRoleName}' />
    </filter>
  </entity>
</fetch>";

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				webRoleId = results.Entities[0].Id;
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Portal Web Role.\n");
				return null;
			}

			return webRoleId;

		}

		private class ContactTeam
		{
			internal Guid TeamId;
			internal Guid ContactId;
			internal string EmailAddress;
		}

		/// <summary>
		/// Retrieve the list of Teams and Contacts associated to a given Web Role name
		/// </summary>
		/// <param name="roleName"></param>
		/// <returns></returns>
		private IList<ContactTeam> GetContactsInWebRoleList(Guid roleId, string roleName, Guid teamId)
		{
			var contactsList = new List<ContactTeam>();

			// Start with the list of contacts with the web role. Assign the contact to the current Team,
			// this will be relaced if other teams are found with the same web role.
			var fetchXml = $@"
<fetch>
    <entity name='powerpagecomponent_mspp_webrole_contact' to='contactid' from='contactid' alias='RC' link-type='inner'>
       <attribute name='powerpagecomponent_mspp_webrole_contactid' />
       <attribute name='contactid' />
       <filter>
          <condition attribute='powerpagecomponentid' operator='eq' value='{roleId.ToString()}' />
       </filter>
    </entity>
</fetch>";

			tracer.Trace($"FetchXml = \n{fetchXml}");

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						contactsList.Add(new ContactTeam
						{
							ContactId = (Guid)entity["contactid"],
							TeamId = teamId
						});
					}
				}
			}
			else
			{
				tracer.Trace("No Contact records returned during retrieval of Contact-WebRoles Association list.");
				return null;
			}

			// Follow with the list of teams for those contacts with the web role

			fetchXml = $@"
<fetch>
  <entity name='teammembership'>
    <attribute name='teamid' />
    <attribute name='teammembershipid' />
    <link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='U' link-type='inner'>
      <attribute name='systemuserid' />
      <attribute name='internalemailaddress' />
      <link-entity name='contact' to='internalemailaddress' from='emailaddress1' alias='C' link-type='inner'>
        <attribute name='contactid' />
        <filter>
			<condition attribute='contactid' operator='in'>";

			foreach(var contact in contactsList)
			{
				fetchXml += $@"<value>{contact.ContactId.ToString()}</value>";
			}

			fetchXml += $@"
			</condition>
		</filter>
        <order attribute='contactid' />
      </link-entity>
    </link-entity>
    <link-entity name='f3s_webroles_team' to='teamid' from='teamid' alias='RT' link-type='inner'>
       <attribute name='f3s_webroles_teamid' />
       <link-entity name='f3s_webroles' to='f3s_webrolesid' from='f3s_webrolesid' link-type='inner'>
		 <attribute name='f3s_webrolesid' />
		 <filter>
			<condition attribute='f3s_rolename' operator='eq' value='{roleName}' />
		 </filter>
	   </link-entity>
    </link-entity>
    <order attribute='teamid' />
  </entity>
</fetch>";

			tracer.Trace($"FetchXml = \n{fetchXml}");

			results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						contactsList.Add(new ContactTeam
						{
							TeamId = (Guid)entity["teamid"],
							ContactId = (Guid)((AliasedValue)entity["C.contactid"]).Value,
							EmailAddress = ((AliasedValue)entity["U.internalemailaddress"]).ToString()
						});
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Contact-Teams Association list.");
			}

			return contactsList;

		}

		internal class UserWebRole
		{
			internal Guid TeamId;
			internal Guid WebRoleId;
			internal string WebRoleName;
		}

		/// <summary>
		///  Retrieve the list of Teams and Power Pages Web Role Ids (mspp_webroleid) 
		///  associated to a given user's Contact Id
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		private IList<UserWebRole> GetUserCurrentTeamWebRolesList(Guid contactId, Guid teamId)
		{
			// Start with the list of webroles for this user/contact
			// When an association is removed, the reference is gone so need to add the current team
			// to the first result list toto make sure the web role is removed by the caller

			var webRolesIdList = new List<UserWebRole>();

			var fetchXml = $@"
<fetch>
  <entity name='powerpagecomponent_mspp_webrole_contact'>
    <attribute name='powerpagecomponentid' />
	<link-entity name='powerpagecomponent' to='powerpagecomponentid' from='powerpagecomponentid' alias='PC' link-type='inner'>
		<attribute name='name' />
	</link-entity>
    <filter>
        <condition attribute='contactid' operator='eq' value='{contactId}' />
    </filter>
  </entity>
</fetch>" ;

			tracer.Trace($"FetchXml = \n{fetchXml}");

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						webRolesIdList.Add(new UserWebRole
						{
							TeamId = teamId,
							WebRoleName = ((AliasedValue)entity["PC.name"]).Value.ToString(),
							WebRoleId = (Guid)entity["powerpagecomponentid"],
						});
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Contact-WebRoles Association list.");
				return null;
			}

			tracer.Trace($"Retrieved {webRolesIdList.Count} webroles for the current user.");

			// Retrieve the list of teams associated with those roles

			fetchXml = $@"
<fetch>
  <entity name='teammembership'>
    <attribute name='teamid' />
    <attribute name='teammembershipid' />
    <link-entity name='systemuser' to='systemuserid' from='systemuserid' alias='U' link-type='inner'>
      <attribute name='systemuserid' />
      <link-entity name='contact' to='internalemailaddress' from='emailaddress1' alias='C' link-type='inner'>
        <filter>
          <condition attribute='contactid' operator='eq' value='{contactId}' />
        </filter>
      </link-entity>
    </link-entity>
	<link-entity name='f3s_webroles_team' to='teamid' from='teamid' alias='RT' link-type='inner'>
		<attribute name='f3s_webroles_teamid' />
		<link-entity name='f3s_webroles' to='f3s_webrolesid' from='f3s_webrolesid' alias='WR' link-type='inner'>
			<attribute name='f3s_rolename' />
			<attribute name='f3s_webrolesid' />
			<filter>
				<condition attribute='f3s_rolename' operator='in'>";

			foreach(var role in webRolesIdList)
			{
				fetchXml += $@"<value>'{role.WebRoleName}'</value>";
			}
			fetchXml += @"
				</condition>
			</filter>
			<order attribute='f3s_webrolesid' />
		</link-entity>
	</link-entity>
    <order attribute='teamid' />
  </entity>
</fetch>";

			tracer.Trace($"FetchXml = \n{fetchXml}");

			results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				foreach (var entity in results.Entities)
				{
					if (entity.Id != null)
					{
						webRolesIdList.Add(new UserWebRole
						{
							TeamId = (Guid)entity["teamid"],
							WebRoleId = (Guid)((AliasedValue)entity["RC.powerpagecomponentid"]).Value
						});
					}
				}
			}
			else
			{
				tracer.Trace("No records returned during retrieval of Team-WebRoles Association list.");
			}

			return webRolesIdList;

		}

        #endregion

    }
}
