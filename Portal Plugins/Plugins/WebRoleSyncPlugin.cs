
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using Portal_Plugins.Plugin_Handling;

namespace Portal_Plugins.Plugins
{
	[CrmPluginRegistration(MessageNameEnum.Create,
	"f3s_webroles",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "",
	"Web Role Duplicate Detection", 1,
	IsolationModeEnum.Sandbox,
	Description = "Verify Web Roles names are not duplicated on record Create",
	Id = "fee02493-4c93-48cc-8508-e9bf6cda3303")]

	[CrmPluginRegistration(MessageNameEnum.Create,
	"f3s_webroles",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Web Role Create Synchronization", 1,
	IsolationModeEnum.Sandbox,
	Description = "Synchronize Web Roles with the Portal Web Roles on Record Create",
	Id = "cc756f7d-d1c3-4c0d-b931-3998f34c4e4b")]

	[CrmPluginRegistration(MessageNameEnum.Update,
	"f3s_webroles",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "f3s_rolename,f3s_description,statecode",
	"Web Role Duplicate Update Prevention", 2,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_rolename,f3s_description,statecode",
	Description = "Verify Web Roles do not generate a duplicate in the Web Roles or Portal Web Roles tables on Update",
	Id = "8c1fee85-406f-4d8b-8bbe-1857da701d29")]

	[CrmPluginRegistration(MessageNameEnum.Update,
	"f3s_webroles",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "f3s_rolename,f3s_description,statecode",
	"Web Role Update Synchronization", 2,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_rolename,f3s_description,statecode",
	Description = "Synchronize Web Roles with the Portal Web Roles on Record Update",
	Id = "3198ecce-288c-487e-9876-f8944daa92e1")]

	[CrmPluginRegistration(MessageNameEnum.Delete,
	"f3s_webroles",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Web Role Delete Synchronization", 3,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_rolename,f3s_description,statecode",
	Description = "Synchronize Web Roles with the Portal Web Roles on Record Delete",
	Id = "af7039a7-0953-4178-ac08-3d92b6fb9dad")]

	public class WebRoleSyncPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext )
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of Web Role Synchronization");

			if (this.context.MessageName == MessageNameEnum.Delete.ToString())
			{

				// A Deleted record does not provide a TargetEntity but a TargetEntityReference
				// so we can deal with the PreImage directly.
				var oldWebRoleRecord = dbService.WebRole.GetRecordFromEntity(context.GetPreImage());

				var portalRoleRecord = dbService.PortalWebRole.GetRecordFromName(oldWebRoleRecord.Name);
				if (portalRoleRecord == null)
				{
					tracer.Trace($"The \"{oldWebRoleRecord.Name}\" Web Role cannot be found. Process termianted.");
					return;
				}
				dbService.Delete(portalRoleRecord.Entity);
			}
			else
			{
				// Get the Web Roles record object from the target entity
				var newWebRoleRecord = dbService.WebRole.GetRecordFromEntity(context.GetTargetEntity());

				tracer.Trace($"Processing \"{this.context.MessageName}\" action for Web Role \"{newWebRoleRecord.Name}\" with Id = {newWebRoleRecord.Id}.");

				if (this.context.MessageName == MessageNameEnum.Create.ToString())
				{
					if (context.ExecutionContext.Stage == (int)StageEnum.PreOperation)
					{
						// Check that the name is unique
						var recordExists = (dbService.WebRole.GetRecordFromName(newWebRoleRecord.Name) != null);
						if (recordExists)
						{
							var msg = $"A Web Role with name \"{newWebRoleRecord.Name}\" already exists. Duplicate Web Roles are not allowed.";
							tracer.Trace(msg);
							throw (new InvalidPluginExecutionException(OperationStatus.Failed, msg));
						}
						return;
					}

					// Check for the existance of a corresponding Portal Web Role (by name)
					var portalRoleRecord = dbService.PortalWebRole.GetRecordFromName(newWebRoleRecord.Name);
					if (portalRoleRecord != null)
					{
						// Update the description
						portalRoleRecord.Description = newWebRoleRecord.Description;
						dbService.Update(portalRoleRecord.Entity, true);
						tracer.Trace($"The \"{newWebRoleRecord.Name}\" Web Role already exists for the Portal site. Description was updated.");
						return;
					}

					tracer.Trace($"Web Role cannot be found, a new record will be created...");

					// Add the Web Role record
					portalRoleRecord = dbService.PortalWebRole.CreateRecord();
					portalRoleRecord.Name = newWebRoleRecord.Name;
					portalRoleRecord.WebsiteRef = dbService.PortalWebRole.F3S_WebSiteRef;
					portalRoleRecord.Description = newWebRoleRecord.Description;
					dbService.Create(portalRoleRecord.Entity, true);
				}
				else if (this.context.MessageName == MessageNameEnum.Update.ToString())
				{
					var oldWebRoleRecord = dbService.WebRole.GetRecordFromEntity(context.GetPreImage());

					tracer.Trace($"Updating Web Role from \"{oldWebRoleRecord.Name}\".");

					if (context.ExecutionContext.Stage == (int)StageEnum.PreOperation)
					{
						if (newWebRoleRecord.HasUpdatedName)
						{
							// Check that the name is unique if the name is updated
							var recordExists = (dbService.WebRole.GetRecordFromName(newWebRoleRecord.Name) != null);
							if (recordExists)
							{
								var msg = $"A Web Role with name \"{newWebRoleRecord.Name}\" already exists. Duplicate Web Roles are not allowed.";
								tracer.Trace(msg);
								throw (new InvalidPluginExecutionException(OperationStatus.Failed, msg));
							}
						}
						return;
					}

					var portalRoleRecord = dbService.PortalWebRole.GetRecordFromName(oldWebRoleRecord.Name);
					if (portalRoleRecord == null) 
					{
						tracer.Trace($"The \"{newWebRoleRecord.Name}\" Web Role cannot be found. Updates will not be applied.");
						return;
					}

					portalRoleRecord.Name = (newWebRoleRecord.HasUpdatedName ? newWebRoleRecord.Name : oldWebRoleRecord.Name);
					portalRoleRecord.Description = (newWebRoleRecord.HasUpdatedDescription ? newWebRoleRecord.Description : oldWebRoleRecord.Description);

					dbService.Update(portalRoleRecord.Entity, true);
				}
			}

		}

    }
}
